# Bake construct — fleets with separate ships

**Status:** now · puzzle creator · updated 2026-11-27 — two and three ships from different origins,
sharing one blocker (Clear) or one bend (Curve). U1 done: the fleet plumbing, fixtures and fast tests
are in; the Clear/Curve recipes are in and the four spikes and 20-seed rates are the next
bake-checkout pass.

**Bakes:** off the editor — the spikes and 20-seed rates are Creator tests that call
`PuzzleConstructBake.Bake` directly (the scan still reports `UnsupportedMechanism` for construct
fleets until [jobs](bake-construct-jobs.md) U1); run them in the bake checkout. The fleet `bake.ps1`
seam lands in jobs U1.

**Related:** [plans board](../README.md) ·
[construct single](../in-progress/bake-construct-single.md) (its recipes and Clear fix are the base) ·
[construct wire](bake-construct-wire.md) (the `Generator` seam; fleet scanning stays in jobs U1) ·
[construct fan-out](bake-construct-fanout.md) (next)

Same rule as the core: only what today's jobs use; **no anchored (`fixedMass`) planets**.

## How the shared planet shapes the build

- **The fleet's chords come first.** The core picks a chord before the lesson and protects its
  endpoints (`PuzzleConstruct.Place`, `PuzzleConstructLesson.Apply`), so the fleet step picks one pair
  per ship first (target length, endpoint clearance, distinct endpoints for `Separate`), then the
  lesson places the shared planet against them: the blocker at the chords' crossing (inside every
  block band, `radius × 2`), the bend in the common region beside the crossing. Both are decoy moves,
  so `PlanetCount` is unchanged. Every ship travels in `PuzzleLessonContext`; every endpoint check
  (`PuzzleLessonPlacement.ClearOfEndpoints`, `PuzzleLessonAnswer.Rejected`) covers all of them.
- **Tuning is an intersection, and `PuzzleLessonScale.TryMiddle` already does it.** It samples
  `SimulateAll().AllArrived()`, so it returns the middle of the all-ships landing run; only the
  multi-ship setup is new (`PuzzleLessonRun.Setup` builds one ship today). No new range helper.
- **Decoys clear every ship.** The decoy step is single-chord today (`PuzzleDecoyField` holds one
  origin/destination, and `NearestUnprotected`/`InCorridor`/`Fits` read it): extend it to every ship's
  start and answered path, and move the decoy nearest the run whose ship did not arrive. The failing
  ship comes from the all-ships session; `PuzzleBakeResult` does not carry it.
- **The shared blocker's role.** `PuzzleRunBlockers.SharedCount` counts a planet that blocks at least
  two ships, so the three-ship copy also needs `PuzzleVerifier.Solved` (every ship lands) to force the
  third ship onto the same cube.
- **The field's single-ship fields follow ship 0.** `origin`, `destination` and `captureRadius` are ship
  0's; `zoneCentre` is the bounding box of every endpoint, as `PuzzleMultiShipBake.Variant` sets them.
  `PuzzleStartCheck`, the start spot and the validator read these. `StartFails`/`Solved` run all ships;
  the curve gate is ship 0's (confirmed 2026-11-27: ship 0 only, no fleet variant).
- **Materialize the fleet fields; do not reuse `Confirm`.** `PuzzleMultiShipBake.Variant` sets `ships`,
  `zoneCentre`, `frames` and `entropyBudget` but is private; `Confirm` is the search's whole
  verify/reject gate. The core kept `PuzzleFieldBake.Variant` private and exposed `Check`, so do the
  same: the construct materializer sets `ships` (each destination planet's radius), the bounding-box
  `zoneCentre` and ship 0's `captureRadius`; `PuzzleFieldBake.Check` recomputes frames, routes,
  entropy and the sun rule. No second `Confirm`.
- **Build, then verify** on the main thread with the core's gate, plus the fleet checks already in
  `PuzzleJobValidator` (every ship lands, relation, ship count). `PuzzleConstructBake.Finish` already
  runs the validator, so the only new wiring is `ships`.

## Prepare the fixtures

- Add pending `PuzzleConstructScratch` contracts, `Generator = Construct`, `Attempts = 4`:
  `02_07C` (02_07's Clear two-ship), `02_07C3` (the same shape, `Ships = 3`, `PlanetCount = 12` for
  the third pair; 02_08 is the parked two-ship job, not this fixture), `02_03C` (02_03's field with
  `Mechanism = Curve`, so the role is checked; the shipped 02_03 stays `Any`) and `02_06C` (02_06's
  Curve three-ship).
- Exit: the contracts resolve through `PuzzleJobContracts.ById` and stay out of `Enabled`.

## Units

### U1 — fleet plumbing

- The plain data and steps gain the other ships: `PuzzleConstructedField` carries the ship list and a
  per-ship capture radius; `PuzzleConstructChord.TryPick` picks one pair per ship (target length,
  endpoint clearance, distinct endpoints for `Separate`, and for these recipes a common region);
  `PuzzleLessonContext`/`PuzzleLessonPlan` carry every ship; `PuzzleConstructLesson` protects every
  endpoint; `PuzzleConstructDecoys` checks every ship's path and can target a given ship's chord;
  `PuzzleLessonRun.Setup` builds the multi-ship `ShipSpecs`; `PuzzleConstructBake.Materialize` sets
  `ships`, the bounding-box `zoneCentre` and ship 0's `captureRadius`.
- Add fleet recipes (new `PuzzleFleetClearLesson`/`PuzzleFleetCurveLesson`, reusing
  `PuzzleLessonScale`/`PuzzleLessonPlacement`/`PuzzleLessonAnswer` with the multi-ship context);
  `PuzzleLessonChoice.For` returns them when `brief.IsMulti`, so the scratch contracts select them.
- Exit: fast unit tests on fixed seeds (chord pairs, shared context, decoy move, all-endpoint checks),
  plus a no-bake replay: the shipped `02_07` field and its stored answer pass `PuzzleConstructBake`
  with its stored ships (the `PuzzleConstructVerdictTests` pattern).

### U2 — shared blocker (Clear)

- **Spike first.** One seed of `02_07C` and one of `02_07C3`, with a decoy moved to the chords'
  crossing and the answer `TryMiddle` between the reachable floor and 1. If either does not bake, stop
  and record: fleet Clear stays on the search.
- The blocker sits where the chords cross and blocks every start; the answer is one cube shrinking it
  (02_07's lesson, built on purpose). The cube must keep clear of the sun (`NoSunAnswer`), the decoys
  clear of the chord, and `MinRoutes = 1` holds (the fleet route count is ship 0's).
- Exit: 20 seeds of `02_07C` and 20 of `02_07C3` through `PuzzleConstructBake.Bake`; bar 12/20 as
  `PuzzleConstructSingleTests.Rate`; record the rates and the status histogram here.

### U3 — shared bend (Curve)

- **Spike first.** One seed of `02_03C` and one of `02_06C`: a scalable blocker near each chord so
  every start fails, one bend beside the crossing, the answer grows the bend. If it does not converge,
  stop and record; 02_03/02_06 stay search-baked (02_06's shipped field no longer solves, so its
  contract/seed is a target, not a proof).
- **Shrinking must not solve.** The single-ship sweep cannot reject a fleet (other blockers hold their
  ships, so `AllArrived` is never true). Use the fleet-aware search instead:
  `PuzzleFleetSolution.TryFind(field, sim, Clear, out _)`; reject the seed if a Clear-solved zone
  exists. This is the real shortcut — one cube shrinking several blockers.
- **The bend's cube must not contain a blocker** (otherwise the answer also shrinks it and the kind is
  Mixed, failing the Curve role). Keep the bend clear of the blockers' bodies and assert the answer
  zone holds only the bend.
- Exit: 20 seeds of `02_03C` and 20 of `02_06C`; bar 12/20; record the rates, the Clear-search rejects
  and the kind histogram here.

## State

- **U1 — done.** The fleet plumbing is in: `PuzzleConstructedShip`,
  `PuzzleConstructedField.Ships/HubIndex/HubHome`, `PuzzleConstructChord.TryPickShips` (forwarding
  to the new `PuzzleConstructFleetChord`: one disjoint pair per ship, the chords' midpoint centre and
  the biggest decoy whose block band reaches every chord), the multi-ship `PuzzleLessonContext`
  (`Ships`, `Chord`, `HubIndex/HubPosition`, `CaptureRadius`), all-endpoint placement/answer checks,
  every-chord decoy clearance with a per-ship target, `PuzzleLessonRun.Setup` with `ShipSpec`s, and
  `PuzzleConstructBake.Materialize` setting `ships`, the bounding-box `zoneCentre` and ship 0's
  `captureRadius` through the existing `Check`. The fixtures are in `PuzzleConstructScratch`.
  `PuzzleConstructFleetTests`: 7 fast tests green (chords/hub for two and three ships, decoy
  corridors, context, protection, decoy target, the shipped 02_07 replay); the construct, verdict and
  single Curve bakes stay green (20/20 in the Creator subset; the fast Game suite is 101/101).
- **U2/U3 — recipes in, spikes pending.** `PuzzleFleetClearLesson`/`PuzzleFleetCurveLesson` are
  selected by `PuzzleLessonChoice` for `IsMulti`; the Curve gate
  (`PuzzleFleetSolution.TryFind(Clear)`) is in `PuzzleConstructBake.Finish`. The one-seed spikes and
  20-seed rates are `[Explicit]` tests (`PuzzleConstructFleetTests`); run them in the bake checkout
  (`-runTests -testFilter PuzzleConstructFleetTests`).
- Next: the four spikes in the bake checkout, then the rates; record them here and decide whether the
  3-ship Clear copy (`02_07C3`) and the shared bend hold.

## Review (2026-11-27)

U1's plumbing is in and tested; U2/U3's recipes are in but unmeasured (the spikes and rates are the
next bake-checkout pass), so this is a review of the code, not the yield.

- **The shared planet is found, not placed first.** `PuzzleConstructFleetChord` picks the ships'
  chords from the generated field, then puts the hub at their endpoints' centre and borrows the biggest
  decoy whose band reaches every chord. The plan says to place the shared planet first and then each
  ship's endpoints so every chord crosses it. With chords picked first, two far-apart or parallel
  chords leave no hub, and the yield depends on luck again. Let the spikes decide: if they come back
  low, invert the order as planned.
- Moving the hub (`PuzzleFleetClearLesson`) does not check separation from endpoints or other planets;
  the verdict catches overlaps, but each one costs an attempt. Check clearance in the lesson move.
- Keep the spikes and rates `[Explicit]` as they are, and record them here before
  [fan-out](bake-construct-fanout.md) starts.

### To do (from the review)

1. **Done — the Clear hub's move is checked.** `PuzzleFleetClearLesson` moves the hub through
   `PuzzleLessonPlacement.TryAt`, so the move is clear of every endpoint, the other planets and a
   Centre sun before the setup is built; an overlap fails the seed instead of costing a verdict.
2. Run the spikes and the 20-seed rates in the bake checkout and record them here. (Checked
   2026-11-27: still open. Jobs U2 already measured the Curve copies through `bake.ps1` at 0/20, all
   `NoRun`; the scan now names the failing build step in `Problems`, so rerun and record which step
   fails before changing the order.)
3. If the rates are low, place the shared planet first and then each ship's endpoints, as the plan's
   "How the shared planet shapes the build" says.
