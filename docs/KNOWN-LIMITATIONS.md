# Prototype boundaries and open engineering work

This is version 0.2.0. The project is not a completed seamless Ringworld planet pack.

## Physics and KSP integration

- Expedition mode reinterprets opted-in vessels in a frame rotating with the ring. It does not globally replace KSP's celestial/orbital reference frames. The stock Sun remains the SOI.
- Automatic arrival and departure now convert loaded solar vessels between inertial flight and a rotating chart, preserving linear/angular motion and elapsed phase. The star stays the SOI. General multi-vessel, packed-vessel and map reference-frame transformations remain incomplete; this is an experimental local flight transition, not a global celestial-frame replacement. The training approach is explicitly a relocation aid.
- The camera and local orientation vectors are adapted to the inward-facing floor. The stock altimeter, map orbit, science situation, and recovery rules remain solar. The ring panel provides surface height and velocity. Stock EVA walking orientation and all wheel suspension modes have not been certified.
- A Harmony prefix bypasses the stock planetary landing-state calculation for expedition vessels. This avoids dereferencing the Sun's absent PQS terrain and prevents spherical anchoring. Physical contact with the ring is not a stock `LANDED` situation.
- Physics warp and orbital warp are suppressed during an expedition. Long-range travel uses the destination relocations.
- Local dry-air pressure, density, temperature, sound speed and dynamic pressure now feed FlightIntegrator and part drag cubes. Stock shock/convection calculations use home-body dry-air constants within this integrator only. Wings can receive native aerodynamic inputs, but aircraft handling, oxygen intakes, parachute deployment gates and heating envelopes are not certified. Water still uses approximate displacement and damping, not hull-volume buoyancy.
- Only loaded, unpacked expedition vessels receive custom forces. Nearby separated stages/EVAs are adopted within 250 m. Arbitrary fleets, docking changes, active-vessel switching, debris at long range, and unloaded trajectories require more lifecycle work.
- Scenario state records positions, velocities, and orientations. Vessels outside loaded simulation are intended to freeze at saved ring coordinates, not advance on stock conics. Complete save/reload and multi-vessel persistence need game-level regression tests. Avoid changing the seed or dimensions mid-save.
- A craft leaving the frame inherits the ring's real tangential velocity. At default settings that is hundreds of km/s, beyond the operating envelope of many KSP physics systems. Very high speed collision/atmospheric flight remains outside the validated envelope; the frame handoff never supplies free braking.

## Terrain and visuals

- High-resolution collision coverage is finite around the active vessel. It is not safe to assume collision coverage for remote craft or very fast low flight.
- Trees and buildings are simple procedural primitives. Buildings have collidable exteriors, not authored room interiors or residents.
- Rivers are carved analytical channels; water patches are opaque mesh surfaces with approximate shores. There are no currents or global drainage simulation.
- Rim walls have collision panels. Monument mountains are terrain approximations; the puncture mountain is a depression, not a fully open hole through a layered scrith shell.
- The scaled ring is a coarse uniform ribbon, not a seamless terrain LOD from every viewing distance. Shadow-square mesh motion and local daylight are illustrative and require visual alignment work. Local terrain brightness changes do not replace stock stellar lighting or affect solar-panel generation.
- Krakensbane's velocity-frame shifting is disabled for the active expedition while floating-origin position shifts remain enabled. This avoids contact instability from moving custom terrain colliders, but limits the practical precision of very fast atmospheric flight.
- Atmospheric visuals use numerically integrated single scattering (Rayleigh plus a Henyey-Greenstein Mie approximation), finite cylindrical boundaries, extinction of the background sky, and local shadow-square illumination. They do not implement multiple scattering or depth-aware aerial perspective over terrain. The three translucent cloud decks are not true volumetric clouds; sheet intersections and finite patch boundaries may be visible. There is no global weather simulation, cloud shadowing, or detailed night lighting. The visual atmosphere currently activates within 600 km of the floor, not over the entire scaled ring from interplanetary distances.

## Science

- The RW-1 creates uniquely tagged Ringworld subjects using the Sun as the KSP body owner. This avoids adding a fictitious spherical Ringworld body but means the research archive may categorize the data under the Sun.
- Data follows stock science containers and transmitters. No direct infinite-science reward button is used. Each site or biome is limited by its subject cap and stock diminishing returns.
- The survey is currently proximity-gated, not contact-gated: it can be run below 120 m above the floor. A landing is not required.
- The review dialog's lab action currently explains transferring data to a container; direct lab processing from this custom module is not implemented.

## Validation

See `VALIDATION.md` for the recorded build, mathematical checks, game-load evidence, and unverified cases. A passing compilation is not proof of a safe landing or a robust save lifecycle.
