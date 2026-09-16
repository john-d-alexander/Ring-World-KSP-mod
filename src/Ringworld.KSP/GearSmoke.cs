#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using UnityEngine;
namespace NivenRingworld
{
    internal static class GearSmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            var v=FlightGlobals.ActiveVessel;int parts=v.parts.Count;
            var legs=v.FindPartModulesImplementing<ModuleWheelBase>();
            Debug.Log("[RingworldSmoke] GEAR fixture parts="+parts+" wheels="+legs.Count);
            if(legs.Count!=8){fail("Expected user's eight-leg craft");yield break;}
            v.ActionGroups.SetGroup(KSPActionGroup.Gear,true);
            foreach(var d in v.FindPartModulesImplementing<ModuleWheels.ModuleWheelDeployment>())HarmonyLib.AccessTools.Method(typeof(ModuleWheels.ModuleWheelDeployment),"ToggleDeployment").Invoke(d,new object[]{true});
            yield return new WaitForSeconds(4);
            // Root is the top pod: its feet are approximately eight metres below it.
            // Only initial placement is controlled. Gravity, suspension and impacts remain live.
            f.arrivalHeight=10;f.Visit();while(!f.Ready)yield return null;
            CheatOptions.NoCrashDamage=false;CheatOptions.UnbreakableJoints=false;
            double peakSpeed=0;float end=Time.realtimeSinceStartup+65,next=0;
            while(Time.realtimeSinceStartup<end)
            {
                peakSpeed=Math.Max(peakSpeed,f.Velocity(v).Length);
                if(Time.realtimeSinceStartup>next)
                {
                    next=Time.realtimeSinceStartup+5;double angular=0;int grounded=0;
                    foreach(var p in v.parts)if(p.rb!=null)angular=Math.Max(angular,p.rb.angularVelocity.magnitude);
                    foreach(var leg in legs)if(leg.isGrounded)grounded++;
                    Debug.Log("[RingworldSmoke] GEAR sample clearance="+f.SurfaceClearance(v)+" speed="+f.Velocity(v).Length+" angular="+angular+" grounded="+grounded+" parts="+v.parts.Count+" stockGravity="+v.gravityForPos+" warp="+f.surfaceWarp.CanAdvance(f)+" "+f.surfaceWarp.Status);
                }
                if(v.state==Vessel.State.DEAD||v.parts.Count!=parts){fail("Natural landing lost parts");yield break;}
                yield return null;
            }
            if(v.parts.Count!=parts||!v.Landed||!f.surfaceWarp.CanAdvance(f)){fail("Natural-gravity landing failed: "+f.surfaceWarp.Status+" peakSpeed="+peakSpeed);yield break;}
            PauseMenu.Display();yield return new WaitForSecondsRealtime(.5f);
            bool clear=PauseMenu.canSaveAndExit==ClearToSaveStatus.CLEAR;PauseMenu.Close();
            if(!clear){fail("Gear fixture cannot save while paused");yield break;}
            foreach(int rate in new[]{10,1000,1000})
            {
                var before=(Vector3d)v.transform.position-f.Star.position;
                f.surfaceWarp.Rate=rate;yield return new WaitForSecondsRealtime(2);
                if(!v.packed){fail("Gear fixture did not enter stock warp");yield break;}
                f.surfaceWarp.Rate=1;while(v.packed)yield return null;yield return new WaitForSeconds(5);
                double drift=((Vector3d)v.transform.position-f.Star.position-before).magnitude;
                Debug.Log("[RingworldSmoke] GEAR warp="+rate+" drift="+drift+" parts="+v.parts.Count);
                if(v.parts.Count!=parts||!v.Landed||drift>1){fail("Gear warp damaged or moved craft");yield break;}
            }
            var saved=(Vector3d)v.transform.position-f.Star.position;var id=v.id;var folder=HighLogic.SaveFolder;
            f.Capture();GamePersistence.SaveGame(HighLogic.CurrentGame.Updated(),"persistent",folder,SaveMode.OVERWRITE);
            HighLogic.LoadScene(GameScenes.SPACECENTER);while(HighLogic.LoadedScene!=GameScenes.SPACECENTER)yield return null;yield return new WaitForSecondsRealtime(5);
            var game=GamePersistence.LoadGame("persistent",folder,true,false);int index=game.flightState.protoVessels.FindIndex(p=>p.vesselID==id);
            if(index<0){fail("Gear resident missing from save");yield break;}
            FlightDriver.StartAndFocusVessel(game,index);while(!HighLogic.LoadedSceneIsFlight||RingworldFlight.Instance==null||!RingworldFlight.Instance.Ready)yield return null;
            yield return new WaitForSeconds(8);f=RingworldFlight.Instance;v=FlightGlobals.ActiveVessel;
            double error=((Vector3d)v.transform.position-f.Star.position-saved).magnitude;
            Debug.Log("[RingworldSmoke] GEAR reload error="+error+" parts="+v.parts.Count+" warp="+f.surfaceWarp.CanAdvance(f));
            if(v.parts.Count!=parts||!v.Landed||error>1||!f.surfaceWarp.CanAdvance(f)){fail("Gear resident roundtrip failed");yield break;}
            Debug.Log("[RingworldSmoke] GEAR natural gravity, paused save, stock warp and reload passed; peakSpeed="+peakSpeed);
        }
    }
}
#endif


