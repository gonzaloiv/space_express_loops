# Space Shaping — Puzzle Creator Improvements (constants, cube, density, conditions)

**Status:** now · space shaping · updated 2026-10-27 — the creator follow-ons, in order: pin the
**working constants** (run `1 m`, cube `0.25 m`), **spawn the cube** per puzzle (current cube as prefab,
one instance), fill the run (**density / run corridor**), define the **brief (conditions + quality)**,
and make the **physics** physical (two-term clock + `mass ∝ radius³`). Supersedes the retired
puzzle-creator plan: its mechanics (P0–P4) are done and test-green.

**Related:** [cube](../../design/systems/cube.md) ·
[progression](../../design/systems/progression.md) §4 ·
[vision](../../design/vision.md) §3, §4, §7 · [variations](../backlog/gameplay/space-shaping-future.md) ·
[integration](../backlog/gameplay/space-shaping-integration.md)

**Wiring rule:** the creator stays **Editor/offline**; the search/verifier/bake stay pure domain code in
the runtime assembly so EditMode tests reach them; the field is **never generated at load**; spawned
objects come from a **serialized prefab** and are pooled/instantiated, never invented.

## 0. Context — what the creator already is

Built, Editor/offline, pure domain, EditMode-tested:

- **Pipeline (one exported artifact per stage):** `PuzzleSeed` → `PuzzleShapeAsset` (a built
  `SectorShapeData`) → `PuzzlePlanetListAsset` (`PuzzleFieldPlanet[]`, via `SectorShape.Generate`) →
  the `PuzzleFieldPlanet → PuzzleSource` bridge → `PuzzleSolutionSearch` → baked `PuzzleData`
  (`PuzzleFieldBake.BakeVariants`).
- **Contract:** a seed in, a solvable puzzle out — the bare start **near-misses** (`Miss`, not a crash),
  the authored solution lands **inside the window** and **inside the budget**.
- **Runtime:** `PuzzleData` carries `distance`, `launchSpeed`, `tmin`/`tmax`, `entropyBudget`,
  `sources`, `solution`; `origin`/`destination` derive from `distance` (±x); the field is **baked, never
  generated at load**; the game never references the creator.
- **Placement:** game `DigitalLove.Game.SpaceShaping`; authoring `…SpaceShaping.Creator` (pure); tools
  `…Creator.Editor`; tests in both `…Tests.EditMode` assemblies.
- **v1 as-is:** **scale-only**, one zone, the run **derived** from the field's outer edge
  (`PuzzleFieldBake.RunLength`), and the cube = the **whole run** (`PuzzleData.ZoneSize = distance³`).

## 1. Goal

Five follow-ons, in order:

1. **Constants** — pin the numbers: run `distance = 1 m` (not editable for now), cube `0.25 m` at the centre.
2. **Spawn the cube** — from the current cube as a prefab, one instance, instantiated on puzzle launch.
3. **Density** — the field **fills** the run, with the endpoints **inside** it and a clear run corridor.
4. **Conditions + quality** — the per-level **brief** ([progression](../../design/systems/progression.md) §4); the creator's PoC tests the **translatable** puzzle, the game's first jobs are **fixed**.
5. **Physics** — the two-term clock and `mass ∝ radius³` (§6), so the sim the creator searches is the model the game plays.

All still authored offline and proven by the same search-and-test.

## 2. Slice 1 — constants

**Landed (2026-10-27):** `runDistance`, `cubeSize` and `planetRadius` live in `SpaceShapingSettings`
(with tooltips); the run and the cube are constants, `launchSpeed` is gone, and the arrival sphere is
the planet radius.

Today the run is **derived** (`PuzzleFieldBake.RunLength` from the field's outer edge) and the cube is
the **whole run** (`ZoneSize = distance³`). Pin both and stop deriving:

- **`runDistance = 1 m`** — the run; `origin`/`destination` at ±0.5 m. It lives in
  `SpaceShapingSettings` (editable there); `PuzzleSeed.distance` is kept but **not editable** — it is no
  longer the editing surface.
- **The cube = `0.25 m`** at the field's centre — a fixed **sub-region** (≈27 % of the run), not the
  whole run. This is what makes a solution a **local shortcut** instead of a global zoom
  ([cube](../../design/systems/cube.md): "a shrunk zone is a shortcut"). Its size becomes an **authored
  range** the creator picks within (Slice 4).
- The cube's **content inset stays a fraction** (`contentInsetFraction = 0.05`), so the content region
  scales with the cube (0.25 m → ≈0.2375 m). Nothing absolute.
- **The planet radius is the arrival sphere** — `planetRadius` sets the field planets (with a small
  spread) **and** the destination's capture sphere, so touching the goal planet is an arrival. There is
  no separate `captureRadius`; the player sees the goal.
- Use the **same** region in the creator's search and on puzzle start; the cube is spawned in Slice 2.
- The **falloff is already unified** (one source: `SpaceShapingSettings.zoneFalloff`); in Slice 2 the
  cube prefab **references the settings** and pulls it on spawn, so there is no second value to keep in
  step.

**Why:** a constant is a stable base to design against. **Per-level** values are a
[progression](../../design/systems/progression.md) decision, not a creator constant.
**Authority is untested** — start with 0.25 m and test whether a single cube bends the run enough.

**Exit:** `runDistance`, `cubeSize` and `planetRadius` are the only knobs; the run and the cube are not
derived; the creator's search and the puzzle start use the same region.

## 3. Slice 2 — spawn the cube per puzzle

Today the cube is a **scene object** that `SpaceShapingState.ResetView` resizes in place. Make it
**spawned per puzzle**, so a puzzle owns its region and the level-selector transition has somewhere to
come from.

- **Convert the current scene cube into the prefab** (the designer edits it later): the "Zone Cube"
  (`Transform` + `MeshRenderer` + `SpaceZoneView`) and its grab handle (`Grabbable` +
  `ZoneGrabListener`). The grab uses the **Oculus Interaction SDK** (`Grabbable` / `PointerEvent`),
  already in the scene — not XRIT.
- **The cube prefab reads its region on spawn** — the falloff from `SpaceShapingSettings`, the region
  from the puzzle (per-puzzle from Slice 4) — so there is no baked duplicate to keep in step.
- **Serialized prefab reference; instantiate when the puzzle launches.** One instance now; the design
  allows **more** later if a puzzle varies its cubes.
- **Per-puzzle region** (centre + size + falloff) lives on `PuzzleData`, baked by the creator; the search
  stays pure data, only the view spawns.
- Re-cache/sync the grab handle after a frame change (`ZoneGrabListener` caches its rest size at gesture
  start).

**Risk:** too small a cube (or one the ship's path misses) → "no scale arrives"; reject and resample.

**Exit:** the cube is spawned per puzzle from the prefab; its region comes from `PuzzleData`; no scene
cube remains.

## 4. Slice 3 — density / the run corridor

With the run pinned at 1 m, the field must **fill** it. Today the cluster is sized to fit *inside* the
run (`PuzzleFieldBuilder.SectorRadius`), so the endpoints land **outside** the planets. Target: a field
**full of planets**, with `origin`/`destination` **inside** it, and the path crossing the field.

- **Fill the run.** The field spans the run; the endpoints sit **inside** the field.
- **Endpoints are planets.** `origin`/`destination` are **planets in the field** (not bare ±x points),
  picked as a pair at least `minRunDistance` apart (a seed/creator rule). The **run is the endpoint
  distance**, with the `1 m` constant as the **target** the creator aims for. The **launch is derived
  from the valid solution** so the bare start near-misses.
- **The pair is chosen by clearance, so the corridor is inherent.** Score candidate pairs by
  (a) separation close to the target run and (b) the segment's **clearance** from every other planet;
  take the best pair, and reject the seed if none clears the threshold. **No field rotation** — the run
  direction is simply the pair's.
- **The run is not axis-locked.** It is whatever direction the pair gives (better design variation), so
  `PuzzleData` carries the **endpoint positions** (not `±x`) and the **cube's centre follows the run**
  (baked per puzzle) — otherwise it sits off the path and never engages.
- **The corridor clearance is a seed field** — the progression definition lives on the seed, and the
  creator's result is the final level. The verifier already requires the start to be a `Miss`; the
  clearance keeps it a near-miss at higher density.
- **The destination is a planet, so exclude it from the crash test.** `TrajectoryIntegrator.Classify`
  already excludes the source at the launch point; it must exclude the one at the **destination** too,
  or arriving registers as a `Crash`.

**Risks:** a dense field can leave **no scale arriving** (the search finds no window) → reject and
resample (`PuzzleSeedScanner`) after a **fixed cap** of tries; the corridor must be wide enough for the
launch, narrow enough to read dense.

**Exit:** the field fills the run with the endpoints **inside** it; the run passes through its widest
gap; the endpoints are a planet pair at least `minRunDistance` apart.

## 5. Slice 4 — the brief: conditions + quality

The **brief** is the per-level contract on a solution. Two halves of the same idea — what a solution
**must** do, and how **good** it is:

- **Conditions (must).** Per level — the vocabulary lives in
  [progression](../../design/systems/progression.md) §4:
  | Condition | Meaning |
  |---|---|
  | `translatable` | the **cube may be moved** (its centre changes) or is fixed at home |
  | `moves` | how many **zones** the solution scales — the cube edits `N` places, up or down |
  | `resizable` | the frame may be **resized** (`[1×, 2×]`) or is locked |
  | `rotate` | the spin gesture is available (later puzzles) |
  | `scalable` | **always true** — scaling the content is the verb; there is no locked-scale level |

- **Placement & size.** **Non-translatable** levels **bake the cube's position *and* size when the
  puzzle is created** (fixed); **translatable** levels let the **player move** it. The size is an
  **authored range** (`[minCubeSize, maxCubeSize]`) the creator **picks within** — bigger is a blunter,
  more forgiving lever, smaller a sharper local edit — so a fixed (non-translatable) region can still be
  made solvable. The **current version tests the translatable** puzzle.
- **Quality (how good).** The **three that decide pass/fail**: **window tightness**, **entropy spent**,
  **start margin** (the near-miss gap). The grade is already entropy-spent vs budget; the quality bar
  turns that into a **selection/ranking** signal — **authoring-side only**, so the player's score stays
  entropy left / stars.

**The brief per band** (from [progression](../../design/systems/progression.md) §4 — the creator bakes to this):

| Band | translatable | moves | resizable | rotate |
|---|---|---|---|---|
| jobs 1–2 (the scale) | no | 1 | no | no |
| job 3 (several zones) | no | 2+ | no | no |
| time tutorial (off-path planet) | no | 1 | no | no |
| job 4 (move the cube) | **yes** | 1+ | no | no |
| job 5 (resize the frame) | yes | 1+ | **yes** | no |
| jobs 6–8 (combine) | yes | 2+ | yes | no |
| later (rotate) | yes | 1+ | yes | **yes** |

**How it lands:**

- **The seed defines the job; the bake makes the level.** The **seed** carries the progression
  definition — the band, the conditions and the dials — applied **before** the field is generated, so the
  creator bakes to the brief rather than filtering afterwards. One seed in, one **level** out:
  `PuzzleData` is the baked result, carrying the **resolved** conditions.
- **Search honours the conditions** — extend `PuzzleSolutionSearch` from the scale-only grid to
  **cube size × scale** (plus **centre** when `translatable`) and up to `moves` edits, on an **adaptive**
  grid (coarse first, refine around the best hits); `PuzzleFieldBake` **rejects** a variant whose solution
  breaks them. The chosen **region** (centre + size) is **baked onto `PuzzleData`**. **`moves` is
  authoring-only** — the search bakes the multi-zone solution; the runtime just plays it.
- **The window is derived, not authored.** `Tcross = runDistance / nominalSpeed`; the window is a band
  `[Tmin, Tmax] = Tcross × [lo, hi]`. The bake **verifies reach**: the bare start is **outside** the band
  and the solution is **inside** it.
- **One best variant per seed** — ranked by the pass/fail three; `PuzzleSeedScanner` keeps the best and
  moves on.
- **Runtime enforces** — `PuzzleData` carries the resolved conditions; the runtime **locks the disallowed
  gestures** (no move when not translatable, no resize when not resizable). **No move count is shown**
  ([vision](../../design/vision.md) §8): the ship's trajectory each edit and the entropy spent are the
  hints — a nearer pass reads as closer to the solution, and needing more entropy means more edits are
  required.
- **PoC v1:** `translatable = true`, `scalable = true`, `moves = 1` (the creator's current test). The
  **game's first jobs are `translatable = false`** (fixed cube) per
  [progression](../../design/systems/progression.md) §4 — the PoC exercises the movable case because the
  scale grid is easier to drive that way.

**Settled in [progression](../../design/systems/progression.md) §4** — the per-level conditions and the
per-job map. The **quality axes** rank variants **authoring-side only**; the exact axes are still to
pick.

**Exit:** the brief is authored on the seed; the search honours the conditions and the bake rejects
breaks; the window is derived and reach-verified; one best variant per seed; the runtime locks the
disallowed gestures.

## 6. Slice 5 — physics (clock + mass)

Two sim changes that make the model physical, plus the time-face level. Both changes are small; both
need a retune and a lab check ([validation](../../design/vision.md) §7).

- **Two-term clock.** Replace the tuned clamp in `TrajectoryIntegrator` with the weak-field form
  `dτ = dt · √(max(0, 1 + 2Φ/c² − v²/c²))` — GR depth + SR speed. `Φ` and `v` are already per step; no
  new data, no new path. The clock then reads both the well depth and the ship's speed.
- **`mass ∝ radius³`.** Replace the `mass ∝ radius` coupling (constant density), so a grown planet
  gains mass the way a real one would. Retune the field strength (`sourceStrengthScale`) — the field is
  far more sensitive to the scale at `r³`.
- **No separate scale term.** Time comes from `Φ` (mass) and `v`: the scale grows a planet → deeper
  well → slower clock. The fictional `g(scale)` is dropped.
- **The time tutorial is an off-path planet.** A level with a planet **off the trajectory** isolates the
  clock: the scale changes its mass and `dτ`, without bending the path
  ([progression](../../design/systems/progression.md) §4).

**Why here:** these are the mechanic's physics; the bakes search the sim, so land this before baking
variants, and keep it offline / EditMode-tested (determinism).

**Exit:** the clock uses the two-term form; mass is `∝ radius³`; a puzzle re-bakes and verifies with the
retuned field; an off-path level reads as a pure clock puzzle.

## 7. Slice 6 — the entropy budget

The budget is the other half of the window invariant: the window must be **reachable within the
budget**. Derive it from the creator rather than authoring it:

- **Budget from the solution's cost.** `entropyBudget = Σ|Δscale|` of the clean solution, plus headroom
  (a per-band multiplier). The creator already knows the clean cost, so it sets the budget.
- **The invariant.** The bare start is **outside** the window (a miss); the solution is **inside** the
  window **and** inside the budget. If the clean solve costs more than the budget, the level is
  unsolvable — reject and resample.
- **Band and budget move together.** A tighter band needs more edits (more entropy), so the creator
  picks the pair with headroom — the same reach check as the window
  ([progression](../../design/systems/progression.md) §4).

**Exit:** the budget is derived per seed from the solution's cost; the bake rejects a seed whose clean
solution exceeds it; a puzzle is solvable inside both the window and the budget.

## 8. Candidate ideas (unscheduled — pick from here)

- **Multi-zone / multiple scales (job 3).** The search over **several** zones; the soft brush blends
  them; the creator must bake a multi-zone solution.
- **Rotate (later puzzles).** The search grid adds **rotation**; frame dragging.
- **Batch / CLI bake** over a seed list, for reproducibility and CI (the search is offline).
- **`PuzzleBrief` as a real asset** — the conditions + quality as one authored object.
- **Creator preview.** Draw the **corridor**, the **cube**, the **window band**, start vs solution, and a
  ship-time **scrub**; add 3D scene handles.
- **Per-job launch lore** — the launch speed is already baked per puzzle; only its *lore* is deferred.
- **The time face / off-path tutorial.** A level with a planet **off the trajectory** isolates the clock:
  the scale changes its mass (`mass ∝ radius³`) and so `dτ`, without bending the path
  ([progression](../../design/systems/progression.md) §4).
- **Window-edge robustness.** Verify the start still fails at the band edge (the "window is knife-edge"
  risk).

## 9. Out of scope

- **Runtime generation** — decided against: the field is baked, never generated at load.
- **The selection field / two-hand scale-out** — [integration](../backlog/gameplay/space-shaping-integration.md).
- **The validation** ([vision](../../design/vision.md) §7) — the creator stays offline/authoring;
  this plan does not touch the game loop.

## 10. Open questions (all closed 2026-10-27)

1. **Cube authority** — **settled for now:** `cubeSize = 0.25 m`, `zoneFalloff = 0.25`,
   `stepSecs = 1/1200` (a ~1 s budget for the 1 m run) and `BareMissMargin = 0.9`; 90/90 EditMode green.
   The size becomes an **authored range** the creator picks within (Slice 4).
2. **Condition vocabulary** — **settled** in [progression](../../design/systems/progression.md) §4
   (translatable / moves / resizable / rotate; scalable always true) plus the per-job map.
3. **Quality axes** — **settled:** the pass/fail three (window tightness, entropy spent, start margin),
   **authoring-side only**.
4. **Corridor** — **settled:** rotate the field so the run passes through its widest gap (no planets
   dropped).
5. **Per-level region** — **settled:** the cube size is an **authored range** in Slice 4, per job now.
6. **`moves > 1`** — **settled:** creator search only; the runtime does not validate the move count.
7. **Budget expression** — **agreed (2026-10-27):** express the budget as a **nominal crossing time**
   (`runDistance / nominalSpeed`) instead of `maxSteps x stepSecs`, which was mis-scaled to the 1 m run
   (the ship crawled, the field dominated). *Nominal* — the field's own effect on the ship's speed rides
   on top, it is not an exact crossing time. The window is a **band** `[Tmin, Tmax] = Tcross × [lo, hi]`,
   and the bake **verifies reach**: the bare start is **outside** the band and the solution is **inside**.
8. **Cube-size range** — **settled:** a **constant range** for now (later derivable from the level's
   size).
9. **Endpoints** — **settled:** a **planet pair** at least `minRunDistance` apart (a seed/creator rule).
10. **Run length** — **settled:** the **endpoint distance**, with `1 m` the target and a minimum.
11. **Resample** — **settled:** a **fixed cap** of tries, then reject and report.
12. **Search grid** — **settled:** **adaptive** (coarse first, refine around the best hits).
13. **Variants** — **settled:** **one best per seed**, ranked by the pass/fail three.
14. **Brief storage** — **settled:** authored on the **seed** (before generation), resolved onto `PuzzleData`.
15. **Runtime enforcement** — **settled:** **lock the disallowed gestures** (move / resize).

The **carried fixes** (code/authoring hygiene from the retired review) moved to
[tech debt](../backlog/gameplay/space-shaping-tech-debt.md).
