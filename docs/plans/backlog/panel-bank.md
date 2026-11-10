# The panel bank — one console built on the grid

**Status:** next · presentation · updated 2026-11-10 — the console windows leave the head arc and hang
from one bank behind the field, built from the grid's own lines. Split from
[the field meter](../in-progress/field-meter.md). Reviewed 2026-11-10 and decided: one stem, a worst-case
margin, a cut-off on map enter. U1 can start now; U2 waits on the field meter's U3 (the map frame).

**Related:** [the field meter](../in-progress/field-meter.md) · [lore](../../design/world/lore.md) §The console ·
[plans board](../README.md)

## Goal

The console windows sit on an arc around the head like monitors, oversized (card 38 × 30 cm, Jim
30 × 30 cm), while the field floats unattached in front of them. Reading one is a head turn up and out,
then back down to the field. Make the windows, the field and its meter read as one machine.

## Contract

- **The windows straight ahead.** The windows and the bank's rule read within about ±25° of straight
  ahead (the field itself spans ~±27–35° and is not part of this check); no sideways head turn.
- **Built from the grid.** The stem, rule and frame use the grid's and the meter's line style and colour
  (`FieldMeter.mat`).
- **Out of the hands' space.** Nothing below or in front of the field.
- **Not over the field.** The bank's bottom stays above the field's worst-case top (grown planets and
  the solve stamp included).
- **Readable by angle, not by size.** Reading distance at the bank is ~0.7–0.8 m (`SeatSettings`
  distance 0.45 m + field half + the bank's setback), not 0.5 m. Text cap height ≥ ~1.1° of view at the
  bank plane (≈1.4 cm at 0.75 m).

## Units

### U1 — smaller windows (can land before the field meter's U3)

- Today the card's text is ~1.3–1.5 cm cap (19–22 px at 0.001 scale, `JobCard.prefab`), which already
  sits near the floor at 0.75 m. So a plain 60 % scale breaks the text rule: **shrink the windows by
  re-authoring their layout** (less padding, shorter lines, tighter header), keeping text at or above
  the floor. Target footprint: card ~25–28 cm wide, Jim ~20 cm; the map card shares the card's slot.
- **Exit:** both windows are clearly smaller in the headset and still read at the bank's distance.

### U2 — the bank

- **Frames and spaces.** `SpaceShapingView.FieldCentre` / `FieldHalf` are puzzle-local; the Puzzle
  instance is scaled 0.75 in `Game.unity`. Convert with `shapingView.transform.TransformPoint` /
  `TransformVector` (as `GesturePromptPresenter.MeterFront` does), never the raw values on the Origin.
- **The map frame** comes from the field meter's U3 (the map square's rest frame), not from the live
  selected field, which moves with the cube and breathes. **On map enter** (the slot zooms to
  `enterScale` 3.6) the bank does a CRT cut-off (`CrtTransition.CutOff`, like the boot) as the zoom
  starts and boots back with the job, so it never overlaps the zoom.
- **Geometry: one stem.** A single thin post rises from the meter's back-centre (where both meter arms
  end, `FieldEdgePath.Point(1, ±1, half)`) to the middle of the bank's rule. The bank has a fixed width
  set by the two windows, centred on the stem, the same in every job and on the map. The stem's points
  vary per job, so runtime code sets them (like `FieldEdgeMeter.LayArm`) on an authored line renderer.
- **Setback.** The stem stands at the grid's back edge, so the bank sits just behind the field; tune
  the setback in the headset.
- **Clearance: worst-case margin.** `FieldFrame.Measure` uses home positions and initial scales, while
  zones grow sources up to 1.8× (`SpaceShapingSettings.maxScaleFactor`) and the solve stamp sits above
  the destination. Once per job, place the bank's bottom above the field's top by the largest source's
  growth at 1.8× plus the stamp's height; it never moves during play.
- **Wiring.** `SeatPlacement` owns `ConsoleLayout` and calls `ArrangeAround` (`SeatPlacement.cs:18,103`).
  Replace that entry with a bank layout that receives the frame as a value (centre, half in Origin
  space) from the owner that already sees both views: the flow states (`PuzzleEditionState` on job
  start, `PuzzleSelectionState` on map show). Keep `Features/UI/Console` free of Puzzle /
  PuzzleSelection references. Show and hide with the field (`SpaceShapingController.HideAll`, the map's
  `Show` / `Hide`).
- **Tests.** Extract the bank's frame math (pure, like `FieldEdgePath`) and cover the largest and
  smallest job frames and the map rest frame in the fast `SpaceShaping.Tests.EditMode` suite.
- **Exit:** in every job and on the map, the bank sits behind and above the field, joined to the grid,
  with no overlap at rest, a grown planet, or the solve stamp.

## Manual check (headset)

1. Job 1: Jim's call and the card hang behind the field; reading them is a glance up, not a head turn.
2. A single stem rises from the meter's back-centre into the middle of the bank's rule.
3. No planet, grown planet, trajectory or solve stamp covers the bank, in any job.
4. The map: the same bank behind the map field, the map card in the card's place; on map enter the bank
   cuts off and boots back with the job.
5. Hands never meet a panel while pinching.
6. Text reads at the bank's distance; tuning: bank height and setback behind the field.
