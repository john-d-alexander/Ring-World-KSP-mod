using UnityEngine;

namespace NivenRingworld
{
    // Presentation only: physics and orbital state change reference frames immediately.
    internal static class RingCameraBlend
    {
        private static FlightCamera camera;
        private static Vessel vessel;
        private static Vector3 startOffset, nativePosition;
        private static Quaternion startRotation, nativeRotation;
        private static Transform parent;
        private static float started;
        private static bool active, applied;
        internal static int Completed {get;private set;}
        internal static float FirstFrameAngle {get;private set;}
        internal static void Cancel()
        {
            if(camera!=null)Restore(camera);
            active=false;vessel=null;camera=null;
        }
        internal static void Begin(Quaternion frameRotation)
        {
            var c=FlightCamera.fetch;var v=FlightGlobals.ActiveVessel;
            if(c==null||v==null||MapView.MapIsEnabled)return;
            // Capture the visible pose, including an unfinished previous handoff.
            var offset=frameRotation*(c.transform.position-v.transform.position);
            var rotation=frameRotation*c.transform.rotation;
            Restore(c);
            camera=c;vessel=v;startOffset=offset;startRotation=rotation;
            started=-1;active=true;
        }
        internal static void Restore(FlightCamera c)
        {
            if(applied&&c==camera&&c.transform.parent==parent)
            {
                c.transform.localPosition=nativePosition;
                c.transform.localRotation=nativeRotation;
            }
            applied=false;
        }
        internal static void Apply(FlightCamera c)
        {
            if(!active)return;
            if(c!=camera||vessel==null||vessel!=FlightGlobals.ActiveVessel||MapView.MapIsEnabled||!HighLogic.LoadedSceneIsFlight)
            {active=false;return;}
            bool first=started<0;if(first)started=Time.realtimeSinceStartup;
            float t=Mathf.Clamp01((Time.realtimeSinceStartup-started)/2f);
            if(t>=1){active=false;Completed++;return;}
            t=t*t*(3-2*t);
            parent=c.transform.parent;nativePosition=c.transform.localPosition;nativeRotation=c.transform.localRotation;
            var targetOffset=c.transform.position-vessel.transform.position;
            // Spherical interpolation stays outside the vessel during large rotations.
            c.transform.position=vessel.transform.position+Vector3.Slerp(startOffset,targetOffset,t);
            c.transform.rotation=Quaternion.Slerp(startRotation,c.transform.rotation,t);
            if(first)FirstFrameAngle=Quaternion.Angle(startRotation,c.transform.rotation);
            applied=true;
        }
    }
}
