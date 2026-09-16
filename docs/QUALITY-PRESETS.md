# Ringworld rendering presets and biome features

Presets are selected from the Settings dropdown and apply immediately to this save. Save the game to persist them. Manual edits still use Apply settings. The dropdown reads Custom when current settings do not match a preset. Presets do not change KSP graphics preferences, seed, biome/physical tree distribution, ring dimensions, gravity, weather timing, science or flight controls.

All names are relative cost tiers, not hardware guarantees. Simple atmosphere rendering does not use volumetric ray steps, so some lower tiers differ chiefly in ground details and weather particles. Stock texture quality, antialiasing and scatter remain respected. The horizon has no artificial upper cap when entered manually.

| Preset | Horizon (km) | Terrain subdivisions | Blocks/frame | Forest | Atmosphere | Water | Ground detail (m) |
|---|---:|---:|---:|---|---|---|---:|
| Absolute Cow | 2,000,000 | 32 | 4 | Ultra | Full-resolution | Waves | 250 |
| Extra Beefy | 1,000,000 | 32 | 3 | Ultra | Full-resolution | Waves | 225 |
| Beefy | 500,000 | 32 | 2 | Ultra | Full-resolution | Waves | 200 |
| Strong | 320,000 | 16 | 2 | High | Half-resolution | Waves | 175 |
| Good | 240,000 | 16 | 2 | High | Half-resolution | Reflective | 150 |
| Mid | 160,000 | 16 | 1 | Low | Half-resolution | Reflective | 125 |
| Slow | 160,000 | 8 | 1 | Economy | Simple | Simple | 100 |
| Better Potato | 160,000 | 8 | 1 | Economy | Simple | Simple | 75 |
| Potato | 160,000 | 8 | 1 | Economy | Simple | Simple | 50 |
| Aged Potato | 160,000 | 8 | 1 | Economy | Simple | Simple | 35 |
| Rotten Potato | 160,000 | 8 | 1 | Economy | Simple | Simple | 25 |

## Biome features: forests

Forest LOD quality is independent of atmosphere and can be changed after an expedition begins. Economy uses one low-polygon near canopy mesh per patch with approximately one-quarter of the visual crowns, enlarged to represent tree groups. The complete physical tree list is retained for nearby pooled trunk contacts. Far woodland remains recognizable through canopy colour and relief, without intermediate crown meshes. Low/High/Ultra use the former Laptop/High/Ultra crown budgets (8/16/24 candidates per side on larger blocks; ranges bounded by 8/16/32 km block sizes). Photo mode temporarily uses Ultra distant crowns.

Economy significantly reduces forest mesh construction, memory and drawing; it does not skip all tree sampling or alter ground collision. It is deliberately coarse. Quality changes rebuild at most one existing scenery tile per frame. Terrain and distant canopy queues use their existing incremental budgets. This is not GPU instancing or an FPS guarantee.

Near trunks remain limited to the existing contact radius. World forest density is a separate generation parameter, locked after visiting the ring; visual quality remains editable. New biome-specific controls should follow this section rather than changing physical generation when a graphics preset is selected.
