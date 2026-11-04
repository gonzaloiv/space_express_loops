# Space Shaping — the scale ghost

**Status:** later · space shaping · updated 2026-11-02 — one scale ladder shared by the bake and the
pinch, and a ghost that shows the value the hands are about to commit.

**Related:** [core loop](../../design/core-loop.md) · [the cube](../../design/systems/cube.md) ·
[jobs](../../design/jobs.md)

## 1. Why

The pinch is continuous but the value is not: the bake searches 32 discrete scales
(`PuzzleZoneCandidates.DefaultSamples`, Lerp'd 0.6–1.8 and sorted near 1), while the player commits any
float and entropy charges the raw delta (`ZoneCommandLog.cs:34`). The player cannot count the cost, and a
baked solution can sit between the values the hand lands on. The fix is one **scale ladder** shared by
authoring and play, and a **ghost** that shows the value the hands are about to commit. The drag stays
continuous; only the committed value is discrete.

## 2. Slices

### S1 — one ladder

- Put the ladder in `SpaceShapingSettings` (points, not a range): ~10–12 geometric points across 0.6–1.8
  with more resolution near 1.0. Do not use a linear 5/5 split — the range is 0.4 down and 0.8 up, so
  even steps make the up side twice as coarse.
- `PuzzleZoneCandidates.Scales` returns the ladder instead of `DefaultSamples` Lineps; the bake's
  solutions are then exactly reachable by the hand.
- Re-bake the job set; if 10–12 drops routes or the capstone's ≥2, raise the count. The ladder is a
  tuning knob, not a constant.
- **Exit:** every baked solution's scale is a ladder point; the existing bake tests pass; the 8-job set
  bakes (or the count is tuned to it).

### S2 — the ghost value

- The solid inner volume follows the hands continuously; an **authored ghost volume** (serialized
  renderer/prefab on the cube, no runtime hierarchy, no collider) shows the **nearest ladder value**.
  Reuse the project's ghost pattern (`UniverseGhostView`/`UniverseGhostSession`, the scale ghost in
  `BubbleScaleSession`), not a new runtime object.
- The ghost eases toward the detent (a soft magnet/spring), so the gap between hand and ghost is the
  rubber band; the hand itself never notches.
- On release the solid takes the ghost's scale and the commit charges that value.
- **One truth:** the live trajectory and the bar's pending marker follow the **ghost**, not the raw hand;
  the hand only pulls the ghost.
- Style: translucent/wireframe and low-contrast; it is a value preview, not a second body — the removed
  sun rings failed by reading as another object.
- **Exit:** before release the player sees the exact value and path; the hand/ghost gap reads as a rubber
  band in the headset.

### S3 — cost and the first pinch

- The pending cost is |ghost step − last committed step|; the bar's pending marker uses that value.
- Decide whether entropy charges **per step** (an integer) or the raw ladder delta. Per step makes the
  3× budget and the 1/3–2/3 bands exact; the raw delta keeps the current model but geometric steps cost
  different amounts.
- The tutorial's first pinch uses the ghost as the teaching cue; the ghost and the bar's pending marker
  are the same value.
- **Exit:** after a pinch the player can state the cost and the remaining budget from the ghost and the
  bar alone.

## 3. Tests

- The ladder is sorted by distance from 1 (cheap first), bounded by 0.6 and 1.8, with no duplicate points.
- A bake's solution scale is on the ladder; the verifier's entropy matches the commit.
- A hand value between points commits to the nearest point; the cost is the step delta.
- The trajectory and readout pending use the ghost scale, not the raw hand.
- The ghost is a serialized authored object (no runtime creation).

## 4. Out of scope

- Snapping the drag itself — the hand stays continuous.
- Moving or resizing the cube; this is the scale only.
- Reworking the multi-zone beam search beyond sampling the ladder.

## 5. Risks

- **Ladder too coarse.** Fewer scales can drop routes and plateaus; re-bake and tune the count before the
  first-minute pass.
- **Two truths.** Ghost, solid and bar can disagree; the ghost must be the single value that the
  trajectory, the bar and the commit use.
- **Cost model.** Per-step entropy changes the budget derivation; decide it before the bar is built.
- **Read.** A translucent ghost near the planets can clutter; keep it low-contrast, depth-soft, and out
  of the committed value's silhouette.
- **Principle.** [The cube](../../design/systems/cube.md) says the verb is continuous; this refines it —
  the drag is continuous, the committed value is discrete.
