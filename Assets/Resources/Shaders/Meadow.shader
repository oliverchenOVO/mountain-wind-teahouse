Shader "MountainTea/Meadow"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;
        struct Input { float4 color : COLOR; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo=IN.color.rgb*_Color.rgb; o.Metallic=0; o.Smoothness=.04; o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
