// 过渡带：写Stencil=2，不画颜色
Shader "FogOfWar/OuterArea"
{
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
        Cull Off
        ZWrite Off
        ZTest Always
        Pass
        {
            Stencil { Ref 2 Comp Always Pass Replace }
            Blend Zero One
            ColorMask 0

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
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
