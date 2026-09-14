# Validation record

Target game: **KSP 1.12.5.3190**, Unity 2019.4.18f1, supplied Windows x64 installation in `template_instance`.

## Latest v0.4.0 feature regression

**PASS, September 14, 2026**, in `template_instance/RingworldSmoke-20260914-090614.log`. The current summary is `artifacts/validation/game-smoke.txt`.

- New-save seed 2110619744 survived configuration round-trip and scenario restoration.
- The 160,000 km / 16-segment horizon completed with 988 blocks, including 581 active scaled-space blocks.
- The Settings tab was captured and visually inspected. The surface sky capture shows the ring strip through the atmosphere; the craft obscures part of the view, so this is not a separate quantitative Sun-flare test.
- All 49 fixture parts remained; settled speed was 0.000914 m/s and restoration drift 0.2468 m.
- Water-camera clearance was 0.6999 m; the navball showed 0.0 m/s with stationary cues hidden.
- EVA recovered, walked 2.977 m initially, completed all six direction changes below 0.817 m/s, and survived rapid reversals at 301.57 K.
- Both Free and Chase EVA cameras measured zero rendered-frame rotation over 120 comparisons each.
- 61,636 core assertions passed, including a bounded 160,000 km quadtree and overhead atmospheric transmission.

The first attempt found an unavailable Unlit/Texture shader in the supplied game. The corrected renderer reuses the known working terrain shader with emission for distant illumination. The normal release was rebuilt and installed with zero compiler warnings/errors. The zero-haze boundary and alternative quality presets were compiled/reviewed, but the recorded game run used the default haze and balanced mesh quality. No laptop frame-rate certification is implied.

The new screenshots are `settings-panel.png`, `clear-sky-ring.png`, and the refreshed landing/cloud/EVA images under `artifacts/validation`. Near/far texture blending removes the unrelated palette-colour bands. Daytime stars, coarse silhouettes, black building sides and other lighting/geometry limitations remain visible.

## Automated core checks

The .NET test runner exercises 61,636 assertions, including:

- closed ground-shell topology, including opposite winding on every shared edge;
- adaptive LOD coverage at positive, negative, boundary and large ring coordinates, with no overlapping blocks or gaps outside the excluded fine-grid footprint;
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

The 50,176-sample terrain benchmark took approximately 100–121 ms in recent runs under the development .NET runtime. This is not a KSP frame-rate benchmark; Unity mesh creation, colliders, drawing, and the game's Mono runtime add costs.

## Game-level regression harness

Run `smoke-test.ps1`. It temporarily builds a diagnostic variant, starts the supplied isolated KSP copy, creates a uniquely named test save from a local stock tutorial fixture, and exercises a spin-matched departure and automatic approach, local atmospheric integration, then expedition entry, a controlled descent, collision settling, science collection/serialization, and scenario coordinate restore. The script always rebuilds and reinstalls the normal release variant after the test process exits.

The lander fixture uses damage immunity and a test-only descent controller. Before EVA, the harness disables crash-damage immunity and unbreakable joints. The Kerbal drops from the ladder, touches down, recovers from ragdoll and walks through KSP's `KeyBinding.GetKey` path. Six four-second directions include diagonal, sideways and backward motion, followed by twelve simulated seconds of rapid reversals. At five-metre zoom, the camera check compares 120 rendered frames in each of Free and Chase modes with camera input locked. This validates those paths; it does **not** certify a piloted mission or safe high-speed crash. No test controller or automatic test-save loading is compiled into the normal release. The copied tutorial fixture explicitly enables EVA and displays an automated-test banner.

**Previous v0.3.0 regression: PASS, September 14, 2026.**

| Measurement | Result |
|---|---:|
| Departure position / velocity error | 0.000000477 m / 0.003819 m/s |
| Automatic arrival altitude / relative speed | 209,923.4 m / 2,095.98 m/s |
| Air density / pressure around 30 km | 0.02903 kg/m³ / 1.806 kPa |
| Mach / dynamic pressure around 30 km | 0.4044 / 0.2067 kPa |
| Fixture parts retained | 49 |
| Settled center-of-mass height above terrain | 4.9863 m |
| Settled craft speed | 0.0006134 m/s |
| Krakensbane velocity-frame offset | 0 m/s |
| Stationary navball, both ordinary speed modes | Cues hidden; 0.0 m/s |
| Test scenario permits EVA | True |
| Saved vessel records / collected surveys | 1 / 1 |
| Survey serialization round trip | Pass |
| Displacement after scenario restore and 3 s settling | 0.2614 m |
| Underside ray hit from 10 m below shell | 9.99994 m |
| Camera clearance after starting below ground | 1.8000 m |
| Camera clearance above water datum | 0.7001 m |
| Ring altitude at rest | 4.9862 m |
| Distant terrain blocks | 599 |
| EVA peak speed during ladder drop/contact | 10.3412 m/s |
| EVA settled speed / parent craft drift | 0.003875 m/s / 0.04574 m |
| EVA recovery | Idle (Grounded), no ragdoll |
| Local horizontal speed / stock solar cache at rest | <0.000001 m/s / 222,529.48 m/s |
| Initial five-second walking distance | 2.9866 m |
| Maximum speed over six direction tests | 0.8161 m/s |
| Distance per four-second direction test | 2.37–2.77 m |
| Rapid reversals | Alive; 0.06747 m/s; 301.60 K |
| Free camera, maximum rendered frame rotation over 120 comparisons | 0° |
| Chase camera, maximum rendered frame rotation over 120 comparisons | 0° |

The camera result measures rendered orientation at rest at five-metre zoom with camera input locked. It does not establish zero positional jitter on every terrain surface, during all animations or under all camera settings. Camera shake effects are disabled only for ring EVA views; this does not suppress physical forces or damage.

The test deliberately relocates between high-altitude atmosphere samples and landing. It verifies automatic frame capture and individual atmospheric/landing stages, rather than a single uninterrupted pilot-controlled descent. EVA movement follows the real key-binding/FSM path; the test does not directly assign walking direction vectors or clamp production velocities.

The previous complete Unity log is `template_instance/RingworldSmoke-20260914-031509.log`. All 61,629 core assertions passed. The normal release was rebuilt and installed after the diagnostic game exited, with zero compiler warnings or errors.

## Defects found during development

- Real movement-key queries reproduced a launch to 14,450,664 m/s. Heading/bounding/landing callbacks copied Sun-relative `horizontalSrfSpeed` into walking interpolation. At rest, the inspected cache was 222,529 m/s while live local horizontal speed was essentially zero. Scoped field-read replacements correct these transition inputs and jump/landing decisions without a speed cap. The failed direct-key runs are preserved in `artifacts/validation/eva-launch-reproduction.txt`; earlier direct-vector walking tests had missed the state-transition defect.
- Camera traces showed a stable reference frame and zero translational velocity while separate stock camera effects perturbed local position/rotation. Ring EVA views now suppress both artificial shake sources. Local terrain heights/up replace Sun-based camera queries, and collision correction no longer feeds back into the next stock camera interpolation. Camera tests sample at end-of-frame to avoid counting intermediate parent-body transformations as rendered motion.
- Stock solar navball cues were misleading on contact. The ring display now uses actual local velocity, suppresses undefined stationary directions below the stock threshold, and excludes solar normal/radial cues. Test-scenario EVA denial came from the tutorial's retained `CanEVA = false` parameter; it is explicitly enabled in the diagnostic fixture without changing player career/scenario rules.
- Ground tiles were zero-thickness sheets. Closed collision shells and visible undersides now provide structure beneath the terrain. Camera clearance also handles a starting position below the floor.
- EVA focus changes could temporarily lose rotating-frame ownership, advance the terrain phase and apply an arrival velocity conversion again. Scene-wide ownership, crew-event registration and a guarded handoff preserve the existing frame.
- Stock EVA contact, movement and ragdoll forces assumed a spherical planet. Scoped ring adapters now supply physical ground contact, inward up, walking velocity and rotating-frame ragdoll forces. The game regression verifies contact with damage enabled, recovery and walking.
- Known participants remain protected from stock solar terrain anchoring while scenario restoration is pending.

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

Not certified: all aircraft, all wheels, advanced EVA interactions (including swimming, boarding and long-distance traversal), full-game save/restart across arbitrary scenes, unloaded fleet simulation, docking/staging permutations, extreme-speed flight/impact, arbitrary interplanetary encounter trajectories, mod interoperability, science transmission interrupted by a lost connection, and performance across hardware. See `KNOWN-LIMITATIONS.md`.

## Visual inspection

The game-produced `orbital-atmosphere.png`, `cloud-approach.png`, `in-game-landing.png`, and `eva-ground-regression.png` are preserved under `artifacts/validation`. They show the actual running mod, not generated concept art. The expanded distant mountains and textured cloud layers are visible. Remaining visual issues include coarse silhouettes, palette bands/LOD transitions, overly bright ground versus dark spacecraft/building sides, and imperfect atmospheric exposure. The broad starter valley removes the previous steep mountain pit around the landing pad. This is a functional terrain/LOD prototype, not a finished visual overhaul.
