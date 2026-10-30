# Multi-ship — bake and play a two-ship puzzle

**Status:** now · space shaping · updated 2026-10-30 — bake and play a two-ship puzzle, N-ready.

## Why

The spike proved one placeable zone rescues two or more runs, field-driven, across a usable scale
plateau (10/10 fields had a ≥2 rescue; 6/6 had a plateau, mean ~22% of the scale range). This plan
turns that into a playable puzzle: the creator bakes a two-ship level from a zone's rescued set, and
the runtime simulates, renders, and solves for N ships. Start at two; keep the pipeline N-agnostic.

## Decisions

- **Inverted bake.** The zone is chosen first; the ships are the rescued runs of its field, not
  authored runs. The engine is `PuzzleShipScan`.
- **`PuzzleData.ships[]`** (origin, destination, captureRadius). Additive: an empty array is the
  existing single-ship puzzle, so the current corpus is not rebaked.
- **Solve = all ships arrive.** Fail = not all arrive after the edit — the same shape as today for one.
- **Robustness by plateau, not margin.** `ClosestApproach` is sampling-bound; select zones by the
  width of the scale range that keeps the ship set arriving.
- **Two now, N-ready.** No `2` hardcoded in the search or the sim.

## Slices

### 1. Bake a two-ship puzzle (creator)

- From a zone's rescued set (`PuzzleShipScan`), pick two pairs whose plateau keeps both arriving, and
  emit a `PuzzleData` with `ships[]`, the frame, and the start-fails guarantee (start: not all arrive;
  edited: all arrive).
- Exit: a baked two-ship `PuzzleData`, verified by `PuzzleVerifier` extended to N runs.

### 2. Runtime: session and solve

- `SpaceShapingSetup` carries a `ShipSpec[]`; `SpaceShapingSession.Simulate` returns one result per
  ship; `IsSolved` = all arrive; step budget = max over runs. Empty `ships[]` falls back to the single
  `origin`/`destination`.
- Exit: session unit tests for a two-ship puzzle (all-arrive, one-misses, budget).

### 3. Runtime: view and flow

- `SpaceShapingController` loops ships; `SpaceShapingView` pools N `TrajectoryLineView` and N
  `ShipTravelView` from serialized prototypes (the `FieldPlanetBehaviour` / `AcquireInactive`
  precedent); flight completion fires when all ships land; the grid frame spans all endpoints.
- Exit: the two-ship puzzle plays in `SpaceShapingPOC` — both ships fly, both must arrive, the grid
  covers both runs.

## Exit

A playable two-ship puzzle in `SpaceShapingPOC`, baked and solved from one field edit, on the same code
path that scales to N.
