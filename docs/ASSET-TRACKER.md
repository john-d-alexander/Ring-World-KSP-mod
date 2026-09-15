# Ringworld asset tracker

The user's Ringworld reference list guides these families. Locations and procedural distributions are adaptations, not reconstructed maps from the books. Niven's [Ringworld study guide](https://larryniven.net/?q=ringworld-study-guide) and [The Color of Sunfire](https://www.larryniven.net/?q=the-color-of-sunfire) provide useful author-hosted context; the latter informs the silver mirror-blossom appearance. Detailed scientific behaviour and canonical structures need separate implementation and validation.

| Family | Current state | Blender / later work |
|---|---|---|
| Broadleaf and conifer trees | First Blender set: two broadleaf and two conifer variants, three local LODs, shared atlas, trunk colliders; existing climate density | More species, higher-detail foliage, dead trees, stumps and distant impostors |
| Grass and low cover | Batched triangle clumps | Shared atlas for grasses, reeds, ferns and scrub; retain batching |
| Riverbank stones and sand | Small batched stones and shoreline colour transition | Pebble clusters, gravel textures, driftwood, reeds; distinct beach materials |
| Forest floor | Simple leaf-litter triangles, canopy-aware raycast placement | Leaf atlas, twigs, roots, mushrooms, fallen logs |
| Mirror sunflowers | Sparse seeded silver-petal patches, visual only | Stalk/petal variants and patch LOD; focused-light hazard is a separate gameplay system |
| Scrith outcrops | Grey procedural surface mask plus existing named excavation | Polished eroded plates, seams, gouges and transition decals |
| Rural buildings | Two Blender habitation variants with roof/door/beam detail and three LODs, used by existing clusters | More river/coastal families, enclosures, wooden towers, campfire sites; no NPC ecology yet |
| Large boulders | Two Blender weathered/sandstone variants with three LODs and convex proxies | More erosion silhouettes and biome-specific selection; current variants use existing generic boulder placement |
| Floating-city ruins | Existing named city interpretation | Crashed disk sections, collapsed towers, levitation grids and debris distributions; decay belts not implemented |
| Rim infrastructure | Existing terminal and wall geometry | Hatches, airlocks, elevator feet, transit tubes, maglev ruins; repeated access-point generator not implemented |
| Water recycling industry | Spill-mountain analogue | Flup outlets, sediment nozzles, pump stations, pipes and maintenance platforms; machinery scatter not implemented |
| Conductive mesh | Not implemented | Exposed dark/live conduits; shared emissive materials, power-state gameplay later |
| Dust and pollen | Cheap local billboard mesh | Particle texture atlas; richer wind animation without expensive volumetrics |

## Authoring contract

Existing registered model slots are `tree_broadleaf`, `tree_conifer`, `rural_building`, and `boulder`; the registry contract and pivots are documented in [ORBITS-WARP-AND-ASSETS.md](ORBITS-WARP-AND-ASSETS.md). Entries in this tracker do not imply those additional slot types already exist.

Keep `.blend` sources outside GameData, inside the mod's `art/` folder. The first-party set uses Blender FBX to Unity prefab/AssetBundle exports; registered `.mu` models remain supported as overrides. See [BLENDER-ASSETS.md](BLENDER-ASSETS.md) for the source file, reproducible scripts, IDs, budgets and validation. Plan several silhouette variants with shared materials/atlases. Use simple trunk/building collision proxies; foliage, grass and litter should remain collision-free. Imported Unity LODGroups support local assets. Ground microdetail needs a batch/atlas adapter before swapping each blade for an individual imported GameObject.

Far LOD currently draws terrain and water. It does not render every distant tree, ruin or pipe. Future city and forest impostors need their own distance/visibility budgets. Prioritize silhouette and shared materials over polygon density so the laptop remains the baseline.


## Habitat and city expansion (2026-09-14)

See [HABITAT-LIBRARY.md](HABITAT-LIBRARY.md) for the complete 45 additional asset kinds, per-LOD triangle counts, placement and collision limitations. Source kits: `art/habitat-kit` and `art/city-kit`.
