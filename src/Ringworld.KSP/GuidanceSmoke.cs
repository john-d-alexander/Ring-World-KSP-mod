#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
namespace NivenRingworld
{
    internal static class GuidanceSmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            var v=FlightGlobals.ActiveVessel;f.arrivalHeight=3000;f.Visit();while(!f.Ready)yield return null;
            var up=ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(v)));
            v.SetWorldVelocity(Vector3.up*30-up*20);yield return new WaitForFixedUpdate();
            var verticalGauge=UnityEngine.Object.FindObjectOfType<KSP.UI.Screens.Flight.VerticalSpeedGauge>();
            if(verticalGauge==null){fail("Stock vertical-speed gauge missing");yield break;}
            AccessTools.Method(typeof(KSP.UI.Screens.Flight.VerticalSpeedGauge),"LateUpdate").Invoke(verticalGauge,null);
            double displayed=Convert.ToDouble(AccessTools.Field(typeof(KSP.UI.Screens.RotationalGauge),"targetValue").GetValue(verticalGauge.gauge));
            double expectedVertical=Vector3.Dot(ConvertVector.Unity(f.Velocity(v)),up);
            // Compare through the native gauge API: its stored target uses a logarithmic scale.
            verticalGauge.gauge.SetValue(expectedVertical);double expectedDial=verticalGauge.gauge.Value;
            Debug.Log("[RingworldSmoke] VERTICAL GAUGE target="+displayed+" expectedDial="+expectedDial+" ringMps="+expectedVertical+" stock="+v.verticalSpeed+" sinkLED="+verticalGauge.sinkRateLED.IsOn);
            if(Math.Abs(displayed-expectedDial)>.002||!verticalGauge.sinkRateLED.IsOn){fail("Vertical gauge or sink-rate LED not ring-relative");yield break;}
            foreach(var display in new[]{FlightGlobals.SpeedDisplayModes.Orbit,FlightGlobals.SpeedDisplayModes.Surface})
            foreach(var mode in new[]{VesselAutopilot.AutopilotMode.Prograde,VesselAutopilot.AutopilotMode.Retrograde,VesselAutopilot.AutopilotMode.RadialOut,VesselAutopilot.AutopilotMode.RadialIn,VesselAutopilot.AutopilotMode.Normal,VesselAutopilot.AutopilotMode.Antinormal})
            {
                FlightGlobals.SetSpeedMode(display);AccessTools.Field(typeof(VesselAutopilot),"mode").SetValue(v.Autopilot,mode);
                AccessTools.Method(typeof(VesselAutopilot),"SetAutopilot").Invoke(v.Autopilot,new object[]{true});
                var velocity=ConvertVector.Unity(f.Velocity(v));Vector3 expected;
                switch(mode)
                {
                    case VesselAutopilot.AutopilotMode.Prograde:expected=velocity;break;
                    case VesselAutopilot.AutopilotMode.Retrograde:expected=-velocity;break;
                    case VesselAutopilot.AutopilotMode.RadialOut:expected=up;break;
                    case VesselAutopilot.AutopilotMode.RadialIn:expected=-up;break;
                    case VesselAutopilot.AutopilotMode.Normal:expected=Vector3.Cross(up,velocity);break;
                    default:expected=-Vector3.Cross(up,velocity);break;
                }
                float dot=Vector3.Dot(v.Autopilot.SAS.targetOrientation,expected.normalized);
                Debug.Log("[RingworldSmoke] SAS "+display+" "+mode+" dot="+dot);
                if(dot<.9999f){fail("SAS did not target ring direction");yield break;}
            }
            v.Autopilot.Disable();v.ActionGroups.SetGroup(KSPActionGroup.SAS,false);
            f.arrivalHeight=400000;f.Visit();while(!f.Ready)yield return null;
            v.SetWorldVelocity(ConvertVector.Ksp(f.Settings.Geometry.Up(f.Position(v))*-1000));f.Leave();
            MapView.EnterMapView();yield return new WaitForSecondsRealtime(2);
            int before=f.trajectory.PredictionCommits;yield return new WaitForSecondsRealtime(2);
            int updates=f.trajectory.PredictionCommits-before;
            Debug.Log("[RingworldSmoke] TRAJECTORY commits/2s="+updates);
            if(updates<8||f.trajectory.EncounterCount==0){fail("Trajectory refresh or encounter regression");yield break;}
            MapView.ExitMapView();f.arrivalHeight=3000;f.Visit();while(!f.Ready)yield return null;
            var chute=v.FindPartModuleImplementing<ModuleParachute>();if(chute==null){fail("Stock parachute missing in fixture");yield break;}
            chute.deployAltitude=1000;v.SetWorldVelocity(ConvertVector.Ksp(f.Settings.Geometry.Up(f.Position(v))*-20));
            yield return new WaitForSeconds(1);chute.Deploy();
            float end=Time.realtimeSinceStartup+30;
            while(chute.deploymentState!=ModuleParachute.deploymentStates.SEMIDEPLOYED&&Time.realtimeSinceStartup<end)yield return null;
            if(chute.deploymentState!=ModuleParachute.deploymentStates.SEMIDEPLOYED){fail("Parachute did not semi-deploy: "+chute.deploymentState);yield break;}
            Debug.Log("[RingworldSmoke] PARACHUTE semi-deployed pressure="+v.staticPressurekPa+" solarPressure="+FlightGlobals.getStaticPressure(chute.part.transform.position,v.mainBody));
            chute.deployAltitude=5000;end=Time.realtimeSinceStartup+30;
            while(chute.deploymentState!=ModuleParachute.deploymentStates.DEPLOYED&&Time.realtimeSinceStartup<end)yield return null;
            if(chute.deploymentState!=ModuleParachute.deploymentStates.DEPLOYED){fail("Parachute did not fully deploy: "+chute.deploymentState);yield break;}
            Debug.Log("[RingworldSmoke] PARACHUTE fully deployed at ring altitude="+f.SurfaceClearance(v));
        }
    }
}
#endif
