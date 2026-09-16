# Niven Ringworld Expedition 1.0.1

## Landing legs and flight instruments

Stock landing legs now read the ring's local apparent gravity for wheel contact, spring/damper adjustment and suspension load distribution. They no longer tune these systems to the Sun. Stored anti-drift orientation and its integral are reset on frame changes. The mod does not freeze Rigidbody motion, reduce gravity or change users' leg spring settings.

Collision-enhancer sweep history is reset after relocation/frame changes. Its penetration-recovery direction follows the ring normal; this avoids interpreting a coordinate-chart change as an impact, or pushing a recovered part beneath the ring floor. Normal impact and thermal damage remain enabled.

A separate rest filter handles persistent, bounded flex in attached parts: root rotation must remain below .05 rad/s, every part below .12 rad/s, and chatter above .05 rad/s requires one physics-second with root and relative part poses within 3 cm / 0.5 degrees of their reference. The existing .25 m/s surface-speed, contact, water and throttle gates remain. This permits tiny solver oscillations without ignoring sustained sliding or rotation.

The stock vertical-speed gauge and sink-rate warning use velocity along the local ring normal. The original gauge response and warning hysteresis are retained. Stock behaviour applies outside the ring frame.

## Scenery

Eight original superstructures include an 80 km rim gate, 120 km causeway and 48 km floating city plate. The asset library now contains 92 prefabs and 276 visual LOD meshes. Designs and dimensions are artistic interpretations; machinery is static scenery. See [the colossus inventory](COLOSSI-AND-FORESTS.md).

Forests use continuous overlapping canopy patches, merged geometry and three LODs instead of sparse seven-tree clumps. Nearby trunk colliders are pooled; terrain-scatter settings and ring forest density remain respected. Forests do not cover dry grasslands indiscriminately and do not extend to the entire distant terrain horizon.

## Validation

The natural-gravity regression launches a copy of the user's 38-part, eight-LT-2-leg craft in an isolated save. Initial placement puts the pod root 10 m above the ground (the extended feet are much lower), spin-matched. The ensuing touchdown uses normal ring gravity with no velocity controller, gravity adjustment or damage immunity. This is a touchdown test, not an unpowered fall from orbital altitude. It exercises grounded settling, the paused save gate, stock rails warp and scene reload. The instrument check inspects the actual stock gauge target and sink LED. Asset checks inspect imported units, LODs, supported materials and contact rays. Live forest counts and frame-rate samples are recorded in VALIDATION.md.

This release remains scoped to tested KSP 1.12.5 Windows/D3D11 workflows. Residual stock suspension motion is possible; arbitrary damaged craft, all leg designs and every mod combination are not certified. The previous 1.0.0 ZIP is preserved. Restart KSP after installing the new DLL.
