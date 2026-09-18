using System;
using System.Collections.Generic;
using Ringworld.Core;
static class ResearchTests
{
    internal static void Run(Action<bool,string> check)
    {
        var g=new RingGeometry(new RingParameters());var terrain=new TerrainGenerator(g);
        var arrival=terrain.Landmarks[0];
        var landed=ResearchRegions.Locate(terrain,g.Position(arrival.Along,arrival.Across,arrival.Height+3),true,false,true,0);
        check(landed.Category=="landmark"&&landed.Id==arrival.Id&&landed.Zone=="surface","science landmark landing");
        var high=ResearchRegions.Locate(terrain,g.Position(arrival.Along,arrival.Across,800000),false,false,true,0);
        check(high.Category=="orbit"&&high.Zone=="highspace","high space independent of surface biome");
        var air=ResearchRegions.Locate(terrain,g.Position(arrival.Along,arrival.Across,1000),false,false,true,0);
        check(air.Zone=="lowair"&&air.Key!=landed.Key,"airborne not landed science");
        check(ResearchRegions.Locate(terrain,g.Position(0,0,5000000),false,false,true,0)==null,"ordinary solar space excluded");
        foreach(int side in new[]{-1,1})
        {
            var wall=ResearchRegions.Locate(terrain,g.Position(0,side*g.P.Width/2,g.P.WallHeight+3),true,false,true,0);
            check(wall.Category=="wall"&&wall.Zone=="surface","wall top science");
        }
        var terminal=terrain.Landmarks.Find(l=>l.Id=="rim");
        check(ResearchRegions.Locate(terrain,g.Position(terminal.Along,terminal.Across,terminal.Height+3),true,false,true,0).Category=="landmark","terminal not confused with wall");
        for(int i=0;i<20;i++)foreach(double time in new[]{0.0,5000.0,1000000.0})
        {
            double a=(i*2*Math.PI/20+2*Math.PI*time/(20*g.P.DaySeconds))*g.P.Radius;
            var panel=ResearchRegions.Locate(terrain,g.Position(a,0,g.P.Radius*(1-46.0/153)),false,false,true,time);
            check(panel!=null&&panel.Category=="panel"&&panel.Id=="panel"+(i+1),"rotating panel identity");
        }
        var receipts=new List<ResearchReceipt>();var completed=new HashSet<string>();
        var goal=new ExpeditionObjective{Id="test",Requires="first",Category="biome",Zone="surface",Count=2};
        receipts.Add(new ResearchReceipt{Location="Forest",Category="biome",Zone="surface",Experiment="crewReport"});
        receipts.Add(new ResearchReceipt{Location="Forest",Category="biome",Zone="surface",Experiment="evaReport"});
        check(goal.Progress(receipts)==1,"two experiments do not fake two locations");
        receipts.Add(new ResearchReceipt{Location="Desert",Category="biome",Zone="lowair",Experiment="crewReport"});
        check(goal.Progress(receipts)==1,"wrong situation does not advance goal");
        receipts.Add(new ResearchReceipt{Location="Desert",Category="biome",Zone="surface",Experiment="crewReport"});
        check(!goal.Ready(receipts,completed),"objective prerequisite respected");completed.Add("first");
        check(goal.Ready(receipts,completed),"objective ready after qualifying deliveries");completed.Add("test");
        check(!goal.Ready(receipts,completed),"completed objective cannot pay again");
        goal.Experiment="surfaceSample";check(goal.Progress(receipts)==0,"experiment restriction respected");
    }
}
