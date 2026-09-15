using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Ringworld.Core;
using CoastState = Ringworld.Core.FlightState;
using UnityEngine;
namespace NivenRingworld
{
    internal sealed class RingTrajectory : MonoBehaviour
    {
        private LineRenderer line;private Material material;private bool computing;
        private float nextPrediction;private Guid vesselId;private CoastState packedState;private double packedEpoch=double.NaN;
        internal string Status="Vacuum coast prediction";
        internal int PointCount {get{return line==null?0:line.positionCount;}}
        internal static DVec InertialAcceleration(DVec p,Settings s,double mu)
        {double r=p.Length;return p*(-mu/(r*r*r))+RibbonGravity.Acceleration(p,s.Geometry.P,s.Geometry.P.SurfaceDensity);}
        internal void PackedUpdate(RingworldFlight f)
        {
            var v=FlightGlobals.ActiveVessel;double now=Planetarium.GetUniversalTime();
            if(v==null||!v.packed||f.Active||v.mainBody!=f.Star){packedEpoch=double.NaN;return;}
            if(double.IsNaN(packedEpoch)||v.id!=vesselId||now<packedEpoch)
            {
                packedState=new CoastState(ConvertVector.Core(ConvertVector.Orbit(v.orbit.getRelativePositionAtUT(now))),ConvertVector.Core(ConvertVector.Orbit(v.orbit.getOrbitalVelocityAtUT(now))));
                vesselId=v.id;packedEpoch=now;return;
            }
            double remaining=now-packedEpoch;
            if(remaining<=0)return;
            // Bound CPU work at extreme stock warp rates; do not silently discard time.
            if(remaining>86400){TimeWarp.SetRate(0,true);packedEpoch=double.NaN;return;}
            while(remaining>1e-6)
            {
                double dt=Math.Min(300,remaining);
                var c=f.Settings.Geometry.Coordinates(packedState.Position);
                double distance=Math.Max(100,Math.Abs(c.Altitude));
                double radialSpeed=Math.Abs((packedState.Position.X*packedState.Velocity.X+packedState.Position.Z*packedState.Velocity.Z)/Math.Max(1,Math.Sqrt(packedState.Position.X*packedState.Position.X+packedState.Position.Z*packedState.Position.Z)));
                dt=Math.Min(dt,Math.Max(.01,distance/(radialSpeed+Math.Abs(packedState.Velocity.Y)+1)*.1));
                dt=Math.Min(dt,Math.Max(.01,Math.Sqrt(distance/Math.Max(1,packedState.Velocity.Length*packedState.Velocity.Length/packedState.Position.Length))*.1));
                packedState=NumericalFlight.Step(packedState,dt,(p,u)=>InertialAcceleration(p,f.Settings,f.Star.gravParameter));remaining-=dt;
                if(f.Settings.Geometry.InArrivalRegion(packedState.Position,false))TimeWarp.SetRate(0,true);
            }
            packedEpoch=now;
            v.orbit.UpdateFromStateVectors(ConvertVector.Orbit(ConvertVector.Ksp(packedState.Position)),ConvertVector.Orbit(ConvertVector.Ksp(packedState.Velocity)),f.Star,now);
        }
        public void Update()
        {
            var f=RingworldFlight.Instance;var v=FlightGlobals.ActiveVessel;
            bool show=f!=null&&f.Settings!=null&&f.Settings.ShowTrajectory&&v!=null&&v.mainBody==f.Star&&MapView.MapIsEnabled;
            if(line!=null)line.enabled=false; // map overlay below remains visible through the ribbon
            if(!show)return;
            if(line==null)
            {
                var shader=Shader.Find("Unlit/Color")??Shader.Find("KSP/Unlit");if(shader==null)return;
                material=new Material(shader){color=new Color(.2f,.9f,1f)};
                var obj=new GameObject("Ringworld numerical coast trajectory");obj.transform.SetParent(transform,false);obj.layer=10;
                line=obj.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=0;
            }
            if(PlanetariumCamera.Camera!=null&&line.positionCount>0)
            {
                float distance=Vector3.Distance(PlanetariumCamera.Camera.transform.position,line.GetPosition(0));
                line.widthMultiplier=Mathf.Max(.002f,distance*.001f);
            }
            if(!computing&&Time.realtimeSinceStartup>=nextPrediction){nextPrediction=Time.realtimeSinceStartup+2;StartCoroutine(Predict(f,v));}
        }
        public void OnGUI()
        {
            var f=RingworldFlight.Instance;var v=FlightGlobals.ActiveVessel;
            if(Event.current.type!=EventType.Repaint||!MapView.MapIsEnabled||f==null||f.Settings==null||!f.Settings.ShowTrajectory||v==null||v.mainBody!=f.Star||line==null||line.positionCount<2||PlanetariumCamera.Camera==null)return;
            var color=GUI.color;var matrix=GUI.matrix;int depth=GUI.depth;
            GUI.depth=100;GUI.color=new Color(.15f,.95f,1f,.95f);
            try
            {
                var previous=PlanetariumCamera.Camera.WorldToScreenPoint(line.GetPosition(0));
                for(int i=1;i<line.positionCount;i++)
                {
                    var next=PlanetariumCamera.Camera.WorldToScreenPoint(line.GetPosition(i));
                    if(previous.z>0&&next.z>0)
                    {
                        var a=new Vector2(previous.x,Screen.height-previous.y);var b=new Vector2(next.x,Screen.height-next.y);
                        if(!((a.x<0&&b.x<0)||(a.x>Screen.width&&b.x>Screen.width)||(a.y<0&&b.y<0)||(a.y>Screen.height&&b.y>Screen.height)))
                        {
                            float length=(b-a).magnitude;
                            if(length>.1f&&length<Screen.width*100)
                            {
                                GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);
                                GUI.DrawTexture(new Rect(a.x,a.y-1,length,2),Texture2D.whiteTexture);GUI.matrix=matrix;
                            }
                        }
                    }
                    previous=next;
                }
            }
            finally{GUI.color=color;GUI.matrix=matrix;GUI.depth=depth;}
        }
        private IEnumerator Predict(RingworldFlight f,Vessel v)
        {
            computing=true;
            try
            {
                var settings=f.Settings;var g=settings.Geometry;bool rotating=f.Owns(v);double start=Planetarium.GetUniversalTime();
                double elapsed=rotating?start-f.FrameEpoch:0;
                DVec position=f.Position(v),velocity=f.Velocity(v);
                if(rotating){velocity=g.ToInertialVelocity(position,velocity,elapsed);position=g.ToInertialPosition(position,elapsed);}
                var state=new CoastState(position,velocity);var points=new List<Vector3>();double time=0;
                string result="Numerical vacuum coast: Sun + uniform ribbon; no thrust or manoeuvres";
                for(int i=0;i<4096&&time<=settings.PredictionSeconds;i++)
                {
                    if(v==null||v!=FlightGlobals.ActiveVessel||settings!=f.Settings||rotating!=f.Owns(v))yield break;
                    var coord=g.Coordinates(state.Position);
                    if(Math.Abs(coord.Across)<g.P.Width/2&&coord.Altitude>=-1300&&coord.Altitude<g.P.AtmosphereHeight+1&&settings.Atmosphere)
                    {result=time==0?"In atmosphere: no reliable vacuum trajectory; aerodynamic prediction pending":"Coast ends at atmosphere entry (+"+time.ToString("F1")+" s)";break;}
                    if(state.Position.Length<f.Star.Radius){result="Coast ends at the Sun";break;}
                    double materialAlong=RingGeometry.Wrap(coord.Along-(g.P.Omega*(start+time)-g.OrientationRadians)*g.P.Radius,g.P.Circumference);
                    if(Math.Abs(coord.Across)<g.P.Width/2&&coord.Altitude>=settings.UndersideAltitude&&coord.Altitude<=settings.Terrain.Sample(materialAlong,coord.Across).Height)
                    {result="Coast ends at terrain contact";break;}
                    if(Math.Abs(Math.Abs(coord.Across)-g.P.Width/2)<2&&coord.Altitude>=settings.UndersideAltitude&&coord.Altitude<=g.P.WallHeight)
                    {result="Coast ends at rim wall";break;}
                    // The map uses the same frozen rotating chart as the local scene.
                    var display=rotating?RingGeometry.Rotate(state.Position,-g.P.Omega*(elapsed+time)):state.Position;
                    points.Add((Vector3)ScaledSpace.LocalToScaledSpace(f.Star.position+ConvertVector.Ksp(display)));
                    double gap=Math.Max(1,Math.Abs(coord.Altitude-g.P.AtmosphereHeight));
                    double radial=Math.Abs((state.Position.X*state.Velocity.X+state.Position.Z*state.Velocity.Z)/Math.Max(1,Math.Sqrt(state.Position.X*state.Position.X+state.Position.Z*state.Position.Z)));
                    double dt=Math.Min(120,Math.Max(.01,Math.Min(gap/(radial+1)*.2,Math.Sqrt(gap/(state.Velocity.Length*state.Velocity.Length/state.Position.Length+1))*.2)));
                    if(coord.Altitude<g.P.WallHeight&&Math.Abs(state.Velocity.Y)>1)dt=Math.Min(dt,Math.Max(.001,Math.Abs(Math.Abs(coord.Across)-g.P.Width/2)/Math.Abs(state.Velocity.Y)*.2));
                    dt=Math.Min(dt,settings.PredictionSeconds-time);if(dt<=0)break;
                    state=NumericalFlight.Step(state,dt,(p,u)=>InertialAcceleration(p,settings,f.Star.gravParameter));time+=dt;
                    if(i%64==63)yield return null;
                }
                if(time<settings.PredictionSeconds&&points.Count>=4096)result="Coast truncated at numerical step budget";
                Status=result;line.positionCount=points.Count;line.SetPositions(points.ToArray());
            }
            finally{computing=false;}
        }
        public void OnDestroy(){if(material!=null)Destroy(material);}
    }
    [HarmonyPatch(typeof(OrbitRendererBase),"DrawSpline")]
    internal static class RingOrbitSplinePatch
    {
        private static readonly System.Reflection.MethodInfo GetOrbit=AccessTools.PropertyGetter(typeof(OrbitRendererBase),"orbit");
        private static bool Prefix(OrbitRendererBase __instance)
        {
            var f=RingworldFlight.Instance;var v=FlightGlobals.ActiveVessel;
            return f==null||f.Settings==null||!f.Settings.ShowTrajectory||v==null||v.mainBody!=f.Star||GetOrbit==null||!ReferenceEquals(GetOrbit.Invoke(__instance,null),v.orbit);
        }
    }
}
