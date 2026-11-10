# The panel bank — the console on two arms from the grid

**Status:** now · presentation · updated 2026-11-10 — the console windows leave the head arc and hang,
smaller, on two arms from the grid's bottom-left-back corner: one at 45° out to the left, one straight
up. Split from [the field meter](../in-progress/field-meter.md). Reviewed twice 2026-11-10; the
corner arms replace the bank-behind-the-field design, which could not clear both the grid and the
reading angle. U1 can start now; U2 can follow once the map square's rest frame exists (the square is
built; the rest frame is part of U2).

**Related:** [the field meter](field-meter.md) · [lore](../../design/world/lore.md) §The console ·
[plans board](../README.md)

## Goal

The console windows sit on an arc around the head like monitors, oversized (card 38 × 30 cm, Jim
30 × 30 cm), while the field floats unattached in front of them. Reading one is a head turn up and out,
then back down to the field. Make the windows, the field and its meter read as one machine.

## Contract

- **One side, a glance.** Both windows sit on the left of the field, within about 35° of straight
  ahead; no turn to the other side.
- **Hung from the grid.** Two arms leave the grid's bottom-left-back corner: one runs up and out to the left at
  45° in the field's front plane (not a 45° head turn), one rises straight up. Each window hangs at the end of its own arm. Arms use the grid's and the
  meter's line style and colour (`FieldMeter.mat`).
- **The pair keeps its shape.** Arm lengths are fixed; only the corner moves with the grid, so the
  pair looks the same in every job and on the map.
- **Above the field in draw order.** The pair stays on the field's left, and its canvases sort above
  the field, so no lattice line, trajectory, grown planet or solve stamp hides the text. The field may
  pass behind the console; that is the one-machine look.
- **Out of the hands' space.** Nothing below or in front of the field; the corner is behind the hands.
- **Readable by angle, not by size.** Reading distance is ~0.7–0.8 m (`SeatSettings` distance 0.45 m
  plus the field half plus the arm). Text cap height ≥ ~1.1° of view (≈1.4 cm at 0.75 m).

## Units

### U1 — smaller windows (can land before the field meter's U3)

- Today the card's text is ~1.3–1.5 cm cap (19–22 px at 0.001 scale, `JobCard.prefab`), near the
  floor at 0.75 m, so a plain 60 % scale breaks the text rule: **shrink by re-authoring the layout**
  (less padding, shorter lines, tighter header), keeping text at or above the floor. Target footprint:
  card ~25–28 cm wide, Jim ~20 cm; the map card shares the card's slot.
- Jim's panel text has `autoSizing: 0` (`Game.unity`, the TTS panel): check his longest line fits the
  20 cm panel, or wrap and grow the panel's height.
- **Exit:** both windows are clearly smaller in the headset and still read at their distance.

### U2 — the corner arms

- **Geometry.** The anchor is the grid's bottom-left-back corner: `FieldCentre + (−half, −half, +half)`
  in puzzle-local space (back = +z, as the meter's −z front implies). The 45° arm runs outward-left and
  up in the field's vertical plane facing the player; the vertical arm rises from just outside the
  corner. Arm lengths are serialized and fixed. The card hangs on the 45° arm (read longest, nearest eye
  level), Jim on the vertical arm; swap if the headset says so. Each window's inner edge stays left of
  the grid's left silhouette edge.
- **Frames and spaces.** `SpaceShapingView.FieldCentre` / `FieldHalf` are puzzle-local and the Puzzle
  instance is scaled 0.75 in `Game.unity`; convert with `shapingView.transform.TransformPoint` /
  `TransformVector` (as `GesturePromptPresenter.MeterFront` does), never the raw values on the Origin.
  Arm points vary per job, so runtime code sets them on authored line renderers (like
  `FieldEdgeMeterView.LayArm`); it creates nothing.
- **The map.** The corner comes from the field meter's U3 map square at rest, not from the live
  selected field (which moves with the cube, breathes, and grows 1→1.8 while pinched). That rest frame
  is new work: `MapStarMeter` lays the square from `RunSlotView.FieldCentreMap` / `FieldHalfMap`, which
  include the live zoom and breath (`MapStarMeter.cs:41`, `RunSlotView.cs:20-26,120-126`), so add a
  rest-frame accessor (or anchor to the square at its 1.8× extent) before taking the corner. Check the
  1.8× pinch preview stays right of the windows; if it can reach them, use the 1.8× extent.
- **One root, one CRT.** The arms and both windows sit under one pair root so a single
  `CrtTransition` boots and cuts the whole console. The card keeps its own `ShrinkOut` for swapping its
  content.
- **Map enter.** The pair cuts off (CRT) when the enter zoom starts and boots back with the job. The zoom
  starts in `PuzzleMapView`'s private `StartEnter`, not at `EnterRequested`: add a public
  `EnterStarted` event there and cut off on it.
- **Wiring.** `SeatPlacement` owns `ConsoleLayout` and calls `ArrangeAround` (`SeatPlacement.cs:18,103`).
  Replace that entry with a pair layout that receives the corner (and the field's right vector) as
  values in Origin space from the owners that already see both views: `PuzzleEditionState` on job
  start, `PuzzleSelectionState` on map show and whenever the map's rest frame changes. Keep
  `Features/UI/Console` free of Puzzle / PuzzleSelection references. Show and hide with the field
  (`SpaceShapingController.HideAll`, the map's `Show` / `Hide`).
- **Tests.** Extract the corner and arm math (pure, like `FieldEdgePath`): the corner for the largest and
  smallest job frames and the map rest frame, the 45° direction, and that each window's inner edge stays
  left of the grid's left edge from the default eye. Fast `SpaceShaping.Tests.EditMode` suite.
- **Exit:** in every job and on the map, both windows hang on their arms on the left of the field,
  joined at the corner, never crossed by the field's content, and readable without turning the head to
  the other side.

## Manual check (headset)

1. Job 1: the card and Jim's call hang left of the field on their arms; reading them is a glance left,
   not a head turn.
2. Both arms leave the grid's bottom-left-back corner: one at 45°, one straight up.
3. No lattice line, planet, grown planet, trajectory or solve stamp crosses either window, in any job.
4. The map: the same pair from the map square's corner, the map card on its arm; on map enter the pair
   cuts off and boots back with the job.
5. Hands never meet a window while pinching or moving the cube.
6. Jim's longest line fits his smaller panel.
7. Text reads at its distance; tuning: arm lengths, which window sits on which arm, the vertical
   panel's height.
