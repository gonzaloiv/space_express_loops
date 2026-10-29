# Space Shaping — Several zones on placed cubes

**Status:** now · space shaping · updated 2026-10-29 — a fixed level **places several cubes** and the
player scales each. The "several zones" rung (job 3), split from the move.

**Related:** [progression](../../design/systems/progression.md) §The learning path ·
[the cube](../../design/systems/cube.md) §Inputs · [jobs](../../design/jobs.md).

## 1. Why

The design split "several zones" from "the move": a fixed cube covers one region, so a level now
**places several fixed cubes** and the player scales each (job 3); moving the lens is its own later
lesson (job 4). The search and bake already produce multi-zone solutions and the session already
applies several zones, but the creator rejects fixed multi-zone (`PuzzleBrief.SupportsMoves`) and the
runtime holds a **single** cube (`SpaceShapingView`), so the rung is unbuildable as-is. This plan lands
the placed-cube path end to end.

## 2. Slices

### S1 — Creator + data · open

- `PuzzleBrief.SupportsMoves` allows `Moves > 1` when fixed (today it requires `Translatable`); the
  property then reduces to a bare `Moves <= 3`, so inline it at the bake's call site.
- `PuzzleData` carries the cube frames (position + size) from the bake, replacing `zoneCentre` +
  `cubeSize`; the solved **scales** are not shipped. The search is unchanged — it already finds each
  zone's centre, size and scale. The frame count is the move count: `Fits` pins
  `solution.Length == puzzle.moves`.
- `PuzzleVerifier.Fits` needs no change: a chain shares one width (`PuzzleBeamSearch` uses `prefix.Size`),
  so the shared-size rule already holds for fixed multi-zone.
- The bake guarantees **distinct regions**: `SpaceShapingSession.UpsertZone` merges zones with the same
  centre and half-extents (`SpaceZone.SameRegion`), so two cubes at the same place and size would
  collapse into one zone.
- **Exit:** a fixed 2-cube seed bakes, verifies and saves; a creator test pins it. The exit leans on the
  field and the settings asset (`planetRadiusMax` well under `runDistance / 2`), so name the seed and the
  settings the test uses rather than scanning.

### S2 — Runtime: the placed cubes · open

- `SpaceShapingView` holds a cube per frame — **all present at once** — and the gesture routes to the
  cube being scaled; `ResetView` places the frames.
- `SpaceShapingController` previews/commits the **active** cube's zone (the session already upserts
  several zones by region).
- **Exit:** a 2-cube level plays in the POC — scaling each cube bends the path; reset and next work.

### S3 — Legibility · open

- Each cube reads as its own zone so the player knows which to scale (per-zone tint/label); the bend is
  diffuse under gravity-global, so the feedback carries it. Absorbs the per-zone legibility left open when
  the window/clock were retired.
- **Exit:** headset pass — the player can tell the cubes apart and see each one's effect.

## 3. Decisions

1. **The bake ships the frames; the player finds the scales.** Each cube's **position and size** come
   from the bake (the solved zone geometry); the level ships them and the player's task is the
   **scale** at each cube. The solved scales are not shipped. Job 3 is a **tutorial**, so the visible
   geometry is the lesson, not a leak — the player still does the verb: scaling the field. The scale may
   go **either way** — a cube's solution can magnify (`scale > 1`) or shrink (`scale < 1`).
2. **Uniform cube size.** A baked chain shares one width (`PuzzleBeamSearch` uses `prefix.Size`), so a
   fixed multi-zone level's cubes are all the same size and the verifier's shared-size rule already
   passes — no relaxation. Differing per-cube sizes would need beam changes and are out of scope.
3. **Two cubes to start, editable.** The count defaults to 2 and is a per-level knob (up to the 2–3 in
   `jobs.md`).
4. **All cubes at once.** The runtime holds a cube per frame, all visible and independently scalable —
   not a single cube stepping through frames.

Settled by the design change: job 3 teaches several zones on placed fixed cubes; the move is job 4.

## 4. Tests

- Creator: a fixed 2-cube bake verifies; a fixed multi-zone with a one-cube shortcut is rejected
  (`FewerZones`); `SupportsMoves` allows fixed multi-zone; a level whose solution shrinks one cube and
  grows the other bakes.
- Runtime: N cube views are placed; each scaled zone appears in the session.

## 5. Out of scope

- The move (job 4) — already built as translatable multi-zone.
- Rotate, multi-ship, runtime generation.

## 6. Risks

- **Legibility** — several cubes bending the same field may clutter; S3 carries that risk.
- **Rebake** — the frame array changes the asset schema; re-bake the POC levels.
