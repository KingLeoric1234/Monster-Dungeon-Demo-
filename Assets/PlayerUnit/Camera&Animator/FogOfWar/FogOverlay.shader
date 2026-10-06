// 战争迷雾-覆盖层：采样模糊后的可见区贴图
// 贴图白=可见 → alpha=0（清晰）；黑=不可见 → alpha=FogAlpha（浓）
// 模糊后边缘柔和
Shader "FogOfWar/FogOverlay"
{
    Properties
    {
        _Color ("Fog Color", Color) = (0,0,0,0.85)
        _FogTex ("Fog Tex", 2D) = "black" {}
        _PlayerPos ("Player Pos", Vector) = (0,0,0,0)
        _RTSize ("RT World Size", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            sampler2D _FogTex;
            float4 _PlayerPos;
            float _RTSize;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(float4 v : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v);
                o.worldPos = mul(unity_ObjectToWorld, v).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = (i.worldPos.xy - _PlayerPos.xy) / _RTSize + 0.5;
                float vis = tex2D(_FogTex, uv).r;  // 1=可见, 0=不可见
                return fixed4(_Color.rgb, _Color.a * (1.0 - vis));
            }
            ENDCG
        }
    }
}
