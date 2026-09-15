Shader "NivenRingworld/DistantSurface"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Back ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #include "UnityCG.cginc"
            float _CircumferenceKm, _WidthKm, _DayPhase, _CloudAmount, _CloudDrift;
            float _SeedLow, _SeedHigh, _Generation, _Detail;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            #include "RingTerrainNoise.cginc"
            float weight(float3 climate,float3 centre)
            { float3 d=climate-centre;return exp(-dot(d*d,float3(1,1,.4))/.045); }
            float4 frag(v2f i):SV_Target
            {
                float2 uv=i.uv;
                float phase=frac(20*uv.x-_DayPhase),edge=min(phase,1-phase);
                float light=saturate((edge-.138307)/.02);
                if(_Detail<.5)return float4(float3(.34,.47,.32)*lerp(.07,1,light),1);
                float3 climate=saturate((float3(noise(uv,210,701),noise(uv,160,709),noise(uv,310,719))-.25)*2);
                climate.x=saturate(climate.x-saturate((abs(uv.y-.5)*2-.94)/.06)*.4);
                float d=weight(climate,float3(.8,.18,.4)),g=weight(climate,float3(.6,.43,.35)),f=weight(climate,float3(.57,.78,.4));
                float c=weight(climate,float3(.18,.45,.4)),h=weight(climate,float3(.42,.5,.9));
                float3 land=(d*float3(.66,.53,.32)+g*float3(.36,.46,.20)+f*float3(.19,.34,.15)+c*float3(.52,.56,.44)+h*float3(.43,.43,.36))/(d+g+f+c+h);
                // Macro basins match the two landmark centres. Tiny channels remain the job of terrain LOD.
                float dx=min(abs(frac(uv.x-.30+.5)-.5),abs(frac(uv.x-.80+.5)-.5))*_CircumferenceKm;
                float basin=length(float2(dx,(uv.y-.5)*_WidthKm))/(_WidthKm*.32);
                float broad=noise(uv,1100,11);
                float ocean=max(1-smoothstep(.98,1.08,basin),1-smoothstep(.15,.19,broad));
                float3 surface=lerp(land,float3(.035,.16,.24),ocean);
                surface*=lerp(.07,1,light);
                #ifndef UNITY_COLORSPACE_GAMMA
                    surface=GammaToLinearSpace(surface);
                #endif
                return float4(surface,1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Color"
}
