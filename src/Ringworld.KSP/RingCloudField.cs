using System;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class RingCloudField
    {
        internal static Vector4 Handoff(Settings s,bool volume)
        {
            float range=volume?(float)Math.Min(180000,s.CloudRange):180000;
            return new Vector4(range*.4f,range*.72f,0,0);
        }
        internal static string Describe(WeatherSample w){return w.Storm>.05?"Thunderstorm":w.Rain>.05?"Rain":w.Cloud>.65?"Overcast":w.Cloud>.15?"Partly cloudy":"Clear";}
        internal static Vector3 Origin(Settings s,double along,double across,double altitude,double time)
        {
            double period=s.Geometry.P.Circumference/Math.Round(s.Geometry.P.Circumference/512000);
            double x=RingGeometry.Wrap((along-time*s.CloudWind)/period,1)*512000;
            double y=RingGeometry.Wrap(across+time*s.CloudWind*.1875,512000);
            return new Vector3((float)(x+s.Terrain.Scatter(1,0,1249)*512000),(float)(y+s.Terrain.Scatter(2,0,1249)*512000),(float)(altitude+s.Terrain.Scatter(3,0,1249)*64000));
        }
        internal static WeatherSample Apply(Material material,Settings s,double along,double across,double altitude,double time)
        {
            var weather=s.Weather(along,across,time);
            material.SetVector("_CloudOrigin",Origin(s,along,across,altitude,time));material.SetFloat("_CloudAmount",(float)weather.Cloud);
            float flash=s.LightningEnabled&&TimeWarp.CurrentRate<=10?(float)RingWeather.Lightning(s.Terrain,time,weather.Storm):0;
            material.SetFloat("_Lightning",flash*.25f);return weather;
        }
    }
}
