using UnityEngine;

namespace NivenRingworld
{
    [KSPAddon(KSPAddon.Startup.TrackingStation,false)]
    public sealed class TrackingRing : MonoBehaviour
    {
        public void Start(){gameObject.AddComponent<ScaledRing>();}
    }
}
