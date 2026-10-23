# Space Shaping — Test Slice (Is It a Game?)

**Status:** in progress - T0 and T1 landed in the lab. The smallest change set that
tests whether space-time editing is a **game**, not a toy. Built in the **POC lab
scene**, not the game scene. Next: T2 (puzzles, prediction toggle, straight-line
reference).

**Related:** [project direction](../../design/project-direction.md) · [relativity as gameplay](../../design/relativity-as-gameplay.md) · [transport company](../../design/transport-company.md) · [POC](../done/space-shaping.md) · [architecture](../done/space-shaping-architecture.md) · [integration](../backlog/gameplay/space-shaping-integration.md)

**Wiring rule:** authored objects + serialized refs only; no runtime component
lookup, mesh, or asset creation. New readouts are authored objects in the lab scene.

## 1. Goal

Answer the one open design question cheaply: **is space-time editing a game, or
just a toy?**

The lab already proves the *feel* — moving a planet warps the mesh and bends the
path live. What it cannot answer is whether there is a **puzzle**, because there is
**no cost**. This slice adds the smallest possible cost (a ship clock + a deadline)
and the instrumentation to read the result.

## 2. Where: the POC scene, not the game scene

Build this in `SpaceShapingPOC.unity`, **not** by integrating into `Game.unity`.

- The editing mechanic is **presentation-independent**: whether it is fun to solve
  does not depend on the bubble, MR, or the flow. Test it flat first.
- Integration is the **largest and last** slice
  ([integration](../backlog/gameplay/space-shaping-integration.md)); its decisions
  (replacement vs mode, identity) depend on this test's result. Doing it first is
  backwards and delays the answer.
- The **domain types are shared** (`DigitalLove.Game.SpaceShaping`), so the slice's
  domain additions (ship time, deadline) carry over to the game with no rework.
- Run the test **seated, on device, with hand tracking** — the target posture — but
  in the lab scene. MR passthrough is not needed to judge the mechanic.

## 3. Non-goals

- No integration into `Game.unity`, the flow, or save/load.
- No bubble / MR / passthrough; no Map/Sculpt apertures.
- No sectors, multi-ship, network map, or level generation.
- No art, audio, or haptics polish.
- No transport-company wrapper (contracts, economy, progression).
- No move budget — the lab recomputes live with no commit step, so a move budget
  needs commit-on-release semantics. A deadline is the cheaper cost (§5).

## 4. Existing facts (what the lab already does)

- **Live editing:** `SpaceShapingState.LateUpdate` polls `SpaceShapingView.HasChanged()`
  and re-applies every frame (late, so the range clamp wins over the Meta grab);
  dragging a planet updates the mesh + path live.
- **Outcome:** `TrajectoryIntegrator.Classify` returns Arrive / Crash / Miss against
  the destination capture radius and planet radii.
- **Move range:** `SpaceSourceView.ShowRange` already shows each source's allowed
  range.
- **Field / prediction:** `SpacetimeMeshView` (deformed lattice) and
  `TrajectoryPredictionView` (path + outcome colour).
- **One puzzle:** `Puzzles/SamplePuzzle.asset`; one settings asset.

So "does it bend / can I reach it / does it feel good" is already testable today.

## 5. Scope (the smallest additions)

### T0 — trust and instrumentation (no design change)
1. **Determinism gate (DONE):** `TrajectoryDeterminismTests` asserts same-input
   identity, the empty-field straight line, and that the session path equals a direct
   integrator run (one integrator, one code path).
2. **Debug HUD (DONE):** authored `SpaceShaping Lab` group in `SpaceShapingPOC.unity`
   — a world `TextMeshPro` readout (`ShapingLabHud`) plus two grab buttons
   (`ShapingLabButton` on a `Grabbable`) for reset and puzzle cycle. The state polls
   the buttons and pushes a `ShapingReadout` snapshot.

### T1 — the cost (ship clock + deadline)
3. **Ship clock (DONE):** `TrajectoryIntegrator` accumulates proper time and carries
   it in `TrajectoryResult.ShipTime`. Depth-only:
   `dτ = dt · clamp(1 + kΦ · Φ(position))`.
4. **Deadline (DONE):** `PuzzleData.deadline`; `SpaceShapingSession.IsSolved` =
   Arrive **and** `shipTime ≤ deadline`.
5. **Readout (DONE):** the HUD shows outcome, ship-time vs deadline (red when over),
   solved (green), and edit count.

### T2 — content and the legibility probe (next)
6. **3–5 puzzles** of rising difficulty (planet count/placement) as assets.
7. **Prediction on/off toggle** for a "predict, then reveal" probe: ask the tester
   which way the ship will curve, then show the line.
8. Optional: faint **straight-line reference** (the empty-field path) as a baseline.

## 6. Phases

- **T0 (landed):** determinism gate + debug HUD.
  **Exit:** prediction == run (EditMode tests pass); HUD shows outcome, time, edits;
  reset/cycle wired. On-device pass still to do.
- **T1 (landed):** ship clock + deadline + readout.
  **Exit:** a puzzle can be failed by missing the deadline and passed by shaping the
  field. On-device pass still to do.
- **T2 (next):** puzzles + prediction toggle.
  **Exit:** 3–5 puzzles with a real difficulty spread; the predict-then-reveal probe
  works.

## 7. Test plan

Run seated, on device, hand-tracked, with a tester who has not read the docs.

1. **Solve rate** per puzzle.
2. **Time-to-solve and edit count** before success.
3. **Legibility:** predict-then-reveal accuracy (do they call the curve correctly?).
4. **Is the deadline binding?** Do they plan against it, or ignore it?
5. **Qualitative:** can they explain *why* their solution worked?

The decision gate (§8) weighs 3 and 5 as much as 1.

## 8. Decision gate

- **Pass:** testers solve the puzzles, call the curve roughly right, and explain why
  → proceed to integration and polish; the design bet holds.
- **Fail:** testers fiddle blindly, cannot predict, and cannot explain → revisit the
  mechanic (see [project direction](../../design/project-direction.md) §8 pivot
  ideas) before any integration.

## 9. Defaults

| Parameter | Default |
|---|---|
| scene | `SpaceShapingPOC.unity` (lab) |
| posture | seated, on device, hand tracking |
| dilation model | depth-only: `dτ = dt · clamp(1 + kΦ·Φ)` |
| `kΦ` | tuned so a deep dive visibly slows the clock |
| deadline | per puzzle, authored **below the straight-line par** so the naive path fails and a shaped path passes (see §12 A1) |
| puzzles | 3–5, rising difficulty |
| prediction toggle | on by default; off for the legibility probe |
| success | Arrive **and** `shipTime ≤ deadline` |

## 10. Risks

| Risk | Mitigation |
|---|---|
| Prediction lies (trust) | T0 determinism gate before any data. |
| Clock unreadable in a flat lab | Large authored readout; test depth-only first. |
| Live recompute vs commit | A deadline works with live editing (the clock is a function of the current path); no commit step needed. |
| Deadline too tight/loose | Author per puzzle; a too-loose deadline is a toy again. |
| Scope creep into integration | §3 non-goals; the gate must pass before integration. |

## 11. Open questions

1. **Deadline vs par:** a hard deadline (fail if late) is a crisp stake; a
   "minimise ship-time" score is more forgiving. Default: hard deadline for the test.
2. **Countdown or threshold line?** A visible countdown is stronger for a demo; a
   threshold line is less stressful. Default: show the number and the threshold.
3. **Dilation from depth only, or depth + speed?** Start depth-only (reads as
   "gravity slows time"); add speed only if the choice needs more tension.
4. **Is the clock per-launch or live per-edit?** Live per-edit (the cost of the
   current path) is simpler and matches the lab's live model.

## 12. Known issues and fixes (2026-10-23)

Open fixes and requirements for the working tree (ship clock, deadline, range rings).
**One owner:** the agent working in the feature applies the code, scene, and asset
changes. This section records the *requirements* only — do not have two agents edit
the same files. The plan (a doc) is conflict-free to edit while code work is in flight.

**A1 — the deadline must bind (blocking).** `stepSecs 0.008333333 × maxSteps 1200` = a
**10.0 s** coordinate-time ceiling, and `Dilation` clamps to `[0.05, 1]`
(`TrajectoryIntegrator.cs:37`) with `Potential ≤ 0`, so `shipTime ≤ coordinate time ≤ 10 s`.
With `deadline: 12`, `IsSolved` (`SpaceShapingSession.cs:31`) can never fail on time — it
equals `Arrive`. The fix is authoring, not code: set each puzzle's `deadline` **between the
best shaped time and the unshaped par** (`bestShaped < deadline < par`). The exact value
follows the dilation retune: with `dilationScale: 1` the clock is saturated and `shipTime` is
tiny (see §13), so pick the deadline only once a passing arrangement and a sane `kΦ` exist.
Time is beaten two ways, both "shaped": a gravity assist (less coordinate time) or skimming a
well (dilation → 0.05, clock runs slow, so `shipTime` drops even at equal coordinate time) —
crashing if you enter a source radius (`scale × 0.5`). Caveat: any deadline **above 10 s** also
needs a larger `maxSteps` (1800 → 15 s, 2400 → 20 s), else long paths truncate to `Miss`. Add a
regression check that a no-shape run yields `IsSolved == false`. The unit logic is already
covered by `SpaceShapingSolveTests`; what is missing is the authored puzzle value and a
puzzle-level check. This *is* the T1 exit criterion.

> **Status (2026-10-23, T0/T1 applied):** the *check* is done — `SpaceShapingSolveTests`
> locks `IsSolved` = Arrive **and** `shipTime ≤ deadline` (arrival inside/outside the
> deadline, and Miss), 6/6 EditMode tests pass. **Still open:** the deadline *value*.
> Measured in-editor on the sample puzzle: dilation is saturated (average factor ≈
> 0.05–0.13 because `dilationScale: 1` against these strengths), so `shipTime` runs
> ≈ 0.07 s (crash) … 1.26 s (full 10 s run) — `deadline: 12` still never binds. Pick the
> deadline below par only once a passing arrangement is known (see the tuning data below).

**A2 — domain purity (decision).** `DigitalLove.Game.SpaceShaping.asmdef` now references
`Oculus.Interaction`; Domain/Data/Views/Flow share one assembly, so the domain is no
longer isolated from the Meta SDK (architecture doc §12 forbids this). Either split a
pure `...SpaceShaping.Domain` assembly from the views, or accept and document. Lean:
extract an `ISourceSelection` interface (domain) + a thin `Grabbable` adapter
(interaction assembly) so only the adapter references `Oculus.Interaction`.
(Unchanged; the lab HUD added `Unity.TextMeshPro` to the same assembly — same decision.)

**A3 — no per-frame `SetActive` (minor).** `SpaceSourceView.Update()` toggles
`rangeSphere.SetActive(...)` every frame per planet. Subscribe to the `Grabbable`
selection event instead (pair `+=`/`-=` in `OnEnable`/`OnDisable`), or cache and toggle
only on change.

> **Done (2026-10-23):** `SpaceSourceView` caches `rangeShown` (seeded in `OnEnable`)
> and only calls `SetActive` on change.

**A4 — verify the ring's parent (minor).** `ShowRange` sets `rangeSphere.position`
(world) but `localScale` (parent-relative). If `RangeA`/`RangeB` are children of the
scaled planet, the radius is multiplied by the planet's scale; they should be siblings
under the `Space` container. Verify in the scene.

> **Verified (2026-10-23):** `RangeA`/`RangeB` are direct children of the `Space`
> container (siblings of `PlanetA`/`PlanetB`), so the radius is not multiplied by the
> planet's scale. No change needed.

**A5 — housekeeping.** `Assets/_Game/Features/Sectors.meta` (folder meta) is untracked
while the folder's files are tracked; folder metas must be version-controlled.

**A6 — edits stay unlimited (intentional).** `SpaceShapingState.editCount` is a HUD metric
only; nothing caps edits. Keep it that way — no edit limit, no "steps remaining". Unlimited
edits are safe because a change re-simulates once (≤ `maxSteps`), not every frame, and the
recompute is allocation-free. The stake that replaces a step cap is the deadline + fuel grade,
which is why A1 must bind: with an inert deadline, free unlimited edits collapse the puzzle to
"tweak until it lands". If undo is added later, bound the history depth, not the edit count.

**A7 — `PuzzleData.solutionMoves` is unused.** Only the declaration exists (`PuzzleData.cs:14`,
`= 1`); nothing reads it. It is the data slot for a "par" move count. Either wire it to a
readout or remove it — no dead fields.

## 13. T1 tuning data (2026-10-23)

Measured by running the domain types in-editor on the sample puzzle's **session**
positions (`PlanetA (-1, 0.2, 0.3)`, `PlanetB (1.5, -2.2, -1.3)`, scale 2.4, strength 2).
These are the positions the runtime uses (`SpaceSourceView.LocalPosition` is
container-local), not the `SamplePuzzle.asset` homes.

| Arrangement | Outcome | shipTime | coordinate |
|---|---|---|---|
| as authored | **Crash** | 0.069 | 1.38 s |
| both planets off-axis (±3.5) | Miss | 1.259 | 10 s (max) |
| one planet grazed (y 1.25–1.5) | Crash | 0.09–0.10 | ~1.8 s |
| coarse 2-planet grid (256 combos, both free) | 0 Arrive | — | best miss distance **0.653** |

Readings:

1. **The unshaped arrangement crashes** (the ship is deflected into a planet), so the
   sample puzzle requires shaping — fine, but it means "predict, then reveal" starts
   from a crash, not a near miss.
2. **`captureRadius: 0.6` is marginal.** The best of 256 coarse arrangements misses by
   0.653. The plan's own default is `1.5 × destination radius`; the destination sphere
   is radius 0.5, i.e. **0.75**. Raise the capture radius (≈ 0.75–1.0) before the
   deadline can be meaningfully authored.
3. **Dilation is saturated.** With `dilationScale: 1` and strength 2, `1 + kΦ·Φ` is
   below the 0.05 clamp almost everywhere, so the clock is uniformly slow and a "deep
   dive" does not read as slower. A smaller `kΦ` (≈ 0.2–0.3) gives factors ≈ 0.8–1.0
   with visible dips. This is the `kΦ` open question (§11.3) and polish item 5.
4. **The deadline is a coordinate-time limit today** (shipTime ≈ k · coordinate time),
   which is a workable but flat stake until (3) is tuned.

Until (2) and (3) are settled, the T1 exit ("failed by missing the deadline, passed by
shaping") cannot be demonstrated; the mechanism is in place and unit-tested.
**Settled — see §16.**

## 14. Lab HUD smoke test (2026-10-23)

Editor Play-mode pass on `SpaceShapingPOC.unity` (no headset; XR fails to init, which is
expected here). **No exceptions from the feature code**, and the state entered cleanly
(`StateMachine : Setting SpaceShapingState state`). The readout rendered live values:

```text
SamplePuzzle
Crash
ship 0,07s / 12,00s
edits 0
grab left: reset / right: next
```

`ship 0,07s` matches the measured crash run (0.069 s), so session → readout is correct
end to end. Two presentation defects were found and **fixed**:

1. **The readout was ~10× too large and wrapped.** `TextMeshPro` `bounds.size` came back
   `(10.9, 53.7)` local units at `lossyScale 0.1` → a ~5.4 m tall block, wrapping into
   13 lines (`lineCount: 13`, `isTextOverflowing: true`). Fixed: the readout's
   `localScale` is `0.1` and its `sizeDelta` is `(44, 22)` local units, so the block is
   ~0.42 m × 0.21 m world with no wrapping.
2. **Decimal separator followed the editor locale** (`0,07s`). Fixed: the readout is
   formatted with `FormattableString.Invariant`, so it reads `0.07s`.

Layout: the readout sits at local `(0, 4.5, 0)` under `SpaceShaping Lab`, with the two
buttons at local `(±3.2, 4.5, 0)` — left/right of the text, so the hint stays true.

The camera in the editor sits at the rig origin (floor), so the HUD is off-screen in a
default game-view capture; frame it with a capture position in front of the readout to
see it. The headset pass is the real check.

## 15. Lab controls → canvas, and puzzle 2 (2026-10-23)

**Decision:** replace the two 3D grab spheres with a **uGUI canvas panel**, matching the
game scene's existing pattern (verified: `Game.unity` has 8 canvases, 6 Meta
`PointableCanvas` components, plain `GraphicRaycaster`s, `UI.Button`s and TMP UGUI
labels; the `EventSystem` carries `PointableCanvasModule`). Reference prefab:
`Assets/_Modules/Casual/UI/ReviewPanel/ReviewPanel.prefab`.

**Done (code + data):**

- `ShapingLabButton` now holds a `UnityEngine.UI.Button` (subscribe `onClick` in
  `OnEnable`, unsubscribe in `OnDisable`, consume the click) instead of a `Grabbable` —
  which also removes the need for the grab/pinch gesture.
- `DigitalLove.Game.SpaceShaping.asmdef` gained `UnityEngine.UI`.
- `ShapingLabHud` dropped the hint line (the buttons carry their own labels).
- `Puzzles/SamplePuzzleB.asset` (guid `…28`): same origin/destination/capture as
  `SamplePuzzle`, different planet arrangement, homes authored in the **session frame**
  (container-local) because of the puzzle-vs-scene frame mismatch noted in §13. Starts as
  a clean **Miss** (`ship 1.139`, separation 6.21), so Next visibly swaps the arrangement
  and the readout name.

**Done (scene, 2026-10-23):** the lab now uses a canvas panel instead of grab spheres.

- `Lab Panel`: WorldSpace `Canvas` + `GraphicRaycaster` + Meta `PointableCanvas` +
  `ShapingLabHud`, at Lab-local `(0, 4.2, 4)` → world `(0, 1.42, 0.73)`, `localScale 0.01`
  → **0.32 m × 0.18 m**. Children: `Backdrop` (dark `Image`), `Readout` (TMP UGUI,
  fontSize 22, left/top), `Reset` / `Next` (`Image` + `Button` + `ShapingLabButton`,
  amber, 140×56) each with a TMP label.
- `EventSystem` + Meta `PointableCanvasModule` added at the scene root (the lab scene had
  none). Without these the canvas receives no Meta events.
- The old world-space readout and the two grab spheres are **deleted** (state rewired to
  the panel).
- `puzzles` = `[SamplePuzzle, SamplePuzzleB]`, so **Next** swaps the arrangement and the
  readout name.

Gotchas found while authoring (worth keeping):

1. **A uGUI canvas reads correctly from its local −z.** The player (rig at the origin,
   content at +z) looks along +z, so the panel stays at **rotation identity**; a 180° Y
   flip mirrors the text.
2. **A `Graphic` on the canvas root draws on top of its children.** A backdrop `Image` on
   the `Canvas` object hid both the readout and the buttons; the backdrop must be a child
   at **sibling index 0** (set from an editor script, since the MCP tools have no
   sibling-order action).
3. **Poke is unproven.** Neither the game scene nor `ReviewPanel.prefab` uses
   `PointableCanvasMesh`, so canvas interaction there is ray-based. If hand poke does not
   press the buttons in the headset, add `PointableCanvasMesh` (it generates the canvas
   collider) or drive the buttons from the XRI raycaster.
   **Resolved:** the user added the Interaction SDK components for the canvas and the
   buttons now work in the headset.
4. `SamplePuzzleB` homes are authored in the **session (container-local) frame**; puzzle A
   still uses the arrangement captured at `Enter` because of the frame mismatch in §13.

## 16. T1 tuning applied (2026-10-23)

**Values applied**

- `SpaceShapingSettings.dilationScale` `1` → **`0.2`** (average factor ≈ 0.67–0.88 on
  arriving paths — visible slowing, no saturation; the plan's guess was 0.2–0.3).
- `SamplePuzzle` / `SamplePuzzleB`: `captureRadius` `0.6` → **`1.2`**, `deadline` `12` →
  **`3.9`**.
- `SamplePuzzleB` sources re-homed to `(0.5, 2.7, 0.3)` / `(-0.5, -3.7, -1.3)` so its move
  range contains a known solving basin.

**Measurements** — coarse 2-planet grid (1296 arrangements, z fixed at the home plane,
`dilationScale 0.2`):

| capture | arrivals | min | p25 | median | p75 | max shipTime |
|---|---|---|---|---|---|---|
| 0.6 | 6 (0.5 %) | — | — | — | — | — |
| 1.0 | 21 (1.6 %) | 2.92 | 3.48 | 3.75 | 4.09 | 4.52 |
| **1.2** | **32 (2.5 %)** | 2.76 | 3.27 | **3.64** | **4.03** | 4.24 |
| 1.5 | 53 (4.1 %) | 2.56 | 3.20 | 3.50 | 3.81 | 4.23 |

`deadline 3.9` sits between the median and p75 at capture 1.2, so arriving is doable and
arriving *fast* is the puzzle. The wells both accelerate the ship (arriving coordinate
times 4.9–6.4 s against a 7.4 s straight line) and slow the clock, so the two effects
reinforce the mechanic.

**Verified with the real assets** (loaded in-editor, settings + puzzle data):

| case | result |
|---|---|
| A live start (scene arrangement) | `Crash 0.937 / 3.90` → not solved |
| A shaped | `Arrive 3.171 / 3.90` → **solved** |
| B asset start | `Arrive 4.065 / 3.90` → not solved (close but over) |
| B shaped | `Arrive 3.171 / 3.90` → **solved** |

So T1's exit criterion is met — a puzzle can be failed by missing the deadline and passed
by shaping the field — and difficulty rises A → B (crash → over-deadline). 6/6 EditMode
tests still pass, console clean.

Caveats: the grid is a **2D slice** of the 3D move space, so the real basin is larger than
2.5 %; confirm on device that `deadline 3.9` is not knife-edge. The §13 frame mismatch is
untouched (puzzle A still starts from the arrangement captured at `Enter`).
**Superseded — see §17** (the frame mismatch is fixed and puzzle homes are authoritative).

## 17. Start-point alignment, planet grab volumes, puzzle set (2026-10-23)

**Fixes from a headset pass**

1. **The predicted path started below the Origin sphere.** The prediction line renders in
   the `SpaceShaping POC` frame, which sat at container-local `(0, -1, -0.5)` while the
   field, sources and `Origin` live in the container frame — a 1-unit (0.1 m) offset.
   Fixed by moving `SpaceShaping POC` to `(0, 0, 0)` and `Destination` from
   `(4, -1, -0.5)` to `(4, 0, 0)`, so line, field and markers share one frame. This closes
   the §13 frame mismatch in the lab scene.
2. **Planet colliders bigger than the planets.** Each planet carried an unreferenced
   trigger `SphereCollider` at radius `1.0` — 2× the visible sphere (radius `0.5`) — so
   grabs/pokes registered well off the surface. Both shrunk to `0.5`.

**Puzzle homes are now authoritative.** `SpaceShapingState.StartPuzzle` used to pull the
view's current positions over the new puzzle and capture them as homes, so `Next` never
applied a puzzle's arrangement (`CaptureHomes` existed only for that). `StartPuzzle` now
builds the session and pushes it, so `sources[].home` defines the start; `CaptureHomes` is
deleted. `SamplePuzzle`'s homes were set to the arrangement the lab had been running with
(`(-1, 0.2, 0.3)` / `(1.5, -2.2, -1.3)`), so puzzle A is visually unchanged.

**Puzzle set — 4, rising difficulty.** Homes are in the session (container-local) frame.
Verified in-editor against the real assets: every start fails, and a solving arrangement
exists inside the move range.

| puzzle | id | deadline | start | best in range | solvable |
|---|---|---|---|---|---|
| SamplePuzzle | puzzle_01 | 3.9 | Crash 0.937 | 2.797 | yes |
| SamplePuzzleB | puzzle_02 | 3.9 | Arrive 4.065 | 2.952 | yes |
| SamplePuzzleC | puzzle_03 | 3.5 | Arrive 3.762 | 2.948 | yes |
| SamplePuzzleD | puzzle_04 | 3.3 | Crash 1.757 | 2.756 | yes |

Progression: learn to arrive (A), learn to speed up (B), tighter (C), tightest (D).
6/6 EditMode tests pass.

Open: the **ship-time unit** question raised in the headset pass (are "seconds" the right
readout?); 3-planet puzzles need a third authored source view, because
`SpaceShapingView` clamps to `min(view sources, session sources)`.
