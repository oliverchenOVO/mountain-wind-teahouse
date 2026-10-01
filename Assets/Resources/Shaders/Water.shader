Shader "MountainTea/Water"
{
    Properties { _Color ("Deep water", Color) = (.12,.44,.48,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 p=IN.worldPos.xz;
            float wave=sin(p.x*4+p.y*2+_Time.y*1.4)*sin(p.y*7-_Time.y*2.5);
            float foam=pow(saturate(sin(p.y*5+sin(p.x*6)-_Time.y*2.2)),20)*.19;
            o.Albedo=_Color.rgb+wave*.035+foam;
            o.Normal=normalize(float3(wave*.06,cos(p.y*4-_Time.y)*.05,1));
            o.Emission=float3(.19,.34,.29)*foam;
            o.Metallic=.05; o.Smoothness=.65; o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
