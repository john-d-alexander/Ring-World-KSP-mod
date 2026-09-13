# Orbital arrival and atmosphere

## Physical model

The default ring radius is 15.3 billion metres. Its angular velocity is sqrt(9.72/R), giving a floor speed of 385,637 m/s. A normal solar orbit is not a velocity-matched rendezvous. Automatic entry preserves that difference; it cannot make an ordinary encounter survivable by removing kinetic energy.

The loaded flight scene uses a rotating chart near the ring. Its axes coincide with inertial axes at entry time t0, avoiding an instantaneous position rotation at arrival. Material longitude zero has orientation omega*t0 in that chart. At elapsed time dt:

    x_inertial = Q(omega*dt) x_chart
    v_inertial = Q(omega*dt) (v_chart + omega cross x_chart)

The frame conversion applies to every loaded, unpacked solar vessel, including each rigidbody's linear and angular velocity. Positions are shifted through FloatingOrigin before conversion to Unity floats. Live rigidbody centre-of-mass and velocity sums are used because KSP's cached vessel fields can lag setters until its next precalculation tick. Scenario format 3 stores the chart epoch alongside each vessel's root position and orientation. Existing format 2 records default to epoch zero.

Entry occurs inside a 50 km margin around the ribbon: datum altitude -50 to 210 km, across-coordinate within half-width plus 50 km. Departure uses 100 km margins, giving an outer altitude of 260 km. The 50 km hysteresis avoids boundary chatter. A two-second cooldown follows explicit departure. The approach predictor intersects a linear trajectory with cylindrical and rim-side boundaries to reduce warp ahead of entry. This is not a guarantee against every extreme-warp curved trajectory.

In the rotating chart, stellar gravity, centrifugal acceleration, and Coriolis acceleration act on each physics rigidbody. Stock solar gravity is subtracted before the additional force is applied, avoiding double counting. Krakensbane velocity offsets are disabled while on the ring, but floating-origin position shifts remain enabled. Surface terrain renders before capture and uses the same material phase on both sides of the handoff. Camera frame bookkeeping is retained, and a sphere sweep from the craft toward the camera limits clipping through custom terrain and buildings.

## Air and visuals

Air occupies the finite inward-facing cylinder between the floor datum and 60 km, bounded by the rim walls. Density is an exponential with an 8 km scale height and a smooth five-kilometre taper to vacuum. A dry-air lapse rate falls from 288.15 K to a 216.65 K floor. Pressure follows rho*R_specific*T; sound speed follows sqrt(gamma*R_specific*T). The profile is a configurable gameplay approximation, not a claim about a fully simulated Ringworld climate.

Scoped Harmony patches provide FlightIntegrator and drag-cube parts with local pressure, density, temperature, Mach number and dynamic pressure. Shock/convection calculations use stock home-body dry-air constants only within the active integrator. The shared Sun is never turned into an atmospheric celestial body. The former isotropic air-drag force is removed, so native aerodynamic drag is not doubled. Buoyancy remains the earlier approximate water model.

Sky radiance is integrated over ray intersections with the cylindrical atmosphere. The implementation uses 32 midpoint samples per intersected segment, RGB Rayleigh coefficients, an exponentially stratified aerosol component, and a Henyey-Greenstein Mie phase function. Background extinction and local shadow-square illumination are included. A low-resolution, vertex-coloured sky mesh is refreshed at five updates per second. This implements single scattering, not multiple scattering or depth-aware fog over terrain.

The radiative-transfer concepts are described in [Bruneton and Neyret's atmospheric scattering work](https://ebruneton.github.io/precomputed_atmospheric_scattering/). This project's CPU cylindrical integrator is original code; it does not incorporate that project's spherical precomputed implementation.

Three translucent procedural cloud decks occupy 4.8, 5.45 and 6.1 km. Material-periodic coverage, slow drift, mountain exclusion, soft opacity and patch-edge fading avoid a static skybox. These are fly-through sheets with depth separation, not volumetric clouds or weather. Clouds cover a 600 km local patch. Visual atmosphere is enabled within 600 km of the floor; the distant scaled ring still uses its earlier coarse ribbon representation.

## Trying it

Launch a rocket-equipped lander in the isolated instance. Open the ring panel with Left Alt+R, select a destination, and use **Training: set up a spin-matched approach**. This explicitly relocates the craft to 250 km, matches the spin and supplies a 1 km/s descent. The craft then leaves the local chart and approaches in solar flight; subsequent capture is automatic. Brake and fly the descent yourself. The training setup is not part of a naturally flown arrival.

General fleet persistence, packed vessels, map trajectories inside the rotating chart, air-breathing engine gates, parachute deployment gates and extreme-speed impacts remain open integration work. See KNOWN-LIMITATIONS.md and the actual game-run evidence in VALIDATION.md.
