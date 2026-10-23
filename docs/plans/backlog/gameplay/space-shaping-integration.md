# Space Shaping — Integration into the Game

**Status:** backlog - design only (sketch). The largest and last slice; after the
[POC](../../done/space-shaping.md) proves the feel.

**Related:** [Spike](../../in-progress/space-shaping-spike.md) · [POC](../../done/space-shaping.md) · [Reframing and variations](space-shaping-future.md) · [View frame](../../done/universe-bubble-window.md) · [Sector generation](../../in-progress/universe-sector-generation.md)

**Wiring rule:** authored scene/prefab objects and serialized refs only. When the legacy
flow is removed, delete its scene objects and components — do not leave them disabled.

## 1. Goal

Turn the POC into the actual game: the puzzle loop **replaces** the current
route/round/economy loop, wired into `Game.unity`, the flow state machine, save/load,
and UI.

This is the phase the [POC](../../done/space-shaping.md) defers when it says removing the existing
route/hub/ship code is "a later phase once the feel is proven."

## 2. Key decision (open)

**Full replacement, or a mode alongside the current game?** This plan assumes **full
replacement** (the reframing: a new approach, nothing of the old loop survives). If it
becomes a mode, the scope below shrinks to adding a flow branch and does not delete the
legacy flow. Settle this before I0.

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

- The universe frame (Map/Sculpt) from
  [view frame](../../done/universe-bubble-window.md).
- Structured system layout from
  [sector generation](../../in-progress/universe-sector-generation.md).
- The planet prefab visual, the XR rig/hands, and the two-hand gesture.
- The existing route line/path-follower rendering conventions for the trajectory.

### 3.4 Scene and flow

- `Game.unity`: remove the legacy route/hub wiring; add the derived spacetime grid
  visualization, source editing, the prediction line, and the puzzle controller.
- Flow states: a puzzle lifecycle replacing `GameStartState` / `EditionState` /
  `NewRoundState` / `GameEndState` as needed. Follow `_Modules/docs/standards/modes.md`
  for what is a Flow state vs a mode.

### 3.5 Save / load

- Store: puzzle identity or seed, source state (planet positions, strengths, and any
  wells), and the per-puzzle score. The field and grid are derived.
- The legacy snapshot schema is dropped. Prefer reset-to-fresh over migration, as in the
  parked [planet route drawing](../../parked/planet-route-drawing.md) plan.

### 3.6 UI

- Per-puzzle score, reset, and next-puzzle affordances.
- Remove route/resource panels.

### 3.7 Level pipeline

- Editor-authored puzzles (primary) plus the seed-based creator (ship count, planet
  density, force variation) from
  [reframing §4](space-shaping-future.md).

## 4. Non-goals

- Multi-ship and multi-destination (see
  [reframing §3](space-shaping-future.md)); integration can land single-ship first.
- Streaming and galaxies beyond what Map View needs.
- Final art/audio polish.

## 5. Phases (sketch)

### I0 — Puzzle flow + one real puzzle

- Puzzle flow states and a single authored puzzle in `Game.unity`, replacing the current
  start/round/economy states for that path.

**Exit:** the game boots into one space-shaping puzzle and can be solved and scored.

### I1 — Map / Sculpt wiring

- Wire the frame as Map/Sculpt over the real scene; the derived grid visualization,
  source editing, and the prediction line in `Game.unity`.

**Exit:** the player selects a sector in Map View and shapes its sources in Sculpt View.

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
4. **Map View's role** — selection/navigation only, or does it carry the demand layer
   ("desire lines")? That layer is undesigned.
5. **Sequencing** — land single-ship integration before multi-ship, or together.
