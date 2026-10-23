# Plans

Plans are grouped by state. Move a plan between folders when its state changes.

## In progress

| Plan | State | Notes |
|---|---|---|
| [Space shaping — spike](in-progress/space-shaping-spike.md) | In progress | Tier 0 math (`SpacetimeField`, `TrajectoryIntegrator`) and the Tier 1 debug scene (`SpaceShapingPOC`) built; the launch-to-curvature ratio band and go/no-go decision are still open. |
| [Sector generation and extent](in-progress/universe-sector-generation.md) | In progress | G0: pure C# bounded extent, sector grid, `GalaxyShape` math, tests. No scene. Substrate for the shaping lattice; re-baseline `viewRadius` against the Map/Sculpt apertures. |

## Backlog

### Gameplay

| Plan | State | Notes |
|---|---|---|
| [Space shaping — POC](backlog/gameplay/space-shaping.md) | Backlog (after the spike) | One ship, one destination. Move the sources; the derived spacetime grid bends the ship's path. Deterministic integrator, prediction line, commands/undo. |
| [Space shaping — reframing and variations](backlog/gameplay/space-shaping-future.md) | Backlog (design only) | Recorded: Map/Sculpt views, simultaneous multi-ship, sequential multi-destination, level generation (editor + seed creator), minimum-move scoring. |
| [Space shaping — integration into the game](backlog/gameplay/space-shaping-integration.md) | Backlog (design only) | Sketch: replace the current route/round/economy flow with the puzzle loop; scene, flow states, save/load, legacy removal. |
| [Space shaping — architecture refactor](backlog/gameplay/space-shaping-architecture.md) | Backlog (design only) | Split the POC driver into one class per responsibility (domain / views / controllers / composition root) before it becomes the game. |

### Universe

| Plan | State | Notes |
|---|---|---|
| [Sector field (hub)](backlog/universe/universe-infinite-sector.md) | Backlog | Hub for the split slices below; bounded seeded field, no streaming. Slice 1 (view frame) done; slice 2 is in progress. |

### Presentation and companion

| Plan | State | Notes |
|---|---|---|
| [TTS voice panel](backlog/presentation/tts-voice-panel-personality.md) | Backlog | Design only; camera-operator companion behavior for the TTS panel. |

## Parked

Not current. Kept for reference; they collide with the
[space shaping](backlog/gameplay/space-shaping-future.md) framing. See
[parked](parked/README.md).

| Plan | Notes |
|---|---|
| [Planet route drawing](parked/planet-route-drawing.md) | Drawn routes replaced by shaping space. |
| [Ambient round activation](parked/universe-ambient-round-activation.md) | A management game, not the physics puzzle. |

## Done

See [done](done/README.md) for completed plans.
