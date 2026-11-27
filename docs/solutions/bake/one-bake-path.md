---
title: One bake path — measure the seed search before building an answer-first generator
date: 2026-11-27
category: design-patterns
module: puzzle-creator
problem_type: design_pattern
applies_when:
  - Deciding whether to add an answer-first generator beside the seed search
  - A fleet bake recipe has not passed one seed
  - Measuring a bake speed-up before committing to a second path
tags:
  - bake
  - seed-scan
  - fleet
  - generator
  - physics
  - construct
---

# One bake path — measure the seed search before building an answer-first generator

## Context

The 2026-11-27 construct chain (core, single, wire, separate, fan-out, jobs) added an answer-first
generator beside the seed search so jobs would bake faster. The single-ship recipes met their bars
(Clear 12/20, Curve 11/20), but chapter 1 was already baked and freelance single slots are cheap
scans. The fleet recipes never converged; the whole chain was removed on 2026-11-27. The search is
the only bake path.

## Guidance

**Measure the search before adding a second path.** The search's cost was never the bottleneck for
the jobs that worked. A second path doubles every bake rule — a `Generator` field, a scratch folder,
pins with attempts, two sets of statuses — for a speed-up nobody measured.

**Measure a fleet idea by the search's passing seeds, not by a guessed recipe.** 02_06 already had
33 passing seeds in its 200-seed scan. A construct recipe that has not passed one seed is not a
shortcut; it is an untested physics problem.

**Answer-first works for one ship and fails on the fleet physics.** With the fleet Curve lesson, the
per-ship blockers placed, but the shared bend at the chords' crossing could not be placed: every one
of the six offsets failed `TryAt` on endpoint/separation clearance. Placing the bend first (before the
blockers) fixed the bend but moved the failure to the start:

- the pristine start crashed into a per-ship blocker with under 5% deviation, failing the Curve brief
  in 310/474 offset attempts;
- the grown bend then failed to land every ship (155/474).

The measured rates were Clear 1/20 and Curve 0/20 for fleets. The next idea is a different design —
a scalable blocker and a bend beside each chord, as the single recipe does — not another tuning knob
at the shared crossing.

**Keep the general fixes.** The search kept the `PuzzleFieldBuilder` refactor, the shared
`PuzzleSunSources.WithSun`, the validator's lines in `PuzzleBakeResult.Problems` on `ContractFailed`,
and `SegmentGeometry.ClosestPoint` under `DistanceToSegment`. No shipped asset, answer, chapter order
or gameplay rule changed.

## What was removed

The `PuzzleConstruct*` types, `PuzzleDecoyField`, `PuzzleGenerator`, the `Recipes/` folder
(`IPuzzleLesson` and the Clear/Curve/Fleet/FanOut lessons) and `PuzzleLessonPlan`, their construct
tests, and the callers' generator/scratch branches (`PuzzleJobContract.Generator`, the scratch
lookups in `PuzzleJobContracts`, `PuzzleSeedScanner.Construct`, the Construct cases in
`PuzzleJobSeedScan` and `PuzzleTestSetBuilder`, and `PuzzleBakeStatus.ClearShortcut`).

## Related

- [Lessons — the answer-first generator](../../lessons.md)
- [Run a long editor bake on a Task over a plain snapshot](../../../Assets/_Modules/Docs~/solutions/architecture-patterns/editor-bakes-on-a-plain-snapshot.md)
- [Bake scan speed](../../plans/backlog/bake-scan-speed.md) — the next search speed pass
- Code: `PuzzleSeedScanner.cs`, `PuzzleSeedBatch.cs`
