#if RINGWORLD_SMOKE_TEST
using System;
using System.Collections;
using HarmonyLib;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal static class ResearchSmoke
    {
        internal static IEnumerator Run(RingworldFlight f,Action<string> fail)
        {
            if(ResearchAndDevelopment.Instance==null||Funding.Instance==null||Reputation.Instance==null){fail("Science/career scenario services unavailable");yield break;}
            var v=FlightGlobals.ActiveVessel;
            var ordinary=RingScience.Subject(ResearchAndDevelopment.GetExperiment("crewReport"),ExperimentSituations.InSpaceLow,v.mainBody,"","",v);
            if(ordinary.id.Contains("Ringworld")){fail("Ring science leaked into initial Kerbin orbit");yield break;}
            f.arrivalHeight=10;f.Visit();while(!f.Ready)yield return null;
            yield return new WaitForSecondsRealtime(5);
            var experiment=ResearchAndDevelopment.GetExperiment("crewReport");
            var module=v.rootPart.FindModuleImplementing<ModuleScienceExperiment>();
            if(module==null)module=(ModuleScienceExperiment)v.rootPart.AddModule("ModuleScienceExperiment");
            module.experimentID="crewReport";module.experiment=experiment;
            AccessTools.Field(typeof(ModuleScienceExperiment),"situation").SetValue(module,ScienceUtil.GetExperimentSituation(v));
            var routine=(IEnumerator)AccessTools.Method(typeof(ModuleScienceExperiment),"OnScienceCompleteDelay").Invoke(module,null);
            while(routine.MoveNext())yield return routine.Current;
            var subject=(ScienceSubject)AccessTools.Field(typeof(ModuleScienceExperiment),"subject").GetValue(module);
            if(subject==null||!subject.id.Contains("RingworldV2_")||subject.scienceCap<experiment.scienceCap*6){fail("Stock experiment did not create valuable ring subject");yield break;}
            var journal=RingworldScenario.Instance.Research;
            if(journal.Receipts.Count!=0){fail("Creating a report incorrectly awarded progress");yield break;}
            double funds=Funding.Instance.Funds;float reputation=Reputation.Instance.reputation;
            // Use the real stock delivery API, exercising the same path as recovery/transmission.
            float earned=ResearchAndDevelopment.Instance.SubmitScienceData(experiment.baseValue*experiment.dataScale,subject,1,v.protoVessel);
            if(earned<=0||!journal.Completed.Contains("first_contact")||Funding.Instance.Funds<=funds||Reputation.Instance.reputation<=reputation){fail("Science delivery / career rewards missing");yield break;}
            double paid=Funding.Instance.Funds;
            ResearchAndDevelopment.Instance.SubmitScienceData(0,subject,1,v.protoVessel);
            journal.Receive(subject.id);
            if(Funding.Instance.Funds!=paid){fail("Repeated science paid the milestone twice");yield break;}
            var saved=new ConfigNode("RESEARCH");journal.Save(saved);var loaded=new ExpeditionJournal();loaded.Load(saved);loaded.Receive(subject.id);
            if(Funding.Instance.Funds!=paid||loaded.Completed.Count!=journal.Completed.Count||loaded.Receipts.Count!=journal.Receipts.Count){fail("Journal persistence duplicated/lost rewards");yield break;}
            var subjectSave=new ConfigNode();subject.Save(subjectSave);var subjectLoad=new ScienceSubject(subjectSave);
            if(subjectLoad.title!=subject.title||subjectLoad.science!=subject.science){fail("Subject reload lost location/earned science");yield break;}
            var again=RingScience.Subject(experiment,ScienceUtil.GetExperimentSituation(v),v.mainBody,"","",v);
            if(again.id!=subject.id||again.science!=subject.science||again.scientificValue>=1){fail("Repeated subject resets exhaustion");yield break;}
            foreach(string id in new[]{"evaReport","surfaceSample","temperatureScan","barometerScan","gravityScan","seismicScan","mysteryGoo","mobileMaterialsLab","atmosphereAnalysis"})
            {
                var e=ResearchAndDevelopment.GetExperiment(id);if(e==null){fail("Stock experiment missing: "+id);yield break;}
                var item=RingScience.Subject(e,ScienceUtil.GetExperimentSituation(v),v.mainBody,"","",v);
                if(!item.title.Contains("Ringworld")||item.scienceCap<=e.scienceCap){fail("Stock science type not integrated: "+id);yield break;}
            }
            HighLogic.CurrentGame.Mode=Game.Modes.SCIENCE_SANDBOX;
            loaded=new ExpeditionJournal();loaded.Receive(subject.id);
            if(!loaded.Completed.Contains("first_contact")||Funding.Instance.Funds!=paid){fail("Science mode progression paid career currency");yield break;}
            HighLogic.CurrentGame.Mode=Game.Modes.CAREER;
            int namedStructures=0;
            foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_LANDMARK_ASSET"))
            {
                var site=f.Settings.Terrain.Landmarks.Find(l=>l.Id==n.GetValue("site"));if(site==null)continue;
                double a=site.Along+ResearchCatalog.Number(n,"along",0),b=site.Across+ResearchCatalog.Number(n,"across",0);
                var ground=f.Settings.Terrain.Sample(a,b);if(ground.Wet)continue;
                var at=new RingPoint(a,b,ground.Height+ResearchCatalog.Number(n,"aboveGround",0)+ResearchCatalog.Number(n,"height",100)/2);
                var location=ResearchCatalog.Structure(f.Settings,at,"lowair");
                if(location==null||location.Category!="structure"){fail("Structure has no research envelope: "+n.GetValue("kind"));yield break;}namedStructures++;
            }
            if(namedStructures<10){fail("Too few structure science locations checked");yield break;}
            foreach(var dialog in UnityEngine.Object.FindObjectsOfType<KSP.UI.Screens.Flight.Dialogs.ExperimentsResultDialog>())UnityEngine.Object.Destroy(dialog.gameObject);
            var cabin=v.parts.Find(part=>part.protoModuleCrew.Count>0);
            var eva=FlightEVA.fetch.spawnEVA(cabin.protoModuleCrew[0],cabin,cabin.airlock,true);
            if(eva==null){fail("Research EVA hatch blocked");yield break;}
            float deadline=Time.realtimeSinceStartup+45;
            while((eva.vessel==null||eva.vessel.packed||!f.Owns(eva.vessel))&&Time.realtimeSinceStartup<deadline)yield return null;
            if(eva.vessel==null||!f.Owns(eva.vessel)){fail("Research EVA did not join ring");yield break;}
            if(eva.OnALadder)eva.fsm.RunEvent(eva.On_ladderLetGo);
            while(!eva.vessel.Landed&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!eva.vessel.Landed){fail("Research EVA did not settle");yield break;}
            foreach(var instrument in eva.vessel.FindPartModulesImplementing<ModuleScienceExperiment>())
            {
                if(instrument.experimentID!="evaReport"&&instrument.experimentID!="surfaceSample")continue;
                var e=ResearchAndDevelopment.GetExperiment(instrument.experimentID);instrument.experiment=e;
                if(!RingScience.Available(e,ScienceUtil.GetExperimentSituation(eva.vessel),eva.vessel.mainBody,eva.vessel)){fail("Landed EVA experiment unavailable: "+e.id);yield break;}
                AccessTools.Field(typeof(ModuleScienceExperiment),"situation").SetValue(instrument,ScienceUtil.GetExperimentSituation(eva.vessel));
                var gather=(IEnumerator)AccessTools.Method(typeof(ModuleScienceExperiment),"OnScienceCompleteDelay").Invoke(instrument,null);
                while(gather.MoveNext())yield return gather.Current;
                var evaSubject=(ScienceSubject)AccessTools.Field(typeof(ModuleScienceExperiment),"subject").GetValue(instrument);
                if(evaSubject==null||!evaSubject.id.Contains("SrfLandedRingworldV2_")){fail("EVA stock subject not landed Ringworld");yield break;}
                Debug.Log("[RingworldSmoke] EVA SCIENCE "+evaSubject.id);
            }
            yield return GroundScienceSmoke.Run(f,fail);
            Debug.Log("[RingworldSmoke] RESEARCH subject="+subject.id+" science="+earned+" cap="+subject.scienceCap+" milestones="+journal.Completed.Count+" funds="+(paid-funds));
            Debug.Log("[RingworldSmoke] STRUCTURE SCIENCE locations="+namedStructures);
            Debug.Log("[RingworldSmoke] PASS stock science generation, all ten experiment subjects, delivery, career rewards, no duplicate payout, save/load, Science mode and Kerbin isolation");
        }
    }
}
#endif
