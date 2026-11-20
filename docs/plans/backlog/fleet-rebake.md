# Fleet rebake

**Status:** next · space shaping · updated 2026-11-20 — search fleet seeds in cancellable slices, then
rebake 2-1 to 2-3 with no sun answer. Blocked on [fleet sun answer](../in-progress/fleet-sun-answer.md)
U2 (`PuzzleSunPlacement` / `OffCorridor` do not exist yet); nothing here has started.

**Related:** [plans board](../README.md) · [fleet sun answer](../in-progress/fleet-sun-answer.md) ·
[release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

2-1 to 2-3 are rebaked through the new sun placement and validate with no sun answer. Each keeps its
`id`, lines and role; 2-4 stays the sun job.

## Why

- The current 2-1 to 2-3 fields are won by shrinking the sun.
- Each two-ship seed costs 60–90 s, and a 50-minute run through the editor bridge froze the editor, so
  the search needs its own cancellable job.

## Units

### U3 — cancellable fleet search

- Decision: run `PuzzleMultiShipProbe.Find` on a `Task` over plain data, never on the editor thread and
  never through the bridge. The probe, `PuzzleShipScan` and `PuzzleSunShortcut` currently read
  `PuzzleData` directly; copy the inputs they need (`PuzzleSource[]`, cube range, origin/destination,
  settings) into a plain `FleetField` at bake start, and keep field construction and variant
  confirmation on the main thread.
- Cancellation: a `CancellationToken` checked between candidate centres; progress through a thread-safe
  counter the creator window polls in `EditorApplication.update`. Launch it from a new window button;
  keep the synchronous `Find working seed` for single-ship and tests.
- Exit: the creator window starts a fleet search, shows per-seed progress, cancels cleanly, and the
  editor stays responsive through a multi-seed scan.

### U4 — rebake 2-1 to 2-3

- Keep each job's `id`, lines and progression role (2-1 separate ships, 2-3 converge); bake with 2-1
  `OffCorridor` and 2-2/2-3 sun-free, then save each variant over its asset.
- Add `02_01`–`02_03` to `PuzzleJobContracts.All` with `NoSunAnswer` (it lists chapter 1 only), so
  the bake and the validator have a contract to check against.
- Move `PuzzleSunAnswerTests`' sun-answer fixture from `02_01` to `02_04`: once 2-1 is rebaked,
  `SunOnlyJob_SunAnswers` and `Validator_RejectsASunAnswerUnderNoSunAnswer` turn red on it.
- Exit: a Creator test loads each saved 02_01–02_03 asset, re-solves it, verifies the solution and
  asserts no sun answer; 2-1 keeps its sun; 2-4 unchanged.

If U4's fields do not bake after a day of seeds, split the layout retune into its own plan rather than
expanding this one.

## Manual check (headset)

1. 2-1 to 2-3: a cube on the sun does not solve.
2. Each is solvable within budget by working the planets.
3. 2-1 still has its sun; 2-2 and 2-3 have none.
4. Jim's lines and the cards still match the new fields.
5. 2-4 still teaches the sun with the fleet.
