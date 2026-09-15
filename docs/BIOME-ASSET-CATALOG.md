# Combined biome and asset catalog

Applies to generator 4. This is the development reference, not a runtime configuration file. `Ecology.cs`, `Terrain.cs`, `SurfaceStreamer.cs`, `GroundDetails.cs`, and `AmbientGroundWeather.cs` are the current implementation. Keep this catalog and the frequency survey in sync when adding biomes or scatter rules. Existing saves keep their generator version.

## Distribution and appearance

Three seeded continuous fields define temperature, moisture and ruggedness. Temperature samples noise at 210,000 m with salt 701, moisture at 160,000 m / 709, and ruggedness at 310,000 m / 719. Each maps `(noise - .25) * 2` into [0,1]. Temperature receives up to a .4 cold bias across the outermost 6% of each half-width. This is an artistic rim climate model, not a thermal simulation.

For each family, weight = exp(-((temperature-T)^2 + (moisture-M)^2 + .4*(ruggedness-R)^2)/.045). Normalize the five weights to sum to one. Ground RGB, sky RGB multiplier, tree-cover coefficient and relief coefficient are weighted blends; the strongest family supplies the dry-land biome label. The Snow label also covers cold steppe/tundra and need not be visually snow-covered.

| Family / dry label | T, M, R centre | Ground RGB | Sky multiplier RGB | Tree-cover coefficient | Relief coefficient |
|---|---|---|---|---:|---:|
| Meadow / Grassland | .60, .43, .35 | .36, .46, .20 | .98, 1, 1.02 | .18 | .70 |
| Forest | .57, .78, .40 | .19, .34, .15 | .94, 1.03, 1.01 | .92 | .85 |
| Desert | .80, .18, .40 | .66, .53, .32 | 1.07, .99, .91 | .03 | .55 |
| Cold / Snow | .18, .45, .40 | .52, .56, .44 | .97, 1.01, 1.06 | .07 | .60 |
| Highland / Mountain | .42, .50, .90 | .43, .43, .36 | .99, 1, 1.04 | .12 | 1.80 |

These coefficients are not area percentages or guaranteed tree probabilities. Sampling 100,000 uniformly distributed surface points on each of three seeds gives the estimates below. Small named sites and thin wall areas may receive no samples; 0.000% does not mean they do not exist. Reproduce with `dotnet run --project tools/BiomeSurvey -c Release`. Full per-seed output: [BIOME-FREQUENCY-SURVEY.txt](BIOME-FREQUENCY-SURVEY.txt).

| Final biome | Sampled surface share, three seeds | Terrain / placement override | Current assets and details | Planned assets |
|---|---:|---|---|---|
| Grassland | 26.52–26.79% | Meadow dominates dry ordinary land | Grass, occasional mirror sunflowers, climate-weighted trees, boulders; rural clusters possible | Grass species, flowering meadow plants, shrub and hominine dwelling families |
| Forest | 23.28–23.53% | Forest dominates dry ordinary land | Broadleaf/conifer silhouettes, grass and leaf litter, boulders, pollen | Irregular tree species, roots, logs, mushrooms, fern and leaf atlases |
| Desert | 9.42–9.65% | Desert dominates; dunes enabled when desert weight > .3 | Sand patches, sparse climate trees, boulders, drifting dust; rural clusters possible | Scrub, dry wood, desert hut families, dune material detail |
| Snow | 22.82–23.07% | Cold family, plus snow treatment at high elevation | Stones and sparse climate trees | Cold scrub, snow/ice material detail, hardy conifers |
| Mountain | 15.43–15.59% | Highland family or engineered named mountains | Stones, climate-weighted trees, boulders | Rock strata, talus, engineered scrith faces |
| Ocean | .162–.163% | Broad-noise depressions / two named Great Oceans | Water surface, sandy shore tint, dry shoreline pebbles | Surf, beach gravel, driftwood; coastal settlements |
| Lake | .305–.332% | Analytic catchment basins / small pond mask | Water surface, dry bank stones and tint | Reeds, muddy banks, aquatic plants |
| River | .177–.184% | Warped analytic channels and tributary forms | Water ribbons, bank stones and shore tint | Reed beds, gravel bars, bridges, riverbank villages |
| Wetland | .601–.609% | Near-water terrain classification | Dry ground cover / climate trees when not wet | Rushes, mud flats, wetland shrubs |
| Scrith | .407–.420% | Erosion mask / named excavation | Grey surface treatment; micro-cover suppressed | Polished plates, seams, exposed conductive grids |
| Road | .177–.193% | 65 m half-width ancient corridor mask on dry terrain | Surface colour; micro-cover suppressed; climate trees can still occur under current scenery rules | Road edges, broken paving, junctions and bridge modules |
| Ruins | Below survey resolution | Named city/outpost/terminal pads | Existing procedural buildings; no ground micro-cover | Crashed city disks, towers, levitation grids, debris belts |
| Rimwall | Below survey resolution | Ribbon edge / wall infrastructure | Near wall collision mesh, distant wall geometry, named terminal | Hatches, elevators, airlocks, transit/maglev ruins |

## Terrain variables

Ordinary v4 height = 100 + heightMultiplier * min(100, 10 + 60*hills + 5*detail + .008*mountainContribution + .15*dunes), before channel/basin/landmark overrides. Height multiplier is a saved new-world control (.25–3). Giant mountains are engineered/named features; the ordinary Highland label does not imply kilometre-high mountain relief.

River catchments use 131,072 m cells deformed by 180,000/145,000 m noise fields, with up to 45,000/35,000 m coordinate displacements. Channel width is 260 + 340*noise metres. Basin lakes use elliptical radii about 2,700–4,500 m along and 2,000–3,400 m across. Small ponds use 4,096 m cells, candidate probability .28*pondAmount, radius 90–320 m and an elliptical cross-axis. Pond amount is a saved new-world control (0–2). None of these constitutes global hydrological connectivity or simulated water flow.

Named pads/mountains/oceans and roads override the ordinary climate labels. Scrith exposure is a visual erosion mask, not a separately simulated soil layer. Adding a biome requires deciding its precedence relative to water, roads and landmarks, not merely adding another enum value.

## Scatter rules and budgets

* Local scenery: 64 jittered candidates per 1,024 m tile. Native KSP Terrain Scatters must be enabled. A separate hash is compared against the native factor (0–1). Tree probability uses climate TreeCover * 1,100 m grove noise * forestDensity (0–2). Conifer versus broadleaf uses 18,000 m noise > .5; height is 8 + 12*variation metres. If no tree was chosen, variation > .9 can select a boulder. These gates share random variables, so multiply-and-round density estimates are not exact.
* Rural clusters: a tile-centre Grassland or Desert label and hash > .97; up to five 18×24 m buildings, each 5–24 m high. Further dry/road checks can reduce the count. Buildings do not follow the foliage toggle. Dedicated river/coast villages are not implemented.
* Micro-cover: native density gate, 25–250 m range (default/laptop 75), hidden above 80 m clearance. Jitter spacing = max(4, range/20) metres, hard cap 2,200 clumps, one mesh, no colliders or shadow casting. Placement uses actual terrain collider raycasts, skips slopes steeper than an up-normal dot of .75, wet samples, Ruins, Scrith, Rimwall and Road.
* Detail precedence: shoreline/Mountain/Snow stones first; then sunflower patches where meadow weight > .35 and 1,400 m noise > .58; then leaf litter under detected canopies or forest weight > .45 with the density sub-gate; then sand when desert weight > .55; otherwise grass. Mirror flowers are visual only, without beam damage.
* Airborne detail: one mesh, at most 48 billboards, 10 Hz updates, within 30 m of dry ground. Dust when desert weight > .35, otherwise pollen weighted by forest*.4. Separate Ringworld toggle; no physical wind.
* Far LOD draws terrain/water and the coarse ring/walls. It does not populate distant trees, cities or machinery. The default is one generated block per frame with 8 subdivisions. Completed blocks appear immediately; the coarse ring supplies a distant fallback during generation.

The current terrain-shell contact profile is shared by all biomes: static friction 1, dynamic friction .85, maximum friction combination, zero bounce. Biome-specific ice/sand/soil physics is not implemented. Local walls use a separate closed collider.

## Asset interface and future additions

The [first Blender set](BLENDER-ASSETS.md) now supplies two variants for each existing slot, through a first-party prefab bundle. Broadleaf/conifer selection, climate weights, grove noise, native scatter density, tree heights, boulder gates and rural-cluster rules above are unchanged. The two variants within each slot are selected deterministically by the existing variant index modulo two; sandstone is an appearance variant, not a newly desert-exclusive scatter rule. Registered `.mu` models override the bundle. See `art/first-set/manifest.json` for all stable IDs and per-LOD mesh bounds/counts. The bundle adds no terrain seed/version migration, science bonus, new hazard or distant impostor renderer.

Live import slots: `tree_broadleaf`, `tree_conifer`, `rural_building`, `boulder`. Multiple `RINGWORLD_SCENERY_ASSET` nodes per kind supply deterministic variants; `model` is the GameDatabase `.mu` path without extension. Whole trees use a ground pivot and normalized one-metre height; buildings/boulders use centred, unit-box proportions. Preserve materials and include simple collision proxies where necessary. Grass/leaf/flower replacements need a batched atlas adapter; do not instantiate thousands of separate Blender exports.

Future families: crashed floating-city disk sections, superconductive grid fragments, collapsed towers, primitive huts/enclosures/campfires/towers, rim access hatches/airlocks/elevators, maglev/transit ruins, flup outlets/pumps/sediment nozzles, exposed dark/live conduits. These are catalogued design targets, not implemented scatter systems.

For each addition record: stable ID; applicable climate weights/final labels; density formula and independent salt; exclusions and slope/water tests; physical scale/pivot; model variants and material atlas; LOD distances and draw/vertex budget; collider policy; science/hazard behaviour; persistence/versioning impact; survey output and runtime test scene. Reuse existing slots where their transform contract fits; otherwise add a documented registry kind. Terrain or placement changes affecting existing craft require an explicit generator migration.


## Habitat and city expansion (2026-09-14)

See [HABITAT-LIBRARY.md](HABITAT-LIBRARY.md) for the complete 45 additional asset kinds, per-LOD triangle counts, placement and collision limitations. Source kits: `art/habitat-kit` and `art/city-kit`.
