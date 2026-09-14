using System;
using System.Globalization;
using UnityEngine;
namespace NivenRingworld
{
    internal sealed class RingSettingsPanel
    {
        private bool initialized,dynamicWeather;
        private string seed,range,height,forest,day,haze,cloud,message="";
        private int quality,budget;
        private static string N(double n){return n.ToString("0.###",CultureInfo.InvariantCulture);}
        private static string Field(string title,string value){GUILayout.Label(title);return GUILayout.TextField(value,40);}
        internal void Reset(){initialized=false;}
        internal void Draw(RingworldFlight flight)
        {
            var state=RingworldScenario.Instance;if(state==null){GUILayout.Label("Waiting for save settings...");return;}
            var s=flight.Settings;
            if(!initialized)
            {
                seed=s.Geometry.P.Seed.ToString(CultureInfo.InvariantCulture);range=N(s.LodRange/1000);height=N(s.HeightMultiplier);forest=N(s.ForestDensity);
                day=N(s.Geometry.P.DaySeconds/3600);haze=N(s.Haze);cloud=N(s.CloudAmount*100);dynamicWeather=s.DynamicWeather;
                quality=s.LodResolution==8?0:s.LodResolution==16?1:2;budget=s.GenerationBudget-1;initialized=true;
            }
            GUILayout.Label("Settings are stored with this save.");
            if(GUILayout.Button("Laptop horizon preset: 160,000 km")){range="160000";quality=0;budget=0;}
            range=Field("Terrain horizon distance (200–160,000 km)",range);
            GUILayout.Label("Distant mesh quality");quality=GUILayout.Toolbar(quality,new[]{"Low (8)","Balanced (16)","High (32)"});
            GUILayout.Label("New mesh blocks per frame");budget=GUILayout.Toolbar(budget,new[]{"1","2","3","4"});
            GUILayout.Label("Near ground retains collision detail. Far terrain/water use scaled space. Small buildings and trees are nearby only; the full ring and rim walls have a coarse global model.");
            bool worldUnlocked=state.Vessels.Count==0&&state.Discoveries.Count==0;
            GUI.enabled=worldUnlocked;
            seed=Field("World seed (blank chooses a random seed)",seed);
            height=Field("Natural terrain height multiplier (0.25–3)",height);
            forest=Field("Forest density multiplier (0–2)",forest);
            GUI.enabled=true;
            if(!worldUnlocked)GUILayout.Label("World generation is locked after the first expedition to preserve ground beneath saved vessels. Use a new save for another world.");
            GUILayout.Label("Resolved seed: "+s.Geometry.P.Seed+" | terrain generation "+s.GenerationVersion);
            day=Field("Shadow-square day/night cycle (0.5–24 hours)",day);
            haze=Field("Atmospheric visual haze (0.1–2)",haze);
            cloud=Field("Cloud amount (0–100%; zero gives clear skies)",cloud);
            dynamicWeather=GUILayout.Toggle(dynamicWeather,"Moving weather fronts");
            GUILayout.Label("Weather changes cloud coverage; rain and wind forces are not simulated. Long views are coarse, not detailed terrain across the entire ribbon.");
            if(GUILayout.Button("Apply settings"))
            {
                double r,h,f,d,a,c;int resolved;
                if(!Number(range,200,160000,out r)||!Number(height,.25,3,out h)||!Number(forest,0,2,out f)||!Number(day,.5,24,out d)||!Number(haze,0,2,out a)||!Number(cloud,0,100,out c))
                {message="Enter finite numbers within the displayed ranges (use a decimal point).";return;}
                if(string.IsNullOrWhiteSpace(seed))resolved=worldUnlocked?BitConverter.ToInt32(Guid.NewGuid().ToByteArray(),0):s.Geometry.P.Seed;
                else if(!int.TryParse(seed,NumberStyles.Integer,CultureInfo.InvariantCulture,out resolved)){message="Seed must be a signed 32-bit integer or blank.";return;}
                var n=s.Save();
                n.SetValue("lodRange",N(r*1000));n.SetValue("lodResolution",new[]{8,16,32}[quality]);n.SetValue("generationBudget",budget+1);
                n.SetValue("haze",N(a));n.SetValue("cloudAmount",N(c/100));n.SetValue("dynamicWeather",dynamicWeather);
                n.SetValue("daySeconds",N(d*3600));
                if(worldUnlocked){n.SetValue("seed",resolved);n.SetValue("heightMultiplier",N(h));n.SetValue("forestDensity",N(f));n.SetValue("generationVersion",2);}
                flight.ApplyOptions(n,worldUnlocked);seed=s.Geometry.P.Seed.ToString(CultureInfo.InvariantCulture);
                message="Applied. Save your game to persist these settings.";
            }
            GUILayout.Label(message);
        }
        private static bool Number(string text,double min,double max,out double value)
        {return double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out value)&&!double.IsNaN(value)&&value>=min&&value<=max;}
    }
}
