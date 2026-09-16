using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Ringworld.Core;
namespace NivenRingworld
{
    [HarmonyPatch(typeof(ModuleParachute),"FixedUpdate")]
    internal static class RingParachutePressure
    {
        internal static double Pressure(Vector3d position,CelestialBody body,ModuleParachute chute)
        {
            if(!RingAir.Applies(chute.vessel))return FlightGlobals.getStaticPressure(position,body);
            var f=RingworldFlight.Instance;
            return new RingAtmosphere(f.Settings.Geometry).Sample(ConvertVector.Core(position-f.Star.position)).PressureKPa;
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var original=AccessTools.Method(typeof(FlightGlobals),"getStaticPressure",new[]{typeof(Vector3d),typeof(CelestialBody)});
            foreach(var instruction in instructions)
            {
                if(instruction.Calls(original))
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);instruction.labels.Clear();yield return load;
                    instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingParachutePressure),"Pressure");
                }
                yield return instruction;
            }
        }
    }
    [HarmonyPatch(typeof(ModuleParachute),"ShouldDeploy")]
    internal static class RingParachuteAltitude
    {
        private static bool Prefix(ModuleParachute __instance,ref bool __result)
        {
            if(!RingAir.Applies(__instance.vessel))return true;
            var f=RingworldFlight.Instance;var p=f.Settings.Geometry.Coordinates(ConvertVector.Core((Vector3d)__instance.part.transform.position-f.Star.position));
            var ground=f.Settings.Terrain.Sample(p.Along,p.Across);
            __result=p.Altitude-(ground.Wet?ground.WaterHeight:ground.Height)<__instance.deployAltitude;return false;
        }
    }
}
