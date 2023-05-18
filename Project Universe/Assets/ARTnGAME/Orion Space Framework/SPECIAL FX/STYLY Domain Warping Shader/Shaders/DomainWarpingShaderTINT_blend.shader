Shader "ORION/Unlit/DomainWarpingShaderTINT_blend"
{
    Properties
	{	_GroundTex("Ground Texture", 2D) = "white" {}
		_GradientTex("Gradient Texture", 2D) = "white" {}
		_GridNumber("Grid Number", Float) = 64.0
		_EllipseSize("Ellipse Size", Float) = 1.0
		_Speed("Speed", Float) = 1.0
		_Fbm_ScaleFactor("Fbm Scale Factor", Vector) = (1.0, 1.0, 4.0, 4.0)

			//v0.1
			colors_Factor("Colors Scale Factor", Vector) = (0.0, 0.0, 0.0, 0.0)
			thres("Colors thresholds", Vector) = (0.0, 0.0, 0.0, 0.0)

			//v0.2 - holes
			_DistortionStrength("Distortion Strength", Float) = 3.020896
			_HoleSize("Hole Size", Float) = 0.7030833
			_HoleEdgeSmoothness("Hole Edge Smoothness", Float) = 0.007289694
			_HoleNumberDistrib("Hole Number and Distribution", Vector) = (1, 0.0, 1.0, 1.0)
			_holePos("Hole Position", Vector) = (0, 0.0, 0.0, 0.0)
			_darkColor("Hole darkcolor", Vector) = (0.4, 0.2, 0.0, 1.0)

			_hole2Pos("Hole 2 Position", Vector) = (0, 0.0, 0.0, 0.0)
			_planetPos("Planet 2 Position", Vector) = (0, 0.0, 0.0, 0.0)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				//v0.1
				float4 colors_Factor;
				float4 thres;

				//v0.2 - holes
				float  _DistortionStrength;
				float  _HoleSize;
				float  _HoleEdgeSmoothness;
				float4 _HoleNumberDistrib;
				float4 _darkColor;
				float4 _hole2Pos;
				float4 _planetPos;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {                
                float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				float4 projPos : TEXCOORD3;
				UNITY_FOG_COORDS(4)
            };

            
            //////////////////////////////////////////////////////////////////////////////
            float random(float2 st) {
                return frac(sin(dot(st.xy,
                                    float2(12.9898,78.233)))*
                            43758.5453123);
            }

            //////////////////////////////////////////////////////////////////////////////
            // Based on Morgan McGuire @morgan3d
            // https://www.shadertoy.com/view/4dS3Wd
            float noise (float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);

                // Four corners in 2D of a tile
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) +
                        (c - a)* u.y * (1.0 - u.x) +
                        (d - b) * u.x * u.y;
            }
            
            //////////////////////////////////////////////////////////////////////////////
            #define OCTAVES 6
            // based on : https://thebookofshaders.com/13/?lan=jp
            float fbm (float2 st) {
                // Initial values
                float value = 0.0;
                float amplitude = .5;
                float frequency = 0.;
                // Loop of octaves
                for (int i = 0; i < OCTAVES; i++) {
                    value += amplitude * noise(st);
                    st *= 2.;
                    amplitude *= .5;
                }
                return value;
            }

            //////////////////////////////////////////////////////////////////////////////
            // domain warping pattern
            // based on : http://www.iquilezles.org/www/articles/warp/warp.htm
            float pattern (float2 p, float4 scale_1, float scale_2, float4 add_1, float4 add_2) {
                // first domain warping
                float2 q = float2( 
                                fbm( p + scale_1.x * add_1.xy ),
                                fbm( p + scale_1.y * add_1.zw ) 
                                );
                            
                // second domain warping
                float2 r = float2( 
                                fbm( p + scale_1.z * q + add_2.xy ),
                                fbm( p + scale_1.w * q + add_2.zw ) 
                                );

                return fbm( p + scale_2 * r );
            }
            
            //////////////////////////////////////////////////////////////////////////////
            sampler2D _GradientTex;
            float4 _GradientTex_ST;

			sampler2D _GroundTex;
			float4 _GroundTex_ST;

            float _EllipseSize;
            float _GridNumber;

            v2f vert (appdata v)
            {
				v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _GradientTex);
				//VertexOutput o = (VertexOutput)0;
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				//o.vertex = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            float2 remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float ellipse(float2 UV, float Size)
            {
                float d = length(2 * UV - 1);
                return step(d, Size);
            }

            fixed2 posterize(fixed2 In, fixed Steps)
            {
                return floor(In * Steps) / Steps;     
            }

			//////////////////////////////////////////////////////////////////////////////
			float _Speed;
			float4 _Fbm_ScaleFactor;
			float _EllipseContrast;
			float4 _holePos;

			//v0.2
			float3x3 AngleAxis3(float angle, float3 axis)
			{
				float c, s;
				sincos(angle, s, c);
				
				float x = axis.x;
				float y = axis.y;
				float z = axis.z;
				float t = 1 - c;

				return float3x3(
					t * pow(x,2) + c, t * x * y - s * z, t * x * z + s * y,
					t * x * y + s * z, t * pow(y, 2) + c, t * y * z - s * x,
					t * x * z - s * y, t * y * z + s * x, t * pow(z, 2) + c
					);
			}

            fixed4 frag (v2f i) : SV_Target
            {
                #define TIME_1 (_Time.y * (-0.1) * _Speed)
                #define TIME_2 (_Time.y * (-0.3) * _Speed)
                #define TIME_3 (_Time.y * (0.15) * _Speed)
                #define SIN_TIME_3 (4.0 * sin(TIME_3))

				//#define ScaleFactor_1 float4(1.0, 1.0, 4.0, 4.0)
				#define ScaleFactor_1 _Fbm_ScaleFactor
                #define ScaleFactor_2 4.0
                #define AddFactor_1 float4(TIME_1, TIME_2, 5.2, 1.3)
                #define AddFactor_2 float4(SIN_TIME_3, 9.2, 9.3, 2.8)

                #define GRID_N _GridNumber
                #define UV_Repeat frac(i.uv * GRID_N)
                #define UV_Posterized posterize(i.uv, GRID_N)

                // get domain warping value
                float domainWarping = pattern(UV_Posterized, ScaleFactor_1, ScaleFactor_2, AddFactor_1, AddFactor_2);

                // remap value
				domainWarping = remap(domainWarping, float2(0.39, 0.83), float2(0, 1));

				float4 gradientTex = tex2D(_GradientTex, domainWarping);



				//v0.2 - holes
				float4 groundTex = tex2D(_GroundTex, i.uv * _GroundTex_ST.xy + _GroundTex_ST.zw);
				//float  _DistortionStrength;
				//float  _HoleSize;
				//float  _HoleEdgeSmoothness;
				//float4 _HoleNumberDistrib;
				i.normalDir = normalize(i.normalDir);
				float3 viewDirection = i.normalDir + normalize( _holePos.rgb - i.posWorld.xyz);  //_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float3 normalDirection = i.normalDir;
				float2 sceneUVs = i.uv;// (float2(i.projPos.x, i.projPos.y) / i.projPos.w) + float2(0, 0);
			//	float2 sceneUVs = (i.projPos.xy / (_Time.y*5+_holePos.w) * i.projPos.w) + float2(0, 0);
				float4 sceneColor = gradientTex;// tex2D(_GrabTexture, sceneUVs);
				////// Lighting:
				////// Emissive:
				float node_9892 = (_HoleSize*-1.0 + 1.0);
				float node_1841 = smoothstep((node_9892 + _HoleEdgeSmoothness), (node_9892 - _HoleEdgeSmoothness), (1.0 - pow(1.0 - max(0, dot(normalDirection, viewDirection)), 0.15))); // Create the hole mask
				float node_3969 = (1.0 - pow(1.0 - max(0, dot(normalDirection, viewDirection)), _DistortionStrength));
				float distortHole = _holePos.w*1.8;// 2;
				float3 emissive = (node_1841*tex2D(_GradientTex, ((pow(node_3969, 6.0)*(sceneUVs.rg*1*distortHole + 1.0)) + sceneUVs.rg)).rgb);
				//emissive = saturate(out1.rgb*(node_1841* ((pow(node_3969, 6.0)*(sceneUVs.rgg*-2.0 + 1.0)) + sceneUVs.rgg)));
				//gradientTex.rgb = saturate(5*pow(1 - (emissive),4) * gradientTex.rgb) + 1* gradientTex.rgb;
				//gradientTex.rgb = gradientTex.rgb +1/( _holePos.w*gradientTex.rgb*node_3969);
				float node_18411 = smoothstep((node_9892 + _HoleEdgeSmoothness), (node_9892 - _HoleEdgeSmoothness),
					(1.0 - pow(1.0 - fbm(max(0, dot(abs(normalDirection), abs(viewDirection)))), 0.15)));
				gradientTex.rgb = gradientTex.rgb + (_holePos.w*(gradientTex.rgb)*node_18411  );
				gradientTex.rgb = gradientTex.rgb*1 - 191*gradientTex.rgb*pow(groundTex.rgb,1.5);
				
				


				//CLAMP BLACK
				if (gradientTex.r < thres.w && gradientTex.g < thres.w && gradientTex.b < thres.w) {
					gradientTex = gradientTex + float4(1, 1, 1, 1)*thres.rgbb * pow(length(thres.rgb - gradientTex.rgb), colors_Factor.w);
				}


				float4 out1= ellipse(UV_Repeat, domainWarping * _EllipseSize) * gradientTex;

				//TINT
				out1 = float4(out1.r + colors_Factor.x, out1.g + colors_Factor.y, out1.b + colors_Factor.z, out1.a );

				//CLAMP BLACK
				//if (out1.r < thres.w && out1.g < thres.w && out1.b < thres.w) {
				//	out1 = out1+float4(1, 1, 1, 1)*thres.rgbb * length(thres.rgb - out1.rgb);
				//}

				if (out1.r < _darkColor.w && out1.g < _darkColor.w && out1.b < _darkColor.w) {
					out1.rgb = out1.rgb + _darkColor.rgb* abs(out1.r - _darkColor.w)* abs(out1.g - _darkColor.w)* abs(out1.b - _darkColor.w);
				}

				//hole2				
				float3 planetCenter = _planetPos.xyz;// float3(2.04, 10.32, 3.51);
				//
				float3 toCenterVec = (i.posWorld.xyz - planetCenter);
				float degreesRot = 1*length(normalize(_hole2Pos.rgb) - normalize(toCenterVec));
				toCenterVec = mul(toCenterVec, AngleAxis3(degreesRot, _hole2Pos.xyz) );
				//float3 holePos = _holePos.rgb;
				float dotPROD = pow(dot(normalize(_hole2Pos.rgb), normalize(toCenterVec)), 31);  //float3(0,1,-0.6)), normalize(i.posWorld.xyz - planetCenter)), 31);
				if (dotPROD > 1- _hole2Pos.w*(pow(domainWarping,0.25))) {
					out1.rgb = float3(5, 311, 0) * out1.rgb * (1) + float3(110,11151,10)*groundTex - float3(1,1,1)*_planetPos.w;
				}

				return  out1;
            }
            ENDCG
        }
    }
}
