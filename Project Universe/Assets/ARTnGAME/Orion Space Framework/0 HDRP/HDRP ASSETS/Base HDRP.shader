Shader "ORION/Earth/Base HDRP"
{
    Properties
    {
        _BaseMap ("Base Map", CUBE) = ""{}
        _SeaColor ("Sea Color", Color) = (0, 0, 1, 0)
        _Saturation ("Saturation", Range(0,2)) = 1

        _NormalMap ("Normal Map", 2D) = ""{}
        _NormalScale ("Normal Scale", Range(0,2)) = 0.5

        _GlossMap ("Gloss Map", CUBE) = ""{}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        _CloudMap ("Cloud Map", CUBE) = ""{}
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.5)
    }
    SubShader
		{
			Tags { "RenderType" = "Opaque" }
			 Pass {
			CGPROGRAM
				 #pragma vertex vert
				#pragma fragment frag
			// #pragma surface surf Standard vertex:vert nolightmap addshadow
			 #pragma target 3.0

			#include "UnityCG.cginc"
			////HDRP
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			 samplerCUBE _BaseMap;
			 half4 _SeaColor;
			 half _Saturation;

			 sampler2D _NormalMap;
			 half _NormalScale;

			 samplerCUBE _GlossMap;
			 half _Glossiness;

			 samplerCUBE _CloudMap;
			 fixed4 _CloudColor;


			 struct v2f
			 {
				 float2 uv : TEXCOORD0;
				 fixed4 diff : COLOR0;
				 float4 vertex : SV_POSITION;
				 float3 localNormal: TEXCOORD1;
				 //float3 uv_NormalMap: TEXCOORD2;
			 };

			 v2f vert(appdata_base v)
			 {
				 v2f o;
				 o.vertex = UnityObjectToClipPos(v.vertex);
				 o.uv = v.texcoord;
				 half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				 half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				 o.diff = nl * _LightColor0;

				 // the only difference from previous shader:
				 // in addition to the diffuse lighting from the main light,
				 // add illumination from ambient or light probes
				 // ShadeSH9 function from UnityCG.cginc evaluates it,
				 // using world space normal
				 o.diff.rgb += ShadeSH9(half4(worldNormal, 1));
				 o.localNormal = v.normal * float3(-1, 1, 1);

				 return o;
			 }

			 sampler2D _MainTex;

			 fixed4 frag(v2f IN) : SV_Target
			 {
				

				// half4 col = texCUBE(_BaseMap, i.localNormal);

				// col *= i.diff;



				 half3 base = texCUBE(_BaseMap, IN.localNormal).rgb;
				 half4 normal = tex2D(_NormalMap, IN.uv);
				 half gloss = texCUBE(_GlossMap, IN.localNormal).r;
				 // half cloud = texCUBE(_CloudMap, IN.localNormal).r; //v0.1
				  //half cloud = texCUBE(_CloudMap, normalize(IN.localNormal + float3(0.1*abs(cos(_Time.y) + sin(_Time.y*0.95)), 0, 0))).r;
				 half cloud = texCUBE(_CloudMap, normalize(IN.localNormal + float3(0.065*abs(cos(_Time.y*0.1)), 0, 0))).r;
				 half cloud2 = texCUBE(_CloudMap, normalize(IN.localNormal + float3(0.045*abs(cos(_Time.y*0.15 + 122.1)), 0, 0))).r;

				 base = lerp((float3)Luminance(base), base, _Saturation);
				 base = lerp(base, _SeaColor.rgb, gloss * _SeaColor.a);
				 cloud = min(1, cloud * _CloudColor.a);
				 cloud2 = min(1, cloud2 * _CloudColor.a*0.2);//v0.1
				 cloud += cloud2;//v0.1

				 float3 outCol = lerp(base, _CloudColor.rgb, cloud);

				 //o.Albedo = lerp(base, _CloudColor.rgb, cloud);
				// o.Normal = UnpackScaleNormal(normal, _NormalScale);
				// o.Smoothness = _Glossiness * gloss * (1 - cloud);
				// o.Metallic = 0;
				// o.Alpha = 1;

				 return float4(outCol,1);
			 }



			// struct VertexInput {
			//	 float4 vertex : POSITION;
			//	 float3 normal : NORMAL;
			// };
			// struct VertexOutput {
			//	 float4 pos : SV_POSITION;
			//	 //float4 posWorld : TEXCOORD0;
			//	 float3 localNormal : TEXCOORD1;
			//	 //float4 projPos : TEXCOORD2;
			//	 //float4 screenPos : TEXCOORD3; //HDRP
			//	 //float4 grabPassPos : TEXCOORD4;//HDRP
			// };
			// VertexOutput vert(VertexInput v) {
			//	 VertexOutput o = (VertexOutput)0;
			////	 o.normalDir = UnityObjectToWorldNormal(v.normal);
			//	 o.localNormal = v.normal * float3(-1, 1, 1);
			//	 //o.posWorld = mul(unity_ObjectToWorld, v.vertex);
			//	 o.pos = UnityObjectToClipPos(v.vertex);
			//	 //o.projPos = ComputeScreenPos(o.pos);

			//	 //HDRP
			//	 //o.pos = UnityObjectToClipPos(v.vertex);
			//	 //ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);
			//	 //END HDRP

			//	 //COMPUTE_EYEDEPTH(o.projPos.z);
			//	 return o;
			// }
			// float4 frag(VertexOutput i) : COLOR{
			//	 return float4(1,0,0,1);
			// }







				 //     struct Input
				 //     {
				 //         float2 uv_NormalMap;
				 //         float3 localNormal;
				 //     };

				 //     void vert(inout appdata_full v, out Input o)
				 //     {
				 //         UNITY_INITIALIZE_OUTPUT(Input, o);
				 //         o.localNormal = v.normal * float3(-1, 1, 1);
				 //     }

				 //     void surf(Input IN, inout SurfaceOutputStandard o)
				 //     {
				 //         half3 base = texCUBE(_BaseMap, IN.localNormal).rgb;
				 //         half4 normal = tex2D(_NormalMap, IN.uv_NormalMap);
				 //         half gloss = texCUBE(_GlossMap, IN.localNormal).r;
				 //        // half cloud = texCUBE(_CloudMap, IN.localNormal).r; //v0.1
						  ////half cloud = texCUBE(_CloudMap, normalize(IN.localNormal + float3(0.1*abs(cos(_Time.y) + sin(_Time.y*0.95)), 0, 0))).r;
						  //half cloud = texCUBE(_CloudMap, normalize(IN.localNormal + float3(0.065*abs(cos(_Time.y*0.1)), 0, 0))).r;
						  //half cloud2 = texCUBE(_CloudMap, normalize(IN.localNormal + float3(0.045*abs(cos(_Time.y*0.15+122.1)), 0, 0))).r;

				 //         base = lerp((float3)Luminance(base), base, _Saturation);
				 //         base = lerp(base, _SeaColor.rgb, gloss * _SeaColor.a);
				 //         cloud = min(1, cloud * _CloudColor.a);
						  //cloud2 = min(1, cloud2 * _CloudColor.a*0.2);//v0.1
						  //cloud += cloud2;//v0.1

				 //         o.Albedo = lerp(base, _CloudColor.rgb, cloud);
				 //         o.Normal = UnpackScaleNormal(normal, _NormalScale);
				 //         o.Smoothness = _Glossiness * gloss * (1 - cloud);

				 //         o.Metallic = 0;
				 //         o.Alpha = 1;
				 //     }
		
		ENDCG
		}
    }
    FallBack "Diffuse"
}
