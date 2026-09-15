using System;
using System.Collections.Generic;
using Ringworld.Core;
class Program
{
    static void Main()
    {
        Console.WriteLine("Uniform-area survey: generator 4, default dimensions, height/pond multiplier 1; 100,000 samples per seed. Sampling RNG 73031. Estimates, not placement quotas.");
        foreach(int seed in new[]{0,1970,76440740})
        {
            var p=new RingParameters{Seed=seed};var t=new TerrainGenerator(new RingGeometry(p)){GenerationVersion=4};var random=new Random(73031);var counts=new Dictionary<Biome,int>();
            foreach(Biome biome in Enum.GetValues(typeof(Biome)))counts[biome]=0;
            for(int i=0;i<100000;i++)counts[t.Sample(random.NextDouble()*p.Circumference,(random.NextDouble()-.5)*p.Width).Biome]++;
            Console.WriteLine("Seed "+seed+":");foreach(var row in counts)Console.WriteLine(row.Key+": "+(row.Value/1000.0).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"%");
        }
    }
}
