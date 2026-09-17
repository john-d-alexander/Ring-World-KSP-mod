# Cyla source assessment — 2026-09-16

Historical pre-implementation assessment. The experimental branch now vendors and bundles Cyla; see [CYLA-INTEGRATION.md](CYLA-INTEGRATION.md) for the current implementation, test results and limitations. Statements below about an unchanged installation describe the initial assessment only.

Reference checkout: C:/Users/Hans/Documents/programming/Cyla-reference
Upstream: https://github.com/LGhassen/Cyla
Pinned commit: 92223648e0674212e488e6fb977b5cffc63be869
Release inspected: 1.1.0.0 / Cyla-1.1.0.zip, extracted beside the checkout.
No upstream code or binaries have been incorporated into NivenRingworld or its release packages. The installed v1.0.3 remains unchanged.

## What is available

The public repository has four C# source files: AtmosphereRenderer, CylindricalAtmosphereModule, ShaderLoader, LightingMode. Its license explicitly says plugin code is GPL v3 and shader code is not public, provided only compiled. The release contains Cyla.dll and Shaders/cylashaders, but no shader source. A complete source transplant cannot be performed from this repository.

The C# renderer creates a quad with an enlarged bound and a Cyla/Atmo material at render queue 2990. It passes cylinder dimensions, centre and axis, Rayleigh/Mie scattering and scale heights, Mie asymmetry, integration counts, dithering, light colour and sun position to the shader. The part module exposes these through the stock part-action window, normalizes scattering coefficients by atmosphere thickness, and multiplies normalized scale heights by that thickness. It is part-attached rather than attached to a celestial habitat manager.

Quality controls are view integration steps (1–500, default 40), light/transmittance steps (1–50, default 10) and dithering. There is no public preset system, temporal accumulation, photo capture or half-resolution renderer to copy. Lighting modes are TransparentTopAndSide, TransparentFloor and Unlit. TransparentTopAndSide is the relevant Niven-ring choice; opaque side-wall and floor geometry need to match the habitat.

The public controller does not reveal the shader's scene-depth sampling, scattering integrator, ray/cylinder intersections or its detailed object occlusion. It locates SunLight/SpotlightSun, falling back to the brightest local directional light, and uses the body named Sun for illumination position. It does not visibly implement cloud weather, ring shadow squares, map-space integration, ocean optics, per-object light attenuation or our photo pipeline. Those capabilities must not be inferred from the screenshots. No runtime rendering compatibility test was performed.

## Integration work required

- Preserve GPL notices and provide corresponding source for any distributed GPL-derived controller. The current MIT-only package description cannot describe copied GPL code as wholly MIT. Shader redistribution/source terms need explicit clarification for a modified integrated renderer; the supplied license distinguishes the unavailable shader source.
- Use a ring-owned renderer, not a required craft part. Bind dimensions and orientation to the existing rotating ring frame and finite wall width. Keep atmospheric forces, parachutes, science and saves in the existing physical model.
- Resolve numerical precision before deployment: upstream passes centre/radii as float/Vector3. Around our 15.3-billion-metre radius, adjacent float values are about 1,024 m apart. This is an inferred integration risk for near-ground density/depth, not a measured Cyla failure. Our present renderer uses a camera-centred tangent chart. A shader port needs comparable stable coordinates; changing the C# adapter alone does not prove the compiled shader stable.
- Integrate scattering with opaque scene depth so aircraft, trees, cities, water and walls occlude it. Explicitly test transparent materials, local/scaled cameras, map entry/exit, ring outline, sun/planets and day/night stars. Queue 2990 alone does not establish correctness.
- Map view/light sample counts and dither controls into our eleven presets. Keep optical parameters separate from quality: changing quality must not change air pressure or weather. Add per-save advanced scattering controls only once the shader backend can consume them.
- Retain the existing cloud renderer and shared global-cloud field unless a new source implementation replaces them. Cyla's published C# is an atmosphere controller, not a cloud system.
- Adapt photo mode explicitly: freeze simulation/lighting/weather, use the frozen scene depth, accumulate controlled dither samples, retain terrain readiness, and restore camera/material state on cancel or resize. Cyla exposes a frame counter but no public photo pipeline.
- Test ground/horizon/orbit/map, water/craft silhouettes, rim lighting, shadow-square night transitions, camera shifts and photo cancellation on the laptop before making it the default. Compare images and frame/memory costs; visual improvement is not established by reading the source.

## Current blocking input

A full source port needs the Cyla atmosphere shader source and usable modification/distribution terms from its author. Alternatively, a separately installed compiled Cyla backend could be explored as an experimental dependency, but it cannot provide editable atmosphere code and may not support this ring scale or our photo composition. Neither path has been silently substituted for the requested full transplant.

Primary sources:
- https://github.com/LGhassen/Cyla/blob/92223648e0674212e488e6fb977b5cffc63be869/License.md
- https://github.com/LGhassen/Cyla/blob/92223648e0674212e488e6fb977b5cffc63be869/Cyla/AtmosphereRenderer.cs
- https://github.com/LGhassen/Cyla/blob/92223648e0674212e488e6fb977b5cffc63be869/Cyla/CylindricalAtmosphereModule.cs
- https://github.com/LGhassen/Cyla/releases/tag/1.1.0.0
