#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class MultiRingSmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            var state=RingworldScenario.Instance;var v=FlightGlobals.ActiveVessel;
            bool missingCyla=Array.IndexOf(Environment.GetCommandLineArgs(),"-ringworld-no-cyla")>=0;
            if(missingCyla)foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())if(assembly.GetType("Cyla.ShaderLoader",false)!=null){fail("Cyla still loaded in dependency-free fixture");yield break;}
            // An isolated save and a plain pod keep the test within the laptop budget.
            var primary=state.GetOptions().CreateCopy();primary.SetValue("ringId","primary",true);f.ApplyOptions(primary,true);
            var second=primary.CreateCopy();second.SetValue("ringId","test-ring",true);second.SetValue("ringName","Remote habitat",true);
            second.SetValue("centerY",4e8,true);second.SetValue("designatedStar",false,true);second.SetValue("seed",90210,true);
            var candidate=Settings.Load();candidate.Apply(second);
            if(RingSelection.Validate(candidate,null)!=null){fail("Separated habitat rejected");yield break;}
            var overlap=Settings.Load();overlap.Apply(primary);if(RingSelection.Validate(overlap,null)==null){fail("Overlapping habitat accepted");yield break;}
            state.Rings.Add(second);
            yield return new WaitForSecondsRealtime(2);
            if(UnityEngine.Object.FindObjectsOfType<ScaledRing>().Length!=2){fail("Both ring renderers were not created");yield break;}
            f.arrivalHeight=8;string reason;
            if(!f.VisitRing("test-ring",out reason)){fail(reason);yield break;}
            float end=Time.realtimeSinceStartup+70;
            while(!f.Ready&&Time.realtimeSinceStartup<end)yield return null;
            if(!f.Ready){fail("Remote ring transfer did not finish");yield break;}
            while((!v.Landed||!f.surfaceWarp.CanAdvance(f))&&Time.realtimeSinceStartup<end)yield return new WaitForSecondsRealtime(.25f);
            if(!v.Landed||FlightGlobals.ClearToSave(false)!=ClearToSaveStatus.CLEAR){fail("Remote landing not saveable "+f.surfaceWarp.Status);yield break;}
            if(missingCyla){var options=f.Settings.Save();options.SetValue("atmosphereBackend",1,true);f.ApplyOptions(options,false);yield return new WaitForSecondsRealtime(2);if(f.visuals!=null&&f.visuals.CylaActive){fail("Missing Cyla backend reported active");yield break;}Debug.Log("[RingworldSmoke] Optional Cyla absent: requested backend safely falls back to Original");}
            f.Capture();var record=state.Vessels[v.id.ToString()];
            if(record.RingId!="test-ring"||!state.Occupied("test-ring")||Math.Abs(f.Center.y-f.Star.position.y-4e8)>1){fail("Remote residence identity/center incorrect");yield break;}
            Settings scienceSettings;ResearchLocation location;
            if(!RingScience.Resolve(v,out scienceSettings,out location)||!location.Id.StartsWith("test-ring-")){fail("Science not isolated by ring");yield break;}
            var expected=f.Position(v);double ut=Planetarium.GetUniversalTime();
            TimeWarp.SetRate(3,true);yield return new WaitForSecondsRealtime(2);TimeWarp.SetRate(0,true);yield return new WaitForSecondsRealtime(3);
            if((f.Position(v)-expected).Length>5||!v.Landed||Planetarium.GetUniversalTime()<=ut+2){fail("Remote landed warp failed");yield break;}
            Debug.Log("[RingworldSmoke] MULTIRING remote landing, science identity and native warp passed");
            MapView.EnterMapView();yield return new WaitForSecondsRealtime(2);PlanetariumCamera.fetch.SetDistance(5000000);yield return new WaitForSecondsRealtime(3);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(KSPUtil.ApplicationRootPath,"RingworldMultiRingMap.png"));yield return new WaitForSecondsRealtime(2);MapView.ExitMapView();
            f.Capture();var id=v.id;var folder=HighLogic.SaveFolder;expected=f.Position(v);
            GamePersistence.SaveGame(HighLogic.CurrentGame.Updated(),"persistent",folder,SaveMode.OVERWRITE);
            HighLogic.LoadScene(GameScenes.SPACECENTER);while(HighLogic.LoadedScene!=GameScenes.SPACECENTER)yield return null;yield return new WaitForSecondsRealtime(4);
            var reload=GamePersistence.LoadGame("persistent",folder,true,false);int index=reload.flightState.protoVessels.FindIndex(p=>p.vesselID==id);
            if(index<0){fail("Remote resident lost");yield break;}
            FlightDriver.StartAndFocusVessel(reload,index);
            end=Time.realtimeSinceStartup+70;
            while((!HighLogic.LoadedSceneIsFlight||RingworldFlight.Instance==null||!RingworldFlight.Instance.Ready)&&Time.realtimeSinceStartup<end)yield return null;
            f=RingworldFlight.Instance;if(f==null||!f.Ready){fail("Remote resume timeout");yield break;}
            yield return new WaitForSecondsRealtime(4);v=FlightGlobals.ActiveVessel;state=RingworldScenario.Instance;
            if(state.Rings.Count!=2||f.Settings.RingId!="test-ring"||!v.Landed||(f.Position(v)-expected).Length>5){fail("Remote save reload incorrect");yield break;}
            var shifted=state.RingOptions("primary").CreateCopy();shifted.SetValue("centerY",-4e8,true);state.Rings[state.Rings.IndexOf(state.RingOptions("primary"))]=shifted;
            yield return new WaitForSecondsRealtime(2);
            if((f.Position(v)-expected).Length>5){fail("Moving another ring moved resident");yield break;}
            state.Rings.Remove(shifted);yield return new WaitForSecondsRealtime(2);
            if(UnityEngine.Object.FindObjectsOfType<ScaledRing>().Length!=1){fail("Deleted ring renderer retained");yield break;}
            yield return TrackingSmoke.Run(f,fail);
            Debug.Log("[RingworldSmoke] PASS multiple rings: independent rendering, overlap guard, offset flight/warp/science, KSC save roundtrip, move/delete unoccupied ring");
        }
    }
}
#endif
