# Niven Ringworld Expedition v1.1.2

Maintenance release of the integrated Cyla build, with the original ZIP layout retained.

## Included

- Cyla atmosphere integration and advanced optical settings, with eleven quality presets.
- Translucent water, procedural ripples and optional waves; camera clearance follows the underwater ground.
- Photo quality selection and aspect-preserving output sizes through 8K/16K, subject to GPU limits.
- Ring-relative camera transitions, re-entry wall rendering fixes, landed save/warp checks and Tracking Station encounter handling.
- The complete scenery bundles, 92 registered scenery prefabs, 25 landmark assets and eight rare colossal structure templates.

These features were integrated on main for v1.1.1. This release rebuilds the current source, increments the version and clarifies installation; it does not introduce a new asset layout or a new atmospheric solver.

## Installation — unchanged ZIP structure

Extract the ZIP into the **KSP instance root**, beside `KSP_x64.exe`. The result must include:

```
GameData/NivenRingworld/
GameData/Cyla/
GameData/000_Harmony/
```

Alternatively, copy the contents of the ZIP's `GameData` folder into your existing `GameData` folder. Do not place the entire extracted archive inside another folder under `GameData`; the standard asset paths expect the layout above. Keep only one installation of each dependency.

The reported missing assets were traced to nested extraction. A file audit found all 26 runtime files present and byte-identical to the tested archive, but the asset loaders could not find them at the expected paths. The proposed alternate-layout changes were cancelled. This release preserves the established paths.

## Credits and limitations

Cyla is by **Ghassen Lahmar (LGhassen / blackrack)**: https://github.com/LGhassen/Cyla. Its unmodified binaries, original license and matching published plugin source are included. See CREDITS.md and THIRD-PARTY-NOTICES.md; the complete bundle is not uniformly MIT.

Slow and below use Original atmosphere. Cyla remains a local optical approximation; whole-ring Cyla scattering is not implemented. Scatterer does not automatically recognize ring water. Higher revised optical budgets and 8K/16K captures were not rendered on this laptop. No 30 FPS guarantee is made.

## Validation

The release build runs 108,655 core checks. Package verification checks archive CRCs, required asset registries, pinned Cyla hashes, source/license inclusion, installed-plugin parity and exclusion of test harnesses and KSP assemblies. Previous dated Slow-only runtime tests cover water transparency, photo size/restoration, gear landing/save/warp, guidance, tracking and re-entry; see VALIDATION.md. This maintenance release does not claim those gameplay tests were newly rerun.
