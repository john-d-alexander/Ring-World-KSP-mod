# Niven Ringworld Expedition v1.1.1



## Changes



- Removed the arbitrary 200 million km diameter limit and upper day/weather-period caps. Finite values, geometric relationships and coordinate precision are still validated.

- Airborne or unstable ring residents now explicitly fail stock save/exit checks; stable ground contact remains required.

- Tracking Station displays the ring-aware trajectory. Warp that would skip an encounter is rejected; imminent entry hands the vessel to Flight for real atmospheric/collision physics. Legacy airborne ring snapshots are discarded in favor of the current stock orbit, avoiding stale-position restoration.

- The scaled ring uses camera-relative double-precision placement, and approach terrain is repositioned after floating-origin updates. A dedicated dark-wall shader fixes the large diagonal wall/sky cutouts reproduced during the ascent/re-entry test at normal and enlarged ring sizes.



- Integrated Cyla's compiled cylindrical atmosphere through a flight-camera adapter, with automatic fallback to the original renderer if unavailable.

- Added depth-aware reduced-resolution Cyla rendering and preset-controlled atmosphere/light sampling.

- Slow and below select the original lightweight atmosphere; Mid and above select Cyla. Backend controls remain independently editable.

- Rotten Potato uses the minimum supported graphics controls, including a 200 km terrain horizon. Absolute Cow uses the maximum supported controls and whole-ring terrain coverage. Stock KSP settings and world generation remain separate.

- Translucent ring water now allows the camera below its surface, with five quality levels: flat, ripples, waves, detailed and ultra. Higher levels add procedural surface variation; reflections remain a sky approximation.

- Photo output resolution preserves screen aspect ratio, with long-edge choices through 8K and 16K, subject to GPU texture/memory limits. The world is rendered at the output resolution rather than upscaled.

- Advanced Cyla controls expose optical scattering coefficients/intensities, scale heights, asymmetry, 1–500 view steps, lighting boundaries and proxy geometry/offsets.

- Both photo entry points offer eleven temporary quality presets. Terrain, forest, water and atmosphere follow the selection; Resume/Cancel restore gameplay settings.

- Trajectory calculation uses a bounded frame-time-aware work budget to improve refresh under low rendering FPS without changing numerical integration.

- Atmosphere rendering is suspended during teleport transitions; Cyla uses a camera-owned background capture instead of a shared grab texture.



## Install



Copy all folders in `GameData` into KSP 1.12.5's GameData folder. Cyla and Harmony are included; no separate Cyla installation is required. Avoid installing duplicate copies under different folder names. Existing saves should reselect their desired graphics preset once to adopt the revised values.



## Credits and licensing



Cyla is by **Ghassen Lahmar (LGhassen / blackrack)**: https://github.com/LGhassen/Cyla.

The upstream license identifies GPLv3 plugin code and compiled-only shaders. Original copyright/license text, published plugin source and pinned binary provenance accompany this release. Cyla binaries are unmodified. Niven Ringworld's original MIT notices remain applicable to its original files; the whole archive is not uniformly MIT. Read CREDITS.md and THIRD-PARTY-NOTICES.md.



## Limits



- Cyla uses a 100,000 km optical proxy to avoid float precision failures at the ring's real 15,300,000 km radius. Actual geometry and physics retain the real dimensions. This is a local scattering approximation; **whole-ring distant Cyla scattering is not implemented**. Map/global ring visuals retain their existing renderer.

- A 2880x1920 live-physics camera sweep with clouds enabled measured **16.8 FPS** on this laptop at Rotten Potato. This release does not promise 30 FPS. Terrain, trees, clouds, resolution and physics all affect frame time.

- Higher photo presets can require several minutes of terrain preparation on a laptop; Cancel restores gameplay.

- The exact reported blue-dot sky/portrait glitch was not conclusively reproduced. Precision/camera hazards were addressed, but a universal fix is not claimed.



- Ring water is not automatically discovered by Scatterer: Scatterer requires a celestial-body ocean adapter, not a mesh tag. Existing ring buoyancy is approximate; the public surface API exposes water state for opt-in integrations.

- 8K/16K captures have not been rendered on this laptop; large outputs are intended for GPUs with sufficient memory.



## Validation



108,655 core assertions and a clean plugin build. Earlier in-game Cyla checks passed daylight visibility, foreground depth, camera isolation, map switching, eleven preset round-trips and endpoint checks, low photo capture, high photo cancellation/completion, and gameplay restoration. Current Slow-only checks cover the expanded settings and aspect-preserving photo output. Higher revised optical budgets and 8K/16K are not laptop-tested. See VALIDATION.md for dated fixtures and limitations.



## Camera transitions



Entering or leaving the ring-relative frame now blends the external camera view over two real-time seconds. The ship's physical coordinate conversion is immediate. The blend tracks the vessel and current stock camera target, restores stock state before each update, and leaves terrain/wall camera clearance active. Explicit relocation and scene changes clear the transition.

