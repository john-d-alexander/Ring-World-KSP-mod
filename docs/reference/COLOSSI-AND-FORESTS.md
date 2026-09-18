# Colossi and continuous forests (1.0.2)

Eight original Blender superstructures add 24 visual LOD meshes, bringing the library to 92 prefabs / 276 LOD meshes. Sources, FBX exports, the manifest, previews and import diagnostics remain under `art/colossus-kit/`. Open `Ringworld-Colossi.blend` for editable geometry and the review scene.

These are deliberately enormous fictional additions. The [Ringworld study guide on Larry Niven’s site](https://larryniven.net/?q=ringworld-study-guide) identifies a floating castle, skyscraper city and spaceport; the [Known Space concordance appendix](https://news.larryniven.net/concordance/content.asp?ovr=t&page=Ringworld+Appendix) documents rim walls and rim transport. These support the architectural themes. The following names, dimensions and designs are original interpretations, not claimed canonical structures.

| Asset | Width / height / depth (km) | LOD0 / LOD1 / LOD2 triangles | Design theme |
|---|---|---|---|
| continent_crown | 48 / 12 / 48 | 33376 / 6344 / 1380 | city |
| rim_gate_titan | 28 / 80 / 16 | 688 / 400 / 256 | rim |
| skyway_leviathan | 120 / 18 / 10 | 792 / 792 / 792 | city |
| world_flup_mouth | 60 / 22 / 26 | 8772 / 3540 / 1212 | spill |
| scrith_memory_spire | 9 / 45 / 9 | 4944 / 1992 / 696 | arrival |
| broken_city_halo | 90 / 8 / 90 | 3576 / 1560 / 672 | city |
| atmosphere_harp | 36 / 50 / 12 | 1336 / 1032 / 896 | scrith |
| rim_engine_reliquary | 42 / 65 / 32 | 9572 / 3956 / 1452 | rim |

The floating city plate has an 8 km clearance beneath it. The rim gate rises halfway up the default 160 km rim wall. The causeway spans 120 km. The engine reliquary and atmosphere harp are inert monuments, not operational propulsion or weather machinery. Interiors, NPCs and moving elevators are not included.

## Placement, performance and customization

`GameData/NivenRingworld/Colossi.cfg` now defines eight `RINGWORLD_COLOSSUS_ASSET` templates rather than fixed landmark attachments. `RINGWORLD_COLOSSUS_DISTRIBUTION` uses 2,000 km cells with 15% candidate occupancy; dry-ground and hull-boundary checks reduce the realized number. Each occupied cell contains at most one seeded, jittered candidate and a seeded choice of template. Empty cells remain empty across visits and reloads. Candidate identity wraps at the circumference seam. Only nearby cells are queried; the entire ring is never enumerated.

A 600 km clearance around every named landmark means **zero** of these new colossi near the landmark clusters by default (within the requested zero-to-one limit). Ordinary city buildings remain at their authored sites. The existing eight large fixed placements are removed. The minimum configurable cell size is 1,000 km; occupancy can be 0..1. Streaming range, dimensions and elevation remain template properties. Distance includes the structure’s extent. Models stream one per frame, independently of ground tiles. Placement rejects wet centres and wall overlap, so all eight do not necessarily appear near a given seed/site. New ground-level collision geometry is not spawned around an already landed vessel. Three merged meshes share the existing 512px atlas; there are no megastructure rigidbodies. Simplified static LOD2 contact meshes retain principal openings, but not fine architectural trim. Large shapes use the ring tangent at their centre; they are not planet-scale deforming structures. They currently render in local flight space, not as building icons or scaled-map impostors.

## Continuous forests

The former 16/24/32 seven-tree clump cap has been replaced by merged, three-LOD canopy patches. Individual global seed cells average 24 m apart at full stock scatter and forest density 1; seed jitter breaks up rows. Crowns overlap, with heights of 30–48 m. A continuous 380 m noise mask controls glades and woodland boundaries, gated by the existing climate TreeCover. Forest requires TreeCover >= .32; acceptance compares noise to `(TreeCover - .22) * 1.9`. Wet ground, roads, ruins and rim walls remain excluded. Grassland is not turned into a uniform forest.

Density is `min(2, forestDensity * stockScatter)`; zero disables the supplement. Spacing is `24 / sqrt(max(.2, density))` metres. Tree positions use salts 2101/2103; stand mask 2111; height 2113; crown size 2117; orientation 2119; crown asymmetry and colour 2121. Each near-terrain tile is divided into patches at most 256 m wide. The LOD screen-height thresholds are .38 / .12 / .0005. Only the nearest visual LOD casts tree shadows. Distant crowns omit trunks and use ten triangles for conifers or thirty for rounded broadleaf crowns. Meshes are merged per patch and CPU mesh copies discarded after upload.

Trunk capsules are pooled and enabled within 180 m of loaded ring vessels, checked four times per second. Foliage has no collision; newly generated trunks leave 18 m around loaded craft. Capsule contacts do not cover the entire visual forest. Original Blender tree variants and isolated-tree scatter remain available. The new mass canopy uses matt atlas-textured foliage with randomized rotation, asymmetry and rounded broadleaf crowns. Leaves and bark share one material per merged patch. It uses procedural opaque geometry, with no transparent leaf-card overdraw. Individual crowns end at the near-terrain tile extent, but the forest itself continues through terrain LODs. `BiomePresentation` is the shared core descriptor: canopy cover, canopy height and canopy colour, with the same climate and 380 m stand mask as near trees. Near ground also carries the forest colour so canopy mesh culling from high altitude does not expose a bright square.

Outside the near tiles, intermediate blocks use one merged, collider-free canopy mesh each. The smallest blocks use the same seed cells, spacing, jitter and height as the close forest when within the candidate cap; coarser blocks double cell spacing in powers of two. Laptop uses at most 8 x 8 candidates per block through 8 km block sizes; Balanced uses 16 x 16 through 16 km; Ultra/photo mode uses 24 x 24 through 32 km. The immediately adjacent tile-sized blocks retain up to 32 x 32 candidates to match the near forest. Each simplified crown has 10 triangles for a conifer or 30 for a broadleaf. A shared matte foliage atlas keeps near/middle colours consistent. Intermediate crowns cast no shadows and have no trunk contacts. Their spacing increases with distance, so large clusters represent multiple trees rather than implausibly tall giant trees.

Still larger blocks retain area-filtered forest colour and a 24–48 m canopy envelope at the normal terrain resolution. Colour and mean height continue through local and scaled-space terrain LODs out to the selected terrain range. Near ground also carries canopy colour. The final near-crown culling threshold was lowered to avoid checkerboard holes during high-altitude views. Individual trees are not rendered around the entire ring. Intermediate mesh work shares the existing nearest-first terrain queue, but samples rows over multiple frames. The default soft sampling budget is 2 ms / at most eight rows per frame, multiplied by the terrain generation-budget setting. Publishing a completed mesh can add upload time. Queued work retains the original patch orientation across a relocation, and retired jobs are discarded. Photo readiness includes both terrain and canopy queues, so a still waits for its forest detail.

Future special biomes can extend the `BiomeAppearance` descriptor and its sampling in `BiomePresentation`, while retaining the existing terrain streaming and material pipeline. Zero stock scatter or zero forest density disables added canopy relief.

The defaults target the laptop preset, but measured frame time and memory depend on the view and hardware. See [VALIDATION.md](../history/VALIDATION.md) for actual in-game measurements rather than assuming a universal frame-rate guarantee.
