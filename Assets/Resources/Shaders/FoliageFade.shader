Shader "MountainTea/FoliageFade"
{
    Properties
    {
        _Color ("Leaf color", Color) = (1,1,1,1)
        _Opacity ("Visibility", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0
        fixed4 _Color;
        half _Opacity;
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo=_Color.rgb; o.Metallic=0; o.Smoothness=.05;
            o.Alpha=_Opacity;
        }
        ENDCG
    }
    Fallback "Transparent/Diffuse"
}
