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

### S1 — Creator + data · landed (needs a Unity bake for a real job-3 seed)

- `PuzzleBrief.SupportsMoves` reduced to `Moves <= 3` — fixed multi-zone is now searched like movable.
  The property is kept (not inlined) because both the bake and the search gate on it.
- `PuzzleData` ships `PuzzleFrame[] frames` (position + size) instead of `cubeSize`; `zoneCentre` stays
  the run midpoint (the start frame a movable level ships). `PuzzleFieldBake.BuildFrames` is the one
  frame builder, shared with the editor's loose test-set bake. The solved **scales** are not shipped.
- `PuzzleVerifier.Fits` unchanged: a chain shares one width (`PuzzleBeamSearch` uses `prefix.Size`), so
  the shared-size rule already holds for fixed multi-zone.
- **Exit:** unit tests pin the relaxation (`SupportsMoves`, `Bake` no longer returns `UnsupportedBrief`),
  the fixed/movable search parity, and frame shipping (one per solved zone; one start frame when movable).
  **Still open:** a real fixed 2-cube seed baked and saved through the creator — needs Find-working-seed
  in Unity, which this plan could not run. The POC's single level asset was hand-patched with its one
  frame so it keeps playing.

### S2 — Runtime: the placed cubes · landed (needs a headset/POC check)

- `SpaceShapingView` pools a `SpaceZoneView` per frame (all active at once, extras hidden), applies
  conditions/envelope/falloff to each on creation, and routes `Zone`/`IsScaling`/`Refuse` to the cube
  being scaled. `SpaceShapingController.ResetView` places the frames; the dead `ApplySolvedScale` and
  the unused `Zone` property were removed.
- **Exit:** a 2-cube level plays in the POC — scaling each cube bends the path; reset and next work.
  **Not yet checked in Unity.**

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

- Creator: `SupportsMoves` allows fixed multi-zone; fixed/movable search parity; a fixed multi-zone with a
  one-cube shortcut is rejected (`FewerZones`); frames ship one per solved zone (one start frame when
  movable). A real fixed 2-cube **bake** is still open (S1's exit).
- Runtime: N cube views are placed; each scaled zone appears in the session.

## 5. Out of scope

- The move (job 4) — already built as translatable multi-zone.
- Rotate, multi-ship, runtime generation.

## 6. Risks

- **Legibility** — several cubes bending the same field may clutter; S3 carries that risk.
- **Rebake** — the frame array changes the asset schema; the POC level and the 8-job test set must be
  re-baked through the creator menu (the single POC level was hand-patched to keep playing meanwhile).
