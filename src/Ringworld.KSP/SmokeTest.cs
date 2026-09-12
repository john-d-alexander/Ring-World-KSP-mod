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
            RingworldFlight.Instance.arrivalHeight=60;
            RingworldFlight.Instance.Visit();
            while(!RingworldFlight.Instance.Ready||v.mainBody!=RingworldFlight.Instance.Star)yield return null;
            FlightCamera.fetch.SetDistanceImmediate(100);FlightCamera.CamPitch=.2f;
            yield return new WaitForSeconds(2);
            var flight=RingworldFlight.Instance;var p=flight.Settings.Geometry.Coordinates(flight.Position(v));
            double first=p.Altitude;Debug.Log("[RingworldSmoke] ENTRY h="+first+" v="+v.obt_velocity.magnitude);
            yield return new WaitForSeconds(1);
            p=flight.Settings.Geometry.Coordinates(flight.Position(v));
            Debug.Log("[RingworldSmoke] DROP dh="+(p.Altitude-first)+" speed="+v.obt_velocity.magnitude);
            // A fixture-only descent controller isolates contact stability from a destructive free fall.
            for(int step=0;step<3000;step++)
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
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldSmoke.png"));
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
