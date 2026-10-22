# Universe — Sector Field (bounded)

**Status:** backlog - hub. This plan was split into two independently deliverable
slices. Each slice below is its own plan; this file only holds shared context and the
order.

**Framing:** [Space shaping](../gameplay/space-shaping-future.md). The universe is a
bounded, seeded field of systems that Map View pans; puzzles are authored
(editor-first, or from the puzzle seed) as systems inside it. There is no
round/hub/economy layer and no streaming here.

**Related:** [Bubble map drag](../../done/universe-transform.md) · [Silent planets](../done/universe-silent-planets.md) · [Color scope](../in-progress/universe-color-scope.md)

## Slices

| Order | Plan | Scope | Depends on |
|---|---|---|---|
| 1 | [View frame (bubble cube)](../../done/universe-bubble-window.md) | The frame moves, the universe does not: a cube `{F, viewRadius, yaw}` drives the view; `m = miniBubbleRadius / viewRadius`. Removes content reparenting. Owns frame culling (hide out-of-frame content). | Transform (done) |
| 2 | [Sector generation and extent](universe-sector-generation.md) | Pure C# bounded extent model, sector grid, and `GalaxyShape` math; tests. No scene. | 1 |

Why no streaming: the frame movement is bounded (`bubbleMoveLimit`), so the whole field is
spawned at level start and out-of-view content is hidden by the frame cube
([Bubble window](../../done/universe-bubble-window.md) §6.1).

## What is already done

The **decor-first slice** landed as [Silent planets](../done/universe-silent-planets.md)
(`SilentPlanetsSpawner` / `SilentPlanetLayout` / `SilentPlanetBehaviour` under
`Assets/_Game/Features/Decoration/`). It was a deliberate separate first slice; its pool
and behaviour should be migrated into the field's ambient system rather than kept
permanently.

## Shared decisions

| Topic | Decision |
|---|---|
| Exploration | The bubble/Map view is the god view; pan translates the view, aperture changes Map <-> Sculpt. |
| Ambient systems | Decor: pooled visuals with no gameplay until a puzzle uses them. |
| Puzzles | Authored (editor-first, or from the puzzle seed), not spawned by rounds. |
| Extent | A bounded field: a fixed extent around the origin, not infinite; the frame clamps inside it. |
| Visibility | Pre-spawn the whole field; hide/disable content outside the frame cube (AABB). No streaming, no floating-origin rebase. |
| Bounds | Soft operational bounds only (far clip, optional safety radius); no gameplay wall. |
| Determinism | Generation is versioned; a puzzle can remap its planets if the generation rules change. |

## Future: element pack (was Phase I4)

Belts/moons, then nebula billboards, stations, comets, gates, and black holes, in that
order. Each element gets a perf budget and a decor/gameplay classification before art.
Do not start with volumetrics or physics.

| Element | Role | Cost | Phase |
|---|---|---|---|
| Anchor planet (sun role) | System anchor/landmark | Low; existing planet prefab, no light | v1 (field) |
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
