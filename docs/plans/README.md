# Plans

Plans are grouped by state. Move a plan between folders when its state changes.

## In progress

No plans in progress.

## Backlog

### Gameplay

| Plan | State | Notes |
|---|---|---|
| [Space shaping — spike](backlog/gameplay/space-shaping-spike.md) | Backlog (do first) | Smallest testable version: pure math + a tiny 3D debug scene to find the thrust-to-force ratio. Gates the rest. |
| [Space shaping — POC](backlog/gameplay/space-shaping.md) | Backlog (after the spike) | One ship, one destination. Move the sources; the derived spacetime grid bends the ship's path. Deterministic integrator, prediction line, commands/undo. |
| [Space shaping — reframing and variations](backlog/gameplay/space-shaping-future.md) | Backlog (design only) | Recorded: Map/Sculpt views, simultaneous multi-ship, sequential multi-destination, level generation (editor + seed creator), minimum-move scoring. |
| [Space shaping — integration into the game](backlog/gameplay/space-shaping-integration.md) | Backlog (design only) | Sketch: replace the current route/round/economy flow with the puzzle loop; scene, flow states, save/load, legacy removal. |

### Universe

| Plan | State | Notes |
|---|---|---|
| [Universe color scope](backlog/universe/universe-color-scope.md) | Backlog | Bubble-first spatial color isolation in one URP Lit-based shader; soft edge, no post-process; room scope future. |
| [Sector field (hub)](backlog/universe/universe-infinite-sector.md) | Backlog | Hub for the split slices below; bounded seeded field, no streaming. |
| [View frame (bubble window)](backlog/universe/universe-bubble-window.md) | Backlog | Slice 1: a box frame `{F, viewRadius, yaw}` moves over a fixed universe; `m = miniBubbleRadius / viewRadius`. Removes content reparenting. Owns frame culling; prereq for color scope; base for the Map/Sculpt frame. |
| [Sector generation and extent](backlog/universe/universe-sector-generation.md) | Backlog | Slice 2: pure C# bounded extent, sector grid, `GalaxyShape` math, tests. No scene. Base for the shaping lattice. |

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
