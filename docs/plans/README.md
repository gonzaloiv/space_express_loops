# Plans

Plans are grouped by state. Move a plan between folders when its state changes.

## In progress

| Plan | State | Notes |
|---|---|---|
| [Bubble map drag](in-progress/universe-transform.md) | In progress | One-hand map drag, route gate, soft bounds, center marker; validation and recenter pending. |

## Backlog

### Universe

| Plan | State | Notes |
|---|---|---|
| [Universe color scope](backlog/universe/universe-color-scope.md) | Backlog | Bubble-first spatial color isolation in one URP Lit-based shader; soft edge, no post-process; room scope future. |
| [Infinite sector](backlog/universe/universe-infinite-sector.md) | Backlog | Effectively infinite procedural sectors in the bubble window; streaming, floating-origin rebase, recenter, ambient planet activation. |
| [Universe galaxies](backlog/universe/universe-galaxies.md) | Backlog | Galaxy groups, level/round creation, two-hand group transform, layout response. |

### Gameplay

| Plan | State | Notes |
|---|---|---|
| [Planet route drawing](backlog/gameplay/planet-route-drawing.md) | Backlog | Design only; planet-to-planet drawn loops, round-granted ships, optional pencil. |

### Architecture

| Plan | State | Notes |
|---|---|---|
| [Reusable modes + Universe migration](backlog/architecture/modes-reusable-and-universe.md) | Backlog | Add `ModeController<T>` in `_Modules/FlowControl`, then replace Universe `isMini`/`isTransitioning` with one mode source of truth. |

### Presentation and companion

| Plan | State | Notes |
|---|---|---|
| [TTS voice panel](backlog/presentation/tts-voice-panel-personality.md) | Backlog | Design only; camera-operator companion behavior for the TTS panel. |

## Done

See [done](done/README.md) for completed plans.
