using System;
using System.Collections.Generic;

namespace Ringworld.Core
{
    public enum Biome { Ocean, Lake, River, Wetland, Grassland, Forest, Desert, Mountain, Snow, Scrith, Ruins, Rimwall, Road }
    public struct TerrainSample
    {
        public double Height, WaterHeight, Temperature, Shore;
        public DVec GroundColor;public bool BlendedColor;
        public Biome Biome;
        public bool Wet { get { return WaterHeight > Height; } }
    }
    public sealed class Landmark
    {
        public string Id, Name, Description;
        public double Along, Across, Radius, Height;
        public string Kind;
        public Landmark(string id,string name,string description,double along,double across,double radius,double height,string kind)
        { Id=id; Name=name; Description=description; Along=along; Across=across; Radius=radius; Height=height; Kind=kind; }
    }
    public sealed class TerrainGenerator
    {
        public const double MinimumHeight=-1200;
        public double HeightMultiplier=1;
        public int GenerationVersion=2;
        public double PondAmount=1;
        public readonly RingGeometry Geometry;
        public readonly List<Landmark> Landmarks = new List<Landmark>();
        public TerrainGenerator(RingGeometry geometry)
        {
            Geometry=geometry;
            double c=geometry.P.Circumference,w=geometry.P.Width;
            Landmarks.Add(new Landmark("arrival","Explorer's landing field","A new expedition outpost beside an ancient transport route. Original gameplay site.", c*.03125,w*.14,650,160,"outpost"));
            Landmarks.Add(new Landmark("city","Abandoned city","A procedural interpretation of the Ringworld's decaying technological cities.",c*.03125+6500,w*.14+2000,900,190,"city"));
            Landmarks.Add(new Landmark("scrith","Exposed scrith","An excavated area reveals the material supporting the habitat.",c*.03125+14500,w*.14-1800,900,100,"scrith"));
            long catchments=Math.Max(2,(long)Math.Round(c/131072));
            double catchmentAlong=(Math.Floor(.03125*catchments)+.5)*c/catchments;
            double catchmentAcross=Math.Round(w*.14/131072)*131072;
            Landmarks.Add(new Landmark("waterway","Catchment lake and river","An original river catchment with a basin lake and descending tributary channels. Explore the banks to sample its wetland habitat.",catchmentAlong,catchmentAcross,2500,0,"waterway"));
            Landmarks.Add(new Landmark("eye","Fist-of-God analogue","A meteor-puncture mountain inspired by the novel. Shape and location are adaptations.",c*.18,w*.08,45000,45000,"puncture"));
            Landmarks.Add(new Landmark("ocean1","Great Ocean I","One of two immense ocean basins; coastlines and islands are procedurally interpreted.",c*.30,0,w*.32,0,"ocean"));
            Landmarks.Add(new Landmark("ocean2","Great Ocean II","The second immense ocean basin, opposite the first.",c*.80,0,w*.32,0,"ocean"));
            Landmarks.Add(new Landmark("map","Map-island research station","A cartographic island inspired by the ocean maps. This is an original island, not a traced Earth map.",c*.30,w*.025,16000,100,"island"));
            Landmarks.Add(new Landmark("rim","Rim service terminal","An original access station at the foot of the atmosphere-retaining wall.",c*.03125,w/2-1800,700,150,"terminal"));
            Landmarks.Add(new Landmark("spill","Spill-mountain analogue","A rim-adjacent mountain suggesting the habitat's large-scale water circulation machinery.",c*.62,w/2-80000,70000,40000,"mountain"));
        }
        private static long Mod(long a,long b) { long r=a%b;return r<0?r+b:r; }
        private double Hash(long x,long y,int salt)
        {
            unchecked {
                ulong h=(ulong)x*0x9E3779B185EBCA87UL ^ (ulong)y*0xC2B2AE3D27D4EB4FUL ^ (uint)(Geometry.P.Seed+salt);
                h^=h>>30; h*=0xBF58476D1CE4E5B9UL;h^=h>>27;h*=0x94D049BB133111EBUL;h^=h>>31;
                return (h>>11)*(1.0/9007199254740992.0);
            }
        }
        private static double Smooth(double t) { return t*t*t*(t*(t*6-15)+10); }
        private static double Mix(double a,double b,double t) { return a+(b-a)*t; }
        public double Noise(double along,double across,double wavelength,int salt)
        {
            long period=Math.Max(2,(long)Math.Round(Geometry.P.Circumference/wavelength));
            double x=RingGeometry.Wrap(along,Geometry.P.Circumference)/Geometry.P.Circumference*period;
            double y=across/wavelength;
            long ix=(long)Math.Floor(x),iy=(long)Math.Floor(y);
            double tx=Smooth(x-ix),ty=Smooth(y-iy);
            if(GenerationVersion>=2)
            {
                double u=x-ix,v=y-iy;
                return .5+.5*Mix(Mix(Gradient(Hash(Mod(ix,period),iy,salt),u,v),Gradient(Hash(Mod(ix+1,period),iy,salt),u-1,v),tx),
                    Mix(Gradient(Hash(Mod(ix,period),iy+1,salt),u,v-1),Gradient(Hash(Mod(ix+1,period),iy+1,salt),u-1,v-1),tx),ty);
            }
            return Mix(Mix(Hash(Mod(ix,period),iy,salt),Hash(Mod(ix+1,period),iy,salt),tx),
                Mix(Hash(Mod(ix,period),iy+1,salt),Hash(Mod(ix+1,period),iy+1,salt),tx),ty);
        }
        private static double Gradient(double hash,double x,double y)
        {
            switch((int)(hash*8)){case 0:return x;case 1:return -x;case 2:return y;case 3:return -y;
                case 4:return (x+y)*.70710678;case 5:return (x-y)*.70710678;case 6:return (-x+y)*.70710678;default:return (-x-y)*.70710678;}
        }
        public double Scatter(long x,long y,int salt){return Hash(x,y,salt);}
        public TerrainSample Sample(double along,double across)
        {
            var p=Geometry.P;
            if(GenerationVersion>=3)along=RingGeometry.Wrap(along,p.Circumference);
            double broad=Noise(along,across,1100000,11), moisture=Noise(along,across,270000,19);
            double hills=Noise(along+(Noise(along,across,17000,401)-.5)*11000,across+(Noise(along,across,23000,409)-.5)*11000,6000,23), detail=Noise(along,across,350,29);
            // Domain warping breaks up aligned noise features; multiple ridge scales form
            // ranges with foothills and smaller gullies instead of isolated smooth bumps.
            if(GenerationVersion<2)hills=Noise(along,across,6000,23);
            double warpA=along+(Noise(along,across,95000,37)-.5)*52000;
            double warpB=across+(Noise(along,across,77000,41)-.5)*44000;
            double ridge=1-Math.Abs(Noise(warpA,warpB,28000,31)*2-1);
            double ridge2=1-Math.Abs(Noise(warpA,warpB,7200,43)*2-1);
            double ranges=Smooth(Math.Max(0,Math.Min(1,(Noise(along,across,240000,47)-.35)/.35)));
            var climate=GenerationVersion>=4?Ecology.Sample(this,along,across):new ClimateBlend();
            double mountains=(GenerationVersion>=4?climate.Relief:1)*ranges*(Math.Pow(ridge,5)*5200+Math.Pow(ridge2,6)*850);
            // The expedition pad belongs in a broad foothill valley, not a 650 m
            // wide pit carved several kilometres down into a generated mountain.
            double startX=Geometry.AlongDistance(along,Landmarks[0].Along),startY=across-Landmarks[0].Across;
            mountains*=Smooth(Math.Min(1,Math.Sqrt(startX*startX+startY*startY)/24000));
            double dunes=(GenerationVersion>=4?climate.Desert>.3:moisture<.27)?24*Math.Pow(.5+.5*Math.Sin(RingGeometry.Wrap(warpA,p.Circumference)/p.Circumference*Math.Round(p.Circumference/(110*2*Math.PI))*2*Math.PI+warpB/500),3):0;
            double h=70+HeightMultiplier*(600*hills+32*detail+mountains+dunes);
            if(GenerationVersion>=4)h=100+HeightMultiplier*Math.Min(100,10+60*hills+5*detail+mountains*.008+dunes*.15);
            double water=double.NegativeInfinity;
            Biome biome=GenerationVersion>=4?climate.Dominant:moisture<.27?Biome.Desert:moisture>.61?Biome.Forest:Biome.Grassland;
            // Smooth analytic catchments. Channels descend across the ribbon toward basin lakes.
            // These are deterministic carved rivers, not a simulated global drainage network.
            double basin=131072;
            double drainageAlong=along,drainageAcross=across;
            if(GenerationVersion>=3)
            {
                // Keep the named research lake navigable while deforming catchments
                // continuously elsewhere; no discontinuous per-cell offsets.
                var lakeSite=Landmarks[3];double sx=Geometry.AlongDistance(along,lakeSite.Along),sy=across-lakeSite.Across;
                double fade=Smooth(Math.Min(1,Math.Sqrt(sx*sx+sy*sy)/16000));
                drainageAlong+=(Noise(along,across,180000,503)-.5)*90000*fade;
                drainageAcross+=(Noise(along,across,145000,509)-.5)*70000*fade;
            }
            long nx=Math.Max(2,(long)Math.Round(p.Circumference/basin));
            double px=RingGeometry.Wrap(drainageAlong,p.Circumference)/p.Circumference*nx;
            double yy=RingGeometry.Wrap(drainageAcross+basin/2,basin)-basin/2;
            double riverX=(px-Math.Floor(px)-.5)*basin-3000*Math.Sin(2*Math.PI*yy/basin)-900*Math.Sin(6*Math.PI*yy/basin);
            double riverLevel=20+120*(1-Math.Cos(Math.PI*yy/(basin/2)))/2;
            double channelWidth=260+340*Noise(along,across,85000,137);
            double bank=Math.Min(1,Math.Abs(riverX)/channelWidth);
            h=Mix(riverLevel-7,h,Smooth(bank));
            if(Math.Abs(riverX)<channelWidth) { water=riverLevel; biome=Biome.River; }
            double lake=Math.Sqrt(Math.Pow((px-Math.Floor(px)-.5)*basin/(2700+1800*Noise(along,across,150000,139)),2)+Math.Pow(yy/(2000+1400*Noise(along,across,120000,149)),2));
            if(lake<1.3) { h=Mix(0,h,Smooth(Math.Max(0,Math.Min(1,(lake-.7)/.6)))); water=20; biome=Biome.Lake; }
            if(GenerationVersion>=3&&PondAmount>0&&double.IsNegativeInfinity(water)&&mountains<500)
            {
                long pondPeriod=Math.Max(2,(long)Math.Round(p.Circumference/4096));
                double pxp=RingGeometry.Wrap(along,p.Circumference)/p.Circumference*pondPeriod,pyp=across/4096;
                long ix=(long)Math.Floor(pxp),iy=(long)Math.Floor(pyp);
                if(Hash(ix,iy,521)<.28*PondAmount)
                {
                    double dx=(pxp-ix-(.2+.6*Hash(ix,iy,523)))*4096,dy=(pyp-iy-(.2+.6*Hash(ix,iy,527)))*4096;
                    double radius=90+230*Hash(ix,iy,529),shape=Math.Sqrt(dx*dx+dy*dy*1.8)/radius;
                    if(shape<1.5)
                    {
                        double level=150+100*Hash(ix,iy,531);
                        h=Mix(level-5,h,Smooth(Math.Max(0,Math.Min(1,(shape-.6)/.9))));
                        water=level;biome=Biome.Lake;
                    }
                }
            }
            if(broad<.17) { h-=900*(.17-broad)/.17;water=0;biome=Biome.Ocean; }
            // An ancient transport corridor network, interpreted procedurally. Roads
            // disappear through open water/high peaks; detailed bridges are future assets.
            double roadX=RingGeometry.Wrap(along,p.Circumference)/p.Circumference*Math.Max(2,Math.Round(p.Circumference/65536));
            double roadOffset=(Noise(along,across,180000,131)-.5)*1800;
            double roadDistance=Math.Abs((roadX-Math.Floor(roadX)-.25)*65536-roadOffset);
            if(roadDistance<65&&double.IsNegativeInfinity(water)&&h<1400)biome=Biome.Road;
            if(GenerationVersion<4&&mountains>1300&&double.IsNegativeInfinity(water))biome=Biome.Mountain;
            foreach(var l in Landmarks)
            {
                double dx=Geometry.AlongDistance(along,l.Along),dy=across-l.Across;
                double d=Math.Sqrt(dx*dx+dy*dy)/l.Radius;
                if(d>=1.2) continue;
                if(l.Kind=="waterway") continue;
                if(l.Kind=="ocean") { h=Mix(MinimumHeight,h,Smooth(Math.Max(0,Math.Min(1,(d-.8)/.4))));water=0;biome=Biome.Ocean; }
                else if(l.Kind=="mountain"||l.Kind=="puncture")
                {
                    double cone=Math.Pow(Math.Max(0,1-d/1.2),1.5)*l.Height;
                    if(l.Kind=="puncture" && d<.1) cone*=.3+.7*d/.1;
                    h+=cone;water=double.NegativeInfinity;biome=h>3000?Biome.Snow:Biome.Mountain;
                }
                else { double blend=Smooth(Math.Min(1,Math.Max(0,(1.2-d)/.35)));h=Mix(h,l.Height,blend);water=double.NegativeInfinity;biome=l.Kind=="scrith"?Biome.Scrith:Biome.Ruins; }
            }
            if(Math.Abs(across)>=p.Width/2) { h=p.WallHeight;water=double.NegativeInfinity;biome=Biome.Rimwall; }
            if(!double.IsNegativeInfinity(water) && h>=water && biome!=Biome.Ocean) biome=Biome.Wetland;
            if(h>2800 && double.IsNegativeInfinity(water) && biome!=Biome.Ruins) biome=Biome.Snow;
            double shore=double.IsNegativeInfinity(water)?0:Math.Max(0,1-Math.Abs(h-water)/15);
            bool natural=biome==Biome.Grassland||biome==Biome.Forest||biome==Biome.Desert||biome==Biome.Mountain||biome==Biome.Snow||biome==Biome.Wetland||biome==Biome.Ocean&&h>=water;
            var tint=climate.Ground;
            double exposure=GenerationVersion>=4?Smooth(Math.Max(0,Math.Min(1,(Noise(along,across,1700,947)-.58)/.13)))*climate.Desert:0;
            if(exposure>.65&&natural&&double.IsNegativeInfinity(water))biome=Biome.Scrith;
            if(GenerationVersion>=4&&natural){tint=tint*(1-exposure)+new DVec(.30,.34,.36)*exposure;tint=tint*(1-shore)+new DVec(.65,.57,.40)*shore;if(h>2800)tint=tint*.3+new DVec(.76,.81,.81)*.7;}
            return new TerrainSample {Height=h,WaterHeight=water,Biome=biome,Shore=shore,BlendedColor=GenerationVersion>=4&&natural,GroundColor=tint,Temperature=Math.Max(210,293-.0065*Math.Max(0,h))};
        }
        public Landmark Nearest(double along,double across,out double distance)
        {
            Landmark best=null;distance=double.MaxValue;
            foreach(var l in Landmarks) { double x=Geometry.AlongDistance(along,l.Along),y=across-l.Across;double d=Math.Sqrt(x*x+y*y);if(d<distance){best=l;distance=d;} }
            return best;
        }
    }
}
