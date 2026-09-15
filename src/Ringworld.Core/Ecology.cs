using System;
namespace Ringworld.Core
{
    public struct ClimateBlend
    {
        public double Desert,Meadow,Forest,Cold,Highland;
        public double TreeCover {get{return .03*Desert+.18*Meadow+.92*Forest+.07*Cold+.12*Highland;}}
        public double Relief {get{return .55*Desert+.7*Meadow+.85*Forest+.6*Cold+1.8*Highland;}}
        public DVec Ground {get{return new DVec(.66,.53,.32)*Desert+new DVec(.36,.46,.20)*Meadow+new DVec(.19,.34,.15)*Forest+new DVec(.52,.56,.44)*Cold+new DVec(.43,.43,.36)*Highland;}}
        public DVec SkyTint {get{return new DVec(1.07,.99,.91)*Desert+new DVec(.98,1,1.02)*Meadow+new DVec(.94,1.03,1.01)*Forest+new DVec(.97,1.01,1.06)*Cold+new DVec(.99,1,1.04)*Highland;}}
        public Biome Dominant {get{double max=Meadow;Biome b=Biome.Grassland;if(Desert>max){max=Desert;b=Biome.Desert;}if(Forest>max){max=Forest;b=Biome.Forest;}if(Cold>max){max=Cold;b=Biome.Snow;}if(Highland>max)b=Biome.Mountain;return b;}}
    }
    public static class Ecology
    {
        private static double Clamp(double x){return Math.Max(0,Math.Min(1,x));}
        private static double Weight(double t,double m,double r,double tc,double mc,double rc)
        {return Math.Exp(-((t-tc)*(t-tc)+(m-mc)*(m-mc)+(r-rc)*(r-rc)*.4)/.045);}
        public static ClimateBlend Sample(TerrainGenerator terrain,double along,double across)
        {
            double t=Clamp((terrain.Noise(along,across,210000,701)-.25)*2);
            double rim=Clamp((Math.Abs(across)/(terrain.Geometry.P.Width*.5)-.94)/.06);t=Clamp(t-rim*.4);
            double m=Clamp((terrain.Noise(along,across,160000,709)-.25)*2);
            double r=Clamp((terrain.Noise(along,across,310000,719)-.25)*2);
            double d=Weight(t,m,r,.8,.18,.4),g=Weight(t,m,r,.6,.43,.35),f=Weight(t,m,r,.57,.78,.4),c=Weight(t,m,r,.18,.45,.4),h=Weight(t,m,r,.42,.5,.9);
            double total=d+g+f+c+h;
            return new ClimateBlend{Desert=d/total,Meadow=g/total,Forest=f/total,Cold=c/total,Highland=h/total};
        }
    }
}
