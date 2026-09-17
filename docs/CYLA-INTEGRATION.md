# Cyla integration — v1.1.1

Cyla by **Ghassen Lahmar (LGhassen / blackrack)**: https://github.com/LGhassen/Cyla.

## Packaging and licensing

`vendor/Cyla` pins upstream 1.1.0.0, commit 92223648e0674212e488e6fb977b5cffc63be869. Releases include unmodified GameData/Cyla binaries, original License.md, matching published plugin source and provenance under ThirdParty/Cyla. The GPLv3 plugin and compiled-only shaders are not relicensed as MIT. Original Ringworld adapter code calls the public shader interface without embedding upstream C# or reconstructing shader source. See CREDITS.md and THIRD-PARTY-NOTICES.md. CKAN dependency ownership/license metadata still need review before indexing.

## Rendering

The adapter loads Cyla/Atmo from ShaderLoader and renders only on the main Flight camera. It detaches for map, relocation and scene transitions. Missing/unsupported Cyla falls back to Original. Its camera-owned background replaces reliance on the shader's named shared grab. Full resolution evaluates Cyla once; reduced resolution evaluates black/white backgrounds and recovers scattering/transmittance with depth-aware upsampling to preserve foreground silhouettes. This black-box adaptation can differ around fine silhouettes.

A local optical cylinder preserves observer altitude and across position. Its default radius is limited to 100,000 km: diagnostic captures became blocky at 10,000,000 km and failed at the actual 15,300,000 km radius. This is a local approximation, not full physical ring curvature. **Whole-ring distant Cyla scattering is not implemented at any preset.** The 600 km observer altitude activation boundary is not an along-ring render distance. Global/map ring visuals retain their existing renderer. Physical geometry and forces remain independent.

Default Rayleigh coefficients are (5.8,13.5,33.1)e-6/m, Mie 12e-6/m, scale heights 8.5/1.2 km, asymmetry 0.76 and optical thickness 60 km. Haze/exposure and moving shadow-square daylight modulate scattering. Weather/clouds remain Ringworld systems; Original air scattering is suppressed while Cyla is active.

## Controls and presets

The save-specific settings menu exposes RGB scattering coefficients, separate intensities, scale heights in metres, Mie asymmetry (capped at 0.99 to avoid the singular endpoint), top/side/floor/unlit lighting boundaries, view steps 1–500, light steps 1–50, dithering, and 1/8, 1/4, 1/2 or full render dimensions. At 1/4, an optical target has 1/16 the pixels. Photo capture disables temporal dithering.

Outer radius is the precision-limited optical proxy radius; inner radius is outer radius minus thickness. Transparent radius is outer radius minus thickness times the boundary fraction (at least 32 m separation to keep the two float radii distinct). Optical width follows physical ring width times its multiplier. Along/across/inward offsets and pitch/yaw are advanced visual controls; nonzero values deliberately misalign the proxy. Physical ring size remains in world settings. These controls do not alter atmospheric forces.

Slow and below choose Original and skip GPU cloud raymarching. Mid and above choose Cyla. View counts from Absolute Cow through Rotten Potato are 500,192,128,80,48,32,24,16,8,4,1; the low five leave Cyla disabled. Quality presets set work budgets, not atmospheric colours or physical geometry. Original atmosphere steps remain independently limited to 16–96.

## Photo capture

Both entry points offer eleven temporary presets and a separate output size through 16K, preserving screen aspect ratio. The stock world camera stack renders at the output resolution; it is not a resized viewport image. GPU texture-size and conservative memory checks reject impossible outputs. Terrain/forest preparation completes before capture. Cyla is included in the frozen scene; Ringworld volumetrics then accumulate in screen tiles. Resume/Cancel restore gameplay settings, time and controls. High settings/resolutions can take minutes and are not intended for this laptop.

## Historical validation and limits

September 16 diagnostic runs initially failed visual review (black sky, stretched quad); these are not evidence of a working result. Later 185846, 190633 and 194949 runs passed visible daylight, magenta foreground depth, camera isolation, map switching, backend lifecycle and photo restoration. Tests were performed before the user's later restriction to Slow-or-lower live rendering.

190633: Adreno X1-85 at 2880x1920, frozen physics/clouds disabled, Original 22.3 FPS; Cyla 16/1 eighth-resolution 21.8 FPS, quarter 19.8; 32/4 half 12.1; 64/8 full 4.6. These isolate shader costs, not complete presets. 194949: revised Rotten Potato with live physics, clouds and camera motion measured 16.8 FPS. A 30 FPS gameplay target has not been demonstrated. The blue-dot/portrait corruption screenshot's exact cause remains unproven; no universal hardware or portrait fix is claimed.

See VALIDATION.md for current regression logs and RELEASE-1.1.1.md for release scope.
