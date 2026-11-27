# Bake construct — the first real jobs

**Status:** now · puzzle creator · updated 2026-11-27 — U1 in: a Construct fleet scans and bakes
sequentially (no worker seam), and the unsupported path is gone. The `bake.ps1` exit waits on the code
reaching the `bake` checkout; U2 waits on the replace-or-prove call.

**Bakes:** off the editor — U1 and U2.

**Related:** [plans board](../README.md) ·
[construct separate](../in-progress/bake-construct-separate.md) (its fleet recipes are the base) ·
[construct fan-out](../backlog/bake-construct-fanout.md) (must land first for 02_09) ·
[chapter 2 rebakes](../in-progress/chapter-2-rebakes.md) (the search bakes this replaces)

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

### U2 — 02_06 and 02_09

- The search bakes stand until here: 02_06 seed 86 (window 0.397) and 02_09 seed 183 (window 0.246),
  both `Curve`. Use `02_06C` (already a scratch fixture) and the fan-out plan's 02_09 fixture; reuse the
  20-seed rates those plans record, scan more only if no seed passes, then move the pick into the real
  contract.
- The fixture's `Attempts = 4` is the pin's other half: a construct scan retries the same seed through
  `PuzzleConstructSeeds.Value(seed, attempt)`, so the field reproduces only with the same `Attempts`.
  Set the shipped contract to that pair (`Seed` and `Attempts`), not just the seed.
- Swap a field, its answer and its contract comment only when the constructed job passes the same
  contract *and* is no worse than that job's search bake: solve window at least the search pin's
  (02_06 0.397, 02_09 0.246), the scan's per-seed wall clock recorded, and the field reads on the
  headset. A construct fleet scan is sequential, so compare per-seed time, not the whole scan against
  the parallel search.
- 02_06 is already `Pending = false` (chapter 2 rebakes U3), so nothing to clear. 02_09 already meets
  `MinScaleDelta` 0.35 / `MinStartDistance` 0.15, so the construct answer must too. Bake, read the
  asset (the sun level with the planets, outside the rim), and commit the job, its answer and its pin.
- Both fields are new content: the chapter 2 headset check runs again on them.
- Exit: both pass `ShippedJobs_MeetTheContract`, or the contract keeps `Generator = Search` and the
  reason is recorded here.

## State

- **U1 — code in.** `PuzzleSeedScanner.Construct` accepts a fleet and `ScanFleetAsync` runs the
  synchronous `Scan` for a Construct job; `ConstructFleet_UsesTheFleetLesson` replaces the unsupported
  test. `PuzzleSeedScannerTests` 4/4 and the fast Game suite 101/101 are green. The `bake.ps1` exit
  waits on the code reaching the `bake` checkout (the user commits and merges).
- **U2 — blocked on the call below:** rebake 02_06 and 02_09, or bake the scratch copies only.

## Review (2026-11-27)

- **Applied — the seam is two guards, not a new batch path.** `FindJob`/`ScanResults` already dispatch
  (`PuzzleTestSetBuilder.cs:153`, `PuzzleJobSeedScan.cs:57`). U1 no longer proposes
  `IPuzzleSeedSearch`/`PuzzleSeedBatch`/`FleetSearchRun` for Construct, which cannot run off the main
  thread; a Construct fleet scans sequentially.
- **Applied — the pin is `Seed` + `Attempts`.** `PuzzleConstructBake.Bake` re-seeds per attempt
  (`PuzzleConstructSeeds.Value`), and the scan passes `job.Attempts`; pinning `Attempts = 1` would
  rebuild attempt 0, not the field that passed.
- **Applied — stale `Pending`, missing baselines.** 02_06's `Pending` was cleared in chapter 2 rebakes
  U3; both search pins and windows are now named so "no worse" has a number.
- **Applied — U1 is a bake too.** The `**Bakes:**` line covers U1's `bake.ps1` exit.
- **Open — replace, or only prove?** Both jobs already pass and shipped on the search. If the goal is
  to prove the constructor, bake the scratch copies and leave the shipped fields; swap only when the
  construct field is materially better or the search is retired. The user's call before U2.
