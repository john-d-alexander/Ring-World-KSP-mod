# Ringworld high-end visuals and photo mode — 0.9

## Current controls (1.0.3)

The old Laptop/High/Ultra atmosphere buttons are now labelled Simple/Half-resolution/Full-resolution. Water Laptop is now Simple. Use the new overall quality preset dropdown to apply coordinated settings; see [QUALITY-PRESETS.md](QUALITY-PRESETS.md). The historical implementation description below retains the old quality names.

## Using it

Open the Ringworld panel → Settings. Atmosphere quality has Laptop, High and Ultra modes, independently of stock planet settings. Apply settings to persist your choices with this save. Laptop remains the default. High uses a half-width/half-height optical buffer; Ultra uses full viewport resolution. Both integrate three-dimensional cloud density on the GPU, with direct-light self-shadowing, soft density edges and per-pixel Rayleigh/Mie atmosphere. High defaults to 64 cloud and 32 atmosphere samples; Ultra defaults to 128/64. Individual sample counts, cloud distance (30–500 km), self-shadow strength and atmosphere brightness are adjustable. Weather settings and their new evolving state are described in [WEATHER-AND-NIGHT.md](WEATHER-AND-NIGHT.md).

Water has Laptop, Reflective and Waves modes. Reflective adds Fresnel sky reflection, sun glints and small animated normal ripples. Waves additionally displace local water vertices, with adjustable amplitude up to 2 m and a finer ripple layer. Waves fade near the shoreline, where a procedural foam approximation appears. Sky reflection is analytical; this renderer does not reflect nearby buildings, craft or terrain. Nearby non-scaled LOD water uses the same water material, with geometric wave displacement fading over distance. The far scaled water remains coarse. Water forces and buoyancy still use the existing mean water level: these are visual waves, not a fluid simulation. Camera clearance includes the configured maximum crest height.

Frame the camera in ordinary flight, at 1x, then press **Photo mode: high-quality still** in Expedition or Settings. The current view is frozen, stock HUD hidden, and input locked. The renderer temporarily uses 32-subdivision distant terrain and a one-chunk-per-frame generation budget at your existing horizon distance. It waits for those chunks before capturing the scene. Photo mode enables wave water and uses 192 cloud, 80 atmosphere and 8 light samples per ray at full viewport resolution. One of sixteen image tiles is refined per frame, over the selected 1–64 accumulation samples (16 by default). The preview fills progressively. This spreads expensive shading over time, rather than requiring high-end real-time frame rates. It still renders frames and uses GPU memory; it is not an external offline path tracer.

The PNG and a small metadata text file are saved to `Screenshots/Ringworld` inside the game folder. Image size is the current viewport resolution; accumulation refines the volumetric integration and does not supersample the spacecraft or synthesize extra terrain detail. **Resume flight** or **Cancel photo** restores time scale, target frame rate, HUD visibility, the input lock, normal terrain LOD/budget, and the normal visual modes. Simulation time, weather and wave phase remain frozen during the capture. Use a higher native game resolution before entering photo mode if you want a larger image.

KSP texture quality, anti-aliasing, ordinary shadow settings and scatter enable/density remain inherited. There is no stock volumetric-cloud quality knob to reuse. Ringworld does not change the global render pipeline, install a global post-processing profile or replace stock planet materials. Photo mode preserves those global quality choices too.

## Renderer boundaries

The new image effect belongs to the local flight camera and only activates near the ring. Ring-local, camera-relative coordinates avoid subtracting large star-centred floats inside the shader. Atmospheric/cloud sampling accounts for ring curvature in a finite tangent region and clips against camera depth and ribbon width. It is not a global volumetric rendering of the full circumference. Existing scaled geometry provides the distant ring outline.

Clouds use an original tileable 3-D value/cellular noise texture and continuous seeded coverage field. Direct-light extinction is raymarched; ambient fill approximates multiple scattering. There is no full multiple-scattering solution or cloud shadow map projected onto terrain. Version 0.9 adds seeded visual weather, rain and lightning; it is not a fluid/weather simulation. Real-time High uses depth-aware spatial upsampling, not motion-vector temporal reprojection. Fine noise can remain visible at low sample counts. Photo accumulation is valid because the camera, depth, scene and simulation time are frozen.

The shader bundle targets Windows Direct3D 11 / Unity 2019.4.18f1. Other graphics APIs and third-party camera stacks are not certified. Unsupported/missing atmosphere assets report an error and retain the laptop sky. The shader project and noise generator live in `tools/VisualShaders`; no third-party shader or paid art assets are embedded.

## Research and integration decisions

These were researched as visual references, not automatically installed dependencies. Compatibility of an entire third-party mod stack has not been tested.

| Reference | Relevant contribution | Ringworld decision |
|---|---|---|
| [Deferred](https://github.com/LGhassen/Deferred) | Deferred lighting and reflection improvements; its compatibility table distinguishes forward-rendered materials | Keep a local image effect rather than globally switching rendering paths. A future Deferred adapter needs its own test matrix. |
| [EVE Redux](https://github.com/LGhassen/EnvironmentalVisualEnhancements) and [volumetric quality documentation](https://github.com/LGhassen/EnvironmentalVisualEnhancements/wiki/Temporal-upscaling-and-noise-detiling) | Cloud rendering; temporal reconstruction amortizes work and handles camera motion through reprojection | Original ring-shaped cloud field and GPU density integration. Frozen tiled accumulation in photo mode; no claim to reproduce Blackrack Volumetric Clouds V5 or its paid assets. |
| [Scatterer](https://github.com/LGhassen/scatterer) | Atmospheric scattering; public repository distinguishes plugin and newer shader distribution | Original bounded ring-atmosphere integrator; no copied planet shader or assumed spherical atmosphere. |
| [Parallax Continued](https://github.com/Gameslinx/Parallax-Continued) | Detailed terrain/scatter and separate planet/terrain texture assets | Photo mode raises our existing LOD. Future authored terrain materials/assets remain separate work; Parallax assets are not bundled. |
| [Firefly](https://github.com/M1rageDev/Firefly) | Replaces stock aerodynamic VFX, with its own API/configuration | Existing ring-relative stock effects remain. Firefly would need a ring-aware adapter, not merely planet configuration. |
| [ZTheme](https://github.com/zapSNH/ZTheme/releases) / [HUDReplacer](https://github.com/UltraJohn/HUDReplacer) | HUD presentation | Hide and restore the actual stock HUD during photos; do not replace the user's theme. |
| [PlanetShine](https://github.com/PapaJoesSoup/ksp-planetshine/releases) | Reflected environmental light, including EVA | Existing ring lighting retained. Optional ambient/bounce integration is future work. |
| [ReStock](https://github.com/PorktoberRevolution/ReStocked) | Spacecraft art revamp | Craft asset replacement is independent of ring rendering; no parts overwritten. |
| [Restock Waterfall Expansion](https://spacedock.info/mod/3149/Restock%20Waterfall%20Expansion) | Engine effect configurations for ReStock/ReStock+ | Engine plume assets/configuration remain owned by that mod; not confused with ring water rendering. |
| [Shabby](https://github.com/KSPModdingLibs/Shabby) | Shader asset-bundle loading | This small renderer loads its own named bundle; no global shader replacement or additional loader dependency is needed. |
| [TUFX](https://github.com/shadowmage45/TUFX) | Post-processing profiles, including bloom, grading and depth of field | Preserve user profiles. Ring atmosphere brightness is local; no global bloom/colour-grade override and no bundled Blackrack profile. |
| [VaporCones](https://spacedock.info/mod/3805/VaporCones) | High-speed condensation effects | Separate aerodynamic effect; not a cloud-system substitute. Ring-aware velocity/density compatibility needs dedicated integration. |

## Building the assets

Run `build-visuals.ps1` with Unity 2019.4.18f1 installed. It generates the original volume noise asset, compiles the atmosphere and wave shaders, and writes `GameData/NivenRingworld/Assets/ringworldvisuals`. Ordinary `build.ps1 -Install` uses the shipped bundle and does not need Unity. The bundle is approximately 1.2 MB; photo render targets scale with viewport size, so large resolutions require substantially more VRAM.


## Clear sky and weather

The original renderer already has moving cloud-front coverage, controlled by **Moving weather fronts**. Version 0.9 adds visual rain and lightning; it does not simulate wind forces or precipitation accumulation. All quality tiers share cloud amount and time-dependent coverage. High/Ultra changes how that cloud field is shaded. **Cloud amount = 0** explicitly disables cloud density and gives a clear-sky comparison; it does not disable the atmosphere. At low coverage, individual clouds may locally obscure the ring, but a clear sky should transmit the ring outline and stellar light. The first visual test intentionally used 75% coverage with moving fronts off, which produced an overcast photo. That test was not evidence of a permanently opaque sky.

## Full-ring views

**Full-ring surface detail** adds a procedural colour shader to the interior of the global ribbon. It has no terrain-distance cutoff: it covers the entire circumference (about 96 million km at the default radius), including the visible upward arc. Enable it separately from atmosphere quality; the Workstation preset enables it, the Laptop preset disables it, and Photo mode enables it temporarily. The plain global ring remains visible with the setting off. The added layer uses the existing 8,192-segment ribbon, not millions of additional chunks.

The distant colour map uses the terrain seed, its gradient hash and climate palette, and the two Great Ocean basin centres. These are coarse representations: coast shapes, minor waterways, the height field and small islands are not exact replicas of local terrain. High-frequency climate noise is filtered when smaller than a pixel. Distant clouds are flat weather colour, not volumetric clouds across the full circumference. They follow cloud amount; moving fronts animate their phase. Twenty day/night bands use the same UT-driven square cadence as local daylight, including when full-ring detail is off. This shader does not add distant buildings or tree geometry. Those require dedicated simplified assets in the later Blender pass.

Detailed terrain remains governed by **Terrain horizon distance**, with no fixed upper setting cap, and its subdivision setting. The global colour layer fills the view beyond that range. Existing floor burial and closed rim-wall geometry are retained so the coarse ribbon does not pass through the camera or fight with local ground.

The supplied painting is an artistic composition. At the default physical dimensions, the width is approximately 1% of the radius: the arc narrows quickly in a wide-angle camera. More rendering distance cannot make that physically narrow arc as broad as the painting. Camera framing, atmospheric visibility and the chosen ring dimensions all affect its appearance. Clear sky removes cloud obstruction; atmospheric haze and foreground geometry can still obscure low-angle parts of the arc. This is a first distant-surface pass, not a reproduction of the painting.
