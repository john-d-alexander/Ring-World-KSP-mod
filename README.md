# Niven Ringworld Expedition 1.0.3 — KSP 1.12.5

A Larry Niven-inspired, star-encircling habitat with a landable rotating interior, procedural terrain, atmosphere, science and persistent expeditions. The default world uses one-tenth of the published linear ring dimensions. Original geography and architecture are interpretations of the setting.

## Install and play

Close KSP and copy the ZIP's `GameData/NivenRingworld` into your KSP `GameData`. The included `GameData/000_Harmony` is the HarmonyKSP dependency; keep a single compatible installation if other mods already provide it. Restart after updating DLLs. No Kopernicus or downloaded art pack is required. The supported release target is KSP 1.12.5 on Windows x64 / Direct3D 11.

Build and launch a lander with enough thrust for approximately 1 g. The stock toolbar's ring icon, or Left Alt+R, opens the Ringworld panel. Sandbox exposes site relocation, a random terrain visit and a spin-matched training approach; Science and Career show flight information without those development controls. A relocation starts above the terrain at rest relative to the ring: **you must brake and land it**.

For ordinary orbital arrival, enter the annular region near the ring. The mod preserves your velocity; it does not supply free braking. An unmatched solar trajectory encounters hundreds of km/s of air-relative motion and is destructive. Leaving the rotating frame restores the corresponding inertial motion, not a shortcut back to Kerbin.

When settled on dry ground, use **KSP's stock time-warp controls**. Universal time and the other celestial bodies advance normally. Save and return to the Space Center through the ordinary controls. **Revert Flight intentionally discards progress.** Airborne/physics warp in the ring frame is not supported.

## Release contents

- Ring-relative centrifugal/Coriolis dynamics, local atmosphere and automatic frame entry/departure.
- Local navball cues, SAS direction targets, altimeter and vertical-speed gauge, parachute deployment gates and numerical map encounter preview.
- Landed residence, pause-menu saving, reload through the Space Center, Ringworld science subjects and tested deployed-science anchoring.
- Seeded terrain, smooth climate biomes, rivers, lakes, oceans, mountains, roads, grass, stones and continuous forest canopies.
- A full scaled ring with closed dark rim walls, moving night bands and sparse procedural cloud coverage; optional high-quality local volumetric clouds, water waves and frozen photo rendering.
- **92 Blender-authored scenery prefabs / 276 LOD meshes**. Thirty-three architectural placements include floating palaces, ruined cities and eight new colossi: an **80 km rim gate**, **120 km causeway** and **48 km floating city plate**. Machinery is scenery, not an operational simulation.

See [release notes](docs/RELEASE-1.0.2.md), [colossi and continuous forests](docs/COLOSSI-AND-FORESTS.md), [landmark designs and placement](docs/LANDMARK-ASSETS.md), [dimensions/triangle inventory](docs/LANDMARK-INVENTORY.md), and the [combined biome catalog](docs/BIOME-ASSET-CATALOG.md).

## Settings and performance

Laptop is the default Ringworld visual preset. KSP's native texture quality, anti-aliasing, shadows and terrain-scatter controls are respected. High/Ultra provide separate Ringworld cloud/water quality; photo mode can render a refined still without requiring a playable high-quality frame rate.

Terrain LOD distance accepts a finite numeric value without the old two-million-km cap. Work and mesh budgets remain bounded, and only the physical ring extent is generated. A distant macro surface supplies the full outline; it is not detailed terrain everywhere. Large landmarks stream separately: the original kit uses 60 km, colossi 250 km plus their extent; small scenery remains within the near-terrain tiles. Initial distant-terrain generation takes time.

Dense forests use merged canopy patches with three LODs and pooled trunk colliders only near loaded vessels. KSP scatter density and Ringworld forest density control their population. See the forest catalog for exact budgets and validation notes for measured performance.

Blank seeds resolve once per new save. Existing saves retain their seed and terrain algorithm. Do not change world dimensions or terrain generation underneath an established expedition. Rendering and weather can be changed independently. See [settings](docs/SETTINGS-AND-HORIZON.md), [high-end visuals](docs/HIGH-END-VISUALS.md), and [weather/night](docs/WEATHER-AND-NIGHT.md).

## Scale

| Property | Default |
|---|---:|
| Radius | 15,300,000 km |
| Ribbon width | 160,500 km |
| Rim-wall height | 160 km |
| Floor acceleration | 9.72 m/sÂ² |
| Rotation period | 249,282.88 s / 69.245 h |
| Surface tangential speed | 385.637 km/s |
| Illumination cycle | 3 hours, configurable adaptation |

The stock Sun remains the native reference body. The ring's influence boundary is annular, not a native spherical SOI. Science subjects identify Ringworld, but the archive may group them under the Sun. See [canon/scale](docs/CANON-AND-SCALE.md) and [residence/encounters](docs/RESIDENCE-AND-ENCOUNTERS.md).

## Validation and limits

The release checks include 90,859 core assertions, real KSP pause-menu save, five damage-enabled warp cycles through 1,000×, stock SAS/parachute gates, flight/EVA/camera tests, powered deployed science, scene reload, asset imports/contact rays and live woodland streaming. Exact logs, fixture assumptions and measurements are in [VALIDATION.md](docs/VALIDATION.md).

This does not certify arbitrary fleets, every mod combination, every aircraft or docking configuration, detailed building interiors, or unloaded atmospheric trajectories. Water buoyancy is approximate; the coast preview ends at atmosphere entry. Read [known limitations](docs/KNOWN-LIMITATIONS.md) and [mod interoperability](docs/MOD-INTEROPERABILITY.md). Back up saves before upgrading. A new DLL cannot reconstruct a craft already destroyed or corrupted in an older version.

## Development and Blender sources

All work stays in this project. The development game is `template_instance`. Editable art is under `art/first-set`, `art/habitat-kit`, `art/city-kit` and `art/landmark-kit`; the latest scene is `art/landmark-kit/Ringworld-Landmarks.blend`. The runtime ZIP contains compiled assets, not Blender or game files.

With a .NET SDK and your KSP 1.12.5 installation:

```powershell
.\build.ps1 -Install -Package
# Another game installation:
.\build.ps1 -KspRoot 'D:\Games\KSP' -Install
.\smoke-test.ps1 -StabilityOnly
.\smoke-test.ps1 -SceneryOnly -LandmarksOnly
.\smoke-test.ps1
python tools/verify_release.py
```

The build runs core checks, compiles .NET Framework 4.7.2 plugins and stages the distributable. Smoke tests use isolated saves and restore the normal plugin afterward. Blender uses `tools/blender_bridge.py` on localhost port 9876; rebuilding the asset bundle requires the Unity editor described in [BLENDER-ASSETS.md](docs/BLENDER-ASSETS.md). Player installations do not need these development tools.

## Uninstall and attribution

Return ring vessels to ordinary spaceflight first. Close KSP and remove `GameData/NivenRingworld`. Craft containing the RW-1 require the mod to load; other mods may still need Harmony. The save's custom state is in `RingworldScenario`.

An unofficial fan project inspired by Larry Niven's *Ringworld*. Original code, descriptions, textures and meshes are covered by [LICENSE](LICENSE); Niven's setting and names are not licensed by that software license. No book passages, cover art or proprietary game assemblies are redistributed.



### 1.0.1 validation update

The user's 38-part, eight-LT-2 craft passed natural-gravity touchdown, paused save, 1000x stock warp and Space Center/reload without a descent controller or crash immunity. Stock suspension now uses ring gravity. A bounded-pose check permits small attached-part joint chatter after one stable physics-second while retaining translation, root-rotation, contact and throttle guards. The stock vertical-speed gauge now uses the local ring normal. The complete flight/EVA/deployed-science regression passed after these changes. See [validation details](docs/VALIDATION.md) and [release notes](docs/RELEASE-1.0.1.md).

### 1.0.2 scenery update

The old square was the close-range tree footprint. Shared biome masks now drive detailed trees, progressively simplified crown clusters and distant canopy colour/height. Forests continue through the selected terrain LOD range. Colossi now use rare seeded cells across the ring, with a 600 km clearance around named landmarks instead of eight fixed landmark attachments. See [the updated catalog](docs/COLOSSI-AND-FORESTS.md).

### 1.0.3 quality presets

The Ringworld settings dropdown now applies eleven rendering presets, from Absolute Cow to Rotten Potato. Biome features has an independent Economy/Low/High/Ultra forest control; Economy uses simpler nearby crown groups and canopy-only distant forests. See [quality settings](docs/QUALITY-PRESETS.md) and [CKAN publishing instructions](docs/CKAN-PUBLISHING.md). This release has not been published to CKAN.
