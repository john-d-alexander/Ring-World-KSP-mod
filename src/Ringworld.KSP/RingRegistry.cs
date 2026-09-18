using System;
using System.Collections.Generic;
using System.Globalization;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class RingSelection
    {
        internal static string Nearest(Vessel vessel)
        {
            var state=RingworldScenario.Instance;if(state==null||vessel==null)return null;state.GetOptions();
            VesselRecord record;if(state.Vessels.TryGetValue(vessel.id.ToString(),out record))return record.RingId;
            string result=null;double distance=double.PositiveInfinity;
            foreach(var node in state.Rings)
            {
                var s=state.RingSettings(node.GetValue("ringId")??"primary");if(s.Body==null||RingTrajectory.SolarPatch(vessel,s.Body)==null)continue;
                var c=s.Geometry.Coordinates(ConvertVector.Core(vessel.GetWorldPos3D()-s.Center));
                double across=Math.Max(0,Math.Abs(c.Across)-s.Geometry.P.Width/2);
                double d=Math.Sqrt(c.Altitude*c.Altitude+across*across);
                if(d<distance){distance=d;result=s.RingId;}
            }
            return result;
        }
        internal static string Validate(Settings candidate,string editingId)
        {
            if(candidate.Body==null)return "The reference star is not installed.";
            // Fixed parallel habitats must not share a contact/arrival shell. A conservative
            // enclosing cylinder test also prevents a spawn from covering an existing ring.
            var state=RingworldScenario.Instance;
            foreach(var n in state.Rings)
            {
                var other=Settings.Load();other.Apply(n);if(other.RingId==editingId||other.Body==null)continue;
                var delta=ConvertVector.Core(candidate.Center-other.Center);
                double horizontal=Math.Sqrt(delta.X*delta.X+delta.Z*delta.Z);
                double separation=Math.Abs(delta.Y)-(candidate.Geometry.P.Width+other.Geometry.P.Width)/2;
                if(separation<1000000&&horizontal<candidate.Geometry.P.Radius+other.Geometry.P.Radius+1000000)
                    return "Habitat envelopes overlap. Separate their centers or axial (Y) positions before applying.";
            }
            return null;
        }
    }
    [KSPAddon(KSPAddon.Startup.FlightAndKSC,false)]
    public sealed class RingRenderRegistry : MonoBehaviour
    {
        private readonly Dictionary<string,ScaledRing> renderers=new Dictionary<string,ScaledRing>();
        public void Update()
        {
            var state=RingworldScenario.Instance;if(state==null)return;state.GetOptions();
            foreach(var node in state.Rings)
            {
                string id=node.GetValue("ringId")??"primary";
                if(renderers.ContainsKey(id))continue;
                var obj=new GameObject("Ringworld "+id);obj.transform.SetParent(transform,false);
                var renderer=obj.AddComponent<ScaledRing>();renderer.RingId=id;renderers[id]=renderer;
            }
            var removed=new List<string>();
            foreach(var pair in renderers)if(state.RingOptions(pair.Key)==null){if(pair.Value!=null)Destroy(pair.Value.gameObject);removed.Add(pair.Key);}
            foreach(var id in removed)renderers.Remove(id);
        }
    }
    internal sealed class RingSandboxEditor
    {
        private bool open,stars,confirmDelete;
        private string selected,name="",reference="Sun",x="0",y="0",z="0",diameter="30600000",width="160500",seed="",message="";
        private bool designated=true;
        private static string N(double v){return v.ToString("R",CultureInfo.InvariantCulture);}
        private static string Field(string label,string value){GUILayout.Label(label);return GUILayout.TextField(value,80);}
        private void Load(ConfigNode node)
        {
            var s=Settings.Load();s.Apply(node);selected=s.RingId;name=s.RingName;reference=s.ReferenceBody;designated=s.DesignatedStar;
            x=N(s.CenterOffset.X/1000);y=N(s.CenterOffset.Y/1000);z=N(s.CenterOffset.Z/1000);
            diameter=N(s.Geometry.P.Radius/500);width=N(s.Geometry.P.Width/1000);seed=N(s.Geometry.P.Seed);confirmDelete=false;
        }
        internal void Draw(RingworldFlight flight)
        {
            if(!RingworldFlight.SandboxControls)return;var state=RingworldScenario.Instance;
            open=GUILayout.Toggle(open,"Sandbox: manage ring worlds");if(!open)return;
            if(selected==null||state.RingOptions(selected)==null)Load(state.GetOptions());
            GUILayout.Label("Each ring is saved separately. Move/delete requires no resident vessels. Save your game after editing.");
            foreach(var n in state.Rings)if(GUILayout.Button((n.GetValue("ringId")==selected?"> ":"")+(n.GetValue("ringName")??"Ringworld")))Load(n);
            name=Field("Name",name);
            designated=GUILayout.Toggle(designated,"Designated existing star");
            if(designated)
            {
                if(GUILayout.Button("Star: "+reference+" v"))stars=!stars;
                if(stars)foreach(var body in FlightGlobals.Bodies)if(body.isStar&&GUILayout.Button(body.displayName)){reference=body.name;stars=false;}
            }
            else GUILayout.Label("No new star. Center is fixed relative to the stock Sun; normal stellar gravity still applies.");
            GUILayout.Label("Center offset in km, in KSP's non-rotating reference axes; Y runs across the ring. Centers follow the reference body. Ring planes remain parallel.");
            x=Field("X (km)",x);y=Field("Y (km)",y);z=Field("Z (km)",z);
            diameter=Field("Diameter (km)",diameter);width=Field("Width (km)",width);seed=Field("Seed (blank = random)",seed);
            if(GUILayout.Button("Spawn a new ring using these fields"))Apply(flight,true);
            if(GUILayout.Button("Apply name / location / dimensions to selected ring"))Apply(flight,false);
            if(GUILayout.Button("Visit selected ring (spin-matched)")){string reason;if(!flight.VisitRing(selected,out reason))message=reason;else message="Transferring to selected ring.";}
            confirmDelete=GUILayout.Toggle(confirmDelete,"Confirm deletion of selected ring");
            if(GUILayout.Button("Delete selected ring"))
            {
                if(!confirmDelete)message="Select the deletion confirmation first.";
                else if(state.Rings.Count<=1)message="Keep at least one habitat in this save.";
                else if(state.Occupied(selected)||(flight.Active&&flight.Settings.RingId==selected))message="Move or recover all resident vessels first.";
                else {var n=state.RingOptions(selected);state.Rings.Remove(n);if(state.ActiveRingId==selected){state.SelectRing(state.Rings[0].GetValue("ringId")??"primary");flight.ApplyOptions(state.GetOptions(),true);}selected=null;message="Ring deleted. Save to keep this change.";}
            }
            if(message!="")GUILayout.Label(message);
        }
        private void Apply(RingworldFlight flight,bool create)
        {
            var state=RingworldScenario.Instance;
            if(flight.AtmosphereTransition||(flight.visuals!=null&&flight.visuals.PhotoActive)){message="Finish the transition/photo first.";return;}
            if(!create&&(state.Occupied(selected)||(flight.Active&&flight.Settings.RingId==selected))){message="Move or recover resident vessels before moving/changing this ring.";return;}
            double px,py,pz,di,wi;int parsed;
            if(!double.TryParse(x,NumberStyles.Float,CultureInfo.InvariantCulture,out px)||!double.TryParse(y,NumberStyles.Float,CultureInfo.InvariantCulture,out py)||!double.TryParse(z,NumberStyles.Float,CultureInfo.InvariantCulture,out pz)||!double.TryParse(diameter,NumberStyles.Float,CultureInfo.InvariantCulture,out di)||!double.TryParse(width,NumberStyles.Float,CultureInfo.InvariantCulture,out wi)||!RingParameters.Finite(px*1000)||!RingParameters.Finite(py*1000)||!RingParameters.Finite(pz*1000)||!RingParameters.Finite(di*500)||!RingParameters.Finite(wi*1000)||di<2000000||wi<10000||wi>di/2){message="Enter finite coordinates, diameter >= 2,000,000 km, and width from 10,000 km to the radius.";return;}
            if(string.IsNullOrWhiteSpace(seed))parsed=BitConverter.ToInt32(Guid.NewGuid().ToByteArray(),0);
            else if(!int.TryParse(seed,out parsed)){message="Seed must be a whole 32-bit number or blank.";return;}
            var n=state.RingOptions(selected).CreateCopy();string id=create?Guid.NewGuid().ToString("N"):selected;
            n.SetValue("ringId",id,true);n.SetValue("ringName",string.IsNullOrWhiteSpace(name)?"Ringworld":name.Trim(),true);
            n.SetValue("referenceBody",designated?reference:"Sun",true);n.SetValue("designatedStar",designated,true);
            n.SetValue("centerX",N(px*1000),true);n.SetValue("centerY",N(py*1000),true);n.SetValue("centerZ",N(pz*1000),true);
            n.SetValue("radius",N(di*500),true);n.SetValue("width",N(wi*1000),true);n.SetValue("seed",parsed,true);
            var candidate=Settings.Load();try{candidate.Apply(n);}catch(ArgumentException e){message=e.Message;return;}
            string invalid=RingSelection.Validate(candidate,create?null:selected);if(invalid!=null){message=invalid;return;}
            if(create)state.Rings.Add(n);else state.Rings[state.Rings.IndexOf(state.RingOptions(selected))]=n;
            if(!create&&state.ActiveRingId==selected)flight.ApplyOptions(n,true);
            Load(n);message=create?"Ring spawned. Use Visit to fly there; save to keep it.":"Ring updated. Save to keep it.";
        }
    }
}
