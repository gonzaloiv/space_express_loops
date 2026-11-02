# Puzzle solution mechanism — curve or clear

**Status:** now · space shaping · updated 2026-11-02 — role classify/request done; only the start check
is open.

**Related:** [the cube](../../design/systems/cube.md) · [jobs](../../design/jobs.md) ·
[core loop](../../design/core-loop.md)

## Why

Scaling a source changes both its radius and its well, so the bake cannot tell a cleared blocker from a
curved well. The creator now names the role so a job teaches the intended one.

## Done

- `PuzzleSolutionKind` (`Other`, `Curve`, `Clear`, `Mixed`) is classified per source from the solved
  scales, with the nearest crash source and solved-path blockers.
- `PuzzleMechanism` (`Any`, `Curve`, `Clear`, `Mixed`) is on the seed/brief. The bake accepts only a
  cheapest route with the requested role; a cheaper wrong-role route is `WrongRoleRoute`. `minRoutes`
  enforces the route count. Multi-ship role requests remain unsupported.
- The creator shows the baked role. Classifier tests cover Clear, Curve, Mixed, Other, fixed-crasher,
  start-miss and grow-then-shrink.

## Open — start check

- `StartPuzzle`/`ResetPuzzle` simulate the untouched field, show its trajectory as a result, and fly it.
  The first pinch interrupts it; `OnFlightCompleted` reports `CRASHED`/`MISSED` without failing the job;
  reset replays it.
- Tutorial: `NoFail` must require `StartOutcome == Crash`; the current guard also accepts a chord body,
  which allows a `MISSED` tutorial.
- Test: the job opens with the blocked start flying and its line visible; the panel says
  `CRASHED`/`MISSED`; the first pinch interrupts; reset replays.

## Out of scope

- Baseline hiding, arrival-radius alignment and endpoint-source identity — separate read fixes.
- Content re-bake (jobs 1–7) — design pass.
- Multi-ship role requests — its search is not sorted by entropy.
