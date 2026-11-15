# No sun answer

**Status:** later · space shaping · updated 2026-11-15 — U1 and U2 landed (the `NoSunAnswer` rule;
1-4 rebaked at seed 79). Left for after the competition: chapter 2 needs generator design, not
seeds.

**Related:** [plans board](../README.md) · [chapters](../../design/chapters.md)

## Goal

Job 1-4 is won by moving the cube to a planet, never by shrinking the sun. Chapter 1 then has no sun
answer outside job 1-2's lesson, and 2-4 stays the fleet's sun job.

## Why

- `PuzzleSunRule.OutOfZone` only keeps the sun out of the *chosen* answer. Nothing stops another
  answer through the sun.
- Measured 2026-11-15 with the shipped settings and every ship required to arrive
  (`PuzzleSunRuleCheck.SunAnswers`): a cube on the sun wins 1-2 (the sun lesson, intended), 1-7
  (`InZone`, intended), 2-1, 2-2 and 2-3. 1-4 did too until U2. **2-4 does not** — an earlier
  one-ship check wrongly called it the sun job.
  - **2-1:** sun ×0.90–0.70 (cost 0.10–0.30 of 0.30); **2-2:** ×0.70–0.60; **2-3:** ×0.80–0.60.
- Today's stopgap: 1-4, 1-6 and 1-7 start their cube where scaling alone cannot win
  (`PuzzleStartFrame`), so the player must move it. The sun stays scalable everywhere: a locked sun
  (`fixedMass`) read as a bug and was reverted.

## Scope

One job: **1-4**, through the single-ship bake (`PuzzleFieldBake`), which is the reliable path.
2-1 is a stretch only: it bakes through `PuzzleMultiShipBake`, whose
`Bake_ProducesAVerifiedTwoShipPuzzle` is already red. If time is short, stop after U2.

## Units

### Landed 2026-11-15

- **U1:** `PuzzleSunRule.NoSunAnswer` (rejects with `PuzzleBakeStatus.SunAnswers`; the validator
  checks it given sim settings). `PuzzleStartFrame.SolvesInPlace` now requires every ship and honours
  `growOnly`. Tests: `PuzzleSunAnswerTests`.
- **U2:** 01_04 rebaked at seed 79: start cube on the sun, no sun answer; the answer grows thennilia
  ×1.18 (cost 0.18 of 0.54), 22 cm from the start. Seeds 16–78 were rejected, mostly `NoSolution`.

### U1 — the rule

- Add `PuzzleSunRule.NoSunAnswer`: after the answer is found, a cube centred on the sun, at the job's
  cube size and every scale in the player's range, must not land every ship within budget. Reuse
  `PuzzleStartFrame.SolvesInPlace(puzzle, settings, sunCentre, size)`.
- Check it where the bake confirms a field (`PuzzleFieldBake.Confirm`, beside `PuzzleStartCheck`);
  reject with a new `PuzzleBakeStatus.SunAnswers` and show it in `PuzzleCreatorWindow`.
- `PuzzleSunRuleCheck` / `PuzzleJobValidator` accept `NoSunAnswer` (the chosen answer may not cover
  the sun either).
- Exit: a unit test in the fast Creator tests where a sun-only field is rejected and a planet answer
  passes.

### U2 — remake 1-4

- Switch 01_04's contract (`PuzzleJobContracts`) to `NoSunAnswer`. Scan seeds (`Attempts` up) until
  one bakes as a Curve with 2+ routes, translatable, start rule `FieldCentre`.
- Bake with the shipped `SpaceShapingSettings.asset` (`runReach` 2, `shipClearance` 0.002).
- Overwrite `Resources/Chapters/01/Job_01_04_Puzzle.asset` keeping its `id`, lines and planet-name
  style; check the card's client, cargo and manual still read ("Move the Wrap…").
- Exit: the new 1-4 validates (`PuzzleJobValidator` with sim settings), its start cube does not solve
  in place, and no cube on the sun solves it.

### U3 — chapter 2 (after the competition; needs generator design)

- Tried 2026-11-15 for 2-3 (converge, `PuzzleSeed_TwoShips_Converge`). Without a filter, every
  converge seed that baked (405, 406, 416) was won by the sun. With `PuzzleSunShortcut` (the probe
  drops runs a sun cube rescues and skips cubes over the sun), seeds 404–440 all returned
  `NoSolution`: in these layouts the sun is the only cube that rescues both ships. The same probe on
  2-1's shipped field (separate ships) found no non-sun cube either (62 s).
- Decision: chapter 2 ships as is. The fleet is the new idea and the sun is the familiar lever;
  reordering broke the progression (2-4 is the only three-ship job).
- A non-sun fleet job needs the multi-ship generator to plan around the sun (fields where the sun
  sits off the ships' corridor, or a sun-free brief), not more seeds. Each two-ship seed costs
  60–90 s on the main thread: search it as a cancellable background job, never through the editor
  bridge (a 50-minute run froze the editor).
- Fix `Bake_ProducesAVerifiedTwoShipPuzzle` first; it already fails at seed 2.

## After the competition

- Retune seeds 01_03, 01_05 and 01_07 to the current settings, or pin the shipped jobs in their
  contracts; fix 01_01's `growOnly` so `JobSet_MeetsTheContract` is green.

## Notes for the agent

- Suites: `DigitalLove.Game.SpaceShaping.Tests.EditMode` is fast; the `.Creator.Tests.EditMode` suite
  bakes and takes tens of seconds. `PuzzleShipSweepTests` / `PuzzleShipPlateauTests` are `[Explicit]`.
- Known red before this plan: `JobSet_MeetsTheContract` (01_01 `growOnly`) and
  `Bake_ProducesAVerifiedTwoShipPuzzle`. Do not count them against U1–U2.

## Manual check (headset)

1. Job 1-4: the cube starts on the sun; scaling it there does not solve.
2. Job 1-4: carrying the cube to thennilia and growing it a little solves within budget.
3. Job 1-4: the card's lines read right with the new planets.
4. Jobs 1-3 and 1-5 play as before (their data did not change).
