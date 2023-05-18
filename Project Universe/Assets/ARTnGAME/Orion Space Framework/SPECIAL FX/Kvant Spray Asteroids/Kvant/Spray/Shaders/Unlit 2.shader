// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// Transparent unlit shader for Spray
//
// Vertex format:
// position.xyz = vertex position
// texcoord.xy  = uv for GPGPU buffers
//
// Position buffer format:
// .xyz = particle position
// .w   = life (+0.5 -> -0.5)
//
// Rotation buffer format:
// .xyzw = particle rotation
//
Shader "ORION/Kvant/Spray/Transparent Unlit2"
{
    Properties
    {
        _PositionBuffer ("-", 2D) = "black"{}
        _RotationBuffer ("-", 2D) = "red"{}

        [Enum(Add, 0, AlphaBlend, 1)]
        _BlendMode ("-", Float) = 0

        [KeywordEnum(Single, Animate, Random)]
        _ColorMode ("-", Float) = 0

        [HDR] _Color  ("-", Color) = (1, 1, 1, 1)
        [HDR] _Color2 ("-", Color) = (0.5, 0.5, 0.5, 1)

        _MainTex ("-", 2D) = "white"{}

        _ScaleMin ("-", Float) = 1
        _ScaleMax ("-", Float) = 1

        _RandomSeed ("-", Float) = 0
    }

    CGINCLUDE

    #pragma shader_feature _COLORMODE_RANDOM
    #pragma shader_feature _MAINTEX
    #pragma multi_compile_fog
    #pragma multi_compile_instancing //NEW

    #include "UnityCG.cginc"
    #include "Common.cginc"

    sampler2D _MainTex;
    float4 _MainTex_ST;
    half _BlendMode;

    //NEW
	UNITY_INSTANCING_BUFFER_START(Props)
	//UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color) // Make _Color an instanced property (i.e. an array)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BufferOffset1)
#define _BufferOffset1_arr Props
	UNITY_INSTANCING_BUFFER_END(Props)
	//float2 _BufferOffset;

    struct appdata
    {
        float4 vertex : POSITION;
        float2 texcoord0 : TEXCOORD0;
        float2 texcoord1 : TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 position : SV_POSITION;
        float2 texcoord : TEXCOORD;
        half4 color : COLOR;
        UNITY_FOG_COORDS(1)
        //UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
    };

    v2f vert(appdata v)
    {   

    	v2f o;

    	//NEW
    	UNITY_SETUP_INSTANCE_ID(v);
        //UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.
    	float4 uv = float4(v.texcoord1.xy + UNITY_ACCESS_INSTANCED_PROP(_BufferOffset1_arr, _BufferOffset1).xy, 0, 0); //NEW

        //float4 uv = float4(v.texcoord1.xy + _BufferOffset, 0, 0);

        float4 p = tex2Dlod(_PositionBuffer, uv);
        float4 r = tex2Dlod(_RotationBuffer, uv);

        float l = p.w + 0.5;
        float s = calc_scale(uv, l);

        v.vertex.xyz = rotate_vector(v.vertex.xyz, r) * s + p.xyz;



        o.position = UnityObjectToClipPos(v.vertex);
        o.texcoord = TRANSFORM_TEX(v.texcoord0, _MainTex);
        o.color = calc_color(uv, l);

        UNITY_TRANSFER_FOG(o, o.position);

        return o;
    }

    half4 frag(v2f i) : SV_Target
    {

    	//UNITY_SETUP_INSTANCE_ID(i); // necessary only if any instanced properties are going to be accessed in the fragment Shader. 

        half4 c = i.color;
    	#if _MAINTEX
        	c *= tex2D(_MainTex, i.texcoord);
   		 #endif
        UNITY_APPLY_FOG_COLOR(i.fogCoord, c, (half4)0);
        //c *= float4(c.aaa, _BlendMode);
        return c*2;//NEW
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
           	// Blend One OneMinusSrcAlpha
           	Blend SrcAlpha OneMinusSrcAlpha //MINE
           	ColorMask RGB //MIINE
           	Cull Off Lighting Off ZWrite Off //MINE 
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    CustomEditor "Kvant.SprayUnlitMaterialEditor"
}
