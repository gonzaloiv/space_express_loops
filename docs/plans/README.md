# Plans

Plans are grouped by state. Move a plan between folders when its state changes.

## In progress

No plans in progress.

## Backlog

### Universe

| Plan | State | Notes |
|---|---|---|
| [Universe color scope](backlog/universe/universe-color-scope.md) | Backlog | Bubble-first spatial color isolation in one URP Lit-based shader; soft edge, no post-process; room scope future. |
| [Infinite sector (hub)](backlog/universe/universe-infinite-sector.md) | Backlog | Hub for the split slices below; shared context and order. |
| [View frame (bubble cube)](backlog/universe/universe-bubble-window.md) | Backlog | Slice 1: a cube frame `{F, viewRadius, yaw}` moves over a fixed universe; `m = miniBubbleRadius / viewRadius`. Removes content reparenting. Prereq for streaming and color scope. |
| [Sector generation and extent](backlog/universe/universe-sector-generation.md) | Backlog | Slice 2: pure C# extent/focus/rebase, sector grid, `GalaxyShape` math, tests. No scene. |
| [Sector streaming](backlog/universe/universe-sector-streaming.md) | Backlog | Slice 3: pooled streaming, pan integration, floating-origin rebase, recenter, room-view visibility, central galaxy. |
| [Ambient round activation](backlog/universe/universe-ambient-round-activation.md) | Backlog | Slice 4: rounds activate existing decor planets; pool growth, intergalactic selection, persistence. |
| [Universe galaxies](backlog/universe/universe-galaxies.md) | Backlog | Galaxy groups, level/round creation, two-hand group transform, layout response. Still to be analysed. |

### Gameplay

| Plan | State | Notes |
|---|---|---|
| [Planet route drawing](backlog/gameplay/planet-route-drawing.md) | Backlog | Design only; planet-to-planet drawn loops, round-granted ships, optional pencil. |

### Presentation and companion

| Plan | State | Notes |
|---|---|---|
| [TTS voice panel](backlog/presentation/tts-voice-panel-personality.md) | Backlog | Design only; camera-operator companion behavior for the TTS panel. |

## Done

See [done](done/README.md) for completed plans.
