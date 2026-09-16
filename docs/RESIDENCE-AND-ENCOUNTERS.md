# Ring encounters, residence and stock science

The Ringworld is an annular physics region, not a spherical CelestialBody. The Sun remains the stock reference body. No solar SOI, PQS, radius or science multiplier is globally changed.

## Map encounters

The numerical coast uses the same InArrivalRegion predicate as flight handoff, including its hysteresis. Its incoming segment ends at a green boundary marker and the following segment uses a different color until atmosphere/contact prediction stops. Hover the marker for time to the boundary. The predictor can start on an upcoming solar patch while the selected vessel is still in a planetary SOI; the preceding native planetary patch remains visible. This is a Ringworld overlay, not a stock patched-conic SOI node. Atmospheric flight, burns and maneuver optimization are not simulated by this coast preview. Calculation remains bounded to 4,096 steps and the saved prediction duration.

## Residence

Actual part ground/permanent-ground contact supplies LANDED, while the ring frame continues supplying dynamics. Stock save permission is granted only for a settled expedition that meets the existing surface-warp checks. The scenario records landed status alongside root position, velocity, rotation and frame epoch. Packed residents retain ring coordinates instead of following a fictitious solar orbit; nearby saved residents are restored together when their physics loads. Stock calls that assume a spherical PQS or spherical unpacking orientation are bypassed only for registered ring vessels.

Use Save and Space Center / Tracking Station. Revert Flight intentionally rolls back the flight and is not a way to leave a persistent base.

## Science and deployables

Stock ModuleScienceExperiment instruments and Breaking Ground deployed experiment initialization receive a Ringworld-specific science namespace and readable title. IDs retain the Sun reference internally for stock persistence compatibility, but include Ringworld_ and do not consume ordinary solar subjects. Biome relevance follows each experiment's mask. Ring subjects use the experiment's base science cap and a neutral subject multiplier; the Sun's multipliers remain untouched. Existing old solar reports are not rewritten.

Cargo settling uses ring-relative speed. Nearby spawned ground parts inherit the ring frame; permanent ground contact is recognized for saving and warp. The stock deployment, controller, power, science generation and inventory systems otherwise continue to run. This integration is not a claim of compatibility with every third-party part module. Specialized seismic-distance calculations and background solar-duty assumptions still use stock behavior and need a dedicated physical model for the annular habitat.

## Walls

The full circumference already has a closed eight-vertex cross-section: outer faces, top caps, inner faces and hull underside. Both fallback and detailed rendering now use the same dark material for non-floor faces. This changes the material assignment, not the mesh budget. A physically 100 m thick wall becomes subpixel when viewed from sufficiently far away; it is not artificially widened as the camera recedes.

## 2026-09-16: SAS, parachutes and responsive coast preview

SAS prograde and retrograde now use the same ring-relative velocity as the navball, in either Orbit or Surface display mode. Radial out/in mean away from/toward the local floor; normal/antinormal use the perpendicular to local up and travel. Undefined near-zero vectors retain an attitude rather than selecting a noisy solar direction. Stock stability, target-pointing, maneuver modes and SAS unlock requirements remain stock. Target-relative velocity uses the instrument vessel's own target and is supported for another registered ring vessel.

Stock ModuleParachute pressure sampling now uses ring air, and its full-deployment height uses the local ground/water surface. Deployment safety, heat failure, shielding, animations and drag remain stock. This does not certify replacement systems such as RealChute.

The coast predictor previously waited two seconds between starts. It now starts up to 30 times per second, spreads numerical work over roughly 1.5 ms integration slices, retains its 4,096-step ceiling, and updates the origin every rendered frame. Actual completed prediction rate depends on flight state, frame rate and path length; it is not a promise of 30 complete long forecasts per second. Prediction beyond atmosphere entry remains unsupported.

## Pause and warp stability

Pause-menu saving now permits the paused clock when checking otherwise-settled ring contact. Packed anchors keep valid inertial orbit bookkeeping while atmosphere queries report zero local air-relative velocity. The earlier mixed-frame packed speed caused catastrophic artificial heating; this correction retains stock thermal damage. Five damage-enabled warp cycles and a paused-save Space Center roundtrip passed; see VALIDATION.md.
