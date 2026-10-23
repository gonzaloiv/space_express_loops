# Universe — Sector Generation and Extent

**Status:** in progress - G0: pure C# generation and extent math; no scene spawning.

**Framing:** [Space shaping](../backlog/gameplay/space-shaping-future.md). The seeded sector
layout is what the shaping lattice operates on; a puzzle sector may be authored
(editor-first, or from the puzzle seed) inside its grid cell.

**Related:** [Sector field hub](../backlog/universe/universe-infinite-sector.md) · [Bubble window](../done/universe-bubble-window.md) · [Color scope](../done/universe-color-scope.md)

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
[Bubble window](../done/universe-bubble-window.md) plan never implemented field spawning, so it
is not handed off there. Out-of-frame content is **grayed** by the
[color scope](../done/universe-color-scope.md), not hidden, and a distance hide beyond the gray
band is still to be specified (see the [sector field hub](../backlog/universe/universe-infinite-sector.md) baseline
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
Aperture           = Map = wide, Sculpt = narrow (one frame, two apertures)
UniverseBounds     = the bounded field extent (a fixed box), not room bounds
```

## 3. Extent

`UniverseBounds` (authored on the `Level`/`Universe` root) is the bounded extent of the
generated field:

```text
origin      = deterministic universe origin in C
fieldRadius = the field half-size in C (how far the frame may pan)
viewRadius  = 1.2 m in the current scene; re-baseline against the Map/Sculpt apertures
```

Rules:

- The field is a bounded box around the origin, not infinite and not room-centered.
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

Scope:

- **Spawn the field** from pooled visuals: one ambient sector per occupied grid cell, anchored on
  the cell center, using the existing planet visual (`NodeBody`) unchanged. Ambient sectors carry
  no gameplay.
- **Selection.** Pick which field planets become the active **destinations** (letters/store) and
  which become **hubs**. Selection is a pure function of (world seed, level/puzzle identity,
  active count), so it is deterministic and reproducible; the field itself is never regenerated.
- **Spawners become selectors.** `PlanetsSpawner` / `HubsSpawner` stop placing room-local planets
  and instead activate field planets; `MrukRoomLocalPlacement` no longer applies to gameplay
  planets. `NodeBody` stays as the visual.
- **One source of truth.** A planet's position comes from the field, so the field and gameplay
  cannot disagree about where a planet is.
- **Visibility.** Out-of-frame content is grayed by the
  [color scope](../done/universe-color-scope.md); the hide band beyond the gray edge is still to
  be specified, and visibility is driven by the composition root, never by the field assembly.

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
| `viewRadius` | `1.2 m` in the current scene; re-baseline against the Map/Sculpt apertures |
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

## 11. Open issues (undefined)

These are not risks with mitigations; they are decisions the plan does not yet make. They
should be settled before G0's numbers are frozen.

| Issue | Why it matters |
|---|---|
| **Units of content space** | The plan is written in universe metres (`cellSize = 5 m`, `sectorRadius = 1.2-2.0 m`), but the game's planets are `0.15-0.35 m` in *room* metres and the view shrinks content by `MiniScale ~ 0.23`. A `5 m` cell is ~`1.17 m` on screen against a `0.28 m` window. Decide whether content space is a large universe space the frame magnifies, or room metres. |
| **Placement model** | Direction: generate the field first and **select** the current destinations/hubs from it, so positions have one source of truth. `PlanetsSpawner` / `HubsSpawner` / `MrukRoomLocalPlacement` become selectors; `NodeBody` stays as the planet visual. This is G1. |
| **Aperture ownership** | Map = wide / Sculpt = narrow over one frame; `modes.md` says a view/camera/input mode is a Mode, not a Flow state. Does the aperture live on the view as a Mode, or as a field on the frame? `viewRadius` is `1.2` in the scene vs `2.5` assumed here. |
| **Save/load** | Deferred: the snapshot has to be rethought, not patched - seed + `generatorVersion` + which field planets are active. Not part of G0 or G1. |
| **Room placement** | Deferred: `MrukRoomLocalPlacement` stops applying to gameplay planets once they come from the field; what survives for genuine room props is open. |
| **Hide band** | Unspecified; it is the only thing bounding draw calls and load time once content grays instead of hiding. |
| **Bounded-edge UX** | The frame "simply stops at the edge (no visible boundary wall or edge fade)". How does the player know they reached the edge? Also the clamp is `0.3 x` today, which fights "pan to bring any sector in front". |
| **Batching** | A large field of individual planet renderers plus per-instance material instances (route colours via `NodeBody.SetRouteColor`) breaks SRP batching. Do ambient field planets share materials? |
| **Silent planets** | The hub says that pool "should be migrated into the field's ambient sectors rather than kept permanently"; when, and does it replace `SilentPlanetsSpawner`? |
| **Puzzle-side numbers** | Planet density, region size, and ship cap are gated by the in-progress [space shaping spike](../in-progress/space-shaping-spike.md). G0 should land the deterministic math and tests, not tune density. |

