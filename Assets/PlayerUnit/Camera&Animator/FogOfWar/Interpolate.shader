// 两张RT之间插值：旧形状→新形状，指数缓变
Shader "FogOfWar/Interpolate"
{
    Properties
    {
        _MainTex ("Old", 2D) = "black" {}
        _NewTex ("New", 2D) = "black" {}
        _T ("T", Float) = 0
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;   // oldRT
            sampler2D _NewTex;    // newRT
            float _T;

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

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 old = tex2D(_MainTex, i.uv);
                fixed4 neww = tex2D(_NewTex, i.uv);
                return lerp(old, neww, _T);
            }
            ENDCG
        }
    }
}
