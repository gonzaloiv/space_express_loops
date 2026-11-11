# Transit routes — one colour per route, and the cube's keep-out shown

**Status:** now · presentation · updated 2026-11-11 — every ship's route gets its own colour, like a
transit map; the outcome moves to the line's end; the cube's no-go zone around every endpoint is shown
and covers every ship, in the bake as well as at runtime. From the two-ship headset test; review
applied 2026-11-11.

**Related:** [two-ship test](../backlog/two-ship-test.md) · [the cube](../../design/systems/cube.md) ·
[plans board](../README.md)

## Goal

In the two-ship test only ship 1's planets are coloured (`FieldPlanetSet` has one origin and one
destination prototype), and the trajectory lines are coloured by outcome, so two routes do not read as
two routes. In movable-cube jobs the cube refuses to sit over ship 1's endpoints with no visible
reason, which reads as a glitch, and it does not protect ship 2's endpoints at all
(`PuzzleData.ToZoneConditions` passes one origin and one destination).

## Contract

- **One colour per route, every job** (decided 2026-11-10). A route is a ship: its trajectory line,
  its baseline and its two endpoint planets share one colour. Single-ship jobs are one route. The map
  dioramas use the same colours, so a job looks the same on the map and in play.
- **Naming.** In code a ship's presentation route is a **`ShipRoute`**; the job's snapshot of them is
  a **`RoutePlan`**. `PuzzleData.routes` (baked solution clusters) keeps its meaning; do not reuse the
  word "route" for anything else in code.
- **Each job picks its colours.** A small palette of route colours (4–5 `ColorValue`s, distinct from
  accent, success, danger and neutral) on `SpaceShapingSettings`, which the controller and
  `FieldContentView` already share. The derived default is
  `(TryAsIndex(id) ?? 1) - 1 + shipIndex`, wrapped non-negative into the palette length
  (`((n % len) + len) % len`), so job `01_01` and an unparsed or empty id both start at entry 0, and
  `01_00` wraps instead of going to -1. The per-puzzle override is a follow-up. The palette is `Resources/Colors/Routes/`, in this order: **Amber** (today's destination
  colour), **Violet**, **Cyan**, **Magenta** — neighbours are far apart in hue, so two ships in one job
  never get close colours.
- **Origin and destination differ by shape, not colour:** the origin a filled ring, the destination an
  open ring (both in the route colour). The current `Origin`/`Destination` prefabs are separate
  single-renderer variants of `FieldPlanet`, so this is new authored art (U3), not a tint.
- **Shared endpoints read as interchanges.** A planet that ends more than one route (fan-out, converge)
  gets a neutral ring with one small pip per route colour, like an interchange station on a transit
  map.
- **The outcome is on the line's end** (decided 2026-11-10). The line keeps its route colour; the end
  marker shows arrive (success ring), crash (danger cross) or miss (plain end). The pending preview is
  the route colour at the dimmed alpha. `success`, `danger` and `pending` stay on the view for the
  marker; only the per-outcome line colours go.
- **The keep-out covers every ship, in the bake and at runtime, and is visible.** The cube may not
  influence any ship's origin or destination. The forbidden volume is the cube's own box inflated by
  `falloff × min half-extent` around each endpoint (`SpaceZone`), and it follows the cube as it
  resizes.
- **Authored, not invented.** Rings, pips, end markers and keep-out markers are authored prototypes,
  pooled; runtime code places and tints them, with property blocks, never `.material`.

## Units

### U0 — bake parity for every ship's endpoints

- One endpoint list on `PuzzleData`, **derived, not serialized**: every ship's origin and destination
  from `ShipSpecs.From`, falling back to `origin` / `destination` when `ships` is empty.
  `PuzzleZoneCandidates.ContainsEndpoint` checks the list; beam search, solution search, the verifier
  and the route count already go through it.
- `ToZoneConditions` builds from the same list and **`ZoneConditions` exposes it** (read-only), so the
  cube and U1's ghosts read endpoints from the conditions, never from the puzzle.
- **The start frame is checked too.** The multi-ship bake centres the start frame on the ships'
  bounding box (`PuzzleMultiShipBake`), and `PuzzleJobValidator.CheckStartFrame` only compares it to
  `zoneCentre`. The validator also rejects a start frame that influences any endpoint. The validator
  only runs in tests today, so `PuzzleMultiShipBake` checks its placed start frame against the same
  endpoint rule and rejects the seed (no nudge): a bad start is found at bake time, not in the headset.
- Re-bake the two-ship test levels; if the stricter keep-out invalidates the current bake, budget a
  new seed per level — or more centres, since `PuzzleMultiShipProbe` takes at most eight
  (`PuzzleZoneCandidates.MaxCentres`), sorted by the primary route. Job 1–7 data is single-ship and
  unchanged.
- Tests, fast suite: a zone over ship 2's origin is an endpoint hit; empty `ships` falls back;
  `ToZoneConditions` exposes every endpoint. Creator suite (`PuzzleJobValidatorTests`): the validator
  flags a start frame over ship 2's destination.
- **Exit:** no baked two-ship solution or start frame covers any ship's endpoint.

### U1 — the keep-out, visible

- `CubeView.CanPlace` returns a placement result (allowed, and the endpoint index it hit) instead of
  `bool`. The index travels on a **new, additive event** (`OutOfBoundsAt(int endpoint)`);
  `Refused(ZoneRefusal)` stays as is, so `PuzzleEditionState` and `PuzzleLabPanel` do not change.
- The new event dedupes on the endpoint, not the reason: `CubeFeedback.Refuse` coalesces by reason
  within `RefuseSecs`, which would swallow a drop refused at endpoint B right after endpoint A.
- The ghosts take their endpoints from `ZoneConditions` (U0), one pooled ghost per entry.
- `CubeGrabListener` exposes the move's start and end (`MoveStarted` / `MoveEnded` or `IsMoving`), not
  only `MoveCompleted`.
- While moving (translatable jobs only), a pooled keep-out ghost per endpoint shows faintly, sized from
  the cube's current box and falloff; a refused drop flashes the ghost it hit. A frame-resize refusal
  keeps the frame flash.
- Test: the placement result names ship 2's origin when a frame sits on it.
- **Exit:** in a movable two-ship job the player sees the zones the cube avoids while moving, and a
  refused drop says which one.

### U2 — route colours on the line

- `LevelData.TryAsIndex` (new); `AsIndex` stays as is, it backs `ManualRevision`. An empty id (the
  two-ship asset, the inspector's *Preview field*) starts at palette entry 0.
- `RoutePlan` is a plain snapshot built once per job: per ship, its colour and its endpoints. It
  replaces adding parameters to `TrajectoryLineView.Show` (already at five).
  - **Play:** the controller builds it on `Enter` and hands it to the spawner, which stores it; the
    line pool and `FieldPlanetSet` both read that one stored snapshot.
  - **Map:** `PuzzleDioramaView` *is* a `FieldContentView`; `FieldContentView` builds its own
    `RoutePlan` from the puzzle and settings it already receives and passes it to its `FieldPlanetSet`.
  - Views below the spawner and `FieldContentView` never read the puzzle.
- Extract the end marker from `TrajectoryLineView` (241 lines) into its own collaborator with the
  authored success ring and danger cross prototypes; the line view only colours the line and baseline.
- Tests (fast suite): palette index from a parsed id and an empty id; two ships get distinct entries;
  wrap past the palette length.
- Update [the cube](../../design/systems/cube.md) §Feedback and [core loop](../../design/core-loop.md)
  §1 with the route colour and the end marker.
- **Exit:** two routes read as two colours end to end, on the map and in play; arrive and crash are
  obvious at the line's end.

### U3 — endpoint shapes and interchanges

If U0–U2 fill the day, U3 moves to its own plan and takes manual-check items 1–2 (shapes,
interchange) with it.

**Landed early (2026-11-11 review fix):** each ship's endpoint planets carry that ship's colour
(`SpaceShapingSession.ShipEndingAt`); a shared endpoint takes the first ship's colour until the
interchange lands. The line's end is authored on `Trajectory.prefab` (`TrajectoryEndMarker`): a dot
while pending, a success ring on an arrival, a danger cross on a crash. Still open in U3: the endpoint
pool, the origin/destination ring shapes and the interchange pips.

- A plain `RouteEndpoints` type derives the distinct endpoint positions, each one's role (origin,
  destination, interchange) and its route pips from the `RoutePlan`; it is tested in the fast suite.
- `FieldPlanetSet` (238 lines) gets an endpoint pool collaborator instead of more code: one pooled
  planet per distinct endpoint, shape chosen by role. `SpawnSources` excludes every endpoint index; the
  bulk passes (`SetLabelsVisible`, `SetStateDim`, `SetMapFade`, `ClearMapFade`, `ShowOnlySunLabel`,
  `Hide`) cover the pool.
- Author the shapes: origin filled ring, destination open ring, interchange neutral ring with pips.
- `FieldPlanetSet.Destination` stays the primary ship's destination, so `PlayArrival` / `PlayStamp`
  keep working on a two-ship solve.
- Tests (fast suite, on `RouteEndpoints`): a shared endpoint yields one interchange with two pips;
  distinct endpoints yield 2N endpoints.
- **Exit:** a fan-out or converge job reads as an interchange; the solve payoff still lands on the
  primary destination.

## Not in this plan

- `ContentFade` measures only the primary segment, so the lattice around ship 2's route can fade.
  Fix it with the two-ship work if the headset shows it.

## Manual check (headset)

1. Two-ship job: each route has its own colour on its line and both endpoints; origin and destination
   tell apart by shape.
2. A fan-out or converge job: the shared planet shows an interchange ring with one pip per route
   colour.
3. A pinch that crashes ship 2: its line keeps its colour and ends in a danger cross.
4. Single-ship jobs: one coloured route, a different colour on a different job, the same colour on
   its map diorama; the pending preview still reads as that route.
5. Movable job: while moving the cube, the keep-out around every endpoint is visible; a refused drop
   flashes the one it hit.
6. Fixed-cube jobs 1–3: no keep-out markers appear.
7. Job 4: the move feels the same; only the visible keep-out and the line colour are new.
8. Clean import: the palette and every authored marker, ring and pip are wired; no missing-prototype
   errors in the console.
9. Tuning: palette contrast against the lattice and the meter; keep-out visibility (faint, not a wall).
