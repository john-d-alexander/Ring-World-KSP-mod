# Science and expedition progression

Version 1.1.3 uses stock science equipment. No new Ringworld instrument is required. The old RW-1 remains defined only to keep existing craft loadable; it is hidden from the editor and research tree.

## Playing

Open the ring toolbar panel and choose **Research**. This tab is available in Sandbox, Science and Career. It shows the current research location, science multiplier, expedition objectives and landmark coordinates. Sandbox previews the objectives; Science and Career record delivered research. Only Career pays funds and reputation.

Use crew reports, EVA reports, surface samples, thermometers, barometers, gravity and seismic sensors, Mystery Goo, the Science Jr. and atmospheric analysis equipment normally. Stock situation restrictions, crew/facility requirements, reset rules, antennas, electric charge, transmission penalties and diminishing returns still apply. Producing or keeping a report does not complete an objective: stock R&D must receive positive science through transmission or recovery. Laboratory-generated generic science is not a substitute for returning a required sample.

## Where science comes from

The subject includes the experiment, situation and stable research location:

- Surface, water, lower atmosphere (below 18 km), upper atmosphere, low space (up to 500 km), and high space (up to 2,000 km above the floor).
- All thirteen procedural biomes, where no named site overrides them.
- Every named landmark; the smallest containing landmark wins where sites overlap, so an island station does not disappear into its parent Great Ocean.
- Individual fixed structures and seeded colossi. Identification uses their configured dimensions and placement rules, not whether their mesh is currently rendered. A structure's research envelope is a circle enclosing its horizontal dimensions, extended by 500 m by default, with matching height bounds. This accommodates prefab orientation without relying on loaded renderers.
- North and south rim walls, distinguished from terminals at their feet. Wall-top contact gives landed science; close approaches give space science.
- Twenty individually identified shadow squares, within 500 km of their radial plane and their plate footprint. Their identities follow the same motion as the rendered panels. They are survey targets, not newly landable bodies.

Altitude is measured inward from the ring floor, not from the Sun. Water science recognizes a slowly floating owned vessel within five metres of local mean water level; this is a science classification for the existing approximate buoyancy system, not a replacement water solver. Local wave crests do not create new subjects.

Regions outside the bounded ring/panel research envelopes retain ordinary stock science. The native reference body remains the Sun, so the stock archive can still group these subjects under it. Saved subject titles retain their Ringworld location names.

## Rewards

Default multipliers scale both experiment value and its science cap:

| Location | Multiplier |
|---|---:|
| Space above the ring | 6× |
| Procedural biome | 10× |
| Named landmark | 14× |
| Rim wall | 16× |
| Individual structure | 18× |
| Shadow square | 20× |

For example, a crew report with a stock cap of 5 has a cap of **70 science at a landmark**. A surface sample with a stock cap of 40 has a cap of **400 in an ordinary biome**. Difficulty and transmission efficiency affect actual returns. Repeating a subject consumes its remaining value rather than resetting it. Each individual seeded structure has its own finite subject, so the enormous world intentionally contains a very large total research budget.

Twelve one-time milestones cover first contact, remote reconnaissance, atmospheric pressure mapping, a crewed foothold, EVA, sample returns, six-biome ecology, a landmark atlas, archaeology, both rims, two shadow squares and comparative gravity measurements. Career base rewards range from 150,000 to 1,200,000 funds and 15 to 75 reputation. Stock difficulty multipliers and currency transactions apply. These are expedition milestones in the Research tab, **not Mission Control contracts**, and they have no deadlines or failure penalties.

The journal counts distinct research locations, not button presses. Two experiments from the same forest do not count as two biomes. Prerequisites can be completed using previously returned qualifying data. Completion and delivery records are saved in `RingworldScenario`; reloading or sending another packet does not repeat a milestone payment.

## Compatibility and current limits

- Existing old-format science and RW-1 reports remain in saves. New, more specific subjects use a `RingworldV2_` namespace; old generic science is not silently relabelled as a newly surveyed landmark or milestone.
- The adapter targets stock `ModuleScienceExperiment` paths and Breaking Ground deployed experiments. A mod that replaces science collection or bypasses stock R&D needs its own adapter; no blanket compatibility with Kerbalism or every science overhaul is claimed.
- Research overlays, physics, artwork and progression are separate. Turning down forest quality or terrain distance does not change an experiment's identity.
- Unmatched arrivals are still physically destructive. This update does not grant free velocity matching in Career. A later progression-gated transport-terminal system is planned to make stock-part expeditions practical. Remote reconnaissance is the first step; the surface campaign currently assumes you have an arrival solution.
- The first release of this progression system does not add Kerbal XP rules, tourism, construction/resource processing, life support or operational megastructure machinery.

See [the extension guide](MODDING-RESEARCH.md) for data-driven rewards, objectives and new research sites.

## Multiple habitats

The original habitat retains its existing science IDs. Additional habitats prefix their stable ring ID into the location portion of each subject and show the habitat name in the report. Renaming does not reset previously earned science. Expedition milestones remain save-wide.
