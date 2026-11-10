# Transit routes — one colour per route, and the cube's keep-out shown

**Status:** next · presentation · updated 2026-11-10 — every ship's route gets its own colour, like a
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
  jobs vary. U2 ships the derived default (job id when it parses, otherwise route index 0); the
  per-puzzle override is a follow-up and does not block the colours.
- **Origin and destination differ by shape, not colour:** the origin a filled ring, the destination an
  open ring (both in the route colour). The current `Origin`/`Destination` prefabs are the same single
  renderer, so this is new authored art (U3), not a tint.
- **Shared endpoints read as interchanges.** A planet that ends more than one route (fan-out, converge)
  gets a neutral ring with one small pip per route colour, like an interchange station on a transit
  map.
- **The outcome is on the line's end** (decided 2026-11-10). The line keeps its route colour; the end
  marker shows arrive (success ring), crash (danger cross) or miss (plain end). The pending preview is
  the route colour at the dimmed alpha, so the routes still read while shaping.
- **The keep-out covers every ship and is visible.** The cube may not influence any ship's origin or
  destination, and the player sees where it cannot go. The forbidden volume is the cube's own box
  inflated by `falloff × min half-extent` around each endpoint — cube-sized, not a small sphere — and
  it follows the cube as it resizes.
- **Authored, not invented.** Rings, pips, end markers and keep-out markers are authored prototypes,
  pooled; runtime code places and tints them.

## Units

### U1 — the keep-out, for every ship

- `ZoneConditions` carries every ship's endpoints, not one pair; `CubeView.CanPlace` checks all of them
  and reports which one it hit. `PuzzleData.ToZoneConditions` builds the list from `ships` (falling back
  to `origin` / `destination` for single-ship data).
- While the cube is held for a move (one-hand grab, translatable jobs), show a faint ghost of the
  cube's keep-out volume around each endpoint; when a move is refused with `OutOfBounds`, the marker
  it hit flashes danger. The ghost's size comes from the cube's current box and falloff, so it is
  correct after a frame resize; a frame-resize refusal keeps the frame flash.
- Test: conditions built from a two-ship puzzle refuse a frame over ship 2's origin and report that
  endpoint; empty `ships` falls back to `origin` / `destination`.
- **Exit:** in a movable two-ship job the cube cannot be dropped over any endpoint, and the player
  sees the zones it avoids while moving.

### U2 — route colours on the line

- A route palette asset wired on the spawner. The derived default: the job id (when it parses) picks
  the job's starting palette entry, and each route takes the next entry, wrapping past the palette
  length (`01_03` with two routes → entries 3 and 4); an unparsed id starts at 0. An empty or
  unparsed id must not throw: the two-ship test asset ships `id:` empty, and the inspector's Preview
  field command builds a real session (`PuzzleFieldPreview.Show`).
- Resolve each route's colour once per job (session or a small route snapshot) and pass it to the line
  pool; the view does not read the puzzle.
- `TrajectoryLineView` colours each line and its baseline by its route; the pending preview is the
  route colour at `pendingAlpha`; the end marker shows arrive (success ring), crash (danger cross) or
  miss (plain end). The miss heading line stays, in the route colour. Remove the now-dead outcome
  colours from the view and the old origin/destination colours from the spawner.
- Scope: the ship/trail and the map's route line keep their current colours here; matching them is a
  follow-up.
- Tests: palette index from a parsed id and from an empty id; two routes in one job get distinct
  entries; wrap past the palette length.
- Update the design docs that now own this language: [the cube](../../design/systems/cube.md)
  §Feedback (line and end marker) and [core loop](../../design/core-loop.md) §1 (what the player
  reads). Neither currently describes endpoint colours or outcome-coloured lines, so this adds the
  language rather than amending it.
- **Exit:** two routes read as two colours end to end; arrive and crash are still obvious at the
  line's end.

### U3 — endpoint shapes and interchanges

- Pool endpoint planets per job: one per distinct endpoint position, not one origin and one
  destination. `SpawnSources` excludes every endpoint index, and all bulk passes (`SetLabelsVisible`,
  `SetStateDim`, `SetMapFade`, `Hide`) cover the pool.
- Author the shape language: origin a filled ring, destination an open ring; a shared endpoint is a
  neutral ring with one pip per route colour, authored and pooled.
- Endpoint shape is chosen by role, not by the source seed (`FieldPlanetBehaviour.Spawn` currently
  picks the renderer variant by index).
- `FieldPlanetSet.Destination` stays the primary ship's destination, so `PlayArrival` / `PlayStamp`
  keep working on a two-ship solve.
- Tests: a shared endpoint yields one planet with two route pips; distinct endpoints yield 2N planets.
- **Exit:** a fan-out or converge job reads as an interchange; the solve payoff still lands on the
  primary destination.

## Manual check (headset)

1. Two-ship job: each route has its own colour on its line and both endpoints; origin and destination
   tell apart by shape.
2. A fan-out or converge job: the shared planet shows an interchange ring with one pip per route
   colour.
3. A pinch that crashes ship 2: its line keeps its colour and ends in a danger cross.
4. Single-ship jobs: one coloured route, a different colour on a different job; the pending preview
   still reads as that route.
5. Movable job: while moving the cube, the keep-out volume around every endpoint is visible; a refused
   drop flashes the one it hit.
6. Fixed-cube jobs 1–3: no keep-out markers appear.
7. Job 4 (first movable job) still feels the same apart from the visible keep-out.
8. Clean import: the palette and every authored marker, ring and pip are wired; no missing-prototype
   errors in the console.
9. Tuning: palette contrast against the lattice and the meter; keep-out visibility (faint, not a wall).
