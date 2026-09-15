# Weather, warp and night visibility — 0.9

## The cloud jumps

The old Laptop renderer rebuilt an entire cloud mesh and 384×384 opacity image once per eight real seconds. High/Ultra replaced a 128×128 weather image on the same timer. At native 1,000× warp this advanced coverage by 8,000 game seconds in one step. The GPU detail used fractional frequencies (5.03 and 17.07) on a periodically wrapped coordinate, so the wrap also introduced a discontinuity. These were implementation issues; neither observation proves exhausted RAM.

Cloud motion now evaluates universal time every rendered frame. Both the lightweight cloud sheets and volumetric renderer use a shared procedural GPU coverage field. The Laptop cloud mesh is rebuilt only after moving far enough to need a new patch; its subdivision count is reduced from 112 to 48. The eight-second texture replacements are removed. All detail frequencies tile consistently across the wrapped origin. The noise remains spatially periodic, as most tileable noise textures are; this is not a prerecorded looping animation.

Unity garbage collection can cause frame-time spikes, but collecting unreachable managed objects does not mean all physical RAM was full and then emptied. See [Unity's memory-management documentation](https://docs.unity3d.com/Manual/performance-managed-memory.html). The weather regression records frame-time percentile, GC collection count and managed-heap delta; those measurements do not substitute for a system-wide RAM/GPU/CPU profiler.

## Weather controls

Open Ringworld → Settings, apply changes, then save the game to persist them.

- **Evolving weather fronts** enables deterministic variation with universal time. It advances during native stock warp and freezes with photo mode. Fixed weather uses the baseline directly.
- **Weather baseline/cloudiness** sets the fixed state and centre of reduced-variation weather. Zero forces clear skies, no rain and no lightning.
- **Weather transition timescale**: 1/6–168 game hours; default 6 hours. This is the interval between smoothly blended seeded control points, not a repeating weather cycle.
- **Weather variation**: 0–1. Zero keeps the baseline; one allows the full generated range.
- **Storm fraction of weather range**: 0–1, default 0.25. This remaps the upper part of the weather field; it is not a promise that exactly that percentage of play time is stormy. Zero prevents generated thunderstorms. A manually fixed maximum baseline can still create a storm.
- **Visual cloud drift**: 0–100 m/s, default 8. This moves cloud features without applying a wind force to craft.
- **Rain visuals / density**: up to 48 streaks in Laptop, 144 in High and 384 in Ultra/photo, scaled by storm strength and density. Disabling rain does not remove clouds.
- **Storm lightning**: optional seeded bolt and cloud illumination, without damage. Individual events are shown at up to 10×. Higher warp uses a low-opacity rain veil and suppresses individual bolts/flashes because very short events cannot be represented reliably when each frame advances minutes.

A continuous seeded regional front and time-varying severity drive fair skies, cloud cover, rain and thunderstorms. All quality tiers use the same state; their rendering detail differs. Rain/lightning are local visual effects below the cloud layer. There is no fluid weather simulation, precipitation accumulation, physical wind, thunder audio, lightning damage or global climate model. The far ring's cloud albedo remains a coarse separate representation; it does not show every local storm exactly.

## Stars and night bands

Only KSP's galaxy cube colour is attenuated in daylight near the ring atmosphere. It fades back with altitude or local night. Ring, shadow panels, Sun and planet renderers are not hidden or masked by this change. The ordinary atmosphere and real geometry can still obscure objects along a sightline. Map view uses the stock starfield. Stock updates restore their ordinary colour outside ring flight; stock planet skies are left to KSP.

The global ring now shades twenty moving night bands even with **Full-ring surface detail** off. Both structural faces and the interior use the same longitude/UT phase as the panel motion and local daylight. Large scaled terrain LOD blocks have per-fragment longitude shading, so distant terrain no longer inherits the observer's entire day/night state. Nearby terrain and water retain the local sunlight calculation. No extra floating shadow overlay is placed over the surface, avoiding another source of depth fighting.

The broad bands are analytical masks with a soft penumbra. They are not ray-traced shadows and do not model small-scale occlusion by terrain/clouds. Flight, map, space-centre and tracking-station ring geometry use the same mask. At extreme warp, a day/night boundary can still move a large distance between rendered frames; that is finite frame sampling rather than a texture-update pause.

For a reproducible visual storm test, turn evolving fronts off and set the baseline to 100%. For clear skies, set it to zero. Re-enable evolving fronts afterward for ordinary weather.
