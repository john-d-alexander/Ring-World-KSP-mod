using System;

namespace Ringworld.Core
{
    public static class TerrainExploration
    {
        // Bounded, repeatable site search. This changes the destination, never the world seed.
        public static bool TryChoose(TerrainGenerator terrain, int selectionSeed, out RingPoint site)
        {
            var random = new Random(selectionSeed);
            var g = terrain.Geometry;
            for (int attempt = 0; attempt < 128; attempt++)
            {
                double along = random.NextDouble() * g.P.Circumference;
                double across = (random.NextDouble() - .5) * (g.P.Width - 10000);
                var sample = terrain.Sample(along, across);
                if (sample.Wet || sample.Biome == Biome.Rimwall || sample.Biome == Biome.Ruins || sample.Biome == Biome.Road) continue;
                bool landmark = false;
                foreach (var l in terrain.Landmarks)
                {
                    double da = g.AlongDistance(along, l.Along), db = across - l.Across;
                    if (da * da + db * db < (l.Radius + 2000) * (l.Radius + 2000)) { landmark = true; break; }
                }
                if (landmark) continue;
                // Avoid steep local slopes; this is a descent site, not guaranteed landing clearance.
                if (Math.Abs(terrain.Sample(along + 50, across).Height - sample.Height) > 10 ||
                    Math.Abs(terrain.Sample(along - 50, across).Height - sample.Height) > 10 ||
                    Math.Abs(terrain.Sample(along, across + 50).Height - sample.Height) > 10 ||
                    Math.Abs(terrain.Sample(along, across - 50).Height - sample.Height) > 10) continue;
                site = new RingPoint(along, across, sample.Height);
                return true;
            }
            site = new RingPoint();
            return false;
        }
    }
}
