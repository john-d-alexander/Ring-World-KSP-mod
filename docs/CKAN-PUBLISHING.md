# Publishing Niven Ringworld Expedition on CKAN

Nothing has been uploaded or submitted by this development task. CKAN indexes a publicly hosted release; it is not the download host. The release is currently local.

1. Publish a versioned release ZIP on SpaceDock (select KSP 1.12.5, MIT license, description/support link), and tick the CKAN checkbox. Alternatively publish a GitHub release and submit a KSP1 mod request to NetKAN.
2. In the request, specify identifier `NivenRingworld`, install only `GameData/NivenRingworld` into `GameData`, and declare `Harmony2` >= 2.2.1.0 as a dependency. The ZIP bundles Harmony for manual users; CKAN must manage it separately rather than install that bundled copy, to avoid ownership conflicts with other mods.
3. Use the shipped `NivenRingworld.version` file via `$vref: '#/ckan/ksp-avc'`. Compatibility is 1.12.5 only; broader game-version support has not been validated.
4. Replace the hosting placeholder in `distribution/NivenRingworld.netkan.example`, then validate with NetKAN and test CKAN installation in a clean 1.12.5 instance. Verify the two plugin DLLs, scenery/visual bundles and Harmony are installed exactly once. Launch, enter the ring, save/reload, then test uninstall/reinstall. This publication test has not run because no public release URL exists yet.
5. The CKAN team reviews the indexing request. Acceptance is required before it appears in the public list. Subsequent indexed releases can be picked up from the host.

Current runtime visuals were tested on Windows/D3D11; cross-platform shader bundles are not verified. Mention this limitation in the public description rather than claiming universal platform support. Breaking Ground is optional for its deployed-science features; no optional visual mods are required.

Sources checked September 16, 2026:
- https://github.com/KSP-CKAN/CKAN/wiki/Adding-a-mod-to-the-CKAN
- https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md
- https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/Harmony2/Harmony2-2.2.1.0.ckan

The draft metadata is an example, not submission-ready until a hosting ID and public release exist. Do not include generated .ckan metadata in the mod archive; submit the indexing information to NetKAN.
