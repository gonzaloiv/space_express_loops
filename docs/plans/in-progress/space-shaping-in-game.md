# Space Shaping — In the Game (one puzzle)

**Status:** now · space shaping · updated 2026-10-29 — boot `Game.unity` into one baked puzzle: the
puzzle flow and the result seam. First of six integration plans; **validation 1 passed on the Quest 3S
POC (2026-10-29)**, so it is unblocked.

**Related:** [campaign map](../backlog/space-shaping-campaign-map.md) · [HUD + validation 2](../backlog/space-shaping-hud-validation.md) ·
[progression](../backlog/space-shaping-progression.md) · [legacy cut](../backlog/space-shaping-legacy-cut.md) ·
[catalogue](../backlog/space-shaping-catalogue.md) · [vision](../../design/vision.md) ·
[core loop](../../design/core-loop.md) · [jobs](../../design/jobs.md) ·
[modes](../../../Assets/_Modules/docs/standards/modes.md)

**Wiring rule:** authored scene/prefab objects and serialized refs only — no runtime asset or hierarchy
creation, no `GetComponent` lookups, no null-checks on serialized refs.

## 1. Context

The POC (`SpaceShapingPOC.unity`) proves the loop but lives outside the game: `Game.unity` has **no** puzzle
wiring (no `SpaceShapingState`, no views, no puzzle asset). This plan puts the puzzle into the game as the
only loop and stops there — the map, the HUD, progression, the legacy cut and the catalogue are their own
plans, each gated in sequence.

**Validation 1 met (2026-10-29).** The POC boots 8 baked jobs (`SpaceShapingPOC.unity:45153`, `01_01`–`01_08`)
and was solved in the Quest 3S headset, so the mechanic survives a goal and a cost on device. The four
translatable `moves: 2` assets do **not** prove multi-zone play — a single zone also solves them, and the
mechanic is now [parked](../backlog/space-shaping-multizone.md). `Game.unity` still has none of it wired —
G1 is the next move.

Decisions carried from the integration split: **full replacement** (one flow, one save, one map), **no
commit step** (every completed pinch flies the ship), **fail = committed spend reaches the budget** (entropy
left is the grade), **no in-level persistence**, Quest 3S **72 fps**.

### Decided 2026-10-28 (this plan only)

| Decision | Value | Why |
|---|---|---|
| Boot entry | A temporary **`PuzzleBootState`** after setup/splash awaits the room, then enters `SpaceShapingState`. | Skips the legacy round spawn and the welcome TTS; becomes `CampaignState` in the [map plan](../backlog/space-shaping-campaign-map.md) instead of being thrown away. |
| Puzzle wiring | One serialized **`PuzzleData`** field, set to a baked job-01 puzzle with `id: 01_01`; drop the `puzzles[]` array and `puzzleIndex`. The asset must match [jobs](../../design/jobs.md): fixed cube, one zone, one planet. | The POC's array is a test harness; the map plan supplies catalogue selection. The checked-in `Generated/Job_01_01_Puzzle.asset` currently has three sources, so it must not be wired unchanged as job 01. |
| Puzzle placement | Parent the puzzle rig under the **room-anchored bubble parent**, with an authored local pose. | The POC root sits at world `(0,0,0)`; in `Game.unity` the puzzle would land at floor height. This establishes stable Bubble-local placement only; it does not prescribe a map-to-puzzle transition. The campaign-map presentation must follow [vision §3](../../design/vision.md#3-mixed-reality-why-the-bubble-is-the-point): one Bubble view, whole map visible, no Map/Sculpt split. |
| Puzzle id | Preserve `id: 01_01` on the baked job-01 asset. | Jobs and campaign order use `XX_YY` ids; the current generated asset already has this id. |
| Result surface | **Temporary** — lab panel / debug line. | The real result panel is the [HUD plan](../backlog/space-shaping-hud-validation.md); building it twice is waste. |
| Run owner | **No `PuzzleRun` type in this plan** — `SpaceShapingState` keeps ownership of setup, session, integrator and launch. | The type does not exist; adding it now is one caller and no seam. The [map plan](../backlog/space-shaping-campaign-map.md) introduces it only if it needs per-job runs. |
| Legacy states | Remove the legacy states we no longer route to from `Main.states` (files stay for the [cut](../backlog/space-shaping-legacy-cut.md)). | `StateMachineFactory.Create` calls `Init` on **every** state (`StateMachineFactory.cs:18-27`), and `GameStartState.Init` calls `levelContainer.Init()` (`GameStartState.cs:34`) → `HideAll()` + the legacy view-mode fanout. |

Do **not** repoint `GameStartState.nextState` or `Main.initialState`: the first keeps the legacy round
alive, the second skips setup/splash and breaks the validation-2 path.

## 2. Slices

### G1 — Puzzle lifecycle in `Game.unity`

- Author `SpaceShapingState` with `SpaceShapingView`, `SpaceZoneView`, `SpacetimeMeshView`,
  `TrajectoryLineView` and `ShipTravelView`, plus `SpaceShapingSettings` and one serialized `PuzzleData`.
  Bake the job-01 asset to match [jobs](../../design/jobs.md) (fixed cube, one zone, one planet, `id: 01_01`);
  do not wire the current three-source `Generated/Job_01_01_Puzzle.asset` unchanged.
- Parent the puzzle rig under the room-anchored bubble parent with an authored local pose; the ship is the
  copied `SpaceShaping/Models/Ship/ShipVisual.prefab`, not the legacy `Spaceships` ship.
- Add the temporary `PuzzleBootState` between setup/splash and the puzzle (see the decisions above) and
  remove the legacy states from `Main.states`. The legacy files stay for the [cut](../backlog/space-shaping-legacy-cut.md).
- Update `SpaceShapingStateEditor`: remove the "Next puzzle" button with `puzzles[]`; keep preview, reset
  and the solved-cube debug button.
- **Exit:** Play `Game.unity` → splash → one puzzle renders at the authored Bubble placement; the live
  trajectory updates while shaping and the ship flies that trajectory on every completed pinch (no separate
  player commit action); reset restores the field and commands, no legacy round spawns; console clean.

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
  [HUD plan](../backlog/space-shaping-hud-validation.md); do not build it here.
- **Exit:** solve → result fires with 1–3 stars; a miss or crash keeps shaping and earns no stars; a crash
  with no flight still fires its result; budget exhaustion fires Failed; an edit after a result does not
  resurrect it.

## 3. Runtime puzzle contract

| Scope | Fields |
|---|---|
| Per puzzle | `id`, origin, destination, `sources`, `captureRadius`, `tmin`, `tmax`, `entropyBudget`, `zoneCentre`, `cubeSize`, brief (`translatable`, `moves`, `resizable`, `rotate`) |
| Global settings | `launchSpeed`, `stepSecs`, `nominalSpeed`, `planetRadiusMin`/`Max`, `corridorClearance`, `cubeSizeMin`/`Max`, `sourceStrengthScale`, `separationScale`, `lightSpeed`, `zoneFalloff`, scale envelope |
| Creator-only | seed, bake record, verification quality |

- `scalable` is always true; `moves` is authoring-only and never shown or enforced at runtime.
- **Launch is global** (decided 2026-10-27): `SpaceShapingSettings.launchSpeed` is the one given launch for
  every job; `PuzzleData.launchSpeed` is an authoring bake record and is not read at runtime
  ([resources](../../design/systems/resources.md): "Fuel = global").
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

- **The job-01 test is scale-only** (`moves: 1`, `translatable: 0`, `resizable: 0`, `cubeSize` at max) and
  must match the one-planet brief in [jobs](../../design/jobs.md). The exit path proves scale + flight + result,
  not move/resize; those claims need a matching [catalogue](../backlog/space-shaping-catalogue.md) bake.
- **The legacy `Init` is neutralised by unwiring, not deleting.** If a legacy state is left in `Main.states`,
  `HideAll()` runs at boot and the fanout subscribes to `UniverseView` — inert now, destructive once the
  campaign field exists.
- **Placement is easy to skip.** Without the room-anchored parent the puzzle renders at floor height, which
  reads as a broken puzzle rather than a placement bug.

## 7. Out of scope

- Campaign map, cube selector, aperture — [campaign map](../backlog/space-shaping-campaign-map.md).
- Entropy bar, clock window band, result/fail panel — [HUD](../backlog/space-shaping-hud-validation.md).
- Save, stars, reset semantics — [progression](../backlog/space-shaping-progression.md).
- Deleting the legacy flow or scene objects — [legacy cut](../backlog/space-shaping-legacy-cut.md).
- Authoring more than the one puzzle — [catalogue](../backlog/space-shaping-catalogue.md).
- Job card intro/TTS — moved to [campaign map](../backlog/space-shaping-campaign-map.md) M3.
- Multi-zone solutions — [multi-zone](../backlog/space-shaping-multizone.md).
