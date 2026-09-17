# Version 0.5 validation



Core verification: **62,453 checks**. Added independent analytic axial-field, distant point-mass, near-sheet, quadrature-convergence, angular-momentum, Kepler-orbit and rotating/inertial agreement checks. Generation v3 pond existence and millimetre-scale periodic precision are checked; the two-million-km LOD plan stays below 2,200 blocks. The legacy terrain regression remains unchanged. Typical legacy 50,176-sample benchmark: 138-174 ms on this development machine; this is not a game FPS benchmark.



Full KSP regression passed in `template_instance/RingworldSmoke-20260914-105525.log`:



- Active packed solar coast: 401 simulated seconds, 0.0000213 m deviation from a 0.5-second-step numerical reference.

- Numerical map approach: 41 samples, atmosphere entry approximately 179.1 seconds ahead.

- Departure state: 0.0000156 m position and 0.0106 m/s velocity error.

- Atmospheric flight: positive native density, pressure, Mach and dynamic pressure.

- Resting surface time: 2,338.97 seconds advanced, 0.00113 m craft drift. Motion guard stopped acceleration.

- Restored expedition displacement: 0.274 m. Underside/water-camera checks passed.

- Horizon: 988 completed blocks, including 581 scaled-space blocks at the default 160,000 km range.

- EVA rapid reversals survived at 301.57 K; peak directional speeds approximately 0.816 m/s. Both FREE and CHASE camera tests reported zero frame wobble.

- Advanced dimensions, mass, one-minute cycle and two-million-km render limit passed option roundtrip checks.



The first close-up map screenshot exposed terrain occlusion of the 3D line. The final overlay is tested separately with `smoke-test.ps1 -MapOnly`; its report is `artifacts/validation/map-smoke.txt`. The focused run passed in `RingworldSmoke-20260914-110312.log`; the cyan curve is visibly readable over the solid ring in `artifacts/validation/map-trajectory.png`. It also repeated a 400-second packed coast within 0.0000203 m of the reference. This focused run does not replace the full physics regression. The final small changes also refresh the global ring after save settings load and preserve legacy scenery silhouettes; imported Blender models are not present and their export pipeline remains untested.



Not certified: arbitrary docking/fleet/unloaded transitions, planetary encounter predictions, maximum native orbital warp, custom world dimensions in long play sessions, true accelerated resource simulation, aerodynamic/thrust prediction, and laptop frame rates at maximum range. Map counts alone do not establish visual legibility; inspect the final map screenshot as well.



---



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

| Air density / pressure around 30 km | 0.02903 kg/mÂ³ / 1.806 kPa |

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

| Free camera, maximum rendered frame rotation over 120 comparisons | 0Â° |

| Chase camera, maximum rendered frame rotation over 120 comparisons | 0Â° |



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





## 0.6 biome / ground-cover regression — 2026-09-14



Core verification: 67,954 checks, including continuous normalized climate weights, periodic climate distribution, all five climate families, gentle ordinary v4 heights, and 100 repeatable dry random-site searches. The printed 50,176-sample timing remains the legacy terrain benchmark, not an in-game FPS result.



Full runtime log `template_instance/RingworldSmoke-20260914-112522.log` passed arrival, landing, surface waiting, save restoration, camera clearance, navball stability and EVA movement. Ground detail produced 1,266 clumps identically on repeated forced rebuilds, disappeared with native scatters disabled, and terrain textures had mip chains. Rapid EVA reversals ended alive at 301.56 K; FREE/CHASE camera wobble metrics were zero. This controlled fixture does not certify every player craft or arbitrary terrain collision.



Random-location/laptop log `RingworldSmoke-20260914-113247.log` passed two relocations: both began at zero ring-relative speed; the second moved about 210,211 km and retained the terrain seed. At 1280x720, AA 2, texture mip limit 1, native scatter 35%, low distant mesh resolution, one block per frame and 75 m detail range, a 20-second nighttime sample recorded 660 frames, mean 30.31 ms, p95 63.80 ms, 388 cover clumps and no pending LOD blocks. This is about 33 FPS for this particular scene, with visible frame-time variation; it is not hardware certification or a guarantee at the original 2880x1920 resolution. Startup included Unity temporary-allocation warnings and the known stock tutorial-fixture exceptions; the complete log is not warning-free.



Original game preferences were backed up to `artifacts/validation/settings-before-laptop-test.cfg`. The authorized testing profile uses 1280x720 windowed, UI scale 1, two shadow cascades, four pixel lights, native scatter enabled at 35%, retaining AA 2 and half-resolution textures. Ringworld itself does not silently override these global preferences. The test harness alone performs artificial relocation/daylight setup and is excluded from normal releases.





Daylight follow-up `RingworldSmoke-20260914-113642.log` passed with the final new-save default of 8 LOD subdivisions. Over 20 seconds it recorded 633 frames, mean 31.88 ms, p95 63.46 ms, 378 cover clumps and zero queued LOD blocks. Both random transfers again began at zero ring-relative speed. The fixture advanced UT to daylight; the actual random-visit button does not change UT. The screenshot `artifacts/validation/random-terrain-daylight.png` was inspected: sand-detail placement and distant scenery are visible, but flat colour, simple triangular patches, overbright terrain and coarse atmosphere remain visual limitations for the later art pass.



The installed and staged normal 0.6 DLL hashes match. Assembly inspection found no SmokeTest type in the normal installed DLL. CRLF-aware diff whitespace verification passed.





## 0.7 stock warp, contact and rendering — 2026-09-14



Core verification passes 67,954 checks. Full game regression `RingworldSmoke-20260914-122935.log` passed packed solar coasting, arrival, contact, camera and EVA rapid reversals. Grounded craft camera wobble/effects were zero in the controlled fixture; EVA remained alive at 301.61 K. These measurements do not certify every crash configuration.



Terrain regression `RingworldSmoke-20260914-125015.log` used world seed 1835517880 and random-selection seed 812794611, with dry grassland at along 72,018,063,334.7862 m, across 74,081,507.2872 m. Completed LOD blocks were visible while other blocks remained queued. The unpowered 60 m drop required a bounded settling wait; an earlier test's fixed 20-real-second deadline was insufficient. The production warp eligibility limits were retained.



Native stock button handler `btnSetHighRate` was exercised with gradual rate changes in `RingworldSmoke-20260914-125459.log`: 1,828.87 seconds of universal time elapsed, Kerbin moved 16,980.17 km, and the landed ring anchor drifted 0.0091 m after unpacking. The ring sunlight intensity matched the expected accelerated day/night phase while packed. This verifies the native clock and one dry surface anchor, not arbitrary scene changes or resource-mod compatibility.



At 1280x720, AA 2, half-resolution textures, 35% stock scatter, 75 m ground detail, LOD resolution 8 and one block per frame, the 125015 daylight sample averaged 31.27 ms/frame with p95 64.18 ms and 422 detail clumps. This particular scene was about 32 FPS with frame-time variation. It is not a guarantee for arbitrary render distances or laptop hardware.



Inspected surface screenshots at camera distances 5, 30, 150 and 1,000 m did not show the reported floating coarse-ring slab. Actual transformed float mesh samples had a highest coarse-floor chord altitude of -13,507.72 m, below the supported terrain minimum (-1,200 m). The exterior map screenshot no longer showed interior skirts. At approximately 1,771 m/s in a deliberately fast spin-matched radial descent, stock effects used 1,774 m/s with direction dot product 0.9999999 against ring-relative velocity; the inspected flame trail pointed upward. Local wall meshes have 24 vertices with separate face normals and finite thickness. Subsequent close-up inspection identified an out-of-bounds terrain plateau, addressed by clipping local and LOD floor generation to the ribbon edge.





### Unmatched atmospheric entry



`RingworldSmoke-20260914-130643.log` passed the new `./smoke-test.ps1 -UnmatchedOnly` test. It uses the stock 49-part Orbiter 1A fixture in an isolated save, positioned at 61,157.52 m ring altitude. Starting within the rotating arrival chart but above its 60 km atmosphere, the harness assigns a prograde solar circular-speed velocity (8,753.47 m/s) plus 1,000 m/s downward motion, with no ring spin matching. The local ring speed was 385,635.59 m/s, producing 376,883.45 m/s air-relative speed. This tests atmospheric entry, not a complete interplanetary flight through the capture boundary.



Crash immunity, unbreakable joints and temperature immunity were all false. The ordinary relocation grace for G/speed checks remains; it does not disable thermal destruction. First nonzero atmospheric density occurred at t=1.16 s. First part losses occurred at t=1.24 s, only 0.08 simulation seconds later, around 59,918 m altitude. The craft went from 49 to 47, 42, 40 and 24 attached parts in successive observations; the Mark1-2 command pod then exploded and the original vessel ceased to exist. Detached debris can remain; a zero remaining count for the original vessel does not mean every detached part was annihilated.



The effects velocity had dot product approximately 1 against the ring-relative velocity and a tangential fraction of 0.99999648 (about 0.15 degrees from sideways). The saved screenshot captures the brief luminous entry; that camera angle does not clearly resolve plume direction, so the direction result is telemetry-based. Stock-model external temperature reached approximately 319,429 K and sampled part/skin temperatures reached 9,759 K. Those are game-model outputs, not validated real plasma predictions at 377 km/s. The log contains actual part explosion events, including the command pod, rather than a scripted destroy command. The test harness does not modify production heat or damage calculations.



Report: `artifacts/validation/unmatched-smoke.txt`; screenshot: `artifacts/validation/unmatched-entry.png`.





Final boundary regression `RingworldSmoke-20260914-130937.log` passed. Near the rim terminal, all local ground vertices stayed at or below 162.018 m floor altitude and within exactly 80,250,000 m across (the half-width); the former wall-height phantom plateau was absent. The inspected close-up shows a finite wall-top strip and vertical face. This is a geometry inspection with a temporary diagnostic camera/fill light, not a new player camera or lighting mode. Stock UI warp again advanced 1,823.09 s with 16,926.52 km Kerbin motion and 0.00926 m anchor drift. The final 20-second laptop sample averaged 30.42 ms/frame (p95 63.40 ms), 422 clumps, with the queue drained. Progressive loading was observed with one visible block and 904 pending at the beginning.





The normal 0.7.0 DLL was restored after the final game run. Installed/staged SHA-256: `68CC0E1B7DD9AA655FF1DDDD14422603BCBE7F774ACF539B165FA31C5D7BFA59`. Assembly inspection found no SmokeTest type. CRLF-aware whitespace validation passed. KSP was closed after testing.





## 0.8 GPU visuals and photo mode — 2026-09-14



Unity 2019.4.18f1 built the original atmosphere/water shader asset bundle for Windows Direct3D11. The runtime GPU identified itself as Qualcomm Adreno X1-85. Core verification remains 67,954 checks. These tests do not establish compatibility with the entire EVE/Scatterer/Deferred/Parallax/TUFX mod stack; none of those packages was installed for this run.



Photo smoke `RingworldSmoke-20260914-142306.log` passed with seed 1970 and the stock 49-part Orbiter 1A over Great Ocean I. It exercised GPU High/Ultra, the wave material, clear sky at all three quality levels and a 16-sample, 1280×720 photo. Photo mode waited for 32-subdivision LOD and an empty queue. Measured universal-time drift and vessel-position drift were both zero. Completion and cancellation restored time scale 1, LOD 8, Laptop atmosphere/water, the original frame-rate target, visible stock HUD and no photo input lock. The completed image was `Screenshots/Ringworld/Ringworld-20260914-142634-237.png`.



An earlier photo test exposed a readiness race: the queue was empty before the first post-rebuild terrain update. Photo mode now requires that update before considering the queue drained. The subsequent tests passed. The inspected 16-sample image shows billowing clouds and a continuous water surface; small dark distant strokes and simple lighting remain visible limitations, so it should not be described as finished photorealism. Waves are visual; reflection is an analytic sky approximation and does not include nearby craft or shores. Daytime stars also remain visible through this prototype atmospheric composite.



The full-ring shader initially failed Unity compilation because `umulExtended` is unavailable through this Unity shader compiler. It was replaced with explicit 16-bit partial products for the same 64-bit hash. The game run launched with the failed bundle was stopped before validation; it is not counted as a successful test.





Full-ring integration/photo regression `RingworldSmoke-20260914-143842.log` passed after the final production shader/mesh changes. The global mesh had 65,544 vertices and separate structural/floor submeshes. The test exercised the saved detail option, disabling it, automatic photo enablement and restoration to disabled. It captured clear daytime views in Laptop, High and Ultra and a second full 16-sample photo, `Ringworld-20260914-144210-669.png`, again with zero UT/position drift and a drained LOD queue. Ground views were inspected: the thin upward arc and its illumination bands are visible; strong near-horizon haze and simple distant colour remain. Outward-facing closed-hull triangles now use back-face culling so the underside cannot compete with the interior at scaled-space depth precision. The emulated GPU 64-bit multiplication was checked against 10,000 deterministic integer products; seed halves are sent as exact 16-bit float values to avoid old Unity integer-property precision loss.



The first elevated diagnostic camera applied the geometry's orientation and then the root rotation, looking at an unintended longitude. The diagnostic was corrected to use neutral local geometry before the root transform. This correction only affects the test camera. Stock AlarmClock/PQS initialization exceptions and Unity temporary-allocation warnings appear in the logs; the run is not claimed to be warning-free.





Corrected elevated-view test `RingworldSmoke-20260914-144229.log` passed (`./smoke-test.ps1 -DistantOnly`). The view is centred on Great Ocean I using the ribbon's local frame; 137,251 pixels met the blue-water criterion and the screenshot was inspected. It shows a coarse circular basin and filtered climate colours, not detailed natural coastlines. `artifacts/validation/distant-surface.png` records this diagnostic view. The preceding flight checks confirmed the full-ring material, separate floor submesh and clear daytime images across all quality modes.



The final installed/staged normal 0.8.0 DLL SHA-256 is `2DF69C7125F9DC6D248A711D8801D4C948BE3A7C9DFFFA252D4569D1BA4A53DE`; installed/source visual bundle SHA-256 is `7D1294ABC332CC512EE36DCE0E84A92050524EE323E728D240E2D5AB1481E316`. Assembly inspection found no SmokeTest type in the normal installed DLL. CRLF-aware whitespace verification passed. KSP was closed after testing.





## 0.9 weather, warp and night — 2026-09-14



Core verification passes 72,985 checks, including seeded weather determinism, bounded cloud/rain/storm values, clear-sky overrides, zero generated storm fraction, full severity range and continuity across weather time intervals. Unity 2019.4.18f1 rebuilt the cloud-deck, volumetric, global-ribbon and terrain-night shaders for Direct3D11. The production C# build completed without warnings/errors.



Initial weather smoke `RingworldSmoke-20260914-165717.log` stopped before visual assertions: the original crashed Orbiter's Small Delta Wing still rotated at 0.070 rad/s, above the normal 0.05 rad/s warp limit, after the settling deadline. Production eligibility limits were retained. The weather-only fixture was changed to a single command pod over the flat expedition pad, isolating rendering/clock tests from articulated crash contacts. This failed fixture run is not evidence that every crash configuration can warp.





Single-pod run `RingworldSmoke-20260914-170247.log` passed its flight assertions but failed the final tracking-station readiness check. It advanced 9,322.16 game seconds during the weather sample at native 1,000×, covering severity 0–0.8284. There were zero cloud mesh rebuilds, one reported GC collection, a managed-heap delta of -15,425,536 bytes and an aggregate p95 frame time of 81.2152 ms across Laptop/High/Ultra. Screenshots show the game actually used the saved 2880×1920 resolution, overriding the launch resolution arguments. These are mixed-quality measurements on this scene, not a laptop FPS guarantee or proof of system RAM exhaustion.



Daytime galaxy visibility was 0.00002253 in all three modes; night and map restoration checks passed. Rain counts were 48/144/384 at maximum intensity, a deterministic lightning event was detected at strength 1, and the global no-detail ribbon probe contained 240 dark and 560 lit pixels across an 800-pixel longitude strip. The daytime/night screenshots were inspected: the ring and its bands remain visible, daytime stars are absent, and stars return at night. The first lightning image overexposed the cloud flash, so its cloud illumination strength was reduced to one quarter. Nearby rain streaks were moved beyond the immediate camera vicinity.



The tracking-station failure exposed duplicate creation after expanding ScaledRing to EveryScene: the repository already had a dedicated TrackingRing loader. ScaledRing was restored to FlightAndKSC and the existing tracking loader retained. The tracking test now waits for the shader to be ready, then requires exactly one ring renderer. The failed run is not counted as an overall pass.





Final weather smoke `RingworldSmoke-20260914-171223.log` passed end-to-end. It used a single command pod on the expedition pad, world seed 1835517880, at the instance's saved 2880×1920 resolution. Native warp advanced 1,853.98 seconds in the anchor check with 17,213.31 km of Kerbin motion and 0.00000735 m pod drift. The following mixed-quality 1,000× weather sample advanced 9,387.10 seconds, covered severity 0–0.82885 and performed zero cloud mesh rebuilds. The API reported one GC collection and a managed-memory usage estimate delta of -18,055,168 bytes; aggregate p95 was 78.88309 ms. This includes transitions through High/Ultra and is not a per-tier benchmark or evidence that physical RAM was full.



All three daytime starfield checks, night/map restoration, 48/144/384 rain counts, seeded lightning, and the global night-mask probe passed. The scene transition now saves the isolated fixture normally and waits for initialization. The night material is installed during ring creation, so there is no first-frame plain-material dependency. Tracking station finished with the night shader active and exactly one ring renderer. The previous `170750` run had passed its flight checks but timed out during tracking initialization; it is not counted as an overall pass.



Final production DLL (installed and staged) SHA-256: `60F5B80CC7CA6581BEFF355210F48B3173135085BF22F491AA1A258D98E1733E`. Shader bundle (installed and source) SHA-256: `212735C6220B48449343C094F52B9C0F9428F9EB9DB57CEB19AF8685CB7E827A`. Normal assembly inspection found no SmokeTest type. CRLF-aware whitespace verification passed and KSP was closed. Screenshots are archived in `artifacts/validation/weather-0.9/`. Visuals remain primitive, with dark building materials, approximate cloud lighting and simple rain streaks; these tests certify the recorded behavior, not finished art or third-party visual-mod compatibility.





## Blender first scenery set — 2026-09-14



Blender MCP on localhost 9876 responded to scene inspection and executed the asset generator successfully (Blender 5.2.1 LTS). Eight original models, three LOD meshes each, were authored and exported with explicit Y-up conversion, transform baking and FBX_SCALE_ALL. The original Blender scene is preserved under art/first-set/before-ringworld.blend.



Unity 2019.4.18f1 import checks caught below-ground root vertices; these were clamped to the shared ground plane. The first KSP render exposed inward-facing hut roof polygons hidden by Blender's double-sided display; winding was corrected. Final source validation passed, with finite bounds, unit-height LOD0 trees, shared scale across simplified LODs, centred unit-box boulders, and decreasing mesh budgets.



Final runtime test: template_instance/RingworldSmoke-20260914-180158.log, PASS scenery-only. All eight installed prefabs loaded with three LOD renderers, supported textured materials, correct runtime scale and ground pivots, no rigidbodies, and working downward collision-ray contacts. All three LOD levels were rendered; final LOD0 and LOD2 images were inspected for the corrected roof culling and upright silhouettes. This is isolated runtime asset validation, not a natural-forest performance benchmark, full flight/EVA test or distant-impostor test. The low-poly art and visible LOD switches remain first-pass limitations.



Core verification: 72,985 checks passed; normal build completed with zero compiler warnings/errors and was restored after testing. KSP exited. Source and installed scenery bundle SHA-256: B5677DCD2DE816B198F7BAF0827860E9231682A9E01723971CE0309CF6B811AF. Installed normal DLL SHA-256: 06A8FA0BBF46FEC78420EB90F7B872DD4FC493CDBF331E724969338143364675. Blender/FBX hashes are in art/first-set/validation/source-hashes.json; Unity bounds/counts and KSP LOD images are beside it. No weather or physics behavior was intentionally changed.





## 2026-09-14: broken cloud banks, city kit and interoperability



- Unity 2019.4.18f1 built the visual and scenery bundles for Windows D3D11. Scenery: 1,945,947 bytes; 53 prefabs with 159 LOD meshes.

- Blender MCP at localhost:9876 executed `art/city-kit/build_cities.py` and saved the native project, five FBX assets, manifest and review render inside `art/city-kit`. Source export audit covers all 45 habitat/city additions. FBX coordinates bake Blender +Z into Unity +Y; all five city assets passed Unity bounds/LOD import checks. The Blender city sheet and KSP city-library page were visually inspected.

- `smoke-test.ps1 -GlobalCloudsOnly -SceneryOnly` passed in `template_instance/RingworldSmoke-20260914-231558.log`. Both fine and macro cloud coordinates are continuous at all 16,384 shell segment boundaries. Day luminance 0.15658, night 0.01554, handoff 0.14169; clouds remain available with full-ring detail disabled and disappear when cloud amount is zero. The isolated day render shows clear holes rather than an opaque white sheet. This is a fixed-view renderer test, not a full descent visual certification.

- All 53 prefabs instantiated in KSP: the initial eight assets passed material, pivot and collider-ray tests; the additional 45 passed three-LOD, unit-height and material tests. Eight small vegetation assets also passed the merged-ground-mesh adapter checks. Generic box collision proxies are not certified as traversable building interiors.

- Core verification passed 72,985 checks. The existing ring-relative physics frame was retained; no native `inverseRotThresholdAltitude` adapter or Sun threshold modification was introduced.



- `smoke-test.ps1 -WeatherOnly` passed in `template_instance/RingworldSmoke-20260914-231844.log`: stock toolbar button present; Sandbox/Career/Science control predicates and hide/show UI events passed; the public API rejected the initial orbital vessel and reported the settled ring pod at 0.0000111 m/s, tangential speed 385,637.135 m/s, biome Ruins.

- Native stock warp advanced UT by 499.933 s and the home planet by 4,641,630 m while anchor drift was 0.00000636 m. This supersedes the earlier short-wait warp-test failure; gameplay rest limits and frame dynamics were not loosened.

- Weather test covered 9,831 simulated seconds at 1,000x across three presets: severity 0..0.89275, cloud rebuilds 0, no lightning flashes at high warp. Rain budgets were 48/144/384 drops; normal-speed lightning, daytime star suppression, global shadow bands and tracking-station rendering passed. At 2880x1920 the mixed-preset sample measured p95 64.64 ms with one generation-0 collection; this is a short fixture measurement, not a laptop FPS guarantee.

- Stock AlarmClock/PQS-related startup exceptions remain in the fixture log as previously documented; these passes do not certify a clean third-party-mod stack or every EVA/contact scenario. Final installed DLL is the normal build with the harness excluded.





## 2026-09-15: fBm clouds, wall camera and map depth



- Core checks: 90,859, including both analytic wall boundaries, permission to pass above wall tops, finite full-ring planning from a `double.MaxValue` preference, and pre-subdivision culling outside the rim width.

- `smoke-test.ps1 -GlobalCloudsOnly` passed in `template_instance/RingworldSmoke-20260915-084345.log`. A 1024-square GPU probe over a 128,000 km field measured 49.9163% samples above the cloud-coverage midpoint. Mean absolute differences after shifting by the old 512 km and 32,768 km periods were 0.19139 and 0.48415; neither repeats. The generated `artifacts/validation/global-clouds/fbm-coverage.png` was visually inspected.

- The same run verified all 16,384 ring seams and day/night/optical handoff responses (0.64511 / 0.04533 / 0.54567 luminance). This fixed-view shading check can intentionally sample a dense local bank even though overall coverage is about half.



- Combined cloud/map run passed in `template_instance/RingworldSmoke-20260915-090010.log`: 49.8507% fBm midpoint coverage; exterior-through-hull cloud luminance zero; 96 skirt-free scaled chunks; both rim camera constraints and uncapped setting roundtrip passed. Three custom map camera captures were inspected. These are limited inspection views, not certification at every interactive map zoom. An earlier map fixture failed because it searched only active renderers while outside the hull; the final fixture waits for built scaled chunks and includes inactive renderers.

## 2026-09-15: denser close cover and graphics diagnosis



- Full KSP smoke passed in `template_instance/RingworldSmoke-20260915-172446.log`. Ground fixture produced 1,238 clumps twice with identical counts; native scatter disable passed. The tested ground renderer is the shipped version. A temporarily increased tile-scenery candidate grid in that test build was reverted before delivery to retain established tree/collider locations; this pass does not measure the final tree population.

- Horizon completed 989 blocks, including 582 scaled patches. Stock surface warp advanced 2,020.04 seconds with 1.11 mm anchor drift. Water-camera clearance was 0.699 m. EVA walking in six directions and rapid reversals passed with damage enabled; FREE and CHASE cameras each measured zero frame rotation in the stationary fixture. This does not establish that every reported intermittent physics issue is eliminated.

- Core suite: 90,859 checks. Ground meshes retain a sub-60,000 vertex bound and 64/128 merged asset budgets; no comparative FPS benchmark was performed for this density increase.

- Research, current hardware observations and cautious screenshot diagnosis are in GRAPHICS-DIAGNOSIS.md. No hardware fault was established and no driver was changed.



## 2026-09-16: annular encounters, residence and stock science



- Targeted residence run passed in `template_instance/RingworldSmoke-20260916-004535.log`: an incoming coast detected one annular frame entry; a real stock crew report used `crewReport@SunSrfLandedRingworld_Ruins`; the base plus four anchored Breaking Ground parts survived Save -> Space Center -> reload. Base position error was 0.00004848 m, and all four part GUIDs remained registered and landed.

- Earlier test iterations exposed a real degenerate solar-orbit problem when creating deployed parts, now addressed by retaining valid inertial osculating state for stock bookkeeping. The ring still supplies local dynamics. Registered landed loads bypass the Sun's absent PQS; native planetary vessels are unaffected by those scoped branches.

- Fixture corrections included setting the stock deployment flag, adding the Breaking Ground scenario absent from the old training save, and loading configured power values into synthetic part snapshots. The first residence pass reported zero power supply/demand, so it establishes anchoring, identity and persistence only, not powered science generation. The final regression requires a nonzero power budget.

- Distant wall faces use a near-black material on the existing closed ring cross-section. No additional full-circumference vertices were added. Physical wall thickness may still be subpixel at distant map zooms.

- The map screenshot from `RingworldSmoke-20260916-005734.log` was inspected: the green encounter icon separates cyan and amber segments, and the amber segment terminates at atmosphere entry. Both stock orbit-rendering paths must be suppressed; hiding only OrbitRendererBase left PatchedConicRenderer's continuation visible. This run verified 2,000.04 seconds of stock warp with 0.00883 m drift and a stationary 0.0 m/s navball, but stopped at an obsolete test assertion comparing inertial solar speed against a ground-speed limit. The assertion now uses ring-relative speed.

- One earlier random terrain fixture rejected warp readiness after a fixed settling wait. The final fixture uses seed -739779896 and a bounded wait for the unchanged contact/motion checks. This is not a claim that every random crash posture settles or qualifies for warp.

- `RingworldSmoke-20260916-010319.log` passed the final production code's coast/map, descent, native warp (2,020.04 s; 0.004705 m drift), save/science, terrain, six-direction EVA walking, rapid reversal and FREE/CHASE camera checks. It then failed the strengthened deployed-power assertion (0 supplied / 0 required) in the synthetic spawn fixture; it is not recorded as a full-suite pass. The fixture now sets the stock 1-unit RTG/controller/experiment budgets through the live module properties, and the focused residence run also checks power after reload. No production power values or warp safety thresholds were relaxed.

- Final focused run **PASS**: `template_instance/RingworldSmoke-20260916-011546.log`. Real stock ground anchoring completed for the controller, two RTGs and goo monitor. The cluster supplied 2 units against 2 required, and used `deployedGooObservation@SunSrfLandedRingworld_global`. Save -> Space Center -> reload retained all four registered LANDED vessels, a powered 2/2 cluster and the resident pod; base displacement was 0.00006910 m. This is a programmatically spawned stock-module test, not an end-to-end EVA inventory UI or long-duration science-transmission certification.

- The preceding focused run stopped at its fixed 15-second pod settling wait. A bounded additional wait for the existing contact/motion conditions passed; no save/warp guard was weakened. The final focused run and the flight/EVA stages above used identical production code; subsequent changes were fixture-only.

- Final core suite: 90,859 checks, zero compiler warnings/errors. The normal build was restored and KSP exited. Stock fixture AlarmClock/PQS startup and KnowledgeBase shutdown exceptions remain; no clean-log or universal third-party compatibility claim is made.



## 2026-09-16: guidance, parachutes and predictor cadence



- `smoke-test.ps1 -GuidanceOnly` passed in `template_instance/RingworldSmoke-20260916-014455.log`. All six stock SAS direction modes in Orbit and Surface display modes produced ring-relative SAS target-vector dot products of at least 0.9999999. This exercises the real patched autopilot target setter; it is not an attitude-settling or fuel-efficiency benchmark.

- The map committed 20 predictions in a two-second sample while retaining an encounter marker, compared with the former two-second start interval. Long-path or low-FPS workloads can complete more slowly; the numerical work is time-sliced.

- The actual stock parachute reached SEMIDEPLOYED in 63.665 kPa ring pressure while the unmodified solar query returned zero, then reached DEPLOYED at 2,926.68 m ground clearance after the ordinary deployment-altitude field was raised from 1,000 to 5,000 m. Pressure/safety logic, animation and the stock state machine ran; chute state was not forced. The test does not certify a complete parachute landing of the heavy fixture or third-party parachute modules.

- Core verification: 90,859 checks; build has zero warnings/errors. Final cleanup uses vessel-local target context and counts only committed (not aborted) predictions. Normal build excludes the test harness.

# 2026-09-16: pause-menu save, damage-enabled warp and expanded art



- Reproduced the user's save rejection by opening the real Pause menu: it pauses time *before* querying ClearToSave. The old ring readiness predicate rejected paused time. The save-only predicate now permits pause while retaining contact, velocity, throttle and rotation checks.

- Reproduced destruction during stock warp with crash damage and joint breakage enabled. A test-only explosion stack showed `Part._CheckPartTemp`, a packed vessel and temperature 1,356,623 K. The atmosphere was consuming solar inertial velocity while the craft was stationary on its rotating-frame anchor. Anchored velocity now reports zero relative to ring air; valid inertial osculating state remains available for stock bookkeeping. Damage is not disabled by the fix.

- **PASS** `RingworldSmoke-20260916-022414.log`: full 49-part stock fixture, real Pause menu `CLEAR`, quick-save allowed; five native warp cycles (10, 100, 1000, 100, 1000), unchanged part count, 0.0013–0.0045 m root drift, non-degenerate packed orbits, saves during warp. Save while paused -> Space Center -> reload retained all 49 parts and LANDED, displacement 0.010581 m. Descent uses a test-only velocity controller; this is not an autonomous flight demonstration.

- Blender authored 31 new assets with 93 meshes. Unity 2019.4 imported the expanded 84-prefab bundle with up-axis/bounds checks. Four Blender review sheets were rendered and inspected. **PASS** KSP library validation `RingworldSmoke-20260916-023816.log`: 72 non-tree library kinds, eight tree variants, original rocks/buildings, supported materials, three LODs, tree ground pivots, seven contacts per canopy grove and ray hits on non-convex architectural contact meshes. This validates import/contact geometry, not every pilot landing on a building.



- **PASS** live landmark/woodland run `RingworldSmoke-20260916-024111.log`: 14 nearby architectural prefabs streamed; 784 seven-tree groves across 49 tiles (5,488 additional tree silhouettes and trunk contacts), within the Laptop cap of 16 groves per tile. A five-second sample observed 35.5 FPS and 666 MB managed memory. This is a single scene/resolution and not a baseline comparison or whole-process memory measurement. Diagnostic renders isolate scenery/near-terrain layer 15, so their sky and distant terrain are intentionally absent; they are not full-atmosphere screenshots.



- **PASS** broad flight/EVA/deployed-science suite `RingworldSmoke-20260916-024420.log`: 400-second packed coast error 0.0000135 m; annular map/depth tests; automatic arrival and air/re-entry direction; 2,000.04 seconds stock surface warp with 0.00538 m drift and advancing Kerbin orbit; stock Ringworld crew report; underside/water-camera checks; 989 completed terrain blocks; deterministic ground detail; six-direction EVA and rapid reversal survived; FREE/CHASE camera peak frame rotation zero; four deployed parts with a powered 2/2 science cluster; Space Center roundtrip resident error 0.04246 m and LANDED. The descent fixture is damage-immune; the separate StabilityOnly regression tests damage-enabled packing/unpacking. The final grove-contact handedness correction was built after this suite and has its own subsequent asset/live-scene regression.



- **PASS** final combined import + live-scene run `RingworldSmoke-20260916-025310.log` using the corrected grove-contact bundle. Unity's FBX handedness mirrors X in addition to changing the up axis; trunk capsules now apply that conversion, and every trunk is checked against nearby bark-mesh vertices during import. KSP repeated the library/contact/LOD checks and streamed 14 landmarks plus 784 groves / 5,488 trunk contacts. The five-second live sample after the complete library render measured 28.9 FPS and 833 MB managed memory. Across the two live samples the observed range was 28.9–35.5 FPS, not a comparative or sustained benchmark.



## 2026-09-16: v1.0.1 landing-leg regression and colossi



- The user's intact Auto-Saved Ship.craft was copied into an isolated save: 38 parts, eight LT-2 landing legs. No user vessel/save was rewritten. Initial spin-matched placement put the root 10 m above ground with feet much lower. Engines, SAS and RCS were off. Descent and settling used full normal ring gravity, with no velocity controller and crash/joint damage enabled. This tests touchdown, not an unpowered orbital-altitude fall.

- Investigation found stock wheel gravity, surface-gee load distribution and remembered launch-pad drift orientation still using spherical-body assumptions. Scoped adapters now supply apparent ring gravity and reset drift history on frame changes. A second fault in CollisionEnhancer treated relocation as a swept impact and used solar up for penetration recovery; its sweep history and recovery normal are now frame-aware. Stock motion gates were not relaxed.

- PASS gear phase in RingworldSmoke-20260916-094035.log: all eight feet grounded, all 38 parts retained, peak descent 5.63470 m/s. During steady samples surface speed was approximately .003–.054 m/s and maximum part angular speed .009–.018 rad/s; native warp gate accepted the craft. Paused save was CLEAR. Stock 10x/1000x/1000x warp drifts were .00227/.00900/.01602 m. Space Center/save/reload preserved every part and the landed state, with .01747 m position error and warp allowed. Residual suspension movement is not claimed to be identically zero.

- PASS guidance phase in the same run: actual stock vertical-gauge logarithmic target -3.005618 matched the native gauge response for -20.198658 m/s ring-normal descent (solar vertical speed was only -.0002003 m/s); sink LED illuminated. All six SAS modes matched target dot 1 in Surface and Orbit display modes. Trajectory updated 13 times in two seconds. Parachute semi/full deployment checks passed.

- PASS library phase in the same run: 92 prefabs, three visual LODs each, import scale/material/contact-ray checks. Blender produced eight new colossi and 24 FBX LOD meshes with explicit transform baking. Live scenery found 19 nearby landmarks, five of the new colossi; the memory spire measured 9 x 45 x 9 km. These checks do not certify every possible architectural landing.

- The combined run later failed a forest diagnostic that accidentally counted legacy isolated-tree LODGroups as merged patches. The completed gear, guidance and library phase reports explicitly cite that source and later failure. Correcting the diagnostic to count named forest patches passed in the subsequent standalone run; no population cap was loosened to pass it. A separate gauge diagnostic originally compared raw m/s with the stock logarithmic dial target; it was corrected to compare through native SetValue.



- PASS final forest visual/performance regression in RingworldSmoke-20260916-095614.log: 30,922 crowns across 784 merged patches / 49 near-terrain tiles, 43 active trunk colliders. Matt atlas foliage, randomized yaw/asymmetry, three colour swatches and taller crowns replaced the initial shiny uniform appearance. Diagnostic canopy image was inspected. The five-second sample at 1280 x 720 / Laptop measured 31.8 FPS and 686 MB managed memory. This is not total process memory or a sustained/minimum-FPS guarantee; dense canopy remains stylized opaque geometry.



- Broad-suite attempt RingworldSmoke-20260916-095934.log stopped at the unlegged, damage-immune 49-part training rocket's rest gate: local speed .00443 m/s, but a Small Delta Wing still rotated at .059 rad/s versus the unchanged .05 rad/s gate after the 45-second realtime settling allowance. This is separate from the successful user's eight-leg fixture. The next diagnostic run permits up to 90 seconds to settle and logs root/part motion; this is a harness-only change, not a relaxed gameplay gate.



- The extended settling run RingworldSmoke-20260916-100634.log also failed: root angular speed stayed approximately .0001–.0022 rad/s while a Small Delta Wing persistently reported .054–.058 rad/s. Waiting longer did not resolve this. A bounded-pose rest filter was subsequently added (see RELEASE-1.0.1.md); the earlier statements about unchanged angular gates refer to the wheel/collision fixes before this separate change. Translation, contact, water and throttle limits remain unchanged.


- PASS final broad regression RingworldSmoke-20260916-101408.log with the bounded-pose filter: rest pose deviation .005333 m / 0 degrees at sampling; deliberately setting root rotation to .2 rad/s was rejected. Native 1000x warp advanced 2,020.04 seconds with .000266 m drift while Kerbin moved 18,755,061 m. The existing .5 m/s translation rejection test also passed. Ring science/save state, 989 completed LOD blocks, underside/water-camera constraints and deterministic detail checks passed.
- The same final run passed six-direction EVA plus rapid reversal, zero measured frame rotation/wobble in FREE and CHASE camera modes, all four deployed parts, a powered 2/2 deployed-science cluster, and Space Center/save/reload with .039282 m resident error and LANDED. The broad fixture remains damage-immune for entry/EVA isolation; the separate eight-leg regression above is the damage-enabled, normal-gravity touchdown evidence. Stock AlarmClock/PQS fixture startup and thermal-UI shutdown exceptions remain as previously documented; no Ringworld exception or failed assertion occurred in the successful run.

## 2026-09-16: v1.0.2 rare colossi and forest horizon

- 108,648 core assertions pass, including seeded colossus occupancy, circumference seam identity, landmark clearances, forest-mask agreement, bounded canopy height and water/road/disabled-scatter cases.
- Initial live scenery regression streamed 14 ordinary landmark assets with no colossi near the main cluster, then one 36 x 50 x 12 km atmosphere harp at a remote seeded cell. Near population remained 30,922 crowns.
- The first canopy-envelope-only experiment still showed a strong near-square texture contrast and was replaced by intermediate crown aggregates. An unrestricted aggregate experiment then measured 13.1 FPS while loading. The initial time-sliced version (RingworldSmoke-20260916-133220.log) measured 8.7 FPS over its long full-horizon generation sample and did not finish the canopy queue inside 150 seconds. These experiments were rejected. Preset-specific aggregate budgets were subsequently introduced, and final validation is recorded below.

- RingworldSmoke-20260916-134127.log was interrupted by the user's reported low-battery shutdown before completing the final test. It is not validation evidence. Earlier FPS comparisons were not controlled for battery/power state; final performance is measured again after restart.

- PASS fresh post-shutdown run RingworldSmoke-20260916-145639.log: 14 ordinary landmark assets and zero nearby colossi; one remote seeded atmosphere harp, 36 x 50 x 12 km. Queued canopy generation retained its original geometry frame across the injected orientation change. Near forest contained 30,922 crowns in 784 merged patches with 43 trunk contacts. All queues finished in 45.2 seconds: 192 intermediate canopy meshes and 980 terrain LOD blocks, terrainPending=0 and canopyPending=0.
- The fresh Laptop fixture measured 20.9 FPS during queue completion, then 21.5 FPS over a five-second settled sample and 938 MB managed memory. This is not total process memory or a sustained/minimum-FPS guarantee. The aerial layer-15 diagnostic forest-horizon.png was inspected: woodland extends beyond the near square and has irregular stand boundaries, but intermediate clusters are conspicuously coarse from high altitude and LOD bands remain visible. This is a functional distant-biome representation, not a visually seamless forest at every altitude.
- Final normal build passed 108,648 assertions with zero compiler warnings/errors. The harness restored the normal installed DLL after testing. The log contains stock CrewHatchController/DialogCanvas teardown exceptions after the successful fixture; they are not a failed Ringworld assertion.

## 2026-09-16: v1.0.3 quality controls

- Initial run RingworldSmoke-20260916-173737.log passed all eleven preset roundtrips and seed/density preservation. Low presets resolved to Economy forests and 160,000 km horizons. The ordinary forest fixture passed with 30,922 candidates and 192 distant canopy patches.
- Its Economy assertion failed because near candidate count was 30,921 rather than exactly 30,922 after rebuilding. The existing craft-clearance exclusion is position-dependent; the test now permits up to four changed candidates in this fixture while checking the same saved seed/density. No gameplay population rule was relaxed. The original vertex diagnostic also counted unrelated scenery meshes beneath the same tile root; it now counts only merged canopy meshes. The initial suspicion that tiles were still rebuilding was not established. The revised test explicitly waits for all scenery tiles as well as terrain queues.

- PASS final quality/scenery run RingworldSmoke-20260916-174318.log: all eleven preset save/apply roundtrips; named landmark and rare-colossus checks; frame-isolated canopy streaming; completed normal and Economy queues. Stored merged near-canopy vertices fell from 2,392,388 to 70,365 (97.1% fewer across all LOD meshes), with zero intermediate crown meshes in Economy. This is geometry storage, not a measured FPS multiplier. Physical candidates were 30,921 versus 30,922 before, within the craft-clearance exclusion tolerance; saved seed/density unchanged. Normal-mode five-second sample was 20.5 FPS / 700 MB managed memory; no comparable Economy FPS benchmark was taken. The Economy aerial diagnostic was inspected; it shows coarse canopy colour beyond the close meshes as intended.
- Final normal build passed 108,648 core assertions with zero warnings/errors. Preset-menu label caching, text clarification and including pending scenery tiles in photo readiness were subsequently compiled in the restored normal build; the dropdown itself was not manually click-tested. CKAN indexing and clean CKAN installation await a public hosting URL and are not claimed tested.


## 2026-09-16: v1.1.0 Cyla integration

- Cyla/photo run `RingworldSmoke-20260916-194949.log` passed camera isolation, visible sky, foreground depth, map return, all eleven preset round-trips and endpoint checks, selected low photo capture, high photo cancellation/completion, and restoration. Core checks: 108,648. High-photo preparation needed more than the earlier fixture's three-minute timeout; the successful run allowed ten minutes. Detailed measurements and optical limitations are recorded in CYLA-INTEGRATION.md.
- Combined run `RingworldSmoke-20260916-200752.log` passed the user's 38-part/eight-leg natural landing, paused save, stock 10x/1000x warp and Space Center/reload (6.5 mm position error, all parts intact). SAS targets matched the ring frame, but the trajectory refresh assertion failed at three updates in two seconds.
- Diagnostic guidance run `RingworldSmoke-20260916-201716.log` reproduced three updates in ten rendered frames, with one valid encounter and 42 path points. The predictor's fixed 1.5 ms slices were spreading work across low-FPS frames. The scheduler now budgets 10% of the previous frame, bounded to 1.5–8 ms. Integration steps, physics and encounter detection are unchanged. The regression retains its original refresh assertion.

- Follow-up combined run `RingworldSmoke-20260916-202103.log`: trajectory refresh increased to nine commits in ten rendered frames over two seconds, with one encounter and 42 path points. The unchanged refresh assertion passed. Both parachute stages and all surface SAS direction checks passed.


### v1.1.1 work in progress — September 17

- Core assertions: 108,655, including 300 million km and 2 billion km ring diameters and rejection of dimensions that lose centimetre coordinate precision.
- Combined run `RingworldSmoke-20260917-015055.log`: the current user craft has 35 parts and eight legs. Natural-gravity landing, paused save, 10x/1000x/1000x warp and resident reload passed. Guidance orientations and ten trajectory commits in ten frames passed. The run then failed because this craft has no parachute; it did not complete the scenery stages. This is not a complete release regression pass.
- Earlier combined run `20260916-202103` timed out during late forest checks; its successful earlier stages do not turn the whole run into a pass.
- New user constraint: all further live rendering tests use Slow or lower. Higher presets receive settings/code checks only; earlier Cyla rendering/photo results are historical tests, not newly rerun on the patch.
- Tracking and re-entry tests, final package verification and publication are still pending.

- `RingworldSmoke-20260917-074649.log` passed the new Tracking Station regression at Slow/Rotten Potato: airborne save blocked, 42 trajectory points with one ring encounter, unsafe warp rejected, and the correct ship handed to Flight after 120.12 game seconds at 209,908.81 m altitude. Returning to flight retained forward progress and the save gate stayed blocked. Stock AlarmClock/PQS and PatchedConicRenderer scene-teardown exceptions appeared; no Ringworld exception was recorded. The later refinement also checks a future solar patch when the craft currently orbits another body; that extra branch has not received a dedicated encounter run.

### September 17 continuation: camera and low-preset regressions

- Added a two-second, unscaled-time camera orientation/offset blend at ring reference-frame entry and exit. The visible pose is transformed into the new coordinate chart before interpolation; vessel physics, velocity and orbital bookkeeping still switch immediately. Stock camera state is restored before its next update so the cosmetic offset cannot accumulate. Map entry, vessel changes, scene teardown and explicit teleports cancel the blend.
- `RingworldSmoke-20260917-080914.log` passed the Slow-only combined guidance/scenery/landmark run: all twelve SAS direction checks, native vertical-speed gauge, both parachute stages, 32 trajectory commits in 37 frames over two seconds, 80 habitat asset kinds/240 LOD meshes, and Economy forest rendering. Higher presets were only checked as settings data.
- `RingworldSmoke-20260917-075815.log` completed reentry sampling at default dimensions and a 200-million-km diameter/50-million-km width. Gravity and drag were normal; temperature damage was disabled for this visual fixture. Measured averages were 24.35 and 22.01 FPS respectively. Screenshot inspection still found diagonal seams near the wall; the automated PASS only certifies sample completion, not a visual fix. Publication remains pending that investigation and the final regression/package checks.
- `RingworldSmoke-20260917-125110.log` passed Tracking Station again, now including the departure camera blend (zero first-frame angular discontinuity, completed transition). Encounter return advanced 118.76 game seconds to 209,951.95 m. This run includes the revised airborne snapshot filtering and future-solar-patch search, although its specific vessel already orbited the Sun.
- `RingworldSmoke-20260917-125544.log` passed the current 35-part/eight-leg craft at Slow: natural-gravity landing with collision damage enabled, paused save eligibility, 10x/1000x/1000x stock warp, and Space Center/reload. All parts survived; final reload error 0.001703 m. The normal plugin was rebuilt and installed after the harness exited.
- `RingworldSmoke-20260917-130144.log` passed both entry and departure camera blends at normal and enlarged ring dimensions (zero first-frame angular discontinuity, completed transitions). Visual inspection still reproduced the seam/cutout. Hiding the scaled ribbon removed it; hiding the local wall did not. The pre-cull sky recentering is retained as a camera synchronization measure, but did not fix this particular defect.

### 2026-09-17: final re-entry wall shader and camera transitions

RingworldSmoke-20260917-132516.log passed normal and enlarged-ring ascent/re-entry, with both entry/exit camera blends completing and zero first-frame angular discontinuity. The normal and enlarged 10 km screenshots were inspected: the reproduced giant diagonal wall lines and rectangular sky cutout are absent. Earlier isolation localized this case to the scaled ring wall's stock material; the dedicated dark-wall shader resolved it. This does not diagnose all vanilla KSP artifacts. Temperature damage was disabled for visual sampling; gravity and drag remained normal. Sample rates 23.9 and 13.8 FPS used different geometry workloads, not a controlled performance comparison.

### 2026-09-17: water, optical controls and output resolution

`RingworldSmoke-20260917-185354.log` passed all eleven updated preset round-trips and all 21 advanced Cyla numeric settings, using detached Settings objects for higher tiers. Live rendering stayed at Slow. The water shader loaded successfully and camera clearance matched submerged ground instead of the water level. Photo captures at 512x341 and 1536x1024 passed pixel-content/aspect/dimension checks, cancellation and gameplay-setting restoration; both images were inspected. These are actual camera-stack output renders. 8K/16K were not attempted on this laptop. GPU memory checks are conservative estimates, not a guarantee against driver allocation failure.

Follow-up `RingworldSmoke-20260917-190028.log` also passed. The GPU probe changed the surface behind the water from red to blue: quality 0 output changed from (0.714,0.071,0.086) to (0.012,0.071,0.788), and quality 1 from (1.000,0.443,0.416) to (0.439,0.443,1.000), confirming actual alpha blending. Both output resolutions, cancellation and restoration passed again with stock antialiasing applied to the capture target. The normal DLL was restored. Higher Cyla optical modes and high-resolution capture remain limited to static validation in this run; historical Cyla gameplay/render tests above predate the user's Slow-only testing restriction.
