Shader "Custom/TreeSway"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SwaySpeed ("Sway Speed", Float) = 2.0
        _SwayAmount ("Sway Amount", Float) = 0.05
        _TreeHeight ("Tree Height", Float) = 1.0
        _LightColor ("Light Color", Color) = (1, 1, 0.9, 1)
        _LightStrength ("Light Strength", Range(0, 1)) = 0.3
        _TopGlow ("Top Glow", Range(0, 1)) = 0.5
        _GlowColor ("Glow Color", Color) = (1.0, 0.85, 0.4, 1.0)
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
                float swayOffset : TEXCOORD1;
                float heightFactor : TEXCOORD2;
            };

            sampler2D _MainTex;
            float _SwaySpeed;
            float _SwayAmount;
            float _TreeHeight;
            float4 _LightColor;
            float _LightStrength;
            float _TopGlow;
            float4 _GlowColor;

            v2f vert (appdata v)
            {
                v2f o;

                float heightFactor = saturate((v.vertex.y + _TreeHeight * 0.5) / _TreeHeight);
                heightFactor = heightFactor * heightFactor;
                float sway = sin(_Time.y * _SwaySpeed + v.vertex.x * 2.0) * _SwayAmount * heightFactor;
                v.vertex.x += sway;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.swayOffset = sway;
                o.heightFactor = heightFactor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 垂直渐变：底部保持当前亮度，顶部更亮
                float gradient = lerp(1.0 - _LightStrength, 1.0 + _TopGlow, i.heightFactor);
                col.rgb *= _LightColor.rgb * gradient;

                // 顶部柔和光晕：随sway略微移动
                float2 glowCenter = float2(0.5 + i.swayOffset * 2.0, 0.85);
                float dist = distance(i.uv, glowCenter);
                float glow = smoothstep(0.4, 0.0, dist) * _TopGlow;
                col.rgb += _GlowColor.rgb * glow;

                return col;
            }
            ENDCG
        }
    }
}
