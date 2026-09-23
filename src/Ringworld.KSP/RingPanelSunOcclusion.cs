using System;
using HarmonyLib;
namespace NivenRingworld
{
    [HarmonyPatch(typeof(SunFlare),"LateUpdate")]
    internal static class RingPanelSunOcclusion
    {
        // Stock flare occlusion only considers celestial-body spheres. The ring's
        // shadow squares are meshes, so apply their existing daylight mask too.
        private static void Postfix(SunFlare __instance)
        {
            var f=RingworldFlight.Instance;
            if(f==null||!f.FrameInUse||MapView.MapIsEnabled||__instance.sun!=f.Star||__instance.sunFlare==null||FlightCamera.fetch==null)return;
            var camera=FlightCamera.fetch.mainCamera;if(camera==null)return;
            var p=ConvertVector.Core((Vector3d)camera.transform.position-f.Center);var g=f.Settings.Geometry;
            var c=g.Coordinates(p);
            if(Math.Abs(c.Across)>g.P.Width/2||g.P.Radius-c.Altitude<=g.P.Radius*(46.0/153.0))return;
            // LateUpdate recomputes stock brightness each frame, so no saved global
            // state is left behind on daybreak, a map switch or scene teardown.
            __instance.sunFlare.brightness*=(float)g.Daylight(c.Along,Planetarium.GetUniversalTime());
        }
    }
}
