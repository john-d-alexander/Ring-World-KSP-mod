# Extending Ringworld research

Put `.cfg` files in your own `GameData/MyRingworldAddon` directory. The following nodes are read through KSP's GameDatabase; ModuleManager is optional, not required. Restart KSP after changing definitions. Do not edit a player's installed DLL.

## Architecture

- `Ringworld.Core/ExpeditionResearch.cs`: deterministic region identities and objective matching, without KSP or Unity dependencies.
- `Ringworld.KSP/ResearchCatalog.cs`: GameDatabase adapters for structures, custom sites, reward values and objectives.
- `Ringworld.KSP/RingScience.cs`: scoped stock-science integration and the public API.
- `Ringworld.KSP/ExpeditionJournal.cs`: delivered-data records, one-time stock career payouts and journal UI.
- `RingworldScenario`: per-save persistence. Rendering never awards research.

Existing structural placement comes from `RINGWORLD_LANDMARK_ASSET`, `RINGWORLD_COLOSSUS_ASSET` and `RINGWORLD_COLOSSUS_DISTRIBUTION`. The research catalog follows those same placement rules, including wet-ground exclusions. New templates therefore acquire science envelopes automatically. Optional fields on either asset node:

```text
scienceName = The Silent Archive
scienceMargin = 500
```

Fixed landmark assets also accept `scienceId`, which should be a globally unique, permanent identifier. Otherwise their identity derives from the parent site, asset kind and placement offsets. Colossi use their kind and deterministic grid cell: all copies of one model do not share one science pool. Changing template order or generation rules can change procedural identities and should be treated as a world-generation migration.

## Add a research site without adding geometry

```text
RINGWORLD_RESEARCH_SITE
{
    id = myaddon_silent_archive
    title = The Silent Archive
    site = city
    along = 20000
    across = 12000
    radius = 800
    minAltitude = 0
    maxAltitude = 5000
}
```

Distances are metres. `site` optionally anchors offsets to an existing named landmark; without it, `along` and `across` are absolute ring material coordinates. This defines a research envelope only; it does not spawn a building. Valid custom sites take precedence over structures and ordinary terrain, within the surface/atmospheric research region. The smallest overlapping custom radius wins. IDs are sanitized to letters, digits, underscores and hyphens; choose IDs that remain unique after sanitization. Keep IDs stable across updates.

## Science values

```text
RINGWORLD_SCIENCE_VALUE
{
    category = structure
    zone = surface
    multiplier = 18
}
```

Omit a filter to match any value. Categories: `biome`, `landmark`, `structure`, `wall`, `panel`, `orbit`. Zones: `surface`, `water`, `lowair`, `highair`, `space`, `highspace`. Matching nodes apply in database order, last match wins. Values must be finite and are bounded to 0.1–1,000 for numerical safety. The multiplier affects base return and the subject cap, without changing an experiment's stock transmission efficiency or availability mask. Existing earned science is retained when a value changes.

## Objectives

```text
RINGWORLD_OBJECTIVE
{
    id = myaddon_archaeology
    title = Read the builders' instruments
    description = Return gravity scans from two different structures.
    requires = first_contact
    category = structure
    experiment = gravityScan
    count = 2
    funds = 300000
    reputation = 25
}
```

Filters are optional and combined. An optional `zone` restricts the situation. `count` counts distinct location IDs, even if several experiments or packets arrive from one location. Rewards are nonnegative, paid once in Career through stock `Progression` transactions; Science mode records the milestone without currency. There is no additional science bonus on top of the submitted experiment.

Use unique objective IDs: first definition wins on duplicates. `requires` names one prerequisite objective; omit it for a root objective. Avoid missing IDs and dependency cycles, which leave an objective locked. Completion IDs persist even if an add-on is temporarily removed. Renaming an ID creates a different milestone and can award a new payout, so treat IDs as save-format keys.

## Result text

```text
RINGWORLD_SCIENCE_REPORT
{
    experiment = gravityScan
    category = structure
    text = The readings distinguish structural vibration from rotation-induced apparent weight.
}
```

The most specific matching definition wins (number of supplied experiment/category filters). For equal specificity, the last matching node wins. This changes Ringworld result prose only, not other celestial bodies.

## Opt-in C# API

Reference `NivenRingworld.dll`, `Ringworld.Core.dll` and KSP's usual assemblies. API calls must run on the game thread.

```csharp
Ringworld.Core.ResearchLocation location;
if (NivenRingworld.RingworldResearchApi.TryGetLocation(vessel, out location))
{
    // location.Id, Name, Category, Zone and Key identify the current context.
    // This is a detached snapshot; querying it does not grant science.
}

ScienceSubject subject;
if (NivenRingworld.RingworldResearchApi.TryGetSubject(
        vessel, ResearchAndDevelopment.GetExperiment("temperatureScan"), out subject))
{
    // Use the subject in your existing collection/transmission workflow.
    // This call registers a subject but does not award points or check your
    // instrument's crew, power, situation or transmission requirements.
}
```

Do not call the delivery API merely because a player views or stores a report. The journal listens to successful `ResearchAndDevelopment.SubmitScienceData` deliveries. Direct `AddScience` calls, generic lab science and old RW-1 subjects do not satisfy these research objectives.

## Validation

`dotnet run --project tests/Ringworld.Tests -c Release` exercises region boundaries, moving panel identities and distinct-location/prerequisite/idempotency rules. `./smoke-test.ps1 -ScienceOnly` runs an isolated Career fixture against real stock science and currency APIs, then restores the normal plugin. Use Slow or lower when testing on the development laptop. A standalone add-on should additionally test its IDs, overlap priorities, prerequisites and upgrade/save behaviour.

## Habitat identities

In v1.1.3 additional habitats prefix their stable `ringId` into research location IDs, preserving the original habitat's subject IDs. `RingworldSurfaceApi` v2 adds `RingId`, `RingName` and `Center`; use these instead of assuming every habitat is centered on HostBody.position. Ring definitions live in save-specific `RingworldScenario/RING` nodes. Duplicating the default NIVEN_RINGWORLD configuration does not spawn habitats.
