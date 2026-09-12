# Canon, scale, and deliberate adaptations

Research checked September 12, 2026. The relevant original game target is **KSP 1.12.5**, not KSP 2. The supplied installation has build 03190.

## Sources

- [Official KSP 1.12.5 Steam announcement](https://store.steampowered.com/news/app/220200/view/3650762049190579639).
- [Larry Niven's official Ringworld bibliography entry](https://larryniven.net/?q=bibliographic-reference%2Fringworld).
- [The Incompleat Known Space Concordance: Ringworld Appendix, hosted by the author's site](https://news.larryniven.net/concordance/content.asp?ovr=t&page=Ringworld+Appendix). This is an editorial concordance, not a uniform numerical specification by the novelist; it identifies discrepancies among books.
- [KSP API: Vessel](https://kspmoddinglibs.github.io/KSPDocsSite/class_vessel.html), [Krakensbane](https://kspmoddinglibs.github.io/KSPDocsSite/class_krakensbane.html), [FlightGlobals](https://kspmoddinglibs.github.io/KSPDocsSite/class_flight_globals.html), and [ResearchAndDevelopment](https://kspmoddinglibs.github.io/KSPDocsSite/class_research_and_development.html). Documentation is labeled 1.12.4; actual signatures are verified against the supplied game's assemblies.
- [Kopernicus PQS API](https://kopernicuswiki.org/API/PQSMod): background for why a conventional spherical height-field body does not provide the needed ring surface.

## Numerical basis

The author's hosted concordance gives approximately 153 million km radius, 1.605 million km width, 1,600 km rim walls, and twenty shadow squares near 46 million km radius. Its gravity discussion contains an editorial reconciliation around 9.72 m/s². This mod takes one tenth of those **lengths** and derives angular speed from the selected acceleration. Rounded published speed and period are not simultaneously enforced.

The ring is a ribbon whose habitable face points toward the central star. The walls retain an atmosphere; the habitat includes land, waterways, engineered material beneath the soil, and remnants of advanced infrastructure. The shadow system provides illumination changes distinct from structural rotation. Those are the high-level source features used here.

## Original implementation choices

- Ring radius is independent of Kerbin's orbit. Choosing exactly Kerbin's orbit would place the stock planet in the structure.
- Stock Kerbol replaces the fictional star. Its mass, luminosity, and surrounding planetary system are not altered.
- Gravity is computed, not implemented as attraction to a torus centerline. In ring coordinates:

  `a = -mu*r/|r|^3 - 2*omega×v - omega×(omega×r)`

  The first term is real stellar gravity, the second is Coriolis acceleration, and the last is centrifugal acceleration. A stationary craft requires the floor's normal reaction to support it. KSP's existing acceleration is subtracted before this total is applied, avoiding double stellar gravity.
- Radius reduction while preserving surface acceleration gives a rotation period proportional to the square root of radius. The resulting spin period is not simply the book's period divided by ten.
- Three-hour lighting is a gameplay setting. Atmosphere depth, scale height, vegetation density, building size, and terrain roughness favor recognizable Kerbal-scale exploration rather than uniform geometric scaling.
- The landing outpost, scrith excavation, city layout, research terminal, settlement placement, and reports are newly authored. The monumental mountains and two ocean regions are inspired interpretations. The map-island is not an exact map of Earth. No exact canonical coordinates are asserted.
- River channels are analytical, carved meanders in repeated catchments with graded water levels. They are not a simulated global hydrological network.
- Rural dwellings, low-poly trees, and rocks are generated as native Unity geometry. There are no imported copyrighted art assets.

## Not recreated

No complete canonical geography, inhabited civilizations, hominid species, character encounters, exact city interiors, full planetary map-islands, functioning maglev civilization, stellar engineering, dynamic puncture catastrophe, meteor defense system, or ecological evolution is implemented. The physical scrith engineering is not simulated. These would be separate major content and engine projects.
