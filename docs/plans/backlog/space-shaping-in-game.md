# Space Shaping — In the Game (one puzzle)

**Status:** next · space shaping · updated 2026-10-28 — boot `Game.unity` into one baked puzzle: the
puzzle flow and the result seam. First of six integration plans; **gated on validation 1**.

**Related:** [campaign map](space-shaping-campaign-map.md) · [HUD + validation 2](space-shaping-hud-validation.md) ·
[progression](space-shaping-progression.md) · [legacy cut](space-shaping-legacy-cut.md) ·
[catalogue](space-shaping-catalogue.md) · [vision](../../design/vision.md) ·
[core loop](../../design/core-loop.md) · [jobs](../../design/jobs.md) ·
[modes](../../../Assets/_Modules/docs/standards/modes.md)

**Wiring rule:** authored scene/prefab objects and serialized refs only — no runtime asset or hierarchy
creation, no `GetComponent` lookups, no null-checks on serialized refs.

## 1. Context

The POC (`SpaceShapingPOC.unity`) proves the loop but lives outside the game: `Game.unity` has **no** puzzle
wiring (no `SpaceShapingState`, no views, no puzzle asset). This plan puts the puzzle into the game as the
only loop and stops there — the map, the HUD, progression, the legacy cut and the catalogue are their own
plans, each gated in sequence.

Decisions carried from the integration split: **full replacement** (one flow, one save, one map), **no
commit step** (every completed pinch flies the ship), **fail = committed spend reaches the budget** (entropy
left is the grade), **no in-level persistence**, Quest 3S **72 fps**.

### Decided 2026-10-28 (this plan only)

| Decision | Value | Why |
|---|---|---|
| Boot entry | A temporary **`PuzzleBootState`** after setup/splash awaits the room, then enters `SpaceShapingState`. | Skips the legacy round spawn and the welcome TTS; becomes `CampaignState` in the [map plan](space-shaping-campaign-map.md) instead of being thrown away. |
| Puzzle wiring | One serialized **`PuzzleData`** field, set to `PuzzleSeed_Shape_Planets_Puzzle.asset`; drop the `puzzles[]` array and `puzzleIndex`. | The POC's array is a test harness; the map plan turns the field into a catalogue lookup. |
| Puzzle placement | The puzzle rig is parented under the **room-anchored bubble parent** the map will use, with an authored local pose. | The POC's root sits at world `(0,0,0)`; in `Game.unity` the puzzle would land at floor height. The map's scale-up then becomes a transform change, not a re-parent. |
| Puzzle id | Set `id: 01_01` on the test asset. | Both baked assets ship `id:` empty, so `LevelData.IntroKey` resolves to `level__init`; the campaign map and the cookies need a real id. |
| Result surface | **Temporary** — lab panel / debug line. | The real result panel is the [HUD plan](space-shaping-hud-validation.md); building it twice is waste. |
| Run owner | **No `PuzzleRun` type in this plan** — `SpaceShapingState` keeps ownership of setup, session, integrator and launch. | The type does not exist; adding it now is one caller and no seam. The [map plan](space-shaping-campaign-map.md) introduces it only if it needs per-job runs. |
| Legacy states | Remove the legacy states we no longer route to from `Main.states` (files stay for the [cut](space-shaping-legacy-cut.md)). | `StateMachineFactory.Create` calls `Init` on **every** state (`StateMachineFactory.cs:18-27`), and `GameStartState.Init` calls `levelContainer.Init()` (`GameStartState.cs:34`) → `HideAll()` + the legacy view-mode fanout. |

Do **not** repoint `GameStartState.nextState` or `Main.initialState`: the first keeps the legacy round
alive, the second skips setup/splash and breaks the validation-2 path.

## 2. Slices

### G1 — Puzzle lifecycle in `Game.unity`

- Author `SpaceShapingState` with `SpaceShapingView`, `SpaceZoneView`, `SpacetimeMeshView`,
  `TrajectoryLineView` and `ShipTravelView`, plus `SpaceShapingSettings` and one serialized `PuzzleData`
  (`PuzzleSeed_Shape_Planets_Puzzle`, `id: 01_01`).
- Parent the puzzle rig under the room-anchored bubble parent with an authored local pose; the ship is the
  copied `SpaceShaping/Models/Ship/ShipVisual.prefab`, not the legacy `Spaceships` ship.
- Add the temporary `PuzzleBootState` between setup/splash and the puzzle (see the decisions above) and
  remove the legacy states from `Main.states`. The legacy files stay for the [cut](space-shaping-legacy-cut.md).
- Update `SpaceShapingStateEditor`: the "Next puzzle" button goes with `puzzles[]`; keep preview, reset and
  the solved-cube debug button.
- **Exit:** Play `Game.unity` → splash → one puzzle renders at hand height, the ship flies on every commit,
  reset restores the field and commands, no legacy round spawns; console clean.

### G2 — Result seam

- The state exposes one result — **Solved / Miss / Failed** — fired when the flight ends **and** the commit
  is still current; a new edit cancels the pending result; the grade comes from entropy left
  (`SpaceShapingSession.Grade()`).
- A miss or crash does not end the job; the only hard fail is committed spend reaching the budget. If one
  commit both solves and empties the budget, the solve wins.
- **A result must fire even when there is no flight.** `ShipTravelView.Travel` returns early for fewer than
  two trajectory points and never invokes `Completed` (`ShipTravelView.cs:31-33`), so a crash or instant
  outcome has to publish its result from the trajectory outcome instead of waiting on the ship.
- Surface it **temporarily** — lab panel or debug line. The real HUD/result panel is the
  [HUD plan](space-shaping-hud-validation.md); do not build it here.
- **Exit:** solve → result fires with 1–3 stars; a miss keeps shaping; a crash with no flight still fires
  its result; budget exhaustion fires Failed; an edit after a result does not resurrect it.

## 3. Runtime puzzle contract

| Scope | Fields |
|---|---|
| Per puzzle | `id`, origin, destination, `sources`, `captureRadius`, `tmin`, `tmax`, `entropyBudget`, `zoneCentre`, `cubeSize`, brief (`translatable`, `moves`, `resizable`, `rotate`) |
| Global settings | `launchSpeed`, `stepSecs`, `nominalSpeed`, `planetRadiusMin`/`Max`, `corridorClearance`, `cubeSizeMin`/`Max`, `sourceStrengthScale`, `separationScale`, `lightSpeed`, `zoneFalloff`, scale envelope |
| Creator-only | seed, bake record, verification quality |

- `scalable` is always true; `moves` is authoring-only and never shown or enforced at runtime.
- **Launch is global** (decided 2026-10-27): `SpaceShapingSettings.launchSpeed` is the one given launch for
  every job; `PuzzleData.launchSpeed` is an authoring bake record and is not read at runtime
  ([resources](../../design/systems/resources.md): “Fuel = global”).
- `PuzzleData.solution` is creator-only verification metadata and is not shipped.
- Falloff and the scale envelope stay global; add per-puzzle values only if a job demands them.

## 4. Run and readout

- `SpaceShapingState` keeps ownership of `SpaceShapingSetup`, `SpaceShapingSession`, the integrator and the
  launch, plus the view, ship and flow. **No `PuzzleRun` type here** — see the decisions.
- API on the state/session: `Load(PuzzleData)`, `Reset`, `PreviewZone`, `Commit`, `Undo`, `Readout`,
  `Result`, `Grade`.
- Readout: entropy spent/left/budget, ship time, `[Tmin, Tmax]`, solved, grade, command count
  (`PuzzleReadout`, landed 2026-10-28, with one `ReadoutChanged` event).
- `Reset` is free and clears commands and entropy; `Undo` refunds. **No Undo UI in this slice** — the
  session call exists, the surface is a later plan.

## 5. Tests

- The existing Space Shaping EditMode suite stays green (session, solve, rescale, trajectory determinism,
  zone conditions, readout).
- Add result-seam tests: solve wins over a same-commit budget exhaustion; a new edit cancels a pending
  result; a miss does not clear commands.

## 6. Risks

- **The test job is scale-only** (`moves: 1`, `translatable: 0`, `resizable: 0`, `cubeSize` at max). The exit
  path proves scale + flight + result, not move/resize; a movable job needs a [catalogue](space-shaping-catalogue.md)
  bake before that claim is tested.
- **The legacy `Init` is neutralised by unwiring, not deleting.** If a legacy state is left in `Main.states`,
  `HideAll()` runs at boot and the fanout subscribes to `UniverseView` — inert now, destructive once the
  campaign field exists.
- **Placement is easy to skip.** Without the room-anchored parent the puzzle renders at floor height, which
  reads as a broken puzzle rather than a placement bug.

## 7. Out of scope

- Campaign map, cube selector, aperture — [campaign map](space-shaping-campaign-map.md).
- Entropy bar, clock window band, result/fail panel — [HUD](space-shaping-hud-validation.md).
- Save, stars, reset semantics — [progression](space-shaping-progression.md).
- Deleting the legacy flow or scene objects — [legacy cut](space-shaping-legacy-cut.md).
- Authoring more than the one puzzle — [catalogue](space-shaping-catalogue.md).
- Job card intro/TTS — moved to [campaign map](space-shaping-campaign-map.md) M3.
- Multi-zone solutions — [multi-zone](../in-progress/space-shaping-multizone.md).
