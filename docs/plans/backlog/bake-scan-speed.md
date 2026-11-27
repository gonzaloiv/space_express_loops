# Bake — scan speed

**Status:** next · space shaping · updated 2026-11-27 — cut the search's wall clock before chapter 3
and the freelance bakes: measure first, then singles on every core, a pruned fleet probe, and one role
pass. No shipped job, answer or contract changes.

**Bakes:** off the editor — U1 and every exit run in the bake checkout
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [lessons](../../lessons.md) ·
[the one bake path solution](../../solutions/bake/one-bake-path.md) (why the generator was removed) ·
[freelance jobs](freelance-jobs.md) (the scans this serves) ·
[chapter 2 rebakes](../in-progress/chapter-2-rebakes.md) (the 200-seed runs that took hours)

## Goal

A seed scan finishes sooner for the same pass set. The search is the only bake path now
([the one bake path solution](../../solutions/bake/one-bake-path.md)), so its cost is paid on every job: about
**2 minutes a two-ship seed on one core** ([freelance jobs](freelance-jobs.md)) and about **1 second a
failing single-ship seed**. A 200-seed fleet hunt is hours; freelance needs ~2 attempts a slot across
20 slots.

## Decisions

- **Measure before cutting.** No cut lands without a before/after wall clock on a fixed seed range.
- **Same answers, same passes.** A change may not alter which seeds pass, which zone ships, or the
  status a failing seed reports. `ShippedJobs_MeetTheContract` and the Creator suite are the gate.
- **Shipped assets are untouchable.** Contracts, chapter order, answers and saved jobs do not change.
  A rebake is out of scope; if a cut would change a shipped bake, it is rejected, not rebaked.
- **Ordering stays.** Largest size first, cheapest entropy, widest window. Speed cuts prune work the
  search would have discarded, not reorder its choice.
- **The fleet probe dominates; singles are single-core.** The fleet tries 8 centres × 3 sizes × 13
  scales and simulates every ordered planet pair per zone; singles run sequentially because the bake
  holds a `PuzzleData` (`PuzzleJobSeedScan.cs:60-63`).

## Product contract

- `bake.ps1 -Workers` shortens a single-ship scan too, with the same entries in the same order.
- A fleet seed's probe examines fewer trajectories but returns the same `FleetZoneResult` for that
  seed (same zone, same rescued list) on the reference ranges.
- Cancellation and release behave as the fleet run does today: a recompile leaves no locked gate and
  no live worker.
- The scan report gains per-phase timings; the default scan output is otherwise unchanged.

## Units

### U1 — measure

- Add timings around the phases a scan already runs: planets/apply, `Select` (and its second pass),
  `Check`, `Confirm` (verify, sun, routes) and `PuzzleSolveWindow.Measure`. Report per seed and totals
  in the scan report and the `bake.ps1` log.
- Record a baseline in the bake checkout on fixed ranges: `01_04` (single, 64 seeds), `02_03`
  (two ships, 20) and `02_06` (three ships, 20). Record wall clock, passing seeds and status
  histogram for each.
- Exit: the plan carries the baseline table; every later unit compares against it.

### U2 — singles on every core

- Mirror the fleet seam for one ship: build a plain snapshot (the read-only field values
  `PuzzleSolutionSearch` needs — sources, endpoints, cube range, budget, settings), prepare it on the
  main thread, run the search on a worker, confirm on the main thread (`PuzzleData` creation, variant,
  verify, routes).
- Route `PuzzleJobSeedScan.ScanResults` through `PuzzleSeedBatch` for singles, so `-Workers` applies;
  keep the seed order of results.
- Exit: the `01_04` range returns the same per-seed statuses as the sequential run, and its wall clock
  drops near `1 / workers`. `-Workers 1` matches the old time.

### U3 — prune the fleet probe

Three cuts to the dominant loop, each with the same result on the reference ranges:

1. **Pair prefilter.** Skip an ordered pair before simulating when its chord cannot be affected enough
   to flip a miss into an arrival — a cheap geometric bound from the zone to the chord. Measure the
   bound on the 20-seed ranges before committing it.
2. **Cap the distinct count.** `Distinct` only needs `>= Ships`; stop enumerating a row once it is met
   (the chosen zone still needs its full rescue list for `Confirm`).
3. **Coarse-to-fine scales.** Sweep every other scale first; refine neighbours only where a plateau
   appears. The plateau test already needs adjacent samples only (`PuzzleMultiShipProbe.Plateau`).

- Exit: the `02_03` and `02_06` ranges return the same passing seeds, the same zone and rescued list
  per seed, and a lower wall clock. If a cut changes one seed's answer, keep the cut that is provably
  neutral and record the other as rejected.

### U4 — one role pass and a cheaper scan window

- `PuzzleSolutionSelection.Select` runs the whole search again when the cheapest route has the wrong
  role (`PuzzleSolutionSelection.cs:15-27`). Track the cheapest solution per mechanism during the
  first pass and pick from it; keep the same status (`Baked` / `WrongRoleRoute` / mismatch).
- In a scan, measure the window on fewer samples (e.g. 50) and re-measure at the full 200 only for the
  entry the scan recommends. The pinned bake still measures 200.
- Exit: for every shipped contract the scan reports the same chosen seed, window to the third decimal
  and status; the reference ranges lose the second search and most of the window cost.
  `ShippedJobs_MeetTheContract` green in the bake checkout.

## Verification

- Fast suite: `DigitalLove.Game.SpaceShaping.Tests.EditMode`.
- Creator suite: `DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode` (deliberate, bake-heavy).
- Bake checkout: `bake.ps1` on the three reference jobs, then `ShippedJobs_MeetTheContract`.
- `docs/check.ps1` after the board and this file change.

## Open

- **Fleet pass rate vs probe count.** Two ships pass often, three ships rarely
  ([lessons](../../lessons.md)). If U3's cuts are neutral, a wider pairing (e.g. 4 centres) may find
  the rare three-ship seed sooner; decide with the baseline, not now.
- **Snapshot refactor size.** U2's `SingleField` is the largest code change here. If it outgrows a
  slice, land it behind the existing sequential path and switch the scan in a second, measured unit.
