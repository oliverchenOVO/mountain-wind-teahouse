Shader "MountainTea/Water"
{
    Properties
    {
        _Color ("Deep jade water", Color) = (.13,.40,.43,1)
        _StreamTime ("Controlled stream time", Float) = 0
        _BankMin ("West bank", Float) = 6.05
        _BankMax ("East bank", Float) = 9.95
        _RainAmount ("Rain ripples", Range(0,1)) = 0
        _NightAmount ("Night highlights", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;float _StreamTime,_BankMin,_BankMax,_RainAmount,_NightAmount;
        struct Input { float3 worldPos; };
        float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
        float streamNoise(float2 p)
        {
            float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);
            return lerp(lerp(hash21(cell),hash21(cell+float2(1,0)),f.x),lerp(hash21(cell+float2(0,1)),hash21(cell+float2(1,1)),f.x),f.y);
        }
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 p=IN.worldPos.xz;float t=_StreamTime;
            float distanceToBank=max(0,min(p.x-_BankMin,_BankMax-p.x));
            float depth=smoothstep(.12,1.25,distanceToBank);
            float broad=streamNoise(float2(p.x*1.2,p.y*.55+t*.24));
            float fine=streamNoise(float2(p.x*3.8,p.y*1.7+t*.55));
            float streak=pow(saturate(sin(p.y*1.8+t*.85+broad*7+p.x*1.1)),15)*smoothstep(.40,.75,fine);
            float shore=(1-smoothstep(.05,.23,distanceToBank+sin(p.y*3+t)*.035))*smoothstep(.35,.65,fine);
            float2 cell=floor(p*1.35),local=frac(p*1.35);
            float seed=hash21(cell),phase=frac(t*.42+seed);
            float2 center=float2(.25+.5*seed,.25+.5*hash21(cell+9));
            float ripple=(1-smoothstep(.014,.045,abs(length(local-center)-phase*.48)))*(1-phase)*_RainAmount*.08;
            float light=streak*.12+shore*.09+ripple;
            o.Albedo=lerp(float3(.29,.53,.48),_Color.rgb,depth)+(broad-.5)*.025+light;
            o.Normal=normalize(float3(sin(p.x*3+broad*5+t*.7)*.065,cos(p.y*1.7+t*.9)*.045,1));
            // Stylized glints, not a screen-space or environment reflection.
            o.Emission=float3(.12,.20,.18)*light*(1-_NightAmount*.8);
            o.Metallic=.035;o.Smoothness=lerp(.58,.66,_RainAmount);o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
