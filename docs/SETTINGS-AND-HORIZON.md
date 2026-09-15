> For the new full-circumference surface colour layer and photo mode, see [HIGH-END-VISUALS.md](HIGH-END-VISUALS.md).

> Historical v0.4 notes. Current bounds, physics and settings are in [ORBITS-WARP-AND-ASSETS.md](ORBITS-WARP-AND-ASSETS.md).

# Save settings and the long horizon — v0.4.0

Open the Ringworld panel with Alt+R, then select **Settings**. Apply changes and save the game to persist them. Settings belong to the save, not to every KSP career on the installation.

| Control | Behaviour |
|---|---|
| Terrain horizon | 200–160,000 km; farther blocks become progressively coarser |
| Quality | 8, 16 or 32 segments along each distant block edge; nearby collision resolution stays unchanged |
| Generation budget | 1–4 new blocks per frame; lower values reduce generation spikes and take longer to finish |
| Laptop preset | 160,000 km, 8 segments, one block per frame |
| Seed | Signed 32-bit integer; blank selects a random seed before the first expedition |
| Height multiplier | 0.25–3 times natural relief; named pads, ocean levels and structural dimensions retain their definitions |
| Forest density | 0–2 times tree placement density |
| Day length | 0.5–24 hours per moving shadow-square illumination cycle |
| Haze | 0.1–2 visual optical-depth multiplier; zero removes visual haze without changing physical atmospheric density |
| Cloud amount | 0–100%; zero disables cloud rendering |
| Moving weather fronts | Advected regional coverage modulation; no rain, wind forces or fluid weather simulation |

The default config has a blank seed. A new save chooses a seed once and stores the resolved integer in the scenario's OPTIONS node. Quicksaving does not regenerate the world. An existing expedition without an OPTIONS node keeps its configured legacy seed and terrain algorithm. Seed, terrain height and forest density lock once vessel records or discoveries exist, so applying visual settings cannot reshape ground beneath saved craft. Use a new save for a different generated world.

## Why the ring disappeared

The old low-altitude daytime sky forced opacity to 99.8% to hide the stock starfield. That covered the scaled ring and Sun as well. It was an exposure shortcut, not a weather event. The renderer now uses the calculated atmospheric optical depth, adjusted by the haze control. Clear overhead air transmits distant objects; longer paths near the horizon are hazier. Actual clouds can obscure the view. Some stock stars can now be visible during the day; selective starfield exposure remains unfinished.

The complete coarse ring and 160 km-high rim-wall model remain present globally. The day/night cycle already existed and is now adjustable. Ring rotation alone does not produce its night: moving shadow squares provide the illumination cycle.

## What 160,000 km means

The adaptive quadtree grows by adding coarser levels, rather than filling the entire area with fine tiles. Beyond the local rendering region, large blocks use scaled-space positions and scaled vertices. The far terrain uses opaque emission-based shading with the shared daylight multiplier. Initial generation can take many seconds; the expedition panel shows queued blocks.

This range can encompass both rim walls from the starter region. It does not imply local colliders over that area, fine mountain silhouettes at maximum range, or terrain detail around the entire circumference. Trees and rural buildings belong to nearby collision tiles; named buildings are generated near their site. Mountains and other height-field landmarks participate in terrain LOD. Small structures do not receive distant impostors in this release.

The laptop preset reduces vertex work substantially, but draw calls, Unity/KSP overhead, CPU generation, memory and graphics hardware still matter. No laptop performance guarantee is claimed. Coarse sampling can miss narrow peaks, and resolution transitions can remain visible.

The design follows multiresolution principles from [NVIDIA GPU Gems 2](https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry). Unity's [screen-relative LOD documentation](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/LOD-screenRelativeTransitionHeight.html) explains why apparent size, rather than distance alone, matters for detailed objects. This implementation remains a CPU quadtree, not GPU geometry clipmaps.

## Less artificial terrain

New saves use gradient noise with warped foothill coordinates instead of the older interpolated random-height lattice. Both near and distant meshes now use spatial colour textures. This fixes a separate palette error: interpolating a grassland-to-snow palette coordinate previously swept through unrelated desert and mountain colours, producing artificial contour bands. Actual RGB colours now interpolate spatially.

Terrain still uses analytic rivers and basin patterns, without erosion simulation. Cities retain deliberately planned layouts and primitive architecture. Further work remains on drainage, settlement impostors, material detail and lighting balance.
