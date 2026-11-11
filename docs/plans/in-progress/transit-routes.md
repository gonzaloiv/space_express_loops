# Transit routes — one colour per route, and the cube's keep-out shown

**Status:** now · presentation · updated 2026-11-11 — every ship's route gets its own colour, like a
transit map; the outcome moves to the line's end; the cube's no-go zone around every endpoint is shown
and covers every ship, in the bake as well as at runtime. From the two-ship headset test. U4 (one red
cube on a refused move, box-only keep-out) is implemented; review fixes R1–R4 are open (§U4 review).

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

### U4 — one red cube on a refused move, and a smaller keep-out

From the headset (2026-11-11): a faint box around every endpoint while moving is too much — with two or
three ships it is four to six boxes, mostly around planets the player is nowhere near. Replaces U1's
per-endpoint ghosts.

- **Nothing shows while a move is valid.** When a move is refused, **one red copy of the cube** — the
  same frame the player holds, tinted danger — appears at the attempted position and follows the hand
  while the move stays blocked. The real cube stays at its last valid spot. The ghost hides as soon as
  the hand is back on a valid spot or the grab ends.
- **The why:** the protected endpoint planet pulses once (danger) when the refusal starts.
- **The data — a refusal state, not only an event.** `CubeGestureSession.Move` computes the attempted
  centre and drops it, and `CubeFeedback.Refuse` dedupes `OutOfBoundsAt` per `(reason, endpoint)`, so an
  event alone would freeze the ghost at the first refused centre. Add a **move-refusal state**: a small
  value type (`CubeMoveRefusal`: blocked, endpoint index, attempted centre, size, with `Set` and
  `Clear`) owned by `CubeFeedback` on the `CubeView`, exposed read-only through `ZoneCubePool` for the
  ghost controller. `CubeGestureSession.Move` sets it on every blocked frame and clears it on the next
  valid move; `CubeGrabListener.EndGesture` clears it (the listener owns the gesture end) — not the
  `RefuseSecs` timer. The deduped `OutOfBoundsAt` stays as the start signal for the planet pulse.
- **One red frame, not two.** `CubeFeedback.Apply` tints the real frame danger for any refusal, which
  would show two red cubes. Key the exclusion on the **endpoint index** (`>= 0`), not on the reason or
  `IsRefusing`: resize also refuses with `OutOfBounds` but no endpoint (`CubeGestureSession.cs:149`) and
  must keep its frame flash, as do locked-gesture and over-budget refusals.
- **The pulse seam.** Nothing maps an endpoint index to a planet view today (`FieldPlanetSet` exposes
  only `Destination`). Add `FieldPlanetSet.PulseEndpoint(Vector3 point)` (finds the planet at that
  endpoint, primary or pooled) and `FieldPlanetBehaviour.PlayRefusal` (a short danger pulse beside
  `PlayArrival`, through `NodeBody`'s tint, not `.material`). The pulse ends through a restore path on
  `FieldPlanetBehaviour` that re-applies the current fade/dim (`ApplyTint`), not `NodeBody.ClearTint`
  alone, so a faded or dimmed planet does not pop back. The view passes
  `ZoneConditions.Endpoints[index]`; the pulse logic stays out of `SpaceShapingView` (238 lines). The
  endpoint→source lookup lives in Domain (`SpaceShapingSession.EndpointSource(point)`, beside
  `ShipEndingAt`) so it is fast-suite tested; `PulseEndpoint` itself is checked in the headset pass
  (FieldContent and its authored planet prototypes are outside the fast suite).
- **The view.** `KeepOutGhost.prefab` is not the cube's frame today (built-in cube mesh, `Trajectory.mat`):
  re-author it with the cube frame's mesh and `CubeFrame.mat`, danger-tinted, one pooled instance sized
  from the cube's current box (`Size`, not `KeepOutSize`), with `body` re-pointed at the frame
  renderer. Drop `idle`, `flashSecs` and the `LateUpdate` flash from `EndpointKeepOutView` and their
  serialized values from the prefab; delete `CubeView.KeepOutSize` and `ZoneCubePool.KeepOutSize`, the per-endpoint
  `EndpointKeepOutPool.Show` path and `MoveStarted`. **Keep `MoveEnded`** as the hide signal:
  `MoveCompleted` fires only on a moved gesture, so a grab that was only ever refused would leave the
  ghost up.
- **A smaller keep-out — the box only (decided 2026-11-11).** The cube only acts inside its own box
  (`SpaceZone.FactorAt` is 0 on and outside the faces), but `SpaceZone.Influences` also forbids a
  `falloff × extent` band outside it. The endpoint rule becomes `Contains` (the endpoint's centre
  inside the box). That only loosens the rule, so existing bakes stay valid: **no rebake**. Adding the
  planet's radius is *not* in this unit: with radii ≈ 0.035–0.045 m against a band ≈ 0.032 m it can be
  stricter than today and would need a sphere-vs-box distance and a rebake — a separate call if the
  headset shows the frame overlapping a planet's body.
- **One shared rule, every caller.** One method used by the cube (`CubeView.CanPlace`) and the bake:
  `PuzzleZoneCandidates.ContainsEndpoint`, the multi-ship start frame, the validator **and
  `PuzzleShipScan`'s rescue check** (`PuzzleShipScan.cs:123`, asserted in `PuzzleShipScanTests`), so the
  scan and the cube never drift. Once `PuzzleShipScanTests` moves to the box rule, delete
  `SpaceZone.Influences` and `PuzzleZone.Influences` if nothing else calls them.
- Tests: fast suite — a frame outside an endpoint's box by less than the old band is allowed, a frame
  over the endpoint is refused (`CubeView.CanPlace` / `ZoneConditions`); `CubeMoveRefusal` keeps the
  latest attempted centre across repeated `Set`s and `Clear` empties it (tested as a value, since
  `CubeGestureSession.Move` needs live hand input); `EndpointSource` finds a ship-2 endpoint's source.
  Creator suite — update `PuzzleShipScanTests` to the box rule. Headset — `PulseEndpoint` pulses the
  right planet and it returns to its faded/dimmed state.
- **Exit:** moving the cube shows nothing until it is refused; a refused move shows one red cube at the
  hand and pulses the protected planet; the cube can sit closer to endpoints than before.

#### U4 review — 2026-11-11

U4 is implemented as planned and the fast suite passes (106/106): the box-only rule everywhere
(`Influences` deleted), the `CubeMoveRefusal` state set per blocked frame and cleared on a valid move,
on `MoveEnded` and on `PlaceFrames`, the real frame no longer flashing for an endpoint refusal (keyed on
the endpoint index, so resize keeps its flash), the ghost re-authored with the cube frame's mesh, and
`PlayRefusal` restoring through `ApplyTint`. Fixes before closing:

- **R1 — the pulse finds its planet by position, not by source.** `FieldPlanetSet.PulseAt` compares a
  planet's current `localPosition` to the endpoint's home point. The separation constraint can push an
  endpoint planet (a grown neighbour), and then no planet matches and the pulse silently does nothing.
  Match by the source index `EndpointSource` already returns (the planet is spawned with its source
  index), not by position.
- **R2 — a collection loop in the pool.** `ZoneCubePool.MoveRefusal` loops over the cubes inline; move it
  to `CubeViewExtensions` (e.g. `FirstMoveRefusal`) beside `AnyScaling` / `AnyHeld`.
- **R3 — the ghost lives in the planets' space, the refusal in the cube's.** `EndpointKeepOutPool` is
  parented under `sourceParent`, while the refused centre and size are in the cube's parent space. It
  works only because `sourceParent` is an identity child of `cubeParent` in `Puzzle.prefab`. Parent the
  ghost under `cubeParent`.
- **R4 — the commit is mixed.** The working tree also holds unrelated changes from the same session:
  the entropy post's move to the back edge (`FieldFrame`, `FieldEdgeMeter*`, the prompts), the upright
  Spend/Stars prompts, the pinch-indicator gate (`PinchIndicatorGate`, `IsHeld` / `AnyHeld` /
  `IsCubeHeld`, `TwoHandGestureIndicator.Shown`, `MetaAimFlagsHomeMenuInvoker.IsInMenuPose` in
  `_Modules`) and the judge-run/ending docs. Stage U4's files only for its completion commit.

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
5. Movable job: moving the cube freely shows no keep-out markers. Pushing it onto an endpoint shows one
   red cube at the hand, the endpoint planet pulses, and the real cube stays put; moving back clears it.
6. Fixed-cube jobs 1–3: no keep-out markers appear.
7. Job 4: the move feels the same; the cube can sit closer to a planet's endpoint than before, and only
   the red cube on a refusal and the line colour are new.
8. Clean import: the palette and every authored marker, ring and pip are wired; no missing-prototype
   errors in the console.
9. Tuning: palette contrast against the lattice and the meter; the red cube's strength (clear, not
   alarming); note if the frame overlapping a planet's body reads wrong (that would be a separate
   radius rule with a rebake).
