# Bake construct — the first real jobs

**Status:** now · puzzle creator · updated 2026-11-27 — U1 in and merged into `bake`; a Construct
fleet scans through `bake.ps1`. U2's proof is negative: fleet Curve builds 0/20 (`02_03C`, `02_06C`),
so 02_06 stays search-baked and 02_09C waits on fan-out. The remaining tail is the fleet recipes'
yield (separate U2/U3), not this seam.

**Bakes:** off the editor — U1 and U2.

**Related:** [plans board](../README.md) ·
[construct separate](../in-progress/bake-construct-separate.md) (its fleet recipes are the base) ·
[construct fan-out](bake-construct-fanout.md) (must land first for 02_09) ·
[chapter 2 rebakes](../in-progress/chapter-2-rebakes.md) (the search bakes this compares against)

## Units

### U1 — the fleet seam

- The dispatch is already in: `PuzzleTestSetBuilder.FindJob` and `PuzzleJobSeedScan.ScanResults` read
  `Generator`, and `Probe` passes `job.Attempts`. Two guards rejected a Construct fleet:
  `PuzzleSeedScanner.Construct` returned `Unsupported` for `brief.IsMulti`, and
  `PuzzleJobSeedScan.ScanFleetAsync` returned `UnsupportedFleet`. The first is removed; the second now
  delegates a Construct job to the synchronous `Scan`.
- Construct bakes on the main thread (`PuzzleConstructBake.Materialize` creates a `PuzzleData` and
  reads `PlanetNames.Instance`), so it cannot implement `IPuzzleSeedSearch.Run` or ride
  `PuzzleSeedBatch`/`FleetSearchRun`. A Construct fleet scans sequentially like a single construct;
  `-Workers` does not apply.
- Replace `PuzzleSeedScannerTests.ConstructFleet_IsUnsupported` with
  `ConstructFleet_UsesTheFleetLesson`: a fleet contract reaches the constructor, the lesson sees every
  ship, and the status is a real bake status rather than `UnsupportedMechanism`.
- Exit: a fleet `Construct` contract scans and bakes through `docs/tools/bake.ps1`.

### U2 — prove on the scratch copies

- **Decision (2026-11-27): do not swap the shipped fields.** 02_06 (seed 86, window 0.397) and 02_09
  (seed 183, window 0.246) keep `Generator = Search`; the constructor is proved on the scratch copies,
  which write under `PuzzleCreator/Scratch` and never ship.
- Scan `02_06C` (Separate, Curve, `Attempts = 4`) and `02_09C` (02_09's contract as a `Pending`
  construct fixture, added when fan-out lands), reusing the 20-seed rates those plans record; scan more
  only if no seed passes. Record per-seed wall clock, the passing count and the best solve window, and
  compare each to its search pin (02_06 0.397, 02_09 0.246).
- A construct fleet scans sequentially (`-Workers` does not apply), so the comparison is per-seed time
  and yield, not whole-scan time against the parallel search. The pin is (`Seed`, `Attempts`): a
  construct scan retries the same seed through `PuzzleConstructSeeds.Value(seed, attempt)`, so a seed
  alone does not reproduce the field.
- Read one accepted asset per job (the sun level with the planets, outside the rim) to confirm the
  constructed fleet looks like a searched one.
- Exit: the numbers and the read are recorded here; no contract, chapter or shipped asset changes.

## State

- **U1 — done.** `PuzzleSeedScanner.Construct` accepts a fleet and `ScanFleetAsync` runs the
  synchronous `Scan` for a Construct job; `ConstructFleet_UsesTheFleetLesson` replaces the unsupported
  test. `PuzzleSeedScannerTests` 4/4 and the fast Game suite 101/101 are green. Merged into `bake` at
  `952fe82b`; `bake.ps1 -Job 02_06C -Mode scan -Seeds 20` exits 0 and reports a status per seed.
  `-Mode bake` is still untested for a fleet: no fixture's pinned seed builds yet.
- **U2 — negative proof (2026-11-27).** In the `bake` checkout at `952fe82b`:
  - `02_06C` (Separate Curve, 3 ships): 0/20, all `NoRun`; 37 s for 20 seeds against a 22 s one-seed
    baseline, so about 0.8 s/seed for a failing build.
  - `02_03C` (Separate Curve, 2 ships): 0/20, all `NoRun`.
  - `02_07C` (Separate Clear control): 1/20 (seed 26, window 0.106, scale delta 0.394).
  No accepted 02_06C asset, so there is no constructed field to read. The fleet recipes are far below
  their 12/20 bars and Curve builds nothing; the blocker is
  [separate](bake-construct-separate.md) U2/U3, not this seam. 02_06 and 02_09 keep the search bakes;
  no contract, chapter or shipped asset changed. `02_09C` also waits on fan-out.
- **Diagnosed (2026-11-27).** Both fail at `Lesson`, and the instrumented lesson names
  `bend offsets`: the per-ship blockers place, but all six shared-bend offsets fail
  `PuzzleLessonPlacement.TryAt` (endpoint clearance or separation from the blockers/planets). `02_03C`
  reached it on 4/4 attempts; `02_06C` on 2/4, the other attempts failing earlier at the chords. The
  fix belongs to [separate](bake-construct-separate.md) U3 — place or reserve the bend before the
  blockers, or widen its search — not to the fleet seam.

## Review (2026-11-27)

- **Applied — the seam is two guards, not a new batch path.** `FindJob`/`ScanResults` already dispatch
  (`PuzzleTestSetBuilder.cs:153`, `PuzzleJobSeedScan.cs:57`). U1 no longer proposes
  `IPuzzleSeedSearch`/`PuzzleSeedBatch`/`FleetSearchRun` for Construct, which cannot run off the main
  thread; a Construct fleet scans sequentially.
- **Applied — the pin is `Seed` + `Attempts`.** `PuzzleConstructBake.Bake` re-seeds per attempt
  (`PuzzleConstructSeeds.Value`), and the scan passes `job.Attempts`; a scratch proof pinned by seed
  alone would rebuild attempt 0, not the field that passed.
- **Applied — stale `Pending`, missing baselines.** 02_06's `Pending` was cleared in chapter 2 rebakes
  U3; both search pins and windows are now named so "no worse" has a number.
- **Applied — U1 is a bake too.** The `**Bakes:**` line covers U1's `bake.ps1` exit.
- **Resolved (2026-11-27) — prove, do not replace.** The user chose the scratch copies; the shipped
  02_06 and 02_09 keep `Generator = Search`, and U2 records the comparison instead of swapping fields.

## Re-review (2026-11-27)

The plan makes sense after the decision: U1 is two guards, not a new batch path, and U2 is a measured
comparison on scratch copies with real baselines (02_06 window 0.397, 02_09 0.246).

- **It depends on the wire fix.** The scratch copies are `Pending`, and today
  `PuzzleTestSetBuilder.FolderFor` sends every `Pending` contract to Scratch. Until
  [wire](bake-construct-wire.md)'s to-do 1 keys the folder on `PuzzleConstructScratch.Has`, a parked or
  not-yet-flipped shipped job would also bake into Scratch. Do not run U2 before that lands.
- **Per-seed time must include the gate.** A Construct fleet runs `ClearShortcut` on every passing seed
  (a full Clear search); count it in the per-seed wall clock, or the comparison flatters the
  constructor.
- 02_09C only exists once [fan-out](bake-construct-fanout.md) lands; if fan-out moves after
  the release, U2 proves 02_06C only, and the plan should say so.

### To do (from the re-review)

1. **Done (checked 2026-11-27) — wire's scratch-folder fix is in.** `PuzzleTestSetBuilder.FolderFor`
   keys on `PuzzleConstructScratch.Has`, so a parked `Pending` job keeps its chapter path. The U2 run
   at `952fe82b` predates the fix but wrote no asset (0/20 Curve), so nothing landed in the wrong
   folder; rerun on a `bake` that has it before baking any fixture's pin.
2. Include the Clear-shortcut gate in the per-seed time.
3. Record whether U2 covers 02_09C, once fan-out's timing is decided.
