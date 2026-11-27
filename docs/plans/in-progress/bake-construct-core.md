# Bake construct — the core

**Status:** now · puzzle creator · updated 2026-11-27 — the answer-first generator's skeleton and its
pass/fail gate: a field built around an intended answer, judged by the bake's existing checks. U1–U2
in; the recipes are the single plan. First of six: core → [single](../backlog/bake-construct-single.md) → [wire](../backlog/bake-construct-wire.md) →
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
  returns a plain `ConstructedField` (sources, `ships` — one origin/destination pair per ship — and the
  cube range) plus the intended answer (`PuzzleZone[]`, kind). The frames are built in U2. The steps
  stay plain; the verdict (U2) runs on the main thread, since `PuzzleData`/`PuzzleSeed` are
  ScriptableObjects and `PlanetNames.Instance` is main-thread only.
- Steps, each a small type:
  1. origin and destination (spread and length from the field shape), with their endpoint planets
     (they count toward `PlanetCount`); a fleet plan adds the other ships' pairs;
  2. the lesson's planets (from [single](../backlog/bake-construct-single.md) or the fleet plans);
  3. decoys: the rest of `PlanetCount` from the public `PuzzleFieldGenerator.Generate` — generate the
     full layout and keep the positions clear of the placed endpoints/lesson, or add a count overload;
     reject any spot within a corridor margin of every ship's start path and answered path. Not
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
  private — make it public or add a materialize seam; `BuildFrames` is already public): frames,
  `routes`, `entropyBudget`, cube range.
- Run the bake's whole gate; no new physics decides pass/fail: `PuzzleStartCheck.Status` (the start
  must fail; `NoFail` means crash; a Curve/Mixed start must deviate ≥ 5 % of the run or 4°),
  `PuzzleVerifier.Verify(...).Solved` and `StartFails`, `PuzzleSunRuleCheck.Matches` and, for
  `NoSunAnswer`, `SunAnswers`, `PuzzleRouteCount.Count` against `brief.MinRoutes`,
  `PuzzleJobValidator.Validate` with the answer (it does not verify a single-ship run, hence the
  others), and `PuzzleSolveWindow.Measure`. The fleet plans add one gate built from these calls: the
  shrink-shortcut sweep ([single](../backlog/bake-construct-single.md) U2), used by
  [separate](../backlog/bake-construct-separate.md) U2.
- A failed verify names its step; repair the knob that caused it. `StartNotFailed`/`StartNotBlocked` →
  the blocker; `StartTooStraight` → the bend's offset; `SunRuleFailed`/`SunAnswers` → the sun's home;
  `MinRoutes`/`SingleRoute` → the lesson planet or the cube; otherwise move the nearest decoy. A local
  repair stays within one attempt; a new attempt re-seeds the stream from `(seed, attempt)`.
- `PuzzleStartCheck` measures ship 0 (`field.origin`/`destination`); a fleet holds the other paths by
  construction and `PuzzleVerifier.StartFails`/`Solved` covers every ship.
- Exit: a hand-placed field (fixed seed, no lesson recipe yet) passes the gate in a Creator test.

## State

- **U1** — done. `PuzzleConstruct` with the plain snapshots (`PuzzleSeedFields`,
  `PuzzleFieldSettings`, `PuzzleConstructedField`, `PuzzleDecoyField`), the chord/lesson/decoy/sun
  steps, and `PuzzleRunChoice.PlanetBlockBandFactor` public. `PuzzleConstructTests`: 7 fast tests
  (counts/endpoints, corridor, Centre sun, OffCorridor sun, attempt variation, lesson move, decoy
  move), green.
- **U2** — done. `PuzzleConstructBake` materializes through the public `PuzzleFieldBake.Check`
  (`Variant` stays private; `Check` runs it), then the contract validator and the solve window, and
  reports the failing `PuzzleBakeStatus`. The retry moves the nearest unprotected decoy; the
  lesson/cube repairs named in U2 belong to the recipes. `PuzzleConstructVerdictTests`: the shipped
  `01_01` and its stored answer pass; an empty answer is `NoSolution`; green (~1.7 s).
- **Found:** `PuzzleRouteCount` now counts 1 route for the shipped `01_03` (the asset stores 2), so
  the construct gate returns `SingleRoute` for it. The shipped check trusts the stored count. The
  single plan's Clear target must construct a field that admits 2 current routes; whether the shipped
  `01_03` is rebaked is a content call.
- Next: [single](../backlog/bake-construct-single.md) — Clear `01_03`, Curve `01_01`.
