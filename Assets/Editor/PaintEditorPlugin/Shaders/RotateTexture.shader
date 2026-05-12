Shader "Basics/RotateTexture"
{
	Properties
	{
		_MainTexture ("Main Texture", 2D) = "white" {}
		_Rotation ("Rotation", float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTexture;
			float4 _MainTexture_ST;
			float _Rotation;

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTexture);
				o.uv.xy = o.uv * 2 - 1;

				float c = cos(_Rotation);
				float s = sin(_Rotation);
				float2x2 mat = float2x2(c,-s,
										s,c);
				o.uv.xy = mul(mat, o.uv.xy);

				o.uv.xy = o.uv * 0.5 + 0.5;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTexture, i.uv);
				//return fixed4(i.uv,0,1);
				return col;
			}

			ENDCG
		}
	}
}