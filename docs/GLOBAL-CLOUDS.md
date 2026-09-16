# Full-ring clouds: seeded fBm coverage

The distant cloud ribbon remains visible from flight, other planetary views, map and tracking station. It sits at a representative 5.5 km altitude, with 16,384 segments and 32,768 triangles. Atmosphere off or cloud amount zero disables it.

## Generation and coverage

Coverage now uses an original seeded gradient-Perlin implementation with quintic interpolation, five fBm octaves (4,000 km down to 250 km), and low-frequency domain warping. Hashes use the complete 32-bit world seed. It no longer tiles the previous 512 km or 32,768 km coverage textures. Longitude repeats only at the ring's actual circumference; the across-axis hash domain exceeds the physical ring width by orders of magnitude.

Distant/fake clouds target approximately 50% coverage, measured as coverage-field values above 0.5. This is a statistical surface-area target, not a promise that half of every view is white. Smooth edges and translucent cloud shading reduce average opacity. Weather lightly shifts distant coverage; local weather is not capped at 50% and can remain clear or overcast. Across the existing handoff band, the field blends to the local weather's amount.

Integer lattice cells and fractional offsets are carried separately, with double-precision CPU origins, so local movement does not lose precision at large ring longitudes. Both renderers evaluate the same field with universal-time wind and ring rotation. Fine 3-D erosion still uses the existing noise texture for volumetric cloud shape; that texture no longer determines repeated large cloud banks. Distant fBm octaves are filtered when smaller than a pixel.

This follows the gradient-noise, multiscale fBm and frequency-filtering concepts in [PBRT: Noise](https://www.pbr-book.org/3ed-2018/Texture/Noise). [OpenSimplex2](https://github.com/KdotJPG/OpenSimplex2) was also reviewed as an alternative. No third-party noise implementation or art assets were copied; Perlin was chosen to retain explicit cylindrical seam handling and cell/fraction precision.

## Map layering

The cloud shell faces inward only. An analytic ray/hull test prevents far-side clouds and terrain bleeding through the outer hull when scaled-space depth values collapse together; views into the opening above the rim remain possible.

The base ribbon renders first, streamed terrain second, transparent clouds afterward. The scaled terrain uses a small depth bias. Night shading is evaluated from the same shadow-square phase in each layer; matching shadow multipliers on terrain and clouds is algebraically equivalent to darkening their final composite. There is no extra coplanar shadow mesh to z-fight.

Distant terrain no longer has deep crack-cover skirts. The quadtree limits cell curvature error and culls outside the ring before subdividing. Map/tracking use a shallower hull mesh without the extra flight-camera safety burial. Flight retains that margin to keep the distant fallback out of the local ground.

## Validation

Build shaders with `build-visuals.ps1`. `smoke-test.ps1 -GlobalCloudsOnly` samples a 128,000 km square coverage field on the GPU, checks roughly 45–55% cloudy samples, tests that offsets of 512 km and 32,768 km do not repeat it, and verifies ring seams, day/night shading and optical handoff. `-MapOnly` also checks skirt-free scaled chunks, large-distance save roundtrips and wall camera constraints, and captures inside/outside map views. These fixtures are not a guarantee of every camera/quality combination or constant laptop FPS.
