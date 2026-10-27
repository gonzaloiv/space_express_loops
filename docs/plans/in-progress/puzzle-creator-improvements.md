# Space Shaping — Puzzle Creator: the brief

**Status:** now · space shaping · updated 2026-10-27 — the **brief is complete**: conditions, the
condition-aware search, the derived run window, one best level, and the **runtime gesture locks**. The
next action is **Validation 1**. The constants, cube, density, budget and verifier have landed; the
physics moved to [its own plan](../backlog/gameplay/space-shaping-physics.md).

**Related:** [physics](../backlog/gameplay/space-shaping-physics.md) ·
[tech debt](../backlog/gameplay/space-shaping-tech-debt.md) · [cube](../../design/systems/cube.md) ·
[progression](../../design/systems/progression.md) §4 · [jobs](../../design/jobs.md) ·
[vision](../../design/vision.md) §3, §4, §7

## Goal

The creator bakes a solvable puzzle from a seed. The **brief** — what a solution **must** do (the
conditions) and how **good** it is (the quality) — is authored on the seed and honoured by the bake.
What remains is **runtime**: carry the conditions onto the level so the cube locks the gestures the
level does not allow. One seed in, one **level** out.

## Landed (2026-10-27)

- **Constants** — run, cube and planet-radius live in `SpaceShapingSettings`; the run and the cube are
  constants, the cube a fixed sub-region.
- **Cube** — spawned per puzzle from the prefab; the region comes from `PuzzleData`.
- **Density** — the field spans ~1.4× the run; the endpoints are a **planet pair** picked by separation +
  clearance; the run is the pair's direction; the destination is excluded from the crash test.
- **Budget** — `entropyBudget` is derived from the solution's cost (`Σ|Δscale| × 3`).
- **Verifier** — `StartFails` now requires `Miss`.
- **Brief (bake side)** — the seed authors `translatable`, `moves`, `resizable`, `rotate` and
  `windowTightness`; the bake carries them onto `PuzzleData`, searches the brief, and rejects the
  unsupported bands (`moves ≥ 2`, rotate) instead of baking a one-zone level.
- **Derived window** — `Tcross = puzzle.Distance / nominalSpeed`; `[tmin, tmax] = Tcross × [1 − t, 1 + t]`.
  The bake verifies reach: the bare start outside, the solution inside.
- **One best level per seed** — ranked by entropy spent, then by closeness to the window centre;
  `startMargin` is reported in the creator readout.
- **Runtime locks** — the baked conditions reach the cube as `ZoneConditions`; a fixed level refuses the
  one-hand move, a locked frame refuses the two-hand resize, and the scale inside the cube always works.
  No move count is shown.

## The brief (the slice)

- **Conditions (must).** Per level — the vocabulary lives in
  [progression](../../design/systems/progression.md) §4:
  | Condition | Meaning |
  |---|---|
  | `translatable` | the **cube may be moved** (its centre changes) or is fixed at home |
  | `moves` | how many **zones** the solution scales — the cube edits `N` places, up or down |
  | `resizable` | the frame may be **resized** (`[1×, 2×]`) or is locked |
  | `rotate` | the spin gesture is available (later puzzles) |
  | `scalable` | **always true** — scaling the content is the verb; there is no locked-scale level |

- **Placement & size.** **Non-translatable** levels bake the cube's position **and** size; **translatable**
  levels let the player move it. The size is an **authored range** the creator picks within. The **PoC**
  tests the translatable case; the **game's first jobs are fixed**.
- **Quality (how good).** The **three that decide pass/fail**: **window tightness**, **entropy spent**,
  **start margin**. Entropy spent and closeness to the window centre rank the baked level; the start
  margin is the field's readout. **Authoring-side only**, so the player's score stays entropy left /
  stars.

**The brief per band** (from [progression](../../design/systems/progression.md) §4):

| Band | translatable | moves | resizable | rotate |
|---|---|---|---|---|
| jobs 1–2 (the scale) | no | 1 | no | no |
| job 3 (several zones) | no | 2+ | no | no |
| time tutorial (off-path planet) | no | 1 | no | no |
| job 4 (move the cube) | **yes** | 1+ | no | no |
| job 5 (resize the frame) | yes | 1+ | **yes** | no |
| jobs 6–8 (combine) | yes | 2+ | yes | no |
| later (rotate) | yes | 1+ | yes | **yes** |

**This slice ships the `moves = 1` bands** (jobs 1–2, 4, 5, time). The `moves = 2+` bands (job 3, jobs
6–8) need the **multi-zone search**, which is a candidate below.

**How it lands:**

- **The seed defines the job; the bake makes the level.** The seed carries the **generation** (`seed`,
  `distance`, `planetCount`, `corridorClearance`), the **brief** (`translatable`, `moves`, `resizable`,
  `rotate`, `minCubeSize`/`maxCubeSize`) and the **quality dial** (`windowTightness`). The remaining
  **number** — the budget — is **derived** by the creator, so the seed stays a small, hand-editable
  recipe.
- **Search — `moves = 1` only.** Fixed levels search **cube size × scale** at the home centre; the chosen
  solution's centre is baked as the fixed position. Translatable levels search the **centre** too
  (sources + close pairs, nearest the run first), coarse then fine, and keep the run midpoint as the
  cube's home. `PuzzleFieldBake` rejects the unsupported bands (`moves ≥ 2`, rotate).
- **The window is derived.** `Tcross = puzzle.Distance / nominalSpeed` — the **actual** run, so the window
  and the budget use the same number; `windowTightness` is the band's half-width as a fraction, so
  `[Tmin, Tmax] = Tcross × [1 − t, 1 + t]`. The bake verifies reach: the start outside, the solution
  inside.
- **One best level per seed** — ranked by **entropy spent**, then by closeness to the window centre; the
  start margin is reported. The cross-seed library is a candidate.
- **Runtime enforces.** The runtime locks the disallowed gestures; **no move count is shown**
  ([vision](../../design/vision.md) §8).

**Exit (met 2026-10-27):** the brief is authored on the seed; the search honours the conditions on a
`moves = 1` grid and the bake rejects breaks; the window is derived and reach-verified; one best level
per seed; the runtime locks the disallowed gestures.

## Candidates (unscheduled)

- **Multi-zone (`moves ≥ 2`).** The search over several zones; the soft brush blends them.
- **Rotate.** The search grid adds rotation; frame dragging.
- **Scored seed library.** The cross-seed half of the ranking: score variants, keep a ranked set.
- **Batch / CLI bake** over a seed list, for reproducibility and CI.
- **`PuzzleBrief` as a real asset** — conditions + quality as one authored object.
- **Creator preview.** Corridor, cube, window band, start vs solution, ship-time scrub.
- **Per-job launch lore.**
- **Window-edge robustness.**

## Out of scope

- **Runtime generation** — the field is baked, never generated at load.
- **The selection field / two-hand scale-out** — [integration](../backlog/gameplay/space-shaping-integration.md).
- **The validation** ([vision](../../design/vision.md) §7) — the creator stays offline/authoring.

## Decisions (closed 2026-10-27)

- **Cube authority** — `cubeSize = 0.25 m`, `zoneFalloff = 0.25`, `stepSecs = 1/1200`,
  `BareMissMargin = 0.9`; 90/90 EditMode green. The size becomes an authored range.
- **Conditions** — translatable / moves / resizable / rotate; scalable always true (progression §4).
- **Quality** — the pass/fail three, authoring-side only.
- **Corridor** — separation + clearance, no field rotation (landed).
- **Budget** — a crossing time `puzzle.Distance / nominalSpeed`; the band `Tcross × [1 − t, 1 + t]`.
- **Endpoints** — a planet pair at least `minRunDistance` apart; the run is the endpoint distance.
- **Resample** — a fixed cap.
- **Search** — adaptive; `moves` authoring-only.
- **Variants** — one best per seed.
- **Brief storage** — authored on the seed, resolved onto `PuzzleData`.
- **Runtime** — lock the disallowed gestures.

## Carried fixes

See [tech debt](../backlog/gameplay/space-shaping-tech-debt.md).
