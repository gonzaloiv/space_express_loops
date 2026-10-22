# Space Shaping — Architecture Refactor (POC → real structure)

**Status:** backlog - refactor plan. The POC works; this is the split into one class per
responsibility, before the feature grows into the game.

**Related:** [POC](space-shaping.md) · [Reframing and variations](space-shaping-future.md) · [Integration into the game](space-shaping-integration.md) · [View frame](../universe/universe-bubble-window.md)

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
