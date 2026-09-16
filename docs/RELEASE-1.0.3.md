# Niven Ringworld Expedition 1.0.3

Added an immediate-apply quality preset dropdown with Absolute Cow, Extra Beefy, Beefy, Strong, Good, Mid, Slow, Better Potato, Potato, Aged Potato and Rotten Potato. Each sets terrain range/resolution, streaming budget, forest quality, atmosphere, water, ground detail and weather visual controls. Low tiers retain a 160,000 km horizon. Individual controls remain available; edits use Apply settings. Save the game to retain changes.

The Biome features section separates forest quality from atmosphere. Economy is cheaper than the former Laptop forest level: approximately one-quarter as many nearby crown shapes, one simple mesh per patch, no tree shadows and no intermediate distant crown meshes. Forest colour/relief remains in distant terrain. Nearby physical tree distribution is retained. Changes rebuild one existing scenery tile per frame. No terrain seed, density or flight physics changes.

Existing saves without forestQuality retain their former atmosphere-linked forest level on first load; thereafter it is independent. Restart KSP to load the new DLL. Quality names are relative cost tiers, not FPS guarantees. Coarse aggregate forests and LOD transitions remain visible.

CKAN-PUBLISHING.md documents hosting, dependency ownership and submission; distribution/NivenRingworld.netkan.example is a local draft requiring a real public hosting ID. Nothing has been submitted or uploaded.

Validation: 108,648 core checks and the live preset/scenery regression passed. Economy reduced stored near-canopy vertices from 2,392,388 to 70,365 in the fixture, with no intermediate distant crown meshes. This does not imply a corresponding FPS increase. See VALIDATION.md.
