# Seeds retune

**Status:** now · space shaping · updated 2026-11-19 — make `JobSet_MeetsTheContract` green against
the shipped settings without changing what chapter 1 plays like.

**Related:** [plans board](../README.md) · [release](../../design/release.md) · [chapters](../../design/chapters.md)

## Goal

Every chapter 1 contract bakes and validates with the shipped `SpaceShapingSettings.asset`, and every
shipped job asset in `Resources/Chapters/01` validates against its contract. The new release jobs
start from a green job set.

## Why

- Since the settings changed (`runReach` 2, `shipClearance` 0.002), seeds 01_03 (146), 01_05 (124)
  and 01_07 (26) no longer reproduce their jobs.
- 01_01 fails `growOnly`. Likely cause: `PuzzleJobBakeTests.Seed` copies every contract field except
  `GrowOnly`, so the baked puzzle never gets it. Check this before touching the asset.

## Rule

**The shipped assets are the content; the contracts follow them.** Rebake a shipped job only if the
asset itself fails its contract. A rebaked job needs a headset replay before the plan closes.

## Units

### U1 — diagnose

- Copy `GrowOnly` in `PuzzleJobBakeTests.Seed`.
- Run `JobSet_MeetsTheContract` (Explicit, Creator suite) and record each failure: status, seed and
  the validator's problems.
- Exit: a short per-job table in this plan (job, failure, cause).

### U2 — validate the shipped assets

- Add a fast test that loads each `Job_01_0x_Puzzle.asset` and runs `PuzzleJobValidator.Validate`
  with `PuzzleSimSettings.From(settings)` against its contract. It needs the job's answer: use the one
  stored in the asset, or re-solve it if none is stored.
- Exit: the test runs in the fast suite; any red job is listed for U3.

### U3 — retune the seeds

- For each red contract: scan seeds (raise `Attempts`) for one whose bake matches the shipped job
  (same mechanism, zones, routes, sun rule, start rule). Write the new seed into
  `PuzzleJobContracts`; leave the asset alone if U2 passed it.
- If a shipped asset fails U2, rebake it keeping its `id`, lines and planet-name style (as 1-4 was
  rebaked on 2026-11-15), and flag it for the headset.
- Exit: `JobSet_MeetsTheContract` green; U2's test green.

## Notes for the agent

- Each seed scan is a full bake: run the Creator suite on purpose, never on every change.
- `Bake_ProducesAVerifiedTwoShipPuzzle` is known red; it belongs to
  [fleet sun answer](../backlog/fleet-sun-answer.md).

## Manual check (headset)

1. Jobs 1-1 to 1-7 open and play as before.
2. Any rebaked job: the card's lines match the planets, and the answer is the intended lesson.
3. 1-1 still only lets the player grow.
