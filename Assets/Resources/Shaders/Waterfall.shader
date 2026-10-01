Shader "MountainTea/Waterfall"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float streak=pow(saturate(sin(IN.worldPos.x*24+sin(IN.worldPos.y*2-_Time.y*5)*.25)),5);
            float falling=sin(IN.worldPos.y*8+_Time.y*9)*.05;
            o.Albedo=lerp(float3(.38,.69,.70),float3(.85,.95,.90),streak*.65+falling);
            o.Emission=float3(.055,.095,.09);o.Metallic=0;o.Smoothness=.15;o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
