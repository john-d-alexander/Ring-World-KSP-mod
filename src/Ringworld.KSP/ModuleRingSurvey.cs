using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens.Flight.Dialogs;

namespace NivenRingworld
{
    public sealed class ModuleRingSurvey : PartModule, IScienceDataContainer
    {
        private readonly List<ScienceData> data=new List<ScienceData>();
        [KSPField(guiActive=true,guiName="Stored surveys")]public int stored;
        [KSPEvent(guiActive=true,guiActiveUnfocused=true,unfocusedRange=3,guiName="Survey Ringworld")]
        public void Survey()
        {
            var controller=RingworldFlight.Instance;
            string key,title,report;
            if(controller==null||!controller.ScienceContext(vessel,out key,out title,out report))
            {ScreenMessages.PostScreenMessage("Survey within 120 m of the ring floor in expedition mode.",5,ScreenMessageStyle.UPPER_CENTER);return;}
            var experiment=ResearchAndDevelopment.GetExperiment("ringworldSurvey");
            if(experiment==null){Debug.LogError("[NivenRingworld] Missing ringworldSurvey definition");return;}
            // A unique experiment and biome tag keeps artificial surface data separate from solar science.
            var subject=ResearchAndDevelopment.GetExperimentSubject(experiment,ExperimentSituations.SrfLanded,controller.Star,"Ringworld_"+key,"Ringworld / "+title);
            if(subject==null)return;
            foreach(var item in data)if(item.subjectID==subject.id)
            {ScreenMessages.PostScreenMessage("This survey is already stored in this instrument.",4,ScreenMessageStyle.UPPER_CENTER);return;}
            subject.title="Ringworld survey: "+title;
            subject.scienceCap=30;subject.subjectValue=1;subject.dataScale=1;
            var result=new ScienceData(30,.65f,0,subject.id,subject.title,false,part.flightID);
            data.Add(result);stored=data.Count;
            if(RingworldScenario.Instance!=null)RingworldScenario.Instance.Discoveries.Add(key);
            ScreenMessages.PostScreenMessage(report+" Survey stored; transmit using an antenna or recover the instrument.",8,ScreenMessageStyle.UPPER_CENTER);
        }
        [KSPAction("Survey Ringworld")]public void SurveyAction(KSPActionParam p){Survey();}
        [KSPEvent(guiActive=true,guiName="Review ring surveys")]
        public void ReviewData()
        {if(data.Count==0)ScreenMessages.PostScreenMessage("No stored ring surveys.",3,ScreenMessageStyle.UPPER_CENTER);else ReviewDataItem(data[0]);}
        public void ReviewDataItem(ScienceData item)
        {
            var page=new ExperimentResultDialogPage(part,item,.65f,0,false,"",true,null,
                DumpData,d=>{},Transmit,d=>ScreenMessages.PostScreenMessage("Transfer this survey to a science container for laboratory processing.",5,ScreenMessageStyle.UPPER_CENTER));
            ExperimentsResultDialog.DisplayResult(page);
        }
        private void Transmit(ScienceData item)
        {
            foreach(var transmitter in vessel.FindPartModulesImplementing<ModuleDataTransmitter>())
            {
                if(!transmitter.CanTransmit())continue;
                transmitter.TransmitData(new List<ScienceData>{item});DumpData(item);return;
            }
            ScreenMessages.PostScreenMessage("No usable transmitter. Keep the data or transfer it to a return vessel.",5,ScreenMessageStyle.UPPER_CENTER);
        }
        public ScienceData[] GetData(){return data.ToArray();}
        public int GetScienceCount(){return data.Count;}
        public bool IsRerunnable(){return true;}
        public void DumpData(ScienceData item){data.Remove(item);stored=data.Count;}
        public void ReturnData(ScienceData item)
        {if(item!=null&&!data.Contains(item)){data.Add(item);stored=data.Count;}}
        public override void OnLoad(ConfigNode node)
        {base.OnLoad(node);data.Clear();foreach(var n in node.GetNodes("ScienceData"))data.Add(new ScienceData(n));stored=data.Count;}
        public override void OnSave(ConfigNode node)
        {base.OnSave(node);foreach(var d in data)d.Save(node.AddNode("ScienceData"));}
        public override string GetInfo(){return "Ringworld surface survey. 30 science per site/biome before diminishing returns. 65% transmission efficiency. Reusable.";}
    }
}
