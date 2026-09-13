using System;
using System.Collections.Generic;
using System.Globalization;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    internal sealed class VesselRecord
    {
        internal string Id;
        internal DVec Position,Velocity;
        internal Quaternion Rotation=Quaternion.identity;
        internal bool Restored;
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames,GameScenes.FLIGHT,GameScenes.SPACECENTER,GameScenes.TRACKSTATION)]
    public sealed class RingworldScenario : ScenarioModule
    {
        public static RingworldScenario Instance;
        internal readonly Dictionary<string,VesselRecord> Vessels=new Dictionary<string,VesselRecord>();
        internal readonly HashSet<string> Discoveries=new HashSet<string>();
        internal bool Expedition;
        private static string Num(double x) { return x.ToString("R",CultureInfo.InvariantCulture); }
        private static double Read(ConfigNode n,string key,double fallback=0)
        { double v;return double.TryParse(n.GetValue(key),NumberStyles.Float,CultureInfo.InvariantCulture,out v)&&RingParameters.Finite(v)?v:fallback; }
        public override void OnAwake() { base.OnAwake();Instance=this; }
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);Instance=this;Vessels.Clear();Discoveries.Clear();Expedition=node.GetValue("expedition")=="True";
            foreach(var n in node.GetNodes("VESSEL"))
            {
                string id=n.GetValue("id");Guid parsed;
                if(!Guid.TryParse(id,out parsed)) continue;
                var r=new VesselRecord{Id=id,Position=new DVec(Read(n,"x"),Read(n,"y"),Read(n,"z")),Velocity=new DVec(Read(n,"vx"),Read(n,"vy"),Read(n,"vz")),
                    Rotation=new Quaternion((float)Read(n,"qx"),(float)Read(n,"qy"),(float)Read(n,"qz"),(float)Read(n,"qw",1))};
                if(r.Position.Length>1000) Vessels[id]=r;
            }
            foreach(string id in node.GetValues("discovery")) Discoveries.Add(id);
        }
        public override void OnSave(ConfigNode node)
        {
            if(RingworldFlight.Instance!=null) RingworldFlight.Instance.Capture();
            base.OnSave(node);node.AddValue("formatVersion",2);node.AddValue("positionReference","vesselRoot");node.AddValue("expedition",Expedition);
            foreach(var r in Vessels.Values)
            {
                var n=node.AddNode("VESSEL");n.AddValue("id",r.Id);
                n.AddValue("x",Num(r.Position.X));n.AddValue("y",Num(r.Position.Y));n.AddValue("z",Num(r.Position.Z));
                n.AddValue("vx",Num(r.Velocity.X));n.AddValue("vy",Num(r.Velocity.Y));n.AddValue("vz",Num(r.Velocity.Z));
                n.AddValue("qx",Num(r.Rotation.x));n.AddValue("qy",Num(r.Rotation.y));n.AddValue("qz",Num(r.Rotation.z));n.AddValue("qw",Num(r.Rotation.w));
            }
            foreach(string id in Discoveries) node.AddValue("discovery",id);
        }
        public void OnDestroy() { if(Instance==this) Instance=null; }
    }
}
