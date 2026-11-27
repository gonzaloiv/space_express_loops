# Bake construct — the core

**Status:** now · puzzle creator · updated 2026-11-27 — the answer-first generator's skeleton and its
pass/fail gate: a field built around an intended answer, judged by the bake's existing checks. In
progress. First of six: core → [single](../backlog/bake-construct-single.md) → [wire](../backlog/bake-construct-wire.md) →
[separate](../backlog/bake-construct-separate.md) → [fan-out](../backlog/bake-construct-fanout.md) →
[jobs](../backlog/bake-construct-jobs.md).

**Related:** [plans board](../README.md) · `PuzzleAnswerAsset` (stores what
this builds)

## Goal

Make jobs quicker to bake without changing what they are. A constructed job uses only what today's
jobs use (scalable planets, the sun, the same contracts and checks) and must look and play like a
searched one. **No anchored (`fixedMass`) planets**: those are chapter 3's
[broken bodies](../backlog/chapter-3-broken-bodies.md) and stay there.

## How the physics shapes the build

- A ship launches straight at its destination (`ShipLaunch.BuildLaunch`), so the unedited path is the
  chord bent by the field; a job fails at start because something on or near the chord bends or blocks
  it.
- `PuzzleSolutionClassifier`: **Clear** = a planet on or near the path shrinks; **Curve** = a planet
  grows.

## Units

### U1 — the skeleton

- New `PuzzleConstruct` (Creator domain): takes the contract, the brief and the seed's plain fields and
  returns a plain `ConstructedField` (sources, origin/destination, cube range, frames) plus the intended
  answer (`PuzzleZone[]`, kind). The steps stay plain; the verdict (U2) runs on the main thread, since
  `PuzzleData`/`PuzzleSeed` are ScriptableObjects and `PlanetNames.Instance` is main-thread only.
- Steps, each a small type:
  1. origin and destination (spread and length from the field shape), with their endpoint planets
     (they count toward `PlanetCount`);
  2. the lesson's planets (from [single](../backlog/bake-construct-single.md) or the fleet plans);
  3. decoys: the rest of `PlanetCount` from the public `PuzzleFieldGenerator.Generate`, rejecting any
     spot within a corridor margin of the start path or the answered path. Not
     `PuzzleArtifactFactory.PlanetsFor`: its `PuzzleBlockerPlacement.Apply` moves a planet onto the run
     and would overwrite the lesson;
  4. the sun: `Centre` is a plain `PuzzleSource` at the field origin (the bake's `PuzzleSunSources` is
     `internal`; do not call it); `OffCorridor` uses the public `PuzzleSunPlacementChoice.TryHome`,
     which needs the planets, so it runs after them.
- **Retries vary.** Each attempt seeds its random stream from `(seed, attempt)`, so a retry builds a
  different field.
- Make `PuzzleRunChoice.PlanetBlockBandFactor` public (it is `internal`, and the constructor reads the
  block band, `radius × 2`); no internal APIs across types (AGENTS).
- Exit: fast unit tests for each step on fixed seeds (no simulation).

### U2 — the verdict

- Materialize the variant on the main thread through `PuzzleFieldBake`'s variant step (`Variant` is
  private; `BuildFrames` is already public): frames, `routes`, `entropyBudget`, cube range.
- Run the bake's whole gate; nothing new decides pass/fail: `PuzzleStartCheck.Status` (the start must
  fail; `NoFail` means crash; a Curve/Mixed start must deviate ≥ 5 % of the run or 4°),
  `PuzzleVerifier.Verify(...).Solved` and `StartFails`, `PuzzleSunRuleCheck.Matches` and, for
  `NoSunAnswer`, `SunAnswers`, `PuzzleRouteCount.Count` against `brief.MinRoutes`,
  `PuzzleJobValidator.Validate` with the answer (it does not verify a single-ship run, hence the
  others), and `PuzzleSolveWindow.Measure`.
- On a failed verify, move only the decoy nearest the failing path and retry a few times. A
  `MinRoutes` failure is not a decoy problem: move the lesson planet or the cube instead.
- Exit: a hand-placed field (fixed seed, no lesson recipe yet) passes the gate in a Creator test.
