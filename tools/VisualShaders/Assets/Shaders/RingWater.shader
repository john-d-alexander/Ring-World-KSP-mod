Shader "NivenRingworld/Waves"
{
 Properties { _Color("Water tint",Color)=(.04,.25,.32,1) }
 SubShader
 {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  Pass
  {
   Cull Off
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   float3 _WaveCamera,_WaveAlong,_WaveAcross,_WaveUp;
   float4 _Wave; // wrapped along, wrapped across, frozen simulation time, amplitude
   float4 _WavePhase,_RipplePhase;
   float _WaterLight,_WaterQuality;
   struct data {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float2 p:TEXCOORD1;float depth:TEXCOORD2;};
   float wave(float2 p){return sin(dot(p,float2(.037,.012))+_WavePhase.x)*.55+sin(dot(p,float2(-.016,.029))+_WavePhase.y)*.3+sin(dot(p,float2(.063,-.054))+_WavePhase.z)*.15;}
   v2f vert(data v)
   {
    v2f o;float3 w=mul(unity_ObjectToWorld,v.vertex).xyz;float3 delta=w-_WaveCamera;
    o.p=float2(dot(delta,_WaveAlong),dot(delta,_WaveAcross))+_Wave.xy;
    float amplitude=_Wave.w*saturate(v.uv.x/5)*(1-smoothstep(2000,20000,length(delta)));
    w+=_WaveUp*wave(o.p)*amplitude;o.world=w;o.depth=v.uv.x;o.pos=mul(UNITY_MATRIX_VP,float4(w,1));return o;
   }
   float ripple(float theta){return sin(theta)*(1-smoothstep(.5,3.14159,fwidth(theta)));}
   float4 frag(v2f i):SV_Target
   {
    float2 p=i.p;float amplitude=_Wave.w*saturate(i.depth/5);
    float dx=(wave(p+float2(.25,0))-wave(p-float2(.25,0)))*2*amplitude;
    float dy=(wave(p+float2(0,.25))-wave(p-float2(0,.25)))*2*amplitude;
    dx+=ripple(p.x*1.7+p.y*.64+_RipplePhase.x)*.055;dy+=ripple(p.y*1.3-p.x*.92+_RipplePhase.y)*.045;
    if(_WaterQuality>1){dx+=ripple(p.x*5+p.y*3.1+_RipplePhase.z)*.022;dy+=ripple(p.y*4.2-p.x*3.7+_RipplePhase.w+1.5708)*.018;}
    float3 n=normalize(_WaveUp-_WaveAlong*dx-_WaveAcross*dy),v=normalize(_WorldSpaceCameraPos-i.world);
    if(dot(n,v)<0)n=-n;
    float fresnel=.025+.975*pow(1-saturate(dot(n,v)),5);
    float3 reflection=reflect(-v,n);float sky=saturate(dot(reflection,_WaveUp));
    float3 skyColor=lerp(float3(.32,.44,.51),float3(.12,.29,.52),sqrt(sky));
    float glint=pow(saturate(dot(reflect(-_WaveUp,n),v)),_WaterQuality>1?220:120)*3;
    float3 water=lerp(float3(.065,.31,.29),float3(.012,.095,.16),saturate(i.depth/25));
    float foam=(1-saturate(i.depth/2))*smoothstep(.25,.8,wave(p));
    float3 color=(lerp(water,skyColor,fresnel)+glint*float3(1,.95,.8)+foam*.3)*(.04+.96*_WaterLight);
    #ifdef UNITY_COLORSPACE_GAMMA
     color=LinearToGammaSpace(color);
    #endif
    return float4(color,1);
   }
   ENDCG
  }
  // Opaque water writes camera depth so atmosphere terminates at its surface.
  Pass
  {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"}
   ZWrite On ZTest LEqual Cull Off
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_shadowcaster
   #include "UnityCG.cginc"
   struct v2f { V2F_SHADOW_CASTER; };
   v2f vert(appdata_base v){v2f o;TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);return o;}
   float4 frag(v2f i):SV_Target{SHADOW_CASTER_FRAGMENT(i)}
   ENDCG
  }
 }
}
