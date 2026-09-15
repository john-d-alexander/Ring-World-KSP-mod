using System;
using System.Globalization;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    internal static class ConvertVector
    {
        internal static DVec Core(Vector3d v) { return new DVec(v.x,v.y,v.z); }
        internal static Vector3d Ksp(DVec v) { return new Vector3d(v.X,v.Y,v.Z); }
        internal static Vector3 Unity(DVec v) { return new Vector3((float)v.X,(float)v.Y,(float)v.Z); }
        internal static Vector3d Orbit(Vector3d v) { return new Vector3d(v.x,v.z,v.y); }
    }
    internal sealed class Settings
    {
        private static bool warnedMultipleDefinitions;
        internal RingGeometry Geometry;
        internal TerrainGenerator Terrain;
        internal int TileResolution=32, TileRadius=3;
        internal double TileSize=1024;
        internal double StructuralThickness=100;
        internal double UndersideAltitude { get { return TerrainGenerator.MinimumHeight-StructuralThickness; } }
        internal bool Atmosphere=true;
        internal double LodRange=160000000, Haze=1, CloudAmount=.45, HeightMultiplier=1, ForestDensity=1;
        internal int LodResolution=8, GenerationBudget=1, GenerationVersion=4;
        internal bool DynamicWeather=true,ShowTrajectory=true;
        internal double PredictionSeconds=3600, SurfaceWarpLimit=1000;
        internal double PondAmount=1,DetailDistance=75;
        internal bool AmbientParticles=true;
        internal int VisualQuality=0,CloudSteps=64,AtmosphereSteps=32,PhotoSamples=16,WaterQuality=0;
        internal double CloudRange=180000,CloudShadow=.85,AtmosphereExposure=1,WaveHeight=.65;
        internal bool FullRingDetail=false;
        internal double WeatherPeriod=21600,WeatherVariation=1,StormChance=.25,CloudWind=8,RainDensity=.7;
        internal bool RainEnabled=true,LightningEnabled=true;
        internal WeatherSample Weather(double along,double across,double time){return RingWeather.Sample(Terrain,along,across,time,CloudAmount,DynamicWeather,WeatherPeriod,WeatherVariation,StormChance);}
        internal void Apply(ConfigNode n)
        {
            WeatherPeriod=Math.Max(600,Math.Min(604800,Read(n,"weatherPeriod",21600)));
            WeatherVariation=Math.Max(0,Math.Min(1,Read(n,"weatherVariation",1)));StormChance=Math.Max(0,Math.Min(1,Read(n,"stormChance",.25)));
            CloudWind=Math.Max(0,Math.Min(100,Read(n,"cloudWind",8)));RainDensity=Math.Max(0,Math.Min(1,Read(n,"rainDensity",.7)));
            RainEnabled=n.GetValue("rainEnabled")!="False";LightningEnabled=n.GetValue("lightningEnabled")!="False";
            VisualQuality=(int)Math.Max(0,Math.Min(2,Read(n,"visualQuality",0)));
            FullRingDetail=n.GetValue("fullRingDetail")=="True";
            CloudSteps=(int)Math.Max(32,Math.Min(256,Read(n,"cloudSteps",64)));
            AtmosphereSteps=(int)Math.Max(16,Math.Min(96,Read(n,"atmosphereSteps",32)));
            PhotoSamples=(int)Math.Max(1,Math.Min(64,Read(n,"photoSamples",16)));
            WaterQuality=(int)Math.Max(0,Math.Min(2,Read(n,"waterQuality",0)));
            CloudRange=Math.Max(30000,Math.Min(500000,Read(n,"cloudRange",180000)));
            CloudShadow=Math.Max(0,Math.Min(1,Read(n,"cloudShadow",.85)));
            AtmosphereExposure=Math.Max(.25,Math.Min(2,Read(n,"atmosphereExposure",1)));
            WaveHeight=Math.Max(0,Math.Min(2,Read(n,"waveHeight",.65)));
            Geometry.P.Radius=Math.Max(1000000000,Math.Min(100000000000,Read(n,"radius",Geometry.P.Radius)));
            Geometry.P.Width=Math.Max(10000000,Math.Min(Geometry.P.Radius*.5,Read(n,"width",Geometry.P.Width)));
            Geometry.P.Gravity=Math.Max(1,Math.Min(30,Read(n,"gravity",Geometry.P.Gravity)));
            Geometry.P.SurfaceDensity=Math.Max(0,Math.Min(100000000,Read(n,"surfaceDensity",1000000)));
            Geometry.P.WallHeight=Math.Max(60000,Math.Min(1000000,Read(n,"wallHeight",Geometry.P.WallHeight)));
            Geometry.P.Validate();
            PredictionSeconds=Math.Max(60,Math.Min(86400,Read(n,"predictionSeconds",PredictionSeconds)));
            SurfaceWarpLimit=Math.Max(10,Math.Min(10000,Read(n,"surfaceWarpLimit",SurfaceWarpLimit)));
            ShowTrajectory=n.GetValue("showTrajectory")!="False";
            DetailDistance=Math.Max(25,Math.Min(250,Read(n,"detailDistance",DetailDistance)));
            AmbientParticles=n.GetValue("ambientParticles")!="False";
            PondAmount=Math.Max(0,Math.Min(2,Read(n,"pondAmount",PondAmount)));
            Geometry.P.Seed=(int)Read(n,"seed",Geometry.P.Seed);
            LodRange=Math.Max(200000,Math.Min(TerrainLodPlan.MaximumRange,Read(n,"lodRange",LodRange)));
            LodResolution=(int)Read(n,"lodResolution",LodResolution);LodResolution=LodResolution>=32?32:LodResolution>=16?16:8;
            GenerationBudget=(int)Math.Max(1,Math.Min(4,Read(n,"generationBudget",GenerationBudget)));
            Haze=Math.Max(0,Math.Min(2,Read(n,"haze",Haze)));
            CloudAmount=Math.Max(0,Math.Min(1,Read(n,"cloudAmount",CloudAmount)));
            DynamicWeather=n.GetValue("dynamicWeather")!="False";
            HeightMultiplier=Math.Max(.25,Math.Min(3,Read(n,"heightMultiplier",HeightMultiplier)));
            ForestDensity=Math.Max(0,Math.Min(2,Read(n,"forestDensity",ForestDensity)));
            GenerationVersion=(int)Read(n,"generationVersion",GenerationVersion);
            Geometry.P.DaySeconds=Math.Max(60,Math.Min(2592000,Read(n,"daySeconds",Geometry.P.DaySeconds)));
            Terrain=new TerrainGenerator(Geometry){HeightMultiplier=HeightMultiplier,GenerationVersion=GenerationVersion,PondAmount=PondAmount};
        }
        internal ConfigNode Save()
        {
            var n=new ConfigNode("OPTIONS");
            n.AddValue("weatherPeriod",WeatherPeriod.ToString("R",CultureInfo.InvariantCulture));n.AddValue("weatherVariation",WeatherVariation.ToString("R",CultureInfo.InvariantCulture));n.AddValue("stormChance",StormChance.ToString("R",CultureInfo.InvariantCulture));n.AddValue("cloudWind",CloudWind.ToString("R",CultureInfo.InvariantCulture));n.AddValue("rainDensity",RainDensity.ToString("R",CultureInfo.InvariantCulture));n.AddValue("rainEnabled",RainEnabled);n.AddValue("lightningEnabled",LightningEnabled);
            n.AddValue("fullRingDetail",FullRingDetail);
            n.AddValue("visualQuality",VisualQuality);n.AddValue("cloudSteps",CloudSteps);n.AddValue("atmosphereSteps",AtmosphereSteps);n.AddValue("photoSamples",PhotoSamples);n.AddValue("waterQuality",WaterQuality);
            n.AddValue("cloudRange",CloudRange.ToString("R",CultureInfo.InvariantCulture));n.AddValue("cloudShadow",CloudShadow.ToString("R",CultureInfo.InvariantCulture));n.AddValue("atmosphereExposure",AtmosphereExposure.ToString("R",CultureInfo.InvariantCulture));n.AddValue("waveHeight",WaveHeight.ToString("R",CultureInfo.InvariantCulture));
            foreach(var pair in new[]{new[]{"radius",Geometry.P.Radius.ToString("R",CultureInfo.InvariantCulture)},new[]{"width",Geometry.P.Width.ToString("R",CultureInfo.InvariantCulture)},new[]{"gravity",Geometry.P.Gravity.ToString("R",CultureInfo.InvariantCulture)},new[]{"wallHeight",Geometry.P.WallHeight.ToString("R",CultureInfo.InvariantCulture)},new[]{"surfaceDensity",Geometry.P.SurfaceDensity.ToString("R",CultureInfo.InvariantCulture)},new[]{"predictionSeconds",PredictionSeconds.ToString("R",CultureInfo.InvariantCulture)},new[]{"surfaceWarpLimit",SurfaceWarpLimit.ToString("R",CultureInfo.InvariantCulture)},new[]{"pondAmount",PondAmount.ToString("R",CultureInfo.InvariantCulture)}})n.AddValue(pair[0],pair[1]);
            n.AddValue("showTrajectory",ShowTrajectory);n.AddValue("detailDistance",DetailDistance.ToString("R",CultureInfo.InvariantCulture));n.AddValue("ambientParticles",AmbientParticles);
            n.AddValue("seed",Geometry.P.Seed);n.AddValue("lodRange",LodRange.ToString("R",CultureInfo.InvariantCulture));
            n.AddValue("lodResolution",LodResolution);n.AddValue("generationBudget",GenerationBudget);
            n.AddValue("haze",Haze.ToString("R",CultureInfo.InvariantCulture));n.AddValue("cloudAmount",CloudAmount.ToString("R",CultureInfo.InvariantCulture));
            n.AddValue("dynamicWeather",DynamicWeather);n.AddValue("heightMultiplier",HeightMultiplier.ToString("R",CultureInfo.InvariantCulture));
            n.AddValue("forestDensity",ForestDensity.ToString("R",CultureInfo.InvariantCulture));n.AddValue("generationVersion",GenerationVersion);
            n.AddValue("daySeconds",Geometry.P.DaySeconds.ToString("R",CultureInfo.InvariantCulture));return n;
        }
        internal static Settings Load()
        {
            var s=new Settings();var p=new RingParameters{SurfaceDensity=1000000};
            var nodes=GameDatabase.Instance.GetConfigNodes("NIVEN_RINGWORLD");
            if(nodes.Length>1&&!warnedMultipleDefinitions){warnedMultipleDefinitions=true;Debug.LogWarning("[NivenRingworld] Multiple NIVEN_RINGWORLD definitions found. This release supports one habitat and uses the first definition; additional nodes do not spawn rings.");}
            if(nodes.Length>0)
            {
                var n=nodes[0];
                p.Radius=Read(n,"radius",p.Radius);p.Width=Read(n,"width",p.Width);p.WallHeight=Read(n,"wallHeight",p.WallHeight);
                p.Gravity=Read(n,"gravity",p.Gravity);p.DaySeconds=Read(n,"daySeconds",p.DaySeconds);
                p.AtmosphereHeight=Read(n,"atmosphereHeight",p.AtmosphereHeight);p.ScaleHeight=Read(n,"scaleHeight",p.ScaleHeight);
                p.Seed=(int)Read(n,"seed",p.Seed);
                s.TileResolution=(int)Math.Max(16,Math.Min(64,Read(n,"tileResolution",32)));
                s.TileRadius=(int)Math.Max(2,Math.Min(5,Read(n,"tileRadius",3)));
                s.TileSize=Math.Max(256,Math.Min(4096,Read(n,"tileSize",1024)));
                s.StructuralThickness=Math.Max(1,Math.Min(10000,Read(n,"structuralThickness",100)));
                s.Atmosphere=n.GetValue("atmosphere")!="false";
            }
            s.Geometry=new RingGeometry(p);s.Terrain=new TerrainGenerator(s.Geometry);return s;
        }
        private static double Read(ConfigNode n,string key,double fallback)
        { double value;return double.TryParse(n.GetValue(key),NumberStyles.Float,CultureInfo.InvariantCulture,out value)&&RingParameters.Finite(value)?value:fallback; }
    }
}
