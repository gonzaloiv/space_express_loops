# First-minute read and cost

**Status:** now · space shaping · updated 2026-11-02 — S1–S3 are implemented (tutorial rebaked, baseline,
  unified colour set, in-field bar); run Validation 1.

**Related:** [core loop](../../design/core-loop.md) · [the cube](../../design/systems/cube.md) ·
[jobs](../../design/jobs.md)

## 1. Why

The field pass is improving the look, but the risk is the read: can the player predict the pinch and
understand what it cost? The straight-line reference is not built; the first job's field is the lab's
9-planet + sun puzzle, not the tutorial; the derived budget hides the cost. Resize and more content
should wait.

## 2. Slices

All slices are read/presentation work: S1 the tutorial field, the cube fill and the cue, S2 the baseline,
S2.5 fewer colours, S3 the bar. The baseline and the bar are authored objects (no runtime creation); the
fill and cue are existing view surfaces.

### S1 — a clean first contact

The first job is a **viable field, baked normally** — no special case in the bake. Three planets, no
sun, one fixed zone: two make the run, the third blocks it, and the cube covers the **blocker** — the only
non-endpoint body a zone may cover (`PuzzleVerifier` rejects a zone that influences an endpoint). The
lesson is the pinch, not the planet count. The creator bakes it from a standard
seed (`planetCount 3`, `fieldShape Ring`, `hasSun` off, `translatable` off, `zones 1`); use `Find working
seed` or `Generate`. The field already forces at least two rings, so the three-planet field does not dome
its rim. Verify with `PuzzleVerifier`: the run misses at start and one pinch solves it.

- **Job 1 is a curve.** The third planet is why the start fails; the cube covers the blocker and **grows**
  it, so its well bends the path around the body. The obstacle lesson is the *shrink* of the same body, a
  later variation, not the tutorial; request `Curve` once the
  [mechanism check](../backlog/puzzle-solution-mechanism.md) lands.
- **Compact the three-planet field.** Keep the sector radius global (`runDistance`); only `spread`
  follows the count — about 0.6 for three planets, 0.7 for six or more — so the tutorial does not read as
  a sparse rim. The run target and planet sizes stay global, and `Build_SpansTheRun`'s outer bounds must
  still hold.

- **The game teaches the pinch.** The tutorial shows the gesture itself: the fill scales with the hands and
  the frame takes its active colour, with the manual/TTS line layered on once the read passes. A player who
  does nothing spends nothing and never fails ([core loop](../../design/core-loop.md) §2), so the game
  cannot wait for discovery.
- **The pinch is the inner volume.** The frame stays fixed; the inner volume and the content it covers
  scale with the hands, and the path preview follows. The fill follows the hand scale (the throb is only
  an accent), so the player sees the volume shrink or grow before release. On release the inner volume
  returns to its base size; the committed scale lives on in the field.
- **Planet labels sit above the body.** `PlanetNameLabel.Place` anchors the label above the planet on
  its local Y — the axis the body spins around — so the label stays put instead of orbiting. Keep it
  clear of the endpoint markers; a short leader is a later polish.
- **Colour the endpoints.** Tint the origin and destination planet bodies with distinct colours, not only
  the marker prefabs; reset the tint when a pooled body is reused.
- **Both trajectory caps clear the surface.** The launch clip uses `LaunchPoint`/`ClipToLaunch` with a
  fixed 0.02 m gap, and the arrival ends at the capture point; in the headset the ship starts and finishes
  inside the bodies. Offset both ends by the surface plus a ship-sized clearance, and apply the same
  clipped points to the line and the flight.
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
  desaturated and tiled so it reads as a dashed reference. It is not the no-edit trajectory: the lab
  already draws that, and the field curves it. Show it from entry through shaping, not during the flight,
  and let it replace the initial pending line as the reference — do not show both.
- It is global to every job, not just job 1.
- **Exit:** before release, the player can compare the baseline and the bent trajectory.

### S2.5 — fewer colours, direct ColorValue

- Keep the direct `ColorValue` references on the views; do not add a palette asset. The small runtime
  `ZonePalette` stays as the internal mapping.
- Make the set consistent: every feature colour is a `ColorValue`, including the spawner's origin and
  destination (currently plain `Color`). The sun is outside the set today: it is material-owned
  (`Sun.mat`); add a `sun` `ColorValue` only if the sun body should follow.
- Remove the feature's `neutralHover`, `slow`, `fast` and `miss` fields: hover uses `accent`; the scale
  gradient runs `neutral` -> `accent`; a miss uses `neutral`; danger stays for crash/refusal.
- Keep the shared `ColorValue` assets and `_Modules/Theming`; do not delete the module colours.
- **Exit:** no plain `Color` fields in the feature; the remaining set is neutral, accent, success,
  danger, origin, destination, plus `sun` only if chosen.

### S3 — the in-field entropy bar

- The meter is an authored **world-space canvas** in the field, anchored **above** the run midpoint
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
- The obstacle/clear lesson and the mixed clear+curve multi-zone job (later).

## 5. Risks

- **Tutorial bake conditions.** Three planets is the minimum for a no-sun blocked run: `TryPick` pairs
  endpoints and needs a third body. Two planets cannot bake. The field already forces at least two rings,
  so the tutorial does not dome its rim. Compact the small field through `spread`, not a smaller sector
  radius; the sector radius stays derived from `runDistance`. The tutorial is a normal bake, so `Find
  working seed` works.
- **Bar vs zone tint.** The lattice tint is the zone warp today; a global entropy tint would fight it.
  Keep the bar as the read and give any field-wide stress its own channel.
- **Direction.** The bar shows remaining; the 1/3 and 2/3 marks are cost, not score.
- **Ordering.** The tutorial bakes like any job, so a field-scale change re-bakes it; freeze it after the
  S4 look pass settles the field's scale and the bar's marks.
- **Stale flight path.** Committing inside the 50 ms preview window would fly the previous tick;
  recompute at commit.
