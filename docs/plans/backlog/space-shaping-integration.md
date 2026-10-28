# Space Shaping — Integration into the Game

**Status:** next · space shaping · updated 2026-10-28 — **full replacement confirmed**. Two stages:
**slice fusion** (I0–I1 = validation 2) and **campaign** (I2–I5). Gated on validation 1; do not start I0
before it passes.

**Related:** [vision](../../design/vision.md) · [core loop](../../design/core-loop.md) ·
[progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[resources](../../design/systems/resources.md) · [cube](../../design/systems/cube.md) ·
[variations](../../ideas/variations.md) ·
[physics](../in-progress/space-shaping-physics.md) ·
[feel](space-shaping-feel.md) · [modes](../../../Assets/_Modules/docs/standards/modes.md) ·
[review](../../reviews/integration-plan.md)

**Wiring rule:** authored scene/prefab objects and serialized refs only. When the legacy flow is removed,
delete its scene objects, components, assets and asmdefs — do not leave them disabled.

## 1. Goal

Turn the POC into the actual game: the puzzle loop **replaces** the route/round/economy loop, wired into
`Game.unity`, the flow state machine, save/load, and UI.

The replacement is decided. The plan no longer carries a mode branch; if that ever changes, §3.2 shrinks,
but this plan does not.

## 2. Decisions (closed)

| Decision | Value |
|---|---|
| Replacement | **Full replacement.** One flow, one save, one map; no mode alongside the legacy loop. |
| Scope | The **full run** — level selector and **3–5 jobs first**, 8–10 as stretch ([jobs](../../design/jobs.md)). |
| Loop | **No commit** — the ship flies on every completed modification; a miss is a check, not a fail. |
| Fail | **Entropy zero** before a solve; the grade is entropy left (1–3 stars). |
| Persistence | **Player progression only** — solved jobs + stars; never in-level state. |
| Target | Meta Quest 3S, **72 fps**. |
| UI | The HUD sits **over the level** (world-anchored). |

## 3. What integration means

### 3.1 Replace the game loop

- One puzzle at a time: present → shape / fly / check → solve → score → result → back to the map.
- Per-job **stars** from entropy left; `SpaceShapingSession.Grade()` already computes the buckets.
- The only hard fail is entropy reaching zero; a miss or crash does not end the job.
- No rounds, letters, money, hubs, or destinations.

### 3.2 Remove the legacy flow

Delete (not disable) the pieces the replacement drops:

- **Flow/panels:** `GameStartState` (mid-level resume), `EditionState` (+ its asmdef), `NewRoundState`,
  `GameEndState`, `GameEndPanel`, `RoundSelector`, `LevelContainer`, and the legacy start/end panels.
- **Content/economy:** `PlanetsSpawner`, `PlanetStore`, `ResourcePanel`, `StorePanel`, `RoundData` and the
  `Resources/Chapters/*` assets, round/chapter configuration.
- **Route/ship:** hubs/stations, ship grab, destination selection, `SplineContainerWrapper`,
  `SplineLegSampler`, `TravellerRouteRunner`, `SpaceshipRouteSession`, and the spaceship/hub prefabs and
  scene objects.
- **UI:** route/resource/store panels, their prefabs, and their localization keys.
- **Persistence:** the route/round/economy fields of `GameSnapshot`; the legacy cookie is ignored on first
  boot (reset-to-fresh, no migration).
- Remove empty asmdefs and `.meta` files after the code goes.

**Before deleting, move the keepers (§3.9).** `LevelContainer` currently owns the sector field and bubble
restore; `GameStartState` owns the resume path.

### 3.3 Keep and reuse

- The one-view universe frame (`UniverseView` / `UniverseContentRig`) and MR bubble anchoring/placement.
- The sector field and `SectorFieldSpawner` as the campaign map substrate.
- The generic level plumbing: `LevelSelector`, `LevelCompleteCookie`, `PlayerData` (order, current id,
  stars). These exist in `_Modules/Casual` but are **not wired into `_Game`** yet.
- The shared planet view (`FieldPlanetBehaviour` + `NodeBody` variants and spin) — **already unified
  (2026-10-28):** the POC spawns it from the shared prefab and `SpaceSourceView` is removed.
- XR rig/hands, the cube two-hand scale gesture, and the POC's lattice, zone, trajectory and ship-travel
  views.
- Setup/splash, analytics, TTS framework, lore.

The **cube level selector is new.** `LevelsPanel` is a generic UI list, not a field of cubes.

### 3.4 Scene and flow

- `Game.unity`: remove the legacy route/hub wiring; add the puzzle lifecycle, the campaign map/selector,
  the derived grid (flat until the player scales), the entropy bar, and the puzzle controller.
- Reuse the existing setup/splash states; replace only the game loop.
- Flow vs mode (to define before I0): the map is the **Bubble view**; the puzzle lifecycle is a **Flow
  state** because it owns progression, panels and save; the cube scale-up is a view/mode transition. Follow
  [modes](../../../Assets/_Modules/docs/standards/modes.md).
- I0–I1 is **validation 2**: one seated MR path from boot to a scored puzzle.

### 3.5 Save / load

- Store **player progression** — solved job ids and their stars; current/unlocked job.
- Reuse `LevelCompleteCookie` (`hasBeenCompleted`, plus a new `stars` field; `score` stays generic) and
  `PlayerData`; `LevelSelector` already derives the following level from the cookies. The puzzle catalogue
  supplies the order.
- **No in-level state:** the field is baked, so a run is rebuilt from the baked puzzle, never resumed
  mid-flight.
- **Reset:** ignore the legacy `GameSnapshot` cookie; no migration.
- **Keepers inside `GameSnapshot`:** `bubbleOffsetLocal`, `bubbleYawDegrees` (MR placement) and
  `silentPlanetSeed` are not legacy route data. Move them to a small placement/campaign snapshot before
  the cookie is dropped.
- `PuzzleData.solution` is creator-only verification metadata; do not ship it to the runtime.

### 3.6 UI

- The HUD sits **over the level** (world-anchored): entropy left / grade, window `[Tmin, Tmax]`,
  result/fail, reset, map/retry.
- The POC has **no HUD and no readout events** today. `SpaceShapingState` must expose a read-only readout
  (entropy spent/left, ship time, window, solved, grade) and one result event; the HUD/result subscribes.
- Remove the route/resource/store panels.
- Locked-gesture refusal, the manual, and the tech voice remain in [feel](space-shaping-feel.md).

### 3.7 Level catalogue

- Integration consumes **baked** `PuzzleData`; authoring and the seed search are the creator’s work.
  **No runtime generation.**
- The **seed + brief** is the source; fixed `sources` remain the hand-tuned fallback
  ([variations](../../ideas/variations.md) §3).
- The catalogue is one ordered `ChapterData` list; `PuzzleData.id` uses the `XX_YY` shape the level
  cookies parse.
- Prove **3–5 jobs** first; 8–10 is a stretch. The `moves = 2+` bands still depend on the creator’s
  multi-zone search; the physics plan must land before more bakes.

### 3.8 The sector field (campaign substrate)

The campaign map is a **field of cubes** over the bounded sector field; the field is the map substrate,
not scenery.

- **The campaign field places cubes, one per job.** A picked cube **scales up into the play space**.
- **The puzzle generates its own field from its seed.** The campaign field does **not** supply a puzzle’s
  layout ([variations](../../ideas/variations.md) §3); the two fields are independent.
- **The planet presentation is shared.** Both fields spawn the same pooled planet view
  (`FieldPlanetBehaviour`); only the layout source differs. Do not add a puzzle-only planet view.
- **Aperture.** Raise `viewRadius` / `fieldRadius` in the **map slice (I1)** because the map needs the wide
  field — not because puzzle content moves onto the field. The wired field is deliberately room-local
  (`viewRadius 1.2`, `fieldRadius 1.8`).
- **Color scope.** It does not use `viewRadius`; out-of-frame content is grayed. The hide band is
  `hideRadiusFactor 1.5`; raise `fieldRadius` and `softSectorBudget` together.
- **Decor migration is done.** `SectorFieldSpawner` already spawns the field’s ambient sectors; there is
  no separate silent-planet pool left to migrate. The shared view and prefab live in
  `DigitalLove.Game.Nodes`; only the `SilentPlanets` spawner naming/ownership moves in §3.9.

### 3.9 What must move before the cut

| Owner today | What to keep | Move to |
|---|---|---|
| `LevelContainer` | `SectorFieldSpawner` spawn/reset, bubble restore | campaign field/placement owner |
| `GameStartState` | boot into the campaign map | new boot/map Flow |
| `GameSnapshot` | `bubbleOffsetLocal` / `bubbleYawDegrees`, `silentPlanetSeed` | placement/campaign snapshot |
| `UniverseView` | bubble mode, room anchor, restore | keep; do not delete with the level container |
| `Decoration` | shared planet view + prefab (`FieldPlanetBehaviour`, `FieldPlanet.prefab`) | `Nodes` — **done 2026-10-28** |

**Cut order:** extract the keepers → land the puzzle flow (I0) → map/HUD (I1) → progression (I2) → delete
the legacy flow (I3). Cutting first breaks the keepers.

## 4. Non-goals

- Simultaneous multi-ship ([variations](../../ideas/variations.md) §2.1).
- A sequential **loop route** — a later variation, not this slice
  ([variations](../../ideas/variations.md) §2.2).
- Runtime/procedural generation; the field is baked.
- Streaming and galaxies beyond the campaign selector’s field.
- A demand/economy layer on the map ([element pack](../../ideas/element-pack.md)).
- Final art, audio and feel polish — the [feel plan](space-shaping-feel.md) owns those.

## 5. Phases

**Gates:** validation 1 (lab) before I0. **I0–I1 is validation 2.** Do not start I2–I5 unless it passes.

**Split:** A — fuse the slice (I0–I1). B — campaign (I2–I5). Keep the gate between them.

### I0 — Puzzle flow + one real puzzle

- The puzzle Flow state and one baked puzzle in `Game.unity`, selected directly for the test.
- Reuse the POC session/views; add the read-only readout and result seam.
- **Exit:** the game boots into one puzzle; solve → grade → reset; a miss keeps shaping; entropy zero
  fails.

### I1 — Campaign map + selector + HUD

- One-view map over the sector field: one numbered cube per job, lit/locked/current, pick → scale up into
  the play space, back out.
- Raise the aperture for the map; derived grid flat until the player scales.
- Entropy bar, window gauge, result/fail and reset/map over the level.
- **Exit:** the player picks a cube, scales up, solves and returns to the map; validation 2 runs.

### I2 — Progression save

- Record solved jobs + stars; current/unlocked; reset legacy; bubble placement carve-out.
- **Exit:** solve → restart → map and stars restored; the legacy cookie is ignored.

### I3 — Remove the legacy flow

- Extract-first per §3.9; delete the code, assets, scene objects, asmdefs, panels and localization.
- **Exit:** no legacy route/hub/round/economy content remains; setup/splash still boots.

### I4 — Campaign catalogue

- Author 3–5 baked puzzles first, then 8–10; briefs per [jobs](../../design/jobs.md); selection by id.
- No runtime generation; the creator bakes offline.
- **Exit:** a library of puzzles is authored, ordered, selectable and star-bearing.

### I5 — Acceptance

- Quest 3S 72 fps capture; boot → map → pick → solve → back → reload; legacy absent; onboarding/tuning.
- Feel/presentation stays with [feel](space-shaping-feel.md); not duplicated here.

## 6. Definitions (proposed 2026-10-27)

The working contract. The 2026-10-27 decisions are recorded in §6.6; the rest are proposed defaults.

### 6.1 Campaign and progression

- **One chapter** (`01`) holds the run in order; `PuzzleData.id` is `01_01` … `01_N`; the map number is the
  global index + 1. `ChapterData.levels` is the order. Acts can split later without changing ids.
- **Counts:** 3–5 jobs for validation 2; 8–10 only after it passes.
- **Map states:** solved (lit + stars), current (first unsolved, highlighted), locked (dark). Solved jobs are
  replayable; locked jobs refuse visibly.
- **Navigation — decided (2026-10-27): direct pick only.** The map is the only navigation; the result
  offers Map/Retry, not Next. `LevelSelector` still supplies the first unsolved/current id, but the player
  always chooses the cube.
- **Progress:** one `LevelCompleteCookie` per job; add `stars` (1–3) to `LevelCompleteCookieMetadata`;
  `hasBeenCompleted` stays the unlock flag; replay keeps the best stars; `LevelSelector` supplies the
  current/following id.
- **Save:** write on solve and on app pause through `PlayerData`; no in-level snapshot. **Reset** clears the
  puzzle completion cookies, keeps bubble placement. First boot ignores the legacy `GameSnapshot`.

### 6.2 Runtime puzzle contract

| Scope | Fields |
|---|---|
| Per puzzle | `id`, origin, destination, `sources`, `captureRadius`, `tmin`, `tmax`, `entropyBudget`, `zoneCentre`, `cubeSize`, brief (`translatable`, `moves`, `resizable`, `rotate`) |
| Global settings | **`launchSpeed`**, `stepSecs`, `nominalSpeed`, `planetRadiusMin`/`Max`, `corridorClearance`, `cubeSizeMin`/`Max`, `sourceStrengthScale`, `separationScale`, `lightSpeed`, `zoneFalloff`, scale envelope |
| Creator-only | seed, bake record, verification quality |

- `scalable` is always true; `moves` is authoring-only and never shown.
- `solution` is removed from the shipped `PuzzleData`.
- **Launch — decided (2026-10-27): global.** `SpaceShapingSettings.launchSpeed` is the one given launch for
  every job; `PuzzleData.launchSpeed` becomes an authoring-only bake record and is not read at runtime.
  This matches [resources](../../design/systems/resources.md) (“Fuel = global”).
- Falloff and the scale envelope stay global for the slice; add per-puzzle values only if a job needs them.

### 6.3 Run and readout

- `PuzzleRun` owns `SpaceShapingSetup`, `SpaceShapingSession`, the integrator and the launch; the MonoState
  owns the view, ship and flow.
- API: `Load(PuzzleData)`, `Reset`, `PreviewZone`, `Commit`, `Undo`, `Readout`, `Result`, `Grade`.
- `Commit` simulates and starts the flight. The result fires when the flight ends **and** is still the
  current commit; a new edit cancels the pending result.
- Outcomes: **Solved** (arrive inside the window), **Miss** (arrive outside, miss, crash, step overrun),
  **Failed** (entropy zero and not solved). If the same commit both solves and empties the budget, the
  solve wins; a fail is shown after the miss flight.
- Readout: entropy spent/left/budget, ship time, `[Tmin, Tmax]`, solved, grade, command count.
- `Reset` is free and clears commands/entropy; `Undo` refunds; no Undo UI in this slice.

### 6.4 Flow and ownership

- Flow: `CampaignState` (boot + map) → `SpaceShapingState` (play + result panel) → back to the map.
  Setup/splash stay.
- The result is a **panel inside `SpaceShapingState`**, not a separate Flow state.
- The map is `UniverseViewMode.Bubble`; there is no new map mode. The cube scale-up is a presentation
  transition.
- `CampaignView` owns the map cubes and `SectorFieldSpawner`; `SpaceShapingView` owns the puzzle; a
  `PlacementStore` owns the bubble offset/yaw. All are authored in `Game.unity`.
- After the cut, `CampaignState` / `CampaignView` / `PlacementStore` replace `LevelContainer` and
  `GameStartState`; `UniverseView` stays.

### 6.5 Map field and aperture

- Map bubble: `miniBubbleRadius 0.45`, `bubbleDistance 0.75`, `viewRadius 12.5`, `fieldRadius 12.5`,
  `cellSize 5`, `softSectorBudget 32`, `softPlanetBudget 64`, `hideRadiusFactor 1.5`; MiniScale ≈ 0.036.
  This keeps ~5 cm hand targets for 8–10 cubes; tune on device.
- **Cube placement — decided (2026-10-27): authored.** One hand-placed position per job for legible
  spacing; the generated sector field supplies the backdrop, not the cube positions.
- Puzzle fields are unchanged and independent; the map aperture does not affect them.
- Performance: 72 fps on Quest 3S; map ≤ 32 sectors / 64 planets + 10 cubes; puzzle ≤ 10 planets + lattice;
  no per-frame allocations; measured with FrameTiming.

### 6.6 Decisions recorded

- **Navigation:** direct pick only; the map is the only way in, and the result offers Map/Retry.
- **Cube placement:** authored positions, one per job.
- **Launch:** global; `SpaceShapingSettings.launchSpeed`, not `PuzzleData.launchSpeed`.

The [review](../../reviews/integration-plan.md) §6 has the prep order; none of it needs the editor or the
scene.
