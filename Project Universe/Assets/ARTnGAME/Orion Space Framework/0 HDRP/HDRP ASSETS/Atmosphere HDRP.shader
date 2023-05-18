Shader "ORION/Earth/Atmosphere HDRP"
{
    Properties
    {
        _NightMap ("Night Light Map", 2D) = ""{}
        _NightColor ("Night Light", Color) = (0.5, 0.5, 0.3, 0)

        _RimColor ("Rim Color", Color) = (0.5, 0.5, 0.5, 0.0)
        _RimPower ("Rim Power", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		 Pass {

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			//v4.2
			Cull[_CullMode]


		CGPROGRAM
		 #pragma vertex vert
				#pragma fragment frag
		//#pragma surface surf Earth alpha nolightmap
		#pragma target 3.0

			#include "UnityCG.cginc"
			////HDRP
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

		sampler2D _NightMap;
		half3 _NightColor;

		half3 _RimColor;
		half _RimPower;



		struct v2f
		{
			float2 uv : TEXCOORD0;
			fixed4 diff : COLOR0;
			float4 vertex : SV_POSITION;
			//float3 lightdir : TEXCOORD1;
			float3 viewdir : TEXCOORD1;
			float3 normal: NORMAL;
			float3 worldNormal: TEXCOORD2;
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

			//
			o.normal = v.normal;
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
			o.viewdir = viewDir;

			return o;
		}

		sampler2D _MainTex;

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_NightMap, i.uv);
			col *= i.diff;

			half ln = dot(_WorldSpaceLightPos0, i.worldNormal);
			half vn = dot(i.viewdir, i.worldNormal);

			half rim = pow(1 - vn, _RimPower) * ln;// *atten;
			half night = -ln * ln * ln *col.r;// s.Albedo.r;

			half4 c;
			c.rgb = lerp(_NightColor, _RimColor * float3(1, 1, 1), ln > 0);// _LightColor0.rgb, ln > 0);
			c.a = max(rim, night);

			col = col * c;

			return float4(col.rgb,c.a);
		}


		//struct VertexInput {
		//	float4 vertex : POSITION;
		//	float3 normal : NORMAL;
		//};
		//struct VertexOutput {
		//	float4 pos : SV_POSITION;
		//	//float4 posWorld : TEXCOORD0;
		//	float3 localNormal : TEXCOORD1;
		//	//float4 projPos : TEXCOORD2;
		//	//float4 screenPos : TEXCOORD3; //HDRP
		//	//float4 grabPassPos : TEXCOORD4;//HDRP
		//};
		//VertexOutput vert(VertexInput v) {
		//	VertexOutput o = (VertexOutput)0;
		//	//	 o.normalDir = UnityObjectToWorldNormal(v.normal);
		//	o.localNormal = v.normal * float3(-1, 1, 1);
		//	//o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		//	o.pos = UnityObjectToClipPos(v.vertex);
		//	//o.projPos = ComputeScreenPos(o.pos);

		//	//HDRP
		//	//o.pos = UnityObjectToClipPos(v.vertex);
		//	//ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);
		//	//END HDRP

		//	//COMPUTE_EYEDEPTH(o.projPos.z);
		//	return o;
		//}
		//float4 frag(VertexOutput i) : COLOR{
		//	return float4(1,0,0,1);
		//}




		/*struct Input
		{
			float2 uv_NightMap;
		};

		half4 LightingEarth(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half ln = dot(lightDir, s.Normal);
			half vn = dot(viewDir, s.Normal);

			half rim = pow(1 - vn, _RimPower) * ln * atten;
			half night = -ln * ln * ln * s.Albedo.r;

			half4 c;
			c.rgb = lerp(_NightColor, _RimColor * _LightColor0.rgb, ln > 0);
			c.a = max(rim, night);
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = tex2D(_NightMap, IN.uv_NightMap);
		}*/

		ENDCG
			}
    }
    Fallback "Diffuse"
}
