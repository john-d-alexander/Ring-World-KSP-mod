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
        private AtmosphereRenderer atmosphere;
        internal double FrameEpoch;
        private double arrivalCooldown;
        private Rect window=new Rect(30,80,370,570);
        private Vector2 scroll;
        private Vector2 panelScroll;
        internal float arrivalHeight=300;
        private bool visible=true,transferring;
        private int destination;
        private int panelTab;
        private readonly RingSettingsPanel settingsPanel=new RingSettingsPanel();
        private ConfigNode boundOptions;
        private string status="Fly toward the ring for automatic arrival, or use the expedition transport below.";
        private float nextCapture;
        private const string WarpLock="NivenRingworld.Warp";
        private RingworldScenario State { get { return RingworldScenario.Instance; } }
        internal bool Active
        {
            get
            {
                var v=FlightGlobals.ActiveVessel;
                return State!=null&&State.Expedition&&(transferring||FrameInUse||(v!=null&&State.Vessels.ContainsKey(v.id.ToString())));
            }
        }
        // A Unity physics scene has one velocity frame even while its active vessel
        // changes. Never infer that frame solely from the newly selected vessel ID.
        internal bool FrameInUse
        {
            get
            {
                if(State==null||Star==null)return false;
                foreach(var v in FlightGlobals.VesselsLoaded)
                {
                    VesselRecord r;
                    if(v!=null&&v.mainBody==Star&&State.Vessels.TryGetValue(v.id.ToString(),out r)&&r.Restored)return true;
                }
                return false;
            }
        }
        internal bool Ready
        {
            get
            {
                var v=FlightGlobals.ActiveVessel;VesselRecord r;
                return Active&&!transferring&&v!=null&&!v.packed&&v.mainBody==Star&&State.Vessels.TryGetValue(v.id.ToString(),out r)&&r.Restored;
            }
        }
        internal bool IsParticipant(Vessel v) { return State!=null&&v!=null&&v.mainBody==Star&&State.Vessels.ContainsKey(v.id.ToString()); }
        internal bool Owns(Vessel v)
        {
            VesselRecord r;
            return !transferring&&State!=null&&v!=null&&v.mainBody==Star&&State.Vessels.TryGetValue(v.id.ToString(),out r)&&r.Restored;
        }
        internal DVec Position(Vessel v)
        {
            if(v.packed)return ConvertVector.Core(v.GetWorldPos3D()-Star.position);
            double mass=0;DVec offset=new DVec();var seen=new HashSet<Rigidbody>();
            foreach(var part in v.parts)
            {
                var rb=part.rb;if(rb==null||rb.isKinematic||!seen.Add(rb))continue;
                offset+=ConvertVector.Core(rb.worldCenterOfMass-v.transform.position)*rb.mass;mass+=rb.mass;
            }
            return RootPosition(v)+(mass>0?offset/mass:new DVec());
        }
        private DVec RootPosition(Vessel v){return ConvertVector.Core((Vector3d)v.transform.position-Star.position);}
        internal DVec Velocity(Vessel v)
        {
            if(v.packed)return ConvertVector.Core(v.obt_velocity);
            double mass=0;DVec velocity=new DVec();var seen=new HashSet<Rigidbody>();
            foreach(var part in v.parts)
            {
                var rb=part.rb;if(rb==null||rb.isKinematic||!seen.Add(rb))continue;
                velocity+=ConvertVector.Core(rb.velocity)*rb.mass;mass+=rb.mass;
            }
            return mass>0?velocity/mass+ConvertVector.Core(Krakensbane.GetFrameVelocity()):ConvertVector.Core(v.obt_velocity);
        }
        public void Start()
        {
            Instance=this;
            try
            {
                Settings=Settings.Load();Star=FlightGlobals.Bodies.Find(b=>b.name=="Sun");
                if(Star==null)throw new InvalidOperationException("This release requires the stock Sun.");
                surface=new SurfaceStreamer(Settings);
                atmosphere=new AtmosphereRenderer(Settings);
                StockIntegration.Install();
                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                Debug.Log("[NivenRingworld] Flight controller ready; R="+Settings.Geometry.P.Radius);
            }
            catch(Exception e){Debug.LogException(e);status=e.Message;enabled=false;}
        }
        internal void ApplyOptions(ConfigNode options,bool rebuildWorld)
        {
            Settings.Apply(options);State.Options=options;boundOptions=options;settingsPanel.Reset();
            if(rebuildWorld){surface.Dispose();surface=new SurfaceStreamer(Settings);}
            else surface.RebuildLod();
            atmosphere.Dispose();atmosphere=new AtmosphereRenderer(Settings);
        }
        public void Update()
        {
            if(State!=null&&Settings!=null&&boundOptions!=State.GetOptions())ApplyOptions(State.GetOptions(),true);
            if(Input.GetKey(KeyCode.LeftAlt)&&Input.GetKeyDown(KeyCode.R))visible=!visible;
            AdoptNearbyActiveVessel();
            if(Settings!=null&&Star!=null&&!Active) Settings.Geometry.OrientationRadians=Settings.Geometry.P.Omega*Planetarium.GetUniversalTime();
            if(!Active||Settings==null){
                PrepareArrival();
                return;
            }
            InputLockManager.SetControlLock(ControlTypes.TIMEWARP,WarpLock);
            if(TimeWarp.CurrentRateIndex!=0)TimeWarp.SetRate(0,true);
            var v=FlightGlobals.ActiveVessel;
            if(v==null||transferring)return;
            VesselRecord record;
            if(State.Vessels.TryGetValue(v.id.ToString(),out record)&&!record.Restored)
            {SetFrameEpoch(record.Epoch);StartCoroutine(Transfer(v,record.Position,record.Velocity,record.Rotation,false));return;}
            if(v.mainBody!=Star||v.packed||!State.Vessels.ContainsKey(v.id.ToString()))return;
            surface.Update(Position(v),Star.position);
            var coord=Settings.Geometry.Coordinates(Position(v));
            surface.Light(Settings.Geometry.Daylight(coord.Along,Planetarium.GetUniversalTime()));
            if(Time.realtimeSinceStartup>nextCapture){Capture();nextCapture=Time.realtimeSinceStartup+1;}
        }
        private void OnCrewOnEva(GameEvents.FromToAction<Part,Part> ev)
        {
            if(ev.from==null||ev.to==null||!Owns(ev.from.vessel))return;
            RegisterParticipant(ev.to.vessel);
        }
        private void RegisterParticipant(Vessel v)
        {
            if(v==null||State==null)return;
            string id=v.id.ToString();if(State.Vessels.ContainsKey(id))return;
            State.Vessels[id]=new VesselRecord{Id=id,Position=RootPosition(v),Velocity=Velocity(v),Rotation=v.transform.rotation,Restored=true,Epoch=FrameEpoch};
            Debug.Log("[NivenRingworld] Inherited rotating frame: "+v.vesselName);
        }
        internal bool AdoptParticipant(Vessel v)
        {
            if(v==null||State==null||!State.Expedition||v.mainBody!=Star)return false;
            if(Owns(v))return true;
            if(State.Vessels.ContainsKey(v.id.ToString()))return false;
            foreach(var other in FlightGlobals.VesselsLoaded)
            {
                if(other==null||other==v||!Owns(other))continue;
                if((v.transform.position-other.transform.position).sqrMagnitude>250*250)continue;
                RegisterParticipant(v);return true;
            }
            return false;
        }
        private void AdoptNearbyActiveVessel(){AdoptParticipant(FlightGlobals.ActiveVessel);}
        public void FixedUpdate()
        {
            if(Settings==null||Star==null)return;
            AdoptNearbyActiveVessel();
            TryArrival();
            if(!Active||transferring)return;
            var active=FlightGlobals.ActiveVessel;
            if(active==null||active.mainBody!=Star||active.packed)return;
            VesselRecord activeRecord;
            if(!State.Vessels.TryGetValue(active.id.ToString(),out activeRecord)||!activeRecord.Restored)return;
            if(!Settings.Geometry.InArrivalRegion(Position(active),true)){Leave();return;}
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
                    RegisterParticipant(v);
                }
                if(!State.Vessels[id].Restored)continue;
                DVec vesselPos=Position(v);
                foreach(var part in v.parts)
                {
                    Rigidbody rb=part.rb;
                    if(rb==null||rb.isKinematic||!bodies.Add(rb))continue;
                    DVec pos=RootPosition(v)+ConvertVector.Core(rb.worldCenterOfMass-v.transform.position);
                    DVec vel=ConvertVector.Core((Vector3d)rb.velocity+Krakensbane.GetFrameVelocity());
                    var coord=Settings.Geometry.Coordinates(pos);
                    // This acceleration belongs to the opted-in rotating frame, not to a spherical SOI.
                    DVec stock=ConvertVector.Core(FlightGlobals.getGeeForceAtPosition(rb.worldCenterOfMass,v.mainBody));
                    DVec acceleration=Settings.Geometry.Acceleration(pos,vel,Star.gravParameter)-stock;
                    if(Settings.Atmosphere && Math.Abs(coord.Across)<Settings.Geometry.P.Width/2)
                    {
                        var terrain=Settings.Terrain.Sample(coord.Along,coord.Across);
                        // FlightIntegrator supplies aerodynamic drag. This term is water damping only.
                        double drag=0,submersion=0;
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
        public void LateUpdate()
        {
            if(Settings==null||Star==null)return;
            if(Active&&!transferring&&surface!=null)surface.Reposition(Star.position);
            var v=FlightGlobals.ActiveVessel;
            if(Owns(v)&&FlightCamera.fetch!=null&&!MapView.MapIsEnabled)
            {
                var camera=FlightCamera.fetch.mainCamera;
                if(camera!=null)
                {
                    var target=(Vector3)(Star.position+ConvertVector.Ksp(Position(v)));
                    float clipRadius=Mathf.Max(.6f,camera.nearClipPlane*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad)*Mathf.Sqrt(1+camera.aspect*camera.aspect)+.2f);
                    RingCameraTerrainPatch.ApplyClearance(FlightCamera.fetch,ConstrainCamera(camera.transform.position,target,clipRadius)-camera.transform.position);
                }
            }
            if(atmosphere!=null)atmosphere.Update(v!=null&&v.mainBody==Star&&!MapView.MapIsEnabled,Star.position);
        }
        internal Vector3 ConstrainCamera(Vector3 desired,Vector3 target,float clearance)
        {
            var delta=desired-target;RaycastHit hit;
            if(delta.magnitude>clearance&&Physics.SphereCast(target,clearance,delta.normalized,out hit,delta.magnitude,1<<15,QueryTriggerInteraction.Ignore))
                desired=target+delta.normalized*Mathf.Max(clearance,hit.distance-.2f);
            var p=Settings.Geometry.Coordinates(ConvertVector.Core((Vector3d)desired-Star.position));
            if(Math.Abs(p.Across)<=Settings.Geometry.P.Width/2)
            {
                double floor=surface.CameraFloor(p.Along,p.Across)+clearance;
                // A cast cannot find a starting overlap, or a one-sided face behind it.
                // This final constraint also protects the near clipping plane.
                if(p.Altitude<floor)desired=(Vector3)(Star.position+ConvertVector.Ksp(Settings.Geometry.Position(p.Along,p.Across,floor)));
            }
            return desired;
        }
        internal int LodCount {get{return surface==null?0:surface.LodCount;}}
        internal int LodPending {get{return surface==null?0:surface.LodPending;}}
        internal int ScaledLodCount {get{return surface==null?0:surface.ScaledLodCount;}}
        internal double SurfaceClearance(Vessel v)
        {
            var p=Settings.Geometry.Coordinates(Position(v));
            return Math.Max(0,p.Altitude-surface.CameraFloor(p.Along,p.Across));
        }
        internal void Capture()
        {
            if(!Active||transferring||Star==null)return;
            foreach(var v in FlightGlobals.VesselsLoaded)
            {
                VesselRecord r;if(v==null||v.packed||v.mainBody!=Star||!State.Vessels.TryGetValue(v.id.ToString(),out r)||!r.Restored)continue;
                r.Position=RootPosition(v);r.Velocity=Velocity(v);r.Rotation=v.transform.rotation;r.Epoch=FrameEpoch;
            }
        }
        internal void Visit()
        {
            var v=FlightGlobals.ActiveVessel;if(v==null||State==null||transferring)return;
            Capture();
            SetFrameEpoch(Planetarium.GetUniversalTime());
            var l=Settings.Terrain.Landmarks[destination];
            // The site center is clear of buildings; allow time to establish a controlled descent.
            double along=l.Along,across=l.Across;
            var t=Settings.Terrain.Sample(along,across);
            double height=Math.Max(t.Height,double.IsNegativeInfinity(t.WaterHeight)?t.Height:t.WaterHeight)+arrivalHeight;
            DVec position=Settings.Geometry.Position(along,across,height);
            Quaternion rotation=Quaternion.FromToRotation(v.transform.up,ConvertVector.Unity(Settings.Geometry.Up(position)))*v.transform.rotation;
            StartCoroutine(Transfer(v,position,new DVec(),rotation,true));
        }
        private IEnumerator TrainingApproach()
        {
            float previous=arrivalHeight;arrivalHeight=250000;Visit();arrivalHeight=previous;
            while(transferring)yield return null;
            var v=FlightGlobals.ActiveVessel;if(!Owns(v))yield break;
            v.SetWorldVelocity(ConvertVector.Ksp(Settings.Geometry.Up(Position(v))*-1000));
            Leave();status="Training setup: spin-matched craft descending at 1 km/s. Automatic handoff occurs near 210 km; air begins at 60 km.";
        }
        private IEnumerator Transfer(Vessel v,DVec position,DVec velocity,Quaternion rotation,bool newVisit)
        {
            transferring=true;status="Preparing surface colliders...";
            TimeWarp.SetRate(0,true);
            if(newVisit)CaptureBeforeTransfer(v);
            State.Expedition=true;
            string id=v.id.ToString();
            State.Vessels[id]=new VesselRecord{Id=id,Position=position,Velocity=velocity,Rotation=rotation,Epoch=FrameEpoch};
            // Use KSP's own orbit placement to handle SOI bookkeeping first.
            FlightGlobals.fetch.SetShipOrbit(Star.flightGlobalsIndex,0,Settings.Geometry.P.Radius,0,0,0,0,Planetarium.GetUniversalTime());
            yield return null;
            double deadline=Time.realtimeSinceStartup+45;
            while((v.packed||v.mainBody!=Star)&&Time.realtimeSinceStartup<deadline)yield return null;
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
                if(other.mainBody==Star&&!other.packed){r.Position=RootPosition(other);r.Velocity=Velocity(other);r.Rotation=other.transform.rotation;r.Restored=false;}
            }
        }
        private void SetFrameEpoch(double epoch)
        {
            FrameEpoch=epoch;Settings.Geometry.OrientationRadians=Settings.Geometry.P.Omega*epoch;
        }
        private void PrepareArrival()
        {
            var v=FlightGlobals.ActiveVessel;
            if(v==null||State==null||v.mainBody!=Star){InputLockManager.RemoveControlLock(WarpLock);return;}
            var g=Settings.Geometry;var pos=Position(v);
            var coord=g.Coordinates(pos);
            if(coord.Altitude>-1000&&coord.Altitude<600000&&Math.Abs(coord.Across)<g.P.Width/2+500000)
            {
                surface.Update(pos,Star.position);surface.Light(g.Daylight(coord.Along,Planetarium.GetUniversalTime()));
            }
            // Drop warp before the boundary, including high speed radial approaches.
            double lead=Math.Max(10,TimeWarp.CurrentRate*Time.fixedDeltaTime*4);
            double eta=g.TimeToArrival(pos,Velocity(v),lead);
            if(eta<=lead)
            {
                InputLockManager.SetControlLock(ControlTypes.TIMEWARP,WarpLock);
                if(TimeWarp.CurrentRateIndex!=0)TimeWarp.SetRate(0,true);
                status="Approaching ring: matching the rotating atmosphere requires "+(g.SpinVelocity(pos).Length/1000).ToString("F1")+" km/s tangential velocity.";
            }
            else InputLockManager.RemoveControlLock(WarpLock);
        }
        internal void TryArrival()
        {
            var v=FlightGlobals.ActiveVessel;
            if(FrameInUse||Active||transferring||State==null||v==null||v.packed||v.mainBody!=Star||Planetarium.GetUniversalTime()<arrivalCooldown)return;
            if(!Settings.Geometry.InArrivalRegion(Position(v),false))return;
            SetFrameEpoch(Planetarium.GetUniversalTime());
            State.Expedition=true;
            ChangeFrame(false);
            surface.Update(Position(v),Star.position,true);Physics.SyncTransforms();
            status="Automatic ring arrival. Velocity relative to the rotating atmosphere preserved.";
            Debug.Log("[NivenRingworld] Automatic arrival: air-relative speed="+Velocity(v).Length+" epoch="+FrameEpoch);
        }
        // One chart change for every loaded vessel: there cannot be two different
        // coordinate frames in the same Unity physics scene. Preserve rigidbody spin
        // and per-part velocities, including flexible craft, rather than braking them.
        private void ChangeFrame(bool leaving)
        {
            var g=Settings.Geometry;double elapsed=leaving?Planetarium.GetUniversalTime()-FrameEpoch:0;
            double angle=g.P.Omega*elapsed;
            Quaternion q=Quaternion.AngleAxis((float)(angle*180/Math.PI),Vector3.up);
            var snapshots=new List<FrameVessel>();
            foreach(var v in FlightGlobals.VesselsLoaded)
            {
                if(v==null||v.packed||v.mainBody!=Star)continue;
                if(leaving&&!State.Vessels.ContainsKey(v.id.ToString()))continue;
                snapshots.Add(new FrameVessel(v,this,leaving,elapsed,q));
            }
            Krakensbane.ResetVelocityFrame(true);
            var active=FlightGlobals.ActiveVessel;
            foreach(var s in snapshots)if(s.Vessel==active)
                FloatingOrigin.SetOffset(Star.position+ConvertVector.Ksp(s.Position));
            foreach(var s in snapshots)
            {
                var v=s.Vessel;
                v.SetRotation(s.Rotation,false);v.SetPosition(Star.position+ConvertVector.Ksp(s.Position),true);
                v.SetWorldVelocity(ConvertVector.Ksp(s.Velocity));
                foreach(var body in s.Bodies)
                {
                    body.Body.position=(Vector3)(Star.position+ConvertVector.Ksp(s.Position))+body.Offset;
                    body.Body.rotation=body.Rotation;body.Body.velocity=body.Velocity;body.Body.angularVelocity=body.Angular;
                }
                v.ResetGroundContact();v.KillPermanentGroundContact();v.Landed=false;
                if(leaving)
                {
                    v.orbit.UpdateFromStateVectors(ConvertVector.Orbit(ConvertVector.Ksp(g.ToInertialPosition(s.OriginalCOM,elapsed))),ConvertVector.Orbit(ConvertVector.Ksp(s.Velocity)),Star,Planetarium.GetUniversalTime());
                    v.AttachPatchedConicsSolver();State.Vessels.Remove(v.id.ToString());
                }
                else
                {
                    v.DetachPatchedConicsSolver();string id=v.id.ToString();
                    State.Vessels[id]=new VesselRecord{Id=id,Position=s.Position,Velocity=s.Velocity,Rotation=s.Rotation,Restored=true,Epoch=FrameEpoch};
                }
            }
            if(leaving&&FlightCamera.fetch!=null)
                FlightCamera.fetch.transform.rotation=q*FlightCamera.fetch.transform.rotation;
            Physics.SyncTransforms();
        }
        private sealed class FrameBody
        {
            internal Rigidbody Body;internal Vector3 Offset,Velocity,Angular;internal Quaternion Rotation;
        }
        private sealed class FrameVessel
        {
            internal Vessel Vessel;internal DVec Position,Velocity,OriginalCOM;internal Quaternion Rotation;
            internal readonly List<FrameBody> Bodies=new List<FrameBody>();
            internal FrameVessel(Vessel v,RingworldFlight f,bool leaving,double elapsed,Quaternion q)
            {
                Vessel=v;var g=f.Settings.Geometry;OriginalCOM=f.Position(v);
                var root=f.RootPosition(v);Position=leaving?g.ToInertialPosition(root,elapsed):root;
                Velocity=leaving?g.ToInertialVelocity(OriginalCOM,f.Velocity(v),elapsed):g.RotatingVelocity(OriginalCOM,f.Velocity(v));
                Rotation=leaving?q*v.transform.rotation:v.transform.rotation;
                var seen=new HashSet<Rigidbody>();
                foreach(var part in v.parts)
                {
                    var rb=part.rb;if(rb==null||rb.isKinematic||!seen.Add(rb))continue;
                    var p=root+ConvertVector.Core(rb.worldCenterOfMass-v.transform.position);
                    var velocity=ConvertVector.Core((Vector3d)rb.velocity+Krakensbane.GetFrameVelocity());
                    var offset=rb.position-v.transform.position;
                    Bodies.Add(new FrameBody{Body=rb,Offset=leaving?q*offset:offset,Rotation=leaving?q*rb.rotation:rb.rotation,
                        Velocity=ConvertVector.Unity(leaving?g.ToInertialVelocity(p,velocity,elapsed):g.RotatingVelocity(p,velocity)),
                        Angular=leaving?q*(rb.angularVelocity+Vector3.up*(float)g.P.Omega):rb.angularVelocity-Vector3.up*(float)g.P.Omega});
                }
            }
        }
        internal void Leave()
        {
            if(transferring||State==null||!Ready)return;
            Capture();ChangeFrame(true);
            State.Expedition=State.Vessels.Count>0;InputLockManager.RemoveControlLock(WarpLock);
            surface.Dispose();surface=new SurfaceStreamer(Settings);
            arrivalCooldown=Planetarium.GetUniversalTime()+2;
            Settings.Geometry.OrientationRadians=Settings.Geometry.P.Omega*Planetarium.GetUniversalTime();
            status="Inertial solar flight restored with the ring's rotation and elapsed phase.";
            Debug.Log("[NivenRingworld] Automatic departure: inertial frame restored.");
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
            window=GUILayout.Window(19700114,window,DrawWindow,"Niven Ringworld | Orbital arrival");
        }
        private void DrawWindow(int id)
        {
            panelScroll=GUILayout.BeginScrollView(panelScroll,GUILayout.Height(Mathf.Max(240,Mathf.Min(610,Screen.height-150))));
            panelTab=GUILayout.Toolbar(panelTab,new[]{"Expedition","Settings"});
            if(panelTab==1){settingsPanel.Draw(this);GUILayout.EndScrollView();GUI.DragWindow(new Rect(0,0,10000,25));return;}
            GUILayout.Label("RINGWORLD  /  1:10 scale");
            GUILayout.Label("Radius "+(Settings.Geometry.P.Radius/1000).ToString("N0")+" km   |   Width "+(Settings.Geometry.P.Width/1000).ToString("N0")+" km");
            var v=FlightGlobals.ActiveVessel;
            if(Active&&v!=null&&v.mainBody==Star)
            {
                var p=Settings.Geometry.Coordinates(Position(v));var terrain=Settings.Terrain.Sample(p.Along,p.Across);
                GUILayout.Label("Above ground: "+(p.Altitude-terrain.Height).ToString("N1")+" m   |   "+terrain.Biome);
                GUILayout.Label("Ring speed: "+Velocity(v).Length.ToString("N1")+" m/s   |   g: "+Settings.Geometry.Acceleration(Position(v),new DVec(),Star.gravParameter).Length.ToString("F3"));
                GUILayout.Label("Spinward: "+(p.Along/1000).ToString("N1")+" km\nAcross: "+(p.Across/1000).ToString("N1")+" km   |   Tiles: "+surface.TileCount+" | LOD: "+surface.LodCount+" (queued "+surface.LodPending+")");
                GUILayout.Label("Air: "+v.atmDensity.ToString("F4")+" kg/m³  |  "+v.staticPressurekPa.ToString("F2")+" kPa  |  Mach "+v.mach.ToString("F2"));
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
            if(GUILayout.Button("Training: set up a spin-matched approach"))StartCoroutine(TrainingApproach());
            if(Active&&GUILayout.Button("Leave ring frame for spaceflight"))Leave();
            GUI.enabled=true;
            GUILayout.Label("Science: fit the RW-1 Surveyor, then use its part menu. Land with engines/legs; descent starts at rest.");
            GUILayout.Label(status);
            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0,0,10000,25));
        }
        public void OnDestroy()
        {
            if(FlightGlobals.fetch!=null)Capture();GameEvents.onCrewOnEva.Remove(OnCrewOnEva);InputLockManager.RemoveControlLock(WarpLock);
            if(surface!=null)surface.Dispose();if(atmosphere!=null)atmosphere.Dispose();if(Instance==this)Instance=null;
        }
    }
}
