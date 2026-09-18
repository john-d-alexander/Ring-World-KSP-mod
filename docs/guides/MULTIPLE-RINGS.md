# Multiple habitats — v1.1.3

Sandbox only: Ringworld toolbar → Settings → Sandbox: manage ring worlds.

Select a habitat to edit. Enter a name, existing star (including loaded planet-pack stars) or no designated star, center X/Y/Z offsets in kilometers, diameter, width and seed. Blank seeds randomize on creation. Spawn creates a new persistent ID; editing retains the ID. Save the game after changing the catalog. Visit selected ring performs a sandbox spin-matched transfer to its arrival landmark.

Coordinates use KSP's non-rotating reference axes, not longitude/latitude. Y is across the ring. A designated ring follows its selected star plus the offset. Without a designated star it follows the stock Sun plus the offset, does not spawn a star and does not become a new celestial SOI. Normal stellar gravity remains. Planes are parallel; arbitrary tilts and freely orbiting centers are future work. Existing saves migrate their original habitat to the stable `primary` ID.

A conservative cylinder-envelope check rejects overlap, including some concentric layouts that might otherwise fit. Move/delete is blocked while the ring has resident vessel records. At least one habitat must remain. Recover or move vessels first. This prevents deleting the surface beneath an expedition. No automatic deletion of ships is performed. World settings outside this editor retain their existing saved-expedition protections.

Each habitat has its own global ring/walls/clouds/panels. Only the current nearby habitat generates local terrain and physics; distant habitats use coarse outlines. Extra habitats still cost rendering time and memory. Multiple independently rotating physics frames cannot coexist in one loaded Unity scene. Ring selection follows a saved resident's ID or the nearest habitat for approaching vessels. Tracking encounter guards test all habitats; the selected vessel's nearest habitat supplies its trajectory preview. Combined multi-ring gravitational trajectory prediction is not implemented.

Saved vessels, landed warp, stock science contexts and reload restoration use their habitat's center/ID. Science in additional habitats has distinct subject IDs. Expedition milestones remain save-wide. Per-ring quality settings are inherited when spawning and can be changed after visiting.

Lighting/cloud-shadow optics retain the central-star/day-panel approximation. Large off-center/no-designated-star habitats are sandbox geography, not a claim of physically correct off-axis illumination. Full multi-star lighting, ring tilt, scripted transport terminals, map icons and arbitrary orbiting habitat centers remain follow-up work.
