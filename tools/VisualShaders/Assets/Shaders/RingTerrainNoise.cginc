            // The CPU terrain's 64-bit hash, represented as low/high uint halves.
            uint2 mul64(uint2 a,uint2 b)
            {
                uint a0=a.x&65535u,a1=a.x>>16,b0=b.x&65535u,b1=b.x>>16;
                uint w0=a0*b0,t=a1*b0+(w0>>16),w1=(t&65535u)+a0*b1;
                uint lo=(w1<<16)|(w0&65535u),hi=a1*b1+(t>>16)+(w1>>16);
                return uint2(lo,hi+a.x*b.y+a.y*b.x);
            }
            uint2 shr64(uint2 a,uint n) { return uint2((a.x>>n)|(a.y<<(32-n)),a.y>>n); }
            uint hash(int x,int y,int salt)
            {
                uint2 a=mul64(uint2(x,0),uint2(0x85EBCA87u,0x9E3779B1u));
                uint2 b=mul64(uint2((uint)y,y<0?0xffffffffu:0u),uint2(0x27D4EB4Fu,0xC2B2AE3Du));
                uint seed=(uint)_SeedLow|((uint)_SeedHigh<<16);
                uint2 h=a^b^uint2(seed+(uint)salt,0);
                h=mul64(h^shr64(h,30),uint2(0x1CE4E5B9u,0xBF58476Du));
                h=mul64(h^shr64(h,27),uint2(0x133111EBu,0x94D049BBu));
                return (h^shr64(h,31)).y;
            }
            float gradient(uint h,float2 p)
            {
                uint k=h>>29;
                if(k==0)return p.x; if(k==1)return -p.x; if(k==2)return p.y; if(k==3)return -p.y;
                return (k==4?p.x+p.y:k==5?p.x-p.y:k==6?-p.x+p.y:-p.x-p.y)*.70710678;
            }
            float noise(float2 uv,float wavelength,int salt)
            {
                int period=max(2,(int)round(_CircumferenceKm/wavelength));
                float2 p=float2(frac(uv.x)*period,(uv.y-.5)*_WidthKm/wavelength);
                int2 cell=(int2)floor(p);float2 f=frac(p),q=f*f*f*(f*(f*6-15)+10);
                uint a=hash(cell.x,cell.y,salt),b=hash((cell.x+1)%period,cell.y,salt);
                uint c=hash(cell.x,cell.y+1,salt),d=hash((cell.x+1)%period,cell.y+1,salt);
                float n=_Generation<2?lerp(lerp(a/4294967296.0,b/4294967296.0,q.x),lerp(c/4294967296.0,d/4294967296.0,q.x),q.y):
                    .5+.5*lerp(lerp(gradient(a,f),gradient(b,f-float2(1,0)),q.x),lerp(gradient(c,f-float2(0,1)),gradient(d,f-1),q.x),q.y);
                // Subpixel climate cells average out instead of shimmering on the far arc.
                float footprint=max(abs(ddx(uv.x))+abs(ddy(uv.x)),0)*period;
                footprint=max(footprint,(abs(ddx(uv.y))+abs(ddy(uv.y)))*_WidthKm/wavelength);
                return lerp(n,.5,smoothstep(.35,1.5,footprint));
            }
