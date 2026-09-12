using HarmonyLib;
using Ringworld.Core;
using UnityEngine;

namespace NivenRingworld
{
    internal static class StockIntegration
    {
        private static bool installed;
        internal static void Install()
        {
            if(installed)return;
            new Harmony("NivenRingworld.surface").PatchAll(typeof(StockIntegration).Assembly);installed=true;
            Debug.Log("[NivenRingworld] Scoped stock surface compatibility patches installed.");
        }
        internal static bool Applies(Vessel v)
        {
            var f=RingworldFlight.Instance;
            return f!=null&&f.Owns(v);
        }
    }
    // The stock method dereferences the body's PQS on contact; the Sun has no PQS.
    // Keep custom-floor contact distinct from stock planetary LANDED/anchored state.
    [HarmonyPatch(typeof(Vessel),nameof(Vessel.checkLanded))]
    internal static class RingLandingPatch
    {
        private static bool Prefix(Vessel __instance,ref bool __result)
        {
            if(!StockIntegration.Applies(__instance))return true;
            __instance.Landed=false;__result=false;return false;
        }
    }
    [HarmonyPatch(typeof(VesselPrecalculate),"CalculatePhysicsStats")]
    internal static class RingOrientationPatch
    {
        private static void Postfix(VesselPrecalculate __instance)
        {
            var v=__instance.Vessel;if(!StockIntegration.Applies(v))return;
            var f=RingworldFlight.Instance;var up=f.Settings.Geometry.Up(f.Position(v));
            v.upAxis=ConvertVector.Ksp(up);v.north=Vector3d.up;
            v.east=ConvertVector.Ksp(DVec.Cross(new DVec(0,1,0),up));
        }
    }
    [HarmonyPatch(typeof(FlightCamera),nameof(FlightCamera.GetCameraFoR))]
    internal static class RingCameraPatch
    {
        private static bool Prefix(FoRModes mode,ref Quaternion __result)
        {
            var v=FlightGlobals.ActiveVessel;if(!StockIntegration.Applies(v))return true;
            if(mode!=FoRModes.SRF_NORTH&&mode!=FoRModes.SRF_HDG&&mode!=FoRModes.SRF_VEL)return true;
            var f=RingworldFlight.Instance;
            __result=Quaternion.LookRotation(Vector3.up,ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(v))));return false;
        }
    }
}
