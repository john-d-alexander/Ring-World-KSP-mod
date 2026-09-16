# Niven Ringworld Expedition 1.0.2

The square forest in 1.0.1 was the near-terrain rendering footprint, not a square biome. Forests now continue into terrain LODs as simplified, merged crown clusters, then area-filtered canopy colour and height at greater distances. Near and distant rendering share the same climate and woodland mask. The representation uses at most one extra canopy mesh per intermediate terrain block, without millions of additional tree objects or distant colliders. Existing terrain range/resolution and scatter settings apply.

The eight colossus templates now spawn rarely from the terrain seed across the ring, instead of clustering around named sites. Defaults: one candidate in 15% of 2,000 km cells, then dry-ground/hull checks. A 600 km exclusion around named landmarks leaves zero colossi there by default; ordinary buildings remain. Configuration and the artist-facing biome representation are documented in COLOSSI-AND-FORESTS.md.

This release changes scenery, not save/warp physics. Restart KSP to load the new plugin/configuration. Existing seeds remain unchanged; megastructure positions change deliberately. Distant canopy is simplified geometry, not individual tree rendering.

Validation after the battery-interrupted run was repeated successfully: 108,648 core assertions, rare-placement checks, queued-frame isolation, and completed forest/terrain streaming. The Laptop fixture measured about 21 FPS. High-altitude views still show coarse intermediate clusters and visible LOD bands; see VALIDATION.md for measurements and limits.
