Shader "ORION/NebulaTut"
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
			brightnessContrast("Brightness-Contrast ", Vector) = (1, 1, 1, 1)

			//v0.1
			cloudSpeed("Increase cloud Speed ", Vector) = (0,0,0,0)
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

			//v0.1
			float4 cloudSpeed;

			float _Distort;
		float4 _HighLight;
		float4 _Color;
		float _Pow;
		float4 brightnessContrast;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _Tex2;
			float4 _Tex2_ST;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			//https://www.codementor.io/@tlhm/procedural-generation-visual-rendering-unity3d-du107jjmr
			//fragment shader
			float4 frag(v2f i) : COLOR{

				//v0.1
				//Use that to create an offset for the second texture
				float2 offsetA = float2(_Time.y*0.001*cloudSpeed.x, _Time.y*0.001*cloudSpeed.y);

				//Get the colors at the right point on the first texture
				half4 col = tex2D(_MainTex,i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw + offsetA);

				//Use that to create an offset for the second texture
				//float2 offset = float2(_Distort*(col.x - .5),_Distort*(col.y - .5));
				//v0.1
				//Use that to create an offset for the second texture
				float2 offset = float2(_Distort*(col.x - .5), _Distort*(col.y - .5)) + float2(_Time.y*0.001*cloudSpeed.x, _Time.y*0.001*cloudSpeed.y);

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
				float3 final_color = lerp(_HighLight, _Color, col2.b*.5).rgb;

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
				final_color = pow(final_color, 5);

				//We're done! Return the final pixel color!
				float4 finalOUT = float4(final_color, final_alpha);

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
