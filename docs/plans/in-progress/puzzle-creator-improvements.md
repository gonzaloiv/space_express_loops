# Space Shaping — Puzzle Creator Improvements (constants, cube, density, conditions)

**Status:** now · space shaping · updated 2026-10-27 — the creator follow-ons, in order: pin the
**working constants** (run `1 m`, cube `0.15 m`), **spawn the cube** per puzzle (current cube as prefab,
one instance), fill the run (**density / run corridor**), and define the **brief (conditions +
quality)**. Supersedes the retired puzzle-creator plan: its mechanics (P0–P4) are done and test-green.

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

Four follow-ons, in order:

1. **Constants** — pin the numbers: run `distance = 1 m` (not editable for now), cube `0.15 m` at the centre.
2. **Spawn the cube** — from the current cube as a prefab, one instance, instantiated on puzzle launch.
3. **Density** — the field **fills** the run, with the endpoints **inside** it and a clear run corridor.
4. **Conditions + quality** — the per-level **brief** ([progression](../../design/systems/progression.md) §4); the creator's PoC tests the **translatable** puzzle, the game's first jobs are **fixed**.

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
- **The cube = `0.15 m`** at the field's centre — a fixed **sub-region** (15 % of the run), not the whole
  run. This is what makes a solution a **local shortcut** instead of a global zoom
  ([cube](../../design/systems/cube.md): "a shrunk zone is a shortcut").
- The cube's **content inset stays a fraction** (`contentInsetFraction = 0.05`), so the content region
  scales with the cube (0.15 m → ≈0.1425 m). Nothing absolute.
- **The planet radius is the arrival sphere** — `planetRadius` sets the field planets (with a small
  spread) **and** the destination's capture sphere, so touching the goal planet is an arrival. There is
  no separate `captureRadius`; the player sees the goal.
- Use the **same** region in the creator's search and on puzzle start; the cube is spawned in Slice 2.
- The **falloff is already unified** (one source: `SpaceShapingSettings.zoneFalloff`); in Slice 2 the
  cube prefab **references the settings** and pulls it on spawn, so there is no second value to keep in
  step.

**Why:** a constant is a stable base to design against. **Per-level** values are a
[progression](../../design/systems/progression.md) decision, not a creator constant.
**Authority is untested** — start with 0.15 m and test whether a single cube bends the run enough.

## 3. Slice 2 — spawn the cube per puzzle

Today the cube is a **scene object** that `SpaceShapingState.ResetView` resizes in place. Make it
**spawned per puzzle**, so a puzzle owns its region and the level-selector transition has somewhere to
come from.

- **Convert the current scene cube into the prefab** (the designer edits it later): the "Zone Cube"
  (`Transform` + `MeshRenderer` + `SpaceZoneView`) and its grab handle (`Grabbable` +
  `ZoneGrabListener`). The grab uses the **Oculus Interaction SDK** (`Grabbable` / `PointerEvent`),
  already in the scene — not XRIT.
- **The cube prefab references `SpaceShapingSettings`** and reads the region/falloff from it **on
  spawn**, so the settings stays the single source (no baked duplicate to keep in step).
- **Serialized prefab reference; instantiate when the puzzle launches.** One instance now; the design
  allows **more** later if a puzzle varies its cubes.
- **Per-puzzle region** (centre + size + falloff) lives on `PuzzleData`, baked by the creator; the search
  stays pure data, only the view spawns.
- Re-cache/sync the grab handle after a frame change (`ZoneGrabListener` caches its rest size at gesture
  start).

**Risk:** too small a cube (or one the ship's path misses) → "no scale arrives"; reject and resample.

## 4. Slice 3 — density / the run corridor

With the run pinned at 1 m, the field must **fill** it. Today the cluster is sized to fit *inside* the
run (`PuzzleFieldBuilder.SectorRadius`), so the endpoints land **outside** the planets. Target: a field
**full of planets**, with `origin`/`destination` **inside** it, and the path crossing the field.

- **Fill the run.** The field spans the 1 m run; the endpoints sit **inside** the field (±0.5 m).
- **Keep the run axis clear** so the bare start **near-misses, not crashes** — **rotate the field so the
  run passes through its widest gap** (no planets dropped).
- The **corridor width** is a seed/brief dial — it is what lets `planetCount` rise. The verifier already
  requires the start to be a `Miss`; the corridor keeps it a near-miss at higher density.
- **Endpoints are planets.** `origin`/`destination` are **planets in the field** (not bare ±x points); an
  **intra-field distance** dial sets the run between them, and the **launch is derived from the valid
  solution** so the bare start near-misses.

**Risks:** a dense field can leave **no scale arriving** (the search finds no window) → reject and
resample (`PuzzleSeedScanner`); the corridor must be wide enough for the launch, narrow enough to read
dense.

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

- **Placement.** **Non-translatable** levels **bake the cube's position when the puzzle is created**
  (fixed); **translatable** levels let the **player move** it. The **current version tests the
  translatable** puzzle.
- **Quality (how good).** The **three that decide pass/fail**: **window tightness**, **entropy spent**,
  **start margin** (the near-miss gap). The grade is already entropy-spent vs budget; the quality bar
  turns that into a **selection/ranking** signal — **authoring-side only**, so the player's score stays
  entropy left / stars.

**How it lands:**

- **Search honours the conditions** — extend `PuzzleSolutionSearch` from the scale-only grid to
  **centre × scale** (`translatable`) and up to `moves` edits; `PuzzleFieldBake` **rejects** a variant
  whose solution breaks them. **`moves` is authoring-only** — the search bakes the multi-zone solution;
  the runtime just plays it.
- **Quality ranks the variants** — score each baked variant on the axes above, keep a **ranked set**, and
  let `PuzzleSeedScanner` become a **scored seed library** (not "first working seed").
- **Runtime enforces** — `PuzzleData` carries the resolved conditions; `ZoneGrabListener` rejects
  one-hand moves when not translatable and two-hand scale when not scalable. **No move count is shown**
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

## 6. Candidate ideas (unscheduled — pick from here)

- **Multi-zone / multiple scales (job 3).** The search over **several** zones; the soft brush blends
  them; the creator must bake a multi-zone solution.
- **Rotate (later puzzles).** The search grid adds **rotation**; frame dragging.
- **Batch / CLI bake** over a seed list, for reproducibility and CI (the search is offline).
- **`PuzzleBrief` as a real asset** — the conditions + quality as one authored object.
- **Creator preview.** Draw the **corridor**, the **cube**, the **window band**, start vs solution, and a
  ship-time **scrub**; add 3D scene handles.
- **Per-job launch lore** — the launch speed is already baked per puzzle; only its *lore* is deferred.
- **The time face / planetless tutorial.** The scale also drives the clock (`dτ · g(scale)`); a
  **planetless** level isolates it ([progression](../../design/systems/progression.md) §4) — the creator
  would bake a zero-planet field with the time effect.
- **Window-edge robustness.** Verify the start still fails at the band edge (the "window is knife-edge"
  risk).

## 7. Out of scope

- **Runtime generation** — decided against: the field is baked, never generated at load.
- **The selection field / two-hand scale-out** — [integration](../backlog/gameplay/space-shaping-integration.md).
- **The device gate** ([vision](../../design/vision.md) §7) — the creator stays offline/authoring;
  this plan does not touch the game loop.

## 8. Open questions

1. **Cube authority** — start at `0.15 m`; test whether one cube bends the run enough for the window.
2. **Condition vocabulary** — **settled** in [progression](../../design/systems/progression.md) §4
   (translatable / moves / resizable / rotate; scalable always true) plus the per-job map.
3. **Quality axes** — **settled:** the pass/fail three (window tightness, entropy spent, start margin),
   **authoring-side only**.
4. **Corridor** — **settled:** rotate the field so the run passes through its widest gap (no planets
   dropped). The **endpoints are planets** with an intra-field distance dial (Slice 3).
5. **Per-level region** — a [progression](../../design/systems/progression.md) decision; when does it
   land? `seed.distance` is kept (non-editable) for it.
6. **`moves > 1`** — **settled:** creator search only; the runtime does not validate the move count.

## 9. Carried fixes (from the retired creator review)

Open code/authoring items the P0–P2 review raised, folded here so the review can be retired. None
gate the slices above, but they should land with them.

**Correctness**

- **The verifier accepts a crashing start.** `PuzzleVerification.StartFails` is `!= Arrive`, so a
  `Crash` passes as "start fails". Require `Miss` and reject `Crash` — the bake path already requires
  `Miss` (`PuzzleFieldBake.StartMisses`); this only tightens the verifier's report.
- **The creator's zone geometry ≠ the game's zone geometry.** The view builds its zone with a
  **content inset** and the unified falloff; the creator's `PuzzleZone.ToSpaceZone()` uses **no inset**.
  The search must use the **same region + inset + falloff** the view uses, or a found solution will not
  reproduce. (Falloff is unified in Slice 1; the inset is not.)
- **`PuzzleData.solution` ships in the player asset.** The runtime loop does not need it — it is
  authoring metadata. Keep it in the creator artifact, or accept it in the build.

**Authoring hygiene**

- **`PuzzleArtifactExporter` churns GUIDs.** `DeleteAsset` + `CreateAsset` mints a new GUID on every
  re-export, breaking references. Load-and-overwrite (`LoadAssetAtPath` + `EditorUtility.CopySerialized`)
  or create only when absent.
- **Intermediates are committed and split across folders.** Keep intermediates under `Generated/` and
  commit only the final `PuzzleData`; stage 1 currently lives outside `Generated/`.

**Code shape**

- **`SpaceShapingSetup` duplicates `PuzzleSimSettings`.** It re-declares the sim scalars as flat fields;
  hold a `PuzzleSimSettings` snapshot instead.
- **`PuzzleZone.size` → half-extents footgun.** `ToSpaceZone` silently halves `size` while
  `SpaceZone.HalfExtents` is named for what it is. Rename the field to `halfExtents`.
- **`SourceSpec.MoveRange` is vestigial.** `SourceSpecs.From` sets it to the planet's scale and the
  constraint clamp never binds (also flagged in [cube](../../design/systems/cube.md) §Open). Drop it or
  give it a meaning.

**Tests**

- **`PuzzleSearchTests` mutates a shared `puzzle`** across tests → order-dependent; build a puzzle per
  test.
- **The seed scan `1..32` with a hard `Assert`** couples the suite to generator tuning; pin a known-good
  seed or assert search behaviour.
- **`PuzzleFieldTests.Build_FieldFitsThePlaySpace` is trivial** (`shape.radius.max < seed.distance` is
  always true); assert the extent or drop it.

**Settled (recorded, not a task)**

- **Planet-size band — keep.** The bridge's `radius * 2f` and the builder's `distance × 0.06 ± 30 %`
  stay; matching the universe's smaller look is a lab/test question, not a doc decision.
- **The fixed cube is the authored region the view presents.** For non-translatable puzzles
  (progression jobs 1–2), the creator must not invent the cube size — the puzzle carries the region the
  view shows (Slices 1–2).
