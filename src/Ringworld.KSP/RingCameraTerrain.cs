using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NivenRingworld
{
    [HarmonyPatch(typeof(FlightCamera),"UpdateCameraTransform")]
    internal static class RingCameraTerrainPatch
    {
        private static FlightCamera constrainedCamera;
        private static Transform constrainedParent;
        private static Vector3 unconstrainedPosition,appliedPosition;
        private static int constrainedFrame=-10;
        private static void Prefix(FlightCamera __instance)
        {
            // Collision correction is a presentation offset. Feeding it back into
            // stock camera interpolation/look rotation creates a frame-to-frame loop.
            if(__instance==constrainedCamera&&Time.frameCount==constrainedFrame+1&&
                __instance.GetPivot().parent==constrainedParent&&
                (__instance.transform.localPosition-appliedPosition).sqrMagnitude<1e-8f)
                __instance.transform.localPosition=unconstrainedPosition;
            RingCameraBlend.Restore(__instance);
        }
        private static void Postfix(FlightCamera __instance){RingCameraBlend.Apply(__instance);}
        internal static void ApplyClearance(FlightCamera camera,Vector3 correction)
        {
            constrainedCamera=camera;constrainedParent=camera.GetPivot().parent;constrainedFrame=Time.frameCount;
            unconstrainedPosition=camera.transform.localPosition;
            camera.transform.position+=correction;appliedPosition=camera.transform.localPosition;
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var wobble=AccessTools.Field(typeof(FlightCamera),"cameraWobbleSensitivity");
            var effects=AccessTools.Field(typeof(GameSettings),"CAMERA_FX_EXTERNAL");
            var up=AccessTools.PropertyGetter(typeof(FlightGlobals),"upAxis");
            var altitude=AccessTools.Method(typeof(FlightGlobals),"getAltitudeAtPos",new[]{typeof(Vector3)});
            var bodyAltitude=AccessTools.Method(typeof(FlightGlobals),"getAltitudeAtPos",new[]{typeof(Vector3),typeof(CelestialBody)});
            foreach(var instruction in instructions)
            {
                if(instruction.opcode==OpCodes.Ldfld&&Equals(instruction.operand,wobble))
                {instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingCameraTerrainPatch),nameof(Wobble));}
                else if(instruction.opcode==OpCodes.Ldsfld&&Equals(instruction.operand,effects))
                {instruction.opcode=OpCodes.Call;instruction.operand=AccessTools.Method(typeof(RingCameraTerrainPatch),nameof(Effects));}
                else if(instruction.Calls(altitude))instruction.operand=AccessTools.Method(typeof(RingCameraTerrainPatch),nameof(Altitude));
                else if(instruction.Calls(bodyAltitude))instruction.operand=AccessTools.Method(typeof(RingCameraTerrainPatch),nameof(BodyAltitude));
                else if(instruction.Calls(up))instruction.operand=AccessTools.Method(typeof(RingCameraTerrainPatch),nameof(Up));
                yield return instruction;
            }
        }
        internal static float Wobble(FlightCamera camera)
        {
            var v=FlightGlobals.ActiveVessel;
            return QuietCamera(v)?0:camera.cameraWobbleSensitivity;
        }
        internal static float Effects()
        {
            var v=FlightGlobals.ActiveVessel;
            return QuietCamera(v)?0:GameSettings.CAMERA_FX_EXTERNAL;
        }
        private static bool QuietCamera(Vessel v)
        {
            if(!StockIntegration.Applies(v))return false;
            if(v.isEVA)return true;
            foreach(var part in v.parts)if(part.GroundContact)return true;
            return RingworldFlight.Instance.SurfaceClearance(v)<10&&RingworldFlight.Instance.Velocity(v).Length<1;
        }
        private static Vector3d Up()
        {
            var v=FlightGlobals.ActiveVessel;if(!StockIntegration.Applies(v))return FlightGlobals.upAxis;
            var f=RingworldFlight.Instance;return ConvertVector.Ksp(f.Settings.Geometry.Up(f.Position(v)));
        }
        private static float Altitude(Vector3 position)
        {
            if(!StockIntegration.Applies(FlightGlobals.ActiveVessel))return FlightGlobals.getAltitudeAtPos(position);
            var f=RingworldFlight.Instance;
            return (float)f.Settings.Geometry.Coordinates(ConvertVector.Core((Vector3d)position-f.Center)).Altitude;
        }
        private static float BodyAltitude(Vector3 position,CelestialBody body)
        {
            return StockIntegration.Applies(FlightGlobals.ActiveVessel)?Altitude(position):FlightGlobals.getAltitudeAtPos(position,body);
        }
    }
}
