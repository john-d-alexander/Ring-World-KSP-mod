// All horizontal frequencies are integer multiples of the 512 km period.
// This makes wrapped origins continuous even at accelerated universal time.
sampler3D _Noise;
float3 _CloudOrigin;
float _CloudAmount,_Lightning;
float cloudCoverageLod(float2 p,float amount,float lod)
{
    float3 q=float3((p+_CloudOrigin.xy)/512000,0);
    float n=.55*tex3Dlod(_Noise,float4(q,max(0,lod))).r+.30*tex3Dlod(_Noise,float4(q*2,max(0,lod+1))).g+.15*tex3Dlod(_Noise,float4(q*8,max(0,lod+3))).b;
    return amount<=0?0:smoothstep(.70-.55*amount,.88-.55*amount,n);
}
float cloudCoverage(float2 p){return cloudCoverageLod(p,_CloudAmount,-10);}
