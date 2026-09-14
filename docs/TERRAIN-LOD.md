# Terrain, LOD and flight refinements — v0.3.0

This records the v0.3.0 design. For the current 160,000 km horizon, clear-sky fix and save settings, see [SETTINGS-AND-HORIZON.md](SETTINGS-AND-HORIZON.md).

## What changed

Terrain uses seeded, domain-warped ridge ranges with foothills, smaller ridges, desert dunes and snow at high elevations. Forest placements use independently jittered candidate positions and smooth grove-density fields. Rocks and rural buildings have independent seeded variation. River width and lake proportions now vary smoothly. Broad ancient road corridors appear as a distinct terrain biome on dry, lower ground; these are surface routes, not a complete transport simulation. Research sites remain at their existing coordinates with level pads.

A terrain seed always produces the same geography. This release changes that geography away from the preserved named-site pads. An older save on wild terrain may no longer sit above its new surface; relocate above a named destination before continuing if necessary. No player save is edited by the installer.

## Level of detail

The old sparse radial backdrop is replaced by an adaptive quadtree of globally aligned terrain blocks, extending approximately **2,000 km** from the observer. Each block has a 32 × 32 grid. Block sizes double with distance. The planner excludes the detailed collision-tile footprint; coverage tests verify that the remaining region has one block per point without overlaps or holes.

The same C# height function supplies both physical ground and distant terrain. Each block has a separate renderer for frustum culling. Render-only skirts cover boundaries between resolutions. Distant meshes have no colliders and cast no shadows; streamed physical ground remains the landing surface. The meshes use double-precision anchors and follow floating-origin shifts.

At most two blocks are generated per frame. Replacements are built hidden, then switched together once the new set is complete. This avoids drawing old and new resolution levels over one another. Initial distant terrain therefore takes several seconds to appear. Coarse sampling, transition popping and visible skirts can still occur; there is no GPU geomorphing or terrain streaming across the entire scaled-space ring. The default test site has approximately 600 blocks, so performance remains hardware-dependent.

The design follows nested-resolution and transition principles described in [NVIDIA GPU Gems 2, Chapter 2](https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry). This implementation is a CPU quadtree with skirts, not that chapter's GPU geometry-clipmap implementation.

## Atmosphere and the shadow squares

Cloud coverage is seeded, warped multiscale noise with slow advection. A 384 × 384 opacity texture resolves softer patches across three uneven decks, roughly 4.3–7 km high. The patch fades near its finite boundary. These remain translucent layers, not ray-marched volumetric clouds or a weather simulation. Mountains mask cloud vertices, but detailed cloud/terrain intersections can still be visible.

Twenty moving shadow-square models and the existing periodic daylight function share the same relative phase. The configured cycle is 10,800 seconds (three hours). It modulates terrain, foliage, structures, local habitat lighting, scattering, cloud illumination and the ring FlightIntegrator's solar-flux multiplier. The low-altitude sky also suppresses the stock daytime starfield as an exposure approximation. This is not a full replacement of KSP's stellar-light and solar-panel occlusion systems; spacecraft illumination can differ from the custom ground.

The [author-hosted Ringworld concordance](https://news.larryniven.net/concordance/content.asp?ovr=t&page=Ringworld+Appendix) describes a sculpted habitat containing mountains, valleys, seas, rivers and a biosphere, with twenty shadow squares. Geography and building layouts here are procedural interpretations, not a map copied from the books. Detailed authored buildings and interiors remain future work.

## Camera, instruments and EVA

Camera clearance includes the interpolated water surface, including shoreline triangles. Automatic and EVA chase views use the stable ring reference frame. Camera terrain pitch uses local ring altitude and up. Both the stock angular-velocity wobble and the separate external camera-effects shake are disabled for ring EVAs only, including cosmetic explosion/contact shake in that view. Saved camera preferences and spacecraft effects are unchanged. Ground-clearance displacement is kept separate from the stock camera's next interpolation step. Distant terrain and artificial streamed underside edges no longer cast shadows. Nearby habitat lighting uses biased soft shadows.

The stock flight altimeter now displays nonnegative inward clearance above ring terrain or water, in either of its display modes while a vessel belongs to the ring frame. It does not rewrite the vessel's orbital altitude or change the Sun SOI. The separate ring panel reports the same surface-relative concept. It is a local downward-clearance reading, not a global nearest-point distance to arbitrary buildings or rim walls.

The flight navball uses ring-relative attitude and velocity while the craft belongs to the rotating frame. Both ordinary speed modes are labelled **Ring surface**; solar orbital normal/radial cues are hidden because they do not describe travel over this habitat. Prograde and retrograde use live physics velocity and disappear below the stock 0.1 m/s display threshold. Target-relative velocity is available for another vessel in the same ring frame; otherwise the speed reads a dash. This does not modify orbital data or SAS guidance. Stock instrument behaviour returns on departure.

Real movement-key tests reproduced an EVA launch above 14 million m/s. KSP's heading, bounding and landing transitions seeded walking interpolation with `Vessel.horizontalSrfSpeed`, a Sun-relative cache hundreds of km/s larger than walking speed. Scoped replacements now read the local horizontal physics velocity, including jump/landing decisions. This corrects the input frame without a speed cap or damage immunity. Ring-relative slope measurements also replace stock solar-up calculations in EVA contact handling. Tests exercise sideways/backward/diagonal travel and rapid reversals with crash damage enabled. See VALIDATION.md for measured results and the limits of that coverage.

The diagnostic fixture explicitly permits EVA; its copied tutorial originally retained a `CanEVA = false` restriction despite being switched to sandbox. The harness now displays an automated-test banner and is excluded from normal release DLLs. Normal career/scenario permissions are not overridden.
