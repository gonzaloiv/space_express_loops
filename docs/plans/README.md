# Plans

Plans are grouped by state. Move a plan between folders when its state changes.

## In progress

| Plan | State | Notes |
|---|---|---|
| [Space shaping — spike](in-progress/space-shaping-spike.md) | Done (passed) | Tier 0 math (`SpacetimeField`, `TrajectoryIntegrator`) and the Tier 1 debug scene; ratio band found and coast chosen. Superseded by the POC. |
| [Space shaping — interaction + presentation polish](in-progress/space-shaping-polish.md) | In progress | Next: mesh-deform lattice (tubes/ribbons), move-range viz, ghost preview, grab feedback, tuning. Carried forward from the POC. |
| [Space shaping — architecture refactor](in-progress/space-shaping-architecture.md) | In progress | First pass done: the lab runs on `Domain/` / `Data/` / `Views/` / `Flow/`; the POC driver is deleted. Remaining: game integration. |
| [Sector generation and extent](in-progress/universe-sector-generation.md) | In progress | G0 done: `DigitalLove.Game.Sectors` assembly with `SectorShape` (Ring/Spiral/Cluster), cell grid, extent/focus, 23 EditMode tests passing. Next: G1 — spawn the field and select destinations/hubs from it. |

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
