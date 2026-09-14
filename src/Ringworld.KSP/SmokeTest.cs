#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.IO;
using Ringworld.Core;
using UnityEngine;
using HarmonyLib;

namespace NivenRingworld
{
    // Compiled only with -p:SmokeTest=true. Release packages do not contain this harness.
    [KSPAddon(KSPAddon.Startup.MainMenu,true)]
    public sealed class SmokeTest : MonoBehaviour
    {
        internal static KerbalEVA WalkingEva;
        internal static Vector2 WalkInput=Vector2.up;
        private string folder;
        private bool running;
        private float deadline;
        public IEnumerator Start()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-ringworld-smoketest")<0)yield break;
            DontDestroyOnLoad(gameObject);deadline=Time.realtimeSinceStartup+500;running=true;
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
            game.Parameters.Flight.CanEVA=true;
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
            flight.ApplyOptions(RingworldScenario.Instance.GetOptions(),true);
            int worldSeed=flight.Settings.Geometry.P.Seed;
            var optionCopy=flight.Settings.Save();var optionCheck=Settings.Load();optionCheck.Apply(optionCopy);
            if(optionCheck.Geometry.P.Seed!=worldSeed||optionCheck.LodRange!=160000000||optionCheck.LodResolution!=16){Fail("Save settings roundtrip failed");yield break;}
            Debug.Log("[RingworldSmoke] OPTIONS seed="+worldSeed+" range="+optionCheck.LodRange+" quality="+optionCheck.LodResolution);
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
            var navball=UnityEngine.Object.FindObjectOfType<KSP.UI.Screens.Flight.NavBall>();
            foreach(var mode in new[]{FlightGlobals.SpeedDisplayModes.Orbit,FlightGlobals.SpeedDisplayModes.Surface})
            {
                FlightGlobals.SetSpeedMode(mode);
                yield return null;yield return new WaitForEndOfFrame();
                if(navball==null||navball.progradeVector.gameObject.activeSelf||navball.retrogradeVector.gameObject.activeSelf||navball.normalVector.gameObject.activeSelf||navball.radialOutVector.gameObject.activeSelf)
                {Fail("Stationary navball cues were visible in "+mode);yield break;}
            }
            Debug.Log("[RingworldSmoke] NAVBALL stationary cues hidden in both modes; speed="+KSP.UI.Screens.Flight.SpeedDisplay.Instance.textSpeed.text+" CanEVA="+HighLogic.CurrentGame.Parameters.Flight.CanEVA);
            if(!HighLogic.CurrentGame.Parameters.Flight.CanEVA||FlightCamera.GetAutoModeForVessel(v)!=FlightCamera.Modes.FREE){Fail("Scenario EVA permission or camera surface mode incorrect");yield break;}
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
            // Check underside collision and camera recovery from below the actual mesh.
            var centre=geom.Coordinates(flight.Position(v));
            var under=geom.Position(centre.Along+300,centre.Across,flight.Settings.UndersideAltitude-10);
            RaycastHit undersideHit;
            bool underside=Physics.Raycast((Vector3)(flight.Star.position+ConvertVector.Ksp(under)),ConvertVector.Unity(geom.Up(under)),out undersideHit,30,1<<15);
            Debug.Log("[RingworldSmoke] UNDERSIDE hit="+underside+" distance="+undersideHit.distance);
            if(!underside||Math.Abs(undersideHit.distance-10)>.1){Fail("Underside is not collidable");yield break;}
            double floor=flight.Settings.Terrain.Sample(centre.Along,centre.Across).Height;
            var below=(Vector3)(flight.Star.position+ConvertVector.Ksp(geom.Position(centre.Along,centre.Across,floor-20)));
            var safe=flight.ConstrainCamera(below,(Vector3)(flight.Star.position+ConvertVector.Ksp(flight.Position(v))),.6f);
            double clearance=geom.Coordinates(ConvertVector.Core((Vector3d)safe-flight.Star.position)).Altitude-floor;
            Debug.Log("[RingworldSmoke] CAMERA clearance="+clearance);
            if(clearance<.59){Fail("Camera stayed below floor");yield break;}
            var lake=flight.Settings.Terrain.Landmarks.Find(l=>l.Id=="waterway");
            var lakeSample=flight.Settings.Terrain.Sample(lake.Along,lake.Across);
            var wetCamera=(Vector3)(flight.Star.position+ConvertVector.Ksp(geom.Position(lake.Along,lake.Across,lakeSample.WaterHeight-2)));
            var wetTarget=(Vector3)(flight.Star.position+ConvertVector.Ksp(geom.Position(lake.Along,lake.Across,lakeSample.WaterHeight+10)));
            var waterSafe=flight.ConstrainCamera(wetCamera,wetTarget,.6f);
            double waterClearance=geom.Coordinates(ConvertVector.Core((Vector3d)waterSafe-flight.Star.position)).Altitude-lakeSample.WaterHeight;
            Debug.Log("[RingworldSmoke] WATER CAMERA clearance="+waterClearance+" ALTITUDE="+flight.SurfaceClearance(v)+" LOD blocks="+flight.LodCount);
            if(waterClearance<.55||flight.SurfaceClearance(v)>10||flight.LodCount<50){Fail("Water camera, ring altitude or terrain LOD failed");yield break;}
            while(flight.LodPending>0)yield return null;
            Debug.Log("[RingworldSmoke] HORIZON complete blocks="+flight.LodCount+" scaled="+flight.ScaledLodCount);
            if(flight.ScaledLodCount<10||flight.Settings.Geometry.P.Seed!=worldSeed){Fail("Far horizon or saved seed failed");yield break;}
            AccessTools.Field(typeof(RingworldFlight),"panelTab").SetValue(flight,1);
            yield return new WaitForSeconds(1);
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldSettings.png"));
            yield return new WaitForSeconds(1);AccessTools.Field(typeof(RingworldFlight),"panelTab").SetValue(flight,0);
            FlightCamera.CamPitch=-1.3f;
            yield return new WaitForSeconds(2);
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldSky.png"));
            yield return new WaitForSeconds(1);FlightCamera.CamPitch=.2f;
            CheatOptions.NoCrashDamage=false;CheatOptions.UnbreakableJoints=false;
            var cabin=v.parts.Find(part=>part.protoModuleCrew.Count>0);
            double epoch=flight.FrameEpoch;var parentBefore=flight.Position(v);
            var eva=FlightEVA.fetch.spawnEVA(cabin.protoModuleCrew[0],cabin,cabin.airlock,true);
            if(eva==null){Fail("EVA hatch blocked");yield break;}
            while(eva.vessel==null||eva.vessel.packed||FlightGlobals.ActiveVessel!=eva.vessel)yield return null;
            if(!flight.Owns(eva.vessel)){Fail("EVA did not inherit rotating frame");yield break;}
            yield return new WaitForSeconds(2);
            if(eva.OnALadder)eva.fsm.RunEvent(eva.On_ladderLetGo);
            bool touched=false;double fastest=0;
            for(int step=0;step<1200;step++)
            {
                if(eva==null||eva.vessel==null||eva.part.State==PartStates.DEAD){Fail("EVA died on contact");yield break;}
                double speed=flight.Velocity(eva.vessel).Length;fastest=Math.Max(fastest,speed);
                if(speed>40||flight.FrameEpoch!=epoch||!flight.Owns(eva.vessel)){Fail("EVA frame discontinuity speed="+speed);yield break;}
                touched|=RingEva.Grounded(eva);
                yield return new WaitForFixedUpdate();
            }
            Debug.Log("[RingworldSmoke] EVA contact="+touched+" peakSpeed="+fastest+" settled="+flight.Velocity(eva.vessel).Length+" state="+eva.fsm.CurrentState.name+" parentDrift="+(flight.Position(v)-parentBefore).Length);
            if(!touched||flight.Velocity(eva.vessel).Length>2||(flight.Position(v)-parentBefore).Length>2){Fail("EVA or parent did not settle");yield break;}
            if(eva.isRagdoll)eva.fsm.RunEvent(eva.On_recover_start);
            yield return new WaitForSeconds(4);
            Debug.Log("[RingworldSmoke] EVA RECOVER state="+eva.fsm.CurrentState.name+" ragdoll="+eva.isRagdoll);
            Debug.Log("[RingworldSmoke] EVA SPEED source="+EvaTransitionSpeedPatch.HorizontalSpeed(eva.vessel)+" stockSolar="+eva.vessel.horizontalSrfSpeed);
            if(eva.isRagdoll){Fail("EVA could not recover from ragdoll");yield break;}
            var walkStart=flight.Position(eva.vessel);WalkingEva=eva;
            for(int tick=0;tick<250;tick++)
            {
                if(eva==null||eva.vessel==null||eva.part.State==PartStates.DEAD){Fail("EVA died on first walk");yield break;}
                if(flight.Velocity(eva.vessel).Length>40){Fail("First EVA walk velocity discontinuity");yield break;}
                yield return new WaitForFixedUpdate();
            }
            bool walking=eva.fsm.CurrentState==eva.st_walk_acd||eva.fsm.CurrentState==eva.st_walk_fps;
            Debug.Log("[RingworldSmoke] EVA WALK state="+eva.fsm.CurrentState.name);
            WalkingEva=null;
            double walkDistance=(flight.Position(eva.vessel)-walkStart).Length;
            Debug.Log("[RingworldSmoke] EVA WALK distance="+walkDistance);
            if(!walking||walkDistance<1||walkDistance>40){Fail("EVA walking did not translate across floor");yield break;}
            WalkingEva=eva;
            foreach(var direction in new[]{new Vector2(1,1),Vector2.right,Vector2.down,new Vector2(-1,-1),Vector2.left,Vector2.up})
            {
                WalkInput=direction;var start=flight.Position(eva.vessel);double peak=0;
                for(int tick=0;tick<200;tick++)
                {
                    if(eva==null||eva.part.State==PartStates.DEAD){Fail("EVA died while turning "+direction);yield break;}
                    double speed=flight.Velocity(eva.vessel).Length;peak=Math.Max(peak,speed);
                    if(speed>40||double.IsNaN(speed)){Fail("EVA turning speed="+speed+" direction="+direction+" state="+eva.fsm.CurrentState.name);yield break;}
                    yield return new WaitForFixedUpdate();
                }
                Debug.Log("[RingworldSmoke] EVA DIRECTION "+direction+" peak="+peak+" distance="+(flight.Position(eva.vessel)-start).Length+" state="+eva.fsm.CurrentState.name);
            }
            // Exercise rapid reversals through the game's key-binding path, with damage on.
            for(int tick=0;tick<600;tick++)
            {
                WalkInput=new Vector2((tick/3)%2==0?1:-1,(tick/5)%2==0?1:-1);
                if(eva==null||eva.part.State==PartStates.DEAD||flight.Velocity(eva.vessel).Length>40){Fail("Rapid EVA direction changes failed");yield break;}
                yield return new WaitForFixedUpdate();
            }
            Debug.Log("[RingworldSmoke] EVA RAPID REVERSALS alive speed="+flight.Velocity(eva.vessel).Length+" temperature="+eva.part.temperature);
            WalkingEva=null;
            yield return new WaitForSeconds(2);
            InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS,"RingworldSmoke.Camera");
            foreach(var mode in new[]{FlightCamera.Modes.FREE,FlightCamera.Modes.CHASE})
            {
                FlightCamera.SetMode(mode);FlightCamera.fetch.SetDistanceImmediate(5);FlightCamera.CamPitch=.3f;
                yield return new WaitForSeconds(2);
                // Compare rendered poses; Update can still contain the preceding
                // camera pose transformed by this tick's EVA body rotation.
                yield return new WaitForEndOfFrame();
                var previous=FlightCamera.fetch.mainCamera.transform.rotation;float peakAngle=0;
                for(int tick=0;tick<120;tick++)
                {
                    yield return new WaitForEndOfFrame();
                    var current=FlightCamera.fetch.mainCamera.transform.rotation;
                    float angle=Quaternion.Angle(previous,current);
                    if(angle>.2f)
                    {
                        var camera=FlightCamera.fetch;
                        Debug.Log("[RingworldSmoke] CAMERA TRACE angle="+angle+" pitch="+camera.camPitch+" pivot="+camera.GetPivot().rotation.eulerAngles+" frame="+camera.getReferenceFrame().eulerAngles+" localPos="+camera.transform.localPosition+" localRot="+camera.transform.localRotation.eulerAngles+" velocity="+flight.Velocity(eva.vessel).Length+" state="+eva.fsm.CurrentState.name);
                    }
                    peakAngle=Mathf.Max(peakAngle,angle);previous=current;
                }
                Debug.Log("[RingworldSmoke] EVA CAMERA mode="+mode+" peakFrameRotation="+peakAngle+" wobble="+RingCameraTerrainPatch.Wobble(FlightCamera.fetch)+" effects="+RingCameraTerrainPatch.Effects());
                if(peakAngle>.2f||RingCameraTerrainPatch.Wobble(FlightCamera.fetch)!=0||RingCameraTerrainPatch.Effects()!=0){Fail("EVA camera orientation unstable");yield break;}
            }
            InputLockManager.RemoveControlLock("RingworldSmoke.Camera");
            ScreenCapture.CaptureScreenshot(Path.Combine(KSPUtil.ApplicationRootPath,"RingworldEva.png"));
            yield return new WaitForSeconds(1);
            Debug.Log("[RingworldSmoke] PASS");
            yield return new WaitForSeconds(2);running=false;Application.Quit();
        }
        public void Update()
        {
            if(!running)return;
            if(WalkingEva!=null&&(WalkingEva.part.State==PartStates.DEAD||WalkingEva.vessel==null)){Fail("EVA lost during walking");return;}
            if(Time.realtimeSinceStartup>deadline)Fail("Timeout");
        }
        public void OnGUI()
        {
            if(running)GUI.Box(new Rect(Screen.width/2-350,145,700,65),"AUTOMATED MOD TEST — scripted controls\nNormal build is restored when this test finishes.",new GUIStyle(GUI.skin.box){fontSize=18});
        }
        private void Fail(string message){EvaTrace.Dump();Debug.LogError("[RingworldSmoke] FAIL: "+message);running=false;Application.Quit();}
    }
    [HarmonyPatch]
    internal static class EvaTrace
    {
        private static readonly System.Collections.Generic.Queue<string> trace=new System.Collections.Generic.Queue<string>();
        private static System.Collections.Generic.IEnumerable<System.Reflection.MethodBase> TargetMethods()
        {
            foreach(var name in new[]{"HandleMovementInput","correctGroundedRotation","UpdateHeading","UpdateMovement","updateRagdollVelocities"})yield return AccessTools.Method(typeof(KerbalEVA),name);
        }
        private static void Prefix(KerbalEVA __instance,System.Reflection.MethodBase __originalMethod){Record(__instance,"before "+__originalMethod.Name);}
        private static void Postfix(KerbalEVA __instance,System.Reflection.MethodBase __originalMethod){Record(__instance,"after "+__originalMethod.Name);}
        private static void Record(KerbalEVA eva,string step)
        {
            if(eva!=SmokeTest.WalkingEva||eva.part.rb==null)return;
            var rb=eva.part.rb;var pivot=AccessTools.Field(typeof(KerbalEVA),"footPivot").GetValue(eva) as Transform;
            string line=step+" frame="+Time.frameCount+" pos="+rb.position+" v="+rb.velocity+" w="+rb.angularVelocity+" com="+rb.centerOfMass+" foot="+(pivot==null?Vector3.zero:pivot.position-eva.transform.position)+" up="+eva.fUp+" tgt="+AccessTools.Field(typeof(KerbalEVA),"tgtRpos").GetValue(eva)+" speed="+AccessTools.Field(typeof(KerbalEVA),"currentSpd").GetValue(eva);
            trace.Enqueue(line);while(trace.Count>60)trace.Dequeue();
        }
        internal static void Dump(){foreach(var line in trace)Debug.Log("[RingworldSmoke] TRACE "+line);}
    }
    [HarmonyPatch(typeof(KeyBinding),"GetKey")]
    internal static class EvaSmokeInput
    {
        private static bool Prefix(KeyBinding __instance,ref bool __result)
        {
            if(SmokeTest.WalkingEva==null)return true;
            if(__instance==GameSettings.EVA_forward)__result=SmokeTest.WalkInput.y>0;
            else if(__instance==GameSettings.EVA_back)__result=SmokeTest.WalkInput.y<0;
            else if(__instance==GameSettings.EVA_left)__result=SmokeTest.WalkInput.x<0;
            else if(__instance==GameSettings.EVA_right)__result=SmokeTest.WalkInput.x>0;
            else return true;
            return false;
        }
    }
}
#endif
