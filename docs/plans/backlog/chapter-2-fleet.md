# Chapter 2 — the fleet

**Status:** later · space shaping · updated 2026-11-07 — the store release after v1: job 8 (Mixed)
closes chapter 1, then chapter 2 ("the fleet", five jobs) brings in multi-ship. Steps 1–2 of the road
to the store; the design is in [chapters](../../design/chapters.md).

**Related:** [jobs](../../design/jobs.md) · [progression](../../design/systems/progression.md) ·
[puzzle creator](../../../Assets/_Game/Features/PuzzleCreator/README.md) ·
[PuzzleJobContracts.cs](../../../Assets/_Game/Features/PuzzleCreator/Bake/Data/PuzzleJobContracts.cs) ·
[LevelSelector.asset](../../../Assets/_Game/Resources/Chapters/LevelSelector.asset)

## Goal

More jobs without new verbs. Job 8 finishes chapter 1 with both roles at once (Mixed). Chapter 2 raises
the pressure by making several ships share one field and one entropy budget.

## Contract

- **Job 8 stays in chapter 1 (`01_08`).** It is the single-ship closer: clear + curve, before ships
  change what the player tracks.
- **Chapter 2 is its own chapter (`02_01`–`02_05`).** `LevelSelector` already joins chapters into one
  ordered run, and the map and saved progress go by that order, so the fleet follows chapter 1 without
  runtime changes. Its ids do not depend on job 8 baking.
- **Numbers restart per chapter.** This settles progression's "reset per act, or 1 → N?" question for
  the chapter split.
- **Job 8 lands before chapter 2 ships,** or the unlock rule changes first: the current job is the one
  after the furthest solve, so a job 8 added behind solved chapter-2 jobs may never open. Verify against
  `RunProgress.FromSolved`; the fix is "any unsolved job before the current one is open".
- **No new verbs.** Same pinch, same move. Multi-ship adds the pressure that every ship must arrive; the
  budget rule is unchanged (3× the bake's solution).
- **The chapter turns the ramp again.** The difficulty drops when ships arrive (2-1) and rises with the
  relation: separate → fan-out → converge → three ships. The field shape restarts at Ring.
- **Multi-ship cannot request a role.** The bake only guarantees that every ship arrives, so each fleet
  job is checked by playing it. Multi-ship bakes need ≥ 9 planets and always use one movable cube.

## The jobs

| # | Lesson | Brief | Field | Why |
|---|---|---|---|---|
| 1-8 | **Mixed** | 2 zones · 1 ship · Mixed | 12 + sun · Cluster | Shrink the blocker with one zone, grow a well with the other — both roles at once. |
| 2-1 | **Two ships** | movable · 2 ships · separate | 10 + sun · Ring · low scramble | One field, two runs; each ship has its own fix. The only new idea: both must arrive. |
| 2-2 | **Fan-out** | movable · 2 ships · one origin | 10–12 · Spiral | An edit near the shared origin bends both paths — it can solve both or ruin one. |
| 2-3 | **Converge** | movable · 2 ships · one destination | 12 · Spiral | The lever is the planet next to the shared destination: fix the approach once, use it twice. |
| 2-4 | **Three ships** | movable · 3 ships · separate | 12 · Cluster | Three fixes on one budget; the cheap answer uses the field for more than one ship. |
| 2-5 | **Fleet capstone** | movable · 3 ships · fan-out | 12 · Cluster · high scramble | Everything: placement, shared paths and the field doing the work across every ship. |

**Lore.** **Slightly Used Stars Ltd.** returns with a grudge at 2-2; **The Very Small Opera** ("Efficiency
is a virtue until it is a cost") takes the capstone. The manual's "new employee" is now handling
several deliveries at once. Strings follow the existing keys (`init_client`, `init_cargo`,
`init_manual_entry`, `init_01`, `complete`, `fail`) in `Levels_en` and `Levels_es`, under `level_02_0N_*`.

## Units

This is more than a morning; each unit is its own sitting.

### U1 — bake job 8 (Mixed)

- Retry the pending `01_08` row. If it still fails, make it easier for the search: one blocker on the
  line and one well-placed planet, a fixed cube with two zones, and lower scramble.
- **Exit:** `01_08` bakes and plays as clear + curve, or it stays pending and the unlock rule is fixed
  before chapter 2 ships.

### U2 — job contracts carry ships and a chapter

- Add `Ships` and `Relation` to `PuzzleJobContract` and copy them in `PuzzleTestSetBuilder.CreateSeed`.
  Raise `Attempts` on multi-ship rows (the search probes fewer seeds per pass).
- Replace the builder's hard-coded `Chapters/01` paths: each contract's chapter comes from its id, and
  the builder writes `Chapters/NN/Job_NN_MM_Puzzle.asset` and wires `Chapters/NN.asset`.
- The sun is kept out of ship runs by the [two-ship test](../in-progress/two-ship-test.md) plan (U1).
- **Exit:** a two-ship `02_01` row builds through *Build Job Test Set* into chapter 2; chapter 1 still
  rebuilds unchanged.

### U3 — chapter 2 and the map

- Add the `02_01`–`02_05` rows, author `Chapters/02.asset` and add it to `LevelSelector.asset`.
- Add `routeSegments` line renderers in `Game.unity`: 8 today; the joined run of 13 jobs needs 12.
- **Exit:** the map shows both chapters in order with the route line; chapter 2 opens after chapter 1.

### U4 — strings and docs

- Write the per-job strings (en + es). Update the "seven-job run" wording in [jobs](../../design/jobs.md),
  [progression](../../design/systems/progression.md), [core loop](../../design/core-loop.md) and
  [the cube](../../design/systems/cube.md), and add chapter 2 to jobs.
- **Exit:** every new job has its card text; the design docs describe both chapters.

## Manual check (headset)

1. The map shows chapter 1 (jobs 1–8) then chapter 2 (2-1 to 2-5) in order, each with its route line.
2. Job 8: one zone shrinks the blocker, the other grows a well; the solve needs both.
3. 2-1 opens only after chapter 1's last job is solved.
4. 2-1: both ships launch on each pinch; the job solves only when both arrive.
5. 2-2: an edit near the shared origin visibly bends both paths.
6. 2-3: the planet next to the shared destination solves both approaches.
7. 2-4 and 2-5: three ships stay readable in the field; the entropy bar is still legible.
8. Saved progress from a seven-job save survives the update: solved jobs stay lit and the next one opens.
9. Tuning: whether the map should mark the chapter break (gap, label) or read as one galaxy.
