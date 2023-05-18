Shader "ORION/Sky/AtmosphereImageEffect"
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
	    Pass 
	    {
			//Blend SrcAlpha OneMinusSrcAlpha

			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
			//Blend One OneMinusSrcAlpha // Premultiplied transparency
			//Blend One One // Additive
			//Blend OneMinusDstColor One // Soft Additive
			//Blend DstColor Zero // Multiplicative
			//Blend DstColor SrcColor // 2x Multiplicative

	    	ZTest Off Cull Off ZWrite Off Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma target 4.0
			#include "Atmosphere.cginc"
			
			sampler2D _MainTex;
			
			struct appdata {
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			float imageFXBlend = 1;

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
				//float3 ray : TEXCOORD1;
			};
			
			v2f vert (appdata v)
			{
			    v2f o;
			    o.pos = UnityObjectToClipPos(v.vertex);
			    o.uv = ComputeScreenPos (o.pos);
			    //o.uv.y = 1-o.uv.y;
				//o.ray.xyz = mul(UNITY_MATRIX_MV, v.vertex).xyz * float3(-1.0, -1.0, 1.0);
			    return o;
			}

			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
			float4x4 _ViewProjectInverse;
			float4x4 _CameraInv;
			float4x4 _ViewMatrix;
			// float4x4 _CameraToWorld;
			float4 _CamScreenDir;
			float4 _LightDir;
			float4 _LightColor;
			float4x4 unity_WorldToLight;
			float _FarPlane;
			int invertY;

			half4 frag (v2f i) : COLOR
			{	
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				depth = Linear01Depth(depth);

				float4 normal = tex2D (_CameraDepthNormalsTexture, i.uv);
				half3 ray = float3(i.uv.x*2-1, i.uv.y*2-1, 1);
				
    			ray *= _CamScreenDir.xyz;
    			ray = ray * (_FarPlane / ray.z);
    			float3 viewNorm = mul(_CameraInv,normal);
				float4 vpos = float4(ray * depth,0.995);
    			float3 wpos = mul (unity_CameraToWorld, vpos).xyz;  
				
				
				
				float4 surfaceColor = tex2D (_MainTex, i.uv);

				if (invertY == 1) {
					surfaceColor = tex2D(_MainTex, float2(i.uv.x, 1 - i.uv.y));
				}


			    float3 viewDir = normalize(wpos-_WorldSpaceCameraPos.xyz);

				float3 attenuation;
				float irradianceFactor = 0;
				float3 inscat = GetInscatteredLight(wpos,viewDir,attenuation,irradianceFactor);
				float3 reflected = GetReflectedLight(wpos, depth, clamp(attenuation,0.8,1),irradianceFactor, normal,surfaceColor); // v1.0.83  clamp(attenuation,0.8,1)

				//return float4(reflected, 1);
				//return saturate(float4(inscat + reflected,imageFXBlend));
				//return saturate(float4(inscat + reflected + surfaceColor.rgb*2, imageFXBlend * (1-surfaceColor.r)));
				//return saturate(float4(inscat * 1 + reflected, imageFXBlend));

				return saturate(float4(inscat+reflected.rgb, imageFXBlend)); // v1.0.83
			}
			ENDCG
	    }
	}
}
