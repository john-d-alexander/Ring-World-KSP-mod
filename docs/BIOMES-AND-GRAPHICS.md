# Biomes, ground detail and laptop rendering (0.6)

New saves use terrain generator 4. Existing saves keep their stored generator: changing terrain beneath a landed craft would be unsafe. Start a new Sandbox save to explore the new landscape. Random visits change coordinates, not the terrain seed.

## Landscape design

Seeded, continuous temperature, humidity and ruggedness fields blend five climate families: meadow, forest, desert, cold and highland. Their weights smoothly influence ground colour, tree cover, relief and a small atmospheric RGB tint. The displayed biome name is the strongest family, so its label changes discretely even though the visual weights blend. Oceans, rivers, roads and authored landmark areas retain their own classifications. The outermost portion of the ribbon receives a cold climate bias; this approximates rim-shadow ecology, not a thermal simulation.

The useful Minecraft reference is separation of biome placement from terrain shape, plus climate-dependent appearance. This is an analytic curved surface, not a voxel implementation or a copy of Minecraft's generator. [Mojang's 1.18 description](https://www.minecraft.net/en-us/article/caves---cliffs--part-ii-out-today-java) explains independent terrain and biome placement; [Microsoft's Bedrock client-biome components](https://learn.microsoft.com/en-us/minecraft/creator/reference/content/clientbiomesreference/examples/componentlist?view=minecraft-bedrock-stable) describe grass, foliage and water appearance controls.

Ordinary v4 land uses low rolling relief, capped at 100 m of variation at height multiplier 1 above a 100 m reference. This is a design interpretation of shallow covering over scrith, not a claim that every point has exactly that soil depth. River carving, water basins and engineered landmark mountains are separate. Existing warped catchments, tributary-shaped channels, ponds, oceans and coastlines remain analytic: there is no globally connected drainage solver or hydraulic erosion. Exposed scrith is presently a grey erosion-mask surface treatment.

## Close detail

Grass blades, small shoreline stones, sand patches, leaf litter and occasional silver sunflower clusters share one collision-free mesh. Candidates are seeded and jittered; placement raycasts against the actual streamed ground triangles. Density follows KSP's Terrain Scatters switch and scatter factor. Trees also follow that native control, with climate-blended density in v4. Buildings are not disabled by the foliage setting.

Close detail is adjustable from 25 to 250 m, defaults to 75 m, and is hidden above 80 m ground clearance. New saves and the laptop preset select 75 m, an 8-segment distant mesh and one new LOD block per frame at a 160,000 km horizon. Candidate spacing grows with range; the hard ceiling is 2,200 clumps, not millions of individual objects. Rebuilding occurs on movement across 16 m cells or settings changes. No grass colliders or grass shadow maps are generated.

Local dust/pollen uses one small billboard mesh, at most 48 particles, updated at 10 Hz near the ground. It has a separate on/off setting. These are visual effects, with no wind force or sunflower heating. Climate tint changes RGB without adding sky opacity.

## Native graphics controls

The runtime does not write global Unity quality settings. Auditing this installation's `GameSettings.ApplySettings` showed KSP applies its quality preset, MSAA, master texture mip limit, v-sync, pixel light count and shadow cascades to Unity. Ringworld's normal camera/renderers share that pipeline. The UI reports the active AA, mip limit, shadows and native scatter density.

Terrain colour maps, clouds and dust now have mip chains. KSP Texture Quality can therefore select lower-resolution mip levels. [Unity 2019.4 mip-limit documentation](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/QualitySettings-masterTextureLimit.html) defines 1 as half resolution and 2 as quarter resolution. [Unity's AA documentation](https://docs.unity3d.com/2019.4/Documentation/ScriptReference/QualitySettings-antiAliasing.html) describes MSAA's forward-rendering limitation: inheriting the setting does not guarantee every transparent shader is antialiased identically.

Global shadow quality affects the local sunlight; global light, v-sync and frame limits remain KSP-controlled. Terrain Shader Quality/PQS subdivisions are stock spherical-terrain controls and do not directly set ribbon topology. Aero FX Quality and reflection-probe scheduling are likewise not substitutes for custom terrain LOD settings. Those distinct Ringworld controls remain in its panel. This is not a claim of implementing every stock terrain shader effect.

## Random rendering inspection

Alt+R > Expedition > **Random terrain test location (spin-matched)** uses the arrival-height slider. It searches at most 128 candidates, avoiding water, named landmark footprints, walls and steep local slopes. Arrival uses the same collider preparation and rotating-frame transfer as named destinations. Initial velocity is zero relative to the ring; inertially it includes the ring's rotation. Gravity acts immediately afterward, so fly the descent. Trees or other local scenery may still require landing-site selection.

The current seed and destination coordinates remain visible in the panel. Random visits work with existing generators too; they do not upgrade a saved world. This is an exploration/testing aid, not propulsion or an automatic landing.
