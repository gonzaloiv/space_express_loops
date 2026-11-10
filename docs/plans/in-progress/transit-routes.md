# Transit routes — one colour per route, and the cube's keep-out shown

**Status:** now · presentation · updated 2026-11-10 — every ship's route gets its own colour, like a
transit map; the outcome moves to the line's end; the cube's no-go zone around every endpoint is shown
and covers every ship. From the two-ship headset test.

**Related:** [two-ship test](two-ship-test.md) · [the cube](../../design/systems/cube.md) ·
[plans board](../README.md)

## Goal

In the two-ship test only ship 1's planets are coloured (`FieldPlanetSet` has one origin and one
destination prototype), and the trajectory lines are coloured by outcome, so two routes do not read as
two routes. In movable-cube jobs the cube refuses to sit over ship 1's endpoints with no visible
reason, which reads as a glitch, and it does not protect ship 2's endpoints at all
(`PuzzleData.ToZoneConditions` passes one origin and one destination).

## Contract

- **One colour per route, every job** (decided 2026-11-10). A route is a ship: its trajectory line,
  its baseline and its two endpoint planets share one colour. Single-ship jobs are one route.
- **Each job picks its colours.** A small palette of route colours (4–5 `ColorValue`s, distinct from
  accent, success, danger and neutral); each puzzle chooses which palette entries its routes use, so
  jobs vary. Default: derived from the job id when the puzzle does not choose.
- **Origin and destination differ by shape, not colour:** the origin a filled ring, the destination an
  open ring (both in the route colour).
- **Shared endpoints read as interchanges.** A planet that ends more than one route (fan-out, converge)
  gets a neutral ring with one small pip per route colour, like an interchange station on a transit
  map.
- **The outcome is on the line's end** (decided 2026-11-10). The line keeps its route colour; the end
  marker shows arrive (success ring), crash (danger cross) or miss (plain end). The pinch preview stays
  dimmed.
- **The keep-out covers every ship and is visible.** The cube may not influence any ship's origin or
  destination, and the player sees where it cannot go.
- **Authored, not invented.** Rings, pips and keep-out markers are authored prototypes, pooled; runtime
  code places and tints them.

## Units

### U1 — the keep-out, for every ship

- `ZoneConditions` carries every ship's endpoints, not one pair; `CubeView.CanPlace` checks all of them.
  `PuzzleData.ToZoneConditions` builds the list from `ships` (falling back to `origin` / `destination`
  for single-ship data).
- While the cube is held for a move (one-hand grab, translatable jobs), show a faint keep-out marker
  around each endpoint at the distance where the cube would influence it; when a move is refused with
  `OutOfBounds`, the marker it hit flashes danger.
- Test: conditions built from a two-ship puzzle refuse a frame over ship 2's origin.
- **Exit:** in a movable two-ship job the cube cannot be dropped over any endpoint, and the player
  sees the zones it avoids while moving.

### U2 — route colours

- A route palette asset and a per-puzzle choice (an index list on `PuzzleData`, or derived from the id).
- `FieldPlanetSet` colours every ship's endpoints (not only ship 1's) with its route colour, origin
  filled, destination open; shared endpoints get the interchange ring with one pip per route.
- `TrajectoryLineView` colours each line (and its baseline) by its route; the outcome moves to the end
  marker (success ring, danger cross, plain end). The pending preview keeps its dimmed alpha.
- Update the design docs that describe endpoint colours and outcome-coloured lines
  ([core loop](../../design/core-loop.md), [the cube](../../design/systems/cube.md)).
- **Exit:** two routes read as two colours end to end; a shared endpoint reads as an interchange;
  arrive and crash are still obvious at the line's end.

## Manual check (headset)

1. Two-ship job: each route has its own colour on its line and both endpoints; origin and destination
   tell apart by shape.
2. A fan-out or converge job: the shared planet shows an interchange ring with both colours.
3. A pinch that crashes ship 2: its line keeps its colour and ends in a danger cross.
4. Single-ship jobs: one coloured route, a different colour on a different job.
5. Movable job: while moving the cube, the keep-out around every endpoint is visible; a refused drop
   flashes the one it hit.
6. Job 4 (first movable job) still feels the same apart from the visible keep-out.
7. Tuning: palette contrast against the lattice and the meter; keep-out visibility (faint, not a wall).
