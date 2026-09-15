# Full-ring cloud rendering

Clouds now have their own transparent scaled-space ribbon at a representative 5.5 km cloud altitude, independent of the full-ring terrain-detail setting. They are visible wherever the ring's inner surface is visible: flight, distant planetary views, map and tracking station. The opaque hull and rim walls can still occlude them. Setting Ringworld atmosphere off or cloud amount to zero removes this layer.

The old cloud tint painted into DistantSurface has been removed. GlobalClouds uses the same seeded 3-D noise texture, 512 km periodic coverage function and universal-time wind offsets as the laptop cloud decks and GPU volumetric atmosphere. Each of 16,384 ribbon segments has its own small, unwrapped cloud coordinate interval, rather than computing small cloud features from a circumference-sized float. Adjacent intervals agree modulo one, including the longitude seam. Texture mip selection filters subpixel clouds on the distant arc.

Global weather follows the CPU weather model's temporal blend, seeded regional noise, storm threshold and severity-to-cloud mapping on the GPU. Near the player it blends to the exact CPU weather sample used by local rendering. The cloud ribbon receives the same twenty moving night bands and day-phase parameter as the ring surface. This is inexpensive shading, not ray-traced shadowing.

Local coverage fades over 40–72% of the available local cloud range (capped at 180 km). The global layer fades in over the complementary interval, using camera distance to the cloud band. Optical-opacity weighting avoids a hard switch on the laptop decks. The High/Ultra renderer fades its volumetric density over the same interval. Cloud positions and major coverage features are shared; the global sheet remains an approximation of the volume's depth and small billows, so this is a gradual representation change rather than identical geometry at all distances.

There is one additional global draw and 32,768 cloud triangles, with no colliders or CPU cloud-mesh rebuild per frame. This is not a claim of constant FPS on every laptop. The Windows/D3D11 visual bundle must be available; an unsupported/missing shader retains the pre-existing reduced visual fallback.

Build shaders with `build-visuals.ps1`. `smoke-test.ps1 -GlobalCloudsOnly` checks the exported shader, all longitude seams, daylight/night response, the local handoff and the off setting. Its isolated render images live in `artifacts/validation/global-clouds/`. Flight weather/warp checks remain available with `-WeatherOnly`.


## Broken cloud banks

Local and scaled renderers now share a 32,768 km macro coverage field in addition to 512 km fine detail. Independently wrapped double-precision origins keep both fields anchored to the rotating ring with wind drift. The scaled mesh carries a separate continuous macro chart. Cloud amount controls coverage thresholds; large clear regions remain when fine noise averages out at long range. This adds two filtered texture samples, without additional geometry or 3D textures.
