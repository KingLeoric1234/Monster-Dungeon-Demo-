// 可见区：画到低分辨率RT上，可见区=白，背景=黑
Shader "FogOfWar/VisibleArea"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert(float4 v : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(v);
            }

            fixed4 frag() : SV_Target
            {
                return fixed4(1, 1, 1, 1);  // 白色=可见
            }
            ENDCG
        }
    }
}
