using System;
using System.Globalization;
namespace NivenRingworld
{
    // Rendering only: never changes seed, ring dimensions, physics or biome density.
    internal static class RingQualityPresets
    {
        internal static readonly string[] Names={"Absolute Cow","Extra Beefy","Beefy","Strong","Good","Mid","Slow","Better Potato","Potato","Aged Potato","Rotten Potato"};
        private static readonly string[] Keys={"lodRange","lodResolution","generationBudget","forestQuality","visualQuality","waterQuality","cloudSteps","atmosphereSteps","cloudRange","detailDistance","photoSamples","rainDensity"};
        private static readonly double[][] Values={
            new double[]{2000000000,32,4,3,2,2,256,96,500000,250,64,1},
            new double[]{1000000000,32,3,3,2,2,192,80,400000,225,48,1},
            new double[]{500000000,32,2,3,2,2,128,64,300000,200,32,.9},
            new double[]{320000000,16,2,2,1,2,112,56,250000,175,24,.8},
            new double[]{240000000,16,2,2,1,1,96,48,200000,150,16,.7},
            new double[]{160000000,16,1,1,1,1,64,32,150000,125,16,.6},
            new double[]{160000000,8,1,0,0,0,64,32,100000,100,12,.5},
            new double[]{160000000,8,1,0,0,0,48,24,75000,75,8,.4},
            new double[]{160000000,8,1,0,0,0,32,16,50000,50,4,.3},
            new double[]{160000000,8,1,0,0,0,32,16,40000,35,2,.15},
            new double[]{160000000,8,1,0,0,0,32,16,30000,25,1,0}
        };
        internal static void Apply(ConfigNode n,int index)
        {
            if(index<0||index>=Names.Length)throw new ArgumentOutOfRangeException("index");
            for(int k=0;k<Keys.Length;k++)n.SetValue(Keys[k],Values[index][k].ToString("R",CultureInfo.InvariantCulture),true);
            n.SetValue("fullRingDetail",index<=5,true);n.SetValue("ambientParticles",index<=7,true);
            n.SetValue("rainEnabled",index<10,true);n.SetValue("lightningEnabled",index<=8,true);
            n.SetValue("cloudShadow",index<=5?"0.85":"0.5",true);
            n.SetValue("waveHeight","0.65",true);n.SetValue("atmosphereExposure","1",true);
        }
        internal static string Match(Settings s)
        {
            var actual=s.Save();
            for(int i=0;i<Names.Length;i++)
            {
                var expected=new ConfigNode();Apply(expected,i);bool same=true;
                foreach(ConfigNode.Value v in expected.values)
                    if(actual.GetValue(v.name)!=v.value){same=false;break;}
                if(same)return Names[i];
            }
            return "Custom";
        }
    }
}
