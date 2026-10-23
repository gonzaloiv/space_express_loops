# Space Shaping — POC (One Ship, One Destination)

**Status:** done - POC **validated and closed**. One ship, one destination; sources -> field
-> grid -> trajectory; bounded moves and grab/scale work on device; the lab runs on the
target architecture. Carried forward (not POC blockers): ghost / pre-move preview,
move-range visualization, final tuning, and the lattice presentation (curved lines done;
mesh deferred) — see the [architecture refactor](space-shaping-architecture.md)
and [integration](../backlog/gameplay/space-shaping-integration.md) plans.

**Related:** [Spike (smallest testable version)](space-shaping-spike.md) · [Reframing and variations](../backlog/gameplay/space-shaping-future.md) · [Integration into the game](../backlog/gameplay/space-shaping-integration.md) · [View frame (bubble window)](universe-bubble-window.md) · [Sector generation](universe-sector-generation.md) · [Parked: planet route drawing](../parked/planet-route-drawing.md)

**Wiring rule:** the grid visualization, the prediction line, the ship, and the sources are authored in the scene/prefab and referenced through serialized fields. No runtime `AddComponent`, `GetComponent`, `FindObjectsByType`, or hierarchy/mesh/asset creation. Follow the SerializeField wiring rules in the project `AGENTS.md`.

## 1. Goal

Prove the core feel: the player moves the **sources** (planets) so the ship's path, bent
by the resulting spacetime, reaches the destination.

One ship, one origin (planet `1`), one destination (planet `2`). No multi-ship, no level
generation, no final art. The deliverable is "moving a source visibly deforms where the
ship goes, the grid shows why, and I can tune a reachable-but-not-trivial path."

The launch-to-curvature ratio is found in the separate
[spike](space-shaping-spike.md); this plan starts once the spike passes. This is the
first slice of the reframing in
[Space shaping — reframing and variations](../backlog/gameplay/space-shaping-future.md).

## 2. Non-goals

- More than one ship.
- Routes with more than one destination (A -> B -> C).
- Map/Sculpt view split (the POC uses one working view).
- Level generation, minimum-move scoring, or a solver.
- Multiple regions, galaxies, or streaming.
- The legacy economy (letters, money, rounds, `PlanetStore`, `EditionState`). This is a
  new approach: puzzles scored one by one. Nothing from the old loop is reused.
- Ship-to-ship collisions (a later addition; not needed for one ship).
- Final art, VFX, audio, or a player-facing UI beyond a debug reset.
- Persistence (the POC may run entirely from a scene configuration).
- Removing the existing route/hub/ship code; that is the separate
  [integration plan](../backlog/gameplay/space-shaping-integration.md), once the feel is proven.

## 3. Existing facts

- Planets are placed by rejection sampling against the physical room
  (`PlanetsSpawner` -> `MrukRoomLocalPlacement.GetValidLocalPosition`), not on a lattice.
  `PlanetData.localPosition` is the saved, content-local position.
- There is no physics: `TravellerPathFollower` interpolates a traveller along stored
  `RouteLeg.positions`. Ships never integrate a field today.
- Routes and route lines are content-local, so they already survive the content
  transform `D`.
- The [view frame](universe-bubble-window.md) gives the box frame
  `{F, viewRadius, yaw}` this POC uses as the Sculpt box. It lands as its own slice; the
  POC consumes it.
- `SeededRandom` (`DigitalLove.Global`) is the deterministic PRNG for any seeding.

## 4. Core model

### 4.1 Relativity model

- Spacetime is a **scalar potential** `Φ` produced by **sources**:
  - **planets** — strength derived from radius, with a per-planet multiplier from the
    level seed;
  - **wells / bumps** (optional) — a sparse extra source that curves space independently
    of a planet (attract or repel). This is the "pull space up over the sofa edge" tool.
- There is **no background gravity**; `Φ` is the sum of the sources only.
- The **grid is derived**, never edited: grid vertex height = `-Φ` (a rubber sheet),
  and/or a geodesic flow field. It is a pure function of the source state: moving,
  scaling, or adding a source deforms the grid automatically.
- The grid is a **read-out** and a **grab surface**: grabbing a grid point selects the
  nearest source, and the grab edits that source. It is never vertex-sculpted.

### 4.2 Ship motion

```text
accel = -grad( Φ )            (+ thrustDir * thrustAccel if the thrust model is used)
```

- The ship follows the curved space. The Newtonian gradient is the practical stand-in
  for a geodesic; the relativity is the metaphor and the visual.
- **Launch model** (from the spike): `Coast` (initial velocity only — the inertial /
  geodesic feel, primary) or `Thrust` (constant force — a rocket).
- No friction, no drag.
- Integrated with a **fixed-step, fixed-count** integrator. No Unity `Rigidbody`, no
  PhysX, no variable `Time.deltaTime`.
- The simulation runs in **content-local space and is scale-independent**: relative
  vectors (differences) only, so content scaling `D` and any floating-origin rebase do
  not change the result.
- In an empty field the path is a straight line: the "expected route".

### 4.3 Lifecycle — constant trajectory deformation

```text
Drag     -> a ghost / pre-move preview shows the proposed source move; the path is
            not recomputed continuously
Commit   -> on release the move is validated and applied as one command, then the
            trajectory is recomputed once (the trajectory is always the current one)
Arrive   -> the trajectory passes within the destination capture radius
Crash    -> the trajectory enters a planet's collision radius
```

- There is **no launch and no fail cost**. The trajectory is always the current one;
  crash and miss are **UI states**, not a run that can be lost.
- Recompute happens **per committed edit** (one command, on release), not per frame.
- Success is a **pass**, not a stop: crossing the destination capture radius.
- Planets are both helpers and hazards: a source that bends the path, and a solid radius
  that reads as a crash in the UI.
- The origin planet's collision is ignored until the ship has cleared it.

### 4.4 Origin and destination

- The origin and destination are planets marked with a number: `1` is the ship's origin,
  `2` is the destination. The destination is just the target the ship must be directed
  to, not a special element.
- The ship launches from planet `1` toward planet `2`; the field decides the path.

### 4.5 Sources

| Source | State |
|---|---|
| Planet | position, radius -> strength, per-planet multiplier |
| Well / bump | position, strength, radius, sign |

- Planets are required content. Wells are optional; the POC may ship without them and
  add them as the "deform space without a planet" tool.

### 4.6 Editing and commands

- Grab a planet -> move it or change its mass. Grab/place a well -> add, move, or remove.
- The **path is never editable**; only the sources are.
- **Propose -> validate -> apply:** a proposed edit is validated (sources inside the
  frame; planets keep min-separation). If it fails, no transform happens.
- A **ghost / pre-move preview** shows the proposed result during the drag; the release
  is the single point where validation, apply, and recompute happen.
- Every accepted edit is a **command** with `Apply` and `Undo`: `MovePlanet`,
  `SetPlanetStrength`, `AddWell`, `MoveWell`, `RemoveWell`.
- The player undoes via the history stack. The same command type is what the system will
  use later to build levels from a solved arrangement. Do not model player and system
  edits differently.

### 4.7 Mass editing (size = mass)

- For the first version, **size = mass**: the scale gesture changes the planet's size and
  its strength together. A bigger planet pulls harder and is a bigger crash radius.
- Decouple the curves: visual radius follows the gesture, mass follows an authored curve
  (`mass ∝ r` or a clamped curve) — never `r³`, which is far too sensitive.
- Prefer **discrete steps** (e.g. 3 sizes) over continuous: countable, legible, and it
  gives clean move counts for scoring.
- **Arbitration (no new UI):** one hand on a planet translates; two hands on the same
  planet scale (size/mass); two hands in open space change the aperture (Map <-> Sculpt).
- Future: a **small heavy planet** (mass independent of size) is a nice later element;
  out of scope for the POC.

## 5. Component map

New feature assembly `DigitalLove.Game.SpaceShaping` (naming follows the existing
`DigitalLove.Game.*` features; confirm at S0). Pure math types have no Unity scene
dependency and are unit-testable.

| Type | Responsibility |
|---|---|
| `SpacetimeField` | Pure C# potential + gradient from the sources. |
| `TrajectoryIntegrator` | Fixed-step, fixed-count deterministic integration; returns sampled points + outcome. |
| `ShipLaunch` | Coast/thrust config; runs the integrator; classifies Arrive / Crash / Miss. |
| `SourceEditCommand` | `Apply`/`Undo` for one source edit (move planet, set strength, add/move/remove well). |
| `SourceEditController` | Grab, propose, validate, apply, undo stack; owns the edit session. |
| `SpacetimeGridView` | Authored grid; derived rubber-sheet / geodesic-flow visualization; grab surface mapping to the nearest source. |
| `TrajectoryPredictionView` | Authored `LineRenderer`; draws the live predicted path and the arrive/crash UI state. |
| `SpaceShapingDebug` | Editor/debug reset and preset arrangements. |

Reuse: `UniverseView` + the frame for the Sculpt box; `LineRendererWithEdges` /
`TravellerPathFollower` conventions for drawing/following the sampled path;
`PlanetData.localPosition`; `SeededRandom`.

Assembly note: `SpaceShaping` may reference `Nodes`, `Spaceships`, and the frame view;
`Nodes` and `Levels` must not reference `SpaceShaping`.

## 6. Phases

The first step is the separate [spike](space-shaping-spike.md): pure math plus a tiny 3D
debug scene to find the launch-to-curvature ratio and the launch model. It gates
everything below — do not build the grid visualization or the frame wiring until the
spike passes.

### S1 — Source editing + derived grid

- A POC arrangement with **several planets** so the sources visibly influence each other
  (one planet does not show the interaction).
- `SpacetimeGridView` over the Sculpt frame: a derived rubber-sheet / flow read-out that
  responds to the sources; grabbing a point selects the nearest source.
- `SpacetimeField` drives the grid; `SourceEditController` proposes, validates, applies;
  `SourceEditCommand` + undo stack; a ghost/pre-move preview during a drag.
- `TrajectoryPredictionView` recomputes on each committed edit (on release), not per
  frame.
- Optional: the well/bump source for curvature without a planet.

**Exit:** committing a source move deforms the grid and recomputes the path; invalid moves
are rejected; undo is clean; a valid arrangement reaches the destination.

### S2 — Tuning and legibility

- Tune the launch-to-curvature ratio, capture radius, collision radius, step size, and
  the grid read-out.
- Make the grid readable and the prediction line clear; add a faint "expected route"
  (the empty-field straight line) as a reference.
- Verify determinism end to end (prediction == the current trajectory).

## 7. Test plan

### Manual

1. With no sources, the path is a straight line along the launch direction.
2. One planet bends the path; moving the planet changes the path predictably.
3. The grid deforms when a source moves; it is derived, not hand-edited.
4. Grabbing a grid point selects the nearest source and edits it.
5. A trajectory that enters the destination capture radius shows Arrive.
6. A trajectory that enters a planet's collision radius shows Crash (UI only).
7. The trajectory keeps deforming live while a source is dragged; no run is lost.
8. A move that violates min-separation or the frame is rejected; sources do not change.
9. Undo restores the previous sources exactly.
10. Reset returns to the authored arrangement.

### Automated

- Integrator determinism: same input -> identical points across runs.
- Straight-line with an empty field.
- Monotonic bend: increasing a source's strength bends the path further toward it.
- Outcome classification: Arrive, Crash, Miss boundaries.
- Grid read-out matches `Φ` at the sampled points.
- Command apply/undo round-trip.
- No per-frame allocations and no LINQ in new code.

## 8. Defaults

| Parameter | Default |
|---|---|
| `launchModel` | `Coast` (from the spike; `Thrust` is the alternative) |
| `launchSpeed` | tuned (coast) |
| `thrustAccel` | tuned (thrust model only) |
| `sourceStrength` | derived from radius |
| `sourceMultiplier` | from the level seed (physics variation) |
| `massEditGesture` | scale (size = mass) |
| `massSteps` | `3` (discrete sizes/masses) |
| `massCurve` | `mass ∝ r` (never `r³`) |
| `backgroundGravity` | none |
| `wellStrength` | tuned; sign allows attract or repel |
| `wellRadius` | tuned; grid-cell scale |
| `captureRadius` | `1.5 x` destination radius |
| `collisionRadius` | `1.0 x` planet radius |
| `stepSecs` | `1/120 s` |
| `maxSteps` | `1200` (~10 s) |
| `gridResolution` | tuned (e.g. `16 x 16`) over the frame box |
| `gridGrabRadius` | generous; on a dedicated interaction layer |
| `recomputeOn` | committed edit (on release); not per frame |
| `minPlanetSeparation` | `1.2 x` sum of planet radii |
| `planetCount` | from the level seed |
| `expectedRouteVisible` | `true` (faint empty-field straight line) |
| `targetPlatform` | Quest (single target; no cross-device determinism work) |

`launchSpeed`, the source strength scale, and `captureRadius` are the three knobs that
decide whether this is a puzzle or a lottery. Tune them together in S2.

## 9. Decisions confirmed

- **Relativity framing.** Mass curves spacetime; the grid is **derived**, never edited.
  The player edits the **sources**.
- **No lattice vertex sculpting.** Grabbing a grid point selects the nearest source.
- **New approach.** No legacy economy or loop is reused; puzzles are scored one by one.
- **Route shape.** One origin and one destination, marked `1` and `2` on the planets.
- **Source strength from radius**, with per-planet variation from the level seed.
- **Size = mass for now.** The scale gesture edits size and strength together, in
  discrete steps; mass follows a gentle curve (never `r³`). A small heavy planet (mass
  independent of size) is a later possibility.
- **No background gravity**; only the sources' own potential.
- **Constant trajectory deformation.** Crash is a UI state; there is no run to lose.
- **Launch model** (`Coast` vs `Thrust`) is decided in the spike.
- **Optional wells/bumps** for curvature independent of a planet.
- **Scale-independent, content-local simulation** using relative vectors only.
- **Quest only** for now.
- **Sculpt View** is the working name for the micro view (replaces the earlier
  "Edition View", which collided with `EditionState`).
- **Ship-to-ship collisions** are a later addition, not required for v1.

## 10. Risks

| Risk / decision | Decision |
|---|---|
| Chaos near sources | Keep the potential soft; use a generous capture radius so a good-enough pass counts. |
| Several planets needed | Use several planets in the POC; one planet does not show the interaction between sources. |
| Readability | The forces read best at a bigger view; that is the reason for the two views (Map reads, Sculpt edits). |
| Recompute cost | Recompute per committed edit (on release), not per frame; a ghost/pre-move preview stands in during a drag. |
| Determinism | Own fixed-step integrator; never Unity `Rigidbody`/PhysX; never `Time.deltaTime`. |
| Prediction trust | The prediction and the trajectory are one integrator and one code path. |
| Grid fidelity | The grid is a derived read-out; it must match `Φ`, not fake it, or the player stops trusting it. |
| VR micro-interaction | Grab a grid point to select a source; do not sculpt vertices. |
| Invalid edits | Propose -> validate -> apply; reject overlap and out-of-frame moves. |
| Reachability | The POC guarantees a baseline: with sources out of the way the straight line reaches. |
| Data | Source state only (positions, strengths, wells); the field and grid are derived. |
| Scale | Simulate in content-local space with relative vectors; `D` scaling and rebase are no-ops. |
| Scope creep | The POC stops at one ship / one destination; everything else is in the future plan. |

## 11. Open questions

1. **Grid read-out form.** Rubber sheet (height = `-Φ`), a geodesic flow field, or both?
   The flow field doubles as the Map View "desire lines". Default: rubber sheet first,
   flow field later.
2. **Wells in v1.** Include the well/bump source in the POC, or planets-only first?
   Default: planets-only; wells in a follow-up.
3. **View for the POC.** Use the [frame](universe-bubble-window.md) as the
   Sculpt box directly, or a stand-in if the frame slice is not wired yet. Default: use
   the frame.
4. **Crash rendering.** How a crash reads in the UI (path color, marker) is a tuning
   detail; keep it minimal for the POC.
