# Chapter 2 to ten jobs

**Status:** next · space shaping · updated 2026-11-25 — two fixed-cube fleet jobs open the chapter,
then 2-1 to 2-4, the 2-5 fleet capstone, and three jobs that bring Clear into the fleet.

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release) ·
[jobs](../../design/jobs.md)

## Goal

Chapter 2 plays ten jobs: a fixed-cube intro (one pinch moves two ships), the four shipped fleet jobs,
the 3-ship capstone, then Clear with a fleet (one obstacle in more than one ship's path).

## Depends on

[Fleet rebake](../in-progress/fleet-rebake.md): new fleet jobs must not be won by the sun.

## Notes

- **Ids stay.** 02_01–02_04 keep their ids, lines and clips; new jobs take the next free ids and the
  play order comes from `Chapters/02.asset`.
- **Fixed fleets already bake.** `PuzzleMultiShipBake` ships the solved zone as the frame when the
  brief is not `translatable`; no bake code is needed for U1.
- **Multi-ship bakes cannot request a role** (`UnsupportedMechanism` for anything but `Any`). U3 needs
  a way to keep only Clear answers.

## Units

### U1 — the fixed-fleet intro (two jobs)

- Bake two `translatable: false` fleets at low scramble, Ring: 2 ships fan-out (one pinch near the
  shared origin bends both), then 2 ships separate (one zone between them serves both).
- Put them before 2-1 in `Chapters/02.asset`.
- Exit: both job assets validate and play first in chapter 2.

### U2 — 2-5, the capstone

- Bake 3 ships, fan-out, high scramble, no sun answer. Decide by playing it whether it follows 2-4
  or closes the chapter.
- Exit: the job asset validates and is in `Chapters/02`.

### U3 — Clear with a fleet (three jobs)

- First decide how to get a Clear answer from a multi-ship bake: classify the multi-ship result with
  `PuzzleSolutionClassifier` and reject non-Clear seeds, or let the multi-ship bake accept `Clear`.
  Take the smaller change.
- Build the three as a ramp: one obvious blocker → a blocker among decoys → two blockers.
- Exit: three job assets validate as Clear and are in `Chapters/02`.

### U4 — lines, cards and the map

- Client, cargo and Jim's lines for each new job, following the
  [story arc](../in-progress/story-arc.md); manual
  entries for the fixed-fleet and Clear-with-a-fleet lessons. Regenerate the TTS clips.
- Check the map's route and progression across ten jobs.
- Exit: every new job has its card, lines and clips; the fast suite is green; chapter 2 runs end to
  end in the editor.

## Manual check (headset)

1. Chapter 2 shows ten jobs on the map, in order.
2. Intro 1: one pinch on the fixed cube moves both ships' lines.
3. Intro 2: the fixed zone between two separate ships rescues both.
4. 2-5: three ships fan out; it is solvable within budget.
5. The first Clear job: the shared blocker is readable; shrinking it frees both ships.
6. The second and third Clear jobs each add one step of difficulty.
7. Jim's lines and clips play for every new job.
8. Time each new job; the target is ~3 min.
