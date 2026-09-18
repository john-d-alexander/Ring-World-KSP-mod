using System;
using System.Collections.Generic;
using HarmonyLib;
using Ringworld.Core;
using UnityEngine;
namespace NivenRingworld
{
    internal sealed class ExpeditionJournal
    {
        internal readonly Dictionary<string,ResearchReceipt> Receipts=new Dictionary<string,ResearchReceipt>();
        internal readonly HashSet<string> Completed=new HashSet<string>();
        private List<ExpeditionObjective> objectives;
        internal List<ExpeditionObjective> Objectives {get{return objectives??(objectives=ResearchCatalog.Objectives());}}
        internal static bool Decode(string id,out ResearchReceipt receipt)
        {
            receipt=null;if(id==null)return false;int marker=id.IndexOf("RingworldV2_",StringComparison.Ordinal),experimentEnd=id.IndexOf('@');
            if(marker<0||experimentEnd<=0||experimentEnd>=marker)return false;
            string[] parts=id.Substring(marker+12).Split(new[]{'_'},3);if(parts.Length!=3||parts[2].Length==0)return false;
            receipt=new ResearchReceipt{Subject=id,Experiment=id.Substring(0,experimentEnd),Zone=parts[0],Category=parts[1],Location=parts[1]+"_"+parts[2]};return true;
        }
        internal void Receive(string id)
        {
            ResearchReceipt receipt;if(!Decode(id,out receipt)||HighLogic.CurrentGame==null||HighLogic.CurrentGame.Mode==Game.Modes.SANDBOX)return;
            Receipts[id]=receipt;
            // Fixed-point evaluation allows objectives to appear in any config order.
            bool changed;do
            {
                changed=false;
                foreach(var objective in Objectives)
                {
                    if(!objective.Ready(Receipts.Values,Completed))continue;
                    if(HighLogic.CurrentGame.Mode==Game.Modes.CAREER&&(Funding.Instance==null||Reputation.Instance==null))continue;
                    Completed.Add(objective.Id);changed=true;
                    if(HighLogic.CurrentGame.Mode==Game.Modes.CAREER)
                    {
                        var difficulty=HighLogic.CurrentGame.Parameters.Career;
                        Funding.Instance.AddFunds(objective.Funds*difficulty.FundsGainMultiplier,TransactionReasons.Progression);
                        Reputation.Instance.AddReputation((float)(objective.Reputation*difficulty.RepGainMultiplier),TransactionReasons.Progression);
                    }
                    ScreenMessages.PostScreenMessage("Ringworld expedition: "+objective.Title+" completed",8,ScreenMessageStyle.UPPER_CENTER);
                    Debug.Log("[NivenRingworld] Expedition milestone: "+objective.Id);
                }
            }while(changed);
        }
        internal void Load(ConfigNode node)
        {
            Receipts.Clear();Completed.Clear();
            foreach(string id in node.GetValues("researchReceived")){ResearchReceipt r;if(Decode(id,out r))Receipts[id]=r;}
            foreach(string id in node.GetValues("objectiveCompleted"))Completed.Add(id);
        }
        internal void Save(ConfigNode node)
        {foreach(string id in Receipts.Keys)node.AddValue("researchReceived",id);foreach(string id in Completed)node.AddValue("objectiveCompleted",id);}
        internal void Draw(Vessel vessel)
        {
            GUILayout.Label("EXPEDITION RESEARCH");
            Settings settings;ResearchLocation location;
            if(RingScience.Resolve(vessel,out settings,out location))
                GUILayout.Label("Research location: "+location.Name+" / "+location.Zone+"\nScience value: "+ResearchCatalog.Multiplier(location).ToString("0.#")+"× stock experiment value");
            GUILayout.Label("Use stock instruments, crew/EVA reports and surface samples. Transmit or recover data to record progress. Keeping a report aboard is not enough.");
            if(HighLogic.CurrentGame.Mode==Game.Modes.SANDBOX)GUILayout.Label("Sandbox preview: research rewards and progress are active in Science and Career.");
            GUILayout.Label(Receipts.Count+" experiment subjects returned / "+Completed.Count+" milestones completed");
            foreach(var goal in Objectives)
            {
                bool done=Completed.Contains(goal.Id),locked=goal.Requires!=""&&!Completed.Contains(goal.Requires);
                GUILayout.Label((done?"[Complete] ":locked?"[Locked] ":"[Research] ")+goal.Title+" ("+goal.Progress(Receipts.Values)+"/"+goal.Count+")");
                if(done)continue;
                GUILayout.Label(goal.Description);
                if(locked){var prerequisite=Objectives.Find(o=>o.Id==goal.Requires);GUILayout.Label("Requires: "+(prerequisite!=null?prerequisite.Title:goal.Requires));}
                if(HighLogic.CurrentGame.Mode==Game.Modes.CAREER)GUILayout.Label("Base reward: "+goal.Funds.ToString("N0")+" funds, "+goal.Reputation.ToString("0.#")+" reputation (difficulty and strategies apply)");
            }
            GUILayout.Label("DESTINATION ATLAS");
            var f=RingworldFlight.Instance;
            if(f!=null&&f.Settings!=null)foreach(var landmark in f.Settings.Terrain.Landmarks)
                GUILayout.Label(landmark.Name+" — spinward "+(landmark.Along/1000).ToString("N0")+" km; across "+(landmark.Across/1000).ToString("N0")+" km");
        }
    }
    [HarmonyPatch(typeof(ResearchAndDevelopment),"SubmitScienceData",new[]{typeof(float),typeof(float),typeof(ScienceSubject),typeof(float),typeof(ProtoVessel),typeof(bool)})]
    internal static class RingResearchReceived
    {
        private static void Postfix(ScienceSubject subject,float __result)
        {if(__result>0&&subject!=null&&RingworldScenario.Instance!=null)RingworldScenario.Instance.Research.Receive(subject.id);}
    }
}
