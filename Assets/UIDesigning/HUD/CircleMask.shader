Shader "UI/CircleMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // UI Image需要这个属性（不用，仅占位）
        _Color ("Mask Color", Color) = (0,0,0,1)
        _Center ("Center (UV Space)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius (UV Space)", Float) = 0.2
        _EdgeSoftness ("Edge Softness", Range(0, 0.05)) = 0.005
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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

            float4 _Color;
            float4 _Center;
            float _Radius;
            float _EdgeSoftness;
            sampler2D _MainTex;  // UI Image需要（不用，仅占位）

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 考虑屏幕宽高比，保持圆形（否则会变成椭圆形）
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 uv = float2(i.uv.x * aspect, i.uv.y);
                float2 center = float2(_Center.x * aspect, _Center.y);

                float dist = distance(uv, center);
                // 圆形区域内alpha=0（透明可视），圆形区域外alpha=1（黑色遮罩）
                float alpha = smoothstep(_Radius - _EdgeSoftness, _Radius + _EdgeSoftness, dist);
                return float4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}
