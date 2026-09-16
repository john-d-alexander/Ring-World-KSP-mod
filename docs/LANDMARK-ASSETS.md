# Landmark architecture and woodland expansion

The release asset set adds **31 original Blender assets / 93 LOD meshes**, bringing the shipped library to **84 prefabs / 252 LOD meshes**. Editable scenes, generators, exports, manifests, previews and validation stay in `art/landmark-kit/`. `Ringworld-Landmarks.blend` contains the sources and four review scenes; the prior Blender scene is preserved in `before-landmarks.blend`.

## Architecture and scale

Twenty-five new architectural prefabs are placed by `GameData/NivenRingworld/Landmarks.cfg`. Each entry names an existing terrain site, metre offsets, width/height/depth, height above the ground and streaming distance. They do not change terrain generation or move old landmarks. The catalog is generated from the Blender manifest by `art/landmark-kit/catalog.py`.

- City: three suspended palace/citadel designs, three broken city-ring designs, archive vault, garden arcology and transit exchange. The largest wreck is 3.2 km wide. Suspended structures have their bottom 2.5 km above the sampled ground.
- Rim: portal, spaceport gate, 4 km elevator cathedral and 2.6 km transit viaduct. These are local works at the foot of the wall, not a complete elevator to its summit.
- Spill mountain: cascade outlets, sediment silos and a 2.2 km pump cathedral.
- Map island: aqueduct, intake works, observatory and stilt village.
- Scrith: resonator complex and an imagined protector sanctuary.
- Expedition region: weather spire, terraced settlement and reclaimed temple.

The designs are **interpretations**, not replicas of named canonical buildings. Palaces use broad terraces and machinery keels; wrecks expose broken perimeter ribs; service works use buttresses, open spans and industrial cylinders. Shapes, colours, dimensions and placements are artistic/gameplay decisions. Pumps, elevators, dishes and settlements are scenery: no operational machinery or NPC simulation is implied.

## Lore references

The [Ringworld appendix in the Known Space concordance](https://news.larryniven.net/concordance/content.asp?ovr=t&page=Ringworld+Appendix) on Larry Niven's site describes the vast engineered habitat, rim walls and rim transport. The [Ringworld timeline](https://news.larryniven.net/concordance/content.asp?page=Timeline+for+the+Ringworld) associates spill mountains, rim tunnels, spaceports, repair works and attitude-control infrastructure. The [official-site Ringworld summary](https://www.larryniven.net/?q=larry-niven-1938) describes the fallen technological civilization. These informed the infrastructure and ruins; the mesh designs are original. The concordance is a reference compilation, not a newly authenticated dimension specification for every building. No downloaded meshes, book text or cover art are included.

## Rendering and contact

Each asset has three merged visual meshes using the existing shared atlas. Export bakes Blender Z-up to Unity Y-up with `FBX_SCALE_ALL`, `use_space_transform` and `bake_space_transform`. Unity validates unit bounds; runtime dimensions supply metres. Huge structures stream independently of near-ground tiles, within 60 km of their centres, one new prefab per frame. They are not full-ring or map building impostors.

Architecture uses static, non-convex collision meshes derived from LOD2, preserving large openings instead of one solid bounding box. Contacts only enable near the structure. Fine trim, windows and tiny ledges may not match the simplified proxy; these are exterior environments, not furnished interiors. The collider stays the same when visual LOD changes. There are no dynamic megastructure rigidbodies.

New near-ground landmark geometry avoids being created around an already landed craft at floor level. A craft saved on a raised structural surface can still have that surface restored. Test installation saves should nevertheless be backed up before upgrading scenery into an existing expedition.

## Woodland

The original four individual tree variants and two seven-tree groves remain in the artist-facing library. Version 1.0.1 replaces runtime clump scattering with continuous merged canopy patches and nearby pooled trunk contacts. See [Colossi and continuous forests](COLOSSI-AND-FORESTS.md) for current placement variables, density, LOD and contact budgets. The 31-asset inventory above describes this kit; the total library now includes eight additional colossi.
