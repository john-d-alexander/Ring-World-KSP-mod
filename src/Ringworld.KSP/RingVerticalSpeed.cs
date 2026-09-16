using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Ringworld.Core;
namespace NivenRingworld
{
    [HarmonyPatch(typeof(KSP.UI.Screens.Flight.VerticalSpeedGauge),"LateUpdate")]
    internal static class RingVerticalSpeed
    {
        internal static double Speed(Vessel vessel)
        {
            if(!StockIntegration.Applies(vessel))return vessel.verticalSpeed;
            var f=RingworldFlight.Instance;
            return DVec.Dot(f.Velocity(vessel),f.Settings.Geometry.Up(f.Position(vessel)));
        }
        // Preserve the native gauge response and sink-rate warning hysteresis.
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var field=AccessTools.Field(typeof(Vessel),"verticalSpeed");
            foreach(var instruction in instructions)
            {
                if(instruction.LoadsField(field))
                {instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingVerticalSpeed),"Speed");}
                yield return instruction;
            }
        }
    }
}
