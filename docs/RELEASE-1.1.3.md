# Niven Ringworld Expedition v1.1.3

KSP 1.12.5, Windows x64 / Direct3D 11.

## Installation change: dependencies are separate

This ZIP contains **only NivenRingworld**, plus its documentation. Extract into your KSP root beside KSP_x64.exe, preserving the existing `GameData/NivenRingworld` layout.

- **Required:** [HarmonyKSP / Harmony 2](https://github.com/KSPModdingLibs/HarmonyKSP/releases), tested with 2.2.1.0. CKAN identifier: `Harmony2`.
- **Optional:** [Cyla by Ghassen Lahmar (LGhassen / blackrack)](https://github.com/LGhassen/Cyla/releases), supported version 1.1.0. Without it, Ringworld uses Original atmosphere.
- Upgrading from a bundled release: retain existing Harmony and Cyla folders; do not create duplicates. Other mods may also depend on Harmony.

The release includes separate `.netkan` and version-specific `.ckan` files for CKAN maintainer review. Harmony is declared required. Cyla is not declared under an invented identifier: optional indexing awaits its author's and CKAN's approval. These files do not mean that CKAN has accepted the listing.

## Sandbox: multiple ring worlds

Ringworld panel → Settings → **Sandbox: manage ring worlds**. Spawn, name, move or delete habitats with their own dimensions, seeds and saved identities. Select an existing star, including a loaded planet-pack star, or no designated star. Enter center offsets in kilometers. Use **Visit selected ring** for a spin-matched sandbox transfer.

Distant outlines, walls, clouds and panels render for each habitat; detailed terrain is streamed around the current habitat. Saved residents and science use per-ring identities. Occupied rings cannot be moved/deleted, overlapping habitat envelopes are rejected, and at least one habitat must remain.

This initial editor supports parallel rings fixed relative to a reference body. Without a designated star the reference is the stock Sun, and no star is created. Off-center illumination retains the existing central-star approximation. Arbitrary tilt, orbiting centers, combined multi-ring gravity prediction and physically accurate off-axis lighting are future work. Planet-pack stars are selectable but cross-mod behavior has not been validated against every pack.

## Exploration and science

Stock instruments, crew/EVA reports and surface samples now produce separate science for flight regions, biomes, named landmarks, individual megastructures, rim walls and shadow panels. Science values range from 6× to 20× stock subject multipliers. The Research tab tracks twelve expedition milestones; Career awards one-time funds and reputation on qualifying data delivery. Progress persists and duplicate delivery does not repay milestones. No new science part is required; legacy RW-1 craft remain supported.

Also includes the development fix for nearby/faraway rim-wall material consistency.

## Validation and limits

Core geometry/terrain/research checks and in-game regressions are recorded in `docs/VALIDATION.md`. Offset-ring landing, native warp, per-ring science and Space Center save/reload were tested at Slow. Missing Cyla was tested separately and fell back to Original. See `docs/MULTIPLE-RINGS.md` for the editor's placement/physics limits and `docs/CKAN-PUBLISHING.md` for metadata submission. CKAN catalog acceptance and a real CKAN client installation are separate from local package/schema verification.
