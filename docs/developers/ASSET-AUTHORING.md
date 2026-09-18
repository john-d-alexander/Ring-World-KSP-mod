# Asset authoring

Use this guide to build or extend the scenery library. The current library contains 92 prefabs with three visual LODs each. Kit-specific counts below describe individual production stages. See the [asset inventories](../README.md#world-and-asset-reference) for models and placement rules. Commands run from the repository root.

<a id="blender-assets"></a>

## First Blender scenery set

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

### Export and integration

Blender authors Z-up metres. FBX exports explicitly use `axis_up='Y'`, `axis_forward='-Z'`, `use_space_transform=True`, `bake_space_transform=True`, `apply_unit_scale=True`, and `apply_scale_options='FBX_SCALE_ALL'`. Export options live in `art/first-set/build_assets.py`; `tools/blender_bridge.py` is the localhost MCP transport and executes that script. Geometry is authored directly at export scale, avoiding unapplied object scaling. The review-scene copies are display-only and are not exported.

Trees have a ground pivot and one-metre height before runtime scaling. Boulders occupy a centred unit box. Habitations have centred, approximately unit-box bounds including roof trim, and use the existing building footprint/height scaling. Existing buildings can therefore be unusually tall; this kit does not change saved-world settlement placement or generation.

No `.mu` exporter was installed in Blender. The first-party set instead exports FBX into the existing Unity 2019.4.18f1 project, which builds native prefabs in a separate `ringworldscenery` AssetBundle. It uses the same Windows/D3D11 target as this mod's visual bundle. This bundle is installed at `GameData/NivenRingworld/Assets/ringworldscenery`. Source `.blend` and FBX files are not copied into GameData or the gameplay release ZIP.

The existing `RINGWORLD_SCENERY_ASSET` `.mu` registry remains supported and takes precedence. Without a registered model, the loader picks one of the two bundled variants per slot. If the bundle is absent, the original procedural primitive fallback still works. `SceneryAssets.cs` owns this selection; `SurfaceStreamer.cs` supplies location, rotation, scale and soil friction.

Unity constructs three-level [LODGroups](https://docs.unity.cn/2019.4/Documentation/ScriptReference/LODGroup.html), switching at relative screen heights 0.12, 0.035 and 0.003, then culling. These are screen-size thresholds affected by camera FOV and stock LOD bias, not fixed distances. Switching currently changes meshes without crossfading. Far-ring terrain does not gain forest or city impostors from this change.

Collision remains simple and static: tree trunk capsules; convex low-detail boulders; habitation boxes plus convex roof prisms. Leaves have no collision and no imported rigidbodies are allowed. Doorways are visual recesses, not enterable interiors. Science and hazards remain existing gameplay systems.

### Rebuild and verify

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

<a id="landmark-assets"></a>

## Landmark architecture and woodland expansion

The release asset set adds **31 original Blender assets / 93 LOD meshes**, bringing the shipped library to **84 prefabs / 252 LOD meshes**. Editable scenes, generators, exports, manifests, previews and validation stay in `art/landmark-kit/`. `Ringworld-Landmarks.blend` contains the sources and four review scenes; the prior Blender scene is preserved in `before-landmarks.blend`.

### Architecture and scale

Twenty-five new architectural prefabs are placed by `GameData/NivenRingworld/Landmarks.cfg`. Each entry names an existing terrain site, metre offsets, width/height/depth, height above the ground and streaming distance. They do not change terrain generation or move old landmarks. The catalog is generated from the Blender manifest by `art/landmark-kit/catalog.py`.

- City: three suspended palace/citadel designs, three broken city-ring designs, archive vault, garden arcology and transit exchange. The largest wreck is 3.2 km wide. Suspended structures have their bottom 2.5 km above the sampled ground.
- Rim: portal, spaceport gate, 4 km elevator cathedral and 2.6 km transit viaduct. These are local works at the foot of the wall, not a complete elevator to its summit.
- Spill mountain: cascade outlets, sediment silos and a 2.2 km pump cathedral.
- Map island: aqueduct, intake works, observatory and stilt village.
- Scrith: resonator complex and an imagined protector sanctuary.
- Expedition region: weather spire, terraced settlement and reclaimed temple.

The designs are **interpretations**, not replicas of named canonical buildings. Palaces use broad terraces and machinery keels; wrecks expose broken perimeter ribs; service works use buttresses, open spans and industrial cylinders. Shapes, colours, dimensions and placements are artistic/gameplay decisions. Pumps, elevators, dishes and settlements are scenery: no operational machinery or NPC simulation is implied.

### Lore references

The [Ringworld appendix in the Known Space concordance](https://news.larryniven.net/concordance/content.asp?ovr=t&page=Ringworld+Appendix) on Larry Niven's site describes the vast engineered habitat, rim walls and rim transport. The [Ringworld timeline](https://news.larryniven.net/concordance/content.asp?page=Timeline+for+the+Ringworld) associates spill mountains, rim tunnels, spaceports, repair works and attitude-control infrastructure. The [official-site Ringworld summary](https://www.larryniven.net/?q=larry-niven-1938) describes the fallen technological civilization. These informed the infrastructure and ruins; the mesh designs are original. The concordance is a reference compilation, not a newly authenticated dimension specification for every building. No downloaded meshes, book text or cover art are included.

### Rendering and contact

Each asset has three merged visual meshes using the existing shared atlas. Export bakes Blender Z-up to Unity Y-up with `FBX_SCALE_ALL`, `use_space_transform` and `bake_space_transform`. Unity validates unit bounds; runtime dimensions supply metres. Huge structures stream independently of near-ground tiles, within 60 km of their centres, one new prefab per frame. They are not full-ring or map building impostors.

Architecture uses static, non-convex collision meshes derived from LOD2, preserving large openings instead of one solid bounding box. Contacts only enable near the structure. Fine trim, windows and tiny ledges may not match the simplified proxy; these are exterior environments, not furnished interiors. The collider stays the same when visual LOD changes. There are no dynamic megastructure rigidbodies.

New near-ground landmark geometry avoids being created around an already landed craft at floor level. A craft saved on a raised structural surface can still have that surface restored. Test installation saves should nevertheless be backed up before upgrading scenery into an existing expedition.

### Woodland

The original four individual tree variants and two seven-tree groves remain in the artist-facing library. Version 1.0.1 replaces runtime clump scattering with continuous merged canopy patches and nearby pooled trunk contacts. See [Colossi and continuous forests](../reference/COLOSSI-AND-FORESTS.md) for current placement variables, density, LOD and contact budgets. The 31-asset inventory above describes this kit; the total library now includes eight additional colossi.

## Registering model overrides

Update: the first-party [Blender scenery set](ASSET-AUTHORING.md#blender-assets) uses FBX to Unity prefabs in a dedicated AssetBundle. The `.mu` registration interface below remains available for overrides; `.blend` files stay in the mod's `art/` directory.

Keep .blend source files outside GameData. Export through the KSP/Unity .mu pipeline. Register model paths without extensions, one node per variant:

```cfg
RINGWORLD_SCENERY_ASSET
{
    kind = tree_broadleaf
    model = NivenRingworld/Models/Trees/Broadleaf01
}
```

Slots: tree_broadleaf, tree_conifer, rural_building, boulder. Multiple entries per kind give deterministic variants. Missing entries retain procedural placeholders. Imported materials are preserved. Child objects use the ground layer. Include simple separate colliders; do not use dense leaf meshes as collision geometry.

Trees use +Y up, a ground-level root pivot and one metre authored height; code scales uniformly. Buildings and boulders use centre pivots and a normalized unit bounding box, scaled independently to the generated footprint/height. Imported Unity LODGroups can handle local detail levels. Long-distance tree/city impostors remain a separate future renderer.

Suggested art families: several irregular broadleaf species, layered/narrow conifers, dead trees/stumps, boulders, rural dwellings, abandoned city modules, bridges, terminals and scrith machinery. Whole-tree slots replace trunk and canopy together. City landmark slots and interiors remain future work alongside their authored assets.

<a id="asset-tracker"></a>

## Further asset work

Potential contributions include additional tree species and foliage materials; accessible city decks and interiors; damaged city-disk variants; biome-specific settlement styles; animated industrial equipment; detailed rim elevators; and distant city impostors. Keep sunflower beam hazards, working pumps, NPC ecology and exposed-grid power systems separate from visual asset work: a mesh does not implement those mechanics.

Use shared atlases and merged meshes for grass, reeds, litter and other small cover. Avoid a GameObject for every blade or pebble. Keep foliage collision-free and use simple separate trunk/building proxies. Keep editable sources, exports and previews under `art/`, outside the runtime GameData tree.
