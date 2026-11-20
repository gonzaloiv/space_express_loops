# Chapter 2 to eight jobs

**Status:** next · space shaping · updated 2026-11-19 — add the 2-5 fleet capstone and 2-6 to 2-8,
which bring Clear into the fleet.

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release) ·
[jobs](../../design/jobs.md)

## Goal

Chapter 2 plays 2-1 to 2-8. 2-5 is the fleet capstone (3 ships, fan-out, high scramble); 2-6 to 2-8
teach clearing an obstacle that blocks more than one ship.

## Depends on

[Fleet rebake](fleet-rebake.md): new fleet jobs must not be won by the sun.

## Units

### U1 — 2-5, the capstone

- Bake 3 ships, fan-out, high scramble, no sun answer. Decide by playing it whether it follows 2-4
  or closes the chapter after 2-8.
- Exit: the job asset validates and is in `Chapters/02`.

### U2 — 2-6 to 2-8, Clear with a fleet

- Existing bakes with the `Clear` role and two or three ships: one obstacle in more than one ship's
  path. Build the three as a ramp: one obvious blocker → a blocker among decoys → two blockers.
- Exit: three job assets validate and are in `Chapters/02`.

### U3 — lines and cards

- Client, cargo and Jim's lines for each new job, following the [story arc](story-arc.md); manual
  entries for the Clear-with-a-fleet lesson. Regenerate the TTS clips.
- Exit: every new job has its card, lines and clips.

### U4 — the chapter and the map

- Add the jobs to `Chapters/02.asset`; check the map's route and progression across eight jobs.
- Exit: the fast suite is green; chapter 2 runs end to end in the editor.

## Manual check (headset)

1. Chapter 2 shows eight jobs on the map, in order.
2. 2-5: three ships fan out; it is solvable within budget.
3. 2-6: the shared blocker is readable; shrinking it frees both ships.
4. 2-7 and 2-8 each add one step of difficulty.
5. Jim's lines and clips play for every new job.
6. Time each new job; the target is ~3 min.
