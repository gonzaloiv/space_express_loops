# Multi-ship spike — can one zone rescue two ships?

**Status:** now · space shaping · updated 2026-10-30 — creator-side feasibility spike before any runtime work.

## Why

Multi-ship is one shared field edited by one placeable cube; the ships are the consequence of the
edit, not authored runs. The runtime and bake changes are mechanical, but the direction rests on one
empirical question the code can't answer on paper:

**Does a single cube zone rescue two or more ship runs on a real generated field, with margin?**

If yes, build the bake + runtime (a later plan). If rarely, widen to two moves or drop it — decided
from numbers, not opinion. This plan is that measurement only: no runtime, no `PuzzleData` change,
no scene work.

## What already holds

- A ship launches straight at its destination and is bent by the shared field
  (`ShipLaunch`, `TrajectoryIntegrator`); the run primitive is already per-origin/destination.
- A zone edits the planets' position and scale in its region (`SpaceZone.ApplyTo`); the grid shows
  the resulting warp.
- "Arrive" = the run reaches the destination capture sphere; margin = `TrajectoryResult.ClosestApproach`.

## Slices

### 1. Pair sim + inverted classifier

- Add a creator-side classifier: for a field and one candidate zone, simulate every ordered planet
  pair twice — pristine (start) and zone-applied (edited) — and return the pairs where start is not
  `Arrive`, edited is `Arrive`, and the edited clearance clears a margin threshold.
- Reuse `PuzzleZoneCandidates` for candidate zones, `SpaceShapingSession` for the edited field and
  zones, `ShipLaunch`/`TrajectoryIntegrator` for the pair run.
- Exit: a unit test on a hand-made field returns the expected rescued pairs with their margins.

### 2. Seed scan + report

- Drive slice 1 over K generated fields (`PuzzleSeed` → `PuzzleArtifactFactory`/`PuzzleFieldGenerator`
  → `PuzzleFieldBake.Apply`), sweeping the candidate zones.
- Report per field: the best zone, its rescued-pair count, and their margins; plus how many of K
  fields yield ≥2.
- Exit: a logged report (counts + distribution) we can read.

### 3. Verdict

- Read the report and record go / no-go in this file.
- Exit: if go, a follow-up plan for bake + runtime (2 ships, N-ready); if no-go, widen to two moves
  (greedy zone cover) or stop.

## Exit

A number: on real fields, how often one zone rescues ≥2 runs with margin. That decides whether
multi-ship is built.
