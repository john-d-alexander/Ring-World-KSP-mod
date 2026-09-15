# First Blender scenery set

Sources, export scripts, previews and validation live in `art/first-set/` under the mod root. Open `Ringworld-Scenery-First-Set.blend`; the **Scenery Review** scene displays the kit, and **First Scenery Set** contains the normalized export meshes. The previous Blender scene was preserved in `before-ringworld.blend` and remains in the working file. No downloaded art is used.

This is the first stylized, low-poly production pass, not a photorealistic foliage library. Rural dwellings are original interpretations of post-collapse habitation; they are not reconstructions of a specific building described by Niven. Unique scrith machinery, floating cities, mirror flowers and detailed interiors still need their own asset passes.

| Stable ID | Existing scatter slot | Triangles LOD0 / LOD1 / LOD2 |
|---|---|---|
| broadleaf_spreading_01 | tree_broadleaf | 1996 / 412 / 86 |
| broadleaf_windswept_02 | tree_broadleaf | 1996 / 412 / 86 |
| conifer_layered_01 | tree_conifer | 1888 / 316 / 196 |
| conifer_slender_02 | tree_conifer | 1888 / 316 / 196 |
| boulder_weathered_01 | boulder | 130 / 48 / 20 |
| boulder_sandstone_02 | boulder | 130 / 48 / 20 |
| habitation_thatch_01 | rural_building | 504 / 152 / 68 |
| habitation_reclaimed_02 | rural_building | 516 / 164 / 68 |

Each LOD is one mesh with one shared material, using the original 512×512 `SceneryAtlas.png`. Leaves use opaque silhouette geometry rather than stacks of transparent cards. The bundle is about 0.58 MiB on disk; that is not a measurement of total runtime RAM/GPU memory. Stock texture quality, antialiasing, shadow quality and Unity LOD bias continue to apply. This set does not add a high-resolution texture mode.

## Export and integration

Blender authors Z-up metres. FBX exports explicitly use `axis_up='Y'`, `axis_forward='-Z'`, `use_space_transform=True`, `bake_space_transform=True`, `apply_unit_scale=True`, and `apply_scale_options='FBX_SCALE_ALL'`. Export options live in `art/first-set/build_assets.py`; `tools/blender_bridge.py` is the localhost MCP transport and executes that script. Geometry is authored directly at export scale, avoiding unapplied object scaling. The review-scene copies are display-only and are not exported.

Trees have a ground pivot and one-metre height before runtime scaling. Boulders occupy a centred unit box. Habitations have centred, approximately unit-box bounds including roof trim, and use the existing building footprint/height scaling. Existing buildings can therefore be unusually tall; this kit does not change saved-world settlement placement or generation.

No `.mu` exporter was installed in Blender. The first-party set instead exports FBX into the existing Unity 2019.4.18f1 project, which builds native prefabs in a separate `ringworldscenery` AssetBundle. It uses the same Windows/D3D11 target as this mod's visual bundle. This bundle is installed at `GameData/NivenRingworld/Assets/ringworldscenery`. Source `.blend` and FBX files are not copied into GameData or the gameplay release ZIP.

The existing `RINGWORLD_SCENERY_ASSET` `.mu` registry remains supported and takes precedence. Without a registered model, the loader picks one of the two bundled variants per slot. If the bundle is absent, the original procedural primitive fallback still works. `SceneryAssets.cs` owns this selection; `SurfaceStreamer.cs` supplies location, rotation, scale and soil friction.

Unity constructs three-level [LODGroups](https://docs.unity.cn/2019.4/Documentation/ScriptReference/LODGroup.html), switching at relative screen heights 0.12, 0.035 and 0.003, then culling. These are screen-size thresholds affected by camera FOV and stock LOD bias, not fixed distances. Switching currently changes meshes without crossfading. Far-ring terrain does not gain forest or city impostors from this change.

Collision remains simple and static: tree trunk capsules; convex low-detail boulders; habitation boxes plus convex roof prisms. Leaves have no collision and no imported rigidbodies are allowed. Doorways are visual recesses, not enterable interiors. Science and hazards remain existing gameplay systems.

## Rebuild and verify

From the mod root, with Blender MCP listening on localhost port 9876:

```powershell
python tools/blender_bridge.py
python tools/blender_bridge.py --script art/first-set/build_assets.py --output art/first-set/validation/blender-build.json
python tools/blender_bridge.py --script art/first-set/render_preview.py --output art/first-set/validation/blender-render.json
python art/first-set/validate_sources.py
.\build-scenery.ps1
.\smoke-test.ps1 -SceneryOnly
```

The generator only replaces its two named Ringworld scenes; hand edits to those scenes must be saved in another source file before rebuilding. Geometry counts/bounds are recorded in `manifest.json`. Unity import validation checks three LOD meshes per prefab, metre-scale Y-up geometry and tree ground pivots. The KSP smoke test checks all eight prefabs, supported materials/textures, runtime bounds, static collision proxies and downward contact rays, and renders each LOD to `validation/ksp-lod0.png` through `ksp-lod2.png`. It restores the normal game plugin afterward. This test is an isolated asset review, not a forest-density benchmark or a flight/EVA collision endurance test.


## Habitat and city expansion (2026-09-14)

See [HABITAT-LIBRARY.md](HABITAT-LIBRARY.md) for the complete 45 additional asset kinds, per-LOD triangle counts, placement and collision limitations. Source kits: `art/habitat-kit` and `art/city-kit`.
