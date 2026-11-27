# Bake construct — fan-out fleets

**Status:** now · puzzle creator · updated 2026-11-27 — U1–U3 code in (picker, aim, lesson, fixtures;
the project compiles clean). The fast run needs the Test Runner after the MCP bridge dropped; the
spikes and 20-seed rates are the next bake-checkout pass. The search stays the fallback.

**Bakes:** off the editor — U2's spike and U3's 20-seed rates are `[Explicit]` Creator tests; run them in
the bake checkout. The fleet `bake.ps1` seam lands in [jobs](bake-construct-jobs.md) U1.

**Related:** [plans board](../README.md) ·
[construct separate](bake-construct-separate.md) (its recipes are the base) ·
[construct jobs](bake-construct-jobs.md) (next)

Same rule as the core: only what today's jobs use; **no anchored (`fixedMass`) planets**.

## How the shared origin shapes the build

- Today's fan-outs are 02_01/02_04 (two ships; Ring, Spiral) and 02_09 (three; Cluster). The validator
  already requires one origin and distinct destinations (`PuzzleMultiShipBake.Allowed`,
  `PuzzleMultiShipBake.cs:111-123`), so every ship carries the **same `OriginIndex`** and its own
  destination planet; `PuzzleConstructedShip` already holds both values and the indices.
- Unlike Separate, the ships cannot be picked once and left: a ship launches at its destination, so
  moving a destination moves its launch. The order is origin + bend first, then the bend's provisional
  answer, then each destination on the answered path. This is the only construct step that simulates;
  it runs on the main thread with the bake (Construct jobs are serial, jobs U1), not in the plain steps.
- The bend is a free planet near the shared origin and clear of it (`PuzzleLessonPlacement.TryAt`
  enforces endpoint, sun and planet clearance); the answer is one cube growing it. Every destination
  stays inside a 90° cone, so one bend bends every path the same way.
- Destinations stay on the generated shape: each aim step re-places a destination at the nearest free
  spot of a fresh `PuzzleFieldGenerator.Generate` of the same shape from the attempt's stream, subject
  to `PairFits` and the cone. An endpoint dropped onto the answered path but off a Spiral/Cluster arm
  breaks the "looks searched" rule the core keeps for decoys.
- `Curve` only. The shipped 02_01/02_04 answers shrink a planet near the start (scale 0.6, both
  contracts `Any`), and jobs U2 left the shipped 02_09 on its search bake; the constructor's users are
  the freelance fleets and later chapters. The code lands now; a chapter 2 swap stays a separate
  content call. A Clear fan-out would be the Separate Clear recipe with a shared origin, not a new
  mechanic.

## Prepare the fixtures

- Add pending `PuzzleConstructScratch` contracts, `Generator = Construct`, `Attempts = 4`,
  `Relation = FanOut`:
  - `02_04C` — 02_04's field (Spiral, 12 planets, 2 ships) with `Mechanism = Curve` pinned; 02_04 is
    `Any`, so `PuzzleLessonChoice.For` would otherwise return null and the build be `NoSolution`.
  - `02_09C` — 02_09's field (Cluster, 12 planets, 3 ships, Curve), keeping
    `MinScaleDelta = 0.35` / `MinStartDistance = 0.15`.
- Exit: both resolve through `PuzzleJobContracts.ById` and stay out of `Enabled`.

## Units

### U1 — the seams

- `PuzzleConstruct.Fleet` branches on `Relation == FanOut` to a new `PuzzleConstructFanOut.Build`, which
  picks and aims and returns the same `Parts`-shaped result; the Separate picker and its tests are
  untouched. It picks one shared origin and N destinations in the run band and the 90° cone (reusing
  `PuzzleConstructChord.PairFits`/`EndpointClearance`), then a free planet near the origin as the bend.
- The aim, per ship: build the provisional answer at the bend (`PuzzleLessonScale.TryMiddle` with `min`
  = `1 + brief.MinScaleDelta`), simulate it (`PuzzleLessonRun.Setup`/`Arrives`), take the answered
  path's closest point to the destination (new `PuzzleRunGeometry.ClosestPoint(TrajectoryResult,
  Vector3)`), and re-place the destination at the nearest free shape spot that fits `PairFits` and the
  cone. Update the planet, `Ships[i].Destination` and `CaptureRadius` (`WithPlanet` moves a body only).
  Stop on a 3-sample landing plateau (`PuzzleLessonScale.MinRun`), not one landing; fail the seed after
  a fixed step count.
- New `PuzzleFanOutCurveLesson`, returned by `PuzzleLessonChoice.For` for `Relation == FanOut` before
  the Separate branches, so the Separate recipes are never selected for a fan-out. It places the bend,
  requires every start to fail (`StartsFailForEveryShip`) and ship 0 to curve, rejects an answer zone
  holding any origin/destination (`PuzzleLessonAnswer.Rejected`), and returns the final `TryMiddle`
  answer at the same floor.
- Extend `PuzzleConstructSun.ClearsChord` to the ship list (single = ship 0), so an OffCorridor sun
  clears every chord, not just ship 0's. The validator's ship-0 rule is unchanged.
- Exit: fast unit tests on fixed seeds — one shared `OriginIndex`, distinct destinations in the cone,
  bend clear of the origin, closest point and side. The aim and the bake are `[Explicit]` in
  `PuzzleConstructFanOutBakeTests` (they simulate).

### U2 — same side and the spike

- New `PuzzleRunGeometry.SideOf` (cross-product sign of the trajectory segment nearest the bend) and a
  fan-out check that every path passes the bend on the same side; a seed that does not fails in the
  lesson.
- Spike: one seed of `02_04C` and one of `02_09C` through `PuzzleConstructBake.Bake`. After the last
  destination move, re-check on the current endpoints that the answer zone holds no origin/destination
  and that the pristine start still fails for every ship (`PuzzleVerifier.StartFails`).
- Log the shortcut risk: `ClearShortcut` (`PuzzleConstructBake.cs:128-141`) rejects a Curve fleet whose
  bend can be solved by shrinking one cube; one shared bend makes that likelier than in Separate, and a
  shortcut fail is a seed fail, not a recipe change.
- Exit: a convergence rate on 20 two-ship seeds of `02_04C` (`Attempts = 4`; the seed loop of
  `PuzzleConstructFleetTests.Rate`). Below 12/20, stop: fan-out stays on the search and
  [jobs](../in-progress/bake-construct-jobs.md) bakes only 02_06.

### U3 — two and three ships

- Twenty seeds of `02_04C` and twenty of `02_09C` through `PuzzleConstructBake.Bake`; assert Curve for
  both and that 02_09's answers meet `MinScaleDelta`/`MinStartDistance` (a seed that cannot reach the
  floor fails, rather than shipping a low-delta answer).
- Record the rates and the status histogram here; bar 12/20 each. If the 02_09 bound cannot be met over
  the fixture's seeds, the contract's bounds are a content call (relax, or keep 02_09 on the search
  bake); record it.

## State

- **U1–U3 — code in.** `PuzzleConstruct.Fleet` branches to `PuzzleConstructFanOut.Build` →
  `PuzzleConstructFanOutChord` (one shared origin, N destinations in the 90° cone, a clear bend near
  the origin) → `PuzzleConstructFanOutAim` (provisional zone, `PuzzleRunGeometry.ClosestPoint`, the
  nearest free spot of a fresh `PuzzleConstructShape.Generate`, the plateau stop). `PuzzleFanOutCurveLesson`
  is selected for `Relation == FanOut` + Curve and enforces starts fail for every ship, one side, no
  endpoint in the cube, and the `ScaleFloor` (`1 + MinScaleDelta`). The shared seams:
  `PuzzleConstructPlacement`, `PuzzleConstructShape`, `PuzzleLessonRun.AnswerResults`,
  `PuzzleLessonAnswer.HoldsAnother`/`ScaleFloor`, `PuzzleRunGeometry.ClosestPoint`/`SideOf`,
  `SegmentGeometry.ClosestPoint`, `PuzzleConstructSun.ClearsChords` (every ship), and
  `PuzzleLessonContext.MinScaleDelta`. Fixtures `02_04C` and `02_09C` are in `PuzzleConstructScratch`
  (`Attempts = 4`, `Relation = FanOut`; 02_09C keeps `MinScaleDelta = 0.35`).
- **Tests.** `PuzzleConstructFanOutTests`: 7 fast tests (fixtures, lesson choice, shared origin,
  distinct destinations, cone, bend clearance, closest point, side). `PuzzleConstructFanOutBakeTests`:
  the aim's shape check plus the two spikes and two 20-seed rates, all `[Explicit]`. The project
  compiles clean; the fast run still needs the Test Runner after the MCP bridge dropped mid-run (the
  known domain-reload drop), and the spikes/rates are the bake-checkout pass.
- **Context.** The Separate fleet Curve came back 0/20 (jobs U2), so the shared-bend recipe is not
  proven; fan-out is a different geometry (one origin, one near-origin bend) and its spike decides. If
  `02_04C` is below 12/20, fan-out stays on the search and 02_06/02_09 keep their current bakes.

## Manual check (headset)

1. A fan-out field shows one ring at the shared origin and one ring per route.
2. The destinations sit inside a 90° fan and are not bunched on one side.
3. The answer cube sits near the shared start, clear of the origin planet.
4. The pristine run misses for every ship; growing the bend lands all of them.
5. Shrinking the bend does not solve the job (no Clear shortcut).
6. The edge sun is clear of every chord and is not the answer.
7. The field reads as a searched Spiral/Cluster field: destinations on the arms, no stray middle planets.
8. Spawn/visibility, reset and replay behave as the shipped fleet jobs.

## Review (2026-11-27)

The plan holds together: the existing helpers it names are in the code (`TryMiddle`, `MinRun`,
`Arrives`, `Rejected`, `StartsFailForEveryShip`, `TryAt`), the two new ones are marked new
(`ClosestPoint`, `SideOf`), and the order (origin and bend first, then aim each destination) is the
right one for a ship that launches at its destination.

- **Stale purpose.** It says it "builds the Curve fan-out for the capstone", but
  [jobs](../in-progress/bake-construct-jobs.md) decided the shipped 02_09 stays the search bake. With no
  chapter 2 swap, fan-out's users are the freelance fleets and later chapters. Say so, and decide
  whether it runs before the 5 Dec release or after: the search now scans fleets on every core, so the
  freelance fan-outs can bake without it.
- **The 90° cone narrows the look.** Searched fan-outs are not limited to a cone. Keep it if the spike
  needs it, but add it to the headset check (the destinations should not all bunch on one side).
- **The Clear-shortcut gate is the expensive step.** `ClearShortcut` runs `PuzzleFleetSolution.TryFind`
  over the full candidate grid for every passing seed; with three ships that may cost more than the
  build. Record its share of the per-seed time in U3.

### To do (from the review)

1. **Done — the purpose is current.** The plan says the shipped 02_09 stays search-baked, the users are
   the freelance fleets and later chapters, and a chapter 2 swap is a separate content call.
2. **Done — the cone is in the headset check.** Item 2 of the manual check covers the 90° fan.
3. Record the Clear-shortcut gate's share of per-seed time with the U3 rates.

## Implementation review (2026-11-27)

U1–U3 code is in and reads as the plan: one shared origin, destinations in the cone, the bend nearest
the origin that `PuzzleLessonPlacement.TryAt` accepts, the aim onto a fresh shape sample, and the
lesson's every-ship / one-side / floor checks. Nothing has run yet: the seven fast tests are
unconfirmed and the spikes and rates are the bake-checkout pass.

### Bugs

- **Moved decoys leave the Spiral/Cluster shape.** After the aim, `PuzzleConstruct.AddLesson` runs
  `PuzzleConstructDecoys.KeepClear`, which still relocates by azimuth only (core's open to-do). Both
  fixtures are Spiral and Cluster, so headset item 7 ("no stray middle planets") fails until the core
  fix lands. Do it in core, reusing `PuzzleConstructShape.Generate`.
- **`MinScaleDelta` reaches only the fan-out.** `PuzzleLessonContext.MinScaleDelta` is now filled for
  every recipe, but only the fan-out calls `PuzzleLessonAnswer.ScaleFloor`; `PuzzleCurveLesson` and
  `PuzzleFleetCurveLesson` still pass a floor of 1, and the Clear recipes a ceiling of 1. A contract
  with a high-scramble floor fails at the validator (`ContractFailed`) instead of in the recipe, which
  costs an attempt. Use `ScaleFloor` in both Curve recipes and a matching ceiling (`1 - MinScaleDelta`)
  in both Clear recipes; the freelance hard slots will need it.

### Minor

- **The aim's shape seed shares the attempt stream.** `PuzzleConstructFanOutAim.ShapeSeed` is
  `Value(seedValue, step + 1)`, and `seedValue` is already `Value(seed, attempt)`, so attempt 0's
  first aim sample is attempt 1's whole field. Harmless for yield, but it breaks "one salted stream per
  attempt"; salt the aim with its own constant.
- `PuzzleConstructFanOutAim.Converged` skips `StartsAsTheBriefAsks` (the lesson checks it later), so a
  field can converge in the aim and still fail the lesson; fine, but count it if the rate is low.
- `using DigitalLove.Game.SpaceShaping;` in `PuzzleConstructFanOutAim` and `PuzzleFanOutCurveLesson`
  is redundant inside the child namespace; drop it if other recipes do not use the pattern.
- `PuzzleConstruct` is at 226 lines; the next branch should go to a collaborator, not into it.

Verified fine: the cone is checked pairwise when picking and when aiming; the aim's `PairFits` on a
copy keeps a moved destination clear of every other planet, so two ships cannot share a spot; the sun
now clears every ship's chord (`ClearsChords`); `HoldsAnother` and `AnswerResults` are shared rather
than copied; `PuzzleLessonChoice` returns no lesson for a fan-out Clear; the NoRun step name reaches
`PuzzleBakeResult.Problems` and has a test.

### To do (from the implementation review)

1. Run the fast suite in the Test Runner and record the seven fan-out tests here.
2. Run the spikes and the 20-seed rates in the bake checkout (with the Clear-shortcut gate's share,
   review to-do 3); below 12/20 on `02_04C`, fan-out stays on the search.
3. Use the `MinScaleDelta` floor/ceiling in the other four recipes.
4. Give the aim's shape sample its own salt.
5. Read the first passing field once core's decoy fix lands (headset item 7).
