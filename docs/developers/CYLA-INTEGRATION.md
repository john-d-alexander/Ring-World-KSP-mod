# Cyla integration

Cyla by **Ghassen Lahmar (LGhassen / blackrack)**: https://github.com/LGhassen/Cyla.

## Packaging and licensing

`vendor/Cyla` retains the development reference for upstream 1.1.0.0, commit 92223648e0674212e488e6fb977b5cffc63be869. Since v1.1.3, release ZIPs do not contain Cyla binaries, shaders or source. Install Cyla separately from https://github.com/LGhassen/Cyla/releases. Its GPLv3 plugin and compiled-only shader notice remain the upstream author's terms. Original Ringworld adapter code calls the public shader interface without embedding upstream C# or reconstructing shader source. See CREDITS.md and THIRD-PARTY-NOTICES.md. Cyla is indexed on CKAN. Public v1.1.3 metadata currently requires it; v1.1.4 development changes this to an optional suggestion.

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

See [VALIDATION.md](../history/VALIDATION.md) for current regression logs and [RELEASE-NOTES.md](../../RELEASE-NOTES.md) for release scope.

## Rendering diagnostics (v1.1.4 development)

The first daytime Cyla session logs `CYLA ENV`: GPU, driver/API, Unity version, colour space, render path, antialiasing, HDR, depth flags, clip planes, camera altitude, optical dimensions and sample settings. A one-time, small floating-point readback logs `CYLA TARGET` statistics. Invalid values indicate a numerical/render-target failure; finite dark values need comparison with geometry, illumination and the background. These measurements are diagnostic evidence, not an automatic verdict. Full-resolution rendering currently records the background only; reduced-resolution rendering also records scattering and the white-background response. Readback errors are logged without disabling rendering.

For a reproducible report, attach the affected session's KSP.log, settings.cfg, the save's persistent.sfs and exact mod versions. State whether the failure occurs only during daylight and whether Original restores the sky. Shader connection messages establish loading, not successful scattering. A working clean install on another GPU does not rule out an integration defect. Avoid changing several settings together during a comparison.

Run `./smoke-test.ps1 -CylaDiagnosticOnly -TimeoutSeconds 900` against the development instance for the current optical sampling regression. It uses Slow terrain, disables clouds, selects one-eighth optical resolution, and compares four optical unit scales at 128 view samples, at 2 km and 20 km camera altitude. It also checks acceleration of a stock loose physical object and stock Sun-flare night/day brightness. The normal plugin is restored when the harness exits. Capture-completion PASS is not an assertion of image correctness: inspect the PNGs and logged measurements. Camera-height probes alter the viewing position, not physical vessel altitude.

### Withdrawn black-sky experiments

Experimental patch 2 was backed up and reverted to public v1.1.3 before this investigation, following a zoom-out jitter report. Its near-floor guard remains withdrawn. The new optical-unit/depth implementation described below was selected through fresh comparisons; historical patch-2 results are not evidence of cross-GPU validation.

Private affected-save probes can still run with `template_instance/CylaFriend.sfs` and `./smoke-test.ps1 -CylaSaveProbe -CylaDiagnosticOnly -TimeoutSeconds 900`. Test saves, logs and captures are excluded from release packages. See the validation record for individual experiments and limitations.
## v1.1.4 optical units

The current fix evaluates the optical model in kilometres (`0.001` optical units per physical metre). Radius, thickness, width, offsets and density scale heights share that scale; extinction coefficients use its reciprocal. The actual camera position remains in the shader's expected Unity coordinate system. A scoped RFloat depth texture encodes the corresponding optical eye depth for Cyla's draw calls; other camera globals and native Cyla parts are not changed. This is a new implementation from the public baseline, not a restoration of the entire experimental patch 2. The earlier floor-clearance guard and unsuccessful camera-matrix overrides are absent.

Affected-save tests compared scales from 0.00001 through 0.001 with normal lighting, one-eighth/full optical resolution, a foreground marker and a native Cyla component present. All four restored the local sky. The larger 0.001 scale reduces the precision penalty of adding the small optical model to a zoomed-out camera position. This does not establish a fix on every GPU. Low sample-count bands and motion quality remain separate validation work. Presets now disable native unfiltered dithering; users may retain it as an advanced custom option.
