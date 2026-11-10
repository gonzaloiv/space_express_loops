# The field meter — entropy on the field's edge

**Status:** now · presentation · updated 2026-11-10 — U1, U2 and U2b are in: the entropy bar is the
bottom edge of the field's grid, and the prompts teach it (Spend and Stars, leader lines, the ENTROPY
label). Next: the same square under the map (U3); the console bank moved to
[its own plan](../backlog/panel-bank.md).

**Related:** [core loop](../../design/core-loop.md) §The bar is the meter · [the cube](../../design/systems/cube.md) ·
[plans board](../README.md)

## Goal

The entropy bar used to float above the cube as a HUD, and the map spells each job's state out in
words. Make the field and its meter carry that feedback, in a job and on the map.

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

### U2 — teach the meter — done (2026-11-10)

- Prompt art re-rendered: `Tools/Our Space/Render Gesture Prompt Art` now draws the base square,
  the two motions and the fading star. Waits on the headset check below.
- **Two new prompts** in the gesture prompt panel (`GesturePrompts`), shown until learned like the
  others (`GestureLearning` cookies):
  - **Spend** — loop: the Scale motion with the base square under the cube shortening from the front as
    the hands pull. Line `> EACH PINCH SPENDS ENTROPY`. Shows in the first job that can fail on budget
    (`!noFail`) right after the first committed pinch, not on entry, so it never stacks with Scale.
    Learned when that job ends, if the prompt was shown.
  - **Stars** — loop: the square alone; the gap slides past the first tick and a star above it dims.
    Line `> SPEND LESS, EARN MORE STARS`. Shows on entry to the first graded job (`!noGrade`). Learned
    on the first graded solve.
  - `GestureKind` gains `Spend` and `Stars` (renaming it to `PromptKind` is optional cleanup, not
    required).
- **A leader line from each prompt to its subject.** A thin phosphor line (authored line renderer on
  the prompt prototype in `Game.unity`) from the panel's edge to what it teaches: Scale and Move → the
  cube, Enter → the map cube, Spend and Stars → the meter's front-centre. Exit has no subject and no
  line. The presenter passes the subject point per prompt; the line follows it while the prompt shows.
- **The meter's own label:** one small text at the base's front-centre, `ENTROPY`, switching to
  `OVER BUDGET` while a preview would overrun. No other states, no digits.
- Strings in the `Global` table, English and Spanish.
- **Exit:** a fresh save reaches job 2, pinches once and sees Spend pointing at the meter; job 3 shows
  Stars; neither shows again; every prompt's line lands on its subject.

### U2b — review fixes (review 2026-11-10) — done (2026-11-10, re-reviewed)

Do these before U3; they touch the same prompt code.

1. **Spend is learned without being seen.** `PuzzleEditionState.EndRun` (`Flow/PuzzleEditionState.cs`
   L117-120) reports Spend on every job end, shown or not. Spend only appears once the Scale prompt
   has left, so a job 2 solved by its first pinch ends before the presenter's `LateUpdate` shows
   Spend, and the cookie says learned. Report Spend at job end only if the presenter actually showed
   it: expose `SpendShown` from `GesturePromptPresenter` (set in `Show`, reset in `HideAll`, not a
   panel timer) and gate `EndRun` on it. A job that ends before the panel appears leaves Spend
   unlearned for the next job.
2. **Prompt offsets are world-axis.** `GesturePromptPresenter` (L28-29, L93-101) adds every offset in
   world space, but the field root is yawed: `SeatPlacement.Place` calls
   `origin.Setup(pose.Position, pose.Forward)` (`Features/Placement/Views/SeatPlacement.cs:102`) and
   `OVROriginBehaviour.Setup` sets `transform.rotation = LookRotation(forward)`
   (`_Modules/XR/Scripts/OVR/Origin/OVROriginBehaviour.cs:46-49`). Away from world +z the panels land
   beside or behind the meter. Express every offset in its owner's frame
   (`shapingView.transform.TransformVector` for the job prompts, `mapView.transform.TransformVector`
   for Enter); do it for Spend/Stars and the older prompts.
3. **Prompt kind count is hard-coded.** `GesturePromptPresenter.KindCount = 6` (L13): a seventh kind
   would silently stop following its subject. Derive it once from `GestureKind` (a static count, not
   `Enum.GetValues` per frame).
4. **The leader line is laid out twice a frame:** `GesturePromptPool.Move` → `Follow` →
   `LayoutLeader` (`GesturePromptPanel.cs:43-47`, `GesturePromptPool.cs:41-50`), and again in
   `GesturePromptPanel.LateUpdate` (L93). Keep the panel's `LateUpdate` as the one layout pass and make
   `Follow` set only the subject; the presenter already calls `Follow` every frame through `Move`.
5. *(Minor)* `FieldEdgeMeter.SetLabel` caches its state (L39, L105-111), so a language change mid-job
   keeps the old word until the over-budget state flips. Re-read on the localization
   `SelectedLocaleChanged` event; do not drop the cache, which would localize on every pinch frame.

- **Exit:** a job 2 solved in one pinch leaves Spend unlearned and shows it next time; with the seat
  turned 90°, every prompt (Spend, Stars, Scale, Move, Exit, Enter) keeps its offset in its owner's
  frame; one leader layout per frame.

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
