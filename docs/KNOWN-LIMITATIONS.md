# Prototype boundaries and open engineering work

This is version 0.9.0. The project is not a completed seamless Ringworld planet pack.

## Physics and KSP integration

- Expedition mode reinterprets opted-in vessels in a frame rotating with the ring. It does not globally replace KSP's celestial/orbital reference frames. The stock Sun remains the SOI.
- Automatic arrival and departure now convert loaded solar vessels between inertial flight and a rotating chart, preserving linear/angular motion and elapsed phase. The star stays the SOI. General multi-vessel, packed-vessel and map reference-frame transformations remain incomplete; this is an experimental local flight transition, not a global celestial-frame replacement. The training approach is explicitly a relocation aid.
- The camera and local orientation vectors are adapted to the inward-facing floor. The flight altimeter shows local ground/water clearance; stock orbit statistics, science situation, and recovery rules remain solar; a custom vacuum map curve replaces the active solar spline. Navball attitude, prograde/retrograde and speed use the ring physics frame; solar normal/radial markers are hidden. Target speed requires another ring participant. These UI changes do not implement ring-aware SAS prograde/retrograde guidance. EVA has explicit ring-relative orientation, surface-contact, movement, state-transition speed and ragdoll adapters. Regression is recorded in VALIDATION.md; all wheel suspension modes and complex EVA interactions have not been certified.
- A Harmony prefix bypasses the stock planetary landing-state calculation for expedition vessels. This avoids dereferencing the Sun's absent PQS terrain and prevents spherical anchoring. Physical contact with the ring is not a stock `LANDED` situation.
- Stock high warp is supported for resting dry ring expeditions through a custom rails anchor. Airborne/physics warp inside the ring frame remains unavailable. Third-party background-resource behaviour and arbitrary scene changes during anchored warp are not certified. See STOCK-WARP-AND-RENDERING.md.
- Local dry-air pressure, density, temperature, sound speed and dynamic pressure now feed FlightIntegrator and part drag cubes. Stock shock/convection calculations use home-body dry-air constants within this integrator only. Wings can receive native aerodynamic inputs, but aircraft handling, oxygen intakes, parachute deployment gates and heating envelopes are not certified. Water still uses approximate displacement and damping, not hull-volume buoyancy.
- Ribbon gravity now applies to unpacked solar vessels and active packed solar flight; other SOIs and inactive/unloaded craft still use stock or saved propagation. EVA inherits its parent frame through the crew-EVA event; nearby separated stages are adopted within 250 m. Arbitrary fleets, docking changes, active-vessel switching, debris at long range, and unloaded trajectories require more lifecycle work.
- Scenario state records positions, velocities, and orientations. Vessels outside loaded simulation are intended to freeze at saved ring coordinates, not advance on stock conics. Complete save/reload and multi-vessel persistence need game-level regression tests. Avoid changing the seed or dimensions mid-save.
- A craft leaving the frame inherits the ring's real tangential velocity. At default settings that is hundreds of km/s, beyond the operating envelope of many KSP physics systems. Very high speed collision/atmospheric flight remains outside the validated envelope; the frame handoff never supplies free braking.

## Terrain and visuals

- Optional full-ring surface colour has no distance cutoff, but it is a macro approximation on the global ribbon. It does not extend detailed terrain, collisions, buildings or volumetric clouds around the circumference. At physical dimensions the arc narrows quickly; increasing range alone cannot reproduce the proportions of an artist illustration.

- Streamed tiles now have closed top/bottom/side collision meshes. Their flat underside is below the deepest generated ocean floor, with a configurable structural margin. This does not create persistent colliders around the entire circumference. High-resolution collision coverage is finite around the active vessel. It is not safe to assume collision coverage for remote craft or very fast low flight.
- Trees and buildings are simple procedural primitives. Buildings have collidable exteriors, not authored room interiors or residents.
- Rivers are carved analytical channels; water patches are opaque mesh surfaces with approximate shores. Optional GPU water adds Fresnel sky reflections, filtered normal ripples, shoreline foam and visual displacement. Buoyancy remains at mean water level; there are no currents or global drainage simulation. Object/shore reflections are not implemented.
- Rim walls have collision panels. Monument mountains are terrain approximations; the puncture mountain is a depression, not a fully open hole through a layered scrith shell.
- Adaptive CPU terrain LOD can extend to 2,000,000 km around the observer, using scaled-space meshes at long range, with finite collision coverage near the vessel. Initial generation can take tens of seconds; coarse sampling, skirts and transition popping remain possible. The full scaled ring has an optional coarse procedural colour layer and analytical night bands. Shadow-square phase drives terrain/cloud/local-light brightness and the flight integrator solar-flux multiplier, but does not replace every stock stellar-light or solar-panel occlusion path.
- Krakensbane's velocity-frame shifting is disabled for the active expedition while floating-origin position shifts remain enabled. This avoids contact instability from moving custom terrain colliders, but limits the practical precision of very fast atmospheric flight.
- The laptop atmospheric visuals use numerically integrated single scattering (Rayleigh plus a Henyey-Greenstein Mie approximation), finite cylindrical boundaries, extinction of the background sky, and local shadow-square illumination. They do not implement multiple scattering or depth-aware aerial perspective over terrain. The three textured, uneven translucent cloud decks are not true volumetric clouds; sheet intersections and finite patch boundaries may be visible. Seeded weather, local rain and lightning are visual only; there is no fluid weather simulation, wind force, terrain cloud-shadow map or detailed night lighting. The opaque daytime overlay is absent; a scoped galaxy-cube fade suppresses daytime stars without masking the ring, Sun, panels or planets. The visual atmosphere currently activates within 600 km of the floor, not over the entire scaled ring from interplanetary distances.

- Optional High/Ultra and photo mode use GPU raymarched 3-D clouds, self-shadowing and depth-clipped per-pixel atmosphere. This is an original bounded renderer, not EVE/Scatterer integration or a full multiple-scattering/weather solution. Photo mode refines existing terrain at the configured horizon and accumulates volumetrics at current viewport resolution. See HIGH-END-VISUALS.md for supported API, rendering limits and third-party references.

## Science

- The RW-1 creates uniquely tagged Ringworld subjects using the Sun as the KSP body owner. This avoids adding a fictitious spherical Ringworld body but means the research archive may categorize the data under the Sun.
- Data follows stock science containers and transmitters. No direct infinite-science reward button is used. Each site or biome is limited by its subject cap and stock diminishing returns.
- The survey is currently proximity-gated, not contact-gated: it can be run below 120 m above the floor. A landing is not required.
- The review dialog's lab action currently explains transferring data to a container; direct lab processing from this custom module is not implemented.

## Validation

See `VALIDATION.md` for the recorded build, mathematical checks, game-load evidence, and unverified cases. A passing compilation is not proof of a safe landing or a robust save lifecycle.

Save-specific settings and migration limits are described in SETTINGS-AND-HORIZON.md. Small trees/buildings have no long-range impostors, and laptop performance has not been certified.


## 0.6 ecology and close detail

The new biome terrain is generator 4 and does not replace saved v1-v3 landscapes. Grass, stones, sand, litter and flowers are small procedural placeholders; sunflower beams, real soil layers, drainage simulation, crashed-city belts, villages, rim access repetition and industrial scatters remain future systems. See ASSET-TRACKER.md. Ground cover has no collision or individual shadows. Distant LOD remains terrain/water only. Native scatter settings govern foliage. Laptop costs depend on resolution, trees, shadows, scenery density and streaming; the horizon distance is not a guarantee of a particular frame rate.

Weather is seeded visual evolution, not a fluid climate model. High warp suppresses brief individual lightning flashes and replaces rain streaks with a light veil. See WEATHER-AND-NIGHT.md for controls and limits.
