// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "ORION/Celestial/MoonA HDRP"
{
	Properties
	{
		[Header(Colours)]
		_PrimaryColA ("Primary A", Color) = (1,1,1,1)
		_SecondaryColA ("Secondary A", Color) = (1,1,1,1)
		_PrimaryColB ("Primary B", Color) = (1,1,1,1)
		_SecondaryColB ("Secondary B", Color) = (1,1,1,1)
		_EjectaCol ("Ejecta Col", Color) = (1,1,1,1)

		_BiomeBlendStrength ("Biome Blend", Float) = 2
		_BiomeWarpStrength("Biome Warp", Float) = 0
		[Toggle()]
      _UseEjecta("Use Ejecta", float) = 1
		_EjectaBrightness("Ejecta Brightness", Float) = 0.1

		[Header(Normals)]
		[NoScaleOffset] _CraterRay ("Crater Ray", 2D) = "white" {}
		[NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _NormalMapFlat ("Normal Map Flat", 2D) = "bump" {}
		[NoScaleOffset] _NormalMapSteep ("Normal Map Steep", 2D) = "bump" {}
		_NormalMapFlatScale ("Normal Map Flat Scale", Float) = 10
		_NormalMapSteepScale ("Normal Map Steep Scale", Float) = 10
		_MainTexScale ("Main Tex Scale", Float) = 10

		_RandomBiomeValues ("Random Biome Values", Vector) = (0,0,0,0)
		_NormalMapStrength ("Normal Map Strength", Range(0,1)) = 0.3

		[Header(Other)]
		_FresnelCol("Fresnel Colour", Color) = (1,1,1,1)
		_FresnelStrengthNear("Fresnel Strength Min", float) = 2
		_FresnelStrengthFar("Fresnel Strength Max", float) = 5
		_FresnelPow("Fresnel Power", float) = 12

		_SmoothnessA ("Smoothness A", Range(0,1)) = 0.5
		_SmoothnessB ("Smoothness B", Range(0,1)) = 0.5
		_SmoothnessEjecta ("Smoothness Ejecta", Range(0,1)) = 0.5
		_MetallicA ("Metallic A", Range(0,1)) = 0.0
		_MetallicB ("Metallic B", Range(0,1)) = 0.0

		_TestParams ("Test Params", Vector) = (0,0,0,0)

	 }
	 SubShader
	 {
		Tags { "RenderType"="Opaque" }
		//LOD 200
			Pass {
		CGPROGRAM
		 #pragma vertex vert
				#pragma fragment frag
		//#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma target 3.5


			#include "UnityCG.cginc"
			////HDRP
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

		#include "./Includes/Triplanar.cginc"
		#include "./Includes/Math.cginc"

		sampler2D _MainTex;
		sampler2D _CraterRay;
		sampler2D _NormalMapFlat;
		sampler2D _NormalMapSteep;

		float _NormalMapStrength;
		float _NormalMapFlatScale;
		float _NormalMapSteepScale;
		float _MainTexScale;

		float _SmoothnessA;
		float _SmoothnessB;
		float _SmoothnessEjecta;
		float _MetallicA;
		float _MetallicB;

		float2 heightMinMax;

		float _EjectaBrightness;
		float _UseEjecta;

		float _BiomeBlendStrength;
		float _BiomeWarpStrength;
		float4 _RandomBiomeValues;
		float4 _TestParams;
		float _AvgBiomeNoiseDst;

		// Colors:
		float4 _PrimaryColA;
		float4 _SecondaryColA;
		float4 _PrimaryColB;
		float4 _SecondaryColB;
		float4 _EjectaCol;

		float4 _FresnelCol;
		float _FresnelStrengthNear;
		float _FresnelStrengthFar;
		float _FresnelPow;
		float bodyScale;

	//	struct Input
	//	{
	//		float3 vertPos;
	//		float3 normal;
	//		float4 terrainData;
	//		float3 worldNormal;
	//		float4 tangent;
	//		float4 craterUV;
	//		float fresnel;
	//		INTERNAL_DATA
	//	};

	//	void vert(inout appdata_full v, out Input o) {
	//		UNITY_INITIALIZE_OUTPUT(Input,o);
	//		o.vertPos = v.vertex;
	//		o.terrainData = v.texcoord;
	//		o.normal = v.normal;
	//		o.tangent = v.tangent;
	//		//
	//		float craterDst = v.texcoord.y;
	//		float2 craterUV = 0.5 + float2(cos(v.texcoord.x), sin(v.texcoord.x)) * craterDst;
	//		o.craterUV = float4(craterUV.xy, craterDst, 0);

	//		// Fresnel (fade out when close to body)
	//		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	//		float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	//		float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
	//		float fresnelT = smoothstep(0,1,camRadiiFromSurface);
	//		float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
	//		float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,0)));
	//		float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
	//		o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));
	//	}

	//	fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
	//	{
	//		fixed4 c;
	//		c.rgb = s.Albedo * 0.8;
	//		c.a = s.Alpha;
	//		return c;
	//	}



	//	void surf(Input IN, inout SurfaceOutputStandard o)
	//	{
	//		float4 terrainData = IN.terrainData;
	//		float height01 = remap01(length(IN.vertPos), heightMinMax.x, heightMinMax.y);

	//		// Calculate steepness: 0 where totally flat, 1 at max steepness
	//		const float maxSteepnessApprox = 0.3;
	//		float steepness = 1 - dot(IN.normal, normalize(IN.vertPos));
	//		steepness = remap01(steepness, 0, maxSteepnessApprox);

	//		float warpNoise = Blend(0, 3, IN.terrainData.z);
	//		// Sample noise texture
	//	float4 texNoise = triplanar(IN.vertPos, IN.normal, _MainTexScale, _MainTex);

	//	// Sample normal maps:
	//	// There are two maps, one for steep slopes like mountains and crater walls, and one for flat regions
	//	// Slopes always use the steep map, but flat regions blend between the flat and steep maps to add variety
	//	float3 normalMapFlat = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _NormalMapFlatScale, IN.tangent, _NormalMapFlat);
	//	float3 normalMapSteep = triplanarNormalTangentSpace(IN.vertPos, IN.normal, _NormalMapSteepScale, IN.tangent, _NormalMapSteep);
	//	//float normalBlend = Blend(_AvgBiomeNoiseDst * 0.8, 1.2, terrainData.w);
	//	//float normalBlend = warpNoise;
	//	float normalBlend = lerp(texNoise.r, texNoise.g, Blend(0, 2, terrainData.z));
	//	float3 flatAndSteepNormal = lerp(normalMapFlat, normalMapSteep, normalBlend);
	//	float3 normal = lerp(flatAndSteepNormal, normalMapSteep, steepness);
	//	normal = lerp(float3(0,0,1), normal, _NormalMapStrength);
	//	o.Normal = normal;


	//	// Blend between primary and secondary colours by height (plus some noise)
	//	float heightNoiseA = -texNoise.g * steepness - (texNoise.b - 0.5) * 0.7 + (texNoise.a - 0.5) * _RandomBiomeValues.x;
	//	float heightNoiseB = (texNoise.g - 0.5) * _RandomBiomeValues.y + (texNoise.r - 0.5) * _RandomBiomeValues.z;
	//	float heightBlendWeightA = Blend(0.5, 0.6, height01 + heightNoiseA) * warpNoise;
	//	float heightBlendWeightB = Blend(0.5, 0.6, height01 + heightNoiseB) * warpNoise;
	//	float3 colBiomeA = lerp(_PrimaryColA, _SecondaryColA, heightBlendWeightA);
	//	float3 colBiomeB = lerp(_PrimaryColB, _SecondaryColB, heightBlendWeightB);

	//	// Blend between colour A and B based on terrain data noise
	//	float biomeNoise = dot(texNoise.ga - 0.5, _RandomBiomeValues.zw) * 4;
	//	float biomeWeight = Blend(_AvgBiomeNoiseDst * 0.8 + terrainData.z * _BiomeWarpStrength, _BiomeBlendStrength + warpNoise * 15, terrainData.w + biomeNoise);
	//	float3 biomeCol = lerp(colBiomeA, colBiomeB, biomeWeight);

	//	// Crater ejecta
	//	float ejecta = tex2D(_CraterRay, IN.craterUV.xy).r;
	//	float craterAlpha = IN.craterUV.z < 1;
	//	craterAlpha = abs(IN.terrainData.y) < 0.5;
	//	float3 compositeCol = lerp(biomeCol, _EjectaCol, saturate(ejecta * 1.5) * craterAlpha * _UseEjecta);
	//	compositeCol = lerp(compositeCol, _FresnelCol, IN.fresnel);
	//	o.Albedo = compositeCol;

	//	// Set metallic and smoothness
	//	o.Metallic = lerp(_MetallicA, _MetallicB, biomeWeight);;
	//	float smoothness = lerp(_SmoothnessA, _SmoothnessB, biomeWeight);
	//	smoothness = lerp(smoothness, _SmoothnessEjecta, saturate(ejecta * 1.5) * craterAlpha);
	//	o.Smoothness = smoothness;

	//}


struct v2f
{
	//float2 uv : TEXCOORD0;
	//fixed4 diff : COLOR0;
	//float4 vertex : SV_POSITION;

	float4 vertex: SV_POSITION;
	float3 normal: NORMAL;
	float4 terrainData: TEXCOORD0;
	float3 worldNormal: TEXCOORD1;
	float4 tangent: TANGET;
	float4 craterUV: TEXCOORD2;
	float fresnel: TEXCOORD3;

	float3 lightdir : TEXCOORD4;
	float3 viewdir : TEXCOORD5;
	float3 worldTangent : TEXCOORD6;
	float3 worldBinormal : TEXCOORD7;
	//INTERNAL_DATA
};

v2f vert(appdata_full v)
{
	v2f o;
	//o.vertPos = UnityObjectToClipPos(v.vertex);
	//o.uv = v.texcoord;
	//half3 worldNormal = UnityObjectToWorldNormal(v.normal);
	//half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
	//o.diff = nl * _LightColor0;

	//// the only difference from previous shader:
	//// in addition to the diffuse lighting from the main light,
	//// add illumination from ambient or light probes
	//// ShadeSH9 function from UnityCG.cginc evaluates it,
	//// using world space normal
	//o.diff.rgb += ShadeSH9(half4(worldNormal, 1));

	UNITY_INITIALIZE_OUTPUT(v2f, o);
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.terrainData = v.texcoord;
	o.normal = v.normal;
	o.tangent = v.tangent;
	//
	float craterDst = v.texcoord.y;
	float2 craterUV = 0.5 + float2(cos(v.texcoord.x), sin(v.texcoord.x)) * craterDst;
	o.craterUV = float4(craterUV.xy, craterDst, 0);

	// Fresnel (fade out when close to body)
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
	float3 bodyWorldCentre = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
	float camRadiiFromSurface = (length(bodyWorldCentre - _WorldSpaceCameraPos.xyz) - bodyScale) / bodyScale;
	float fresnelT = smoothstep(0, 1, camRadiiFromSurface);
	float3 viewDir = normalize(worldPos - _WorldSpaceCameraPos.xyz);
	float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)));
	float fresStrength = lerp(_FresnelStrengthNear, _FresnelStrengthFar, fresnelT);
	o.fresnel = saturate(fresStrength * pow(1 + dot(viewDir, normWorld), _FresnelPow));
	o.worldNormal =  UnityObjectToWorldNormal(v.normal);// normalize(mul(float4(v.normal, 0.0), unity_ObjectToWorld).xyz);


	//MINE	
	float3 lightDir = worldPos.xyz - _WorldSpaceLightPos0.xyz - _WorldSpaceCameraPos.xyz;;
	o.lightdir = normalize(lightDir);	
	o.viewdir = viewDir;
	float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
	float3 worldTangent = mul((float3x3)unity_ObjectToWorld, v.tangent);
	float3 binormal = cross(v.normal, v.tangent.xyz); // *input.tangent.w;
	float3 worldBinormal = mul((float3x3)unity_ObjectToWorld, binormal);
	o.worldTangent = normalize(worldTangent);
	o.worldBinormal = normalize(worldBinormal);
	return o;
}

//sampler2D _MainTex;

float4 frag(v2f IN) : SV_Target
{
	float4 terrainData = IN.terrainData;
	float height01 = remap01(length(IN.vertex), heightMinMax.x, heightMinMax.y);

	// Calculate steepness: 0 where totally flat, 1 at max steepness
	const float maxSteepnessApprox = 0.3;
	float steepness = 1 - dot(IN.normal, normalize(IN.vertex));
	steepness = remap01(steepness, 0, maxSteepnessApprox);

	float warpNoise = Blend(0, 3, IN.terrainData.z);
	// Sample noise texture
	float4 texNoise = triplanar(IN.vertex, IN.normal, _MainTexScale, _MainTex);

		// Sample normal maps:
		// There are two maps, one for steep slopes like mountains and crater walls, and one for flat regions
		// Slopes always use the steep map, but flat regions blend between the flat and steep maps to add variety
		float3 normalMapFlat = triplanarNormalTangentSpace(IN.vertex, IN.normal, _NormalMapFlatScale, IN.tangent, _NormalMapFlat);
		float3 normalMapSteep = triplanarNormalTangentSpace(IN.vertex, IN.normal, _NormalMapSteepScale, IN.tangent, _NormalMapSteep);
		//float normalBlend = Blend(_AvgBiomeNoiseDst * 0.8, 1.2, terrainData.w);
		//float normalBlend = warpNoise;
		float normalBlend = lerp(texNoise.r, texNoise.g, Blend(0, 2, terrainData.z));
		float3 flatAndSteepNormal = lerp(normalMapFlat, normalMapSteep, normalBlend);
		float3 normal = lerp(flatAndSteepNormal, normalMapSteep, steepness);
		normal = lerp(float3(0,0,1), normal, _NormalMapStrength);
//		o.Normal = normal;


		// Blend between primary and secondary colours by height (plus some noise)
		float heightNoiseA = -texNoise.g * steepness - (texNoise.b - 0.5) * 0.7 + (texNoise.a - 0.5) * _RandomBiomeValues.x;
		float heightNoiseB = (texNoise.g - 0.5) * _RandomBiomeValues.y + (texNoise.r - 0.5) * _RandomBiomeValues.z;
		float heightBlendWeightA = Blend(0.5, 0.6, height01 + heightNoiseA) * warpNoise;
		float heightBlendWeightB = Blend(0.5, 0.6, height01 + heightNoiseB) * warpNoise;
		float3 colBiomeA = lerp(_PrimaryColA, _SecondaryColA, heightBlendWeightA);
		float3 colBiomeB = lerp(_PrimaryColB, _SecondaryColB, heightBlendWeightB);

		// Blend between colour A and B based on terrain data noise
		float biomeNoise = dot(texNoise.ga - 0.5, _RandomBiomeValues.zw) * 4;
		float biomeWeight = Blend(_AvgBiomeNoiseDst * 0.8 + terrainData.z * _BiomeWarpStrength, _BiomeBlendStrength + warpNoise * 15, terrainData.w + biomeNoise);
		float3 biomeCol = lerp(colBiomeA, colBiomeB, biomeWeight);

		// Crater ejecta
		float ejecta = tex2D(_CraterRay, IN.craterUV.xy).r;
		float craterAlpha = IN.craterUV.z < 1;
		craterAlpha = abs(IN.terrainData.y) < 0.5;
		float3 compositeCol = lerp(biomeCol, _EjectaCol, saturate(ejecta * 1.5) * craterAlpha * _UseEjecta);
		compositeCol = lerp(compositeCol, _FresnelCol, IN.fresnel);
	//	o.Albedo = compositeCol;

		// Set metallic and smoothness
	//	o.Metallic = lerp(_MetallicA, _MetallicB, biomeWeight);;
		float smoothness = lerp(_SmoothnessA, _SmoothnessB, biomeWeight);
		smoothness = lerp(smoothness, _SmoothnessEjecta, saturate(ejecta * 1.5) * craterAlpha);
	//	o.Smoothness = smoothness;

		//fixed4 col = tex2D(_MainTex, IN.terrainData);
		//col *= i.diff;

		//MINE
		float3 lightDir = normalize(_WorldSpaceLightPos0);// IN.lightdir);
		float3 diffuse = saturate(dot(IN.worldNormal, -lightDir));
		diffuse = 1 * compositeCol.rgb * diffuse;

		// Specular here
		float3 specular = 0;
		if (diffuse.x > 0) {
			float3 reflection = reflect(lightDir, IN.normal);
			float3 viewDir = normalize(IN.viewdir);

			specular = saturate(dot(reflection, -viewDir));
			specular = pow(specular, 20.0f);

			//float4 specularIntensity = tex2D(_SpecularMap, IN.uv);
			//specular *= _LightColor0 * 1;
		}

	return float4(diffuse + specular + smoothness,1)*1.5 ;
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
//
//	//HDRP
//	//o.pos = UnityObjectToClipPos(v.vertex);
//	//ComputeScreenAndGrabPassPos(o.pos, o.screenPos, o.grabPassPos);
//	//END HDRP
//
//	//COMPUTE_EYEDEPTH(o.projPos.z);
//	return o;
//}
//float4 frag(VertexOutput i) : COLOR{
//	return float4(1,0,0,1);
//}





	ENDCG
		}
	}
	//FallBack "Diffuse"
}