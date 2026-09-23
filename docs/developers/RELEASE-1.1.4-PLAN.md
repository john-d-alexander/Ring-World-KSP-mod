# v1.1.4 delivery and continuing compatibility work

The v1.1.4 release delivers the locally verified fixes below for testing on affected NVIDIA/Proton systems. Cross-GPU confirmation and remaining integrations are ongoing; they are not prerequisites silently claimed complete by this release.

## Implemented and locally verified

- [x] Reproduce the daytime black sky from an affected save on Adreno/D3D11, starting from public v1.1.3.
- [x] Use kilometre optical units with scoped matching scene depth. Four scale comparisons restored the sky with foreground geometry preserved. No camera-global overrides or floor-clearance guard remain.
- [x] Disable unfiltered Cyla dithering in presets. Existing custom settings remain unchanged until a preset is selected.
- [x] Apply ring acceleration to stock loose physical objects; measured approximately 9.7154 m/s² toward the floor.
- [x] Use ring atmosphere for stock EVA helmet safety, retaining pressure and temperature checks; science/career regression passed.
- [x] Hide the stock Sun flare under the night-panel mask and restore it in daylight; measured brightness 0 at night and 1.576891 by day.
- [x] Automatic preset selection of installed Cyla, with Original fallback; retain manual diagnostic override under advanced settings.
- [x] Release metadata suggests Cyla and requires Harmony; no dependencies bundled.

## Release delivery

- [ ] Build, verify and publish v1.1.4 with the existing KSP-root ZIP layout.
- [x] Submitted the optional-Cyla change in [NetKAN PR #11604](https://github.com/KSP-CKAN/NetKAN/pull/11604). CKAN maintainer acceptance is pending.

## Continuing work

- [ ] Confirm the black-sky correction on affected NVIDIA Windows and Windows-through-Proton installations using new logs. Neither GPU vendor nor OS alone explains the local reproduction.
- [ ] Resolve remaining low-sample high-altitude bands and validate camera motion/performance. Kilometre units reduced the precision steps compared with smaller units; broad sampling bands remain.
- [ ] Validate sun-disc occlusion and custom Scatterer flares, beyond the tested stock flare.
- [ ] Installed-mod compatibility tests for Waterfall, TUFX, Deferred, ReStock and Trajectories. Waterfall atmosphere/Mach controllers already read fields provided by Ringworld; do not claim an installed-mod test yet.
- [ ] Investigate cylindrical adapters for EVE/volumetric clouds, Scatterer and Parallax. Their spherical-body configuration formats are not a drop-in ring renderer.
- [ ] Assess Kopernicus interoperability without replacing the rotating frame merely to obtain spherical APIs.
- [ ] Native-Linux shader bundle support. The current Windows/D3D11 bundle does not certify native Linux; the reported Linux tester uses Windows KSP through Proton.
- [ ] Further photo-mode, extreme custom optics, unloaded-vessel and detached-module regression coverage.

## Development boundaries

Use template_instance for live testing, Slow-or-lower presets on the laptop, and low-cost isolated shader probes where needed. Keep private saves, logs and screenshots out of packages. Experimental patch 2 was backed up and reverted before the current implementation. See the validation record for individual tests and their limitations.
