# Universe — Element Pack

**Status:** idea · universe · updated 2026-10-26 — the ambient/decor element set for the sector field. Not scheduled.

**Related:** [integration](../plans/backlog/space-shaping-integration.md) · [progression](../design/systems/progression.md)

Belts/moons, then nebula billboards, stations, comets, gates, and black holes, in that order. Each
element gets a perf budget and a decor/gameplay classification before art. Do not start with
volumetrics or physics.

| Element | Role | Cost | Phase |
|---|---|---|---|
| Anchor planet (sun role) | Sector anchor/landmark | Low; existing planet prefab, no light | v1 (field) |
| Planets | Decor, activatable for a puzzle | Low-medium; pooled prefab | v1 (field) |
| Moons/rings | Planet variety | Low; child visuals | v1.5 |
| Asteroid belt/field | Visual ring, optional obstacle | Medium; instanced meshes | v1.5 |
| Nebula | Visual region | High if volumetric | v2 (billboards/particles only) |
| Station/outpost | Landmark; future demand layer | Medium; TBD | v2 |
| Comet/derelict | Rare event/resource | Medium; timed spawn | v2 |
| Wormhole/gate | Travel between regions | Medium; gameplay decision | v3 |
| Black hole | Hazard/landmark | Medium; VFX | v3 |

Suns are not a new element type: the anchor is a scaled planet using the existing planet
prefab/material, with no per-sun lights and no new shaders.
