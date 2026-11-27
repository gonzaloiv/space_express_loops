# Bake construct — wire it in and compare

**Status:** next · puzzle creator · updated 2026-11-27 — a contract can ask for the constructor, the
bake tools run it, and search and construct are timed side by side. Not started.

**Bakes:** off the editor — the timing comparison.

**Related:** [plans board](../README.md) · [construct single](bake-construct-single.md) (must land
first) · [construct separate](bake-construct-separate.md) (next)

## Units

### U1 — the `Generator` seam

- `PuzzleJobContract.Generator` (`Search` default, `Construct`).
- `PuzzleSeedScanner.Find` sees only the seed, while the generator, `MinBlockers` and the scramble
  bounds live on the contract: add a `Find(job, ...)` overload (or copy those fields onto
  `PuzzleSeed`/`PuzzleBrief`) and thread it through `PuzzleJobSeedScan.Probe`.
  `PuzzleTestSetBuilder.FindJob` already has the contract.
- The constructed field goes through the same variant step, export and answer asset as a searched one
  (`PuzzleAnswerAsset`).
- `Attempts` means construct retries at the same seed (the core varies the stream per attempt); a run
  that never passes reports the last attempt's `PuzzleBakeStatus` (`NoSolution` if none built a field).
- Exit: a `Construct` contract bakes through `docs/tools/bake.ps1` with no other change.

### U2 — search vs construct

- Time and yield for `01_03` and `01_01`, search vs construct, in the bake checkout; write both here.
- Exit: the numbers decide whether chapter 3 and the freelance jobs default to `Construct`. The
  shipped `01_03` and `01_01` stay as they are: this is a comparison, not a rebake.
