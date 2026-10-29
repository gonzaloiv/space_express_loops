# Space Shaping — Several zones on placed cubes

**Status:** now · space shaping · updated 2026-10-29 — a fixed level **places several cubes** and the
player scales each. The "several zones" rung (job 3), split from the move.

**Related:** [progression](../../design/systems/progression.md) §The learning path ·
[the cube](../../design/systems/cube.md) §Inputs · [jobs](../../design/jobs.md) ·
folds in the per-zone legibility the shape-the-path plan left open.

## 1. Why

The design split "several zones" from "the move": a fixed cube covers one region, so a level now
**places several fixed cubes** and the player scales each (job 3); moving the lens is its own later
lesson (job 4). The search and bake already produce multi-zone solutions and the session already
applies several zones, but the creator rejects fixed multi-zone (`PuzzleBrief.SupportsMoves`) and the
runtime holds a **single** cube (`SpaceShapingView`), so the rung is unbuildable as-is. This plan lands
the placed-cube path end to end.

## 2. Slices

### S1 — Creator + data · open

- `PuzzleBrief.SupportsMoves` allows `Moves > 1` when fixed (today it requires `Translatable`).
- `PuzzleData` carries the cube frames (position + size) from the bake; the solved **scales** are not
  shipped. The search is unchanged — it already finds each zone's centre, size and scale.
- `PuzzleVerifier.Fits` accepts per-cube sizes for fixed multi-zone (relax the shared-size rule).
- **Exit:** a fixed 2-cube seed bakes, verifies and saves; a creator test pins it.

### S2 — Runtime: the placed cubes · open

- `SpaceShapingView` holds a cube per frame — **all present at once** — and the gesture routes to the
  cube being scaled; `ResetView` places the frames.
- `SpaceShapingController` previews/commits the **active** cube's zone (the session already upserts
  several zones by region).
- **Exit:** a 2-cube level plays in the POC — scaling each cube bends the path; reset and next work.

### S3 — Legibility · open

- Each cube reads as its own zone so the player knows which to scale (per-zone tint/label); the bend is
  diffuse under gravity-global, so the feedback carries it. Absorbs the parked shape-the-path tail.
- **Exit:** headset pass — the player can tell the cubes apart and see each one's effect.

## 3. Decisions

1. **The bake ships the frames; the player finds the scales.** Each cube's **position and size** come
   from the bake (the solved zone geometry); the level ships them and the player's task is the
   **scale** at each cube. The solved scales are not shipped. Job 3 is a **tutorial**, so the visible
   geometry is the lesson, not a leak — the player still does the verb: scaling the field. The scale may
   go **either way** — a cube's solution can magnify (`scale > 1`) or shrink (`scale < 1`).
2. **Per-cube size, as today.** Each cube's size is calculated like now, per zone, so the cubes may
   differ — the verifier's shared-size rule relaxes for fixed multi-zone.
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
