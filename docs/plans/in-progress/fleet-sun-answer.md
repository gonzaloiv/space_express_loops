# Fleet sun answer

**Status:** now · space shaping · updated 2026-11-20 — make the multi-ship generator bake fleet fields
the sun cannot solve; the rebake follows in [fleet rebake](../backlog/fleet-rebake.md).

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release) ·
[fleet rebake](../backlog/fleet-rebake.md)

## Goal

A two-ship field bakes with `SunRule.NoSunAnswer`: either no sun, or a sun the ships' corridors do not
touch. 2-1 keeps a sun on the field; 2-2 and 2-3 drop it.

## Why

- Measured 2026-11-15: a cube on the sun wins 2-1 (×0.90–0.70), 2-2 (×0.70–0.60) and 2-3
  (×0.80–0.60). 2-4 is the intended sun job.
- The search half of the rule shipped in `565bb7b0`: `PuzzleMultiShipProbe` skips cubes over the sun
  and drops the runs a sun cube rescues, and `PuzzleMultiShipBake.Reject` rejects a variant the sun
  still solves. The field is the missing half — `PuzzleSunSources.WithSun` pins the sun at
  `Vector3.zero`, and the run helpers assume it there, so every corridor meets it.
- More seeds do not help: converge seeds 404–440 all return `NoSolution` with the sun filter on.

## Units

### U1 — green the two-ship bake test

- Done: seed 2's default nine-planet field has no zone that rescues two separate runs, so the bake
  returned `NoSolution`. Seed 13 bakes (found by scanning 1–60 with the same brief); the test now uses
  it. Run it selected in the Test Runner — the bridge runner skips explicit tests.
- Exit: green in the Creator suite.

### U2 — sun placement for fleet fields

- Done: `PuzzleSunPlacement.Centre/OffCorridor`; `OffCorridor` picks a home clear of every planet-pair
  chord (`PuzzleSunPlacementChoice`), and `PuzzleRunChoice` now uses the sun's home instead of the
  origin. Chapter 1 stays on `Centre`.
- Exit: `OffCorridorSun_BakesWithoutASunAnswer` is green with seed 17 and a 0.12 sun; the Creator suite
  is 145/145. 2-1 can bake `OffCorridor`; 2-2 and 2-3 bake sun-free in [fleet rebake](../backlog/fleet-rebake.md).

## Manual check (headset)

1. A generated 2-1 field still shows its sun, and a cube on it does not solve.
2. A generated 2-2/2-3 field has no sun and is solvable by working the planets.
3. 2-4 is untouched and still teaches the sun with the fleet.
