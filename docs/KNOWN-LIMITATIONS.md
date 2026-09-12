# Prototype boundaries and open engineering work

This is version 0.1.0. The project is not a completed seamless Ringworld planet pack.

## Physics and KSP integration

- Expedition mode reinterprets opted-in vessels in a frame rotating with the ring. It does not globally replace KSP's celestial/orbital reference frames. The stock Sun remains the SOI.
- Entry is an explicit relocation command. There is no validated seamless interplanetary approach, rendezvous, surface capture, or re-entry corridor.
- The camera and local orientation vectors are adapted to the inward-facing floor. The stock altimeter, map orbit, science situation, and recovery rules remain solar. The ring panel provides surface height and velocity. Stock EVA walking orientation and all wheel suspension modes have not been certified.
- A Harmony prefix bypasses the stock planetary landing-state calculation for expedition vessels. This avoids dereferencing the Sun's absent PQS terrain and prevents spherical anchoring. Physical contact with the ring is not a stock `LANDED` situation.
- Physics warp and orbital warp are suppressed during an expedition. Long-range travel uses the destination relocations.
- Air uses altitude-dependent isotropic drag. Water uses an approximate displacement model and drag. Neither is a full FlightIntegrator atmosphere/ocean implementation; stock lift, jet oxygen intake, parachute deployment, heating, swimming, pressure, and buoyancy by actual hull volume need adapters.
- Only loaded, unpacked expedition vessels receive custom forces. Nearby separated stages/EVAs are adopted within 250 m. Arbitrary fleets, docking changes, active-vessel switching, debris at long range, and unloaded trajectories require more lifecycle work.
- Scenario state records positions, velocities, and orientations. Vessels outside loaded simulation are intended to freeze at saved ring coordinates, not advance on stock conics. Complete save/reload and multi-vessel persistence need game-level regression tests. Avoid changing the seed or dimensions mid-save.
- A craft leaving the frame inherits the ring's real tangential velocity. At default settings that is hundreds of km/s, beyond the operating envelope of many KSP physics systems. Exit has not been validated as a general mission workflow.

## Terrain and visuals

- High-resolution collision coverage is finite around the active vessel. It is not safe to assume collision coverage for remote craft or very fast low flight.
- Trees and buildings are simple procedural primitives. Buildings have collidable exteriors, not authored room interiors or residents.
- Rivers are carved analytical channels; water patches are opaque mesh surfaces with approximate shores. There are no currents or global drainage simulation.
- Rim walls have collision panels. Monument mountains are terrain approximations; the puncture mountain is a depression, not a fully open hole through a layered scrith shell.
- The scaled ring is a coarse uniform ribbon, not a seamless terrain LOD from every viewing distance. Shadow-square mesh motion and local daylight are illustrative and require visual alignment work. Local terrain brightness changes do not replace stock stellar lighting or affect solar-panel generation.
- There is no full atmospheric scattering, weather, volumetric cloud system, detailed night lighting, starfield occlusion solution, or procedural texture asset library.

## Science

- The RW-1 creates uniquely tagged Ringworld subjects using the Sun as the KSP body owner. This avoids adding a fictitious spherical Ringworld body but means the research archive may categorize the data under the Sun.
- Data follows stock science containers and transmitters. No direct infinite-science reward button is used. Each site or biome is limited by its subject cap and stock diminishing returns.
- The survey is currently proximity-gated, not contact-gated: it can be run below 120 m above the floor. A landing is not required.
- The review dialog's lab action currently explains transferring data to a container; direct lab processing from this custom module is not implemented.

## Validation

See `VALIDATION.md` for the recorded build, mathematical checks, game-load evidence, and unverified cases. A passing compilation is not proof of a safe landing or a robust save lifecycle.
