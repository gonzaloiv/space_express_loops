# Universe — Infinite Sector (Finite Bounds) + Round Planet Activation

**Status:** backlog - design ready, not implemented. Requires the view/transform work from
[Transforming the universe](universe-transform.md) for the bubble drag.

**Related:** [Two views + interaction unify](../done/universe-views-and-interaction.md) ·
[Transforming the universe](universe-transform.md) · [Universe galaxies](universe-galaxies.md) ·
[Universe color scope](universe-color-scope.md)

**Wiring rule:** ambient content uses pooled prefabs authored in the scene. No runtime
`AddComponent`, `GetComponent`, `FindObjectsByType`, generated meshes/materials, or
`new GameObject` presentation objects.

## 1. Decisions from review

| Topic | Decision |
|---|---|
| Exploration | The bubble view is the god view. Pinching switches views; dragging the bubble translates the view. The drag gesture belongs to the universe-transform plan. |
| Ambient planets | Decor first. Rounds select already-spawned planets and add the letter-production system to them; rounds do not spawn new planets. |
| Extent | Finite, not literally infinite. A large outer bound stops the view. |
| Shapes | Galaxies/systems are placed as shapes (rings, arms, clusters) around a sun/planet anchor. |
| v1 elements | Suns and planets first; belts, moons, nebula, stations, and events come later. |

## 2. Goal

Fill a large but finite universe with deterministic, pooled content and expose more
planets than the current round needs. The player explores it in the bubble god view
and the rounds reuse that content.

- A finite **universe bound** replaces the current room-only placement for ambient
  content.
- **Galaxies/systems** are generated in shapes around a central sun (or, later, a
  planet), not as isolated planet-to-planet chains.
- **Ambient planets** are cheap decor. A round selects existing ambient planets,
  activates their authored letter-production components, and leaves the rest as decor.
- The **bubble view is a window** into the universe. Panning translates the focus
  within the bound; the bubble shows a readable local region, not the whole universe
  squeezed into 0.26 m.

## 3. Non-goals

- A literally unbounded universe.
- A new pan gesture; [Transforming the universe](universe-transform.md) owns it.
- Full gameplay on ambient planets before a round activates them.
- Galaxy transform/layout propagation from [Universe galaxies](universe-galaxies.md);
  that plan remains the transform feature. This plan owns generation and streaming.
- Volumetric nebulae, physics asteroid fields, real orbital mechanics, or runtime
  mesh generation.
- Persisting every ambient prop; ambient content is deterministic from a seed.
- Replacing the current round/hub/ship flow beyond planet activation.

## 4. Core model

```text
Universe space (C) = Level local space
UniverseBounds     = finite sphere around the room center in C
Focus F            = C point shown at the bubble center
View window        = sphere of radius viewRadius around F
Bubble scale       = miniBubbleRadius / viewRadius
```

- The player pans F in C by dragging the bubble view.
- F is clamped so the whole view window stays inside `UniverseBounds`.
- The bubble renders content around F at a readable scale; content outside the view
  window is still rendered inside the cull radius and is grayed by the color scope.
- Room view keeps the current room-scale content; the bubble is the god view.

### 4.1 Bubble window change

The current `UniverseView` scales the entire content bounds into the bubble
(`miniBubbleRadius / contentRadius`). A large universe makes that unusable: planets
become microscopic.

This plan uses:

```text
MiniScale = miniBubbleRadius / viewRadius
Bubble pose = anchor - rotation * (MiniScale * focus)
```

- `viewRadius` defaults to `2.0 m` of universe space.
- `miniBubbleRadius` stays the authored bubble radius (`0.22-0.28 m`).
- The camera-relative bubble anchor is unchanged; only the content point placed at
  the anchor changes from the content center to the focus.
- This supersedes the whole-content mini scale in the two-views plan. Update that
  plan before implementing; the bubble radius/color-scope sphere should become the
  view window, not the content bounds.
- Keep the bubble focus per session: leaving and re-entering Bubble resumes the last
  focus. Room view stays the room pose. A new round anchored on the hub recenters the
  bubble on the hub/selected cluster (§8.3).

### 4.2 Spaces

| Space | Meaning |
|---|---|
| World (W) | Physical room, camera, hands. |
| Universe/content (C) | `Level` local. |
| Bubble view | A window over C at `MiniScale`, centered on F. |

The existing `Level` transform `D` still owns Room/Bubble scale and pose. Panning the
bubble is a translation of the focus, not a change to the content hierarchy.

## 5. Universe bounds and the outer limit

`UniverseBounds` (new, authored on the `Level` or `Universe` root):

```text
center          = room center in C
radius          = 20 m (default)
viewRadius      = 2 m
edgeFadeRadius  = 3 m
focusMargin     = viewRadius
```

Rules:

- `|F - center| <= radius - viewRadius`; clamp the focus before applying the bubble
  pose. The player cannot pan the window past the universe edge.
- The bound is a hard gameplay limit, felt as a stop, not a wall the player can cross.
- Content density and brightness fade over `edgeFadeRadius` so the edge reads as a
  horizon; do not spawn a new boundary mesh at runtime.
- The boundary visual is an authored backdrop/fog shell or a star-density falloff,
  pooled with the ambient content. No runtime asset creation.
- `UniverseBounds` also provides the clamp for the universe-transform plan in Bubble
  view. In Room view, the existing room constraints remain.
- Panning back over previously visited space regenerates the same content from the
  seed unless round-activated state changed it.

**Why finite:** a deterministic seed with a finite radius is testable, performant, and
lets the player exhaust the content in a controlled way. It is "infinite" in the sense
of generated space, not unbounded.

## 6. Sector streaming

### 6.1 Grid

```text
sectorSize   = 3 m
spawnRadius  = viewRadius + 3 m   (default 5 m)
cullRadius   = viewRadius + 5 m   (default 7 m)
density      = 0.55               (chance a sector contains a system)
maxActiveSystems = 12
maxActiveAmbientPlanets = 48
```

- Sectors are deterministic cells in C; each sector has a seed derived from
  `worldSeed + sectorCoord`.
- Load sectors whose bounds intersect the spawn radius; unload beyond the cull radius
  with hysteresis (no per-frame thrash).
- The central sector contains the room/round content and is always loaded.
- Keep the system count and planet count under the caps; empty sectors are normal.
- Spawn/despawn at most a small budget per frame (e.g., one system per frame) to avoid
  hitches.
- Pooled prefabs only; no allocations after warmup; no LINQ.

### 6.2 What is loaded

| Content | Loaded | Persisted |
|---|---|---|
| Required round planets/hubs/ships | Always (current flow) | Yes |
| Ambient systems and planets | Around F, streamed | No, deterministic from seed |
| Round-activated ambient planets | Their sector plus state | Yes (ids + letter state) |
| Universe bounds/seed/focus | Always | Seed and focus in the snapshot |

## 7. Galaxy/system shapes

### 7.1 Data

`GalaxyShapeData` (authored or generated per system):

```text
shape            Ring | Spiral | Cluster | Binary (future)
anchorKind       Sun | Planet (Planet = future)
radius           system radius at scale 1 (e.g. 1.2-2.0 m)
planetCount      2-6
planetRadius     MinMaxFloat (matches the current 0.15-0.35 m)
ringCount        1-3 (Ring)
armCount         2-3 (Spiral)
spread           fraction of radius
yaw              random Y rotation
inclination      small vertical jitter
minSeparation    planet radius multiplier
```

`GalaxyShape` is a pure C# generator returning local positions, radii, and an anchor
pose. It is seeded and testable; no Unity random or scene iteration.

### 7.2 Shapes

- **Ring:** 1-3 concentric rings around the sun; planets evenly spaced with jitter.
  Default for v1.
- **Spiral:** 2-3 logarithmic arms; planets sampled along the arms with jitter.
  Second v1 shape.
- **Cluster:** Poisson-disc-ish placement in a flattened sphere; fallback for dense
  sectors.
- **Binary:** two suns with a barycenter; future.
- **Planet anchor:** a large planet at the center with moons/satellites; future.

All shapes keep planets outside the sun radius and enforce minimum separation.

### 7.3 Placement

- Ambient systems are placed in `C` with a seeded jitter inside their sector; they do
  not use `MrukRoomLocalPlacement` and are not constrained to the room.
- The central sector is reserved for the current room/round content; do not spawn an
  ambient system inside the room footprint.
- Systems do not overlap: the system radius plus a gap must fit inside the sector, or
  the sector is treated as empty.
- The sun is a non-interactive landmark in v1: an emissive visual, not a hub and not
  a letter source. Do not add a real-time light per sun; the URP asset limits
  additional lights per object, and many lights are expensive on Quest.
- The galaxy/system root is a pooled prefab with a sun and a fixed number of planet
  slots. Slot count is the authored maximum; shape generation enables only the used
  slots.

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

### 8.2 Activation

A round selects `N` inactive ambient planets and activates their letter production:

```text
Select N planets from the ambient registry
  -> prefer planets inside the current bubble view window
  -> then nearest to the focus
  -> avoid already-active/on-route planets
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
- if the pool is exhausted, reuse the least-recently activated planet and reset its
  letters; never fail a round for lack of planets;
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
  `universeFocus`, and the ambient activation fields.

## 9. Universe elements

The shape system needs a small set of element types. Recommended order:

| Element | Role | Cost | Phase |
|---|---|---|---|
| Sun/star | System anchor, light, landmark | Low; one mesh + light | v1 |
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

## 10. Performance

- Pooled authored prefabs; no runtime-created assets or hierarchies.
- Ambient planets: no letters, no UI, no colliders, no `PlanetStore.Update`; only
  visual rotation.
- Active planets: at most the round's selection (2-4) plus existing active planets.
- Sector budget: max 12 systems / 48 ambient planets; spawn/despawn budget per frame.
- LOD/culling: disable distant planet renderers beyond the cull radius; the bubble
  window is small, so most content is outside it.
- No per-frame allocations, no LINQ, no `UnityEngine.Random` in per-frame code.
- Measure on the headset; the current render scale is 1.6 and MSAA 4, so cap the
  active ambient count before adding VFX-heavy elements.
- Reuse the existing planet materials/meshes; do not create materials per planet.

## 11. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `UniverseBounds` | Center, radius, edge fade, focus clamp API. | `DigitalLove.Game.Levels` |
| `UniverseFocus` | Focus point in C, view radius, bubble pose handoff. | `DigitalLove.Game.Levels` |
| `UniverseSectorSpawner` | Sector grid, streaming, pooling, spawn budget, outer bound. | `DigitalLove.Game.Levels` |
| `GalaxyShapeData` | Shape/seed data for a system. | `DigitalLove.Game.Nodes` |
| `GalaxyShape` | Pure seeded layout generator (positions, radii, anchor). | `DigitalLove.Game.Nodes` |
| `AmbientGalaxyBehaviour` | Pooled system root: sun, planet slots, apply shape. | `DigitalLove.Game.Nodes` |
| `AmbientPlanetBehaviour` | Decor/activation state for a pooled planet. | `DigitalLove.Game.Nodes` |
| `AmbientPlanetRegistry` | Deterministic ids, sectors, loaded planets, lookup. | `DigitalLove.Game.Nodes` |
| `RoundPlanetSelector` | Pick inactive ambient planets for a round. | `DigitalLove.Game.Levels` |
| `AmbientPanelPool` | Small pool of world-space resource panels. | `DigitalLove.Game.Levels` |

Modified:

| Type | Change |
|---|---|
| `UniverseView` | Bubble window: `miniBubbleRadius / viewRadius`, focus-based pose, expose `MiniScale`/bubble anchor; no whole-content mini scale. |
| `UniverseTransform` (future) | Pan the focus in Bubble view, clamp to `UniverseBounds`; keep room constraints in Room view. |
| `LevelContainer` | Spawn/restore ambient registry; round activation instead of planet spawn. |
| `PlanetsSpawner` | Keep required planets; add ambient spawn/activation APIs or split into an ambient spawner. |
| `PlanetBehaviour` / `PlanetRefs` | `IsGameplayActive`, `SpawnAmbient`, `ActivateForRound`, `Deactivate`, panel assignment, collider/layer toggle. |
| `PlanetStore` / `ResourcePanel` | Enable only for active planets; panel pool. |
| `PlanetSelectionRules` | Require `IsGameplayActive`. |
| `RoundData` | `planetsToActivate`; keep `planetsToAdd` fallback; deprecate distance multiplier. |
| `GameSnapshot` | `schemaVersion`, `worldSeed`, `universeFocus`, active ambient planet ids/letters. |
| `MrukRoomLocalPlacement` | Required/round content only; ambient content does not register room occupants. |
| `NewRoundState` | Round selection/activation instead of spawning new planets. |
| `Game.unity` | Universe bounds/focus root, ambient system/planet pools, panel pool, spawner wiring. |

Assembly note: generation/data in `DigitalLove.Game.Nodes`, spawning/round flow in
`DigitalLove.Game.Levels`, and the bubble pan in the universe-transform assembly. Do
not make `Nodes` or `Levels` reference the transform assembly.

## 12. Phases

### Phase I0 - Bounds, seed, and shape math

- Add `UniverseBounds`, `UniverseFocus`, sector coordinates, deterministic PRNG.
- Add `GalaxyShapeData`/`GalaxyShape` with Ring, Spiral, Cluster.
- Add edit-mode tests/preview for shape positions, min separation, and determinism.
- No scene spawning yet.

**Exit:** seeded shapes are stable, non-overlapping, and fit their sector.

### Phase I1 - Ambient systems in the bubble window

- Add the ambient galaxy/planet prefabs and pools.
- Add `UniverseSectorSpawner` with a fixed debug focus (no pan).
- Render one central cluster and a few nearby sectors.
- Update `UniverseView` to the view-window scale.
- No round changes.

**Exit:** the bubble shows a readable system/window; content streams around the focus;
decor planets have no letters, UI, or colliders.

### Phase I2 - Pan, bounds, and streaming

- Integrate the bubble drag from [Transforming the universe](universe-transform.md).
- Clamp the focus to `UniverseBounds`; implement hysteresis and the edge fade.
- Stream/unstream sectors while panning; verify no hitches.
- Room view keeps the current room content and constraints.

**Exit:** the player can pan the bubble across a large but finite universe and is stopped
at the bound; content regenerates identically when panning back.

### Phase I3 - Ambient planets and round activation

- Add `AmbientPlanetRegistry`, `RoundPlanetSelector`, and the panel pool.
- Add `planetsToActivate` to `RoundData` and migrate the round assets.
- Add `PlanetBehaviour` activation/deactivation and `IsGameplayActive`.
- Update selection rules and `LevelLoopPlanetAvailability`.
- Update the snapshot/restore path.

**Exit:** a round activates existing ambient planets instead of spawning planets; letters
start, routes see only active planets, and restore returns the same active set.

### Phase I4 - More elements

- Add belts/moons, then nebula billboards, stations, comets, gates, and black holes in
  the order in §9.
- Each element gets a perf budget and a gameplay/decor classification before art.

### Phase I5 - Tuning

- Tune universe radius, view radius, sector size, density, caps, and the edge feel.
- Balance round selection (nearby vs. explore), reuse, and letter rates.
- Profile on the headset; verify no GC and stable frame time.

## 13. Test plan

1. Universe bound: focus clamps; the view cannot pan past the edge; no content spawns
   outside the bound.
2. Bubble window: `MiniScale = miniBubbleRadius / viewRadius`; planets are readable;
   the bubble shows a local region, not the whole universe.
3. Panning: focus moves, content streams in/out with hysteresis, and panning back
   regenerates identical systems from the seed.
4. Shapes: Ring/Spiral/Cluster produce the expected counts; no planet overlaps the sun
   or another planet; same seed -> same layout.
5. Streaming caps: never exceed max systems/planets; spawn/despawn budget holds; no
   per-frame GC after warmup.
6. Ambient planets: no letters, no UI, no route selection, no collider/ray target.
7. Round activation: selects N existing planets; no new planet is spawned; letters
   start; UI appears; only active planets are selectable.
8. Selection near the focus: the round prefers visible planets; if it expands, the view
   recenters or the UI locates them.
9. Exhaustion: reusing a planet resets its letters and never fails the round.
10. Restore: universe seed/focus/active ids reproduce the same content and letters;
    old snapshots still load.
11. Room view: current hubs/ships/routes still work; ambient decor does not interfere.
12. Performance: headset frame time, draw calls, active system/planet counts, no GC.

## 14. Risks and decisions

| Risk / decision | Choice / mitigation |
|---|---|
| Literal infinity | Finite seeded universe with a hard outer bound. |
| Bubble scale | Switch from whole-content mini scale to a `viewRadius` window; requires a two-views plan update. |
| Pan control | Owned by [Transforming the universe](universe-transform.md); this plan exposes bounds/focus only. |
| Content outside the room | Ambient content is not room-constrained; required round content stays near the room/focus. |
| Ambient gameplay | Decor first; rounds activate authored letter components. No `AddComponent`. |
| Round planets | Select existing planets; keep `planetsToAdd` fallback for assets; prefer visible planets and recenter if needed. |
| Pool exhaustion | Prefer unused, then reuse the least-recently activated planet and reset letters. |
| Ambient performance | Cap systems/planets, disable stores/UI/colliders, stream with hysteresis. |
| Panel cost | Small authored panel pool, not one Canvas per ambient planet. |
| Persistence | Seed/focus/active ids only; ambient regenerates; snapshot migration. |
| Galaxy plan overlap | Reuse its galaxy data/transform later; do not build two galaxy systems. This plan owns procedural creation/streaming. |
| Color scope | Bubble sphere becomes the view window; update the color-scope plan's center/radius. |
| Boundary feel | Signed-distance fade/backdrop plus a hard focus clamp; no wall object. |
| Element scope | Suns/planets first; belts/moons next; avoid volumetrics/physics early. |
| No runtime assets | Pooled authored prefabs; no generated meshes/materials/hierarchies. |

## 15. Defaults

| Parameter | Default |
|---|---|
| `universeRadius` | `20 m` |
| `viewRadius` | `2.0 m` |
| `miniBubbleRadius` | `0.26 m` |
| `sectorSize` | `3 m` |
| `spawnRadius` | `5 m` |
| `cullRadius` | `7 m` |
| `sectorDensity` | `0.55` |
| `maxActiveSystems` | `12` |
| `maxActiveAmbientPlanets` | `48` |
| `planetsPerSystem` | `2-6` |
| `systemRadius` | `1.2-2.0 m` |
| `planetRadius` | `0.15-0.35 m` |
| `planetsToActivate` | `RoundData` 2-4 |
| `edgeFadeRadius` | `3 m` |
| `focusMargin` | `viewRadius` |
| `spawnBudgetPerFrame` | `1 system` |

## 16. Future

- Binary systems, planet-anchored satellite galaxies, and moons/rings.
- Asteroid belts, nebula billboards, stations/outposts, comets, wormholes, black holes.
- Galaxy transform/layout propagation from [Universe galaxies](universe-galaxies.md).
- Chunked persistence of modified ambient planets if ambient gameplay expands.
- A real infinite mode only if the finite bound is proven too small.
