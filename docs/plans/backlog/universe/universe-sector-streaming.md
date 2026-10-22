# Universe — Sector Streaming

**Status:** backlog - pooled streaming, pan integration, floating-origin rebase, recenter,
and room-view ambient visibility. Depends on [Bubble window](universe-bubble-window.md)
and [Sector generation](universe-sector-generation.md).

**Related:** [Infinite sector hub](universe-infinite-sector.md) · [Bubble map drag](../../done/universe-transform.md) · [Sector generation](universe-sector-generation.md) · [Ambient round activation](universe-ambient-round-activation.md) · [Color scope](universe-color-scope.md)

**Wiring rule:** ambient content uses pooled prefabs authored in the scene. No runtime
`AddComponent`, `GetComponent`, `FindObjectsByType`, generated meshes/materials, or
`new GameObject` presentation objects.

## 1. Goal

Fill an effectively infinite universe with deterministic, pooled content around the
bubble focus, and let the player pan it in the bubble god view.

- Sectors stream around the focus and rebase before float precision degrades.
- The physical room is the central gameplay region, not the universe edge.
- The bubble is a window over the focus, not the whole universe squeezed small.

## 2. Non-goals

- Simulating or persisting unvisited sectors; content exists only while streamed.
- A hard visible boundary wall, fog shell, or edge fade.
- A new pan gesture; [Bubble map drag](../../done/universe-transform.md) owns it.
- Full gameplay on ambient planets before a round activates them
  ([Ambient round activation](universe-ambient-round-activation.md)).
- Galaxy transform/layout propagation from [Universe galaxies](universe-galaxies.md).
- Volumetric nebulae, physics asteroid fields, real orbital mechanics, or runtime mesh
  generation.
- Replacing the current round/hub/ship flow beyond planet activation.

## 3. Spaces

| Space | Meaning |
|---|---|
| World (W) | Physical room, camera, hands. |
| Universe/content (C) | `Level` local. |
| Bubble view | A window over C at `MiniScale`, centered on F. |

The existing `Level` transform `D` still owns Room/Bubble scale and pose. Panning the
bubble translates the focus, not the content hierarchy.

## 4. Streaming

- Load sectors whose bounds intersect `spawnRadius`; unload beyond `cullRadius` with
  hysteresis (no per-frame thrash).
- The central sector contains the pre-spawned gameplay galaxy and is always loaded.
- Spawn/despawn at most a small budget per frame (e.g., one system per frame) to avoid
  hitches.
- Pooled prefabs only; no allocations after warmup; no LINQ.
- When `|F - origin| > originRebaseDistance`, shift the content root and `F` by the
  same offset; sector coordinates remain stable.

What is loaded:

| Content | Loaded | Persisted |
|---|---|---|
| Central gameplay galaxy, hubs, ships | Always | Yes |
| Ambient systems and planets | Around F, streamed | No, deterministic from seed |
| Round-activated ambient planets | Their sector plus state | Yes (ids + letter state) |
| Universe bounds/seed/focus | Always | Seed, generator version, and focus |

## 5. Placement

- Ambient systems are placed in `C` with a seeded jitter inside their sector; they do
  not use `MrukRoomLocalPlacement` and are not constrained to the room.
- The **central gameplay galaxy** is the exception: it is pre-spawned in the room
  footprint and its planets use room placement. It is always loaded and is not treated
  as an ambient system. Outer ambient systems do not overlap the room.
- Systems do not overlap: `sectorSize >= 2 * systemRadius + systemGap` plus a spacing
  check against systems in neighboring sectors.
- The anchor planet is a non-interactive landmark in v1: an ordinary planet visual, not
  a hub and not a letter source. No real-time light and no new shader.
- The galaxy/system root is a pooled prefab with an anchor planet and a fixed number of
  planet slots. Shape generation enables only the used slots. The same planet prefab is
  reused for anchors and members.

### 5.1 Central gameplay galaxy

- One galaxy is generated at level start inside the room footprint. It is the gameplay
  galaxy and the primary round pool; it is not an ambient system.
- Its planets use `MrukRoomLocalPlacement` and register as room occupants even while
  still decor, so hubs and later planets do not overlap them.
- The hub and ship spawn among the central planets; the round selector prefers this
  galaxy and guarantees at least two selectable active planets near the hub.
- The central galaxy has enough pre-spawned planets for the early rounds; the pool grows
  for later rounds rather than spawning one-off planets.

## 6. Recenter

The player needs a way back to the central gameplay region without leaving Bubble.

- **Trigger:** close both hands to the minimum scale (not a literal scale of 0, which is
  degenerate and tracking-sensitive) and hold for `recenterHoldSecs` (default `0.35 s`).
  Animate the focus back to the central galaxy/hub.
- **Alternative:** a double pinch on the bubble or a small authored home affordance if
  the hold gesture proves unreliable in the headset.
- **Also:** a round start anchored on the hub recenters the view on the hub/selected
  cluster; leaving Bubble resets on re-entry in the transform plan.
- **Room transition:** when far from the central galaxy, Bubble → Room returns to the
  hub/central gameplay region instead of mapping the remote focus into the room.
- Reset translation and yaw to canonical; keep the player's current view scale or return
  to the default scale. Add a short visual/haptic confirmation.

## 7. Room-view ambient visibility

- In **Room view**, ambient decor is visible near the room and fades out with distance
  from the room volume/surfaces. Beyond `roomSurfaceFade` (default `1.0 m`) it is hidden;
  within that distance it fades smoothly.
- In **Bubble view**, disable the room-distance rule; the window shows content normally.
- v1 uses the MRUK room bounds/AABB as the distance reference. Phase 4 uses wall/floor/
  ceiling anchors for exact surface distance if the AABB is too coarse.
- This is a per-object/sector visibility cull in C#, not a shader, and needs no new
  material. It also gives the future room color scope a natural boundary.
- The rule applies to ambient decor only; central gameplay content stays visible.

## 8. Bubble scale change

```text
MiniScale = miniBubbleRadius / viewRadius
```

See [Bubble window](universe-bubble-window.md). This supersedes the whole-content mini
scale in the two-views and transform plans. The bubble radius / color-scope sphere
becomes the view window, not the content bounds.

## 9. Performance

- Pooled authored prefabs; no runtime-created assets or hierarchies.
- Ambient planets: no letters, no UI, no colliders, no `PlanetStore.Update`; only visual
  rotation.
- Sector budget: soft budgets (16 systems / 64 ambient planets); the pool may grow; add
  LOD/occlusion/simplification later.
- In Room view, cull ambient decor by distance from the room surfaces.
- No real-time light per anchor; the main directional lighting is enough.
- No per-frame allocations, no LINQ, no `UnityEngine.Random` in per-frame code.
- Measure on the headset (current render scale 1.6, MSAA 4); watch active ambient count.
- Reuse the existing planet materials/meshes; do not create materials per planet.

## 10. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `UniverseSectorSpawner` | Sector grid, streaming, pooling, spawn budget, outer bound. | `DigitalLove.Game.Levels` |
| `UniverseRoomVisibility` | Ambient decor fade/hide by distance from the room surfaces in Room view. | `DigitalLove.Game.Levels` |
| `AmbientGalaxyBehaviour` | Pooled system root: anchor planet, planet slots, apply shape. | `DigitalLove.Game.Nodes` |

Modified:

| Type | Change |
|---|---|
| `UniverseView` | Bubble window: `miniBubbleRadius / viewRadius`, focus-based pose, expose `MiniScale`/bubble anchor. |
| `UniverseBounds` / `UniverseFocus` | From [Sector generation](universe-sector-generation.md); replace `bubbleMoveLimit` with the focus clamp. |
| `LevelContainer` | Spawn/restore the central galaxy and ambient registry; wire the spawner. |
| `Game.unity` | Universe bounds/focus root, ambient system/planet pools, spawner wiring. |

Assembly note: spawning/round flow in `DigitalLove.Game.Levels`, generation/data in
`DigitalLove.Game.Nodes`, bubble pan in the transform assembly. Do not make `Nodes` or
`Levels` reference the transform assembly.

## 11. Phases

### S1 — Ambient systems in the bubble window

- Add the ambient galaxy/planet prefabs and pools.
- Add `UniverseSectorSpawner` with a fixed debug focus (no pan).
- Pre-spawn the central gameplay galaxy in the room footprint, then a few nearby ambient
  sectors.
- Update `UniverseView` to the view-window scale ([Bubble window](universe-bubble-window.md)).
- No round changes yet.

**Exit:** the bubble shows a readable system/window; content streams around the focus;
decor planets have no letters, UI, or colliders.

### S2 — Pan, bounds, and streaming

- Integrate the bubble drag from [Bubble map drag](../../done/universe-transform.md).
- Replace `bubbleMoveLimit` with the sector/focus bounds; implement streaming hysteresis
  and floating-origin rebasing. No edge fade.
- Add the within-Bubble recenter (minimum-scale hold) and round-start recenter.
- Add `UniverseRoomVisibility`: fade/hide ambient decor by distance from the room
  surfaces in Room view.
- Stream/unstream sectors while panning; verify no hitches.
- Room view keeps the current room content and constraints.

**Exit:** the player can pan indefinitely, the origin rebases without visual jumps, and
content regenerates identically when panning back. Recenter returns to the central hub.

## 12. Test plan

1. Infinite extent: pan far beyond the room; the origin rebases without visual jumps, no
   edge appears, and content keeps streaming around the focus.
2. Bubble window: `MiniScale = miniBubbleRadius / viewRadius`; planets are readable.
3. Panning: focus moves, content streams in/out with hysteresis, and panning back
   regenerates identical systems from the seed.
4. Streaming budgets: soft budgets hold at the start; the pool grows with rounds without
   breaking spawn/despawn; no per-frame GC after warmup.
5. Ambient planets: no letters, no UI, no route selection, no collider/ray target;
   anchors have no light/new shader.
6. Recenter: the minimum-scale hold returns the bubble to the central galaxy/hub; round
   start recenters on the hub; a far Room transition returns to the central gameplay
   region.
7. Room-view visibility: ambient decor fades with distance from the room surfaces;
   Bubble view ignores the room-distance rule.
8. Room view: current hubs/ships/routes still work; ambient decor does not interfere.
9. Performance: headset frame time, draw calls, active system/planet counts, no GC.

## 13. Defaults

| Parameter | Default |
|---|---|
| `miniBubbleRadius` | `0.26 m` |
| `spawnBudgetPerFrame` | `1 system` |
| `recenterHoldSecs` | `0.35 s` |
| `roomSurfaceFade` | `1.0 m` |

## 14. Risks

| Risk | Mitigation |
|---|---|
| Float precision | Rebase the content root and focus at `originRebaseDistance`. |
| Wayfinding | No visible edge; recenter and round-start hub focus provide orientation. |
| Bubble scale | Use the view-window scale; coordinate with the transform plan before implementing. |
| Pan control | Owned by the transform plan; this plan exposes bounds/focus only. |
| Ambient performance | Soft budgets, no stores/UI/colliders, streaming hysteresis; LOD later. |
| Room-view visibility | Fade/hide by distance from the room surfaces; Bubble ignores the rule. |
| Boundary feel | No boundary object or edge fade; the universe continues with streamed sectors. |
| No runtime assets | Pooled authored prefabs; no generated meshes/materials/hierarchies. |
| Duplicate ambient systems | Migrate the silent-planets pool/behaviour into this system rather than keeping both. |
