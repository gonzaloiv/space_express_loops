# Bake construct — the first real jobs

**Status:** now · puzzle creator · updated 2026-11-27 — U1 in: a Construct fleet scans and bakes
sequentially (no worker seam), and the unsupported path is gone. U2 is the scratch proof on `02_06C`
and `02_09C`; it waits on fan-out for 02_09 and on U1 reaching the `bake` checkout.

**Bakes:** off the editor — U1 and U2.

**Related:** [plans board](../README.md) ·
[construct separate](../in-progress/bake-construct-separate.md) (its fleet recipes are the base) ·
[construct fan-out](../backlog/bake-construct-fanout.md) (must land first for 02_09) ·
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

- **U1 — code in.** `PuzzleSeedScanner.Construct` accepts a fleet and `ScanFleetAsync` runs the
  synchronous `Scan` for a Construct job; `ConstructFleet_UsesTheFleetLesson` replaces the unsupported
  test. `PuzzleSeedScannerTests` 4/4 and the fast Game suite 101/101 are green. The `bake.ps1` exit
  waits on the code reaching the `bake` checkout (the user commits and merges).
- **U2 — decided: scratch proof.** 02_06 and 02_09 keep the search bakes; `02_06C` runs once U1
  reaches the `bake` checkout, and `02_09C` also waits on fan-out.

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
