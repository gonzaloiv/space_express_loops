# Reusable Modes + Universe Mode Migration

**Status:** backlog - short implementation plan; follows
[`Assets/_Modules/FlowControl/Docs/modes.md`](../../../../Assets/_Modules/FlowControl/Docs/modes.md).

**Related:** [Bubble map drag](../../in-progress/universe-transform.md) · [Silent planets](../universe/universe-silent-planets.md) · [Infinite sector](../universe/universe-infinite-sector.md)

## Goal

Add one small reusable mode primitive, then make `UniverseView` the first consumer so Room/Bubble has a single source of truth instead of `isMini` + `isTransitioning`.

## Non-goals

- Converting Flow states to modes.
- Per-mode `MonoState` GameObjects.
- Moving gameplay/round progression out of Flow.
- Changing Bubble behaviour or visuals.

## Phase 1 — `ModeController<TMode>`

Location: `Assets/_Modules/FlowControl/Scripts/Modes/ModeController.cs`.

```csharp
ModeController<TMode> where TMode : struct, Enum
  Current
  TransitionTarget
  IsTransitioning
  TrySet(mode)          // instant
  BeginTransition(to)   // owner tween/animation completes it
  CompleteTransition()
  CancelTransition()
  event ModeChanged(from, to)
  event TransitionStarted(to)
  event TransitionCompleted(to)
```

Rules:

- Pure C#; no Unity dependency.
- One owner per controller.
- No re-entrant transitions from events.
- `CompleteTransition` and `CancelTransition` are idempotent.
- Invalid/no-op transitions do not fire events.
- Add EditMode tests for: instant set, begin/complete, cancel, no-op, re-entrancy guard.
- No feature-specific enums in the module.

**Exit:** the controller compiles and its tests pass without a Unity scene.

## Phase 2 — Universe mode

Add `UniverseViewMode { Room, Bubble }` in `Universe/Presentation/`.

`UniverseView` owns `ModeController<UniverseViewMode>`:

- Remove `isMini`.
- Replace `isTransitioning` with `Mode.IsTransitioning`.
- Replace `TransitionTarget`/`TransitionStarted`/`TransitionCompleted` plumbing with the controller's events.
- Initial mode is Bubble after the room anchor is ready.
- `SetBubbleView` → `TrySet` (instant) or `BeginTransition` + tween completion.
- `SetRoomAnchor`/`SetBubbleView` become the only mode writers.
- Keep a temporary `IsMini => Mode.Current == UniverseViewMode.Bubble` compatibility property only while migrating call sites; remove before closing the plan.

## Phase 3 — Migrate readers

| Reader | Change |
|---|---|
| `UniverseGhostController` | gate gestures on `Mode.Current` and `Mode.IsTransitioning` |
| `UniverseCenterMarker` | show only in `Bubble` |
| `BubbleScaleSession` | read `UniverseViewMode` instead of `IsMini` |
| `BubbleMapTransformSession` | no mode ownership; unchanged |
| `SilentPlanetsSpawner` | subscribe to `ModeChanged` instead of polling `IsMini` |
| `SpaceshipsSpawner` / `SpaceshipBehaviour` | drive Bubble view state from `ModeChanged` |
| `LevelContainer` | subscribe to `ModeChanged`/`TransitionCompleted` instead of `IsMini` polling |
| Flow/TTS/UI | read mode/events if needed; never write it |

## Phase 4 — Validation

- Room ↔ Bubble still works, including initial Bubble.
- Every transition fires `TransitionStarted`/`TransitionCompleted` exactly once.
- Gestures, ship interaction, route editing, and cooldowns follow the mode.
- No `isMini`/`isBubble` parallel booleans remain.
- Existing save/restore of Bubble placement still loads.
- No new missing script references after moving/adding mode types.

## Risks

| Risk | Mitigation |
|---|---|
| Generic controller not serializable in the inspector | Keep it runtime-only; owner MonoBehaviour serializes only tunables |
| Transition events re-entered by callbacks | Guard inside the controller; document no re-entrant transitions |
| Migration misses a reader | Temporary `IsMini` alias + grep before removal |
| Flow starts owning the mode again | Keep Flow read-only; promotion rule in the standard |
| Over-engineering for one consumer | Keep the controller ~100 lines; extract only when a second consumer needs it |

## Acceptance

- One reusable mode primitive in `_Modules/FlowControl`.
- Universe has a single mode source of truth.
- Flow and other systems observe rather than poll private flags.
- The modes standard is updated if the implementation settles a new convention.
