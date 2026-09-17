# Publishing Niven Ringworld Expedition on CKAN

CKAN indexing has not been submitted. GitHub release publication is separate from CKAN approval. v1.1.0 bundles Cyla and therefore requires updated dependency ownership and license metadata before indexing.

1. Use the versioned GitHub release for KSP 1.12.5 and submit a KSP1 mod request to NetKAN after validating the metadata. Do not describe the whole bundle as MIT: Cyla carries its own GPLv3/compiled-shader notice.
2. In the request, specify identifier `NivenRingworld`, install only `GameData/NivenRingworld` into `GameData`, and declare `Harmony2` >= 2.2.1.0 as a dependency. Coordinate a separate Cyla dependency with the CKAN maintainers (do not guess an existing identifier); preserve its license and source distribution. The ZIP bundles Harmony for manual users; CKAN must manage it separately rather than install that bundled copy, to avoid ownership conflicts with other mods.
3. Use the shipped `NivenRingworld.version` file via `$vref: '#/ckan/ksp-avc'`. Compatibility is 1.12.5 only; broader game-version support has not been validated.
4. Complete the Cyla ownership/dependency information in `distribution/NivenRingworld.netkan.example`, then validate with NetKAN and test CKAN installation in a clean 1.12.5 instance. Verify the two plugin DLLs, scenery/visual bundles and Harmony are installed exactly once. Launch, enter the ring, save/reload, then test uninstall/reinstall. This clean CKAN installation test has not run.
5. The CKAN team reviews the indexing request. Acceptance is required before it appears in the public list. Subsequent indexed releases can be picked up from the host.

Current runtime visuals were tested on Windows/D3D11; cross-platform shader bundles are not verified. Mention this limitation in the public description rather than claiming universal platform support. Breaking Ground is optional for its deployed-science features; no optional visual mods are required.

Sources checked September 16, 2026:
- https://github.com/KSP-CKAN/CKAN/wiki/Adding-a-mod-to-the-CKAN
- https://github.com/KSP-CKAN/CKAN/blob/master/Spec.md
- https://raw.githubusercontent.com/KSP-CKAN/CKAN-meta/master/Harmony2/Harmony2-2.2.1.0.ckan

The draft metadata is an example, not submission-ready until Cyla ownership/dependency metadata is resolved and the clean install test passes. Do not include generated .ckan metadata in the mod archive; submit the indexing information to NetKAN.
