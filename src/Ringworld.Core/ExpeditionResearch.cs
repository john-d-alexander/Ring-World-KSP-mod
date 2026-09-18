using System;
using System.Collections.Generic;
namespace Ringworld.Core
{
    // Stable, renderer-independent identities: changing LOD never changes science.
    public sealed class ResearchLocation
    {
        public string Id,Name,Category,Zone;
        public ResearchLocation(string id,string name,string category,string zone)
        {Id=id;Name=name;Category=category;Zone=zone;}
        public string Key {get{return Zone+"_"+Category+"_"+Id;}}
    }
    public static class ResearchRegions
    {
        public static ResearchLocation Locate(TerrainGenerator terrain,DVec position,bool landed,bool splashed,bool atmosphere,double time)
        {
            var g=terrain.Geometry;var p=g.P;var c=g.Coordinates(position);
            double radius=Math.Sqrt(position.X*position.X+position.Z*position.Z);
            double panelRadius=p.Radius*46/153;
            if(Math.Abs(radius-panelRadius)<500000&&Math.Abs(c.Across)<p.Width/2+500000)
            {
                double panelAngle=RingGeometry.Wrap(c.Along/p.Radius-2*Math.PI*time/(20*p.DaySeconds),2*Math.PI);
                int index=(int)Math.Floor(panelAngle/(2*Math.PI/20)+.5)%20;
                double offset=RingGeometry.Wrap(panelAngle-index*2*Math.PI/20+Math.PI,2*Math.PI)-Math.PI;
                if(Math.Abs(offset)*panelRadius<p.Radius*2/153+500000)
                    return new ResearchLocation("panel"+(index+1),"Near shadow square "+(index+1),"panel","space");
            }
            if(c.Altitude< -50000||c.Altitude>2000000||Math.Abs(c.Across)>p.Width/2+500000)return null;
            bool wall=Math.Abs(Math.Abs(c.Across)-p.Width/2)<500&&
                (Math.Abs(c.Altitude-p.WallHeight)<500||Math.Abs(c.Across)>=p.Width/2);
            if(wall)return new ResearchLocation(c.Across<0?"south":"north",(landed?"On":"Near")+(c.Across<0?" south":" north")+" rim wall","wall",landed?"surface":"space");
            if(c.Altitude>=p.AtmosphereHeight||!atmosphere)
            {
                if(!landed&&!splashed)return new ResearchLocation(c.Altitude>500000?"high":"low","Above the Ringworld"+(c.Altitude>500000?" — high space":" — low space"),"orbit",c.Altitude>500000?"highspace":"space");
            }
            if(Math.Abs(c.Across)>p.Width/2)return null;
            string zone=landed?"surface":splashed?"water":c.Altitude<18000?"lowair":"highair";
            Landmark chosen=null;double best=double.MaxValue;
            foreach(var l in terrain.Landmarks)
            {
                double da=g.AlongDistance(c.Along,l.Along),db=c.Across-l.Across;
                if(da*da+db*db<=l.Radius*l.Radius&&l.Radius<best){chosen=l;best=l.Radius;}
            }
            if(chosen!=null)return new ResearchLocation(chosen.Id,chosen.Name,"landmark",zone);
            var sample=terrain.Sample(c.Along,c.Across);
            return new ResearchLocation(sample.Biome.ToString(),sample.Biome.ToString(),"biome",zone);
        }
    }
    public sealed class ResearchReceipt
    {
        public string Subject,Experiment,Location,Category,Zone;
    }
    public sealed class ExpeditionObjective
    {
        public string Id,Title,Description,Requires="",Category="",Zone="",Experiment="";
        public int Count=1;
        public double Funds,Reputation;
        public int Progress(IEnumerable<ResearchReceipt> receipts)
        {
            var places=new HashSet<string>();
            foreach(var r in receipts)
                if((Category==""||Category==r.Category)&&(Zone==""||Zone==r.Zone)&&(Experiment==""||Experiment==r.Experiment))places.Add(r.Location);
            return Math.Min(Count,places.Count);
        }
        public bool Ready(IEnumerable<ResearchReceipt> receipts,ISet<string> completed)
        {return !completed.Contains(Id)&&(Requires==""||completed.Contains(Requires))&&Progress(receipts)>=Count;}
    }
}
