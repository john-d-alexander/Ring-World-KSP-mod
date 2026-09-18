# [1.12.5] Niven Ringworld Expedition — A landable Ringworld in KSP [v1.1.3]

What if your next destination wasn't a planet, but the inside of a ring wrapped around a star?

**Niven Ringworld Expedition** is an unofficial fan mod inspired by Larry Niven's *Ringworld*. It adds a rotating, landable habitat with procedural landscapes, an atmosphere, moving night bands, forests, ruins and enormous structures to explore. You can descend to its interior, land, take a Kerbal outside, collect science and establish a persistent expedition.

The default ring uses one-tenth of the published linear dimensions: **30.6 million kilometres across, 160,500 kilometres wide, with 160-kilometre rim walls**. Its rotation produces approximately Earth-like surface gravity. The geography and architecture are interpretations of the setting, with original locations alongside book-inspired landmarks.

## Downloads and source

- **[Download on SpaceDock](https://spacedock.info/mod/4573/Niven%27s%20Ring%20World)**
- **[Latest release on GitHub](https://github.com/theplatecrafter/Ring-World-KSP-mod/releases/latest)**
- **[Source code and documentation](https://github.com/theplatecrafter/Ring-World-KSP-mod)**
- **[Report a bug](https://github.com/theplatecrafter/Ring-World-KSP-mod/issues)**

**Current version: v1.1.3.** Supported target: **KSP 1.12.5, Windows x64, Direct3D 11**.

## What's on the ring?

### A landscape to explore

Seeded terrain combines smoothly blended biomes with rivers, lakes, oceans, mountain ranges, deserts, roads and forests. Grass, stones and other ground details fill the nearby landscape, while progressively simplified terrain and forest canopies extend into the distance.

There are **92 Blender-authored scenery prefabs with 276 LOD meshes**, including vegetation, settlements, ruins, industrial scenery and landmarks. Rare colossal structures are scattered across the ring: an 80-kilometre rim gate, a 120-kilometre causeway and a 48-kilometre floating city plate are among the designs. These are exterior scenery and exploration destinations; they do not have fully simulated machinery or inhabited interiors.

### Flight on a rotating world

The mod uses a ring-relative frame with centrifugal and Coriolis effects. Navball cues, SAS direction targets, altitude and vertical-speed instruments account for the local ring surface. A numerical trajectory preview helps show encounters with the ring's influence region.

Ordinary arrivals preserve your velocity. The default surface moves at approximately **386 km/s**, so approaching from a solar orbit without matching its motion is a very different problem from landing on Kerbin! Sandbox includes a spin-matched approach and relocation tools for anyone who wants to start exploring immediately.

Once settled on dry ground, you can save, leave your expedition there and use **KSP's stock time warp**. Ringworld science subjects, the RW-1 Ringworld Surveyor and deployed-science anchoring support longer visits. Breaking Ground is needed only for that DLC's deployable equipment.

### Atmosphere, weather and night

Cloud coverage and moving shadow-square night bands are visible across the ring, including from space and the map. Local weather adds changing cloud conditions, with rain and lightning effects. Higher settings offer local volumetric clouds and the optional, separately installed **Cyla atmosphere**, alongside translucent water, procedural ripples and optional waves.

Photo mode freezes the flight and lets you choose a separate quality preset and an aspect-preserving output resolution, up to **8K or 16K where GPU limits allow**. Your gameplay settings are restored afterward.

## Installation

No Kopernicus, Blender or separate art download is required. Cyla and HarmonyKSP are included in the release.

1. Close KSP and download the release ZIP.
2. **Extract it into your KSP instance folder, beside `KSP_x64.exe`.** The ZIP's `GameData` folder should merge with the existing one.
3. Check that the resulting layout looks like this:

```text
Your KSP folder/
├── KSP_x64.exe
└── GameData/
    ├── NivenRingworld/
    ├── Cyla/
    └── 000_Harmony/
```

Alternatively, open the ZIP and copy **the contents of its `GameData` folder** into your existing `GameData` folder.

Do not extract the whole archive inside `GameData`, or leave it nested inside a version-numbered folder. Keep one compatible installation of each dependency if another mod already supplies it. Back up your saves before updating.

## Your first visit

For a quick introduction, start a **Sandbox** save and launch a lander capable of landing in approximately 1 g. Open the Ringworld panel using the stock toolbar's **ring icon**, or **Left Alt+R**.

Choose a destination and relocate above it, try a random terrain location, or use the spin-matched training approach. Relocation places you above the surface; **you still need to brake and land**.

After settling on dry ground, save and return through the ordinary Space Center controls. **Revert Flight discards your progress.** Science and Career retain the information panel without the Sandbox relocation tools.

## Settings and performance

The eleven quality presets range from **Rotten Potato** through **Slow**, **Mid** and **Beefy**, all the way to **Absolute Cow**. New saves start at **Slow**. Forest detail, terrain distance, water and atmospheric settings can also be adjusted independently.

- **Slow and below** use the lightweight Original atmosphere.
- **Mid and above** use Cyla by default.
- **Rotten Potato** prioritizes minimum visual cost, including a shorter terrain horizon.
- Higher presets and large photo captures are intended for stronger hardware.

Forests use combined canopy meshes and multiple LOD levels to reduce their cost. Distant terrain streams in over time; the complete ring outline does not require detailed terrain everywhere. KSP's own texture quality, antialiasing and other graphics settings still matter. Performance depends on your hardware, craft and settings; there is no fixed FPS guarantee.

## Current limitations

This is an unusual environment for KSP, and there are still edges to polish:

- The ring is a custom rotating environment, not a native spherical celestial body. The Sun remains the underlying reference body, and the science archive may group Ringworld subjects under it.
- Airborne and physics time warp inside the ring frame are not supported. The vacuum trajectory preview stops at atmosphere entry.
- Detailed terrain and collision geometry are local; distant views use simplified representations. Water buoyancy is approximate.
- Cyla currently supplies a **local atmospheric approximation**, not whole-ring scattering. Distant atmosphere visuals use the existing Ringworld renderer.
- Scatterer does not automatically recognize the ring's water. Every combination of other mods, craft and flight configurations has not been validated.

## Credits and licensing

Inspired by **Larry Niven's *Ringworld***. This is an unofficial fan project, not an endorsed adaptation.

Atmosphere technology comes from **[Cyla by Ghassen Lahmar — LGhassen / blackrack](https://github.com/LGhassen/Cyla)**. The release includes its unmodified upstream binaries, original license notice and published plugin source. Cyla's plugin code is GPLv3; its shaders are supplied as compiled binaries with unpublished source, and are not relicensed by this project.

Original mod code and assets use the MIT license. Harmony retains its MIT license. The complete distribution includes components with their own terms: see the [project license](https://github.com/theplatecrafter/Ring-World-KSP-mod/blob/main/LICENSE), [credits](https://github.com/theplatecrafter/Ring-World-KSP-mod/blob/main/CREDITS.md) and [third-party notices](https://github.com/theplatecrafter/Ring-World-KSP-mod/blob/main/THIRD-PARTY-NOTICES.md).

## Feedback welcome

I'd love to see your expeditions, unusual discoveries and ambitious landers. Suggestions for biomes, exploration sites and future structures are welcome too.

If something breaks, please include the mod version, quality preset, atmosphere backend, other installed mods and steps to reproduce it. A relevant `KSP.log` and a craft file or save, where practical, make investigation much easier. You can reply here or [open a GitHub issue](https://github.com/theplatecrafter/Ring-World-KSP-mod/issues).

## Dependencies (v1.1.3 onward)

Install [HarmonyKSP / Harmony 2](https://github.com/KSPModdingLibs/HarmonyKSP/releases) separately. [Cyla](https://github.com/LGhassen/Cyla/releases) is optional; without it the built-in Original atmosphere is used. Neither mod is bundled in the ZIP. CKAN metadata is prepared, but listing still awaits maintainer acceptance.
