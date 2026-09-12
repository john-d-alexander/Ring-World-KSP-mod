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
        internal bool Atmosphere=true;
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
                s.Atmosphere=n.GetValue("atmosphere")!="false";
            }
            s.Geometry=new RingGeometry(p);s.Terrain=new TerrainGenerator(s.Geometry);return s;
        }
        private static double Read(ConfigNode n,string key,double fallback)
        { double value;return double.TryParse(n.GetValue(key),NumberStyles.Float,CultureInfo.InvariantCulture,out value)&&RingParameters.Finite(value)?value:fallback; }
    }
}
