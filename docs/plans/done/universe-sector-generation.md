# Universe — Sector Generation and Extent

**Status:** done - G0 (pure C# extent, sector grid, `SectorShape` math) and G1 (field spawned into
the scene, replacing the decor pool; destinations selected from the field) are complete and
verified on device, with 39 EditMode tests. The **aperture is a trigger, not a leftover**: see the
hub's baseline notes before raising `viewRadius`.

**Framing:** [Space shaping](../backlog/gameplay/space-shaping-future.md). The seeded sector
layout is what the shaping lattice operates on; a puzzle sector may be authored
(editor-first, or from the puzzle seed) inside its grid cell.

**Related:** [Sector field hub](../backlog/universe/universe-infinite-sector.md) · [Bubble window](universe-bubble-window.md) · [Color scope](universe-color-scope.md)

**Wiring rule:** generation is deterministic C#. No `UnityEngine.Random` in per-frame
code, no scene iteration, no runtime-created assets.

## 1. Goal

Provide the deterministic, testable foundation for a bounded, seeded universe:

- the focus/extent model;
- a sector grid with stable, seed-derived cell coordinates;
- sector shape generation (Ring, Spiral, Cluster) returning local positions, radii, and
  an anchor pose.

Everything in G0 is pure C# and unit-testable without a Unity scene. Putting the field into the
scene and selecting the active destinations/hubs from it is **G1** (below) - the
[Bubble window](universe-bubble-window.md) plan never implemented field spawning, so it
is not handed off there. Out-of-frame content is **grayed** by the
[color scope](universe-color-scope.md), not hidden; the hide band beyond the gray edge is
`SectorFieldSpawner.hideRadiusFactor`. See the [sector field hub](../backlog/universe/universe-infinite-sector.md) baseline
notes).

## Vocabulary

| Term | Meaning |
|---|---|
| field | The whole bounded universe (`UniverseBounds`). |
| grid cell | One deterministic cell of the sector grid; spacing, density, and budgets live here. |
| sector | A group of planets around an anchor (the "sun" role) - the galaxy-like unit the player sees. |
| region | Reserved for a puzzle's area in [space shaping](../backlog/gameplay/space-shaping-future.md); may span several grid cells. Not implemented here. |

"System" and "galaxy" are retired: the group is a **sector** (`SectorShape`), its container is a
**grid cell**.

## 2. Model

```text
Universe space (C) = Level local space, fixed
Focus F            = C point shown at the view center
View window        = cube of half-size viewRadius around F
Aperture           = the level selector's wide view vs a picked cube scaled up (one frame)
UniverseBounds     = the bounded field extent (a fixed box), not room bounds
```

## 3. Extent

`UniverseBounds` (authored on the `Level`/`Universe` root) is the bounded extent of the
generated field:

```text
origin      = deterministic universe origin in C
fieldRadius = the field half-size in C (how far the frame may pan); <= ~14 m (aperture cap)
viewRadius  = the aperture: Map = fieldRadius, Sculpt = one sector radius
```

Rules:

- The field is a bounded box around the origin, not infinite and not room-centered.
- **Content space is real metres** (planets `0.15-0.35 m`, sectors `1.2-2.0 m`, cells `5 m`); the
  rig's parent scale/offset presents it (`UniverseContentRig.ApplyDisplayPose`).
- **`viewRadius` is the aperture.** `MiniScale = miniBubbleRadius / viewRadius` makes the chosen
  region fill the window, so Map = `fieldRadius` and Sculpt = one sector radius need no new
  mechanism. `minMiniScale = 0.02` and `miniBubbleRadius = 0.28` cap the aperture at **14 m**.
- The focus clamps so the frame stays inside `fieldRadius`; the frame simply stops at the
  edge (no visible boundary wall or edge fade).
- Because the extent is small, there is no floating-origin rebase: content coordinates
  stay within float-safe range.
- Panning back over previously visited space shows the same content unless a puzzle
  changed it (the field is pre-spawned, not regenerated).

## 4. Sector grid

```text
cellSize     = 5 m                (>= 2 * maxSectorRadius + gap)
fieldRadius  = bounded; cell count = fieldRadius / cellSize
density      = 0.55               (chance a cell contains a sector)
softSectorBudget  = 16            (soft)
softPlanetBudget  = 64            (soft; add occlusion/simplification later)
```

- Cells are deterministic in C; each cell seed derives from `worldSeed + cellCoord`.
- Enforce `cellSize >= 2 * sectorRadius + sectorGap`, then run a spacing check against
  sectors in neighboring cells. If two sectors would overlap, mark the cell empty or
  relocate the new sector.
- A puzzle's sector is authored (editor-first, or from the puzzle seed) inside its cell; the
  grid itself only lays out ambient sectors.
- Budgets are soft, not hard caps; they bound the field size and therefore the load time.

## 5. Sector shapes

### 5.1 Data

`SectorShapeData` (authored or generated per sector):

```text
shape             Ring | Spiral | Cluster | Binary (future)
anchorScale       1.2-2.5x a normal planet radius (the "sun" role)
radius            sector radius at scale 1 (e.g. 1.2-2.0 m)
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

### 5.2 Shapes

- **Ring:** 1-3 concentric rings around the anchor planet; planets evenly spaced with
  jitter. Default for v1.
- **Spiral:** 2-3 logarithmic arms; planets sampled along the arms with jitter. Second
  v1 shape.
- **Cluster:** Poisson-disc-ish placement in a flattened sphere; fallback for dense cells.
- **Binary:** two anchor planets with a barycenter; future.
- **Satellite:** planets/moons around a large anchor planet; future.

`SectorShape` is a pure C# generator returning local positions, radii, and an anchor pose.
It is seeded and testable; no Unity random or scene iteration. All shapes keep planets
outside the anchor radius and enforce minimum separation.

The same generator supplies a puzzle's seed layout: the designer may author a puzzle sector
from the seed, or hand-place it, then tune it by moving planets. Shape variety
(Ring/Spiral/Cluster) matters less once the player reshapes the field, so Ring + jitter is
the v1 default and the rest can follow.

## 6. Component map

New, all in the **`DigitalLove.Game.Sectors`** assembly (`Assets/_Game/Features/Sectors`), which
references only `DigitalLove.Global`:

| Type | Responsibility |
|---|---|
| `SectorShapeKind` | `Ring`, `Spiral`, `Cluster`. |
| `SectorShapeData` | Authored/generated shape data for a sector (kind, radii, counts, spread, inclination, min separation). |
| `SectorPlanet` | One generated planet: local position + radius. |
| `SectorLayout` | Generator result: sector radius, anchor radius/rotation, planets. |
| `SectorShape` | Pure seeded layout generator (Ring/Spiral/Cluster, separation, containment). |
| `SectorCell` / `SectorGrid` | Deterministic cell coordinates, per-cell seed, fit and spacing checks. |
| `SectorFieldData` | Field parameters: world seed, cell size, field radius, density, gap, layers, sector/planet budgets, shape data. |
| `SectorField` | Pure field generator: occupied cells, spacing rejection, budgets, per-cell layout. |
| `SectorPlacement` | One placed sector: cell, world center, layout. |
| `UniverseBounds` / `UniverseFocus` | Bounded field extent and focus clamp. |
| `SectorSeededExtensions` | Seeded sampling of `MinMaxFloat` / `MinMaxInt`. |

Reuse: `SeededRandom` (`DigitalLove.Global`). Note `MinMaxFloat.GetRandomValue()` and
`MinMaxInt.GetRandomValue()` use `UnityEngine.Random`, so they must not be used here - the
seeded extensions replace them.

Assembly note: the field is its own pure assembly so the retiring `Nodes` spawners are not
entangled with it; `Levels` and `SpaceShaping` reference it. Tests live in
`Tests/EditMode/DigitalLove.Game.Sectors.Tests.EditMode`.

## 7. Phases

### G0 — Extent, seed, and shape math (done)

- `UniverseBounds`, `UniverseFocus`, cell coordinates, seeded PRNG wiring - done.
- `SectorShapeData` / `SectorShape` with Ring, Spiral, Cluster - done.
- EditMode tests for shape counts, containment, separation, radius range, determinism, cell
  coordinates/seed, fit and spacing checks, and focus clamping - done, 23 passing.
- No scene spawning yet.

**Exit:** met - seeded shapes are stable, non-overlapping, and fit their grid cell.

### G1 — Spawn the field and select the active nodes

Bridge from the generated field to the scene: G0's math is not yet visible in the game, and the
current destinations/hubs become a **selection over the field**.

Pure layer (done): `SectorFieldData` + `SectorField` + `SectorPlacement` generate the whole
field - occupied cells by density, spacing rejection, sector/planet budgets, and a per-cell
`SectorLayout` seeded from the cell. EditMode tests cover budgets, cell centers, spacing,
density, determinism, and radius range.

Spawner (done): `SectorFieldSpawner` (in `DigitalLove.Game.SilentPlanets`, which now references
`DigitalLove.Game.Sectors`) generates the field and pools `SilentPlanet.prefab` instances - one
per placement planet plus a scaled-up anchor per sector - in content-local space, keeping the
old public surface (`ResetPool`, `SetBubbleView`, `SpawnIfNeeded`). It **replaced**
`SilentPlanetsSpawner` (and `SilentPlanetLayout`); `LevelContainer` and `LevelViewModeFanout`
were retyped, and the component is wired on `Level/SilentPlanets` with `prototype` from the
prefab and `viewTransform` on `CenterEyeAnchor`.

**Staged deliberately.** A real-metre field (`fieldRadius = 12.5`, `cellSize = 5`) is ~25 m
across, so it only fits the window if the aperture moves to `fieldRadius` - and at that aperture
the existing room-local content shrinks to a speck, which is why the selection half below is
coupled to it. To avoid that upheaval the wired field is **small and at the current aperture**,
and tuned on device: `cellSize 0.6`, `fieldRadius 1.8`, `destinationRadius 1.2`, `density 0.55`,
sector radius `0.2-0.24`, `planetRadius 0.04-0.09`, `anchorScale 1.0-1.1`, `minSeparation 1.0`.
Raise the aperture (Map = `fieldRadius`) only together with the real-metre field.

Selection (pure layer done): `SectorSelector` + `SectorSelection` pick active nodes from the
field - `SelectDestinations(field, levelSeed, offset, count)` takes the sectors in a seed-scrambled
order and returns the planet nearest each sector center. One selection per sector, deterministic
in (world seed, level seed, count). EditMode tests cover determinism, clamping, distinct sectors,
planet fidelity, level seeding, the destination radius, and the round offset.

`TrySelectHub` was written and then **removed**: hubs stay room-local (the
[integration](../backlog/gameplay/space-shaping-integration.md) deletes them), so it had no
caller. Restoring it is a few lines if hubs ever come from the field.

Remaining:

- **Destinations are field-selected (done).** `LevelContainer.SpawnPlanets` calls
  `SectorSelector.SelectDestinations(field, worldSeed, offset, count)` - the offset is the
  snapshot's planet count, so each round takes the next sectors - and passes the positions to
  `PlanetsSpawner`, which no longer places room-locally and no longer registers with
  `MrukRoomLocalPlacement`. The planet's own radius still comes from `PlanetSeedData`; the field
  supplies position only, and the bigger gameplay planet hides the smaller ambient one at the
  same spot.
- **Hubs stay room-local (deliberate).** `HubsSpawner` keeps `MrukRoomLocalPlacement`, because the
  [integration](../backlog/gameplay/space-shaping-integration.md) deletes hubs/stations.
  `SyncFromSnapshot` now registers hubs only.
- **Aperture - do not touch until it is forced.** `viewRadius` stays `1.2` and the field stays
  small on purpose: the current game works and looks right at that aperture. Raising it to the
  field's real-metre extent is a **trigger**, not a task - do it only when the gameplay content
  comes from the field (the level selector + [integration](../backlog/gameplay/space-shaping-integration.md)
  work), because at a `12.5 m` aperture the existing room-local content shrinks to a ~2 cm speck.
  `minMiniScale = 0.02` caps the aperture at 14 m. Changing it before then buys nothing and breaks
  the current game.
- **Field budget (done).** `softSectorBudget 32` / `softPlanetBudget 128` with `density 0.55` means
  the cap no longer binds (~27 occupied cells), so every occupied cell is generated and the play
  core gets its full share (~14 destinations). *Caveat:* `SectorField.Generate` iterates cells
  `x`-outer, so if a budget ever binds again the trim is asymmetric - prefer the core by ordering
  cells centre-out before raising `fieldRadius` much further.
- **Visibility (done).** Out-of-frame content is grayed by the
  [color scope](universe-color-scope.md), and `SectorFieldSpawner.hideRadiusFactor`
  (`1.5`) hides field planets beyond that multiple of the scope, so the gray band ends before the
  field edge. The spawner reads the scope from the shader globals, so it never references the
  view. Room view keeps the distance cull (`viewTransform` + `roomVisibleRadius`).

## 12. Fixes found while building G1

- **Content radius units.** `SetContentRadius` took a **world** radius and divided by the content
  root's scale; the field's play radius is **content-local**, so in Bubble pose it was divided by
  `MiniScale` (~0.233) and inflated ~4.3x - the scope swallowed the whole field and nothing grayed.
  Now `SetContentRadiusLocal` takes content-local metres and does not divide. `ContentRadiusWorld`
  still multiplies by `Scale` for the shader/pan consumers.
- **Content centre was set after spawning.** `LevelContainer` centred the content on the *average*
  of hubs + gameplay planets in `FinalizeSpawn`, i.e. **after** `PlanetBehaviour` positioned its
  letters panel in world space (`PlanetBehaviour.cs:84`). The content then re-centred, leaving the
  panels offset by that delta - visible as the HUD sitting at one extreme of the cluster, and much
  worse once destinations came from a skewed subset. Now `LevelContainer.Init` sets the content
  centre to the content origin and the content radius to the field's play radius **before**
  anything spawns, so the content never shifts. `LevelContentCenter` is deleted.
- **Scope radius source.** With a field, the content radius is the field's `destinationRadius`
  (the playable core), not the outer field: the core stays coloured and the ambient field grays
  outward, which is the look that works. `SectorFieldSpawner.PlayRadius` exposes it.

- **Destination distance.** Destinations were selected from anywhere in the field, so they landed
  up to 3.6 m apart and routes were unreachable. The old room placement kept planets within
  `maxDistanceToOtherPlanet 1.5 x 0.66 ~ 0.99 m`. `SectorFieldData.destinationRadius` now limits
  selection to sectors inside a core radius (`0` = no limit), so the field can stay big for the
  gray band while only its core hosts destinations. The scope and the marker cube follow, because
  `ContentRadiusWorld` is measured over hubs + gameplay planets.
- **Planet size relationship.** Destinations take their radius from `PlanetSeedData.radius`
  (`0.15-0.35 m`, an asset); field planets take it from the shape data. A destination is placed at
  a field planet's position and hides it, so the gameplay planet must be the larger of the two.
  The gap is capped by the shape math: the sector must contain the anchor plus the separation, so
  `planetRadius <= sectorRadius / ~2.1` with `anchorScale 1.0-1.15` and `minSeparation 1.05`. With a
  `0.4 m` cell the field tops out near `0.06 m`, so destinations stay visibly larger. Widening the
  field's size range (`planetRadius 0.02-0.06`) makes the jump less obvious; closing it fully needs
  either smaller destinations (edit `PlanetSeedData.radius`) or bigger cells (fewer destinations in
  the core).
- **`SectorGrid.CellSeed` was a poor hash.** It mixed the coordinates with multiplies and XORs
  whose low bits barely moved, so for some world seeds every cell's `seed % 1000` landed in a
  narrow high band: at `density 0.55` the field came out **empty** and at `1.0` full, and
  occupancy was effectively a function of `x` alone. Replaced with a lowbias32-style mixer and
  an unsigned modulo for the density test. `CellSeed_IsWellDistributed` now checks that every
  `% 10` bucket is hit across cells for several world seeds, and
  `Generate_ProducesSectorsAtDefaultDensity` checks the field is non-empty for 16 seeds - the
  earlier tests passed only because they were trivially satisfied by an empty field.
- **Cleanup done.** `RoundData.distanceBetweenPlanetsMultiplier` and
  `PlanetSeedData.maxDistanceToOtherPlanet` are deleted (code + the nine authored assets), and
  `SectorSelector.TrySelectHub`/`SelectAnchor`/`SectorSelection.IsAnchor` with them.
  `MaxDistanceBetweenPlanets.asset` stays - the spaceship prefab also references it.

- **`SectorGrid.CellSeed` was a poor hash.** It mixed the coordinates with multiplies and XORs
  whose low bits barely moved, so for some world seeds every cell's `seed % 1000` landed in a
  narrow high band: at `density 0.55` the field came out **empty** and at `1.0` full, and
  occupancy was effectively a function of `x` alone. Replaced with a lowbias32-style mixer and
  an unsigned modulo for the density test. `CellSeed_IsWellDistributed` now checks that every
  `% 10` bucket is hit across cells for several world seeds, and
  `Generate_ProducesSectorsAtDefaultDensity` checks the field is non-empty for 16 seeds - the
  earlier tests passed only because they were trivially satisfied by an empty field.

Not here: the snapshot rewrite and the room-placement rethink (§11), and the puzzle-loop
replacement ([integration](../backlog/gameplay/space-shaping-integration.md)).

**Exit:** the field is visible in the scene, and a level's destinations/hubs are selected from it
deterministically, with no room-local placement for gameplay planets.

## 8. Test plan

1. Sector shapes: Ring/Spiral/Cluster produce the expected counts; no planet overlaps the
   anchor or another planet; same seed -> same layout.
2. Grid: cell coordinates are stable; the spacing check rejects overlaps; a sector fits
   within `cellSize`.
3. Extent: the focus clamps inside `fieldRadius`; the frame stays inside the field.
4. Determinism: identical inputs produce identical output across runs.
5. No per-frame allocations and no LINQ in the new code.

## 9. Defaults

| Parameter | Default |
|---|---|
| `fieldRadius` | bounded; tuned to the desired field size |
| `viewRadius` | `1.2 m` in the current scene; re-baseline against the level selector's aperture |
| `cellSize` | `5 m` (`>= 2 * maxSectorRadius + gap`) |
| `sectorDensity` | `0.55` |
| `softSectorBudget` | `16` |
| `softPlanetBudget` | `64` |
| `planetsPerSector` | `2-6` |
| `sectorRadius` | `1.2-2.0 m` |
| `planetRadius` | `0.15-0.35 m` |
| `anchorScale` | `1.2-2.5x` planet radius |
| `generatorVersion` | `1` |

## 10. Risks

| Risk | Mitigation |
|---|---|
| Overlapping sectors | `cellSize >= 2 * sectorRadius + gap` plus a neighbor spacing check. |
| Generation changes invalidate saves | Version generation (`generatorVersion`); remap puzzle planets by position. |
| Shape overlap with anchor | Keep planets outside the anchor radius and enforce min separation. |
| Field too large to pre-spawn | Bound `fieldRadius` and the budgets to the load-time and memory budget; park streaming if the field must grow. |
| No hide band | Out-of-frame content is grayed, not hidden, so every pre-spawned sector renders. Bound `fieldRadius` and the budgets, and add a distance hide beyond the gray band before the field grows. |

## 11. Open issues

Raised before G0/G1 and settled or handed off as noted. Items still open are owned by the
[integration](../backlog/gameplay/space-shaping-integration.md) or the level-selector work, not by this
plan.

| Issue | Outcome |
|---|---|
| **Units of content space** | **Settled: content space is real metres**, and the rig's parent scale/offset presents it (`UniverseContentRig.ApplyDisplayPose`). `viewRadius` is the **aperture**: the selector = `fieldRadius`, a picked cube = one sector radius, so `MiniScale = miniBubbleRadius / viewRadius` fills the window with no new mechanism. `minMiniScale = 0.02` and `miniBubbleRadius = 0.28` cap the aperture at **14 m**. The wired field is deliberately *small* (`fieldRadius 1.8`) at the current aperture - see the hub's trigger note. |
| **Placement model** | **Done for destinations**: the field generates first and `SectorSelector` selects the active destinations from it, so positions have one source of truth. `HubsSpawner` stays room-local on purpose (integration deletes hubs). |
| **Aperture ownership** | **Open, handed off.** The selector is wide / a picked cube is narrow over one frame; `modes.md` says a view/camera/input mode is a Mode, not a Flow state. Does the aperture live on the view as a Mode, or as a field on the frame? Settle it when the content moves onto the field. |
| **Save/load** | **Open, handed off.** The snapshot has to be rethought, not patched - seed + `generatorVersion` + which field planets are active. |
| **Room placement** | **Settled for planets**: `MrukRoomLocalPlacement` no longer applies to gameplay planets (and `SyncFromSnapshot` registers hubs only). What survives for genuine room props is still open. |
| **Hide band** | **Settled:** `SectorFieldSpawner.hideRadiusFactor` (`1.5`) hides field planets beyond that multiple of the scope, so a grown field does not render everything. |
| **Bounded-edge UX** | **Open.** The frame "simply stops at the edge (no visible boundary wall or edge fade)". How does the player know they reached the edge? The pan clamp is now a hard stop at `panLimitFactor x ScopeRadiiWorld`.
| **Batching** | A large field of individual planet renderers plus per-instance material instances (route colours via `NodeBody.SetRouteColor`) breaks SRP batching. Do ambient field planets share materials? |
| **Silent planets** | The hub says that pool "should be migrated into the field's ambient sectors rather than kept permanently"; when, and does it replace `SilentPlanetsSpawner`? |
| **Puzzle-side numbers** | Planet density, region size, and ship cap are gated by the (now done) [space shaping spike](space-shaping-spike.md). G0 landed the deterministic math and tests, not density tuning. |

