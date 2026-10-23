# Space Shaping — Architecture Refactor (POC → real structure)

**Status:** backlog - **first pass done.** The lab scene (`SpaceShapingPOC`) was rebuilt on
the target architecture: `Domain/` (Sources / Spacetime / Ship / Puzzle), `Data/`
(`PuzzleData`, `PuzzleSource`, `SpaceShapingSettings`), `Views/` (`SpaceSourceView`,
`SpacetimeGridView`, `TrajectoryPredictionView`, `SpaceShapingView`), `Flow/`
(`SpaceShapingState`, `SpaceShapingFlow`). `SpaceShapingPocDriver` is deleted. Remaining:
the game integration (this shape drops into the real flow).

**Related:** [POC](space-shaping.md) · [Reframing and variations](space-shaping-future.md) · [Integration into the game](space-shaping-integration.md) · [View frame](../../done/universe-bubble-window.md)

## 1. Why

`SpaceShapingPocDriver` currently does five jobs at once: source sync, constraints, field
build, simulation, and two renderers (the lattice grid and the trajectory line). It is
~245 lines and growing. The project rules ("orchestrators orchestrate", one class per
responsibility, < 250 lines per type, < 25 lines per method) say this must split before
it becomes the real feature.

This document is the target shape and the migration path. It is a plan, not code.

## 2. Current state

- **`SpaceShapingPocDriver`** (MonoBehaviour, ~245 lines) owns everything: serialized
  `origin`, `destination`, `planets`, `pathLine`, `container`, `gridLines` + params, and
  the methods `Update`, `Simulate`, `BuildField`, `Draw`, `UpdateGrid`,
  `ConstrainSources`, `ResolveOverlaps`, `CaptureHomes`, `ResetSources`, `HasMoved`,
  `Snapshot`.
- **Already extracted (pure C#):** `SpaceSource`, `SpacetimeField`, `LatticeGrid`,
  `TrajectoryIntegrator`, `ShipLaunch`, `LaunchModel`, `TrajectoryOutcome`,
  `TrajectoryResult`, `TrajectorySettings`, `LaunchSettings`.
- The scene works in **container-local space** (`Space` container; positions via
  `InverseTransformPoint` / `TransformPoint`; grid + path LineRenderers are local space).

## 3. Target architecture

Four layers, dependencies point inward:

```text
  Composition root (scene)
    SpaceShapingPuzzle            <- wires everything, thin orchestrator
        |
  Controllers
    SpaceSourceEditor             <- input (grab) -> commands
        |
  Views (MonoBehaviour, render only)
    SpacetimeGridView             <- lattice read-out
    TrajectoryPredictionView      <- path + outcome colour
    SpaceSourceView               <- Transform <-> source binding
        |
  Domain (pure C#, no scene)
    SpaceSourceSet                <- sources + constraints (single writer)
    SourceEditCommand / History   <- move/scale + undo
    SpacetimeField                <- potential / acceleration / pull
    LatticeGrid                   <- node + warp math
    TrajectoryIntegrator          <- fixed-step integration
    ShipLaunch                    <- launch model + outcome
```

Rule: domain types never reference a view; views never mutate domain state; only the
composition root sequences calls.

## 4. Class responsibilities

| Type | Layer | Owns | Responsibility |
|---|---|---|---|
| `SpaceSource` | Domain | — | One source: position, radius, strength (container-local). |
| `SpaceSourceSet` | Domain | source state | The list of sources; the **only** writer of source positions/scales. Clamp-to-home/range and overlap separation live here. |
| `SpaceSourceView` | View | a Transform | Binds one `Transform` to one source; reads its local position/scale; writes a source back on command. |
| `SourceEditCommand` | Domain | — | One edit (move/scale): `Apply` / `Undo` against a `SpaceSourceSet`. |
| `SourceEditHistory` | Domain | undo stack | Pushes commands, `Undo()`. |
| `SpacetimeField` | Domain | — | `Potential`, `Acceleration`, `Pull` from a source set. |
| `LatticeGrid` | Domain | — | Lattice node position + warp by the field. |
| `TrajectoryIntegrator` | Domain | — | Fixed-step, deterministic integration -> `TrajectoryResult`. |
| `ShipLaunch` | Domain | — | Coast/thrust config; classifies Arrive/Crash/Miss. |
| `SpacetimeGridView` | View | 48 line renderers + grid params | Renders the warped lattice from a `SpacetimeField`. |
| `TrajectoryPredictionView` | View | the path line renderer | Renders a `TrajectoryResult` and its outcome colour. |
| `SpaceSourceEditor` | Controller | selection + history | Turns grab/scale input into `SourceEditCommand`s (propose -> validate -> apply). |
| `SpaceShapingPuzzle` | Composition root | the wiring | Per update: sync sources -> constrain -> build field -> simulate -> render. ~40 lines. |

## 5. Data flow (one update)

```text
SpaceSourceView.SyncToModel()        // Transform -> SpaceSourceSet (container-local)
SpaceSourceSet.Constrain()           // clamp to home/range, resolve overlaps
SpacetimeField field = set.BuildField()
TrajectoryResult r = launch.Simulate(field, origin, destination, settings, integrator)
gridView.Render(field)               // lattice read-out
predictionView.Render(r)             // path + outcome colour
SpaceSourceView.ApplyToScene()       // SpaceSourceSet -> Transform (after a constrain/edit)
```

Recompute stays **per committed edit**, not per frame (see the POC's lifecycle): the
editor applies a command on release; the puzzle recomputes once.

## 6. State ownership

- **`SpaceSourceSet` is the single writer** of source state. Views read snapshots;
  controllers return intentions; the puzzle applies them.
- Source positions/scales are **container-local**; `SpaceSourceView` owns the
  Transform <-> local conversion for its one planet.
- Home positions/scales (for the move range and reset) live on `SpaceSourceView`, not the
  orchestrator.

## 7. Scene wiring changes

- Add **`SpacetimeGridView`** on the `SpacetimeGrid` object (holds the 48 line renderers +
  `gridMin/Max/count/depthScale/maxDisplacement`).
- Add **`TrajectoryPredictionView`** on the `SpaceShaping POC` object (holds the path line).
- Add a **`SpaceSourceView`** on each of `PlanetA` / `PlanetB` (holds its home + range).
- **`SpaceShapingPuzzle`** keeps `origin`, `destination`, `container`, the two views, and
  the `SpaceSourceView` list. The old `SpaceShapingPocDriver` is deleted once migrated.

## 8. Migration steps

1. Add the domain types (`SpaceSourceSet`, `SourceEditCommand`, `SourceEditHistory`) with
   EditMode tests; no scene impact.
2. Add `SpacetimeGridView` + `TrajectoryPredictionView` and move the rendering code from
   the driver into them.
3. Add `SpaceSourceView` and move `CaptureHomes` / reset / local<->world conversion into it.
4. Rewire the scene onto the new components (via MCP): add the components, move the
   serialized refs (grid lines -> grid view; path line -> prediction view; planets ->
   source views).
5. Reduce the driver to `SpaceShapingPuzzle` (the orchestrator) and delete the old driver.
6. Verify the scene behaves identically, then save.

Each step is independently verifiable; the scene keeps working after every step.

## 9. Testing

- **Domain (EditMode):** field potential/acceleration, integrator determinism, lattice
  node/warp, constraint clamp + separation, command apply/undo round-trip.
- **Scene:** the POC's existing checks still pass (path bends/arrives/crashes, grid
  deforms, move range clamps, container scale applies).
- No new runtime allocations per frame; no LINQ (project rule).

## 10. Open questions

1. **Source state home:** do sources live purely in `SpaceSourceSet` (Transforms only
   present the result), or do Transforms stay the source of truth and the set is a
   snapshot? Current lean: the set is the model; Transforms are the presentation.
2. **Grid view input:** does `SpacetimeGridView` receive a `SpacetimeField` (lean: yes) or
   build one from a source set itself?
3. **Naming:** `SpaceShapingPuzzle` (the solved puzzle) vs `SpaceShapingSession` (one
   attempt) vs `SpaceShapingLevel`. The future plan uses "puzzle"; confirm.
4. **Editor input:** for now the editor drags Transforms; `SpaceSourceEditor` will take
   grab events from the hand interactor later. Keep the same command path for both.

## 11. Known follow-ups (POC findings)

Recorded from the working POC; not blocking, but improve these in later versions.

### Scaling interaction feel
- Two-hand scale uses Meta's `GrabFreeTransformer`, which also **rotates**. Harmless
  while planets are spheres (only `localScale` feeds the field), but it will matter once
  planets are non-spherical or textured. Eventual fix: a **scale-only** transformer.
- Scaling is **continuous and clamped** to `[minScale, maxScale]`; it does not snap to the
  discrete mass steps. Later: snap to ~3 steps so moves are countable for scoring.
- The constraint is applied **on change**, not on release, so an out-of-range grab can
  feel sticky at the boundary. Later: constrain on release through `SpaceSourceEditor`.
- No grab/scale **haptics or highlight** feedback, and no **ghost / pre-move preview**.

### Arrival visual
- The trajectory stopped at the **capture-sphere entry**, which read as ending "under" the
  destination. Fixed by appending the destination point on `Arrive`. Later: review
  `captureRadius` against the destination's visual radius.

### Line width
- Lines are container-scaled (proportional), which keeps the editor look. A
  **constant on-screen width** option would need per-line compensation that preserves the
  grid-vs-path ratio (not one shared value).

### Grab setup (as built)
- Planets use Meta `HandGrabInteractable` (pinch, no grab poses) on **layer 12** (the
  ship's grabbable layer) with `Rigidbody` (kinematic) + `Grabbable`.
- `Grabbable._transferOnSecondSelection` must be **false** for two-hand scale to work
  (the ship has it true, which is why a straight copy did not scale).

## 12. Integration-aware revisions

Reading the target with the game integration in mind changes a few things.

### Rethink
- **The composition root is the Flow, not the puzzle.** The project rules say the
  composition root is the level/flow, which observes the views and fans state into content
  systems. So split the puzzle:
  - `SpaceShapingPuzzle` = the **definition** (immutable: source homes/ranges, origin,
    destination, seed, solution move count).
  - `SpaceShapingSession` = the **attempt** (mutable domain state: current source
    positions/scales, move count).
  - A **Flow state** (`MonoState`) owns the session, binds the views, and sequences
    present -> solve -> score -> next.
- **Map/Sculpt is a Mode, not Flow states.** Per `_Modules/docs/standards/modes.md`, a
  view/camera/input mode is a Mode (one owner, one enum); only gameplay progression (the
  puzzle lifecycle) is a Flow state.
- **Recompute on commit, not per frame.** The POC's `Update`/`HasMoved` polling is a lab
  convenience. In the game the session recomputes when an edit commits; the ghost /
  pre-move preview is a separate cheap path.
- **Definition vs session** (closes the naming open question).

### Close
- **Naming:** `SpaceShapingPuzzle` (definition) + `SpaceShapingSession` (attempt).
- **Snapshot:** definition id/seed + source state + move count; domain-owned and
  serializable (primitives / `Vector3` only).
- **Grab input path:** planet interactable -> `SpaceSourceView` (select/release) ->
  `SpaceSourceEditor` (command) -> `SpaceShapingSession.Apply` -> recompute -> views
  render. The domain never sees the interactable.
- **Frame:** the frame provides the content-local space; the domain stays content-local
  and the views convert.
- **Wiring:** the session is wired by serialized refs from the Flow state (not DI),
  matching the existing Flow pattern.

### Risks
- **Scene churn:** moving the `gridLines` / `pathLine` / `planets` refs from the driver to
  the views; do it in the lab scene first.
- **Removing the legacy flow** (rounds/letters/economy) breaks `Game.unity`; land the new
  flow behind a switch first, then delete.
- **Domain purity:** the session must not reference views or the Meta interactable.
- **Determinism / save:** the session must round-trip through the snapshot without scene
  state.
- **Level-gen / solution format** must be settled before scoring (the definition carries
  the solution move count).
- **Live vs commit feedback:** the ghost preview must not run the full integrator every
  frame at scale.

## 13. First pass: rebuild the POC scene on the target architecture

Goal: replace the monolithic driver with the layered structure **in the lab scene**, keeping
the existing Flow pattern so it drops into the game later.

### Reuse (do not reinvent)
- **`LevelData`** (`DigitalLove.Casual.Levels`) stays the level-definition base. The puzzle
  definition extends it: **`SpaceShapingPuzzleData : LevelData`** — authored puzzle (id,
  source homes/ranges/scales, origin, destination, limits, launch settings, solution move
  count).
- The **Flow** (`MonoState` + `StateMachine`) stays the composition root: a
  **`SpaceShapingState : MonoState`** owns the session and binds the views.

### Domain (pure C#, no scene)
- **`SpaceShapingSetup`** — immutable values from the data (origin, destination, source
  homes/ranges/scales, `SourceLimits`, launch settings, capture radius).
- **`SpaceShapingSession`** — the attempt: source positions/scales + move count;
  `Constrain`, `BuildField`, `Simulate`, `Snapshot`/`Restore`.
- **`SourceEditCommand`** + **`SourceEditHistory`** — move/scale with undo.
- Existing: `SpaceSource`, `SpacetimeField`, `LatticeGrid`, `TrajectoryIntegrator`,
  `ShipLaunch`, `SourceConstraints`, `SourceLimits`.

### Views (MonoBehaviour, render only)
- **`SpaceSourceView`** — one planet: home/range, `Transform` <-> source, select/release.
- **`SpacetimeGridView`** — the lattice read-out; `Render(field)`.
- **`TrajectoryPredictionView`** — the path + outcome colour; `Render(result)`.

### Flow
- **`SpaceShapingState : MonoState`** — `Enter`: load the session from the data + source
  views, render, listen for edits. `Exit`: stop.
- A thin container object holds the scene refs (source views, grid view, prediction view,
  container) so the state stays sequencing-only.

### Scene layout (lab)
- `Space` container: `Origin`, `Destination`, `PlanetA`, `PlanetB` (each with
  `SpaceSourceView`), `SpacetimeGrid` (`SpacetimeGridView`), the path line
  (`TrajectoryPredictionView`).
- A `StateMachine` + `SpaceShapingState`.
- `SpaceShapingPocDriver` is **deleted** once migrated.



