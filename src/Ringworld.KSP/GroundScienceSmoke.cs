#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Ringworld.Core;
using UnityEngine;
using Expansions.Serenity.DeployedScience.Runtime;
namespace NivenRingworld
{
    internal static class GroundScienceSmoke
    {
        internal static readonly List<Guid> Ids=new List<Guid>();
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            var g=f.Settings.Geometry;var at=g.Coordinates(f.Position(FlightGlobals.ActiveVessel));
            var modules=new List<ModuleGroundSciencePart>();int index=0;
            foreach(string name in new[]{"DeployedCentralStation","DeployedRTG","DeployedRTG","DeployedGoExOb"})
            {
                double a=at.Along+25+index++*4,b=at.Across;
                var pos=g.Position(a,b,f.Settings.Terrain.Sample(a,b).Height+1);
                var part=ProtoVessel.CreatePartNode(name,ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState));
                var seedOrbit=new Orbit();double elapsed=Planetarium.GetUniversalTime()-f.FrameEpoch;
                seedOrbit.UpdateFromStateVectors(ConvertVector.Orbit(ConvertVector.Ksp(g.ToInertialPosition(pos,elapsed))),ConvertVector.Orbit(ConvertVector.Ksp(g.ToInertialVelocity(pos,new DVec(),elapsed))),f.Star,Planetarium.GetUniversalTime());
                var node=ProtoVessel.CreateVesselNode("Ring test "+name,VesselType.DeployedGroundPart,seedOrbit,0,new[]{part});
                node.SetValue("sit","LANDED");node.SetValue("landed",true);node.SetValue("landedAt","Ringworld");
                string id=Guid.Parse(node.GetValue("pid")).ToString();
                RingworldScenario.Instance.Vessels[id]=new VesselRecord{Id=id,Position=pos,Epoch=f.FrameEpoch,Rotation=Quaternion.FromToRotation(Vector3.up,ConvertVector.Unity(g.Up(pos))),Landed=true};
                var proto=HighLogic.CurrentGame.AddVessel(node);var vessel=proto.vesselRef;Ids.Add(vessel.id);
                float end=Time.realtimeSinceStartup+35;
                while((!vessel.loaded||vessel.packed||!f.Owns(vessel))&&Time.realtimeSinceStartup<end)yield return null;
                if(!vessel.loaded||vessel.packed||!f.Owns(vessel)){fail("Ground part did not join frame: "+name);yield break;}
                yield return new WaitForSeconds(4);
                var module=vessel.rootPart.FindModuleImplementing<ModuleGroundSciencePart>();
                if(module==null){fail("Missing deployed module "+name);yield break;}
                // Synthetic snapshots omit inventory deployment's power setup.
                // Match the shipped centralStation/rtg/gooObservation.cfg budgets.
                module.PowerUnitsProduced=name=="DeployedRTG"?1:0;
                module.ActualPowerUnitsProduced=module.PowerUnitsProduced;
                module.PowerUnitsRequired=name=="DeployedRTG"?0:1;
                Debug.Log("[RingworldSmoke] DEPLOY BEGIN "+name+" contact="+vessel.rootPart.GroundContact+" speed="+f.Velocity(vessel).Length+" clearance="+f.SurfaceClearance(vessel));
                AccessTools.Field(typeof(ModuleGroundPart),"beingDeployed").SetValue(module,true);
                var routine=(IEnumerator)AccessTools.Method(typeof(ModuleGroundPart),"MakePartKinematic").Invoke(module,null);
                while(routine.MoveNext())yield return routine.Current;
                yield return new WaitForSeconds(2);
                if(!module.DeployedOnGround||!vessel.Landed||!vessel.rootPart.PermanentGroundContact){fail("Ground part failed stock anchoring: "+name);yield break;}
                modules.Add(module);Debug.Log("[RingworldSmoke] DEPLOYED "+name+" speed="+f.Velocity(vessel).Length);
            }
            var control=(ModuleGroundExpControl)modules[0];
            if(control.ScienceClusterData==null)DeployedScience.Instance.RegisterCluster(control,modules.GetRange(1,modules.Count-1));
            var cluster=control.ScienceClusterData;cluster.UpdateCluster(control,true,modules);
            yield return new WaitForSeconds(3);
            var experiment=UnityEngine.Object.FindObjectOfType<DeployedScienceExperiment>();
            if(experiment==null||!experiment.ExperimentSituationValid||!cluster.IsPowered||cluster.PowerAvailable<2||cluster.PowerRequired!=2){fail("Deployed science not powered/available: "+cluster.PowerAvailable+"/"+cluster.PowerRequired);yield break;}
            var subject=(ScienceSubject)AccessTools.Field(typeof(DeployedScienceExperiment),"subject").GetValue(experiment);
            if(subject==null||!subject.id.Contains("Ringworld_")){fail("Deployed subject is solar science");yield break;}
            Debug.Log("[RingworldSmoke] DEPLOYED SCIENCE subject="+subject.id+" power="+cluster.PowerAvailable+" required="+cluster.PowerRequired);
        }
    }
}
#endif
