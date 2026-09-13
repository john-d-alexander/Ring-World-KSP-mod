# Validation record

Target game: **KSP 1.12.5.3190**, Unity 2019.4.18f1, supplied Windows x64 installation in `template_instance`.

## Automated core checks

The .NET test runner exercises 20,751 assertions, including:

- double-precision ring coordinate round trips across the full circumference and width;
- correct outward centrifugal acceleration and its variation with distance from the axis;
- reversible inertial/rotating velocity conversion, elapsed phase and material-longitude consistency;
- capture hysteresis, a swept fast approach, and preservation of unmatched encounter speed;
- dry-air pressure and rim/backside vacuum, finite optical depth, blue Rayleigh scattering, atmosphere visibility from space and periodic cloud coverage;
- Coriolis acceleration perpendicular to velocity;
- a ten-second freely falling trajectory and antispinward deflection;
- terrain determinism, ring seam continuity, and river-catchment boundary continuity;
- atmosphere density cutoff and monotonic decrease;
- shadow-square cadence, darkness at each of the twenty square centers, and daylight between squares;
- dry, level research sites and a lake at the waterway destination.

The 50,176-sample terrain benchmark typically took approximately 56–78 ms under the development .NET runtime. This is not a KSP frame-rate benchmark; Unity mesh creation, colliders, drawing, and the game's Mono runtime add costs.

## Game-level regression harness

Run `smoke-test.ps1`. It temporarily builds a diagnostic variant, starts the supplied isolated KSP copy, creates a uniquely named test save from a local stock tutorial fixture, and exercises a spin-matched departure and automatic approach, local atmospheric integration, then expedition entry, a controlled descent, collision settling, science collection/serialization, and scenario coordinate restore. The script always rebuilds and reinstalls the normal release variant after the test process exits.

The fixture uses damage immunity and a test-only descent controller. This validates collision response and integrations; it does **not** certify a piloted mission or safe high-speed crash. No test controller or automatic test-save loading is compiled into the normal release.

**Final regression: PASS, September 13, 2026.**

| Measurement | Result |
|---|---:|
| Departure position error | 0 m (within recorded precision) |
| Departure velocity error | 0.00382 m/s |
| Automatic arrival altitude | 209,923.4 m |
| Air-relative arrival speed | 2,095.98 m/s |
| Air density around 30 km | 0.02903 kg/m³ |
| Static pressure around 30 km | 1.806 kPa |
| Mach / dynamic pressure around 30 km | 0.4044 / 0.2067 kPa |
| Fixture parts retained | 49 |
| Settled center-of-mass height above sampled terrain | 4.986 m |
| Settled speed | 0.00102 m/s |
| Krakensbane velocity-frame offset | 0 m/s |
| Saved vessel records | 1 |
| Collected surveys | 1 |
| Survey serialization round trip | Pass |
| Displacement after scenario restore and 3 s settling | 0.266 m |

The test deliberately relocates between the high-altitude atmosphere sample, cloud screenshots and landing fixture. It verifies automatic frame capture and individual atmospheric/landing stages; it is not a single uninterrupted, pilot-controlled atmospheric descent.

The outcome is recorded in `artifacts/validation/game-smoke.txt`. The complete Unity log is `template_instance/RingworldSmoke-20260913-163454.log`. The final daylight calculation and all 20,751 core assertions passed in this build. The normal release was rebuilt and installed after the diagnostic game exited, with zero compiler warnings or errors.

## Defects found during development

- A .NET Standard-only core assembly failed to load in KSP; the game now receives a .NET Framework 4.7.2 build.
- Stock wheel contact dereferenced the Sun's nonexistent PQS; a scoped Harmony prefix keeps custom ring contact out of stock planetary anchoring.
- A distant float Transform placement lost precision; the floating origin is shifted before craft placement.
- Krakensbane's velocity frame activated at the ring floor because the stock altitude is solar altitude, destabilizing contact. It is now disabled for expedition craft while ordinary floating-origin shifts remain enabled.
- Frame handoff initially read stale cached KSP position/velocity fields. It now reads mass-weighted live rigidbody data; the game checks the inertial result against the expected rotation/velocity conversion.
- Solar terrain cannot provide ring camera clearance. The ring now preserves the camera frame-mode field and sweeps against streamed colliders. An inverted screenshot was also traced to requesting capture immediately before scenario restoration; the harness now waits for capture before restoring, and the stable surface image is upright.
- Readiness could be reported while KSP was still unpacking a relocated craft; it now requires an unpacked, restored vessel.
- Saved center-of-mass coordinates were passed to a root-position placement API. Save format 2 consistently records vessel-root coordinates.

An earlier controlled test exposed a 31 m restore error from mixing center-of-mass and root coordinates. The final regression specifically verifies the corrected root-coordinate save format.

## Test limitations

The legacy tutorial fixture can produce stock alarm-clock initialization, PQS teardown, and application-shutdown exceptions. These are distinguishable from the corrected repeated `Vessel.checkLanded` exceptions. A `PASS` marker alone is not a claim that the entire Unity log is warning-free.

Not certified: all aircraft, all wheels/EVA locomotion, full-game save/restart across arbitrary scenes, unloaded fleet simulation, docking/staging permutations, extreme-speed flight/impact, arbitrary interplanetary encounter trajectories, mod interoperability, science transmission interrupted by a lost connection, and performance across hardware. See `KNOWN-LIMITATIONS.md`.

## Visual inspection

The game-produced `orbital-atmosphere.png`, `cloud-approach.png`, and `in-game-landing.png` are preserved under `artifacts/validation`. They show the actual running mod, not generated concept art. Cloud sheets and a blue scattering sky render, but the distant terrain is visibly coarse, sheet boundaries can be apparent, and starfield/exposure handling needs further work.
