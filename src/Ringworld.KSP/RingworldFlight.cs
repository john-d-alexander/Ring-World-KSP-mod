using System;
using System.Collections;
using System.Collections.Generic;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    [KSPAddon(KSPAddon.Startup.Flight,false)]
    [DefaultExecutionOrder(10000)]
    public sealed class RingworldFlight : MonoBehaviour
    {
        public static RingworldFlight Instance;
        internal Settings Settings;
        internal CelestialBody Star;
        private SurfaceStreamer surface;
        private Rect window=new Rect(30,80,370,570);
        private Vector2 scroll;
        private Vector2 panelScroll;
        internal float arrivalHeight=300;
        private bool visible=true,transferring;
        private int destination;
        private string status="Launch a craft, then start a ring expedition.";
        private float nextCapture;
        private const string WarpLock="NivenRingworld.Warp";
        private RingworldScenario State { get { return RingworldScenario.Instance; } }
        internal bool Active
        {
            get
            {
                var v=FlightGlobals.ActiveVessel;
                return State!=null&&State.Expedition&&(transferring||(v!=null&&State.Vessels.ContainsKey(v.id.ToString())));
            }
        }
        internal bool Ready { get { return Active&&!transferring; } }
        internal bool Owns(Vessel v)
        {
            VesselRecord r;
            return Ready&&v!=null&&v.mainBody==Star&&State.Vessels.TryGetValue(v.id.ToString(),out r)&&r.Restored;
        }
        internal DVec Position(Vessel v){return ConvertVector.Core(v.GetWorldPos3D()-Star.position);}
        internal DVec Velocity(Vessel v){return ConvertVector.Core(v.obt_velocity);}
        public void Start()
        {
            Instance=this;
            try
            {
                Settings=Settings.Load();Star=FlightGlobals.Bodies.Find(b=>b.name=="Sun");
                if(Star==null)throw new InvalidOperationException("This release requires the stock Sun.");
                surface=new SurfaceStreamer(Settings);
                StockIntegration.Install();
                Debug.Log("[NivenRingworld] Flight controller ready; R="+Settings.Geometry.P.Radius);
            }
            catch(Exception e){Debug.LogException(e);status=e.Message;enabled=false;}
        }
        public void Update()
        {
            if(Input.GetKey(KeyCode.LeftAlt)&&Input.GetKeyDown(KeyCode.R))visible=!visible;
            AdoptNearbyActiveVessel();
            if(!Active||Settings==null){InputLockManager.RemoveControlLock(WarpLock);return;}
            InputLockManager.SetControlLock(ControlTypes.TIMEWARP,WarpLock);
            if(TimeWarp.CurrentRateIndex!=0)TimeWarp.SetRate(0,true);
            var v=FlightGlobals.ActiveVessel;
            if(v==null||transferring)return;
            VesselRecord record;
            if(State.Vessels.TryGetValue(v.id.ToString(),out record)&&!record.Restored)
            {StartCoroutine(Transfer(v,record.Position,record.Velocity,record.Rotation,false));return;}
            if(v.mainBody!=Star||v.packed||!State.Vessels.ContainsKey(v.id.ToString()))return;
            surface.Update(Position(v),Star.position);
            var coord=Settings.Geometry.Coordinates(Position(v));
            surface.Light(Settings.Geometry.Daylight(coord.Along,Planetarium.GetUniversalTime()));
            if(Time.realtimeSinceStartup>nextCapture){Capture();nextCapture=Time.realtimeSinceStartup+1;}
        }
        private void AdoptNearbyActiveVessel()
        {
            var v=FlightGlobals.ActiveVessel;
            if(State==null||!State.Expedition||v==null||v.mainBody!=Star||State.Vessels.ContainsKey(v.id.ToString()))return;
            foreach(var other in FlightGlobals.VesselsLoaded)
            {
                VesselRecord r;
                if(other==null||other==v||other.mainBody!=Star||!State.Vessels.TryGetValue(other.id.ToString(),out r)||!r.Restored)continue;
                if((v.GetWorldPos3D()-other.GetWorldPos3D()).magnitude>250)continue;
                string id=v.id.ToString();State.Vessels[id]=new VesselRecord{Id=id,Position=Position(v),Velocity=Velocity(v),Rotation=v.transform.rotation,Restored=true};return;
            }
        }
        public void FixedUpdate()
        {
            if(!Active||transferring||Star==null)return;
            var active=FlightGlobals.ActiveVessel;
            if(active==null||active.mainBody!=Star||active.packed)return;
            VesselRecord activeRecord;
            if(!State.Vessels.TryGetValue(active.id.ToString(),out activeRecord)||!activeRecord.Restored)return;
            surface.Reposition(Star.position);
            var bodies=new HashSet<Rigidbody>();
            foreach(var v in FlightGlobals.VesselsLoaded)
            {
                if(v==null||v.packed||v.mainBody!=Star)continue;
                string id=v.id.ToString();
                if(!State.Vessels.ContainsKey(id))
                {
                    // Pick up newly separated stages and EVAs close to the expedition.
                    if((v.GetWorldPos3D()-active.GetWorldPos3D()).magnitude>250)continue;
                    State.Vessels[id]=new VesselRecord{Id=id,Restored=true};
                }
                if(!State.Vessels[id].Restored)continue;
                DVec vesselPos=Position(v);
                foreach(var part in v.parts)
                {
                    Rigidbody rb=part.rb;
                    if(rb==null||rb.isKinematic||!bodies.Add(rb))continue;
                    DVec pos=vesselPos+ConvertVector.Core((Vector3d)rb.worldCenterOfMass-v.GetWorldPos3D());
                    DVec vel=ConvertVector.Core((Vector3d)rb.velocity+Krakensbane.GetFrameVelocity());
                    var coord=Settings.Geometry.Coordinates(pos);
                    // This acceleration belongs to the opted-in rotating frame, not to a spherical SOI.
                    DVec stock=ConvertVector.Core(FlightGlobals.getGeeForceAtPosition(rb.worldCenterOfMass,v.mainBody));
                    DVec acceleration=Settings.Geometry.Acceleration(pos,vel,Star.gravParameter)-stock;
                    if(Settings.Atmosphere && Math.Abs(coord.Across)<Settings.Geometry.P.Width/2)
                    {
                        var terrain=Settings.Terrain.Sample(coord.Along,coord.Across);
                        double density=Settings.Geometry.Density(coord.Altitude);
                        // Isotropic drag prototype; lift/engine oxygen require a FlightIntegrator adapter.
                        double drag=.00025*density,submersion=0;
                        if(terrain.Wet)
                        {
                            submersion=Math.Max(0,Math.Min(1,(terrain.WaterHeight-coord.Altitude+1)/2));
                            acceleration+=Settings.Geometry.Up(pos)*(Settings.Geometry.P.Gravity*2.5*submersion);
                            drag+=submersion*.7;
                        }
                        double speed=vel.Length;
                        if(speed>1e-6)acceleration-=vel*((1-Math.Exp(-drag*speed*Time.fixedDeltaTime))/Time.fixedDeltaTime);
                    }
                    rb.AddForce(ConvertVector.Unity(acceleration),ForceMode.Acceleration);
                }
            }
        }
        public void LateUpdate(){if(Active&&!transferring&&surface!=null&&Star!=null)surface.Reposition(Star.position);}
        internal void Capture()
        {
            if(!Active||transferring||Star==null)return;
            foreach(var v in FlightGlobals.VesselsLoaded)
            {
                VesselRecord r;if(v==null||v.packed||v.mainBody!=Star||!State.Vessels.TryGetValue(v.id.ToString(),out r)||!r.Restored)continue;
                r.Position=Position(v);r.Velocity=Velocity(v);r.Rotation=v.transform.rotation;
            }
        }
        internal void Visit()
        {
            var v=FlightGlobals.ActiveVessel;if(v==null||State==null||transferring)return;
            var l=Settings.Terrain.Landmarks[destination];
            // The site center is clear of buildings; allow time to establish a controlled descent.
            double along=l.Along,across=l.Across;
            var t=Settings.Terrain.Sample(along,across);
            double height=Math.Max(t.Height,double.IsNegativeInfinity(t.WaterHeight)?t.Height:t.WaterHeight)+arrivalHeight;
            DVec position=Settings.Geometry.Position(along,across,height);
            Quaternion rotation=Quaternion.FromToRotation(v.transform.up,ConvertVector.Unity(Settings.Geometry.Up(position)))*v.transform.rotation;
            StartCoroutine(Transfer(v,position,new DVec(),rotation,true));
        }
        private IEnumerator Transfer(Vessel v,DVec position,DVec velocity,Quaternion rotation,bool newVisit)
        {
            transferring=true;status="Preparing surface colliders...";
            TimeWarp.SetRate(0,true);
            if(newVisit)CaptureBeforeTransfer(v);
            State.Expedition=true;
            string id=v.id.ToString();
            State.Vessels[id]=new VesselRecord{Id=id,Position=position,Velocity=velocity,Rotation=rotation};
            // Use KSP's own orbit placement to handle SOI bookkeeping first.
            FlightGlobals.fetch.SetShipOrbit(Star.flightGlobalsIndex,0,Settings.Geometry.P.Radius,0,0,0,0,Planetarium.GetUniversalTime());
            yield return null;
            for(int i=0;i<120&&(v.packed||v.mainBody!=Star);i++)yield return null;
            if(v.packed||v.mainBody!=Star)
            {
                status="KSP did not finish switching to the Sun. Retry when unpacked.";transferring=false;yield break;
            }
            v.Landed=false;v.Splashed=false;v.landedAt="";
            // Shift first: putting a float Transform billions of metres away loses hundreds of metres.
            FloatingOrigin.SetOffset(Star.position+ConvertVector.Ksp(position));
            Krakensbane.ResetVelocityFrame(true);
            v.SetRotation(rotation,false);v.SetPosition(Star.position+ConvertVector.Ksp(position),true);
            v.SetWorldVelocity(ConvertVector.Ksp(velocity));
            v.DetachPatchedConicsSolver();
            v.IgnoreGForces(30);v.IgnoreSpeed(30);
            surface.Update(position,Star.position,true);
            Physics.SyncTransforms();
            State.Vessels[id].Restored=true;
            transferring=false;status="Ring frame active. Alt+R shows/hides this panel. Stock navball altitude is solar altitude.";
            FlightCamera.SetMode(FlightCamera.Modes.FREE);
            Debug.Log("[NivenRingworld] Expedition entered at "+Settings.Geometry.Coordinates(position).Along);
        }
        private void CaptureBeforeTransfer(Vessel v)
        {
            // New visits deliberately move only the current vessel; other saved expeditions remain frozen.
            if(!Active)return;
            foreach(var other in FlightGlobals.VesselsLoaded)
            {
                VesselRecord r;if(other==null||other==v||!State.Vessels.TryGetValue(other.id.ToString(),out r))continue;
                if(other.mainBody==Star&&!other.packed){r.Position=Position(other);r.Velocity=Velocity(other);r.Rotation=other.transform.rotation;r.Restored=false;}
            }
        }
        private void Leave()
        {
            if(transferring||State==null)return;
            Capture();
            // All currently loaded participants must leave together: they share a physics frame.
            foreach(var v in FlightGlobals.VesselsLoaded)
            {
                if(v==null||v.packed||v.mainBody!=Star||!State.Vessels.ContainsKey(v.id.ToString()))continue;
                v.ResetGroundContact();v.KillPermanentGroundContact();v.Landed=false;
                v.ChangeWorldVelocity(ConvertVector.Ksp(Settings.Geometry.SpinVelocity(Position(v))));
                v.AttachPatchedConicsSolver();
                State.Vessels.Remove(v.id.ToString());
            }
            State.Expedition=State.Vessels.Count>0;InputLockManager.RemoveControlLock(WarpLock);
            surface.Dispose();surface=new SurfaceStreamer(Settings);
            status="Returned to inertial flight with the ring's tangential velocity. This is far above solar escape speed.";
        }
        internal bool ScienceContext(Vessel v,out string key,out string title,out string report)
        {
            key=title=report="";
            if(!Active||v==null||v.mainBody!=Star||!State.Vessels.ContainsKey(v.id.ToString()))return false;
            var p=Settings.Geometry.Coordinates(Position(v));var t=Settings.Terrain.Sample(p.Along,p.Across);
            if(p.Altitude-t.Height>120||p.Altitude-t.Height< -5||Math.Abs(p.Across)>Settings.Geometry.P.Width/2)return false;
            double distance;var l=Settings.Terrain.Nearest(p.Along,p.Across,out distance);
            key=distance<Math.Min(l.Radius,1500)?"site_"+l.Id:"biome_"+t.Biome;
            title=distance<Math.Min(l.Radius,1500)?l.Name:t.Biome.ToString();
            report=distance<Math.Min(l.Radius,1500)?l.Description:"A sample of the artificial habitat's "+t.Biome.ToString().ToLowerInvariant()+" environment.";
            return true;
        }
        public void OnGUI()
        {
            if(!visible||Settings==null)return;
            window=GUILayout.Window(19700114,window,DrawWindow,"Niven Ringworld | Expedition prototype");
        }
        private void DrawWindow(int id)
        {
            panelScroll=GUILayout.BeginScrollView(panelScroll,GUILayout.Height(Mathf.Max(240,Mathf.Min(610,Screen.height-150))));
            GUILayout.Label("RINGWORLD  /  1:10 scale");
            GUILayout.Label("Radius "+(Settings.Geometry.P.Radius/1000).ToString("N0")+" km   |   Width "+(Settings.Geometry.P.Width/1000).ToString("N0")+" km");
            var v=FlightGlobals.ActiveVessel;
            if(Active&&v!=null&&v.mainBody==Star)
            {
                var p=Settings.Geometry.Coordinates(Position(v));var terrain=Settings.Terrain.Sample(p.Along,p.Across);
                GUILayout.Label("Above ground: "+(p.Altitude-terrain.Height).ToString("N1")+" m   |   "+terrain.Biome);
                GUILayout.Label("Ring speed: "+Velocity(v).Length.ToString("N1")+" m/s   |   g: "+Settings.Geometry.Acceleration(Position(v),new DVec(),Star.gravParameter).Length.ToString("F3"));
                GUILayout.Label("Spinward: "+(p.Along/1000).ToString("N1")+" km\nAcross: "+(p.Across/1000).ToString("N1")+" km   |   Tiles: "+surface.TileCount);
                GUILayout.Label("Local light: "+(100*Settings.Geometry.Daylight(p.Along,Planetarium.GetUniversalTime())).ToString("F0")+"%   |   Time warp held at 1x");
            }
            scroll=GUILayout.BeginScrollView(scroll,GUILayout.Height(190));
            for(int i=0;i<Settings.Terrain.Landmarks.Count;i++)
                if(GUILayout.Toggle(destination==i,Settings.Terrain.Landmarks[i].Name))destination=i;
            GUILayout.EndScrollView();
            GUILayout.Label(Settings.Terrain.Landmarks[destination].Description);
            GUILayout.Label("Arrival height: "+arrivalHeight.ToString("F0")+" m above ground / water");
            arrivalHeight=GUILayout.HorizontalSlider(arrivalHeight,60,2000);
            GUI.enabled=!transferring&&v!=null&&State!=null;
            if(GUILayout.Button(Active?"Relocate above selected site":"Begin expedition at selected site"))Visit();
            if(Active&&GUILayout.Button("Leave ring frame for spaceflight"))Leave();
            GUI.enabled=true;
            GUILayout.Label("Science: fit the RW-1 Surveyor, then use its part menu. Land with engines/legs; descent starts at rest.");
            GUILayout.Label(status);
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0,0,10000,25));
        }
        public void OnDestroy()
        {
            Capture();InputLockManager.RemoveControlLock(WarpLock);
            if(surface!=null)surface.Dispose();if(Instance==this)Instance=null;
        }
    }
}
