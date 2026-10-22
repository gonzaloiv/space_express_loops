# Universe — Infinite Sector (split into slices)

**Status:** backlog - hub. This plan was split into four smaller, independently
deliverable slices. Each slice below is its own plan; this file only holds shared
context and the order.

**Related:** [Bubble map drag](../../done/universe-transform.md) · [Silent planets](../done/universe-silent-planets.md) · [Universe galaxies](universe-galaxies.md) · [Color scope](universe-color-scope.md)

## Slices

| Order | Plan | Scope | Depends on |
|---|---|---|---|
| 1 | [View frame (bubble cube)](universe-bubble-window.md) | The frame moves, the universe does not: a cube `{F, viewRadius, yaw}` drives the view; `m = miniBubbleRadius / viewRadius`. Removes content reparenting. | Transform (done) |
| 2 | [Sector generation and extent](universe-sector-generation.md) | Pure C# extent/focus/rebase model, sector grid, and `GalaxyShape` math; tests. No scene. | 1 |
| 3 | [Sector streaming](universe-sector-streaming.md) | Pooled streaming spawner, pan integration, floating-origin rebase, recenter, room-view ambient visibility, central gameplay galaxy. | 1, 2 |
| 4 | [Ambient round activation](universe-ambient-round-activation.md) | Rounds activate existing decor planets instead of spawning new ones; pool growth, intergalactic selection, persistence. | 3 |

[Universe galaxies](universe-galaxies.md) stays a separate plan and is still to be
analysed; it composes above the bubble transform and is not part of this split.

## What is already done

The **decor-first slice** landed as [Silent planets](../done/universe-silent-planets.md)
(`SilentPlanetsSpawner` / `SilentPlanetLayout` / `SilentPlanetBehaviour` under
`Assets/_Game/Features/Decoration/`). It was a deliberate separate first slice; its pool
and behaviour should be migrated into the ambient system in
[Sector streaming](universe-sector-streaming.md) rather than kept permanently.

## Shared decisions (from the original plan)

| Topic | Decision |
|---|---|
| Exploration | The bubble view is the god view; pinch switches views; drag translates the view. |
| Ambient planets | Decor first. Rounds activate already-spawned planets; rounds do not spawn new planets. |
| Extent | Effectively infinite: deterministic sectors stream around the focus without a room-centered edge. |
| Bounds | Soft operational bounds only (streaming budget, far clip, optional safety radius); no gameplay wall. |
| Room | The central gameplay galaxy stays anchored to the physical room; the room is not the universe bound. |
| Floating origin | Rebase content/focus at `originRebaseDistance` so precision stays stable. |
| Galaxy grouping | Planets carry a galaxy id; early rounds use the central galaxy, later rounds can select across galaxies. |
| Room-view visibility | Ambient decor fades by distance from the room surfaces; Bubble view disables that rule. |
| Determinism | Generation is versioned; saves can remap active planets if the generation rules change. |

## Future: element pack (was Phase I4)

Belts/moons, then nebula billboards, stations, comets, gates, and black holes, in that
order. Each element gets a perf budget and a decor/gameplay classification before art.
Do not start with volumetrics or physics.

| Element | Role | Cost | Phase |
|---|---|---|---|
| Anchor planet (sun role) | System anchor/landmark | Low; existing planet prefab, no light | v1 (streaming) |
| Planets | Decor, activatable for rounds | Low-medium; pooled prefab | v1 (streaming/activation) |
| Moons/rings | Planet variety | Low; child visuals | v1.5 |
| Asteroid belt/field | Visual ring, optional obstacle | Medium; instanced meshes | v1.5 |
| Nebula | Visual region | High if volumetric | v2 (billboards/particles only) |
| Station/outpost | Hub/trade variant | Medium; reuse hub flow | v2 |
| Comet/derelict | Rare event/resource | Medium; timed spawn | v2 |
| Wormhole/gate | Travel between regions | Medium; gameplay decision | v3 |
| Black hole | Hazard/landmark | Medium; VFX | v3 |

Suns are not a new element type: the anchor is a scaled planet using the existing planet
prefab/material, with no per-sun lights and no new shaders.
