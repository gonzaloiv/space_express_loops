# Universe — Infinite Sector Streaming + Round Planet Activation

**Status:** backlog - design updated for an effectively infinite, non-room-limited universe;
not implemented. Requires the view/transform work from
[Transforming the universe](../../done/universe-transform.md) for the bubble drag.

**Related:** [Two views + interaction unify](../../done/universe-views-and-interaction.md) ·
[Transforming the universe](../../done/universe-transform.md) · [Universe galaxies](universe-galaxies.md) ·
[Universe color scope](universe-color-scope.md)

**Wiring rule:** ambient content uses pooled prefabs authored in the scene. No runtime
`AddComponent`, `GetComponent`, `FindObjectsByType`, generated meshes/materials, or
`new GameObject` presentation objects.

## 1. Decisions from review

| Topic | Decision |
|---|---|
| Exploration | The bubble view is the god view. Pinching switches views; dragging the bubble translates the view. The drag gesture belongs to the universe-transform plan. |
| Ambient planets | Decor first. Rounds select already-spawned planets and add the letter-production system to them; rounds do not spawn new planets. |
| Extent | Effectively infinite: deterministic sectors stream around the focus without a room-centered edge. |
| Bounds | Soft operational bounds only (streaming budget, far clip, optional safety radius); no gameplay wall. |
| Room | The central gameplay galaxy stays anchored to the physical room; the room is not the universe bound. |
| Floating origin | Rebase content/focus when the focus exceeds `originRebaseDistance` so precision stays stable. |
| Shapes | Galaxies/systems are placed as shapes (rings, arms, clusters) around an anchor planet. |
| v1 elements | Anchor planets and planets first; belts, moons, nebula, stations, and events come later. |
| Galaxy grouping | Planets carry a galaxy id; early rounds use the central galaxy, later rounds can select across galaxies. |
| Recenter | Within Bubble, close the hands to the minimum scale and hold; the view returns to the central galaxy/hub. Also recenter on round start. Room transitions from far away return to the central gameplay region. |
| Pool growth | The ambient pool grows with rounds. Start with soft budgets; occlusion/simplification is future work. |
| Suns | Planet-like anchors using the existing planet prefabs/materials. No new shader and no per-sun light. |
| Room-view visibility | Ambient decor fades by distance from the room surfaces; Bubble view disables that rule. |
| Determinism | Generation is versioned; saves can remap active planets if the generation rules change. |
| Bounds | `UniverseBounds` is a soft safety/streaming extent, not room bounds; Room stays anchored to the central gameplay galaxy. |

## 2. Goal

Fill an effectively infinite universe with deterministic, pooled content and expose
more planets than the current round needs. The player explores it in the bubble god
view and the rounds reuse that content.

- An **effectively infinite sector stream** replaces the room-only placement for ambient
  content. Content is generated around the bubble focus and rebased before float
  precision degrades; the physical room is the central gameplay region, not the edge.
- **Galaxies/systems** are generated in shapes around an anchor planet, not as isolated
  planet-to-planet chains. The central galaxy is the gameplay galaxy; later rounds can
  select planets across galaxies.
- **Ambient planets** are cheap decor. A round selects existing ambient planets,
  activates their authored letter-production components, and leaves the rest as decor.
  The pool grows with the rounds.
- The **bubble view is a window** into the universe. Panning translates the focus
  within the bound; the bubble shows a readable local region, not the whole universe
  squeezed into 0.26 m.

## 3. Non-goals

- Simulating or persisting unvisited sectors; content exists only while streamed around the focus.
- A hard visible boundary wall, fog shell, or edge fade.
- A new pan gesture; [Transforming the universe](../../done/universe-transform.md) owns it.
- Full gameplay on ambient planets before a round activates them.
- Galaxy transform/layout propagation from [Universe galaxies](universe-galaxies.md);
  that plan remains the transform feature. This plan owns generation and streaming.
- Volumetric nebulae, physics asteroid fields, real orbital mechanics, or runtime
  mesh generation.
- Persisting every ambient prop; ambient content is deterministic from a seed.
- Replacing the current round/hub/ship flow beyond planet activation.

## 4. Core model

```text
Universe space (C) = Level local space, periodically rebased
Focus F            = C point shown at the bubble center
View window        = sphere of radius viewRadius around F
Bubble scale       = miniBubbleRadius / viewRadius
UniverseBounds     = soft safety/streaming extent, not room bounds
```

- The player pans F in C by dragging the bubble view; there is no room-centered edge.
- Sectors load around F out to the spawn radius and unload past the cull radius.
- When `|F|` exceeds `originRebaseDistance`, shift the content root and F by the same
  offset so world coordinates stay small; sector coordinates remain stable.
- The bubble renders content around F at a readable scale; content outside the view
  window is still rendered inside the cull radius and is grayed by the color scope.
- Room view keeps the central room-scale content; the bubble is the god view.

### 4.1 Bubble window change

The current `UniverseView` scales the entire content bounds into the bubble
(`miniBubbleRadius / contentRadius`). A large universe makes that unusable: planets
become microscopic.

This plan uses:

```text
MiniScale = miniBubbleRadius / viewRadius
Bubble pose = anchor - rotation * (MiniScale * focus)
```

- `viewRadius` defaults to `2.5 m` of universe space, and must cover the active
  gameplay cluster.
- `miniBubbleRadius` stays the authored bubble radius (`0.22-0.28 m`).
- The camera-relative bubble anchor is unchanged; only the content point placed at
  the anchor changes from the content center to the focus.
- This supersedes the whole-content mini scale in the two-views plan. Update that
  plan before implementing; the bubble radius/color-scope sphere should become the
  view window, not the content bounds.
- Bubble focus is per visit. Room stays anchored to the central gameplay galaxy; a far
  Bubble focus does not map into Room. Re-entering Bubble rebuilds the canonical
  front-of-player pose, matching the in-progress transform plan. A new round anchored on
  the hub also recenters the bubble on the hub/selected cluster (§8.3).
- Add an in-Bubble recenter action so the player can return to the central galaxy/hub
  without leaving Bubble (§5.4).

### 4.2 Spaces

| Space | Meaning |
|---|---|
| World (W) | Physical room, camera, hands. |
| Universe/content (C) | `Level` local. |
| Bubble view | A window over C at `MiniScale`, centered on F. |

The existing `Level` transform `D` still owns Room/Bubble scale and pose. Panning the
bubble is a translation of the focus, not a change to the content hierarchy.

## 5. Universe extent and floating origin

`UniverseBounds` (new, authored on the `Level` or `Universe` root) is a **soft safety
extent**, not a room bound:

```text
origin              = deterministic universe origin in C
safetyRadius        = 10 km (optional guard, not a gameplay wall)
viewRadius          = 2.5 m
originRebaseDistance = 500 m
```

Rules:

- There is no room-centered edge. The focus F can move indefinitely in sector space.
- `safetyRadius` is only a guard against runaway values and teleport bugs; it is not a
  clamp the player feels. If it is hit, recenter to the central hub instead of stopping.
- When `|F - origin| > originRebaseDistance`, rebase: shift `Level`/content positions and
  F by the same offset, keep sector coordinates unchanged, and continue. This keeps
  world float precision stable across long pans.
- Content streams around F; sectors outside the cull radius unload, and the same seed
  regenerates them when the player returns.
- No boundary mesh, fog shell, or edge fade. The universe has no visible edge.
- `UniverseBounds` replaces the transform plan's `bubbleMoveLimit` as the Bubble drag
  clamp. The transform plan must use the focus clamp instead; Room remains anchored to
  the central gameplay galaxy, not to the exploration focus.
- Panning back over previously visited space regenerates the same content from the
  seed unless round-activated state changed it.

**Why effectively infinite:** deterministic sector coordinates plus streaming and
rebasing give an unbounded map without storing or simulating unvisited space. The only
bounds are operational: spawn/cull radii, soft budgets, and the safety guard.

### 5.4 Recenter

The player needs a way back to the central gameplay region without leaving Bubble.

- **Trigger:** close both hands to the minimum scale (do not use a literal scale of 0;
  that is degenerate and tracking-sensitive) and hold for `recenterHoldSecs` (default
  `0.35 s`). Animate the focus back to the central galaxy/hub.
- **Alternative:** a double pinch on the bubble or a small authored home affordance if
  the hold gesture proves unreliable in the headset.
- **Also:** a round start anchored on the hub recenters the view on the hub/selected
  cluster (§8.3). Leaving Bubble resets on re-entry in the transform plan.
- **Room transition:** when the player is far from the central galaxy, Bubble → Room
  returns to the hub/central gameplay region instead of mapping the remote exploration
  focus into the room. Room is the gameplay anchor, not an infinite map.
- Reset translation and yaw to canonical; keep the player's current view scale or
  return to the default scale. Add a short visual/haptic confirmation.
- This is a tuning decision; test closed-hand tracking before committing to the
  minimum-scale trigger.

### 5.5 Room-view ambient visibility

Answer to the room-view visibility question: use distance from the room surfaces, not a
hard hide/show switch.

- In **Room view**, ambient decor is visible near the room and fades out as it gets
  farther from the room volume/surfaces. Beyond `roomSurfaceFade` (default `1.0 m`) it
  is hidden; within that distance it fades smoothly.
- In **Bubble view**, disable the room-distance rule; the bubble window shows the
  content normally.
- v1 uses the MRUK room bounds/AABB as the distance reference. If the AABB is too
  coarse, phase 4 uses the wall/floor/ceiling anchors for exact surface distance.
- This is a per-object/sector visibility cull in C#, not a shader, and does not require
  a new material. It also gives the future room color scope a natural boundary.
- The rule applies to ambient decor only; central gameplay content stays visible.

## 6. Sector streaming

### 6.1 Grid

```text
sectorSize   = 5 m                (>= 2 * maxSystemRadius + gap)
spawnRadius  = viewRadius + 3 m   (default 5.5 m)
cullRadius   = viewRadius + 5 m   (default 7.5 m)
density      = 0.55               (chance a sector contains a system)
softSystemBudget  = 16            (soft; the pool grows with rounds)
softPlanetBudget  = 64            (soft; add occlusion/simplification later)
```

- Sectors are deterministic cells in C; each sector has a seed derived from
  `worldSeed + sectorCoord`.
- Load sectors whose bounds intersect the spawn radius; unload beyond the cull radius
  with hysteresis (no per-frame thrash).
- The central sector contains the pre-spawned gameplay galaxy and is always loaded
  (§7.3).
- The system count and planet count are soft budgets, not hard caps. The pool grows
  with the rounds; LOD/occlusion/simplification is a later performance task.
- Enforce `sectorSize >= 2 * systemRadius + systemGap`, then run a spacing check
  against the systems in neighboring sectors before accepting a placement. If two
  systems would overlap, mark the sector empty or relocate the new system.
- Spawn/despawn at most a small budget per frame (e.g., one system per frame) to avoid
  hitches.
- Pooled prefabs only; no allocations after warmup; no LINQ.

### 6.2 What is loaded

| Content | Loaded | Persisted |
|---|---|---|
| Central gameplay galaxy, hubs, ships | Always | Yes |
| Ambient systems and planets | Around F, streamed | No, deterministic from seed |
| Round-activated ambient planets | Their sector plus state | Yes (ids + letter state) |
| Universe bounds/seed/focus | Always | Seed, generator version, and focus in the snapshot |

## 7. Galaxy/system shapes

### 7.1 Data

`GalaxyShapeData` (authored or generated per system):

```text
shape             Ring | Spiral | Cluster | Binary (future)
anchorScale       1.2-2.5x a normal planet radius (the "sun" role)
radius            system radius at scale 1 (e.g. 1.2-2.0 m)
planetCount       2-6
planetRadius      MinMaxFloat (matches the current 0.15-0.35 m)
ringCount         1-3 (Ring)
armCount          2-3 (Spiral)
spread            fraction of radius
yaw               random Y rotation
inclination       small vertical jitter
minSeparation     planet radius multiplier
```

The anchor is an ordinary planet visual, scaled up for the sun role. There is no
separate sun prefab/shader and no per-sun light.

`GalaxyShape` is a pure C# generator returning local positions, radii, and an anchor
pose. It is seeded and testable; no Unity random or scene iteration.

### 7.2 Shapes

- **Ring:** 1-3 concentric rings around the anchor planet; planets evenly spaced with
  jitter. Default for v1.
- **Spiral:** 2-3 logarithmic arms; planets sampled along the arms with jitter.
  Second v1 shape.
- **Cluster:** Poisson-disc-ish placement in a flattened sphere; fallback for dense
  sectors.
- **Binary:** two anchor planets with a barycenter; future.
- **Satellite:** planets/moons around a large anchor planet; future.

All shapes keep planets outside the anchor radius and enforce minimum separation.

### 7.3 Placement

- Ambient systems are placed in `C` with a seeded jitter inside their sector; they do
  not use `MrukRoomLocalPlacement` and are not constrained to the room.
- The **central gameplay galaxy** is the exception: it is pre-spawned in the room
  footprint and its planets use room placement. It is always loaded and is not treated
  as an ambient system. Outer ambient systems do not overlap the room.
- Systems do not overlap: `sectorSize >= 2 * systemRadius + systemGap`, plus a spacing
  check against systems in neighboring sectors. If the check fails, treat the sector
  as empty or relocate the system.
- The anchor planet is a non-interactive landmark in v1: an ordinary planet visual,
  not a hub and not a letter source. No real-time light and no new shader.
- The galaxy/system root is a pooled prefab with an anchor planet and a fixed number
  of planet slots. Slot count is the authored maximum; shape generation enables only
  the used slots. The same planet prefab is reused for anchors and members.

### 7.4 Central gameplay galaxy

- One galaxy is generated at level start inside the room footprint. It is the
  **gameplay galaxy** and the primary round pool; it is not an ambient system.
- Its planets use `MrukRoomLocalPlacement` and are registered as room occupants even
  while they are still decor, so hubs and later planets do not overlap them.
- The hub and ship spawn among the central planets; the round selector prefers this
  galaxy and guarantees at least two selectable active planets near the hub.
- Outer systems are ambient decor. Later rounds can select across galaxies once the
  pool/interaction supports it (`galaxyId`, `galaxiesToActivate`/intergalactic flag).
- The central galaxy has enough pre-spawned planets for the early rounds; the pool
  grows for later rounds rather than spawning one-off planets.

## 8. Ambient planets

### 8.1 Decor first

Ambient planets are pre-spawned decor:

- no letters, no resource panel, no route selection, no destination ray target;
- visual rotation and the existing planet look only;
- no collider (or a non-interactive layer) until activated;
- the authored `PlanetStore`/`ResourcePanel` components exist on the prefab but are
  disabled, so activation never uses `AddComponent`.

Use the existing planet prefab as the shared visual. Keep the per-planet components
disabled for ambient planets; activation enables them.

Prefab check: `Body.prefab` is visual-only today (no `PlanetStore`/`ResourcePanel`).
The ambient prefab must be the full planet prefab with the letter components authored
and disabled. If it is only the visual prefab, activation cannot add the letter system
without `AddComponent`, which is not allowed. Settle this before Phase I3.

Decor readability: ambient planets use the normal planet look but no letters, no
highlight, and no rays. An activated planet gains its panel and collider, so the
transition from decor to gameplay is visible.

### 8.2 Activation

A round selects `N` inactive ambient planets and activates their letter production:

```text
Select N planets from the ambient registry
  -> prefer planets inside the bubble view window around the hub/room
  -> then nearest loaded systems; recenter the view on the chosen cluster
  -> avoid already-active/on-route planets and planets too close to the hub
ActivateForRound(planet, lettersPerMinute, maxLetters)
  -> enable PlanetStore and its panel
  -> PrepareStoring + ShowLetters
  -> enable the collider/layer for destination rays
  -> mark IsGameplayActive = true
```

- `PlanetBehaviour` gains `IsGameplayActive`; `PlanetSelectionRules` requires it.
- `PlanetBehaviour.Spawn` keeps the current required-content path; add a separate
  `SpawnAmbient`/`ActivateForRound` path. Do not reuse the round spawn path.
- The resource panel is pulled from a small authored panel pool, not one Canvas per
  ambient planet. Assign it on activation; return it on deactivation.
- `PlanetStore.Update` is disabled for ambient planets (component disabled), so decor
  costs no per-frame letter work.
- `ResourcePanel`/outline are inactive for decor.

### 8.3 Round selection

`RoundData`:

- replace `planetsToAdd` with `planetsToActivate` (keep the old field as a fallback
  for existing assets and migration);
- keep `planetSeed` for the letter rates and max letters;
- add `galaxiesToActivate`/`intergalacticAfterRound` (or a `selectionMode`) so early
  rounds use the central galaxy and later rounds can select across galaxies;
- `distanceBetweenPlanetsMultiplier` becomes legacy/unused for round spawning; the
  shape generator owns spacing;
- `shouldSpawnSpaceship`, `resetsLetters`, `hasLoseCondition` keep their meaning.

Selection rules:

- the hub is the route origin, so prefer planets inside the bubble view window around
  the **hub/room**, not an arbitrary far exploration focus;
- if the player has panned away, recenter the bubble view on the hub when the round
  starts, or select a visible cluster and recenter on it;
- if not enough planets are in the window, expand to the nearest loaded systems;
- avoid planets already active, on a route, or too close to the hub;
- early rounds select from the central gameplay galaxy; later rounds may select
  across galaxies (`galaxyId`) as the pool grows; routes can span galaxies;
- the pool grows with rounds, so exhaustion is not expected; if a round cannot find
  enough inactive planets, reuse the least-recently activated planet and reset its
  letters rather than failing the round;
- guarantee at least two selectable active planets near the hub for the current
  route flow.

The round assigns each selected planet's `lettersPerMinute` and `maxLetters` from the
round's `planetSeed`, then calls `BeginStoring` through the existing unlock step.

### 8.4 Identity and persistence

Ambient planet ids are deterministic:

```text
planetId = worldSeed + ":" + galaxyId + ":" + slotIndex
```

- Store the universe seed and the current focus in `GameSnapshot`.
- Store only round-activated planets (id, letters, maxLetters, active state) and the
  existing hub/ship/loop data; ambient decor is not persisted.
- On restore: regenerate the universe from the seed, stream the sectors around the
  stored focus, resolve the active ids, and activate them.
- If an active id is outside the loaded sectors, load its sector explicitly or fall
  back to the stored position.
- Keep the current snapshot fields for compatibility; add `schemaVersion`, `worldSeed`,
  `generatorVersion`, `universeFocus`, and the ambient activation fields.

**Why the generator version matters:** the universe is generated from `worldSeed`.
If the generation code changes (shape rules, planet counts, ordering), the same seed
produces different ids/positions. A save that lists active planet ids can then point
to the wrong planet or to one that no longer exists. Store `generatorVersion` and
either keep the old rules for old saves or regenerate and remap the saved active
planets by their stored position. This matters before shipping, not during the
prototype.

## 9. Universe elements

The shape system needs a small set of element types. Recommended order:

| Element | Role | Cost | Phase |
|---|---|---|---|
| Anchor planet (sun role) | System anchor/landmark | Low; existing planet prefab, no light | v1 |
| Planets | Decor, activatable for rounds | Low-medium; pooled prefab | v1 |
| Moons/rings | Planet variety | Low; child visuals | v1.5 |
| Asteroid belt/field | Visual ring, optional obstacle | Medium; instanced meshes | v1.5 |
| Nebula | Visual region; maybe route modifier | High if volumetric | v2 (billboards/particles only) |
| Station/outpost | Hub/trade variant | Medium; reuse hub flow | v2 |
| Comet/derelict | Rare event/resource | Medium; timed spawn | v2 |
| Wormhole/gate | Travel between regions; edge landmark | Medium; gameplay decision | v3 |
| Black hole | Hazard/landmark | Medium; VFX | v3 |

Do not start with volumetrics or physics. A belt can be a ring of instanced low-poly
rocks with no colliders; a nebula can be a billboard/particle volume or a tinted fog
region. Stations should reuse the hub content and route flow rather than inventing a
second station system.

Suns are not a new element type: the anchor is a scaled planet using the existing
planet prefab/material. There are no per-sun lights and no new shaders.

## 10. Performance

- Pooled authored prefabs; no runtime-created assets or hierarchies.
- Ambient planets: no letters, no UI, no colliders, no `PlanetStore.Update`; only
  visual rotation.
- Active planets: the round's selection plus existing active planets; the pool grows
  with rounds.
- Sector budget: start with soft budgets (16 systems / 64 ambient planets), not hard
  caps. The pool is allowed to grow; add LOD/occlusion/simplification as a later
  performance task.
- LOD/culling: disable distant planet renderers beyond the cull radius; the bubble
  window is small, so most content is outside it. In Room view, cull ambient decor by
  distance from the room surfaces (§5.5).
- Suns/anchors: no real-time light per anchor; the directional/main lighting is enough.
- No per-frame allocations, no LINQ, no `UnityEngine.Random` in per-frame code.
- Measure on the headset; the current render scale is 1.6 and MSAA 4, so watch the
  active ambient count before adding VFX-heavy elements.
- Reuse the existing planet materials/meshes; do not create materials per planet.

## 11. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `UniverseBounds` | Soft safety extent, rebase trigger, sector-grid origin; no gameplay wall. | `DigitalLove.Game.Levels` |
| `UniverseFocus` | Focus point in C, view radius, bubble pose handoff. | `DigitalLove.Game.Levels` |
| `UniverseSectorSpawner` | Sector grid, streaming, pooling, spawn budget, outer bound. | `DigitalLove.Game.Levels` |
| `UniverseRoomVisibility` | Ambient decor fade/hide by distance from the room surfaces in Room view. | `DigitalLove.Game.Levels` |
| `GalaxyShapeData` | Shape/seed data for a system. | `DigitalLove.Game.Nodes` |
| `GalaxyShape` | Pure seeded layout generator (positions, radii, anchor). | `DigitalLove.Game.Nodes` |
| `AmbientGalaxyBehaviour` | Pooled system root: anchor planet, planet slots, apply shape. | `DigitalLove.Game.Nodes` |
| `AmbientPlanetBehaviour` | Decor/activation state for a pooled planet. | `DigitalLove.Game.Nodes` |
| `AmbientPlanetRegistry` | Deterministic ids, sectors, loaded planets, lookup. | `DigitalLove.Game.Nodes` |
| `RoundPlanetSelector` | Pick inactive ambient planets for a round. | `DigitalLove.Game.Levels` |
| `AmbientPanelPool` | Small pool of world-space resource panels. | `DigitalLove.Game.Levels` |

Modified:

| Type | Change |
|---|---|
| `UniverseView` | Bubble window: `miniBubbleRadius / viewRadius`, focus-based pose, expose `MiniScale`/bubble anchor; no whole-content mini scale. |
| `UniverseTransform` (in progress) | Replace `bubbleMoveLimit` with the sector/focus bounds; add the within-Bubble recenter; keep Room anchored to the central galaxy and reset-on-reentry. |
| `LevelContainer` | Spawn/restore ambient registry; round activation instead of planet spawn. |
| `PlanetsSpawner` | Keep required planets; add ambient spawn/activation APIs or split into an ambient spawner. |
| `PlanetBehaviour` / `PlanetRefs` | `IsGameplayActive`, `SpawnAmbient`, `ActivateForRound`, `Deactivate`, panel assignment, collider/layer toggle. |
| `PlanetStore` / `ResourcePanel` | Enable only for active planets; panel pool. |
| `PlanetSelectionRules` | Require `IsGameplayActive`. |
| `RoundData` | `planetsToActivate`, `galaxiesToActivate`/intergalactic flag; keep `planetsToAdd` fallback; deprecate distance multiplier. |
| `GameSnapshot` | `schemaVersion`, `worldSeed`, `generatorVersion`, `universeFocus`, active ambient planet ids/letters. |
| `MrukRoomLocalPlacement` | Required/round content only; ambient content does not register room occupants. |
| `NewRoundState` | Round selection/activation instead of spawning new planets. |
| `Game.unity` | Universe bounds/focus root, ambient system/planet pools, panel pool, spawner wiring. |

Assembly note: generation/data in `DigitalLove.Game.Nodes`, spawning/round flow in
`DigitalLove.Game.Levels`, and the bubble pan in the universe-transform assembly. Do
not make `Nodes` or `Levels` reference the transform assembly.

## 12. Phases

### Phase I0 - Bounds, seed, and shape math

- Add `UniverseBounds` (soft safety extent + rebase trigger), `UniverseFocus`, sector coordinates, deterministic PRNG.
- Add `GalaxyShapeData`/`GalaxyShape` with Ring, Spiral, Cluster.
- Add edit-mode tests/preview for shape positions, min separation, and determinism.
- No scene spawning yet.

**Exit:** seeded shapes are stable, non-overlapping, and fit their sector.

### Phase I1 - Ambient systems in the bubble window

- Add the ambient galaxy/planet prefabs and pools.
- Add `UniverseSectorSpawner` with a fixed debug focus (no pan).
- Pre-spawn the central gameplay galaxy in the room footprint, then a few nearby
  ambient sectors.
- Update `UniverseView` to the view-window scale.
- No round changes yet.

**Exit:** the bubble shows a readable system/window; content streams around the focus;
decor planets have no letters, UI, or colliders.

### Phase I2 - Pan, bounds, and streaming

- Integrate the bubble drag from [Transforming the universe](../../done/universe-transform.md).
- Replace `bubbleMoveLimit` with the sector/focus bounds; implement streaming
  hysteresis and floating-origin rebasing. There is no edge fade.
- Add the within-Bubble recenter (minimum-scale hold) and round-start recenter.
- Add `UniverseRoomVisibility`: fade/hide ambient decor by distance from the room
  surfaces in Room view.
- Stream/unstream sectors while panning; verify no hitches.
- Room view keeps the current room content and constraints.

**Exit:** the player can pan indefinitely, the origin rebases without visual jumps, and
content regenerates identically when panning back. Recenter returns to the central hub.

### Phase I3 - Ambient planets and round activation

- Add `AmbientPlanetRegistry`, `RoundPlanetSelector`, and the panel pool.
- Add `planetsToActivate`, `galaxiesToActivate`/intergalactic selection to `RoundData`
  and migrate the round assets.
- Add `PlanetBehaviour` activation/deactivation and `IsGameplayActive`.
- Update selection rules and `LevelLoopPlanetAvailability`.
- Implement the growing pool and intergalactic selection.
- Update the snapshot/restore path, including `generatorVersion`.

**Exit:** a round activates existing ambient planets instead of spawning planets; the
pool grows; later rounds can select across galaxies; letters start; routes see only
active planets; restore returns the same active set.

### Phase I4 - More elements

- Add belts/moons, then nebula billboards, stations, comets, gates, and black holes in
  the order in §9.
- Each element gets a perf budget and a gameplay/decor classification before art.

### Phase I5 - Tuning

- Tune universe radius, view radius, sector size, density, caps, and the edge feel.
- Balance round selection (nearby vs. explore), reuse, and letter rates.
- Profile on the headset; verify no GC and stable frame time.

## 13. Test plan

1. Infinite extent: pan far beyond the room; the origin rebases without visual jumps,
   no edge appears, and content keeps streaming around the focus.
2. Bubble window: `MiniScale = miniBubbleRadius / viewRadius`; planets are readable;
   the bubble shows a local region, not the whole universe.
3. Panning: focus moves, content streams in/out with hysteresis, and panning back
   regenerates identical systems from the seed.
4. Shapes: Ring/Spiral/Cluster produce the expected counts; no planet overlaps the sun
   or another planet; same seed -> same layout.
5. Streaming budgets: soft system/planet budgets hold at the start; the pool grows
   with rounds without breaking spawn/despawn; no per-frame GC after warmup.
6. Ambient planets: no letters, no UI, no route selection, no collider/ray target;
   anchors have no light/new shader.
7. Round activation: selects N existing planets; no new planet is spawned; letters
   start; UI appears; only active planets are selectable.
8. Selection near the hub: the round prefers visible planets; if it expands, the view
   recenters or the UI locates them.
9. Intergalactic: later rounds can select planets from more than one galaxy and routes
   still resolve.
10. Growth: the pool grows across rounds; reuse/reset is only a fallback and never
    fails a round.
11. Recenter: the minimum-scale hold returns the bubble to the central galaxy/hub;
    round start recenters on the hub; a far Room transition returns to the central
    gameplay region, and re-entry resets as before.
12. Room-view visibility: ambient decor fades with distance from the room surfaces;
    Bubble view ignores the room-distance rule.
13. Restore: universe seed/generator version/focus/active ids reproduce the same
    content and letters; old snapshots still load.
14. Room view: current hubs/ships/routes still work; ambient decor does not interfere.
15. Performance: headset frame time, draw calls, active system/planet counts, no GC.

## 14. Risks and decisions

| Risk / decision | Choice / mitigation |
|---|---|
| Literal infinity | Effectively infinite via deterministic sector streaming and floating-origin rebasing; no hard outer bound. |
| Float precision | Rebase the content root and focus at `originRebaseDistance`; sector coordinates stay stable. |
| Wayfinding | No visible edge; the recenter action and round-start hub focus provide orientation. |
| Bubble scale | Switch from whole-content mini scale to a `viewRadius` window. The in-progress transform plan currently drags the whole-content bubble; coordinate the change before implementing. |
| Pan control | Owned by [Transforming the universe](../../done/universe-transform.md); this plan exposes bounds/focus only. Replace `bubbleMoveLimit` with the `UniverseBounds` clamp. |
| Recenter | Minimum-scale hold (or a double pinch/home affordance) returns to the canonical bubble; round start recenters on the hub. |
| Central gameplay | Pre-spawn a central gameplay galaxy in the room footprint; ambient systems stay outside. This resolves the "no pre-spawned planets near the hub" gap. |
| Ambient gameplay | Decor first; rounds activate authored letter components. No `AddComponent`. |
| Round planets | Select existing planets; early rounds central, later rounds intergalactic; keep `planetsToAdd` fallback. |
| Pool growth | The pool grows with rounds; start with soft budgets. Reuse/reset is a fallback, not the plan. |
| Ambient performance | Soft budgets, no stores/UI/colliders, streaming hysteresis; add LOD/occlusion/simplification later. |
| Panel cost | Small authored panel pool, not one Canvas per ambient planet. |
| Suns | Anchor planets use the existing planet prefab/material; no new shader, no per-sun light. |
| Room-view visibility | Fade/hide ambient decor by distance from the room surfaces; Bubble ignores the rule. |
| Persistence | Seed, generator version, focus, and active ids only; ambient regenerates; snapshot migration. |
| Determinism | Save a `generatorVersion`; keep old rules or remap active planets by position when generation changes. |
| Galaxy plan overlap | Reuse its galaxy data/transform later; do not build two galaxy systems. This plan owns procedural creation/streaming. |
| Color scope | Bubble sphere becomes the view window; update the color-scope plan's center/radius. |
| Boundary feel | No boundary object or edge fade; the universe continues with streamed sectors. |
| Element scope | Anchor planets/planets first; belts/moons next; avoid volumetrics/physics early. |
| No runtime assets | Pooled authored prefabs; no generated meshes/materials/hierarchies. |

## 15. Defaults

| Parameter | Default |
|---|---|
| `safetyRadius` | `10 km` (guard only, not a wall) |
| `originRebaseDistance` | `500 m` |
| `viewRadius` | `2.5 m` (must cover the active cluster) |
| `miniBubbleRadius` | `0.26 m` |
| `sectorSize` | `5 m` (`>= 2 * maxSystemRadius + gap`) |
| `spawnRadius` | `5.5 m` |
| `cullRadius` | `7.5 m` |
| `sectorDensity` | `0.55` |
| `softSystemBudget` | `16` |
| `softPlanetBudget` | `64` |
| `planetsPerSystem` | `2-6` |
| `systemRadius` | `1.2-2.0 m` |
| `planetRadius` | `0.15-0.35 m` |
| `anchorScale` | `1.2-2.5x` planet radius |
| `planetsToActivate` | `RoundData` 2-4, growing with rounds |
| `intergalacticAfterRound` | tuned per level |
| `recenterHoldSecs` | `0.35 s` |
| `roomSurfaceFade` | `1.0 m` |
| `generatorVersion` | `1` |

| `spawnBudgetPerFrame` | `1 system` |

## 16. Future

- Binary systems, satellite systems around large anchor planets, and moons/rings.
- Asteroid belts, nebula billboards, stations/outposts, comets, wormholes, black holes.
- Galaxy transform/layout propagation from [Universe galaxies](universe-galaxies.md).
- LOD/occlusion/simplification for the growing ambient pool.
- Chunked persistence of modified ambient planets if ambient gameplay expands.
- Chunked persistence or simulation of distant visited sectors, if the map ever needs memory beyond deterministic regeneration.
