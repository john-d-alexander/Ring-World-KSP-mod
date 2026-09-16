using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
namespace NivenRingworld
{
    [HarmonyPatch(typeof(CollisionEnhancer),"FixedUpdate")]
    internal static class RingCollisionFrame
    {
        private static readonly System.Reflection.FieldInfo previous=AccessTools.Field(typeof(CollisionEnhancer),"lastPos");
        internal static void Reset(Vessel vessel)
        {
            foreach(var part in vessel.parts)if(part!=null)
                foreach(var detector in part.GetComponentsInChildren<CollisionEnhancer>())
                {
                    previous.SetValue(detector,detector.transform.position);
                    // The origin shift has already been included in the new position.
                    detector.wasPacked=true;
                }
        }
        internal static Vector3d Up(CelestialBody body,Vector3d position,CollisionEnhancer detector)
        {
            if(detector.part==null||!StockIntegration.Applies(detector.part.vessel))return FlightGlobals.getUpAxis(body,position);
            var f=RingworldFlight.Instance;
            return ConvertVector.Ksp(f.Settings.Geometry.Up(ConvertVector.Core(position-f.Star.position)));
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var original=AccessTools.Method(typeof(FlightGlobals),"getUpAxis",new[]{typeof(CelestialBody),typeof(Vector3d)});
            foreach(var instruction in instructions)
            {
                if(instruction.Calls(original))
                {
                    var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);instruction.labels.Clear();yield return load;
                    instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingCollisionFrame),"Up");
                }
                yield return instruction;
            }
        }
    }
}
