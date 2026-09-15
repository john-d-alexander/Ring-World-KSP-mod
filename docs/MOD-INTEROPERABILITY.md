# Reference frames and mod interoperability

## Decision: retain the ring-relative frame

On 2026-09-14 the user chose to retain the existing frame and improve interoperability. No native-frame adapter is enabled, and no changes are made to the Sun's rotation threshold.

Inspection of the installed KSP 1.12.5 assemblies confirmed `CelestialBody.inverseRotThresholdAltitude`. `OrbitPhysicsManager.checkReferenceFrame` compares spherical altitude above the dominant rotating body against that threshold. Switching calls `setRotatingFrame`, changes `inverseRotation`, and adjusts unpacked vessel velocities using the body's rotating-frame velocity. Centrifugal and Coriolis calculations also depend on that body flag. Local inspection outputs are development tools, not redistributed KSP source.

The ring already performs the analogous operation in `RingworldFlight`: arrival/exit converts loaded vessel positions, rotations and linear/angular velocities; saved vessel records carry frame state. Its cylindrical surface, rotation and surface acceleration cannot be represented by the Sun's spherical threshold. Changing that threshold alone would not implement a ring frame and could affect ordinary solar vessels.

## Read-only API, version 1

`RingworldSurfaceApi.TryGetSurfaceState(Vessel, out RingworldSurfaceState)` supplies surface-relative velocity, up direction, cylindrical location, terrain/water elevations, biome, frame epoch and physical tangential ring speed. Units are metres and seconds; vectors use the current Unity world axes. The API returns false for orbital/non-ring vessels, packed vessels and frame transitions. Call on the Unity main thread and refresh each physics tick. Do not use these rotating-frame velocities to construct stock Keplerian orbital elements.

This is an opt-in integration point for instruments and future adapters. It does **not** automatically adapt an autopilot, FAR, Principia or other third-party physics system. Those combinations remain unverified.

## Toolbar and game modes

The panel uses the stock `ApplicationLauncher.AddModApplication` lifecycle, with ready/destroy subscription and cleanup. It does not assign a toolbar slot or replace another mod's button. Alt+R stays synchronized with its selected state; F2 hides the window. The panel starts closed. This follows the same stock launcher pattern used by [MechJeb](https://github.com/MuMech/MechJeb2/blob/dev/MechJeb2/MechJebModuleMenu.cs). [ToolbarController](https://github.com/linuxgurugamer/ToolbarControl) is an optional ecosystem alternative, not a new dependency here.

Sandbox exposes expedition transports and world settings. Science/Career keep information, stock-warp status and photo mode, without relocation, training, forced frame exit or world-edit controls. Automatic physical arrival and exit still work in all modes. Physical tangential speed and vessel surface-relative speed are separately labeled.

## Graphics and content boundaries

Ring shaders, materials and bundles use ring-specific names. Existing scenery registration supports external `.mu` replacements ahead of bundled fallbacks. No EVE or Scatterer configuration is injected into the Sun: their body-based systems require dedicated adapters for this geometry. See [Scatterer](https://github.com/LGhassen/Scatterer) and [Kopernicus](https://github.com/Kopernicus/Kopernicus) for the respective source projects. Shared post-processing, camera ordering and alternate aerodynamic models still require installed-mod tests; no broad compatibility guarantee is implied.

## Multiple configured rings: design status

Multiple simultaneously landable rings are **not implemented**. The current flight controller, saved options and vessel frame records represent one ring; adding multiple `NIVEN_RINGWORLD` nodes does not create multiple habitats. Avoid publishing such a configuration as working.

A future catalog needs stable ring IDs, host-body names, individual geometry/seed/phase parameters, per-ring saved settings, and a ring ID in every vessel record. Scaled rendering can then instantiate each definition, while only one nearby ring owns the local physics chart at a time. Transfers must convert through an inertial state and preserve unloaded vessels. Overlapping capture volumes need deterministic selection and hysteresis. Save migration and interactions with third-party gravity solvers must be tested before enabling this feature. Kopernicus-style configurable definitions are a useful model, but its spherical body/SOI machinery does not directly supply these ring dynamics.
