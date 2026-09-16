# Niven Ringworld Expedition 1.0.0

Target: **KSP 1.12.5, Windows x64 / Direct3D 11**. This is the first packaged stable baseline for the supported local expedition workflow. It is not a claim that every KSP mod combination, vessel, or high-speed manoeuvre is certified.

## Install

Close KSP. Extract the archive's `GameData/NivenRingworld` into your KSP `GameData` folder. The archive also includes HarmonyKSP in `GameData/000_Harmony`; use a single compatible Harmony installation if your mod list already supplies it. Do not install a second nested `GameData` directory. Restart the game after updating DLLs. Back up an existing expedition save before changing versions.

The development instance has the normal plugin installed automatically. Blender, FBX sources, Unity import projects, game assemblies, test saves and smoke-test code are not needed by players and are not in the runtime ZIP.

## Fixed

- Pause-menu save rejection: saving can evaluate settled ground contact while time is paused. The same contact/motion safety checks still apply.
- Landed stock-warp destruction: thermal integration now reads zero ring-relative airspeed for a rails-anchored craft, rather than its enormous solar orbital velocity. Stock bookkeeping retains a valid inertial conic.
- Stock SAS prograde, retrograde, radial and normal targets use the ring frame. Near-zero velocity holds the current target rather than selecting a noisy direction.
- Stock parachutes use ring pressure and ground/water height for their deployment gates.
- Numerical map paths update with a bounded per-frame work budget and annular entry/escape markers. Predictions stop at the atmosphere; they do not predict atmospheric descent or thrust.
- Landed residence, stock science subjects, powered deployed-science anchoring and Space Center reload have dedicated in-game regressions.

## Assets and forests

31 new Blender assets bring the library to **84 prefabs / 252 LOD meshes**. There are 25 newly configured architectural landmarks: suspended palaces, ruined city rings, archive and garden complexes, rim gates, a 4 km elevator cathedral, viaducts, aqueducts, water-processing works, observatories and settlements. Designs are original, lore-inspired interpretations; machinery and inhabitants are not simulated.

Large landmarks stream independently to 60 km, one prefab per frame, with three visual LODs and simplified exterior contact meshes. They appear around the existing city, rim, spill-mountain, map-island, scrith and expedition sites. Flight scenery is not mirrored as individual map-mode building models.

Four individual tree variants and two seven-tree groves thicken woodland without changing the terrain seed or existing individual-tree coordinates. Additional grove caps are 16/24/32 per tile for Laptop/High/Ultra. Stock scatter settings and Ringworld forest density remain authoritative. The Laptop fixture reached 5,488 added tree silhouettes over 49 tiles at 28.9–35.5 FPS across two five-second samples; this is not a general FPS guarantee.

## Remaining boundaries

The stock Sun remains the native SOI and archive body owner; the ring has an annular local reference frame. Atmospheric/physics warp, arbitrary unloaded fleets, detailed building interiors, operational elevators, full-ring building impostors, third-party aerodynamics/thermal integration and all possible EVA or docking combinations are outside the validated release scope. Water buoyancy is approximate. The science archive may group Ringworld subjects under the Sun even though their names and subject IDs distinguish the ring.

Use the ordinary Save and Space Center controls to keep an expedition. **Revert Flight intentionally discards progress.** Existing corrupted or exploded craft cannot be repaired by loading the new DLL. Keep world seed and dimensions fixed after establishing a settlement.

See [VALIDATION.md](VALIDATION.md) for exact test evidence, [KNOWN-LIMITATIONS.md](KNOWN-LIMITATIONS.md) for the detailed scope, and [LANDMARK-ASSETS.md](LANDMARK-ASSETS.md) for Blender sources, lore references and rendering/contact budgets.

