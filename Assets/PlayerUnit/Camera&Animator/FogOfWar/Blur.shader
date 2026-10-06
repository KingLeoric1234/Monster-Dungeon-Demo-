// 战争迷雾-模糊：对小贴图做高斯模糊（横向+纵向分离）
// Pass 0: 横向模糊；Pass 1: 纵向模糊
Shader "FogOfWar/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Float) = 3
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // Pass 0: 横向模糊
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragH
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurRadius;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 fragH(v2f i) : SV_Target
            {
                float2 offset = float2(_MainTex_TexelSize.x * _BlurRadius, 0);
                fixed4 c = tex2D(_MainTex, i.uv) * 0.227027;
                c += tex2D(_MainTex, i.uv + offset * 1.3846) * 0.3162;
                c += tex2D(_MainTex, i.uv - offset * 1.3846) * 0.3162;
                c += tex2D(_MainTex, i.uv + offset * 3.2308) * 0.07027;
                c += tex2D(_MainTex, i.uv - offset * 3.2308) * 0.07027;
                return c;
            }
            ENDCG
        }

        // Pass 1: 纵向模糊
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragV
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurRadius;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 fragV(v2f i) : SV_Target
            {
                float2 offset = float2(0, _MainTex_TexelSize.y * _BlurRadius);
                fixed4 c = tex2D(_MainTex, i.uv) * 0.227027;
                c += tex2D(_MainTex, i.uv + offset * 1.3846) * 0.3162;
                c += tex2D(_MainTex, i.uv - offset * 1.3846) * 0.3162;
                c += tex2D(_MainTex, i.uv + offset * 3.2308) * 0.07027;
                c += tex2D(_MainTex, i.uv - offset * 3.2308) * 0.07027;
                return c;
            }
            ENDCG
        }
    }
}
