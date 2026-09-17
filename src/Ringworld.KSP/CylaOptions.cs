using System;
using System.Globalization;
using UnityEngine;
namespace NivenRingworld
{
    // Optical values only. Ring geometry and atmospheric forces remain independent.
    internal sealed class CylaOptions
    {
        internal sealed class Option
        {
            internal readonly string Key,Label; internal readonly double Default,Min,Max;
            internal Option(string key,string label,double value,double min,double max){Key="cyla"+key;Label=label;Default=value;Min=min;Max=max;}
        }
        internal static readonly Option[] Definitions={
            new Option("ViewSteps","View integration steps (1–500)",32,1,500),
            new Option("RayleighR","Rayleigh red coefficient (per million metres)",5.8,0,1000),
            new Option("RayleighG","Rayleigh green coefficient (per million metres)",13.5,0,1000),
            new Option("RayleighB","Rayleigh blue coefficient (per million metres)",33.1,0,1000),
            new Option("RayleighIntensity","Rayleigh intensity (0–20)",1,0,20),
            new Option("RayleighHeight","Rayleigh scale height (metres, 1–60000)",8500,1,60000),
            new Option("MieR","Mie red coefficient (per million metres)",12,0,1000),
            new Option("MieG","Mie green coefficient (per million metres)",12,0,1000),
            new Option("MieB","Mie blue coefficient (per million metres)",12,0,1000),
            new Option("MieIntensity","Mie intensity (0–20)",1,0,20),
            new Option("MieHeight","Mie scale height (metres, 1–60000)",1200,1,60000),
            new Option("Asymmetry","Mie asymmetry (0–0.99; 1 is singular)",.76,0,.99),
            new Option("Thickness","Optical atmosphere thickness (metres, 100–600000)",60000,100,600000),
            new Option("TransparentDepth","Transparent boundary depth / thickness (0.001–1)",1,.001,1),
            new Option("ProxyRadius","Optical proxy radius (metres, 1000000–100000000)",100000000,1000000,100000000),
            new Option("WidthScale","Optical width multiplier (0.01–2)",1,.01,2),
            new Option("OffsetAlong","Optical offset along ring (metres, ±600000)",0,-600000,600000),
            new Option("OffsetAcross","Optical offset across ring (metres, ±600000)",0,-600000,600000),
            new Option("OffsetUp","Optical offset inward (metres, ±600000)",0,-600000,600000),
            new Option("Pitch","Optical axis pitch (degrees, ±90)",0,-90,90),
            new Option("Yaw","Optical axis yaw (degrees, ±90)",0,-90,90)
        };
        private readonly double[] values=new double[Definitions.Length];
        internal int LightingMode;
        internal CylaOptions(){for(int i=0;i<values.Length;i++)values[i]=Definitions[i].Default;}
        internal double this[string key]{get{for(int i=0;i<values.Length;i++)if(Definitions[i].Key=="cyla"+key)return values[i];throw new ArgumentException(key);}}
        internal void Load(ConfigNode n)
        {
            for(int i=0;i<values.Length;i++){double v;var d=Definitions[i];values[i]=double.TryParse(n.GetValue(d.Key),NumberStyles.Float,CultureInfo.InvariantCulture,out v)&&!double.IsNaN(v)&&!double.IsInfinity(v)?Math.Max(d.Min,Math.Min(d.Max,v)):d.Default;}
            int mode;LightingMode=int.TryParse(n.GetValue("cylaLightingMode"),out mode)?Math.Max(0,Math.Min(2,mode)):0;
        }
        internal void Save(ConfigNode n){for(int i=0;i<values.Length;i++)n.SetValue(Definitions[i].Key,values[i].ToString("R",CultureInfo.InvariantCulture),true);n.SetValue("cylaLightingMode",LightingMode,true);}
        internal Vector3 Scattering(string kind){return new Vector3((float)this[kind+"R"],(float)this[kind+"G"],(float)this[kind+"B"])*(float)(this[kind+"Intensity"]*1e-6);}
    }
}
