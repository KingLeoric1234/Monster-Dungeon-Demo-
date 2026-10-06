Shader "Custom/RoughSurface"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 5.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _ColorTint ("Color Tint", Color) = (1,1,1,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _HighlightStrength ("Highlight Strength", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _NoiseScale;
            float _NoiseStrength;
            float4 _ColorTint;
            float4 _HighlightColor;
            float _HighlightStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 噪点纹理采样，制造粗糙表面的明暗变化
                float noise = tex2D(_NoiseTex, i.uv * _NoiseScale).r;
                float variation = (noise - 0.5) * _NoiseStrength;
                col.rgb += variation;

                // 叠加高光色，模拟材质对光的反射
                col.rgb = lerp(col.rgb, col.rgb * _HighlightColor.rgb, _HighlightStrength);

                col.rgb *= _ColorTint.rgb;

                return col;
            }
            ENDCG
        }
    }
}
