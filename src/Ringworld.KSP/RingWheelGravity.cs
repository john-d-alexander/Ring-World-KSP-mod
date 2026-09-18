using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Ringworld.Core;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NivenRingworld
{
    // Adapt only the wheel consumers. Changing mainBody or gravityForPos globally
    // would also change FlightIntegrator's force, which the rotating frame already replaces.
    [HarmonyPatch]
    internal static class RingWheelGravity
    {
        private static Vessel cachedVessel;private static float cachedTime=-1;private static Vector3d cachedGravity;
        internal static Vector3d Gravity(Vessel vessel)
        {
            if(!StockIntegration.Applies(vessel))return vessel.gravityForPos;
            if(cachedVessel==vessel&&cachedTime==UnityEngine.Time.fixedTime)return cachedGravity;
            var f=RingworldFlight.Instance;
            cachedVessel=vessel;cachedTime=UnityEngine.Time.fixedTime;
            return cachedGravity=ConvertVector.Ksp(f.Acceleration(f.Position(vessel),new DVec()));
        }
        internal static double SurfaceGee(CelestialBody body,PartModule module)
        {
            if(module.vessel==null||!StockIntegration.Applies(module.vessel))return body.GeeASL;
            return Gravity(module.vessel).magnitude/PhysicsGlobals.GravitationalAcceleration;
        }
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach(string name in new[]{"OnStart","wheelSetup","FixedUpdate","getFixTorque","EvtAutoFrictionToggle","ActionUIUpdate"})
                yield return AccessTools.Method(typeof(ModuleWheelBase),name);
            yield return AccessTools.Method(typeof(ModuleWheels.ModuleWheelSuspension),"SuspensionSpringUpdate");
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var gravity=AccessTools.Field(typeof(Vessel),"gravityForPos");
            var gee=AccessTools.Field(typeof(CelestialBody),"GeeASL");
            foreach(var instruction in instructions)
            {
                if(instruction.LoadsField(gravity))
                {instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingWheelGravity),"Gravity");}
                else if(instruction.LoadsField(gee))
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);
                    load.labels.AddRange(instruction.labels);instruction.labels.Clear();
                    load.blocks.AddRange(instruction.blocks);instruction.blocks.Clear();yield return load;
                    instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingWheelGravity),"SurfaceGee");
                }
                yield return instruction;
            }
        }
    }
    [HarmonyPatch(typeof(ModuleWheelBase),"updateDriftFix")]
    internal static class RingWheelFrameReset
    {
        private sealed class Frame {internal bool Ring;internal double Epoch;}
        private static readonly ConditionalWeakTable<ModuleWheelBase,Frame> frames=new ConditionalWeakTable<ModuleWheelBase,Frame>();
        private static readonly MethodInfo direction=AccessTools.Method(typeof(ModuleWheelBase),"getFixFwd");
        private static void Prefix(ModuleWheelBase __instance,ref Vector3 ___error,ref Vector3 ___errorLast,ref Vector3 ___fixFwd)
        {
            bool ring=StockIntegration.Applies(__instance.vessel);var frame=frames.GetValue(__instance,key=>new Frame());
            double epoch=ring?RingworldFlight.Instance.FrameEpoch:0;
            if(frame.Ring==ring&&frame.Epoch==epoch)return;
            frame.Ring=ring;frame.Epoch=epoch;
            // A landing leg may remember its launch-pad orientation. That direction
            // is meaningless after rotating the entire craft into another local frame.
            ___error=___errorLast=Vector3.zero;___fixFwd=(Vector3)direction.Invoke(__instance,null);
            if(__instance.vessel.mainBody!=null)__instance.ApplyGeeBias((float)RingWheelGravity.SurfaceGee(__instance.vessel.mainBody,__instance));
        }
    }
    [HarmonyPatch(typeof(SuspensionLoadBalancer),"FixedUpdate")]
    internal static class RingSuspensionLoad
    {
        internal static double Gee(CelestialBody body,SuspensionLoadBalancer balancer)
        {
            var vessel=balancer.Vessel;
            return StockIntegration.Applies(vessel)?RingWheelGravity.Gravity(vessel).magnitude/PhysicsGlobals.GravitationalAcceleration:body.GeeASL;
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var field=AccessTools.Field(typeof(CelestialBody),"GeeASL");
            foreach(var instruction in instructions)
            {
                if(instruction.LoadsField(field))
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);instruction.labels.Clear();yield return load;
                    instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingSuspensionLoad),"Gee");
                }
                yield return instruction;
            }
        }
    }
}

