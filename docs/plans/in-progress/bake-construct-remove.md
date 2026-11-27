# Bake construct — remove the generator

**Status:** now · puzzle creator · updated 2026-11-27 — one way to bake: delete the answer-first
generator and keep the search, whose speed-ups (every core, stored answers, bakes that write only
their job) already landed. Closes the six construct plans once the user says so.

**Related:** [plans board](../README.md) · [core](bake-construct-core.md) ·
[single](bake-construct-single.md) · [wire](bake-construct-wire.md) ·
[separate](bake-construct-separate.md) · [fan-out](bake-construct-fanout.md) ·
[jobs](bake-construct-jobs.md) · [freelance jobs](../backlog/freelance-jobs.md)

## Why

The generator works only for single-ship jobs (Clear 12/20, Curve 11/20), where the search was never
the bottleneck: chapter 1 is baked and freelance single slots are cheap scans. Fleets need the search
anyway (separate Curve 0/20, even with the bend placed first). Two paths double every bake rule
(`Generator`, scratch folder, pins with attempts) for an unmeasured gain.

## Decided

- **The search is the only bake path.** No `Generator` field, no scratch contracts, no recipes.
- **No gameplay or shipped asset changes.** Shipped jobs, answers and chapter orders stay as they are.
- **Keep the general fixes** the construct work brought, where the search uses them: the
  `PuzzleFieldBuilder` refactor, the shared `PuzzleSunSources.WithSun`, the validator's lines in
  `PuzzleBakeResult.Problems` on `ContractFailed`, and `SegmentGeometry.ClosestPoint` (now under
  `DistanceToSegment`).

## Units

### U1 — delete the generator

- Delete every `PuzzleConstruct*` type (Bake/Data and Bake/Domain), `PuzzleDecoyField`,
  `PuzzleGenerator`, the `Recipes/` folder (`IPuzzleLesson`, `PuzzleLesson*`, the Clear/Curve/Fleet/
  FanOut lessons) and `PuzzleLessonPlan`, with their `.meta` files.
- Delete the construct tests: `PuzzleConstructTests`, `PuzzleConstructVerdictTests`,
  `PuzzleConstructSingleTests`, `PuzzleConstructFleetTests`, `PuzzleConstructDecoyTests`,
  `PuzzleConstructFanOutTests`, `PuzzleConstructFanOutBakeTests`, and the construct cases in
  `PuzzleSeedScannerTests`.
- Take the branches out of their callers: `PuzzleJobContract.Generator`/`Attempts` (if only the
  generator reads it), `PuzzleJobContracts`' scratch lookups, `PuzzleSeedScanner.Construct` and its
  `lesson` parameter, the Construct cases in `PuzzleJobSeedScan` and `PuzzleTestSetBuilder` (the
  scratch folder and `IsScratch`), `PuzzleBakeStatus.ClearShortcut` and its window message.
- The uncommitted fleet-trial code goes too: `PuzzleBakeResult.FailedShip`,
  `PuzzleVerification.AllStarts`/`AllSolved`, `PuzzleVerifier.FirstFailing` and the `PuzzleFieldBake`
  pass-through.
- Exit: the project compiles; no type or member mentions Construct, Lesson recipes, Generator or
  Scratch.

### U2 — undo what only the generator needed

- With the callers gone, remove members that lost their last caller (`PuzzleRunGeometry.ClosestPoint`
  /`SideOf`, `PuzzleLessonRun`-era helpers, `PuzzleSeedScanResult` fields only construct filled) and
  put back the visibility widened only for it (`PuzzleFieldBake.Check`, `PuzzleRunChoice
  .PlanetBlockBandFactor`, `PuzzleSunSources` if nothing else calls it publicly). Keep each "keep" in
  Decided.
- Add a test or a note for the found drift: `PuzzleRouteCount` counts 1 route for the shipped `01_03`,
  which stores 2 (the shipped check trusts the stored count).
- Exit: the fast suite and the Creator suite green; `ShippedJobs_MeetTheContract` green in the bake
  checkout after merging.

### U3 — the docs

- [Freelance jobs](../backlog/freelance-jobs.md): replace "Search or construct" with "every slot uses
  the search"; drop the scratch-folder note and the `Attempts` pin in U3.
- AGENTS and `docs/tools/bake.ps1` help: drop any `Generator`/scratch mention.
- A [lessons](../../lessons.md) entry: answer-first works for one ship and fails on the physics for
  fleets; a new fleet idea starts by measuring passing search seeds (02_06 had 33/200), not by guessing
  a recipe; measure the speed-up before building the second path.
- Exit: `docs/check.ps1` passes; the board lists this plan and the six construct plans waiting for the
  user's close.

## Closing

Core, single, wire, separate, fan-out and jobs close with this plan, on the user's word: delete their
files and board rows; the lessons entry above is their post-mortem.
