Shader "ORION/NebulaTut STARS LIGHTS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Tex2 ("_Tex2", 2D) = "white" {}
		_MaskTex ("_MaskTex", 2D) = "white" {}
		_Distort("_Distort", Float) = 0.5
			_HighLight("_HighLight", Color) = (1,1,1,1)
			_Color("_Color", Color) = (1,1,1,1)
			_Pow("_Pow", Float) = 0.5

			//STARS
			_StarsColor("Light Color", Color) = (1,1,1,1)
		_Ambient("Ambient Color", Color) = (0,0,0,0)
		_StarsMainTex("Base (RGB)", 2D) = "white" {}
		_Jitter("Jitter", 2D) = "white" {}
		_SunDir("Sun Direction", Vector) = (0,1,0,0)
		_CloudCover("Cloud Cover", Range(0,1)) = 0.5
		_CloudSharpness("Cloud Sharpness", Range(1,30)) = 8
		_CloudDensity("Density", Range(0,5)) = 1
		_CloudSpeed("Cloud Speed", Vector) = (0.001, 0, 0, 0)
		_Light("Sun Intensity", Range(0,10)) = 1
		_Bump("Bump", 2D) = "white" {}
		increaseGalaxy("Increase Galaxy Effect", Vector) = (1, 1, 1, 1)
		cloudSpeed("Increase cloud Speed ", Vector) = (1, 1, 1, 1)
		brightnessContrast("Brightness-Contrast ", Vector) = (1, 1, 1, 1)

		edgeControlA("edge Controls A ", Vector) = (1, 1, 1, 1)
		edgeControlB("edge Controls B ", Vector) = (1, 1, 1, 1)
		edgeMode("edge mode Controls", Vector) = (0, 0, 0, 0)
	}
		SubShader
	{
		Tags {  "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		ZWrite Off
		 Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
			Cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

		float _Distort;
		float4 _HighLight;
		float4 _Color;
		float _Pow;
		float4 edgeMode;

		//STARS
		sampler2D _StarsMainTex;
		float4 _StarsMainTex_ST;
		float4 _Bump_ST;
		sampler2D _Jitter;
		sampler2D _Bump;
		float4 _StarsColor;
		float4 _Ambient;
		float4 _SunDir;
		float4 _CloudSpeed;
		float _Light;
		float _CloudCover;
		uniform float _CloudDensity;
		float _CloudSharpness;
		float4 increaseGalaxy;
		float4 cloudSpeed;
		float4 brightnessContrast;

            /*struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };*/

            struct v2f
            {
                float2 uv : TEXCOORD0;
               // UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float4 tex : TEXCOORD1;				
				float3 dir:TEXCOORD2;
				float2 uv2:TEXCOORD3;
				UNITY_FOG_COORDS(4)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _Tex2;
			float4 _Tex2_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float4 edgeControlA;
			float4 edgeControlB;

            v2f vert (appdata_tan v)//(appdata v)
            {
               // v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
               // return o;

				//STARS
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 vertnorm = normalize(v.vertex.xyz);
				float2 vertuv = vertnorm.xz / (vertnorm.y + 0.2);
				//o.uv = TRANSFORM_TEX(v.texcoord,_Bump);
				o.uv = v.texcoord;
				o.uv2 = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.tex = float4(vertuv.xy * 0.2, 0, 0);
				TANGENT_SPACE_ROTATION;
				o.dir = mul(rotation, _SunDir.xyz);
				//TRANSFER_VERTEX_TO_FRAGMENT(o); 
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
            }

			//https://www.codementor.io/@tlhm/procedural-generation-visual-rendering-unity3d-du107jjmr
			//fragment shader
			float4 frag(v2f i) : COLOR{
				//Get the colors at the right point on the first texture
				half4 col = tex2D(_MainTex,i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);

				//Use that to create an offset for the second texture
				float2 offset = float2(_Distort*(col.x - .5),_Distort*(col.y - .5)) + float2(_Time.y*0.001*cloudSpeed.x, _Time.y*0.001*cloudSpeed.y);

				//Get the colors from the second texture, using the offset to distort the image
				half4 col2 = tex2D(_Tex2,i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw + offset);

				//Create a circular mask: if we're close to the edge the value is 0
				//If we're by the center the value is 1
				//By multipling the final alpha by this, we mask the edges of the box
				fixed radA = max(1 - max(length(half2(.5,.5) - i.uv.xy) - .25,0) / .25,0);

				//Get the mask color from our mask texture
				half4 mask = tex2D(_MaskTex,i.uv.xy*_MaskTex_ST.xy + _MaskTex_ST.zw);

				//Add the color portion : apply the gradient from the highlight to the color
				//To the gray value from the blue channel of the distorted noise
				float3 final_color = lerp(_HighLight, _Color, col2.b*.5).rgb * 1;

				//calculate the final alpha value:
				//First combine several of the distorted noises together
				float final_alpha = col2.a*col2.g*col.b;

				//Apply the a combination of two tendril masks
				final_alpha *= mask.g*mask.r;

				//Apply the circular mask
				final_alpha *= radA;

				//Raise it to a power to dim it a bit 
				//it should be between 0 and 1, so the higher the power
				//the more transparent it becomes
				final_alpha = pow(final_alpha, _Pow);

				//Finally, makes sure its never more than 90% opaque
				final_alpha = min(final_alpha, .9);

				//v0.1
				final_color = pow(final_color, 4);
				float smoothOffset = 0.005 * edgeControlB.x;
				if (//(final_alpha < 0.12 + smoothOffset && final_alpha > 0.1 - smoothOffset) ||
					 (final_alpha < 0.34 + smoothOffset && final_alpha > 0.31 - smoothOffset)) {
					//final_color = final_color + saturate(float3(100,30,0) * final_color) * 7;
					//increaseGalaxy = 0;
				}
				/*
				if ((final_alpha < 0.12 && final_alpha > 0.1) || (final_alpha < 0.34 && final_alpha > 0.31)) {
					final_color = saturate(float3(100, 30, 0) * final_color) * 5;
				}*/

				//STARS
				float2 TexC = float2(i.uv.x, i.uv.y);
				float2 offsetA = _Time.y * _CloudSpeed.xy;
				// float4 tex = tex2D( _MainTex, (( i.uv2 ) + offsetA)*10.5  );       
				float4 tex = tex2D(_StarsMainTex, TexC * _CloudDensity);
				//float Density = 0;
				// fixed4 tex2N = tex2D( _Bump,i.uv2 + offsetA);
				//fixed3 UnpackedN = UnpackNormal(tex2N) * _Light;
				//fixed Bumped_lightingN = dot( UnpackedN, i.dir);
				fixed Bumped_lightingN = 1;
				fixed4 tex2N = tex2D(_Jitter, (TexC + offsetA / 15) * _CloudDensity * 11);
				//fixed4 tex2N = tex2D( _Jitter,TexC  );
				tex = max(tex - (1 - _CloudCover * 2), 0);
				float4 res = _StarsColor.a * lerp(pow(tex2N, 2), 0.6, _CloudSharpness) *
						float4 (0.95 * _StarsColor.r*Bumped_lightingN * tex.r,
						0.95* _StarsColor.g*Bumped_lightingN * tex.g,
						0.95 * _StarsColor.b*Bumped_lightingN * tex.b,
						_StarsColor.a);
				//  res.xyz += _Ambient.xyz;
				float4 starPower = pow(abs(res), _Light);
				//END STARS

				if (final_alpha < 0.12 + smoothOffset + edgeControlB.z && final_alpha > 0.1 - smoothOffset + edgeControlB.w) {
					//starPower =1;
					float dist = abs(final_alpha - smoothOffset);
					starPower = (1 * lerp(starPower, float3(4, 2, 0)*starPower, edgeControlA.x *12*pow(dist, edgeControlA.y * 3) / 0.02)).r * 1.2*edgeControlA.z;
					//increaseGalaxy = 0;
					final_color =  (float3(15, 1, 0) * final_color) * 2 * edgeControlB.y;
					final_alpha = edgeControlA.w *0.05;

					if (edgeMode.x != 0) {
						final_alpha = edgeControlA.w *0.05 * dist;
					}
					if (edgeMode.y != 0) {
						final_alpha = edgeControlA.w *0.05 / (dist *edgeMode.y);
					}

					//final_color = 0 + saturate(float3(414, 111, 0) * final_color) * 5  ;
					//final_color = 0 + (float3(414, 1111, 0) * final_color) * 15 * pow(dist, 10);
					//final_color = 1*lerp(final_color, float3(4, 2, 0)*final_color, pow(dist, 1)/0.02);
				}


				//We're done! Return the final pixel color!
				float4 finalOUT = float4(final_color, final_alpha) * starPower + starPower * 0.03 * increaseGalaxy;

				finalOUT = pow(finalOUT, brightnessContrast.y)* brightnessContrast.x;

				return finalOUT;
			}

            //fixed4 frag (v2f i) : SV_Target
            //{
            //    // sample the texture
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    // apply fog
            //    UNITY_APPLY_FOG(i.fogCoord, col);
            //    return col;
            //}
            ENDCG
        }
    }
}
