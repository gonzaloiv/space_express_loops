---
title: Bake scan speed — profile the scan's phases before cutting the candidate grid
date: 2026-11-27
category: performance-issues
module: puzzle-creator
problem_type: performance
applies_when:
  - Deciding whether a seed scan needs a speed cut
  - Sizing a fleet scan's wall clock before a 200-seed hunt
  - Adding timing or progress instrumentation to a bake path
tags:
  - bake
  - seed-scan
  - fleet
  - profiling
  - timings
---

# Bake scan speed — profile the scan's phases before cutting the candidate grid

## Context

A fleet seed was assumed to cost ~2 minutes and the probe was assumed to be the whole cost. A
`PuzzleScanTimings` collector was threaded optionally through the seed scan (pinned bakes and Creator
tests pass null), and the batch report printed per-seed and total phase times, simulation counts and a
status histogram.

## Measured (32 cores, 20 seeds unless noted)

- `01_04` single, 64 seeds, one worker: 60 s; 91% solution search (`select`), no probe simulations. A
  single seed is ~0.9 s.
- `02_03` two ships: 658 s at `-Workers 1`, 105 s at 31 workers; 93% probe, 120,627 simulations; ~43 s
  of CPU a seed.
- `02_06` three ships: 199 s at 31 workers; 92% probe, 332,020 simulations; ~132 s of CPU a seed.
- The 6.3× two-ship speed-up is a floor: 20 seeds leave 11 of 31 workers idle, so the 20-seed wall is
  the slowest seed's run, not 20 × the cost. For longer scans, wall ≈ CPU total ÷ workers.
- 200 three-ship seeds is ~15 min at 31 workers (~23 min if every round waits for the slowest seed).

## Guidance

**Profile the phases before choosing a cut.** The fleet probe is the fleet cost; the single-ship cost
is the solution search, not the run picker. A cut aimed at the wrong phase buys nothing.

**Size a cut from CPU-per-seed, not a 20-seed wall.** With more seeds than workers, wall is close to
the total CPU divided by the worker count; a 20-seed run on a 32-core box hides that.

**No cut was needed.** The slowest shape fits 200 seeds inside an hour, so the scans can run in the
background. If a cut is ever needed, shrink the fleet candidate grid (centres or scales) for that
contract; probe bookkeeping saves no simulation and a pair-skip bound changes results.

## Gotchas

- **Invariant formatting.** Report floats followed the machine's culture (`window=0,160804`); use
  `CultureInfo.InvariantCulture` so the `.txt` report stays parseable on any machine.
- **Optional means nullable.** Pass the collector as a nullable argument and guard every record; do
  not enable timing globally, or the pinned bake and the Creator tests pay for it.
- **Worker-safe.** The probe records from worker threads: use `Interlocked` for phase ticks and the
  simulation count, and read a seed's collector only after its search returns.

## Related

- [One bake path](one-bake-path.md) — the search is the only bake path
- [Lessons](../../lessons.md)
