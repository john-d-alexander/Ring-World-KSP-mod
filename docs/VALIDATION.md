# Validation record

Target game: **KSP 1.12.5.3190**, Unity 2019.4.18f1, supplied Windows x64 installation in `template_instance`.

## Automated core checks

The .NET test runner exercises 20,339 assertions, including:

- double-precision ring coordinate round trips across the full circumference and width;
- correct outward centrifugal acceleration and its variation with distance from the axis;
- reversible inertial/rotating velocity conversion;
- Coriolis acceleration perpendicular to velocity;
- a ten-second freely falling trajectory and antispinward deflection;
- terrain determinism, ring seam continuity, and river-catchment boundary continuity;
- atmosphere density cutoff and monotonic decrease;
- shadow-square cadence, darkness at each of the twenty square centers, and daylight between squares;
- dry, level research sites and a lake at the waterway destination.

The 50,176-sample terrain benchmark typically took approximately 56–78 ms under the development .NET runtime. This is not a KSP frame-rate benchmark; Unity mesh creation, colliders, drawing, and the game's Mono runtime add costs.

## Game-level regression harness

Run `smoke-test.ps1`. It temporarily builds a diagnostic variant, starts the supplied isolated KSP copy, creates a uniquely named test save from a local stock tutorial fixture, and exercises expedition entry, a controlled descent, collision settling, science collection/serialization, and scenario coordinate restore. The script always rebuilds and reinstalls the normal release variant after the test process exits.

The fixture uses damage immunity and a test-only descent controller. This validates collision response and integrations; it does **not** certify a piloted mission or safe high-speed crash. No test controller or automatic test-save loading is compiled into the normal release.

**Final regression: PASS, September 13, 2026.**

| Measurement | Result |
|---|---:|
| Fixture parts retained | 49 |
| Settled center-of-mass height above sampled terrain | 4.986 m |
| Settled speed | 0.00205 m/s |
| Krakensbane velocity-frame offset | 0 m/s |
| Saved vessel records | 1 |
| Collected surveys | 1 |
| Survey serialization round trip | Pass |
| Displacement after scenario restore and 3 s settling | 0.151 m |

The outcome is recorded in `artifacts/validation/game-smoke.txt`. The complete Unity log is `template_instance/RingworldSmoke-20260913-112939.log`. The final daylight calculation and all 20,339 core assertions passed in this build. The normal release was rebuilt and installed after the diagnostic game exited, with zero compiler warnings or errors.

## Defects found during development

- A .NET Standard-only core assembly failed to load in KSP; the game now receives a .NET Framework 4.7.2 build.
- Stock wheel contact dereferenced the Sun's nonexistent PQS; a scoped Harmony prefix keeps custom ring contact out of stock planetary anchoring.
- A distant float Transform placement lost precision; the floating origin is shifted before craft placement.
- Krakensbane's velocity frame activated at the ring floor because the stock altitude is solar altitude, destabilizing contact. It is now disabled for expedition craft while ordinary floating-origin shifts remain enabled.
- Saved center-of-mass coordinates were passed to a root-position placement API. Save format 2 consistently records vessel-root coordinates.

An earlier controlled test exposed a 31 m restore error from mixing center-of-mass and root coordinates. The final regression specifically verifies the corrected root-coordinate save format.

## Test limitations

The legacy tutorial fixture can produce stock alarm-clock initialization, PQS teardown, and application-shutdown exceptions. These are distinguishable from the corrected repeated `Vessel.checkLanded` exceptions. A `PASS` marker alone is not a claim that the entire Unity log is warning-free.

Not certified: all aircraft, all wheels/EVA locomotion, full-game save/restart across arbitrary scenes, unloaded fleet simulation, docking/staging permutations, high-speed flight or exit, ordinary interplanetary arrival, mod interoperability, science transmission interrupted by a lost connection, and performance across hardware. See `KNOWN-LIMITATIONS.md`.
