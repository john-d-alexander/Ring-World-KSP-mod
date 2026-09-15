using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace NivenRingworld
{
    [HarmonyPatch(typeof(AerodynamicsFX),"Update")]
    internal static class RingAeroEffects
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            var velocity=AccessTools.Field(typeof(Vessel),"srf_velocity");
            var speed=AccessTools.PropertyGetter(typeof(Vessel),"speed");
            foreach(var i in code)
            {
                if(i.opcode==OpCodes.Ldfld&&Equals(i.operand,velocity)){i.opcode=OpCodes.Call;i.operand=AccessTools.Method(typeof(RingAeroEffects),nameof(Velocity));}
                else if(speed!=null&&i.Calls(speed)){i.opcode=OpCodes.Call;i.operand=AccessTools.Method(typeof(RingAeroEffects),nameof(Speed));}
                yield return i;
            }
        }
        internal static Vector3d Velocity(Vessel v){return RingAir.Applies(v)?ConvertVector.Ksp(RingworldFlight.Instance.Velocity(v)):v.srf_velocity;}
        internal static double Speed(Vessel v){return RingAir.Applies(v)?RingworldFlight.Instance.Velocity(v).Length:v.speed;}
    }
}
