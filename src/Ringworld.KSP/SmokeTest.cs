#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    // Compiled only with -p:SmokeTest=true. Release packages do not contain this harness.
    [KSPAddon(KSPAddon.Startup.MainMenu,true)]
    public sealed class SmokeTest : MonoBehaviour
    {
        private string folder;
        private bool running;
        private float deadline;
        public IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ringworld-smoketest")<0)yield break;
            DontDestroyOnLoad(gameObject);deadline=Time.realtimeSinceStartup+240;running=true;
            Debug.Log("[RingworldSmoke] MAIN MENU READY");
            yield return new WaitForSeconds(3);
            foreach(var dialog in UnityEngine.Object.FindObjectsOfType<WhatsNewDialog>())HarmonyLib.AccessTools.Method(typeof(WhatsNewDialog),"Dismiss").Invoke(dialog,null);
            folder="RingworldSmoke-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            Directory.CreateDirectory(Path.Combine(KSPUtil.ApplicationRootPath,"saves",folder));
            var root=ConfigNode.Load(Path.Combine(KSPUtil.ApplicationRootPath,"saves","training","C_Orbit101.sfs"));
            var node=root.GetNode("GAME");node.SetValue("Mode","0");node.SetValue("Title",folder);node.SetValue("scene","7");
            foreach(var scenario in node.GetNodes("SCENARIO"))if((scenario.GetValue("name")??"").StartsWith("Tutorial"))node.RemoveNode(scenario);
            var state=node.GetNode("FLIGHTSTATE");
            foreach(var vessel in state.GetNodes("VESSEL"))if(vessel.GetValue("type")=="SpaceObject")state.RemoveNode(vessel);
            state.SetValue("activeVessel","0",true);
            var game=GamePersistence.LoadGameCfg(root,folder,true,false);
            if(game==null){Fail("Unable to load test fixture");yield break;}
            game.Mode=Game.Modes.SANDBOX;game.startScene=GameScenes.FLIGHT;HighLogic.SaveFolder=folder;HighLogic.CurrentGame=game;
            game.AddProtoScenarioModule(typeof(RingworldScenario),GameScenes.FLIGHT,GameScenes.SPACECENTER,GameScenes.TRACKSTATION);
            game.Start();
            while(!HighLogic.LoadedSceneIsFlight||RingworldFlight.Instance==null||FlightGlobals.ActiveVessel==null||FlightGlobals.ActiveVessel.packed)yield return null;
            var v=FlightGlobals.ActiveVessel;
            Debug.Log("[RingworldSmoke] FLIGHT READY "+v.vesselName);
            while(RingworldScenario.Instance==null)yield return null;
            Debug.Log("[RingworldSmoke] SCENARIO READY");
            // This harness tests an unpowered impact with a damage-immune fixture.
            CheatOptions.NoCrashDamage=true;CheatOptions.UnbreakableJoints=true;
            deadline=Time.realtimeSinceStartup+500;
            FlightInputHandler.state.mainThrottle=0;v.ctrlState.mainThrottle=0;
            v.ActionGroups.SetGroup(KSPActionGroup.SAS,false);v.ActionGroups.SetGroup(KSPActionGroup.RCS,false);
            foreach(var engine in v.FindPartModulesImplementing<ModuleEngines>())engine.Shutdown();
            // Fixture setup only: establish a spin-matched approach outside the capture shell.
            var flight=RingworldFlight.Instance;
            flight.arrivalHeight=230000;flight.Visit();
            while(!flight.Ready||v.mainBody!=flight.Star)yield return null;
            var geom=flight.Settings.Geometry;
            v.SetWorldVelocity(ConvertVector.Ksp(geom.Up(flight.Position(v))*-2000));
            var departurePos=flight.Position(v);var departureVel=flight.Velocity(v);
            double elapsed=Planetarium.GetUniversalTime()-flight.FrameEpoch;
            var expectedPos=geom.ToInertialPosition(departurePos,elapsed);
            var expectedVel=geom.ToInertialVelocity(departurePos,departureVel,elapsed);
            flight.Leave();
            double dp=(flight.Position(v)-expectedPos).Length,dv=(flight.Velocity(v)-expectedVel).Length;
            Debug.Log("[RingworldSmoke] DEPARTURE positionError="+dp+" velocityError="+dv);
            if(dp>2||dv>1){Fail("Departure did not preserve inertial state");yield break;}
            while(!flight.Owns(v))yield return null;
            double arrivalSpeed=flight.Velocity(v).Length;
            Debug.Log("[RingworldSmoke] AUTO ARRIVAL altitude="+geom.Coordinates(flight.Position(v)).Altitude+" speed="+arrivalSpeed);
            if(arrivalSpeed<1800||arrivalSpeed>2300){Fail("Arrival erased or corrupted relative velocity");yield break;}
            // Sample the real flight integrator inside the atmosphere.
            flight.arrivalHeight=30000;flight.Visit();
            while(!flight.Ready)yield return null;
            v.SetWorldVelocity(ConvertVector.Ksp(geom.Up(flight.Position(v))*-100));
            yield return new WaitForSeconds(2);
            Debug.Log("[RingworldSmoke] AIR density="+v.atmDensity+" pressure="+v.staticPressurekPa+" mach="+v.mach+" q="+v.dynamicPressurekPa);
            if(v.atmDensity<=0||v.staticPressurekPa<=0||v.mach<=0||v.dynamicPressurekPa<=0){Fail("Native air integration missing");yield break;}
            FlightCamera.fetch.SetDistanceImmediate(40);FlightCamera.CamPitch=.6f;
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldArrival.png"));
            yield return new WaitForSeconds(1);
            flight.arrivalHeight=8000;flight.Visit();
            while(!flight.Ready)yield return null;
            yield return new WaitForSeconds(2);
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldClouds.png"));
            yield return new WaitForSeconds(1);
            RingworldFlight.Instance.arrivalHeight=150;
            RingworldFlight.Instance.Visit();
            while(!RingworldFlight.Instance.Ready||v.mainBody!=RingworldFlight.Instance.Star)yield return null;
            FlightCamera.fetch.SetDistanceImmediate(35);FlightCamera.CamPitch=.2f;
            yield return new WaitForSeconds(2);
            var p=flight.Settings.Geometry.Coordinates(flight.Position(v));
            double first=p.Altitude;Debug.Log("[RingworldSmoke] ENTRY h="+first+" v="+v.obt_velocity.magnitude);
            yield return new WaitForSeconds(1);
            p=flight.Settings.Geometry.Coordinates(flight.Position(v));
            Debug.Log("[RingworldSmoke] DROP dh="+(p.Altitude-first)+" speed="+v.obt_velocity.magnitude);
            // A fixture-only descent controller isolates contact stability from a destructive free fall.
            for(int step=0;step<6000;step++)
            {
                bool contact=false;foreach(var part in v.parts)if(part.GroundContact){contact=true;break;}
                if(contact){Debug.Log("[RingworldSmoke] CONTACT");break;}
                v.SetWorldVelocity(ConvertVector.Ksp(flight.Settings.Geometry.Up(flight.Position(v))*-2));
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(12);
            p=flight.Settings.Geometry.Coordinates(flight.Position(v));var terrain=flight.Settings.Terrain.Sample(p.Along,p.Across);
            double agl=p.Altitude-terrain.Height;
            Debug.Log("[RingworldSmoke] SETTLED agl="+agl+" speed="+v.obt_velocity.magnitude+" parts="+v.parts.Count);
            Debug.Log("[RingworldSmoke] FRAME velocity="+Krakensbane.GetFrameVelocity().magnitude);
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldSmoke.png"));
            // ScreenCapture runs at end-of-frame. Do not start scenario restoration
            // (which temporarily returns the camera to the solar frame) in that frame.
            yield return new WaitForSeconds(1);
            flight.Capture();var saved=new ConfigNode("SCENARIO");RingworldScenario.Instance.OnSave(saved);
            Debug.Log("[RingworldSmoke] SAVE vessel records="+saved.GetNodes("VESSEL").Length);
            saved.Save(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldSmoke-scenario.cfg"));
            if(agl< -10||agl>100||v.obt_velocity.magnitude>10){Fail("Surface did not settle");yield break;}
            // Validate KSP's science container and subject registration using a temporary survey module.
            var scanner=(ModuleRingSurvey)v.rootPart.AddModule("ModuleRingSurvey");scanner.Survey();
            Debug.Log("[RingworldSmoke] SCIENCE stored="+scanner.GetScienceCount());
            if(scanner.GetScienceCount()!=1){Fail("Survey was not collected");yield break;}
            var copy=new ConfigNode();scanner.OnSave(copy);scanner.OnLoad(copy);
            if(scanner.GetScienceCount()!=1){Fail("Survey failed persistence roundtrip");yield break;}
            var before=flight.Position(v);
            RingworldScenario.Instance.OnLoad(saved);
            while(!flight.Owns(v))yield return null;
            yield return new WaitForSeconds(3);
            double restoredDrift=(flight.Position(v)-before).Length;
            Debug.Log("[RingworldSmoke] RESTORE displacement="+restoredDrift);
            if(restoredDrift>15){Fail("Saved ring coordinates did not restore");yield break;}
            Debug.Log("[RingworldSmoke] PASS");
            yield return new WaitForSeconds(2);running=false;Application.Quit();
        }
        public void Update(){if(running&&Time.realtimeSinceStartup>deadline)Fail("Timeout");}
        private void Fail(string message){Debug.LogError("[RingworldSmoke] FAIL: "+message);running=false;Application.Quit();}
    }
}
#endif
