# Fleet sun answer

**Status:** next · space shaping · updated 2026-11-19 — 2-1 to 2-3 are won by shrinking the sun;
fix the multi-ship generator so fleet jobs need the planets.

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

2-1 to 2-3 are rebaked with no sun answer, and the multi-ship bake can make new fleet jobs (2-5 to
2-8) that the sun does not win. Growing chapter 2 depends on this.

## Why

- Measured 2026-11-15: a cube on the sun wins 2-1 (×0.90–0.70), 2-2 (×0.70–0.60) and 2-3
  (×0.80–0.60). 2-4 is the intended sun job.
- More seeds do not help: in these layouts the sun is the only cube that rescues both ships
  (converge seeds 404–440 all returned `NoSolution` with the sun filter on).

## Units

### U1 — fix the two-ship bake test

- `Bake_ProducesAVerifiedTwoShipPuzzle` fails at seed 2. Find out why and make it green.
- Exit: the test is green in the Creator suite.

### U2 — plan around the sun

- Give `PuzzleMultiShipBake` a layout choice: the sun off the ships' corridor, or a sun-free brief.
  Apply `PuzzleSunRule.NoSunAnswer` to multi-ship fields.
- Exit: a Creator test where a two-ship field bakes with no sun answer.

### U3 — background search

- Run the multi-ship seed search as a cancellable background job, never on the main thread or through
  the editor bridge (a 50-minute run froze the editor). Each two-ship seed costs 60–90 s.
- Exit: the creator window starts a fleet search, shows its progress and can cancel it.

### U4 — rebake 2-1 to 2-3

- Keep each job's `id`, lines and progression role (2-1 separate ships, 2-3 converge).
- Exit: each validates with no sun answer; 2-4 stays the sun job.

If U2 grows past a morning, split U3–U4 into their own plan.

## Manual check (headset)

1. 2-1 to 2-3: a cube on the sun does not solve.
2. Each is solvable within budget by working the planets.
3. Jim's lines and the cards still match the new fields.
4. 2-4 still teaches the sun with the fleet.
