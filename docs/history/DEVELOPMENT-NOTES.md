# Development history

Archived design and implementation notes. These describe earlier versions, including superseded settings, dependencies and planned work. For current play instructions, use the [player documentation](../README.md); for version changes, use [release notes](../../RELEASE-NOTES.md).

<a id="orbital-arrival"></a>

## Orbital arrival and atmosphere

### Physical model

The default ring radius is 15.3 billion metres. Its angular velocity is sqrt(9.72/R), giving a floor speed of 385,637 m/s. A normal solar orbit is not a velocity-matched rendezvous. Automatic entry preserves that difference; it cannot make an ordinary encounter survivable by removing kinetic energy.

The loaded flight scene uses a rotating chart near the ring. Its axes coincide with inertial axes at entry time t0, avoiding an instantaneous position rotation at arrival. Material longitude zero has orientation omega*t0 in that chart. At elapsed time dt:

    x_inertial = Q(omega*dt) x_chart
    v_inertial = Q(omega*dt) (v_chart + omega cross x_chart)

The frame conversion applies to every loaded, unpacked solar vessel, including each rigidbody's linear and angular velocity. Positions are shifted through FloatingOrigin before conversion to Unity floats. Live rigidbody centre-of-mass and velocity sums are used because KSP's cached vessel fields can lag setters until its next precalculation tick. Scenario format 3 stores the chart epoch alongside each vessel's root position and orientation. Existing format 2 records default to epoch zero.

Entry occurs inside a 50 km margin around the ribbon: datum altitude -50 to 210 km, across-coordinate within half-width plus 50 km. Departure uses 100 km margins, giving an outer altitude of 260 km. The 50 km hysteresis avoids boundary chatter. A two-second cooldown follows explicit departure. The approach predictor intersects a linear trajectory with cylindrical and rim-side boundaries to reduce warp ahead of entry. This is not a guarantee against every extreme-warp curved trajectory.

In the rotating chart, stellar gravity, centrifugal acceleration, and Coriolis acceleration act on each physics rigidbody. Stock solar gravity is subtracted before the additional force is applied, avoiding double counting. Krakensbane velocity offsets are disabled while on the ring, but floating-origin position shifts remain enabled. Surface terrain renders before capture and uses the same material phase on both sides of the handoff. Camera frame bookkeeping is retained, and a sphere sweep from the craft toward the camera limits clipping through custom terrain and buildings.

### Air and visuals

Air occupies the finite inward-facing cylinder between the floor datum and 60 km, bounded by the rim walls. Density is an exponential with an 8 km scale height and a smooth five-kilometre taper to vacuum. A dry-air lapse rate falls from 288.15 K to a 216.65 K floor. Pressure follows rho*R_specific*T; sound speed follows sqrt(gamma*R_specific*T). The profile is a configurable gameplay approximation, not a claim about a fully simulated Ringworld climate.

Scoped Harmony patches provide FlightIntegrator and drag-cube parts with local pressure, density, temperature, Mach number and dynamic pressure. Shock/convection calculations use stock home-body dry-air constants only within the active integrator. The shared Sun is never turned into an atmospheric celestial body. The former isotropic air-drag force is removed, so native aerodynamic drag is not doubled. Buoyancy remains the earlier approximate water model.

Sky radiance is integrated over ray intersections with the cylindrical atmosphere. The implementation uses 32 midpoint samples per intersected segment, RGB Rayleigh coefficients, an exponentially stratified aerosol component, and a Henyey-Greenstein Mie phase function. Background extinction and local shadow-square illumination are included. A low-resolution, vertex-coloured sky mesh is refreshed at five updates per second. This implements single scattering, not multiple scattering or depth-aware fog over terrain.

The radiative-transfer concepts are described in [Bruneton and Neyret's atmospheric scattering work](https://ebruneton.github.io/precomputed_atmospheric_scattering/). This project's CPU cylindrical integrator is original code; it does not incorporate that project's spherical precomputed implementation.

Three translucent procedural cloud decks occupy 4.8, 5.45 and 6.1 km. Material-periodic coverage, slow drift, mountain exclusion, soft opacity and patch-edge fading avoid a static skybox. These are fly-through sheets with depth separation, not volumetric clouds or weather. Clouds cover a 600 km local patch. Visual atmosphere is enabled within 600 km of the floor; the distant scaled ring still uses its earlier coarse ribbon representation.

### Trying it

Launch a rocket-equipped lander in the isolated instance. Open the ring panel with Left Alt+R, select a destination, and use **Training: set up a spin-matched approach**. This explicitly relocates the craft to 250 km, matches the spin and supplies a 1 km/s descent. The craft then leaves the local chart and approaches in solar flight; subsequent capture is automatic. Brake and fly the descent yourself. The training setup is not part of a naturally flown arrival.

General fleet persistence, packed vessels, map trajectories inside the rotating chart, air-breathing engine gates, parachute deployment gates and extreme-speed impacts remain open integration work. See [KNOWN-LIMITATIONS.md](../guides/KNOWN-LIMITATIONS.md) and the actual game-run evidence in [VALIDATION.md](VALIDATION.md).

<a id="ground-and-eva"></a>

## Ground, camera and EVA corrections

The 0.2.0 floor was a single triangle surface. A backface or a camera already intersecting the floor could defeat the camera sphere cast. Streamed terrain now uses a closed collision mesh with opposite top/underside winding and perimeter faces. The top renderer retains terrain materials; separate underside/edge geometry renders the foundation. The underside is 100 m below the minimum terrain height of -1,200 m by default, so the structure never inverts under the oceans. This thickness is an engineering choice, not a canonical measurement.

The camera first sweeps toward the desired position against ring colliders. It then compares the camera altitude against the same piecewise-linear terrain triangles and lifts its near clipping plane out of penetration. The latter check addresses starting overlaps that a sweep alone cannot detect.

The former flight controller tied rotating-frame ownership to the current active vessel ID. During an EVA switch, the new vessel could be unregistered briefly. Update could then advance material phase as if in inertial space, or FixedUpdate could call automatic arrival and subtract the approximately 386 km/s spin velocity again from loaded craft. Multiple faulty handoffs can compound the velocity error. This is a frame/lifecycle defect, not an intended centrifugal effect.

The corrected controller recognizes a rotating frame as long as any loaded registered participant holds it. The crew-EVA event registers the Kerbal before focus switches; nearby adoption also runs before arrival detection. Registered participants retain landing and velocity-frame protection while restoring. The global Krakensbane guard remains active through the focus change, independently of the newly active vessel's registration state.

EVA-specific patches provide an inward up vector, ground rays against the ring, contact states without assigning stock spherical LANDED anchoring, movement projected onto the actual collision normal, and ragdoll acceleration in the ring frame. The Sun's global parameters are not changed. No production speed clamp or damage immunity hides the failure.

The diagnostic game test spawns a stock EVA from the landed test craft with crash damage enabled, releases the ladder, observes contact and velocity, recovers from a ragdoll fall if necessary, and requires an actual walking FSM state as well as distance travelled. It also tests camera recovery from below the floor and a ray hitting the structural underside. Exact results and remaining limitations are in [VALIDATION.md](VALIDATION.md).

The subsequent v0.3.0 regression uses KSP's real movement-key queries. It reproduced another launch during a change of walking direction: stock state transitions copied Sun-relative `horizontalSrfSpeed` into walking interpolation. Those reads now use local horizontal physics velocity. Camera terrain heights, automatic/chase orientation and EVA camera-shake handling also use the ring context. See TERRAIN-LOD.md for the instrument changes and [VALIDATION.md](VALIDATION.md) for before/after measurements.

<a id="terrain-lod"></a>

## Terrain, LOD and flight refinements — v0.3.0

This records the v0.3.0 design. For the current 160,000 km horizon, clear-sky fix and save settings, see [SETTINGS-AND-HORIZON.md](DEVELOPMENT-NOTES.md#settings-and-horizon).

### What changed

Terrain uses seeded, domain-warped ridge ranges with foothills, smaller ridges, desert dunes and snow at high elevations. Forest placements use independently jittered candidate positions and smooth grove-density fields. Rocks and rural buildings have independent seeded variation. River width and lake proportions now vary smoothly. Broad ancient road corridors appear as a distinct terrain biome on dry, lower ground; these are surface routes, not a complete transport simulation. Research sites remain at their existing coordinates with level pads.

A terrain seed always produces the same geography. This release changes that geography away from the preserved named-site pads. An older save on wild terrain may no longer sit above its new surface; relocate above a named destination before continuing if necessary. No player save is edited by the installer.

### Level of detail

The old sparse radial backdrop is replaced by an adaptive quadtree of globally aligned terrain blocks, extending approximately **2,000 km** from the observer. Each block has a 32 × 32 grid. Block sizes double with distance. The planner excludes the detailed collision-tile footprint; coverage tests verify that the remaining region has one block per point without overlaps or holes.

The same C# height function supplies both physical ground and distant terrain. Each block has a separate renderer for frustum culling. Render-only skirts cover boundaries between resolutions. Distant meshes have no colliders and cast no shadows; streamed physical ground remains the landing surface. The meshes use double-precision anchors and follow floating-origin shifts.

At most two blocks are generated per frame. Replacements are built hidden, then switched together once the new set is complete. This avoids drawing old and new resolution levels over one another. Initial distant terrain therefore takes several seconds to appear. Coarse sampling, transition popping and visible skirts can still occur; there is no GPU geomorphing or terrain streaming across the entire scaled-space ring. The default test site has approximately 600 blocks, so performance remains hardware-dependent.

The design follows nested-resolution and transition principles described in [NVIDIA GPU Gems 2, Chapter 2](https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry). This implementation is a CPU quadtree with skirts, not that chapter's GPU geometry-clipmap implementation.

### Atmosphere and the shadow squares

Cloud coverage is seeded, warped multiscale noise with slow advection. A 384 × 384 opacity texture resolves softer patches across three uneven decks, roughly 4.3–7 km high. The patch fades near its finite boundary. These remain translucent layers, not ray-marched volumetric clouds or a weather simulation. Mountains mask cloud vertices, but detailed cloud/terrain intersections can still be visible.

Twenty moving shadow-square models and the existing periodic daylight function share the same relative phase. The configured cycle is 10,800 seconds (three hours). It modulates terrain, foliage, structures, local habitat lighting, scattering, cloud illumination and the ring FlightIntegrator's solar-flux multiplier. The low-altitude sky also suppresses the stock daytime starfield as an exposure approximation. This is not a full replacement of KSP's stellar-light and solar-panel occlusion systems; spacecraft illumination can differ from the custom ground.

The [author-hosted Ringworld concordance](https://news.larryniven.net/concordance/content.asp?ovr=t&page=Ringworld+Appendix) describes a sculpted habitat containing mountains, valleys, seas, rivers and a biosphere, with twenty shadow squares. Geography and building layouts here are procedural interpretations, not a map copied from the books. Detailed authored buildings and interiors remain future work.

### Camera, instruments and EVA

Camera clearance includes the interpolated water surface, including shoreline triangles. Automatic and EVA chase views use the stable ring reference frame. Camera terrain pitch uses local ring altitude and up. Both the stock angular-velocity wobble and the separate external camera-effects shake are disabled for ring EVAs only, including cosmetic explosion/contact shake in that view. Saved camera preferences and spacecraft effects are unchanged. Ground-clearance displacement is kept separate from the stock camera's next interpolation step. Distant terrain and artificial streamed underside edges no longer cast shadows. Nearby habitat lighting uses biased soft shadows.

The stock flight altimeter now displays nonnegative inward clearance above ring terrain or water, in either of its display modes while a vessel belongs to the ring frame. It does not rewrite the vessel's orbital altitude or change the Sun SOI. The separate ring panel reports the same surface-relative concept. It is a local downward-clearance reading, not a global nearest-point distance to arbitrary buildings or rim walls.

The flight navball uses ring-relative attitude and velocity while the craft belongs to the rotating frame. Both ordinary speed modes are labelled **Ring surface**; solar orbital normal/radial cues are hidden because they do not describe travel over this habitat. Prograde and retrograde use live physics velocity and disappear below the stock 0.1 m/s display threshold. Target-relative velocity is available for another vessel in the same ring frame; otherwise the speed reads a dash. This does not modify orbital data or SAS guidance. Stock instrument behaviour returns on departure.

Real movement-key tests reproduced an EVA launch above 14 million m/s. KSP's heading, bounding and landing transitions seeded walking interpolation with `Vessel.horizontalSrfSpeed`, a Sun-relative cache hundreds of km/s larger than walking speed. Scoped replacements now read the local horizontal physics velocity, including jump/landing decisions. This corrects the input frame without a speed cap or damage immunity. Ring-relative slope measurements also replace stock solar-up calculations in EVA contact handling. Tests exercise sideways/backward/diagonal travel and rapid reversals with crash damage enabled. See [VALIDATION.md](VALIDATION.md) for measured results and the limits of that coverage.

The diagnostic fixture explicitly permits EVA; its copied tutorial originally retained a `CanEVA = false` restriction despite being switched to sandbox. The harness now displays an automated-test banner and is excluded from normal release DLLs. Normal career/scenario permissions are not overridden.

<a id="settings-and-horizon"></a>

> For the new full-circumference surface colour layer and photo mode, see [HIGH-END-VISUALS.md](DEVELOPMENT-NOTES.md#high-end-visuals).

> Historical v0.4 notes. Current bounds, physics and settings are in [ORBITS-WARP-AND-ASSETS.md](DEVELOPMENT-NOTES.md#orbits-warp-and-assets).

## Save settings and the long horizon — v0.4.0

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

### Why the ring disappeared

The old low-altitude daytime sky forced opacity to 99.8% to hide the stock starfield. That covered the scaled ring and Sun as well. It was an exposure shortcut, not a weather event. The renderer now uses the calculated atmospheric optical depth, adjusted by the haze control. Clear overhead air transmits distant objects; longer paths near the horizon are hazier. Actual clouds can obscure the view. Some stock stars can now be visible during the day; selective starfield exposure remains unfinished.

The complete coarse ring and 160 km-high rim-wall model remain present globally. The day/night cycle already existed and is now adjustable. Ring rotation alone does not produce its night: moving shadow squares provide the illumination cycle.

### What 160,000 km means

The adaptive quadtree grows by adding coarser levels, rather than filling the entire area with fine tiles. Beyond the local rendering region, large blocks use scaled-space positions and scaled vertices. The far terrain uses opaque emission-based shading with the shared daylight multiplier. Initial generation can take many seconds; the expedition panel shows queued blocks.

This range can encompass both rim walls from the starter region. It does not imply local colliders over that area, fine mountain silhouettes at maximum range, or terrain detail around the entire circumference. Trees and rural buildings belong to nearby collision tiles; named buildings are generated near their site. Mountains and other height-field landmarks participate in terrain LOD. Small structures do not receive distant impostors in this release.

The laptop preset reduces vertex work substantially, but draw calls, Unity/KSP overhead, CPU generation, memory and graphics hardware still matter. No laptop performance guarantee is claimed. Coarse sampling can miss narrow peaks, and resolution transitions can remain visible.

The design follows multiresolution principles from [NVIDIA GPU Gems 2](https://developer.nvidia.com/gpugems/gpugems2/part-i-geometric-complexity/chapter-2-terrain-rendering-using-gpu-based-geometry). Unity's [screen-relative LOD documentation](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/LOD-screenRelativeTransitionHeight.html) explains why apparent size, rather than distance alone, matters for detailed objects. This implementation remains a CPU quadtree, not GPU geometry clipmaps.

### Less artificial terrain

New saves use gradient noise with warped foothill coordinates instead of the older interpolated random-height lattice. Both near and distant meshes now use spatial colour textures. This fixes a separate palette error: interpolating a grassland-to-snow palette coordinate previously swept through unrelated desert and mountain colours, producing artificial contour bands. Actual RGB colours now interpolate spatially.

Terrain still uses analytic rivers and basin patterns, without erosion simulation. Cities retain deliberately planned layouts and primitive architecture. Further work remains on drainage, settlement impostors, material detail and lighting balance.

<a id="orbits-warp-and-assets"></a>

## Ringworld 0.5: trajectories, time and assets

### Gravity and map predictions

There is no new spherical celestial body or sharp physical SOI. The Sun remains the stock reference body. In inertial flight the real forces are stellar attraction and ribbon attraction. Centrifugal and Coriolis terms apply only in the rotating expedition chart. Ring spin supplies inherited velocity, not attraction.

The assumed mass distribution is a uniform cylindrical ribbon of radius R, axial width W and surface density sigma. Total mass is 2*pi*R*W*sigma. Default sigma is 1,000,000 kg/m2: a gameplay assumption, not a claim about fictional scrith. Near the floor its pull is approximately 0.0004 m/s2, far below the default 9.72 m/s2 spin acceleration. Width is integrated analytically; azimuth uses Gaussian quadrature on logarithmic panels. A 50 m Plummer softening length avoids a singular ideal sheet. Walls, mountains, buildings and the star's reaction are not separately modelled.

The cyan map curve integrates a vacuum coast under the same ribbon and stellar field used by flight. Expedition predictions use the rotating map chart; external predictions use inertial coordinates. It replaces the active solar vessel's stock spline when enabled. Stock manoeuvre nodes, orbit statistics and other vessels' conics remain two-body data. The curve stops at atmosphere entry; inside air the panel explains why no vacuum curve is shown. Drag, lift, future thrust and manoeuvres are not predicted. A sampling budget can truncate a long forecast near the floor.

Unpacked solar vessels receive ribbon attraction. Active packed solar flight receives numerical propagation. Other planetary SOIs and inactive/unloaded vessels retain stock propagation. This is not a full N-body conversion. Extreme warp, planetary encounters and arbitrary vessel-switch/save transitions need more validation.

Research: [Igata, circular ring potential](https://arxiv.org/abs/2005.01418) and [Bannikova et al., torus potential](https://arxiv.org/abs/1009.4324). These support treating the mass as extended. Our finite-width softened ribbon is its own approximation, not either paper's full torus solution.

### Surface time acceleration

Superseded in v0.7: use KSP's stock warp controls with the resting ring-surface anchor. See [STOCK-WARP-AND-RENDERING.md](DEVELOPMENT-NOTES.md#stock-warp-and-rendering). The old separate UT-increment buttons and their non-accelerated-resource behaviour no longer apply.

### Expanded settings

Terrain horizon is a numeric kilometre field with a minimum of 200 and no fixed upper cap. Values exceeding the ring extent do not generate repeated laps. Laptop preset: 160,000 km, 8 subdivisions, one new block/frame. Workstation: 1,000,000 km, 32 subdivisions, two blocks/frame. Distant meshes are terrain/water; trees and small buildings remain local. Hardware performance is not guaranteed.

New-world controls include diameter (minimum 2 million km; no preset upper cap, subject to finite coordinate precision), width (10,000 km to the radius), walls (60 to 1,000 km), spin acceleration (1 to 100 m/s2), floor mass density (0 to 100 million kg/m2), seed, height, forests and pond coverage. They lock after an expedition or discovery to preserve terrain beneath saved craft.

Runtime controls include shadow-square cycle (one minute to 30 days), prediction duration (1 to 1,440 minutes), numerical map toggle, surface wait limit (10 to 10,000x), weather and rendering quality. Day length is independent of ring spin period, which follows radius and spin acceleration.

New saves use terrain generation v3. Existing saved generator versions remain unchanged. V3 continuously deforms river catchments, adds small ponds, and scatters tree candidates throughout a tile instead of one per small grid cell. Regions mix broadleaf and conifer silhouettes. Existing ranges, foothills, deserts, two Great Oceans, islands and named lakes remain. These are analytic landscapes, not hydraulic erosion. A selection bug that prevented rural habitation clusters with the newer noise generator is fixed.

### Blender model contract

Update: the first-party [Blender scenery set](../developers/ASSET-AUTHORING.md#blender-assets) uses FBX to Unity prefabs in a dedicated AssetBundle. The `.mu` registration interface below remains available for overrides; `.blend` files stay in the mod's `art/` directory.

Keep .blend source files outside GameData. Export through the KSP/Unity .mu pipeline. Register model paths without extensions, one node per variant:

```cfg
RINGWORLD_SCENERY_ASSET
{
    kind = tree_broadleaf
    model = NivenRingworld/Models/Trees/Broadleaf01
}
```

Slots: tree_broadleaf, tree_conifer, rural_building, boulder. Multiple entries per kind give deterministic variants. Missing entries retain procedural placeholders. Imported materials are preserved. Child objects use the ground layer. Include simple separate colliders; do not use dense leaf meshes as collision geometry.

Trees use +Y up, a ground-level root pivot and one metre authored height; code scales uniformly. Buildings and boulders use centre pivots and a normalized unit bounding box, scaled independently to the generated footprint/height. Imported Unity LODGroups can handle local detail levels. Long-distance tree/city impostors remain a separate future renderer.

Suggested art families: several irregular broadleaf species, layered/narrow conifers, dead trees/stumps, boulders, rural dwellings, abandoned city modules, bridges, terminals and scrith machinery. Whole-tree slots replace trunk and canopy together. City landmark slots and interiors remain future work alongside their authored assets.

<a id="biomes-and-graphics"></a>

## Biomes, ground detail and laptop rendering (0.6)

New saves use terrain generator 4. Existing saves keep their stored generator: changing terrain beneath a landed craft would be unsafe. Start a new Sandbox save to explore the new landscape. Random visits change coordinates, not the terrain seed.

### Landscape design

Seeded, continuous temperature, humidity and ruggedness fields blend five climate families: meadow, forest, desert, cold and highland. Their weights smoothly influence ground colour, tree cover, relief and a small atmospheric RGB tint. The displayed biome name is the strongest family, so its label changes discretely even though the visual weights blend. Oceans, rivers, roads and authored landmark areas retain their own classifications. The outermost portion of the ribbon receives a cold climate bias; this approximates rim-shadow ecology, not a thermal simulation.

The useful Minecraft reference is separation of biome placement from terrain shape, plus climate-dependent appearance. This is an analytic curved surface, not a voxel implementation or a copy of Minecraft's generator. [Mojang's 1.18 description](https://www.minecraft.net/en-us/article/caves---cliffs--part-ii-out-today-java) explains independent terrain and biome placement; [Microsoft's Bedrock client-biome components](https://learn.microsoft.com/en-us/minecraft/creator/reference/content/clientbiomesreference/examples/componentlist?view=minecraft-bedrock-stable) describe grass, foliage and water appearance controls.

Ordinary v4 land uses low rolling relief, capped at 100 m of variation at height multiplier 1 above a 100 m reference. This is a design interpretation of shallow covering over scrith, not a claim that every point has exactly that soil depth. River carving, water basins and engineered landmark mountains are separate. Existing warped catchments, tributary-shaped channels, ponds, oceans and coastlines remain analytic: there is no globally connected drainage solver or hydraulic erosion. Exposed scrith is presently a grey erosion-mask surface treatment.

### Close detail

Grass blades, small shoreline stones, sand patches, leaf litter and occasional silver sunflower clusters share one collision-free mesh. Candidates are seeded and jittered; placement raycasts against the actual streamed ground triangles. Density follows KSP's Terrain Scatters switch and scatter factor. Trees also follow that native control, with climate-blended density in v4. Buildings are not disabled by the foliage setting.

Close detail is adjustable from 25 to 250 m, defaults to 75 m, and is hidden above 80 m ground clearance. New saves and the laptop preset select 75 m, an 8-segment distant mesh and one new LOD block per frame at a 160,000 km horizon. Candidate spacing grows with range; the hard ceiling is 2,200 clumps, not millions of individual objects. Rebuilding occurs on movement across 16 m cells or settings changes. No grass colliders or grass shadow maps are generated.

Local dust/pollen uses one small billboard mesh, at most 48 particles, updated at 10 Hz near the ground. It has a separate on/off setting. These are visual effects, with no wind force or sunflower heating. Climate tint changes RGB without adding sky opacity.

### Native graphics controls

The runtime does not write global Unity quality settings. Auditing this installation's `GameSettings.ApplySettings` showed KSP applies its quality preset, MSAA, master texture mip limit, v-sync, pixel light count and shadow cascades to Unity. Ringworld's normal camera/renderers share that pipeline. The UI reports the active AA, mip limit, shadows and native scatter density.

Terrain colour maps, clouds and dust now have mip chains. KSP Texture Quality can therefore select lower-resolution mip levels. [Unity 2019.4 mip-limit documentation](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/QualitySettings-masterTextureLimit.html) defines 1 as half resolution and 2 as quarter resolution. [Unity's AA documentation](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/QualitySettings-antiAliasing.html) describes MSAA's forward-rendering limitation: inheriting the setting does not guarantee every transparent shader is antialiased identically.

Global shadow quality affects the local sunlight; global light, v-sync and frame limits remain KSP-controlled. Terrain Shader Quality/PQS subdivisions are stock spherical-terrain controls and do not directly set ribbon topology. Aero FX Quality and reflection-probe scheduling are likewise not substitutes for custom terrain LOD settings. Those distinct Ringworld controls remain in its panel. This is not a claim of implementing every stock terrain shader effect.

### Random rendering inspection

Alt+R > Expedition > **Random terrain test location (spin-matched)** uses the arrival-height slider. It searches at most 128 candidates, avoiding water, named landmark footprints, walls and steep local slopes. Arrival uses the same collider preparation and rotating-frame transfer as named destinations. Initial velocity is zero relative to the ring; inertially it includes the ring's rotation. Gravity acts immediately afterward, so fly the descent. Trees or other local scenery may still require landing-site selection.

The current seed and destination coordinates remain visible in the panel. Random visits work with existing generators too; they do not upgrade a saved world. This is an exploration/testing aid, not propulsion or an automatic landing.

<a id="stock-warp-and-rendering"></a>

## Stock warp, contact and rendering — 0.7

### Stock controls

Use KSP's top-left warp controls or comma/period. The separate Ringworld rate buttons have been removed. Ring-surface high warp uses KSP's native `TimeWarp` rate and universal clock; it no longer manually increments UT in an ordinary physics frame. Other stellar bodies and ordinary stock orbital propagation therefore use the same accelerated clock.

Resting expedition craft are packed through KSP's rails lifecycle, but their position/rotation are held in the ring's existing rotating chart. Scoped OrbitDriver/VesselPrecalculate patches prevent the Sun's Kepler orbit from carrying an anchored ring craft away. On return to 1x, the same pose is restored with zero ring-relative velocity. Terrain generation and lighting continue while those craft are packed.

Entry requires actual ground contact, dry terrain, speed at most .25 m/s, angular speed at most .05 rad/s, no throttle, and no nearby unpacked vessel outside the expedition. This is surface anchoring, not accelerated flight integration: rails warp while flying over the ring and low/physics warp inside the expedition are not enabled. Use normal flight until contact is settled. Ordinary stock orbital warp outside the arrival region remains available. The existing maximum surface-warp setting limits native ring-surface rates, default 1,000x, adjustable through 10,000x.

Stock rails behaviour governs other vessels and background systems; compatibility with arbitrary resource/life-support mods and scene changes during anchored warp has not been certified. Do not interpret the new clock integration as a bespoke full simulation of every third-party module.

### Contact and camera

The terrain collision shell now has a dedicated friction material: static 1, dynamic .85, maximum friction combination, zero bounce. This addresses the default-material sliding observed on ordinary ground. It does not flatten slopes or stop a moving craft by teleporting it. Unity's [physics material documentation](https://docs.unity.cn/2019.1/Documentation/Manual/class-PhysicMaterial.html) explains how the two contact materials are combined.

Camera wobble/effects suppression now includes ground-contact craft and nearly stationary craft close to the floor, in addition to EVAs. This covers a surviving craft lying on its side. Actual contact movement and collision clearance still affect the camera; the change does not conceal structural breakup or promise stability for every vehicle.

### Coarse geometry, map and chunk publication

The old global ring used straight chords whose midpoints intruded above the true curved floor. At the default radius and 8,192 segments, that inward error is about 1.1 km. The fallback floor now uses an outward radial offset derived from `R * (sec(pi/segments) - 1)`, plus the minimum terrain depth, hull thickness and a float roundoff margin of max(512 m, radius*1e-6). The extra depth belongs only to the coarse visual fallback; local collision thickness is unchanged. The chord lies below the minimum supported terrain instead of cutting through the local view. This also separates fallback floor geometry from rendered terrain.

The coarse ring has a closed cross-section, including wall tops, outer faces and underside. Local rim wall colliders/render meshes also have inner/outer faces, caps and finite thickness, with separate face normals. Ground tiles stop at the ribbon edge instead of extending a phantom wall-height plateau outside it; LOD floor sampling no longer turns the last terrain row into a ramp up the wall. Wall thickness currently follows the configured structural thickness (100 m by default); this is a modelling choice, not a claimed canonical dimension. Distant walls use the global angular mesh and nearby walls use local tile geometry.

In map view from below the habitat, interior terrain and its seam skirts are hidden and the closed coarse underside owns the view. Skirts are crack covers, not visible underside structures. From inside the habitat, terrain remains visible.

Each completed LOD block becomes visible in the same update, after positioning. Obsolete blocks are retired rather than retained over new child blocks, avoiding parent/child overlap. The old behaviour held all new blocks invisible until the whole queue drained. The budget remains one block per frame by default; generation is bounded work on the main thread, not a new background worker. During movement or rebuilding, some areas may temporarily show the coarse fallback as their detailed blocks arrive.

### Re-entry effects

Stock AerodynamicsFX read Sun-relative `srf_velocity` and speed. The scoped replacement supplies ring-air-relative velocity and magnitude, consistent with the mod's atmosphere model. Effect direction is opposite that velocity. A nearly radial, spin-matched descent should therefore trail generally upward; a real crosswind, tangential mismatch or Coriolis-induced lateral motion can tilt it. [NASA's relative-velocity explanation](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/relative-velocity/) describes why velocity relative to the air is the relevant quantity.

The combined development reference is [BIOME-ASSET-CATALOG.md](../reference/BIOME-ASSET-CATALOG.md), including equations, placement gates, budgets, current/planned assets and measured frequency ranges. It is documentation; editing it does not itself change runtime generation.


An unprotected test at solar circular speed without matching the ring produced 376.9 km/s air-relative speed. The effects direction was almost entirely tangential, and part explosions began 0.08 simulation seconds after first nonzero atmospheric density. The command pod was destroyed shortly afterward. The high-altitude fade means first atmospheric contact need not destroy the craft in the very first physics step. Exact heating numbers at this speed are outputs of KSP's stock model, not a validated plasma simulation. See [VALIDATION.md](VALIDATION.md) for the fixture and limitations.

<a id="high-end-visuals"></a>

## Ringworld high-end visuals and photo mode — 0.9

### Current controls (1.0.3)

The old Laptop/High/Ultra atmosphere buttons are now labelled Simple/Half-resolution/Full-resolution. Water Laptop is now Simple. Use the new overall quality preset dropdown to apply coordinated settings; see [QUALITY-PRESETS.md](../guides/QUALITY-PRESETS.md). The historical implementation description below retains the old quality names.

### Using it

Open the Ringworld panel → Settings. Atmosphere quality has Laptop, High and Ultra modes, independently of stock planet settings. Apply settings to persist your choices with this save. Laptop remains the default. High uses a half-width/half-height optical buffer; Ultra uses full viewport resolution. Both integrate three-dimensional cloud density on the GPU, with direct-light self-shadowing, soft density edges and per-pixel Rayleigh/Mie atmosphere. High defaults to 64 cloud and 32 atmosphere samples; Ultra defaults to 128/64. Individual sample counts, cloud distance (30–500 km), self-shadow strength and atmosphere brightness are adjustable. Weather settings and their new evolving state are described in [WEATHER-AND-NIGHT.md](../developers/CLOUDS-AND-WEATHER.md#weather-and-night).

Water has Laptop, Reflective and Waves modes. Reflective adds Fresnel sky reflection, sun glints and small animated normal ripples. Waves additionally displace local water vertices, with adjustable amplitude up to 2 m and a finer ripple layer. Waves fade near the shoreline, where a procedural foam approximation appears. Sky reflection is analytical; this renderer does not reflect nearby buildings, craft or terrain. Nearby non-scaled LOD water uses the same water material, with geometric wave displacement fading over distance. The far scaled water remains coarse. Water forces and buoyancy still use the existing mean water level: these are visual waves, not a fluid simulation. Camera clearance includes the configured maximum crest height.

Frame the camera in ordinary flight, at 1x, then press **Photo mode: high-quality still** in Expedition or Settings. The current view is frozen, stock HUD hidden, and input locked. The renderer temporarily uses 32-subdivision distant terrain and a one-chunk-per-frame generation budget at your existing horizon distance. It waits for those chunks before capturing the scene. Photo mode enables wave water and uses 192 cloud, 80 atmosphere and 8 light samples per ray at full viewport resolution. One of sixteen image tiles is refined per frame, over the selected 1–64 accumulation samples (16 by default). The preview fills progressively. This spreads expensive shading over time, rather than requiring high-end real-time frame rates. It still renders frames and uses GPU memory; it is not an external offline path tracer.

The PNG and a small metadata text file are saved to `Screenshots/Ringworld` inside the game folder. Image size is the current viewport resolution; accumulation refines the volumetric integration and does not supersample the spacecraft or synthesize extra terrain detail. **Resume flight** or **Cancel photo** restores time scale, target frame rate, HUD visibility, the input lock, normal terrain LOD/budget, and the normal visual modes. Simulation time, weather and wave phase remain frozen during the capture. Use a higher native game resolution before entering photo mode if you want a larger image.

KSP texture quality, anti-aliasing, ordinary shadow settings and scatter enable/density remain inherited. There is no stock volumetric-cloud quality knob to reuse. Ringworld does not change the global render pipeline, install a global post-processing profile or replace stock planet materials. Photo mode preserves those global quality choices too.

### Renderer boundaries

The new image effect belongs to the local flight camera and only activates near the ring. Ring-local, camera-relative coordinates avoid subtracting large star-centred floats inside the shader. Atmospheric/cloud sampling accounts for ring curvature in a finite tangent region and clips against camera depth and ribbon width. It is not a global volumetric rendering of the full circumference. Existing scaled geometry provides the distant ring outline.

Clouds use an original tileable 3-D value/cellular noise texture and continuous seeded coverage field. Direct-light extinction is raymarched; ambient fill approximates multiple scattering. There is no full multiple-scattering solution or cloud shadow map projected onto terrain. Version 0.9 adds seeded visual weather, rain and lightning; it is not a fluid/weather simulation. Real-time High uses depth-aware spatial upsampling, not motion-vector temporal reprojection. Fine noise can remain visible at low sample counts. Photo accumulation is valid because the camera, depth, scene and simulation time are frozen.

The shader bundle targets Windows Direct3D 11 / Unity 2019.4.18f1. Other graphics APIs and third-party camera stacks are not certified. Unsupported/missing atmosphere assets report an error and retain the laptop sky. The shader project and noise generator live in `tools/VisualShaders`; no third-party shader or paid art assets are embedded.

### Research and integration decisions

These were researched as visual references, not automatically installed dependencies. Compatibility of an entire third-party mod stack has not been tested.

| Reference | Relevant contribution | Ringworld decision |
|---|---|---|
| [Deferred](https://github.com/LGhassen/Deferred) | Deferred lighting and reflection improvements; its compatibility table distinguishes forward-rendered materials | Keep a local image effect rather than globally switching rendering paths. A future Deferred adapter needs its own test matrix. |
| [EVE Redux](https://github.com/LGhassen/EnvironmentalVisualEnhancements) and [volumetric quality documentation](https://github.com/LGhassen/EnvironmentalVisualEnhancements/wiki/Temporal-upscaling-and-noise-detiling) | Cloud rendering; temporal reconstruction amortizes work and handles camera motion through reprojection | Original ring-shaped cloud field and GPU density integration. Frozen tiled accumulation in photo mode; no claim to reproduce Blackrack Volumetric Clouds V5 or its paid assets. |
| [Scatterer](https://github.com/LGhassen/scatterer) | Atmospheric scattering; public repository distinguishes plugin and newer shader distribution | Original bounded ring-atmosphere integrator; no copied planet shader or assumed spherical atmosphere. |
| [Parallax Continued](https://github.com/Gameslinx/Parallax-Continued) | Detailed terrain/scatter and separate planet/terrain texture assets | Photo mode raises our existing LOD. Future authored terrain materials/assets remain separate work; Parallax assets are not bundled. |
| [Firefly](https://github.com/M1rageDev/Firefly) | Replaces stock aerodynamic VFX, with its own API/configuration | Existing ring-relative stock effects remain. Firefly would need a ring-aware adapter, not merely planet configuration. |
| [ZTheme](https://github.com/zapSNH/ZTheme/releases) / [HUDReplacer](https://github.com/UltraJohn/HUDReplacer) | HUD presentation | Hide and restore the actual stock HUD during photos; do not replace the user's theme. |
| [PlanetShine](https://github.com/PapaJoesSoup/ksp-planetshine/releases) | Reflected environmental light, including EVA | Existing ring lighting retained. Optional ambient/bounce integration is future work. |
| [ReStock](https://github.com/PorktoberRevolution/ReStocked) | Spacecraft art revamp | Craft asset replacement is independent of ring rendering; no parts overwritten. |
| [Restock Waterfall Expansion](https://spacedock.info/mod/3149/Restock%20Waterfall%20Expansion) | Engine effect configurations for ReStock/ReStock+ | Engine plume assets/configuration remain owned by that mod; not confused with ring water rendering. |
| [Shabby](https://github.com/KSPModdingLibs/Shabby) | Shader asset-bundle loading | This small renderer loads its own named bundle; no global shader replacement or additional loader dependency is needed. |
| [TUFX](https://github.com/shadowmage45/TUFX) | Post-processing profiles, including bloom, grading and depth of field | Preserve user profiles. Ring atmosphere brightness is local; no global bloom/colour-grade override and no bundled Blackrack profile. |
| [VaporCones](https://spacedock.info/mod/3805/VaporCones) | High-speed condensation effects | Separate aerodynamic effect; not a cloud-system substitute. Ring-aware velocity/density compatibility needs dedicated integration. |

### Building the assets

Run `build-visuals.ps1` with Unity 2019.4.18f1 installed. It generates the original volume noise asset, compiles the atmosphere and wave shaders, and writes `GameData/NivenRingworld/Assets/ringworldvisuals`. Ordinary `build.ps1 -Install` uses the shipped bundle and does not need Unity. The bundle is approximately 1.2 MB; photo render targets scale with viewport size, so large resolutions require substantially more VRAM.


### Clear sky and weather

The original renderer already has moving cloud-front coverage, controlled by **Moving weather fronts**. Version 0.9 adds visual rain and lightning; it does not simulate wind forces or precipitation accumulation. All quality tiers share cloud amount and time-dependent coverage. High/Ultra changes how that cloud field is shaded. **Cloud amount = 0** explicitly disables cloud density and gives a clear-sky comparison; it does not disable the atmosphere. At low coverage, individual clouds may locally obscure the ring, but a clear sky should transmit the ring outline and stellar light. The first visual test intentionally used 75% coverage with moving fronts off, which produced an overcast photo. That test was not evidence of a permanently opaque sky.

### Full-ring views

**Full-ring surface detail** adds a procedural colour shader to the interior of the global ribbon. It has no terrain-distance cutoff: it covers the entire circumference (about 96 million km at the default radius), including the visible upward arc. Enable it separately from atmosphere quality; the Workstation preset enables it, the Laptop preset disables it, and Photo mode enables it temporarily. The plain global ring remains visible with the setting off. The added layer uses the existing 8,192-segment ribbon, not millions of additional chunks.

The distant colour map uses the terrain seed, its gradient hash and climate palette, and the two Great Ocean basin centres. These are coarse representations: coast shapes, minor waterways, the height field and small islands are not exact replicas of local terrain. High-frequency climate noise is filtered when smaller than a pixel. Distant clouds are flat weather colour, not volumetric clouds across the full circumference. They follow cloud amount; moving fronts animate their phase. Twenty day/night bands use the same UT-driven square cadence as local daylight, including when full-ring detail is off. This shader does not add distant buildings or tree geometry. Those require dedicated simplified assets in the later Blender pass.

Detailed terrain remains governed by **Terrain horizon distance**, with no fixed upper setting cap, and its subdivision setting. The global colour layer fills the view beyond that range. Existing floor burial and closed rim-wall geometry are retained so the coarse ribbon does not pass through the camera or fight with local ground.

The supplied painting is an artistic composition. At the default physical dimensions, the width is approximately 1% of the radius: the arc narrows quickly in a wide-angle camera. More rendering distance cannot make that physically narrow arc as broad as the painting. Camera framing, atmospheric visibility and the chosen ring dimensions all affect its appearance. Clear sky removes cloud obstruction; atmospheric haze and foreground geometry can still obscure low-angle parts of the arc. This is a first distant-surface pass, not a reproduction of the painting.

<a id="residence-and-encounters"></a>

## Ring encounters, residence and stock science

The Ringworld is an annular physics region, not a spherical CelestialBody. The Sun remains the stock reference body. No solar SOI, PQS, radius or science multiplier is globally changed.

### Map encounters

The numerical coast uses the same InArrivalRegion predicate as flight handoff, including its hysteresis. Its incoming segment ends at a green boundary marker and the following segment uses a different color until atmosphere/contact prediction stops. Hover the marker for time to the boundary. The predictor can start on an upcoming solar patch while the selected vessel is still in a planetary SOI; the preceding native planetary patch remains visible. This is a Ringworld overlay, not a stock patched-conic SOI node. Atmospheric flight, burns and maneuver optimization are not simulated by this coast preview. Calculation remains bounded to 4,096 steps and the saved prediction duration.

### Residence

Actual part ground/permanent-ground contact supplies LANDED, while the ring frame continues supplying dynamics. Stock save permission is granted only for a settled expedition that meets the existing surface-warp checks. The scenario records landed status alongside root position, velocity, rotation and frame epoch. Packed residents retain ring coordinates instead of following a fictitious solar orbit; nearby saved residents are restored together when their physics loads. Stock calls that assume a spherical PQS or spherical unpacking orientation are bypassed only for registered ring vessels.

Use Save and Space Center / Tracking Station. Revert Flight intentionally rolls back the flight and is not a way to leave a persistent base.

### Science and deployables

Stock ModuleScienceExperiment instruments and Breaking Ground deployed experiment initialization receive a Ringworld-specific science namespace and readable title. IDs retain the Sun reference internally for stock persistence compatibility, but include Ringworld_ and do not consume ordinary solar subjects. Biome relevance follows each experiment's mask. Ring subjects use the experiment's base science cap and a neutral subject multiplier; the Sun's multipliers remain untouched. Existing old solar reports are not rewritten.

Cargo settling uses ring-relative speed. Nearby spawned ground parts inherit the ring frame; permanent ground contact is recognized for saving and warp. The stock deployment, controller, power, science generation and inventory systems otherwise continue to run. This integration is not a claim of compatibility with every third-party part module. Specialized seismic-distance calculations and background solar-duty assumptions still use stock behavior and need a dedicated physical model for the annular habitat.

### Walls

The full circumference already has a closed eight-vertex cross-section: outer faces, top caps, inner faces and hull underside. Both fallback and detailed rendering now use the same dark material for non-floor faces. This changes the material assignment, not the mesh budget. A physically 100 m thick wall becomes subpixel when viewed from sufficiently far away; it is not artificially widened as the camera recedes.

### 2026-09-16: SAS, parachutes and responsive coast preview

SAS prograde and retrograde now use the same ring-relative velocity as the navball, in either Orbit or Surface display mode. Radial out/in mean away from/toward the local floor; normal/antinormal use the perpendicular to local up and travel. Undefined near-zero vectors retain an attitude rather than selecting a noisy solar direction. Stock stability, target-pointing, maneuver modes and SAS unlock requirements remain stock. Target-relative velocity uses the instrument vessel's own target and is supported for another registered ring vessel.

Stock ModuleParachute pressure sampling now uses ring air, and its full-deployment height uses the local ground/water surface. Deployment safety, heat failure, shielding, animations and drag remain stock. This does not certify replacement systems such as RealChute.

The coast predictor previously waited two seconds between starts. It now starts up to 30 times per second, spreads numerical work over roughly 1.5 ms integration slices, retains its 4,096-step ceiling, and updates the origin every rendered frame. Actual completed prediction rate depends on flight state, frame rate and path length; it is not a promise of 30 complete long forecasts per second. Prediction beyond atmosphere entry remains unsupported.

### Pause and warp stability

Pause-menu saving now permits the paused clock when checking otherwise-settled ring contact. Packed anchors keep valid inertial orbit bookkeeping while atmosphere queries report zero local air-relative velocity. The earlier mixed-frame packed speed caused catastrophic artificial heating; this correction retains stock thermal damage. Five damage-enabled warp cycles and a paused-save Space Center roundtrip passed; see [VALIDATION.md](VALIDATION.md).


### v1.1.1 save and Tracking Station rules

The ring uses a rotating frame, not a new spherical celestial body. Airborne and unstable residents explicitly fail KSP's save/exit eligibility check. Stable contact uses the same safety gate as ring-surface warp. The stock unsafe-exit/revert dialog remains responsible for what happens if the player chooses to abandon that flight.

Tracking Station draws the same numerical coast/encounter overlay as flight map mode. It checks incoming solar patches before allowing large warp steps, stops warp near an encounter, and opens the approaching vessel in Flight shortly before frame entry. This is a handoff to real part physics, not simulated background heating or collisions. Orbiting spacecraft outside the ring remain persistent. Legacy airborne ring snapshots are removed outside Flight so an old snapshot cannot rewind a stock orbit that has kept advancing; ships themselves are not deleted. Landed ring anchors remain preserved.

<a id="cyla-assessment"></a>

## Cyla source assessment — 2026-09-16

Historical pre-implementation assessment. The experimental branch now vendors and bundles Cyla; see [CYLA-INTEGRATION.md](../developers/CYLA-INTEGRATION.md) for the current implementation, test results and limitations. Statements below about an unchanged installation describe the initial assessment only.

Reference checkout: C:/Users/Hans/Documents/programming/Cyla-reference
Upstream: https://github.com/LGhassen/Cyla
Pinned commit: 92223648e0674212e488e6fb977b5cffc63be869
Release inspected: 1.1.0.0 / Cyla-1.1.0.zip, extracted beside the checkout.
No upstream code or binaries have been incorporated into NivenRingworld or its release packages. The installed v1.0.3 remains unchanged.

### What is available

The public repository has four C# source files: AtmosphereRenderer, CylindricalAtmosphereModule, ShaderLoader, LightingMode. Its license explicitly says plugin code is GPL v3 and shader code is not public, provided only compiled. The release contains Cyla.dll and Shaders/cylashaders, but no shader source. A complete source transplant cannot be performed from this repository.

The C# renderer creates a quad with an enlarged bound and a Cyla/Atmo material at render queue 2990. It passes cylinder dimensions, centre and axis, Rayleigh/Mie scattering and scale heights, Mie asymmetry, integration counts, dithering, light colour and sun position to the shader. The part module exposes these through the stock part-action window, normalizes scattering coefficients by atmosphere thickness, and multiplies normalized scale heights by that thickness. It is part-attached rather than attached to a celestial habitat manager.

Quality controls are view integration steps (1–500, default 40), light/transmittance steps (1–50, default 10) and dithering. There is no public preset system, temporal accumulation, photo capture or half-resolution renderer to copy. Lighting modes are TransparentTopAndSide, TransparentFloor and Unlit. TransparentTopAndSide is the relevant Niven-ring choice; opaque side-wall and floor geometry need to match the habitat.

The public controller does not reveal the shader's scene-depth sampling, scattering integrator, ray/cylinder intersections or its detailed object occlusion. It locates SunLight/SpotlightSun, falling back to the brightest local directional light, and uses the body named Sun for illumination position. It does not visibly implement cloud weather, ring shadow squares, map-space integration, ocean optics, per-object light attenuation or our photo pipeline. Those capabilities must not be inferred from the screenshots. No runtime rendering compatibility test was performed.

### Integration work required

- Preserve GPL notices and provide corresponding source for any distributed GPL-derived controller. The current MIT-only package description cannot describe copied GPL code as wholly MIT. Shader redistribution/source terms need explicit clarification for a modified integrated renderer; the supplied license distinguishes the unavailable shader source.
- Use a ring-owned renderer, not a required craft part. Bind dimensions and orientation to the existing rotating ring frame and finite wall width. Keep atmospheric forces, parachutes, science and saves in the existing physical model.
- Resolve numerical precision before deployment: upstream passes centre/radii as float/Vector3. Around our 15.3-billion-metre radius, adjacent float values are about 1,024 m apart. This is an inferred integration risk for near-ground density/depth, not a measured Cyla failure. Our present renderer uses a camera-centred tangent chart. A shader port needs comparable stable coordinates; changing the C# adapter alone does not prove the compiled shader stable.
- Integrate scattering with opaque scene depth so aircraft, trees, cities, water and walls occlude it. Explicitly test transparent materials, local/scaled cameras, map entry/exit, ring outline, sun/planets and day/night stars. Queue 2990 alone does not establish correctness.
- Map view/light sample counts and dither controls into our eleven presets. Keep optical parameters separate from quality: changing quality must not change air pressure or weather. Add per-save advanced scattering controls only once the shader backend can consume them.
- Retain the existing cloud renderer and shared global-cloud field unless a new source implementation replaces them. Cyla's published C# is an atmosphere controller, not a cloud system.
- Adapt photo mode explicitly: freeze simulation/lighting/weather, use the frozen scene depth, accumulate controlled dither samples, retain terrain readiness, and restore camera/material state on cancel or resize. Cyla exposes a frame counter but no public photo pipeline.
- Test ground/horizon/orbit/map, water/craft silhouettes, rim lighting, shadow-square night transitions, camera shifts and photo cancellation on the laptop before making it the default. Compare images and frame/memory costs; visual improvement is not established by reading the source.

### Current blocking input

A full source port needs the Cyla atmosphere shader source and usable modification/distribution terms from its author. Alternatively, a separately installed compiled Cyla backend could be explored as an experimental dependency, but it cannot provide editable atmosphere code and may not support this ring scale or our photo composition. Neither path has been silently substituted for the requested full transplant.

Primary sources:
- https://github.com/LGhassen/Cyla/blob/92223648e0674212e488e6fb977b5cffc63be869/License.md
- https://github.com/LGhassen/Cyla/blob/92223648e0674212e488e6fb977b5cffc63be869/Cyla/AtmosphereRenderer.cs
- https://github.com/LGhassen/Cyla/blob/92223648e0674212e488e6fb977b5cffc63be869/Cyla/CylindricalAtmosphereModule.cs
- https://github.com/LGhassen/Cyla/releases/tag/1.1.0.0

