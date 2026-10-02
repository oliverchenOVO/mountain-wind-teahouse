Shader "MountainTea/LanternPaper"
{
    Properties
    {
        _Color ("Paper tint", Color) = (.92,.82,.61,1)
        _EmissionColor ("Warm paper glow", Color) = (0,0,0,1)
        _Glossiness ("Paper sheen", Range(0,1)) = .06
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color, _EmissionColor;half _Glossiness;
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo=_Color.rgb;o.Emission=_EmissionColor.rgb;
            o.Metallic=0;o.Smoothness=_Glossiness;o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
