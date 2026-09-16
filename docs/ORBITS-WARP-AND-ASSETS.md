# Ringworld 0.5: trajectories, time and assets

## Gravity and map predictions

There is no new spherical celestial body or sharp physical SOI. The Sun remains the stock reference body. In inertial flight the real forces are stellar attraction and ribbon attraction. Centrifugal and Coriolis terms apply only in the rotating expedition chart. Ring spin supplies inherited velocity, not attraction.

The assumed mass distribution is a uniform cylindrical ribbon of radius R, axial width W and surface density sigma. Total mass is 2*pi*R*W*sigma. Default sigma is 1,000,000 kg/m2: a gameplay assumption, not a claim about fictional scrith. Near the floor its pull is approximately 0.0004 m/s2, far below the default 9.72 m/s2 spin acceleration. Width is integrated analytically; azimuth uses Gaussian quadrature on logarithmic panels. A 50 m Plummer softening length avoids a singular ideal sheet. Walls, mountains, buildings and the star's reaction are not separately modelled.

The cyan map curve integrates a vacuum coast under the same ribbon and stellar field used by flight. Expedition predictions use the rotating map chart; external predictions use inertial coordinates. It replaces the active solar vessel's stock spline when enabled. Stock manoeuvre nodes, orbit statistics and other vessels' conics remain two-body data. The curve stops at atmosphere entry; inside air the panel explains why no vacuum curve is shown. Drag, lift, future thrust and manoeuvres are not predicted. A sampling budget can truncate a long forecast near the floor.

Unpacked solar vessels receive ribbon attraction. Active packed solar flight receives numerical propagation. Other planetary SOIs and inactive/unloaded vessels retain stock propagation. This is not a full N-body conversion. Extreme warp, planetary encounters and arbitrary vessel-switch/save transitions need more validation.

Research: [Igata, circular ring potential](https://arxiv.org/abs/2005.01418) and [Bannikova et al., torus potential](https://arxiv.org/abs/1009.4324). These support treating the mass as extended. Our finite-width softened ribbon is its own approximation, not either paper's full torus solution.

## Surface time acceleration

Superseded in v0.7: use KSP's stock warp controls with the resting ring-surface anchor. See [STOCK-WARP-AND-RENDERING.md](STOCK-WARP-AND-RENDERING.md). The old separate UT-increment buttons and their non-accelerated-resource behaviour no longer apply.

## Expanded settings

Terrain horizon is a numeric kilometre field with a minimum of 200 and no fixed upper cap. Values exceeding the ring extent do not generate repeated laps. Laptop preset: 160,000 km, 8 subdivisions, one new block/frame. Workstation: 1,000,000 km, 32 subdivisions, two blocks/frame. Distant meshes are terrain/water; trees and small buildings remain local. Hardware performance is not guaranteed.

New-world controls include diameter (2 to 200 million km), width (10,000 km to one quarter of diameter), walls (60 to 1,000 km), spin acceleration (1 to 30 m/s2), floor mass density (0 to 100 million kg/m2), seed, height, forests and pond coverage. They lock after an expedition or discovery to preserve terrain beneath saved craft.

Runtime controls include shadow-square cycle (one minute to 30 days), prediction duration (1 to 1,440 minutes), numerical map toggle, surface wait limit (10 to 10,000x), weather and rendering quality. Day length is independent of ring spin period, which follows radius and spin acceleration.

New saves use terrain generation v3. Existing saved generator versions remain unchanged. V3 continuously deforms river catchments, adds small ponds, and scatters tree candidates throughout a tile instead of one per small grid cell. Regions mix broadleaf and conifer silhouettes. Existing ranges, foothills, deserts, two Great Oceans, islands and named lakes remain. These are analytic landscapes, not hydraulic erosion. A selection bug that prevented rural habitation clusters with the newer noise generator is fixed.

## Blender model contract

Update: the first-party [Blender scenery set](BLENDER-ASSETS.md) uses FBX to Unity prefabs in a dedicated AssetBundle. The `.mu` registration interface below remains available for overrides; `.blend` files stay in the mod's `art/` directory.

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
