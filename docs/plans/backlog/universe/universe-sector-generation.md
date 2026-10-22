# Universe — Sector Generation and Extent

**Status:** backlog - pure C# generation and extent math; no scene spawning.

**Related:** [Infinite sector hub](universe-infinite-sector.md) · [Sector streaming](universe-sector-streaming.md) · [Bubble window](universe-bubble-window.md)

**Wiring rule:** generation is deterministic C#. No `UnityEngine.Random` in per-frame
code, no scene iteration, no runtime-created assets.

## 1. Goal

Provide the deterministic, testable foundation for an effectively infinite universe:

- the focus/extent model and the floating-origin trigger;
- a sector grid with stable, seed-derived coordinates;
- galaxy/system shape generation (Ring, Spiral, Cluster) returning local positions,
  radii, and an anchor pose.

Everything here is pure C# and unit-testable without a Unity scene. Scene spawning and
pooling live in [Sector streaming](universe-sector-streaming.md).

## 2. Model

```text
Universe space (C) = Level local space, periodically rebased
Focus F            = C point shown at the bubble center
View window        = sphere of radius viewRadius around F
UniverseBounds     = soft safety/streaming extent, not room bounds
```

## 3. Extent and floating origin

`UniverseBounds` (authored on the `Level`/`Universe` root) is a soft safety extent, not
a room bound:

```text
origin               = deterministic universe origin in C
safetyRadius         = 10 km (optional guard, not a gameplay wall)
viewRadius           = 2.5 m
originRebaseDistance = 500 m
```

Rules:

- There is no room-centered edge; the focus can move indefinitely in sector space.
- `safetyRadius` only guards against runaway values/teleport bugs; if hit, recenter to
  the central hub instead of stopping.
- When `|F - origin| > originRebaseDistance`, rebase: shift `Level`/content positions
  and `F` by the same offset, keep sector coordinates unchanged. This keeps world float
  precision stable across long pans.
- Panning back over previously visited space regenerates the same content from the seed
  unless round-activated state changed it.

## 4. Sector grid

```text
sectorSize   = 5 m                (>= 2 * maxSystemRadius + gap)
spawnRadius  = viewRadius + 3 m   (default 5.5 m)
cullRadius   = viewRadius + 5 m   (default 7.5 m)
density      = 0.55               (chance a sector contains a system)
softSystemBudget  = 16            (soft; the pool grows with rounds)
softPlanetBudget  = 64            (soft; add occlusion/simplification later)
```

- Sectors are deterministic cells in C; each sector seed derives from
  `worldSeed + sectorCoord`.
- Enforce `sectorSize >= 2 * systemRadius + systemGap`, then run a spacing check
  against systems in neighboring sectors. If two systems would overlap, mark the
  sector empty or relocate the new system.
- The central sector contains the pre-spawned gameplay galaxy and is always loaded.
- Budgets are soft, not hard caps; the pool grows with the rounds.

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

## 6. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `UniverseBounds` | Soft safety extent, rebase trigger, sector-grid origin; no gameplay wall. | `DigitalLove.Game.Levels` |
| `UniverseFocus` | Focus point in C, view radius, bubble pose handoff. | `DigitalLove.Game.Levels` |
| `GalaxyShapeData` | Shape/seed data for a system. | `DigitalLove.Game.Nodes` |
| `GalaxyShape` | Pure seeded layout generator (positions, radii, anchor). | `DigitalLove.Game.Nodes` |

Reuse: `SeededRandom` (`DigitalLove.Global`) for deterministic PRNG.

Assembly note: generation/data in `DigitalLove.Game.Nodes`, extent/focus in
`DigitalLove.Game.Levels`. Do not make `Nodes` or `Levels` reference the transform
assembly.

## 7. Phases

### G0 — Extent, seed, and shape math

- Add `UniverseBounds` (soft safety extent + rebase trigger), `UniverseFocus`, sector
  coordinates, deterministic PRNG wiring.
- Add `GalaxyShapeData` / `GalaxyShape` with Ring, Spiral, Cluster.
- Add edit-mode tests/preview for shape positions, min separation, and determinism.
- No scene spawning yet.

**Exit:** seeded shapes are stable, non-overlapping, and fit their sector.

## 8. Test plan

1. Shapes: Ring/Spiral/Cluster produce the expected counts; no planet overlaps the sun
   or another planet; same seed -> same layout.
2. Sector grid: coordinates are stable; the spacing check rejects overlaps; a sector
   fits within `sectorSize`.
3. Rebase: `|F - origin|` past `originRebaseDistance` produces a shift that keeps
   sector coordinates unchanged.
4. Determinism: identical inputs produce identical output across runs.
5. No per-frame allocations and no LINQ in the new code.

## 9. Defaults

| Parameter | Default |
|---|---|
| `safetyRadius` | `10 km` (guard only, not a wall) |
| `originRebaseDistance` | `500 m` |
| `viewRadius` | `2.5 m` |
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
| `generatorVersion` | `1` |

## 10. Risks

| Risk | Mitigation |
|---|---|
| Float precision over long pans | Rebase at `originRebaseDistance`; sector coordinates stay stable. |
| Overlapping systems | `sectorSize >= 2 * systemRadius + gap` plus a neighbor spacing check. |
| Generation changes invalidate saves | Version generation (`generatorVersion`); remap active planets by position. |
| Shape overlap with anchor | Keep planets outside the anchor radius and enforce min separation. |
