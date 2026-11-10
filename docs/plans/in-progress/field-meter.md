# The field meter — entropy on the field's edge

**Status:** now · presentation · updated 2026-11-10 — U1 is in: the entropy bar is the bottom edge of
the field's grid. Next: prompts teach the meter (U2), the same square under the map shows the
chapter's stars (U3), then the console windows become one bank built on the grid (U4).

**Related:** [core loop](../../design/core-loop.md) §The bar is the meter · [the cube](../../design/systems/cube.md) ·
[plans board](../README.md)

## Goal

The entropy bar used to float above the cube as a HUD. The console windows sit on an arc around the
head like monitors, oversized (card 38 × 30 cm, Jim 30 × 30 cm), while the field floats unattached in
front of them. Make the field, its meter and the windows read as one machine.

## Contract

- **The field is the meter.** Feedback is drawn on the grid's base square, not on a panel. No digits
  in the field; the lab panel keeps the numbers.
- **Two arms, front to back.** Each arm runs from the base's front-centre (the player's side, local
  −z) around one side to the back-centre (`FieldEdgePath`).
- **In a job it shows entropy.** Spending cuts both arms from the front; ticks at ⅓ and ⅔ are the 3★
  and 2★ limits (`PuzzleGrade`). Remaining is accent, the last edit neutral, the pinch preview accent
  (danger when it would overflow).
- **On the map it shows the chapter.** The same square, the same arms, fed stars instead of entropy.
- **Authored, not invented.** Arms and ticks are line renderers on the prefabs (`FieldMeter.mat`,
  URP Particles/Unlit so vertex colours show); runtime code only sets points and colours.

## Units

### U1 — the edge meter — done (2026-11-10)

- `FieldEdgeMeter` (`Puzzles/Views/Feedback`) on `Puzzle.prefab` › `FieldMeter`; geometry in
  `FieldEdgePath` (`Puzzles/Domain/Spacetime`, tested). `SpaceShapingView` exposes `FieldCentre` /
  `FieldHalf`. `EntropyBarView` is deleted. Waits on the headset check below.

### U2 — teach the meter

- **Two new prompts** in the gesture prompt panel (`GesturePrompts`), shown until learned like the
  others (`GestureLearning` cookies):
  - **Spend** — loop: the Scale motion with the base square under the cube shortening from the front as
    the hands pull. Line `> EACH PINCH SPENDS ENTROPY`. Shows in job 2 (first job that can fail on
    budget) right after the first committed pinch, not on entry, so it never stacks with Scale.
    Learned when that job ends.
  - **Stars** — loop: the square alone; the gap slides past the first tick and a star above it dims.
    Line `> SPEND LESS, EARN MORE STARS`. Shows on entry to job 3 (first graded job). Learned on the
    first graded solve.
  - Art from `Tools/Our Space/Render Gesture Prompt Art`: add the base square to `GestureCaptureRig`
    and two motions to `GestureMotion`. `GestureKind` gains `Spend` and `Stars` (renaming it to
    `PromptKind` is optional cleanup, not required).
- **A leader line from each prompt to its subject.** A thin phosphor line (authored line renderer on
  the prompt panel prefab) from the panel's edge to what it teaches: Scale and Move → the cube, Enter →
  the map cube, Spend and Stars → the meter's front-centre. Exit has no subject and no line. The
  presenter passes the subject point per prompt; the line follows it while the prompt shows.
- **The meter's own label:** one small text at the base's front-centre, `ENTROPY`, switching to
  `OVER BUDGET` while a preview would overrun. No other states, no digits.
- Strings in the `Global` table, English and Spanish.
- **Exit:** a fresh save reaches job 2, pinches once and sees Spend pointing at the meter; job 3 shows
  Stars; neither shows again; every prompt's line lands on its subject.

### U3 — the square under the map

- The map gets the same square under its field: a second meter instance on the map (author it next to
  the map field in `Game.unity` › `PuzzleMapView`), framed from the map field's bounds.
- Split the drawing from the data: the arm/tick layout and the gradient become one view that takes a
  frame (centre, half) and a set of shares; `FieldEdgeMeter` (entropy) and a new map feeder (stars)
  each fill it. Do not copy the layout code.
- Map shares: both arms fill with **stars earned / stars possible** in the chapter, from the front.
  Possible = 3 × graded jobs (skip `NoGrade` jobs). Source: `RunProgress.Stars(i)` and `JobEntry`
  (`PuzzleSelection/Domain`). One tick per graded job along each arm instead of the ⅓/⅔ ticks; the
  meter needs a tick count, not four fixed ticks (author the maximum and hide the rest).
- No last-edit or preview segments on the map; one colour for earned (accent), dim for the rest.
- **No word over the selected job.** Remove `MapSelectionLabel` (hard-coded, unlocalized "locked" /
  "next" / "solved · n/3") and its text object; each state is shown by the field instead:
  - **Locked:** the diorama is outline-only and dim (no fill, no glow); when the map cube reaches it,
    the cube's fill stays dark, so it reads as closed before the pinch. The existing refusal confirms.
  - **Next:** the only fully lit diorama that breathes; the route line ends at it.
  - **Solved:** the slot's stars (`RunSlotView.starsLabel`, text today) become the same stamped star
    glyphs as the solve moment (`StarStampView` look).
  - Check `MapNodeTint` / `RunSlotView` / `PuzzleDioramaView` first; extend what they already do
    rather than adding a parallel tint.
- **Exit:** the map shows the square under its field; solving a job for more stars lengthens both arms
  on return; a fresh save shows an empty square with its ticks; locked, next and solved jobs are told
  apart with no text over the selection.

### U4 — one panel bank on the grid (decided 2026-11-10)

- Shrink both windows to about 60% (card ~23 × 18 cm, Jim ~18 × 18 cm); keep text cap height
  ≥ ~1 cm at 0.5 m.
- Replace the head arc (`ConsoleLayout`) with one bank **behind and a little above the field**: the
  grid's two back corners rise as thin rails, a top rule spans the bank, Jim (left) and the card
  (right) hang from it side by side in one frame. Same line style and colour as the grid and meter.
- The bank keeps a fixed width centred on the rails (grid size changes per job); its bottom stays
  above the grid's top so planets and the trajectory never overlap it.
- Nothing below or in front of the field: that is the hands' space.
- **Exit:** everything reads within about ±25° of straight ahead; no sideways head turn; text is
  readable in every job.

## Manual check (headset)

1. Job 1: the base edge glows all round; no floating bar above the cube.
2. Pinch: the front of both arms changes colour as the preview grows; release: the last edit shows
   beside the spent part.
3. Spend past a third: the gap passes the first tick; the solve shows 2★.
4. An over-budget pinch turns the preview red.
5. The gap opens on the player's side, not the back (else flip the −z assumption in `FieldEdgePath`).
6. The meter follows the field when it is re-placed (headset off and on).
7. Fresh save, job 2: after the first pinch the Spend prompt shows, its line landing on the meter;
   job 3 shows Stars; neither returns on replay.
8. Every prompt's line lands on its subject (cube, map cube, meter); Exit has none.
9. The meter reads `ENTROPY`, and `OVER BUDGET` during an overrunning preview.
10. Map: the square shows the chapter's stars, one tick per graded job; a better solve lengthens it.
    Locked jobs are outline-only and the cube stays dark over them; solved jobs show stamped stars;
    no word above the selection.
11. Tuning: line width and brightness against the lattice; never brighter than the trajectory line.
