# Niven Ringworld Expedition — KSP 1.12.5

## First Blender scenery set

Eight original low-poly assets now replace the local broadleaf/conifer, boulder and rural-building placeholders. Each has three LOD meshes and simple collision proxies; all share a 512px atlas. Editable sources and previews are in `art/first-set/`. See the [asset guide and rebuild instructions](docs/BLENDER-ASSETS.md). This first set is stylized; detailed Ringworld structures and distant forest/city impostors remain future work.

## Version 0.9 weather and night update

Continuous UT-driven cloud drift replaces timed cloud rebuilds. Shared weather can evolve from fair skies to visual rain/thunderstorms, with separate controls and quality-scaled precipitation. Daytime starfield suppression preserves celestial renderers. Moving analytical night bands apply to the whole ring, including the low-detail setting and scaled terrain. See [weather, warp and night visibility](docs/WEATHER-AND-NIGHT.md).

## Version 0.8 visual update

Optional High/Ultra GPU volumetric clouds and atmospheric scattering, independent Ringworld water quality with animated waves, and a frozen tiled photo renderer. A full-ring surface detail option adds seeded distant land/ocean/cloud colour beyond the terrain horizon. Photo mode waits for finer terrain LOD and restores normal settings on resume. Laptop remains the default. See [high-end visuals and photo mode](docs/HIGH-END-VISUALS.md).

## Version 0.7 development update

Stock warp controls now support resting ring expeditions through native rails anchoring. Terrain has explicit friction, ground-contact craft share EVA camera-shake suppression, and re-entry effects use ring-relative airflow. Coarse hull geometry is moved below the terrain, walls have closed thickness, and LOD blocks publish progressively. See [stock warp and rendering](docs/STOCK-WARP-AND-RENDERING.md) and the [combined biome/asset catalog](docs/BIOME-ASSET-CATALOG.md).

## Version 0.6 development update

Smooth climate-blended biomes for new worlds, shallow ordinary relief, batched grass/shore stones/litter, visual sunflower patches and local dust/pollen. Graphics follow KSP's native settings. The laptop preset now includes 75 m ground detail. A random procedural-site button transfers the craft with ring-matched velocity for rendering inspection.

Read [biomes, graphics and random visits](docs/BIOMES-AND-GRAPHICS.md), the [Blender asset tracker](docs/ASSET-TRACKER.md), and the existing [physics/warp contract](docs/ORBITS-WARP-AND-ASSETS.md). Older saves retain their terrain generator; use a new Sandbox save for v4 terrain.

An experimental, source-included KSP 1 mod implementing a **1:10 linear-scale, star-encircling ring habitat**. This is an early playable-physics prototype, not a finished planet pack or a complete recreation of the novels.

The development game is `template_instance` beside this file. The included **HarmonyKSP 2.2.1.0** library applies scoped stock-compatibility patches. No Kopernicus, ModuleManager, or downloaded art packs are required. The survey instrument references the thermometer model in your own KSP installation; no game assets are redistributed.

## Play

1. Start `template_instance/KSP_x64.exe`. Create a **Sandbox** save for initial testing.
2. Build and launch a small rocket lander with landing legs, sufficient thrust for 1 g, and the **RW-1 Ringworld Surveyor** from the Science category. Bring a transmitter and electrical power for science transmission. Rockets are supported by stock propulsion; atmospheric aircraft are not yet fully supported.
3. The **Niven Ringworld** flight panel opens automatically. **Left Alt+R** toggles it.
4. To test orbital entry, choose a site and click **Training: set up a spin-matched approach**. This explicitly places the craft at 250 km with a 1 km/s descent and matched tangential motion. Entry then happens automatically near 210 km above the floor datum; air starts at 60 km. Fly and brake the descent. For direct surface exploration, choose **Explorer's landing field** and click **Begin expedition**. This relocates the current craft to 300 m above the ring by default; the height slider allows 60–2,000 m. Control your descent: it begins at rest and falls under approximately 1 g. This is not an automatic landing.
5. Use the panel's **Above ground** and **Ring speed** readouts. During an active expedition, the stock altimeter reports ring ground/water clearance and speed cues use the ring-relative frame.
6. Land, drive, explore the nearby abandoned city or scrith excavation, and right-click the RW-1 to **Survey Ringworld**. Data can be reviewed, transmitted through a stock antenna, transferred through stock science containers, or recovered after return to a stock body.
7. The destination list provides expedition relocation to distant sites. These jumps are an exploration aid; the distances are genuinely enormous. Use stock high time warp after settling on dry terrain. Ring craft use a custom surface anchor while KSP advances the universal clock; airborne ring flight remains at 1x.

**Leaving the ring frame** converts the loaded expedition craft to inertial velocity, including the ring's approximately **386 km/s** surface speed. This is physically well above solar escape speed. It is not a return-to-Kerbin shortcut.

## Clear sky and save settings (v0.4.0)

The Ringworld panel now has **Expedition / Settings** tabs (Alt+R shows the panel). Clear air transmits the ring and Sun: the previous nearly opaque daytime sky overlay has been removed. Cloud amount, moving weather fronts, haze and shadow-square day length are adjustable.

Terrain horizon distance reaches **160,000 km**, with 8/16/32-segment quality levels and a laptop preset. Coarse distant blocks use scaled space; nearby ground retains collision detail. This is not 160,000 km of detailed ground or buildings. New worlds use warped gradient noise and spatial colour textures. Blank configured seeds generate once per new save and are then persisted. Old expeditions preserve their original seed and terrain algorithm.

Read [settings and horizon details](docs/SETTINGS-AND-HORIZON.md) for controls, migration and limits. World-generation controls lock after the first expedition; rendering and weather remain adjustable. Laptop frame rates have not been certified.

## Ground and EVA fixes

The original 0.2.0 floor was a zero-thickness collision sheet. Loaded terrain tiles now have a closed collision shell, rendered sides and an underside. The default underside lies 1,300 m below the height datum, including 100 m beneath the deepest modeled ocean floor. `structuralThickness` controls that minimum foundation; it is an implementation setting, not an asserted book measurement. Collision coverage remains local to the streamed tiles.

The camera checks both collider obstructions and the height of its near clipping plane above the terrain triangles. EVA spawning inherits the parent craft's rotating frame before the active-vessel switch. Frame ownership persists for the whole loaded scene, so entering EVA cannot subtract the ring's spin velocity a second time. Kerbal ground detection, movement orientation and ragdoll gravity now use the ring surface.

Restart KSP after installing an updated DLL. To retry an EVA that already suffered the old velocity fault, load a save from before that incident; the patch does not rewrite a craft's already-corrupted trajectory.

## Terrain and flight refinement (v0.3.0)

Adaptive terrain LOD replaces the sparse distant backdrop with nested blocks out to about 2,000 km. Seeded mountain ranges, foothills, dunes, variable-width rivers and lakes, road corridors, forest clustering and irregular rural buildings add more regional variation. Distant geometry takes several seconds to build after relocation. The same height function drives distant terrain and physical ground.

Camera clearance now includes water. EVA chase orientation uses the ring frame, and the stock flight altimeter shows clearance above ring ground/water without changing orbital altitude. Clouds use an advected procedural opacity texture over uneven layers; twenty shadow squares drive the configured three-hour light cycle. See [terrain and LOD notes](docs/TERRAIN-LOD.md) for design, source references and remaining visual limitations.

Terrain away from named-site pads changes in this version. An old wilderness surface save may need relocation above a destination to avoid intersecting the new terrain. EVA walking transitions now use local horizontal velocity instead of the stock Sun-relative speed cache. Ring navball markers and speed use the same local physics frame; see [validation](docs/VALIDATION.md) for real-key and rapid-reversal test results.

## Included systems

- Double-precision cylindrical coordinates and a floating-origin-aware surface renderer.
- A visible full ring and twenty shadow-square models in scaled space.
- Streamed collision meshes, an inward-facing floor, rim walls, and visual terrain extending to 2,000 km.
- Seeded hills, ridges, mountains, grasslands, forests, deserts, snow, carved river channels, basin lakes, and large oceans.
- Procedural trees, rocks, rural buildings, abandoned city blocks, roof machinery, window belts, causeways, and research plinths.
- Named exploration sites inspired by the books, with original geography and report text.
- Centrifugal and Coriolis terms, stellar gravity, automatic arrival/departure with rotation phase and per-rigidbody velocity conversion, and approximate water buoyancy.
- Dry-air pressure, density, sound speed and native FlightIntegrator drag/lift inputs; stock shock/convection calculations using local ring air.
- An inward-facing atmospheric shell with numerical Rayleigh/Mie single scattering and three textured, uneven procedural cloud decks around 4.3-7 km. Cloud decks are translucent geometry, not a weather simulation.
- Saved ring positions, velocities, orientations, and discoveries; science data uses stock `ScienceData` and subject diminishing returns.

For an ordinary solar trajectory, fly into the ribbon near its 15.3-billion-metre radius. No speed is gifted on entry: an unmatched solar orbit encounters roughly 386 km/s of relative air motion. The ring can be approached from the central opening or above a rim. Entry occurs within 50 km of a rim edge and from -50 km to 210 km relative to the floor datum; departure uses wider boundaries to avoid repeated switching. Warp is reduced when a linear look-ahead predicts the approach. Extreme warp steps and untested multi-vessel missions remain experimental.

Read [known limitations](docs/KNOWN-LIMITATIONS.md) before treating an expedition as a long-running save. In particular, stock EVA orientation, aircraft aerodynamics, background vessels, precision during frame transitions, and stock landing/recovery semantics require further validation and integration.

## Scale

| Property | Default |
|---|---:|
| Radius | 15,300,000 km |
| Ribbon width | 160,500 km |
| Rim-wall height | 160 km |
| Centrifugal acceleration at floor datum | 9.72 m/s² |
| Rotation period, derived from radius and acceleration | 249,282.88 s / 69.245 h |
| Surface tangential velocity | 385.637 km/s |
| Illumination cycle | 3 h, configurable gameplay adaptation |

This is a **one-tenth linear scale**, not a promise that every stellar property and atmospheric dimension is also one-tenth. The ring surrounds the stock Sun. The published dimensions, adaptations, and bibliography are recorded in [canon and scale](docs/CANON-AND-SCALE.md).

## Build and install

Requires a .NET SDK (the project was developed with .NET 10) and your installed KSP 1.12.5 assemblies.

```powershell
.\build.ps1 -Install
# Or use another KSP copy:
.\build.ps1 -KspRoot 'D:\Games\KSP' -Install
```

The script runs core checks, compiles the plugin for .NET Framework 4.7.2, installs `GameData/NivenRingworld` and the included `GameData/000_Harmony` dependency, and creates `artifacts/NivenRingworld-0.3.0.zip`. It does not copy the proprietary game into the archive. Close the game before rebuilding an installed DLL.

```powershell
dotnet run --project tests/Ringworld.Tests -c Release -- artifacts/preview
```

This also exports an SVG terrain plan from the **actual C# terrain generator**. It is a diagnostic map, not an in-game screenshot.

The game-level regression passed automatic orbital capture, inertial departure within 0.004 m/s of the expected velocity, native atmospheric values, a 49-part surface landing, science serialization, and saved-coordinate restoration. See [orbital arrival and atmosphere](docs/ORBITAL-ARRIVAL.md) for the frame model and training controls. See [validation details](docs/VALIDATION.md), including the damage-immune test fixture and remaining limitations. `smoke-test.ps1` reruns this test in the isolated instance and restores the normal build afterward.

## Configuration and source

`GameData/NivenRingworld/Settings.cfg` controls dimensions, seed, atmosphere, daylight period, and tile settings. Default collision coverage is a 7×7 grid of 1,024 m tiles, each subdivided 32×32. Do not change geometry or seed in a save containing ring expeditions; the coordinate save format does not migrate worlds.

`src/Ringworld.Core` contains the portable geometry and terrain implementation. `src/Ringworld.KSP` connects it to Unity/KSP. `tests/Ringworld.Tests` checks the mathematical invariants. `tools/ApiInspect` reads assembly metadata without loading KSP into the .NET runtime.

## Uninstall

Return expedition vessels to ordinary spaceflight first. Close KSP, then remove only `GameData/NivenRingworld`. Craft containing the RW-1 will require the mod to load. Scenario data uses the `RingworldScenario` node in your save.

## Attribution

An unofficial fan project inspired by Larry Niven's *Ringworld*. Niven's names, fictional setting, and book text are not licensed by this project's software license. Original code and newly authored descriptions are included; no book passages, maps, cover art, or models are copied. See [LICENSE](LICENSE).


The stock ring toolbar button (or Alt+R) opens flight information; Sandbox additionally exposes testing transports and world settings. See [mod interoperability and the surface API](docs/MOD-INTEROPERABILITY.md), [global clouds](docs/GLOBAL-CLOUDS.md), and the [habitat/city asset inventory](docs/HABITAT-LIBRARY.md). Native Blender city sources and their preview are in `art/city-kit`.
