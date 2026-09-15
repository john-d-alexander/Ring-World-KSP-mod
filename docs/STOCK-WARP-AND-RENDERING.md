# Stock warp, contact and rendering — 0.7

## Stock controls

Use KSP's top-left warp controls or comma/period. The separate Ringworld rate buttons have been removed. Ring-surface high warp uses KSP's native `TimeWarp` rate and universal clock; it no longer manually increments UT in an ordinary physics frame. Other stellar bodies and ordinary stock orbital propagation therefore use the same accelerated clock.

Resting expedition craft are packed through KSP's rails lifecycle, but their position/rotation are held in the ring's existing rotating chart. Scoped OrbitDriver/VesselPrecalculate patches prevent the Sun's Kepler orbit from carrying an anchored ring craft away. On return to 1x, the same pose is restored with zero ring-relative velocity. Terrain generation and lighting continue while those craft are packed.

Entry requires actual ground contact, dry terrain, speed at most .25 m/s, angular speed at most .05 rad/s, no throttle, and no nearby unpacked vessel outside the expedition. This is surface anchoring, not accelerated flight integration: rails warp while flying over the ring and low/physics warp inside the expedition are not enabled. Use normal flight until contact is settled. Ordinary stock orbital warp outside the arrival region remains available. The existing maximum surface-warp setting limits native ring-surface rates, default 1,000x, adjustable through 10,000x.

Stock rails behaviour governs other vessels and background systems; compatibility with arbitrary resource/life-support mods and scene changes during anchored warp has not been certified. Do not interpret the new clock integration as a bespoke full simulation of every third-party module.

## Contact and camera

The terrain collision shell now has a dedicated friction material: static 1, dynamic .85, maximum friction combination, zero bounce. This addresses the default-material sliding observed on ordinary ground. It does not flatten slopes or stop a moving craft by teleporting it. Unity's [physics material documentation](https://docs.unity.cn/2019.1/Documentation/Manual/class-PhysicMaterial.html) explains how the two contact materials are combined.

Camera wobble/effects suppression now includes ground-contact craft and nearly stationary craft close to the floor, in addition to EVAs. This covers a surviving craft lying on its side. Actual contact movement and collision clearance still affect the camera; the change does not conceal structural breakup or promise stability for every vehicle.

## Coarse geometry, map and chunk publication

The old global ring used straight chords whose midpoints intruded above the true curved floor. At the default radius and 8,192 segments, that inward error is about 1.1 km. The fallback floor now uses an outward radial offset derived from `R * (sec(pi/segments) - 1)`, plus the minimum terrain depth, hull thickness and a float roundoff margin of max(512 m, radius*1e-6). The extra depth belongs only to the coarse visual fallback; local collision thickness is unchanged. The chord lies below the minimum supported terrain instead of cutting through the local view. This also separates fallback floor geometry from rendered terrain.

The coarse ring has a closed cross-section, including wall tops, outer faces and underside. Local rim wall colliders/render meshes also have inner/outer faces, caps and finite thickness, with separate face normals. Ground tiles stop at the ribbon edge instead of extending a phantom wall-height plateau outside it; LOD floor sampling no longer turns the last terrain row into a ramp up the wall. Wall thickness currently follows the configured structural thickness (100 m by default); this is a modelling choice, not a claimed canonical dimension. Distant walls use the global angular mesh and nearby walls use local tile geometry.

In map view from below the habitat, interior terrain and its seam skirts are hidden and the closed coarse underside owns the view. Skirts are crack covers, not visible underside structures. From inside the habitat, terrain remains visible.

Each completed LOD block becomes visible in the same update, after positioning. Obsolete blocks are retired rather than retained over new child blocks, avoiding parent/child overlap. The old behaviour held all new blocks invisible until the whole queue drained. The budget remains one block per frame by default; generation is bounded work on the main thread, not a new background worker. During movement or rebuilding, some areas may temporarily show the coarse fallback as their detailed blocks arrive.

## Re-entry effects

Stock AerodynamicsFX read Sun-relative `srf_velocity` and speed. The scoped replacement supplies ring-air-relative velocity and magnitude, consistent with the mod's atmosphere model. Effect direction is opposite that velocity. A nearly radial, spin-matched descent should therefore trail generally upward; a real crosswind, tangential mismatch or Coriolis-induced lateral motion can tilt it. [NASA's relative-velocity explanation](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/relative-velocity/) describes why velocity relative to the air is the relevant quantity.

The combined development reference is [BIOME-ASSET-CATALOG.md](BIOME-ASSET-CATALOG.md), including equations, placement gates, budgets, current/planned assets and measured frequency ranges. It is documentation; editing it does not itself change runtime generation.


An unprotected test at solar circular speed without matching the ring produced 376.9 km/s air-relative speed. The effects direction was almost entirely tangential, and part explosions began 0.08 simulation seconds after first nonzero atmospheric density. The command pod was destroyed shortly afterward. The high-altitude fade means first atmospheric contact need not destroy the craft in the very first physics step. Exact heating numbers at this speed are outputs of KSP's stock model, not a validated plasma simulation. See [VALIDATION.md](VALIDATION.md) for the fixture and limitations.
