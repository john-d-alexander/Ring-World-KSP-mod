namespace NivenRingworld
{
    /// <summary>Read-only integration point for flight instruments and other mods.</summary>
    public static class RingworldSurfaceApi
    {
        public const int Version=2;

        /// <summary>
        /// Returns false outside a restored, unpacked ring frame, including transfers.
        /// Vectors use the current Unity world axes, not stock orbital axes.
        /// Call on Unity's main thread and refresh every physics tick; never cache a frame.
        /// </summary>
        public static bool TryGetSurfaceState(Vessel vessel,out RingworldSurfaceState state)
        {
            state=default(RingworldSurfaceState);
            var flight=RingworldFlight.Instance;
            if(flight==null||!flight.enabled||flight.Settings==null||vessel==null||vessel.packed||!flight.Owns(vessel))return false;
            var geometry=flight.Settings.Geometry;
            var position=flight.Position(vessel);var coordinates=geometry.Coordinates(position);
            var terrain=flight.Settings.Terrain.Sample(coordinates.Along,coordinates.Across);
            state=new RingworldSurfaceState
            {
                HostBody=flight.Star,RingId=flight.Settings.RingId,RingName=flight.Settings.RingName,Center=flight.Center,
                Along=coordinates.Along,Across=coordinates.Across,
                Altitude=coordinates.Altitude,
                TerrainElevation=terrain.Height,WaterElevation=terrain.WaterHeight,OverWater=terrain.Wet,
                SurfaceRelativeVelocity=ConvertVector.Ksp(flight.Velocity(vessel)),
                SurfaceUp=ConvertVector.Ksp(geometry.Up(position)),
                FrameEpoch=flight.FrameEpoch,
                TangentialSpeed=geometry.P.Omega*geometry.P.Radius,
                Biome=terrain.Biome.ToString()
            };
            return true;
        }
    }

    /// <summary>Metres, seconds and metres/second. Altitude is above the ring datum.</summary>
    public struct RingworldSurfaceState
    {
        public CelestialBody HostBody { get; internal set; }
        public string RingId { get; internal set; }
        public string RingName { get; internal set; }
        public Vector3d Center { get; internal set; }
        public double Along { get; internal set; }
        public double Across { get; internal set; }
        public double Altitude { get; internal set; }
        public double TerrainElevation { get; internal set; }
        public double WaterElevation { get; internal set; }
        public bool OverWater { get; internal set; }
        public Vector3d SurfaceRelativeVelocity { get; internal set; }
        public Vector3d SurfaceUp { get; internal set; }
        public double FrameEpoch { get; internal set; }
        public double TangentialSpeed { get; internal set; }
        public string Biome { get; internal set; }
    }
}
