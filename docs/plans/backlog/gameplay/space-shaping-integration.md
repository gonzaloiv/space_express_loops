# Space Shaping — Integration into the Game

**Status:** next · space shaping · updated 2026-10-26 — design sketch. The largest and last slice;
**gated on Gate B** (the lens must prove the game first).

**Related:** Spike · POC · [Variations](space-shaping-future.md) · View frame · Sector generation

**Wiring rule:** authored scene/prefab objects and serialized refs only. When the legacy
flow is removed, delete its scene objects and components — do not leave them disabled.

## 1. Goal

Turn the POC into the actual game: the puzzle loop **replaces** the current
route/round/economy loop, wired into `Game.unity`, the flow state machine, save/load,
and UI.

This is the phase the POC defers when it says removing the existing
route/hub/ship code is "a later phase once the feel is proven."

## 2. Key decision (open)

**Full replacement, or a mode alongside the current game?** This plan assumes **full
replacement** (the reframing: a new approach, nothing of the old loop survives). If it
becomes a mode, the scope below shrinks to adding a flow branch and does not delete the
legacy flow. Settle this before I0.

## Decisions (2026-10-26)

- **Scope:** the **full run** — the level selector and **8–10 jobs** are in scope, not one puzzle.
- **Loop:** **no commit** — travel on every modification ([travel plan](../../in-progress/space-shaping-travel.md)).
- **Persistence:** save **player progression** (stars / cubes solved), **not** in-level state (§3.5).
- **Target:** Meta Quest 3S, **72 fps**.
- **UI:** the HUD sits **over the level** (world-anchored) (§3.6).

## 3. What integration means (scope)

### 3.1 Replace the game loop

- Puzzle by puzzle with a per-puzzle score, instead of rounds/letters/money.
- A puzzle lifecycle: present puzzle -> solve -> score -> next puzzle.

### 3.2 Remove the legacy flow

Delete (not disable) the pieces the reframing drops:

- Hubs/stations, ship grab, destination selection, `SplineContainerWrapper` /
  `SplineLegSampler`, `TravellerRouteRunner` loops.
- The round/letter/money economy: `EditionState`, `PlanetStore` letters,
  `ResourcePanel`, round spawning (`NewRoundState`, `RoundData`), `PlanetsSpawner`
  round placement.
- The legacy route panel and its edit flow.

### 3.3 Keep and reuse

- The one-view universe frame + the cube level selector (campaign menu) from
  [progression](../../../design/systems/progression.md) and the view frame.
- Structured system layout from the sector field.
- The planet prefab visual, the XR rig/hands, and the cube's two-hand scale gesture.
- The existing route line/path-follower rendering conventions for the trajectory.

### 3.4 Scene and flow

- `Game.unity`: remove the legacy route/hub wiring; add the cube/zone scale, the derived
  spacetime grid, the prediction line, the entropy bar (eco rating), and the puzzle
  controller.
- Flow states: a puzzle lifecycle replacing `GameStartState` / `EditionState` /
  `NewRoundState` / `GameEndState` as needed. Follow `_Modules/docs/standards/modes.md`
  for what is a Flow state vs a mode.

### 3.5 Save / load

- Store **player progression** — the cubes solved and their stars. **Not** the in-level state: a
  puzzle is regenerated from its seed, so a run is never resumed mid-flight.
- The legacy snapshot schema is dropped. Prefer reset-to-fresh over migration, as in the
  planet route drawing plan.

### 3.6 UI

- The entropy bar (eco rating), the window readout, reset, and next-puzzle affordances — **over the
  level** (world-anchored).
- Remove route/resource panels.

### 3.7 Level pipeline

- Editor-authored puzzles (primary) plus the seed-based creator (ship count, planet
  density, force variation) from
  [variations §3](space-shaping-future.md).

### 3.8 The sector field (level-selector substrate)

The level selector is a **field of cubes over the bounded sector field**
([progression](../../../design/systems/progression.md)); the sector field is the substrate for the
puzzle loop, not scenery.

- **Coupling.** `SectorShape` supplies a puzzle's seed layout; the level selector is the wide view,
  and a picked cube **scales up into the play space**.
- **Aperture is a trigger.** `viewRadius` is `1.2` in `Game.unity` and the wired field is
  deliberately small (`fieldRadius 1.8`). **Do not raise the aperture until the gameplay content
  comes from the field** (this integration): at a `12.5 m` aperture the current room-local content
  shrinks to a ~2 cm speck. The trigger is the content move, not a date.
- **Scope.** The color scope does not use `viewRadius`; out-of-frame content is grayed (an ellipsoid
  from the content bounds). The hide band is `SectorFieldSpawner.hideRadiusFactor = 1.5`; raise
  `fieldRadius` with `softSectorBudget` together.
- **Decor migration.** The `SilentPlanets*` pool should move into the field's ambient sectors rather
  than stay a separate feature.

## 4. Non-goals

- Simultaneous multi-ship (see
  [variations §2.1](space-shaping-future.md)); integration lands single-ship first. A
  sequential **loop route** (multi-destination) is in scope — see
  [vision](../../../design/vision.md) §3.
- Streaming and galaxies beyond what Map View needs.
- Final art/audio polish.

## 5. Phases (sketch)

### I0 — Puzzle flow + one real puzzle

- Puzzle flow states and a single authored puzzle in `Game.unity`, replacing the current
  start/round/economy states for that path.

**Exit:** the game boots into one space-shaping puzzle and can be solved and scored.

### I1 — Universe / level-selector wiring

- Wire the one-view universe frame and the cube level selector over the real scene; the
  derived grid visualization, the cube/zone scale, and the prediction line in `Game.unity`.

**Exit:** the player picks a cube, scales it up into the play space, and shapes its zone.

### I2 — Save / load + score

- Puzzle snapshot (identity/seed, positions, multipliers, score); reset-to-fresh for
  legacy saves.

**Exit:** solving, scoring, and restoring a puzzle round-trips.

### I3 — Remove the legacy flow

- Delete the hub/station/route/round/economy code and scene objects listed in §3.2.

**Exit:** no legacy route/hub/round content remains in the game flow.

### I4 — Level pipeline

- Editor puzzle authoring plus the seed-based creator; puzzle selection.

**Exit:** a library of puzzles is authored and selectable.

### I5 — Polish

- Onboarding, feedback, comfort, tuning, tests.

## 6. Open questions

1. **Replacement vs mode** (§2) — the gating decision.
2. **What survives of the old game** — score/progression/menu, or a clean slate?
3. **Snapshot policy** — reset legacy saves (assumed) or migrate.
4. **Level selector's role** — selection only (pick a cube, solve, back out), or does it
   also carry a "demand layer"? That layer is undesigned.
5. **Sequencing** — land single-ship integration before multi-ship, or together.
