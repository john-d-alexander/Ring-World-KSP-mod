Shader "NivenRingworld/VolumetricAtmosphere"
{
 Properties { _MainTex("Scene",2D)="white"{} }
 SubShader
 {
  Cull Off ZWrite Off ZTest Always
  CGINCLUDE
  #include "UnityCG.cginc"
  sampler2D _CylaBlack,_CylaWhite;float4 _CylaTexel;float _CylaUnitScale;
  sampler2D _MainTex,_Weather,_Volume,_Previous,_SavedDepth;
  UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
  #include "CloudField.cginc"
  float4 _MainTex_TexelSize,_Volume_TexelSize;
  float3 _RayRight,_RayUp,_RayForward,_Sun,_NoiseOrigin;
  // All distances in metres in a camera-centred tangent chart; z points inward.
  float4 _Habitat; // camera altitude, radius, across, half width
  float4 _WeatherMap; // camera offsets from weather centre, extent, cloud amount
  float4 _Quality; // cloud steps, atmosphere steps, light steps, cloud range
  float4 _Look; // haze, sunlight, cloud shadow strength, exposure
  float4 _Photo; // active, tile index (-1 for full screen), grid side, sample index
  float2 _FrameSize;
  float _Jitter;
  float4 _CloudHandoff;
  struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
  v2f vert(appdata_img v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
  float3 ray(float2 uv){return normalize(_RayForward+(uv.x*2-1)*_RayRight+(uv.y*2-1)*_RayUp);}
  float height(float3 p){return _Habitat.x+p.z-p.x*p.x/(2*max(1,_Habitat.y-_Habitat.x-p.z));}
  float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7))+_Jitter*37.91)*43758.5453);}
  float depthAt(float2 uv)
  {
   if(_Photo.x>.5)return tex2D(_SavedDepth,uv).r;
   return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));
  }
  float density(float3 p)
  {
   float h=height(p);if(_WeatherMap.w<=0||h<2200||h>10000||abs(_Habitat.z+p.y)>_Habitat.w)return 0;
   float2 uv=(p.xy+_WeatherMap.xy)/(_WeatherMap.z*2)+.5;
   float fade=saturate((.5-max(abs(uv.x-.5),abs(uv.y-.5)))*12);
   float2 weather=float2(cloudCoverage(p.xy),tex3Dlod(_Noise,float4((p+_CloudOrigin)/512000,0)).g);
   if(weather.r<.001)return 0;
   float top=5200+weather.g*4300;
   float vertical=saturate((h-2400)/650)*(1-smoothstep(top-1800,top,h));
   float3 q=(p+_CloudOrigin)/64000;
   float4 n=tex3Dlod(_Noise,float4(q,0));
   float shape=n.r*.55+n.g*.45;
   float detail=.65*tex3Dlod(_Noise,float4(q*5,0)).g+.35*tex3Dlod(_Noise,float4(q*17,0)).b;
   float body=saturate((shape-(.72-weather.r*.62))*3.4);
   float local=1-smoothstep(_CloudHandoff.x,max(_CloudHandoff.x+1,_CloudHandoff.y),length(p));
   return saturate(body-(1-detail)*.32)*vertical*fade*local;
  }
  float shadow(float3 p)
  {
   float tau=0,ds=4000/max(1,_Quality.z);
   [loop]for(int j=0;j<8;j++){if(j>=_Quality.z)break;tau+=density(p+_Sun*((j+.5)*ds))*ds*.0011;}
   return lerp(1,exp(-tau),_Look.z);
  }
  float4 volume(v2f i):SV_Target
  {
   if(_Photo.x>.5&&_Photo.y>=0)
   {
    float2 tile=min(_Photo.z-1,floor(i.uv*_Photo.z));
    if(tile.y*_Photo.z+tile.x!=_Photo.y)return tex2D(_Previous,i.uv);
   }
   float3 d=ray(i.uv);float scene=depthAt(i.uv)/max(.001,dot(d,normalize(_RayForward)));
   float limit=min(scene,_Quality.w);float jitter=hash(floor(i.uv*_FrameSize));
   float3 light=0;float transmission=1;
   // Atmosphere interval: tangent approximation corrected for ring curvature per sample.
   float atmoEnd=min(scene,400000);
   if(d.z<-.0001)atmoEnd=min(atmoEnd,max(0,_Habitat.x/-d.z));
   if(d.z>.0001)atmoEnd=min(atmoEnd,max(0,(60000-_Habitat.x)/d.z));
   float atmoStart=0;if(_Habitat.x>60000&&d.z<-.0001)atmoStart=(_Habitat.x-60000)/-d.z;
   float ads=max(0,atmoEnd-atmoStart)/max(1,_Quality.y);float3 optical=0,airLight=0;
   float mu=dot(d,_Sun),phaseR=.0596831*(1+mu*mu);
   float phaseM=.0336/pow(max(.05,1.5776-1.52*mu),1.5);
   [loop]for(int a=0;a<96;a++)
   {
    if(a>=_Quality.y||ads<=0)break;
    float3 p=d*(atmoStart+(a+jitter)*ads);float h=height(p);
    if(h<0||h>60000||abs(_Habitat.z+p.y)>_Habitat.w)continue;
    float rho=exp(-h/8500)*pow(saturate((60000-h)/5000),2),mie=exp(-h/1200)*.000012;
    float3 beta=float3(.0000058,.0000135,.0000331)*rho;
    float3 ext=(beta+mie)*_Look.x;
    float3 sunDepth=beta*8500+mie*1200;
    airLight+=(beta*phaseR+mie*phaseM)*exp(-optical-ext*ads*.5-sunDepth)*ads*_Look.y*8*_Look.x;
    optical+=ext*ads;
   }
   float airT=dot(exp(-optical),float3(.333333,.333333,.333333));
   // Restrict cloud integration to the cloud band; depth clips foreground craft/terrain.
   float start=0,end=limit;
   if(_Habitat.x<2200){if(d.z<=0)end=0;else {start=(2200-_Habitat.x)/d.z;end=min(end,(10200-_Habitat.x)/d.z);}}
   else if(_Habitat.x>10200){if(d.z>=0)end=0;else {start=(_Habitat.x-10200)/-d.z;end=min(end,(_Habitat.x-2000)/-d.z);}}
   else if(d.z>.00001)end=min(end,(10200-_Habitat.x)/d.z);
   else if(d.z<-.00001)end=min(end,(_Habitat.x-2000)/-d.z);
   float ds=max(0,end-start)/max(1,_Quality.x);
   float phase=.35+.65*pow(saturate(mu),8);
   [loop]for(int c=0;c<256;c++)
   {
    if(c>=_Quality.x||ds<=0||transmission<.008)break;
    float3 p=d*(start+(c+jitter)*ds);float den=density(p);if(den<.001)continue;
    float t=exp(-den*ds*.0011);
    float direct=shadow(p);float ambient=.13+.23*saturate((height(p)-2400)/6000);
    // Ambient fill approximates multiple scattering; direct sunlight is self-shadowed.
    float3 illumination=float3(.72,.82,1)*ambient+float3(1,.95,.86)*direct*(.75+phase);
    illumination=illumination*(.035+_Look.y*.965)+_Lightning*1.7;
    light+=transmission*(1-t)*illumination;transmission*=t;
   }
   float4 result=float4((airLight*transmission+light*sqrt(airT))*_Look.w,1-airT*transmission);
   if(_Photo.x>.5&&_Photo.w>0)result=lerp(tex2D(_Previous,i.uv),result,1/(_Photo.w+1));
   return result;
  }
  float4 composite(v2f i):SV_Target
  {
   float4 fog=tex2D(_Volume,i.uv);
   // Depth-aware 4-tap upsampling keeps low-resolution clouds off foreground silhouettes.
   if(_Photo.x<.5&&_Volume_TexelSize.z<_FrameSize.x-.5)
   {
    float z=depthAt(i.uv),sum=0;fog=0;
    [unroll]for(int k=0;k<4;k++)
    {
     float2 uv=i.uv+float2((k&1)? .5:-.5,(k&2)? .5:-.5)*_Volume_TexelSize.xy;
     float weight=exp(-abs(depthAt(uv)-z)/max(2,z*.015));fog+=tex2D(_Volume,uv)*weight;sum+=weight;
    }
    fog=sum>.0001?fog/sum:tex2D(_Volume,i.uv);
   }
   float3 scene=tex2D(_MainTex,i.uv).rgb;
   #ifdef UNITY_COLORSPACE_GAMMA
    scene=GammaToLinearSpace(scene);
   #endif
   float3 color=scene*(1-saturate(fog.a))+fog.rgb;
   #ifdef UNITY_COLORSPACE_GAMMA
    color=LinearToGammaSpace(max(0,color));
   #endif
   return float4(color,1);
  }
  // The opaque Cyla shader is evaluated against black and white inputs.
  // Recover scattering and transmission without downsampling the spacecraft.
  float4 cylaComposite(v2f i):SV_Target
  {
   float z=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv));
   float3 scatter=0,transmission=0;float total=0;
   float2 base=(floor(i.uv*_CylaTexel.zw-.5)+.5)*_CylaTexel.xy;
   float2 f=frac(i.uv*_CylaTexel.zw-.5);
   [unroll]for(int y=0;y<2;y++)[unroll]for(int x=0;x<2;x++)
   {
    float2 uv=base+float2(x,y)*_CylaTexel.xy;
    float dz=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));
    float w=(x?f.x:1-f.x)*(y?f.y:1-f.y)*exp(-abs(dz-z)/max(1,z*.01));
    float3 b=tex2D(_CylaBlack,uv).rgb,white=tex2D(_CylaWhite,uv).rgb;
    scatter+=b*w;transmission+=saturate(white-b)*w;total+=w;
   }
   // No matching low-resolution depth: preserve the original foreground pixel.
   if(total<.0001)return tex2D(_MainTex,i.uv);
   return float4(tex2D(_MainTex,i.uv).rgb*(transmission/total)+scatter/total,1);
  }
  float4 copyDepth(v2f i):SV_Target{return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv));}
  float4 cylaDepth(v2f i):SV_Target
  {
   float d=max(1e-8,LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv))*_CylaUnitScale);
   return (rcp(d)-_ZBufferParams.w)/_ZBufferParams.z;
  }
  ENDCG
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment volume
   #pragma target 3.0
  ENDCG }
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment composite
   #pragma target 3.0
  ENDCG }
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment copyDepth
   #pragma target 3.0
  ENDCG }
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment cylaComposite
   #pragma target 3.0
  ENDCG }
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment cylaDepth
   #pragma target 3.0
  ENDCG }
 }
 Fallback Off
}
