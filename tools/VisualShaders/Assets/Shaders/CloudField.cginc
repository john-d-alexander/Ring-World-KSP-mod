// Periodic double-precision origins are supplied by RingCloudField.
sampler3D _Noise;
float3 _CloudOrigin,_CloudMacroOrigin;
float _CloudAmount,_Lightning;
float cloudCoverageAt(float2 micro,float2 macro,float amount,float lod)
{
    float3 q=float3(micro,0),m=float3(macro,0);
    float n=.55*tex3Dlod(_Noise,float4(q,max(0,lod))).r+.30*tex3Dlod(_Noise,float4(q*2,max(0,lod+1))).g+.15*tex3Dlod(_Noise,float4(q*8,max(0,lod+3))).b;
    // Large cloud banks and clear regions survive the averaging of fine cloud detail.
    float bank=.7*tex3Dlod(_Noise,float4(m,max(0,lod-6))).r+.3*tex3Dlod(_Noise,float4(m*2,max(0,lod-5))).b;
    float region=smoothstep(.65-.30*amount,.80-.30*amount,bank);
    float detail=smoothstep(.48-.18*amount,.64-.18*amount,n);
    return amount<=0?0:region*detail;
}
float cloudCoverageLod(float2 p,float amount,float lod)
{
    return cloudCoverageAt((p+_CloudOrigin.xy)/512000,p/32768000+_CloudMacroOrigin.xy,amount,lod);
}
float cloudCoverage(float2 p){return cloudCoverageLod(p,_CloudAmount,-10);}
