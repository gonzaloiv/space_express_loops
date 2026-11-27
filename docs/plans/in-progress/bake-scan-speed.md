# Bake — scan speed

**Status:** now · space shaping · updated 2026-11-27 — measure where a seed scan's time goes, then
decide whether any cut is needed before chapter 3 and the freelance bakes. No shipped job, answer or
contract changes.

**Bakes:** off the editor — U1 runs in the bake checkout
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [lessons](../../lessons.md) ·
[one bake path](../../solutions/bake/one-bake-path.md) (the search is the only bake path) ·
[freelance jobs](../backlog/freelance-jobs.md) (the scans this serves) ·
[chapter 2 rebakes](chapter-2-rebakes.md) (the 200-seed runs)

## Goal

Know what a scan costs on every core before cutting anything. A fleet seed is about 2 minutes on one
core and a failing single-ship seed about 1 second; the scan already spreads fleet seeds over
`-Workers` (default cores − 1), so a 200-seed two-ship scan should be minutes, not hours.

## Where the time goes (read, not measured)

The fleet probe is the cost: `PuzzleMultiShipProbe` tries up to 8 centres × 3 sizes × 13 scales, and
for each zone `PuzzleShipScan.RescuedFixed` simulates every ordered planet pair that does not already
arrive (14 planets: up to 182). That is tens of thousands of trajectories a seed. The bookkeeping
around it (`Distinct`, the rescue lists), the shared solution context and the window's 200 samples are
small beside it, so cuts there are not worth a unit.

## Units

### U1 — measure

- Timings in: `PuzzleScanTimings` (setup, start cache, probe + simulations, select, confirm, window)
  is passed optionally through `PuzzleSeedScanner`, `PuzzleMultiShipSearch`/`Probe`/`ShipScan`,
  `PuzzleFieldBake` and `PuzzleSolveWindow`; the pinned bake and Creator tests pass null. The batch
  report (`Logs/bake-<job>.txt`) prints per seed and a totals line (`wallMs`, per phase, sims, passing,
  status histogram), and every line also goes to the Unity log.
- Baseline in the bake checkout after this code is merged into `bake`: `01_04` (single, 64 seeds),
  `02_03` (two ships, 20) and `02_06` (three ships, 20); fleets with `-Workers 1` and the default
  (`docs/tools/bake.ps1 -Job <id> -Seeds <n> -Workers <n>`). Record wall clock, passing seeds and the
  status histogram from the report's totals line.
- This answers the board's *Parallel scan timing* tail: put the measured speed-up in AGENTS' bake
  section.
- Exit: the baseline table here; AGENTS updated.

### U2 — decide

- If a 200-seed scan of the slowest freelance shape fits in an hour on every core, close the plan: the
  scans run in the background and do not block a bake.
- If three-ship scans are the slow ones, the targeted cut is a smaller candidate grid for that job
  (fewer centres or scales). `MaxCentres` and `ScaleSamples` are constants today, so that is a small
  per-contract setting plus a fresh scan of that job; shipped bakes keep the current grid. Write it as
  its own short plan with the numbers.
- Exit: the decision written here, and the plan closed or the follow-on plan written.

## Not in this plan

- Probe bookkeeping cuts (counting once, lists only for the winner): no simulation saved. Capping the
  count at `best + 1` would also change which zone wins on a tie.
- A pair-skip bound: scaling a planet changes gravity everywhere, so no bound is provably neutral.
  An idea, not a unit.
- A shared solution context and a two-stage scan window: singles fail in ~1 s and the window runs only
  on passing seeds.
- Singles on every core: decide after U1 shows a single seed's share.
