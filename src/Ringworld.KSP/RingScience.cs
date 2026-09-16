using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Ringworld.Core;
using Expansions.Serenity.DeployedScience.Runtime;
namespace NivenRingworld
{
    internal static class RingScience
    {
        internal static bool Context(Vessel v,out Settings s,out string key,out string title)
        {
            s=null;key=title=null;VesselRecord r;
            if(!RingResidence.Saved(v,out r))return false;
            var f=RingworldFlight.Instance;
            s=f!=null&&f.Settings!=null?f.Settings:Settings.Load();
            if(f==null||f.Settings==null)s.Apply(RingworldScenario.Instance.GetOptions());
            var g=new RingGeometry(s.Geometry.P);g.OrientationRadians=s.Geometry.P.Omega*r.Epoch;
            var c=g.Coordinates(r.Position);var t=s.Terrain.Sample(c.Along,c.Across);
            key="Ringworld_"+t.Biome;title="Ringworld / "+t.Biome;
            return true;
        }
        public static ScienceSubject Subject(ScienceExperiment e,ExperimentSituations situation,CelestialBody body,string biome,string display,Vessel v)
        {
            Settings s;string key,title;
            if(!Context(v,out s,out key,out title))return ResearchAndDevelopment.GetExperimentSubject(e,situation,body,biome,display);
            if(((int)e.biomeMask&(int)situation)==0){key="Ringworld_global";title="Ringworld";}
            var subject=ResearchAndDevelopment.GetExperimentSubject(e,situation,body,key,title);
            subject.title=e.experimentTitle+" — "+title+" ("+situation+")";
            subject.subjectValue=1;subject.scienceCap=e.scienceCap;subject.dataScale=e.dataScale;
            return subject;
        }
        public static bool Available(ScienceExperiment e,ExperimentSituations situation,CelestialBody body,Vessel v)
        {
            Settings s;string key,title;
            if(!Context(v,out s,out key,out title))return e.IsAvailableWhile(situation,body);
            bool air=s.Atmosphere&&(situation==ExperimentSituations.SrfLanded||situation==ExperimentSituations.FlyingLow||situation==ExperimentSituations.FlyingHigh);
            return ((int)e.situationMask&(int)situation)!=0&&(!e.requireAtmosphere||air)&&(!e.requireNoAtmosphere||!air);
        }
    }
    [HarmonyPatch]
    internal static class RingCargoSettle
    {
        private static MethodBase TargetMethod()
        {
            foreach(var type in typeof(ModuleCargoPart).GetNestedTypes(BindingFlags.Public|BindingFlags.NonPublic))if(type.Name.Contains("MakePartSettle"))return AccessTools.Method(type,"MoveNext");
            throw new InvalidOperationException("Cargo settling iterator unavailable");
        }
        public static Vector3d RelativeVelocity(Vessel v){var f=RingworldFlight.Instance;return f!=null&&f.Owns(v)?ConvertVector.Ksp(f.Velocity(v)):v.srf_velocity;}
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code,ILGenerator generator)
        {
            var field=AccessTools.Field(typeof(Vessel),"srf_velocity");
            var velocity=generator.DeclareLocal(typeof(Vector3d));
            foreach(var i in code)
            {
                bool address=i.opcode==OpCodes.Ldflda;
                if((i.opcode==OpCodes.Ldfld||address)&&Equals(i.operand,field))
                {
                    i.opcode=OpCodes.Call;i.operand=AccessTools.Method(typeof(RingCargoSettle),"RelativeVelocity");yield return i;
                    if(address){yield return new CodeInstruction(OpCodes.Stloc,velocity);yield return new CodeInstruction(OpCodes.Ldloca,velocity);}
                }
                else yield return i;
            }
        }
    }
    // Add the actual instrument's vessel to each call, including iterator bodies.
    // This avoids borrowing the active vessel for background deployed experiments.
    [HarmonyPatch]
    internal static class RingScienceCalls
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach(var t in typeof(ModuleScienceExperiment).GetNestedTypes(BindingFlags.Public|BindingFlags.NonPublic))
                if(t.Name.Contains("gatherData")||t.Name.Contains("OnScienceCompleteDelay"))yield return AccessTools.Method(t,"MoveNext");
            yield return AccessTools.Method(typeof(DeployedScienceExperiment),"Start");
            yield return AccessTools.Method(typeof(ModuleScienceExperiment),"updateModuleUI");
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code,MethodBase __originalMethod)
        {
            var subject=AccessTools.Method(typeof(ResearchAndDevelopment),"GetExperimentSubject",new[]{typeof(ScienceExperiment),typeof(ExperimentSituations),typeof(CelestialBody),typeof(string),typeof(string)});
            var available=AccessTools.Method(typeof(ScienceExperiment),"IsAvailableWhile");
            foreach(var instruction in code)
            {
                if(instruction.Calls(subject)||instruction.Calls(available))
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);instruction.labels.Clear();yield return load;
                    if(__originalMethod.DeclaringType==typeof(DeployedScienceExperiment))yield return new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(DeployedScienceExperiment),"ExperimentVessel"));
                    else if(__originalMethod.DeclaringType==typeof(ModuleScienceExperiment))yield return new CodeInstruction(OpCodes.Call,AccessTools.PropertyGetter(typeof(PartModule),"vessel"));
                    else
                    {
                        FieldInfo owner=null;foreach(var field in __originalMethod.DeclaringType.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))if(field.FieldType==typeof(ModuleScienceExperiment)){owner=field;break;}
                        if(owner==null)throw new InvalidOperationException("Science iterator owner missing");
                        yield return new CodeInstruction(OpCodes.Ldfld,owner);yield return new CodeInstruction(OpCodes.Call,AccessTools.PropertyGetter(typeof(PartModule),"vessel"));
                    }
                    instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingScience),instruction.Calls(subject)?"Subject":"Available");
                }
                yield return instruction;
            }
        }
    }
}
