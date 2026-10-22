# Universe — Sector Generation and Extent

**Status:** backlog - pure C# generation and extent math; no scene spawning.

**Framing:** [Space shaping](../gameplay/space-shaping-future.md). The seeded system
layout is what the shaping lattice operates on; a puzzle system may be authored
(editor-first, or from the puzzle seed) inside its sector.

**Related:** [Sector field hub](universe-infinite-sector.md) · [Bubble window](../../done/universe-bubble-window.md)

**Wiring rule:** generation is deterministic C#. No `UnityEngine.Random` in per-frame
code, no scene iteration, no runtime-created assets.

## 1. Goal

Provide the deterministic, testable foundation for a bounded, seeded universe:

- the focus/extent model;
- a sector grid with stable, seed-derived coordinates;
- galaxy/system shape generation (Ring, Spiral, Cluster) returning local positions,
  radii, and an anchor pose.

Everything here is pure C# and unit-testable without a Unity scene. Scene spawning and
visibility live in [Bubble window](../../done/universe-bubble-window.md): the whole bounded field is
pre-spawned and out-of-frame content is hidden by the frame cube.

## 2. Model

```text
Universe space (C) = Level local space, fixed
Focus F            = C point shown at the bubble center
View window        = cube of half-size viewRadius around F
UniverseBounds     = the bounded field extent (a fixed box), not room bounds
```

## 3. Extent

`UniverseBounds` (authored on the `Level`/`Universe` root) is the bounded extent of the
generated field:

```text
origin      = deterministic universe origin in C
fieldRadius = the field half-size in C (how far the frame may pan)
viewRadius  = 2.5 m
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
sectorSize   = 5 m                (>= 2 * maxSystemRadius + gap)
fieldRadius  = bounded; sector count = fieldRadius / sectorSize
density      = 0.55               (chance a sector contains a system)
softSystemBudget  = 16            (soft)
softPlanetBudget  = 64            (soft; add occlusion/simplification later)
```

- Sectors are deterministic cells in C; each sector seed derives from
  `worldSeed + sectorCoord`.
- Enforce `sectorSize >= 2 * systemRadius + systemGap`, then run a spacing check
  against systems in neighboring sectors. If two systems would overlap, mark the
  sector empty or relocate the new system.
- A puzzle's system is authored (editor-first, or from the puzzle seed) inside its
  sector; the grid itself only lays out ambient systems.
- Budgets are soft, not hard caps; they bound the field size and therefore the load time.

## 5. Galaxy/system shapes

### 5.1 Data

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

### 5.2 Shapes

- **Ring:** 1-3 concentric rings around the anchor planet; planets evenly spaced with
  jitter. Default for v1.
- **Spiral:** 2-3 logarithmic arms; planets sampled along the arms with jitter. Second
  v1 shape.
- **Cluster:** Poisson-disc-ish placement in a flattened sphere; fallback for dense
  sectors.
- **Binary:** two anchor planets with a barycenter; future.
- **Satellite:** planets/moons around a large anchor planet; future.

`GalaxyShape` is a pure C# generator returning local positions, radii, and an anchor
pose. It is seeded and testable; no Unity random or scene iteration. All shapes keep
planets outside the anchor radius and enforce minimum separation.

The same generator supplies a puzzle's seed layout: the designer may author a puzzle
system from the seed, or hand-place it, then tune it by moving planets. Shape variety
(Ring/Spiral/Cluster) matters less once the player reshapes the field, so Ring + jitter
is the v1 default and the rest can follow.

## 6. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `UniverseBounds` | Bounded field extent and sector-grid origin; no gameplay wall. | `DigitalLove.Game.Levels` |
| `UniverseFocus` | Focus point in C, view radius, bubble pose handoff, extent clamp. | `DigitalLove.Game.Levels` |
| `GalaxyShapeData` | Shape/seed data for a system. | `DigitalLove.Game.Nodes` |
| `GalaxyShape` | Pure seeded layout generator (positions, radii, anchor). | `DigitalLove.Game.Nodes` |

Reuse: `SeededRandom` (`DigitalLove.Global`) for deterministic PRNG.

Assembly note: generation/data in `DigitalLove.Game.Nodes`, extent/focus in
`DigitalLove.Game.Levels`. Do not make `Nodes` or `Levels` reference the transform
assembly.

## 7. Phases

### G0 — Extent, seed, and shape math

- Add `UniverseBounds` (bounded field extent), `UniverseFocus`, sector coordinates,
  deterministic PRNG wiring.
- Add `GalaxyShapeData` / `GalaxyShape` with Ring, Spiral, Cluster.
- Add edit-mode tests/preview for shape positions, min separation, and determinism.
- No scene spawning yet.

**Exit:** seeded shapes are stable, non-overlapping, and fit their sector.

## 8. Test plan

1. Shapes: Ring/Spiral/Cluster produce the expected counts; no planet overlaps the sun
   or another planet; same seed -> same layout.
2. Sector grid: coordinates are stable; the spacing check rejects overlaps; a sector
   fits within `sectorSize`.
3. Extent: the focus clamps inside `fieldRadius`; the frame stays inside the field.
4. Determinism: identical inputs produce identical output across runs.
5. No per-frame allocations and no LINQ in the new code.

## 9. Defaults

| Parameter | Default |
|---|---|
| `fieldRadius` | bounded; tuned to the desired field size |
| `viewRadius` | `2.5 m` |
| `sectorSize` | `5 m` (`>= 2 * maxSystemRadius + gap`) |
| `sectorDensity` | `0.55` |
| `softSystemBudget` | `16` |
| `softPlanetBudget` | `64` |
| `planetsPerSystem` | `2-6` |
| `systemRadius` | `1.2-2.0 m` |
| `planetRadius` | `0.15-0.35 m` |
| `anchorScale` | `1.2-2.5x` planet radius |
| `generatorVersion` | `1` |

## 10. Risks

| Risk | Mitigation |
|---|---|
| Overlapping systems | `sectorSize >= 2 * systemRadius + gap` plus a neighbor spacing check. |
| Generation changes invalidate saves | Version generation (`generatorVersion`); remap puzzle planets by position. |
| Shape overlap with anchor | Keep planets outside the anchor radius and enforce min separation. |
| Field too large to pre-spawn | Bound `fieldRadius` and the budgets to the load-time and memory budget; park streaming if the field must grow. |
