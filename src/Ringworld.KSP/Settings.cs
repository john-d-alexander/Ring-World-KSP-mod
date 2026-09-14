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
        internal RingGeometry Geometry;
        internal TerrainGenerator Terrain;
        internal int TileResolution=32, TileRadius=3;
        internal double TileSize=1024;
        internal double StructuralThickness=100;
        internal double UndersideAltitude { get { return TerrainGenerator.MinimumHeight-StructuralThickness; } }
        internal bool Atmosphere=true;
        internal double LodRange=160000000, Haze=1, CloudAmount=.45, HeightMultiplier=1, ForestDensity=1;
        internal int LodResolution=16, GenerationBudget=1, GenerationVersion=2;
        internal bool DynamicWeather=true;
        internal void Apply(ConfigNode n)
        {
            Geometry.P.Seed=(int)Read(n,"seed",Geometry.P.Seed);
            LodRange=Math.Max(200000,Math.Min(160000000,Read(n,"lodRange",LodRange)));
            LodResolution=(int)Read(n,"lodResolution",LodResolution);LodResolution=LodResolution>=32?32:LodResolution>=16?16:8;
            GenerationBudget=(int)Math.Max(1,Math.Min(4,Read(n,"generationBudget",GenerationBudget)));
            Haze=Math.Max(0,Math.Min(2,Read(n,"haze",Haze)));
            CloudAmount=Math.Max(0,Math.Min(1,Read(n,"cloudAmount",CloudAmount)));
            DynamicWeather=n.GetValue("dynamicWeather")!="False";
            HeightMultiplier=Math.Max(.25,Math.Min(3,Read(n,"heightMultiplier",HeightMultiplier)));
            ForestDensity=Math.Max(0,Math.Min(2,Read(n,"forestDensity",ForestDensity)));
            GenerationVersion=(int)Read(n,"generationVersion",GenerationVersion);
            Geometry.P.DaySeconds=Math.Max(1800,Math.Min(86400,Read(n,"daySeconds",Geometry.P.DaySeconds)));
            Terrain=new TerrainGenerator(Geometry){HeightMultiplier=HeightMultiplier,GenerationVersion=GenerationVersion};
        }
        internal ConfigNode Save()
        {
            var n=new ConfigNode("OPTIONS");
            n.AddValue("seed",Geometry.P.Seed);n.AddValue("lodRange",LodRange.ToString("R",CultureInfo.InvariantCulture));
            n.AddValue("lodResolution",LodResolution);n.AddValue("generationBudget",GenerationBudget);
            n.AddValue("haze",Haze.ToString("R",CultureInfo.InvariantCulture));n.AddValue("cloudAmount",CloudAmount.ToString("R",CultureInfo.InvariantCulture));
            n.AddValue("dynamicWeather",DynamicWeather);n.AddValue("heightMultiplier",HeightMultiplier.ToString("R",CultureInfo.InvariantCulture));
            n.AddValue("forestDensity",ForestDensity.ToString("R",CultureInfo.InvariantCulture));n.AddValue("generationVersion",GenerationVersion);
            n.AddValue("daySeconds",Geometry.P.DaySeconds.ToString("R",CultureInfo.InvariantCulture));return n;
        }
        internal static Settings Load()
        {
            var s=new Settings();var p=new RingParameters();
            var nodes=GameDatabase.Instance.GetConfigNodes("NIVEN_RINGWORLD");
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
