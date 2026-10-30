# First-minute read and cost

**Status:** next · space shaping · updated 2026-11-02 — the first job is a baked tutorial field, and the
in-field entropy bar is the read; stars stay score.

**Related:** [core loop](../../design/core-loop.md) · [the cube](../../design/systems/cube.md) ·
[jobs](../../design/jobs.md) · [field plan](../in-progress/space-shaping-realistic-field.md)

## 1. Why

The field pass is improving the look, but the risk is the read: can the player predict the pinch and
understand what it cost? The straight-line reference is not built; the first job's field is the lab's
9-planet + sun puzzle, not the tutorial; the derived budget hides the cost. Resize and more content
should wait.

## 2. Slices

### S1 — a clean first contact

The first job is a **viable field, baked normally** — no special case in the bake. Three planets, no
sun, one fixed zone: two make the run, the third blocks it, and the cube covers one. The lesson is the
pinch, not the planet count. The creator bakes it from a standard seed (`planetCount 3`, `fieldShape
Ring`, `hasSun` off, `translatable` off, `zones 1`); use `Find working seed` or `Generate`. Fix the
single-ring case (force ≥2 rings for the puzzle field) so a small field does not dome its rim. Verify
with `PuzzleVerifier`: the run misses at start and one pinch solves it.

- **The game teaches the pinch.** The tutorial shows the gesture itself: an authored visual cue (the
  cube's frame pulse/outline), with the manual/TTS line layered on once the read passes. A player who does
  nothing spends nothing and never fails ([core loop](../../design/core-loop.md) §2), so the game cannot
  wait for discovery.
- **The pinch is the inner volume.** The frame stays fixed; the inner volume and the content it covers
  scale with the hands, and the path preview follows. The fill follows the hand scale (the throb is only
  an accent), so the player sees the volume shrink or grow before release.
- **The budget stays derived.** Record the reference solution and set `entropyBudget = 3×` its entropy;
  "generous" means a cheap solution, not a hand-set budget.
- **No fail, no stars.** Job 1 cannot fail and does not grade: the bar shows spend and remaining, but the
  entropy fail and the star grade start at job 2.
- **The flags are data.** Add `noFail`/`noGrade` (negative so existing assets keep the rules) for the
  tutorial and carry them into `SpaceShapingSetup`/`PuzzleReadout`; `IsOverBudget` and the grade row
  respect them. The bar still uses the `3×` budget as its denominator.
- **The bar cannot overflow.** Job 1 cannot fail, so a player may spend past the derived budget; the fill
  clamps at empty and the label reports the overspend. The budget is a read here, not a gate.
- One idea in the frame; one pinch moves the path enough to arrive.
- **Exit:** a player with only the in-game cue makes one pinch and sees the path change without a second
  idea to read.

### S2 — the straight-line baseline

- The baseline is the **zero-field segment** origin→destination, drawn under the live trajectory, thin,
  dashed and desaturated. It is not the no-edit trajectory: the lab already draws that, and the field
  curves it. Show it from entry through shaping, not during the flight, and let it replace the initial
  pending line as the reference — do not show both.
- It is global to every job, not just job 1.
- **Exit:** before release, the player can compare the baseline and the bent trajectory.

### S3 — the in-field entropy bar

- The meter is an authored **3D bar in the field**, anchored **above** the run midpoint
  (`PuzzleData.zoneCentre`) and billboarded to the player, clear of the trajectory, the cube and the hands.
  It **drains** as entropy is spent. It is not a screen-space panel; the lab panel stays a diagnostic.
- The bar is **entropy only**: remaining budget, the committed spend, the pending pinch, and the last
  committed edit's cost. Job 1 shows no stars (S1); later jobs award them on a solve and show them in the
  reward/readout, never on the bar.
- **The bar's grammar is fixed.** Fill = remaining; a **pending marker** = the in-progress pinch; a
  **last-edit segment** = the cost of the last committed edit, drawn as a short segment at the committed
  end. The 1/3 and 2/3 **band marks** are static. One short label: the remaining budget; the band labels
  are the only other text. Do not add a fourth moving part.
- The **1/3 and 2/3** marks are **cost bands**, not star badges. The bar shows **remaining**: ≥2/3 left is
  cheap, 1/3–2/3 is medium, <1/3 is dear; label the marks so they cannot be read as "spent". The same
  numbers grade later runs, but the bar never displays a grade.
- Add the read model (last cost + zone). Simulate synchronously at commit so the flown path matches the
  final pose and the bar; the 50 ms preview throttle is for the live drag only.
- Do not reuse the lattice tint: `SpacetimeMeshView` already tints the lattice from the zone warp. A
  field-wide stress (rim or pulse) is optional and needs its own channel; the bar carries the read.
- Moving the cube is free, but jobs 1–2 are fixed; the free-move cue is out of scope (job 4+).
- **Exit:** after a pinch, the player can say what it cost and how much budget is left from the bar
  alone.

## 3. Validation

The same three seated hand-tracked testers, think-aloud, no VO. Run the slices in order and judge the
gate on the S3 build; an S1 or S2 failure is fixed before S3.

1. Before the first release, ask for the predicted outcome and why.
2. After release, ask what changed, what the pinch cost, and how much budget is left.
3. Ask which cost band the bar shows (cheap/medium/dear) — not a star.
4. On a solve, ask whether they want the next job.

Pass: all three predict the direction, at least two state the cost and the remaining budget, at least
two read the cost band, and at least two want the next job. Record verbatim; a missed item is the next
slice's fix. This is Validation 1 ([vision](../../design/vision.md) §7) on the current loop.

## 4. Out of scope

- Resize, multi-ship, the field look, more than the first two jobs, and the free-move cue (job 4+).

## 5. Risks

- **Tutorial bake conditions.** Three planets is the minimum for a no-sun blocked run: `TryPick` pairs
  endpoints and needs a third body. Two planets cannot bake. Force ≥2 rings for the puzzle field, or the
  three-planet ring domes its rim. The tutorial is a normal bake, so `Find working seed` works.
- **Bar vs zone tint.** The lattice tint is the zone warp today; a global entropy tint would fight it.
  Keep the bar as the read and give any field-wide stress its own channel.
- **Direction.** The bar shows remaining; the 1/3 and 2/3 marks are cost, not score.
- **Ordering.** The tutorial bakes like any job, so a field-scale change re-bakes it; freeze it after the
  S4 look pass settles the field's scale and the bar's marks.
- **Stale flight path.** Committing inside the 50 ms preview window would fly the previous tick;
  recompute at commit.
