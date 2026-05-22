Shader "PaintEditorExtension/BlendTexture"
{
    Properties
    {
        _MainTexture ("Texture", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactor("Src Factor", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactor("Dst Factor", Float) = 10
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcFactorA("Src Factor A", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstFactorA("Dst Factor A", Float) = 10
        [Enum(UnityEngine.Rendering.BlendOp)]
        _OpColor("Operation Color", Float) = 0
        [Enum(UnityEngine.Rendering.BlendOp)]
        _OpAlpha("Operation Alpha", Float) = 0

    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        Blend [_SrcFactor] [_DstFactor], [_SrcFactorA] [_DstFactorA]
        BlendOp [_OpColor], [_OpAlpha]

        Pass
        {
            AlphaToMask On

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTexture);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvs = i.uv;
                fixed4 col = tex2D(_MainTexture, uvs);
                return col;
            }

            ENDCG
        }
    }
}
