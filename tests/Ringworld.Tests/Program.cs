using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Ringworld.Core;

static class Program
{
    static int checks;
    static void Check(bool ok,string text){checks++;if(!ok)throw new Exception(text);}
    static void Near(double actual,double expected,double tolerance,string text){Check(Math.Abs(actual-expected)<=tolerance,text+": "+actual+" != "+expected);}
    static void Main(string[] args)
    {
        var p=new RingParameters();var g=new RingGeometry(p);var t=new TerrainGenerator(g);
        Near(g.Position(0,0,0).Length,p.Radius,.00001,"radius");
        Near(p.Omega*p.Omega*p.Radius,p.Gravity,1e-10,"centrifugal acceleration");
        Near(p.RotationSeconds,2*Math.PI*Math.Sqrt(p.Radius/p.Gravity),1e-7,"spin period");
        var random=new Random(1970);
        for(int i=0;i<2000;i++)
        {
            double a=random.NextDouble()*p.Circumference,b=(random.NextDouble()-.5)*p.Width,h=random.NextDouble()*60000;
            var pos=g.Position(a,b,h);var c=g.Coordinates(pos);
            Near(g.AlongDistance(c.Along,a),0,.00004,"double precision along roundtrip");Near(c.Across,b,1e-8,"across roundtrip");Near(c.Altitude,h,.00001,"altitude roundtrip");
            var force=g.Acceleration(pos,new DVec(),0);
            Check(DVec.Dot(force,g.Up(pos))<0,"gravity must push toward inner floor");
            Near(force.Length,p.Gravity*(1-h/p.Radius),1e-10,"height-dependent g");
            var v=new DVec(random.NextDouble()*100,17,-34);
            Check((g.RotatingVelocity(pos,g.InertialVelocity(pos,v))-v).Length<1e-8,"frame velocity roundtrip");
            var coriolis=g.Acceleration(pos,v,0)-g.Acceleration(pos,new DVec(),0);
            Near(DVec.Dot(coriolis,v),0,1e-10,"Coriolis must do no work");
            var sample=t.Sample(a,b);var again=t.Sample(a,b);
            Near(sample.Height,again.Height,0,"terrain determinism");
            Check(RingParameters.Finite(sample.Height),"finite terrain");
            Near(t.Sample(a+p.Circumference,b).Height,sample.Height,.0001,"periodic terrain");
        }
        for(int i=0;i<100;i++)
        {
            double across=(random.NextDouble()-.5)*p.Width;
            Near(t.Sample(-.001,across).Height,t.Sample(p.Circumference-.001,across).Height,.0001,"circumference seam");
        }
        long catchments=(long)Math.Round(p.Circumference/131072);
        double riverAlong=(12345.5)*p.Circumference/catchments;
        for(int i=-50;i<50;i++)
        {
            double boundary=(i+.5)*131072;
            var left=t.Sample(riverAlong,boundary-.001);var right=t.Sample(riverAlong,boundary+.001);
            Near(left.Height,right.Height,.001,"catchment boundary continuity");
            Near(left.WaterHeight,right.WaterHeight,.001,"catchment water continuity");
        }
        var waterSite=t.Landmarks.Find(l=>l.Id=="waterway");
        Check(t.Sample(waterSite.Along,waterSite.Across).Wet,"waterway destination reaches a lake");
        // A stationary object becomes a freely falling trajectory inward in the inertial frame.
        var initial=g.Position(0,0,1000);var position=initial;var velocity=new DVec();double dt=.002;
        for(int i=0;i<5000;i++)
        {
            // Midpoint integration captures the Coriolis deflection without explicit Euler energy drift.
            var acceleration=g.Acceleration(position,velocity,0);
            var middleP=position+velocity*(dt/2);var middleV=velocity+acceleration*(dt/2);
            position+=middleV*dt;velocity+=g.Acceleration(middleP,middleV,0)*dt;
        }
        Near(g.Coordinates(position).Altitude,1000-.5*p.Gravity*100,0.01,"ten second drop");
        Check(g.AlongDistance(g.Coordinates(position).Along,0)<0,"drop deflects antispinward");
        Near(g.Density(p.AtmosphereHeight),0,0,"atmosphere cutoff");Check(g.Density(0)>g.Density(20000),"density falls with height");
        Near(g.Daylight(0,0),g.Daylight(0,p.DaySeconds),1e-12,"day periodicity");Near(g.Daylight(0,p.DaySeconds/2),0,1e-12,"shadow night");
        foreach(var l in t.Landmarks)
        {
            if(l.Kind=="city"||l.Kind=="outpost"||l.Kind=="terminal"||l.Kind=="scrith")
            {var s=t.Sample(l.Along,l.Across);Near(s.Height,l.Height,1e-6,"level landmark pad "+l.Id);Check(!s.Wet,"dry landmark "+l.Id);}
        }
        // The resolved terrain must be continuous across tile borders, independent of generation order.
        var site=t.Landmarks[0];var timer=Stopwatch.StartNew();
        for(int y=0;y<224;y++)for(int x=0;x<224;x++)t.Sample(site.Along+x*32,site.Across+y*32);
        timer.Stop();
        Console.WriteLine("PASS: "+checks+" checks; 50,176 terrain samples in "+timer.ElapsedMilliseconds+" ms.");
        Console.WriteLine("Radius: "+p.Radius+" m; width: "+p.Width+" m; spin: "+p.RotationSeconds.ToString("F2")+" s; rim speed: "+(p.Omega*p.Radius).ToString("F2")+" m/s.");
        if(args.Length>0)Export(t,args[0]);
    }
    static void Export(TerrainGenerator terrain,string dir)
    {
        Directory.CreateDirectory(dir);var origin=terrain.Landmarks[0];int n=240;double size=32000;
        string[] colors={"#074159","#186478","#267f92","#5d7046","#849453","#385d37","#baa373","#79746c","#e5ecec","#586a76","#ad9f83","#6d7885"};
        using(var w=new StreamWriter(Path.Combine(dir,"terrain-preview.svg")))
        {
            w.WriteLine("<svg xmlns='http://www.w3.org/2000/svg' width='1200' height='1320' viewBox='0 0 1200 1320'><rect width='1200' height='1320' fill='#111c25'/><text x='40' y='52' fill='white' font-family='sans-serif' font-size='26'>Ringworld | generated terrain near the expedition outpost</text><text x='40' y='85' fill='#bac8cf' font-family='sans-serif' font-size='18'>32 km square • same C# terrain sampler as the KSP plugin • plan view</text>");
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                var sample=terrain.Sample(origin.Along+(x/(double)n-.5)*size,origin.Across+(y/(double)n-.5)*size);
                double brightness=.58+Math.Min(.42,sample.Height/1800);
                string color=colors[(int)(sample.Wet?Biome.River:sample.Biome)];
                w.WriteLine(string.Format(CultureInfo.InvariantCulture,"<rect x='{0}' y='{1}' width='5' height='5' fill='{2}'/><rect x='{0}' y='{1}' width='5' height='5' fill='black' opacity='{3:F2}'/>",x*5,110+y*5,color,1-brightness));
            }
            w.WriteLine("<circle cx='600' cy='710' r='12' fill='none' stroke='white' stroke-width='3'/><text x='620' y='715' fill='white' font-family='sans-serif' font-size='18'>Landing field</text></svg>");
        }
    }
}
