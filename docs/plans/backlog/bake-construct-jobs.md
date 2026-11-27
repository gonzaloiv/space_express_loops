# Bake construct — the first real jobs

**Status:** next · puzzle creator · updated 2026-11-27 — the fleet bake paths can run the
constructor, and 02_06 and 02_09 are rebaked with it if it beats their search bakes. Not started.

**Bakes:** off the editor — U2.

**Related:** [plans board](../README.md) · [construct fan-out](bake-construct-fanout.md) (must land
first) · [chapter 2 rebakes](../in-progress/chapter-2-rebakes.md)

## Units

### U1 — the fleet seam

- Carry the wire plan's `Generator` seam into the fleet paths: `PuzzleTestSetBuilder.FindJob` /
  `FindFleetJob` (a Construct branch; `Attempts` is same-seed retries) and
  `PuzzleJobSeedScan.ScanResults`, so the scan, `FleetSearchRun` and `PuzzleSeedBatch` can run a
  Construct job. Implement it through `IPuzzleSeedSearch` if it fits, or add a Construct branch to
  `PuzzleSeedBatch`/`FleetSearchRun`.
- Exit: a fleet `Construct` contract scans and bakes through `docs/tools/bake.ps1`.

### U2 — 02_06 and 02_09

- The search bakes from [chapter 2 rebakes](../in-progress/chapter-2-rebakes.md) stand until this unit
  (02_09: seed 183). Rebake 02_06 (separate, Curve) and, if the fan-out spike converged, 02_09
  (fan-out, Curve) with `Generator = Construct`. Replace a field, its answer and its contract comment
  only when the constructed job passes the same contract and is no worse than that job's search bake on
  the wire U2 measures (time, yield, window).
- Pin the seed, clear 02_06's `Pending` (the shipped check walks `Enabled` only), bake, read the
  asset (the sun level with the planets, outside the rim), and commit the job, its answer and its pin.
  Both fields are new content: the chapter 2 headset check runs again on them.
- Exit: both pass `ShippedJobs_MeetTheContract`, or a job is recorded as search-baked with the reason.
