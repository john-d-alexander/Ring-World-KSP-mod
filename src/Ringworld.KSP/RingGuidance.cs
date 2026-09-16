using HarmonyLib;
using UnityEngine;
namespace NivenRingworld
{
    [HarmonyPatch(typeof(VesselAutopilot),"SetAutopilot")]
    internal static class RingGuidance
    {
        internal static bool Direction(Vessel v,VesselAutopilot.AutopilotMode mode,out Vector3 direction)
        {
            direction=Vector3.zero;Vector3 velocity;
            bool valid=RingNavigation.TryVelocity(v,out velocity);
            var f=RingworldFlight.Instance;var up=ConvertVector.Unity(f.Settings.Geometry.Up(f.Position(v)));
            switch(mode)
            {
                case VesselAutopilot.AutopilotMode.Prograde: direction=valid?velocity:Vector3.zero;return true;
                case VesselAutopilot.AutopilotMode.Retrograde: direction=valid?-velocity:Vector3.zero;return true;
                case VesselAutopilot.AutopilotMode.RadialOut: direction=up;return true;
                case VesselAutopilot.AutopilotMode.RadialIn: direction=-up;return true;
                case VesselAutopilot.AutopilotMode.Normal: direction=valid?Vector3.Cross(up,velocity):Vector3.zero;return true;
                case VesselAutopilot.AutopilotMode.Antinormal: direction=valid?-Vector3.Cross(up,velocity):Vector3.zero;return true;
                default:return false;
            }
        }
        private static bool Prefix(VesselAutopilot __instance,bool initialize,ref bool __result)
        {
            var v=__instance.Vessel;if(!StockIntegration.Applies(v))return true;
            Vector3 direction;if(!Direction(v,__instance.Mode,out direction))return true;
            if(direction.sqrMagnitude>.01f)__instance.SAS.SetTargetOrientation(direction,initialize);
            else if(initialize)__instance.SAS.SetTargetOrientation(v.ReferenceTransform.up,true);
            __result=true;return false;
        }
    }
    [HarmonyPatch(typeof(VesselAutopilot),"VectorLockInvalid")]
    internal static class RingGuidanceVelocityGuard
    {
        private static bool Prefix(Vessel v,float threshold,ref bool __result)
        {
            if(!StockIntegration.Applies(v))return true;
            Vector3 speed;__result=!RingNavigation.TryVelocity(v,out speed)||speed.sqrMagnitude<threshold;return false;
        }
    }
}
