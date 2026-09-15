# Habitat and city asset inventory

Original Blender assets; three LOD meshes per prefab. First-pass stylized art, not final canonical replicas. Sources, FBX exports, manifests and review images are retained under art/.

| Family | Asset kind | LOD triangles (0 / 1 / 2) | Collision |
|---|---|---|---|
| vegetation | grass_patch | 36 / 20 / 10 | none |
| vegetation | reed_patch | 504 / 280 / 140 | none |
| vegetation | fern_patch | 936 / 360 / 180 | none |
| vegetation | desert_scrub | 576 / 220 / 110 | none |
| vegetation | bush | 576 / 220 / 110 | none |
| vegetation | mirror_sunflower | 184 / 136 / 84 | none |
| vegetation | root_cluster | 156 / 108 / 64 | none |
| vegetation | dead_tree | 268 / 188 / 112 | box |
| vegetation | stump | 156 / 108 / 64 | box |
| vegetation | fallen_log | 92 / 76 / 16 | box |
| vegetation | driftwood | 92 / 76 / 16 | box |
| vegetation | leaf_litter | 128 / 72 / 32 | none |
| vegetation | mushrooms | 320 / 144 / 72 | none |
| vegetation | pebble_cluster | 392 / 80 / 32 | none |
| ruins | scrith_outcrop | 96 / 80 / 20 | box |
| ruins | conduit_live | 180 / 132 / 84 | box |
| ruins | conduit_dark | 180 / 132 / 84 | box |
| ruins | levitation_grid | 564 / 388 / 244 | box |
| ruins | debris_beam | 96 / 72 / 48 | box |
| ruins | city_disk_fragment | 276 / 180 / 96 | box |
| ruins | crashed_city_disk | 268 / 172 / 88 | box |
| ruins | ruin_tower | 336 / 276 / 276 | box |
| ruins | habitat_block | 360 / 360 / 360 | box |
| settlements | stone_enclosure | 320 / 120 / 80 | box |
| settlements | watchtower | 232 / 196 / 160 | box |
| settlements | campfire | 148 / 118 / 98 | box |
| settlements | bridge_segment | 108 / 108 / 108 | box |
| settlements | research_station | 176 / 144 / 120 | box |
| infrastructure | flup_outlet | 516 / 356 / 236 | box |
| infrastructure | sediment_nozzle | 516 / 356 / 236 | box |
| infrastructure | transit_tube | 516 / 356 / 236 | box |
| infrastructure | rim_hatch | 276 / 212 / 84 | box |
| infrastructure | rim_airlock | 264 / 200 / 72 | box |
| infrastructure | elevator_base | 92 / 92 / 92 | box |
| infrastructure | terminal_gantry | 92 / 92 / 92 | box |
| infrastructure | maglev_segment | 168 / 120 / 96 | box |
| infrastructure | transport_causeway | 168 / 120 / 96 | box |
| infrastructure | maintenance_platform | 96 / 96 / 96 | box |
| infrastructure | pump_station | 400 / 256 / 136 | box |
| infrastructure | roof_machinery | 400 / 256 / 136 | box |
| cities | floating_city | 9628 / 3024 / 684 | none |
| cities | fallen_city | 9152 / 2740 / 512 | box |
| cities | city_arcology | 3568 / 1572 / 280 | box |
| cities | ruined_district | 508 / 348 / 104 | box |
| cities | city_concourse | 252 / 252 / 132 | box |

The eight initial trees, rocks and rural buildings remain in art/first-set. Total: 53 prefabs and 159 LOD meshes. All sets share the existing 512-pixel atlas.

City placement: the abandoned-city landmark receives the fallen disk, district, arcology and concourse. The floating city is exported and bundled for later placement and deck/collision work; it is not spawned as a landable city. The older crashed_city_disk remains available in the registry as an alternative. Box collision proxies include gaps between structures: interiors and open concourses are not yet traversable.

Close vegetation uses a bounded merged mesh (32 laptop / 64 high-quality instances), alongside existing cheap ground cover. Named-site props and forest/shore replacements use the existing streaming radius. These assets are not drawn individually across the entire distant ring.

Export convention: centered unit bounds for the new kits; runtime dimensions supply metres. Blender +Z is baked to Unity +Y with FBX_SCALE_ALL, use_space_transform and bake_space_transform. Unity import and in-game checks verify upright bounds and three LODs.

Next art work: collision meshes for accessible city decks, damaged disk variants, interiors, distinctive settlement styles, rim elevator detail, material/texture refinement and animated industrial props. No biological hazards or operational pumps are implied by the meshes.
