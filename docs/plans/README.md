# Plans

Plans are grouped by state. Move a plan between folders when its state changes.

## In progress

| Plan | State | Notes |
|---|---|---|
| [Space shaping — test slice](in-progress/space-shaping-test-slice.md) | In progress | T0 + T1 done and tuned (`dilationScale 0.2`, `captureRadius 1.2`, deadlines 3.3-3.9): every puzzle start fails and is solvable inside the move range. Lab HUD is a canvas panel; 4 puzzles wired to Next. Next: decide the ship-time unit, then the prediction toggle. |
| [Space shaping — interaction + presentation polish](in-progress/space-shaping-polish.md) | In progress | Mesh-deform lattice done; move-range viz done (grab-only, no per-frame `SetActive`). Remaining: ghost / pre-move preview, grab feedback, tuning. |
| [Sector generation and extent](in-progress/universe-sector-generation.md) | In progress | G0 + G1 field/spawner done (`SectorField`, `SectorFieldSpawner`, 39 EditMode tests passing); `SectorSelector` picks active nodes from the field. Next: `PlanetsSpawner`/`HubsSpawner` as selectors + the aperture move. |

## Backlog

### Gameplay

| Plan | State | Notes |
|---|---|---|
| [Space shaping — reframing and variations](backlog/gameplay/space-shaping-future.md) | Backlog (design only) | Recorded: Map/Sculpt views, simultaneous multi-ship, sequential multi-destination, level generation (editor + seed creator), minimum-move scoring. |
| [Space shaping — integration into the game](backlog/gameplay/space-shaping-integration.md) | Backlog (design only) | Sketch: replace the current route/round/economy flow with the puzzle loop; scene, flow states, save/load, legacy removal. |

### Universe

| Plan | State | Notes |
|---|---|---|
| [Sector field (hub)](backlog/universe/universe-infinite-sector.md) | Backlog | Hub for the split slices below; bounded seeded field, no streaming. Slice 1 (view frame) done; slice 2 is in progress. |

### Presentation and companion

| Plan | State | Notes |
|---|---|---|
| [TTS voice panel](backlog/presentation/tts-voice-panel-personality.md) | Backlog | Design only; camera-operator companion behavior for the TTS panel. |
| [Hologram look](backlog/presentation/hologram-look.md) | Backlog | Design only; hologram pass for the bubble view first, then edition view (object-scoped). Independent of other work streams. |

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
