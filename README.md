# Niven Ringworld Expedition — KSP 1.12.5

An experimental, source-included KSP 1 mod implementing a **1:10 linear-scale, star-encircling ring habitat**. This is an early playable-physics prototype, not a finished planet pack or a complete recreation of the novels.

The development game is `template_instance` beside this file. The included **HarmonyKSP 2.2.1.0** library applies scoped stock-compatibility patches. No Kopernicus, ModuleManager, or downloaded art packs are required. The survey instrument references the thermometer model in your own KSP installation; no game assets are redistributed.

## Play

1. Start `template_instance/KSP_x64.exe`. Create a **Sandbox** save for initial testing.
2. Build and launch a small rocket lander with landing legs, sufficient thrust for 1 g, and the **RW-1 Ringworld Surveyor** from the Science category. Bring a transmitter and electrical power for science transmission. Rockets are supported by stock propulsion; atmospheric aircraft are not yet fully supported.
3. The **Niven Ringworld** flight panel opens automatically. **Left Alt+R** toggles it.
4. To test orbital entry, choose a site and click **Training: set up a spin-matched approach**. This explicitly places the craft at 250 km with a 1 km/s descent and matched tangential motion. Entry then happens automatically near 210 km above the floor datum; air starts at 60 km. Fly and brake the descent. For direct surface exploration, choose **Explorer's landing field** and click **Begin expedition**. This relocates the current craft to 300 m above the ring by default; the height slider allows 60–2,000 m. Control your descent: it begins at rest and falls under approximately 1 g. This is not an automatic landing.
5. Use the panel's **Above ground** and **Ring speed** readouts. The stock navball and altimeter still refer to the Sun and do not describe the ring surface.
6. Land, drive, explore the nearby abandoned city or scrith excavation, and right-click the RW-1 to **Survey Ringworld**. Data can be reviewed, transmitted through a stock antenna, transferred through stock science containers, or recovered after return to a stock body.
7. The destination list provides expedition relocation to distant sites. These jumps are an exploration aid; the distances are genuinely enormous. Ordinary high time warp is held at 1x during an expedition because stock patched conics cannot represent a stationary object on the ring.

**Leaving the ring frame** converts the loaded expedition craft to inertial velocity, including the ring's approximately **386 km/s** surface speed. This is physically well above solar escape speed. It is not a return-to-Kerbin shortcut.

## Included systems

- Double-precision cylindrical coordinates and a floating-origin-aware surface renderer.
- A visible full ring and twenty shadow-square models in scaled space.
- Streamed collision meshes, an inward-facing floor, rim walls, and visual terrain extending to 2,000 km.
- Seeded hills, ridges, mountains, grasslands, forests, deserts, snow, carved river channels, basin lakes, and large oceans.
- Procedural trees, rocks, rural buildings, abandoned city blocks, roof machinery, window belts, causeways, and research plinths.
- Named exploration sites inspired by the books, with original geography and report text.
- Centrifugal and Coriolis terms, stellar gravity, automatic arrival/departure with rotation phase and per-rigidbody velocity conversion, and approximate water buoyancy.
- Dry-air pressure, density, sound speed and native FlightIntegrator drag/lift inputs; stock shock/convection calculations using local ring air.
- An inward-facing atmospheric shell with numerical Rayleigh/Mie single scattering and three procedural cloud decks around 4.8-6.1 km. Cloud decks are translucent geometry, not a weather simulation.
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

The script runs core checks, compiles the plugin for .NET Framework 4.7.2, installs `GameData/NivenRingworld` and the included `GameData/000_Harmony` dependency, and creates `artifacts/NivenRingworld-0.2.0.zip`. It does not copy the proprietary game into the archive. Close the game before rebuilding an installed DLL.

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
