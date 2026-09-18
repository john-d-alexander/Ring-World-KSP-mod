using System;
using System.Collections.Generic;
using System.Globalization;
using Ringworld.Core;
namespace NivenRingworld
{
    internal static class ResearchCatalog
    {
        internal static double Number(ConfigNode n,string key,double fallback)
        {double x;return double.TryParse(n.GetValue(key),NumberStyles.Float,CultureInfo.InvariantCulture,out x)&&RingParameters.Finite(x)?x:fallback;}
        internal static string Safe(string text)
        {return System.Text.RegularExpressions.Regex.Replace(text??"","[^a-zA-Z0-9_-]","-");}
        internal static string Name(ConfigNode n)
        {return n.GetValue("scienceName")??CultureInfo.InvariantCulture.TextInfo.ToTitleCase((n.GetValue("kind")??"Structure").Replace('_',' '));}
        internal static ResearchLocation Structure(Settings s,RingPoint p,string zone)
        {
            ResearchLocation result=null;double nearest=double.MaxValue;
            foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_LANDMARK_ASSET"))
            {
                var site=s.Terrain.Landmarks.Find(l=>l.Id==n.GetValue("site"));if(site==null)continue;
                double a=site.Along+Number(n,"along",0),b=site.Across+Number(n,"across",0);
                string id=n.GetValue("scienceId")??(site.Id+"-"+n.GetValue("kind")+"-"+Number(n,"along",0).ToString("R",CultureInfo.InvariantCulture)+"-"+Number(n,"across",0).ToString("R",CultureInfo.InvariantCulture));
                Match(s,p,n,a,b,id,zone,ref result,ref nearest);
            }
            var templates=GameDatabase.Instance.GetConfigNodes("RINGWORLD_COLOSSUS_ASSET");
            double cell=2000000,chance=.15,reach=500000;
            foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_COLOSSUS_DISTRIBUTION")){cell=Math.Max(1000000,Number(n,"cellSize",cell));chance=Math.Max(0,Math.Min(1,Number(n,"occupancy",chance)));}
            foreach(var n in templates)reach=Math.Max(reach,Number(n,"range",250000)+Number(n,"width",0)+Number(n,"height",0)+Number(n,"depth",0));
            if(templates.Length>0)foreach(var candidate in ColossusDistribution.Nearby(s.Terrain,p.Along,p.Across,cell,chance,reach))
            {
                var n=templates[Math.Min(templates.Length-1,(int)(candidate.Variant*templates.Length))];
                Match(s,p,n,candidate.Along,candidate.Across,n.GetValue("kind")+"-"+candidate.Key,zone,ref result,ref nearest);
            }
            return result;
        }
        internal static ResearchLocation CustomSite(Settings s,RingPoint p,string zone)
        {
            ResearchLocation result=null;double smallest=double.MaxValue;
            foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_RESEARCH_SITE"))
            {
                string id=Safe(n.GetValue("id"));if(id.Length==0)continue;
                double a=Number(n,"along",0),b=Number(n,"across",0),radius=Math.Max(1,Math.Min(1000000,Number(n,"radius",1000)));
                var parent=s.Terrain.Landmarks.Find(l=>l.Id==n.GetValue("site"));if(parent!=null){a+=parent.Along;b+=parent.Across;}
                double da=s.Geometry.AlongDistance(p.Along,a),db=p.Across-b;
                if(da*da+db*db>radius*radius||radius>=smallest||p.Altitude<Number(n,"minAltitude",-50000)||p.Altitude>Number(n,"maxAltitude",s.Geometry.P.AtmosphereHeight))continue;
                smallest=radius;result=new ResearchLocation("addon-"+id,n.GetValue("title")??id,"landmark",zone);
            }
            return result;
        }
        private static void Match(Settings s,RingPoint p,ConfigNode n,double a,double b,string id,string zone,ref ResearchLocation result,ref double nearest)
        {
            double w=Number(n,"width",100),h=Number(n,"height",100),d=Number(n,"depth",100),margin=Math.Max(0,Math.Min(10000,Number(n,"scienceMargin",500)));
            if(w<=0||h<=0||d<=0||Math.Abs(b)+d/2>=s.Geometry.P.Width/2)return;
            double da=Math.Abs(s.Geometry.AlongDistance(p.Along,a)),db=Math.Abs(p.Across-b);
            // A circumscribed footprint remains valid for every prefab yaw and LOD;
            // querying a renderer's bounds would make science depend on visibility.
            double footprint=Math.Sqrt(w*w+d*d)*.5+margin;
            if(da*da+db*db>footprint*footprint)return;
            var ground=s.Terrain.Sample(a,b);if(ground.Wet)return;
            double bottom=ground.Height+Number(n,"aboveGround",0);
            if(p.Altitude<bottom-margin||p.Altitude>bottom+h+margin)return;
            double score=da*da+db*db;if(score>=nearest)return;nearest=score;
            result=new ResearchLocation(Safe(id),Name(n),"structure",zone);
        }
        internal static float Multiplier(ResearchLocation location)
        {
            double value=location.Category=="structure"?18:location.Category=="panel"?20:location.Category=="wall"?16:location.Category=="landmark"?14:location.Zone=="surface"||location.Zone=="water"?10:location.Category=="orbit"?6:8;
            foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_SCIENCE_VALUE"))
                if((!n.HasValue("category")||n.GetValue("category")==location.Category)&&(!n.HasValue("zone")||n.GetValue("zone")==location.Zone))value=Number(n,"multiplier",value);
            return (float)Math.Max(.1,Math.Min(1000,value));
        }
        internal static List<ExpeditionObjective> Objectives()
        {
            var list=new List<ExpeditionObjective>();var ids=new HashSet<string>();
            foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_OBJECTIVE"))
            {
                string id=n.GetValue("id");if(string.IsNullOrEmpty(id)||!ids.Add(id))continue;
                list.Add(new ExpeditionObjective{Id=id,Title=n.GetValue("title")??id,Description=n.GetValue("description")??"Return or transmit research.",Requires=n.GetValue("requires")??"",Category=n.GetValue("category")??"",Zone=n.GetValue("zone")??"",Experiment=n.GetValue("experiment")??"",Count=(int)Math.Max(1,Math.Min(1000,Number(n,"count",1))),Funds=Math.Max(0,Number(n,"funds",0)),Reputation=Math.Max(0,Number(n,"reputation",0))});
            }
            return list;
        }
    }
}
