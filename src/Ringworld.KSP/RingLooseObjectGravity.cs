using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NivenRingworld
{
    // Jettisoned shrouds/fairings are physicalObjects, not vessels. Retain the
    // stock integration and drag, replacing only its body-relative gravity.
    [HarmonyPatch(typeof(FlightIntegrator), "IntegratePhysicalObjects")]
    internal static class RingLooseObjectGravity
    {
        internal static void Apply(Rigidbody body, Vector3 stockForce, ForceMode mode)
        {
            var flight=RingworldFlight.Instance;
            if(body!=null&&flight!=null&&flight.FrameInUse)
            {
                var position=ConvertVector.Core((Vector3d)body.worldCenterOfMass-flight.Center);
                if(flight.Settings.Geometry.InArrivalRegion(position,true))
                {
                    var velocity=ConvertVector.Core((Vector3d)body.velocity+Krakensbane.GetFrameVelocity());
                    stockForce=ConvertVector.Unity(flight.Acceleration(position,velocity))*(float)PhysicsGlobals.GraviticForceMultiplier;
                }
            }
            if(body!=null)body.AddForce(stockForce,mode);
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var add=AccessTools.Method(typeof(Rigidbody),"AddForce",new[]{typeof(Vector3),typeof(ForceMode)});
            bool replaced=false;
            foreach(var instruction in instructions)
            {
                if(!replaced&&instruction.Calls(add))
                {instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingLooseObjectGravity),"Apply");replaced=true;}
                yield return instruction;
            }
            if(!replaced)throw new System.InvalidOperationException("Stock loose-object gravity call was not found");
        }
    }
}
