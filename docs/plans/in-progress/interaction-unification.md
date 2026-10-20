# Interaction Unification — Plan

**Status:** in progress - ships are pinch-only; node handles are being replaced by the two-hand space gesture.

**Related:** [Two views + two-hand scale](universe-two-views.md) · [Transforming the universe](../backlog/universe-transform.md)

## Next step (implementation order)

1. **Add `TwoHandGestureInput`** on the `Universe` root.
   - Serialized left/right `HandRef`, `HandGrabInteractor`, `TouchHandGrabInteractor`, and hand-ray references.
   - Both-hands-simultaneous pinch activation, optional `dwellSecs = 0`, release grace.
   - Busy-hand gate when either hand is selecting a ship or UI.
   - Expose the two pinch points and active state; no `FindObjectsByType`/`GetComponent`.

2. **Rewire `UniverseGhostController`** to read `TwoHandGestureInput` instead of `UniverseNodeHandle`.
   - Keep scale mapping, thresholds, hand line, route pause, and commit.
   - Remove `planetsSpawner`/`hubsSpawner` dependencies from the controller.

3. **Delete the node-handle path** (not disable):
   - `UniverseNodeHandle`, planet/hub `UniverseHandle` proxies and their components.
   - `PlanetBehaviour.universeHandle`, `HubBehaviour.universeHandle`, and `IRouteWorldNode.UniverseHandle`.
   - Node outline components and the `UniverseHandles` layer.
   - First-hand hint and sphere ghost remnants.

4. **Preserve route selection across view changes.**
   - Do not cancel `SpaceshipSelectingState` on transition.
   - Refresh the active station, destination ray, and grab zone when the transition completes.
   - Extend `SpaceshipBehaviour.RefreshGrabbableAtStation` beyond `SpaceshipRunningState`.

5. **Scene wiring**
   - Add `TwoHandGestureInput` and its serialized references on the `Universe` root.
   - Remove the node proxy children and the `UniverseHandles` layer.
   - Keep the authored hand-line `UniverseGhostView`.

6. **Validate in the headset**
   - One hand = nothing; both hands pinch = scale; ship/UI held = no scale.
   - Route selection survives Room ↔ Bubble and the station/ray refreshes.
   - Then remove or gate the temporary debug logs (`logEvents`).

## 1. Goal

Keep one **targeted grab** path and one **global two-hand gesture**:

- **Targeted grabs** use `HandGrabInteractable` with thumb + index pinch. Ships are the primary targeted grab.
- **Universe scale** uses a global two-hand pinch in open space. It has no target and no per-node handle proxies.
- No touch-grab/pinch mix for the primary interactions.

This keeps ship grabbing and universe scaling clear:

- one hand on a target = targeted interaction;
- two hands pinching open space = universe scale;
- one hand alone never scales.

## 2. Wiring rules

All setup lives in prefabs and scenes:

- no runtime `AddComponent`, `InjectOptional...`, or runtime-created interaction objects;
- no `FindObjectsByType`/`FindObjectsOfType`/`GetComponent` lookups for these dependencies;
- no null checks on serialized references - missing wiring is a broken prefab/scene and should fail loudly;
- no disabling unused components - remove them from the prefab;
- shared references (`UniverseView`, `TwoHandGestureInput`, ghost line, ship interactors) are serialized in the scene/prefab.

## 3. Current state

### Ships

- `TouchHandGrabInteractable` has been removed from `Spaceship (1).prefab`.
- `HandGrabInteractable` is configured for pinch only: thumb + index required, any-finger release.
- `Grabbable` and `GrabbableWrapper` listen to the same pointer-event flow.

### Node handles (being removed)

The provisional `UniverseNodeHandle` proxies on planets and hubs are replaced by the global gesture. They should be deleted, not disabled:

- `UniverseNodeHandle` component;
- planet/hub `UniverseHandle` proxy children;
- `PlanetBehaviour.universeHandle`, `HubBehaviour.universeHandle`, `IRouteWorldNode.UniverseHandle`;
- per-node `Grabbable`, `GrabFreeTransformer`, `HandGrabInteractable`, proxy Rigidbody/colliders;
- `GrabbablePointerEventTypeOutline` on node proxies;
- `UniverseHandles` layer.

## 4. Unified model

### 4.1 Targeted pinch

- Ships use `HandGrabInteractable` with `GrabTypeFlags.Pinch`.
- No `TouchHandGrabInteractable` on ships.
- `TouchHandGrabRelease` remains only for other touch-grabbable content; it is not part of the ship path.
- Other targeted content can adopt the same pinch configuration later.

### 4.2 Global two-hand gesture

`TwoHandGestureInput`:

- serialized left/right `HandRef`;
- thumb-tip/index-tip pinch strength and midpoint;
- activates when both hands pinch at the same time; optional `dwellSecs` debounce defaults to `0`;
- no activation when either hand is already selecting a ship or UI (`HandGrabInteractor`/`TouchHandGrabInteractor`/ray state);
- release grace for pinch jitter;
- feeds `UniverseGhostController` with two pinch points;
- no `FindObjectsByType` or `GetComponent`;
- this is a global hand gesture, not a second target-based interactable.

### 4.3 Arbitration

```text
Idle
  ├─ one hand selects ship/UI ──> TargetedInteraction
  ├─ one hand pinches space ──> Nothing
  └─ both hands pinch space (+ optional debounce) ──> UniverseScale
        └─ release/cancel ──> Idle
```

Rules:

- a ship grab is always one hand;
- a single pinch in space never scales;
- two hands pinching at the same time scale the universe; debounce is optional;
- if either hand is selecting a ship/UI, the gesture does not start;
- route editing and view changes coexist; the gesture is suppressed only while a hand is selecting a ship/UI, then available again on release;
- a view change must not cancel an active route-selection state; refresh the station, ray, and grab zone after the transition;
- state changes force-release the active gesture.

### 4.4 Feedback

- Ship: existing authored hover/selection behavior.
- Universe scale: authored hand-to-hand line only.
- No planet/hub outline requirement for scale.
- No sphere ghost.

## 5. Scene/prefab checklist

| Asset | Change |
|---|---|
| `Spaceship (1).prefab` | Pinch-only `HandGrabInteractable`; `TouchHandGrabInteractable` removed. |
| `Game.unity` / `Universe` root | `UniverseView` with `contentRoot = Level`; `UniverseGhostController`; `TwoHandGestureInput` with serialized hand/interactor refs. |
| `Game.unity` / ghost | Authored hand-line `UniverseGhostView`; serialize on controller. |
| `Game.unity` / planet and hub templates | Remove node handle proxies and per-node handle fields/components. |
| `SpaceshipBehaviour` / `SpaceshipsSpawner` | On transition completion, refresh the active station, destination ray, and grab zone without cancelling `SpaceshipSelectingState`. |
| Layers | Remove `UniverseHandles` if nothing else uses it. |
| Input | Keep hand ray/poke for UI; pinch wins on ships. |

## 6. Migration phases

### Phase U0 — Ship pinch only (done)

- Remove touch grab from the ship.
- Configure pinch-only grab.
- Verify ship grab/drop and route editing.

### Phase U1 — Global two-hand gesture (current)

- Add `TwoHandGestureInput` with serialized hand and interactor references.
- Add simultaneous-pinch activation, optional debounce, busy-hand gating, and release grace.
- Rewire `UniverseGhostController` to use pinch points.
- Preserve route-selection state across view changes and refresh the active station/ray/grab zone.
- Remove all node handle proxies, fields, outlines, and the handle layer.
- Keep the hand-to-hand line.

**Exit:** two-hand pinch anywhere scales the universe; no node targets; ships remain pinch-only.

### Phase U2 — Cleanup and tuning

- Remove obsolete touch code from the ship path if unused.
- Tune optional debounce, release grace, and activation feedback.
- Verify hand ray/pinch priority.
- Verify no disabled/unused components remain.

## 7. Risks

| Risk | Mitigation |
|---|---|
| Accidental activation while pinching a ship/UI | Busy-hand gate; optional short debounce. |
| Pinch fatigue during long gestures | Keep sessions short; consider open-palm gesture later. |
| Pinch tracking jitter | Release grace and dead zone. |
| One-hand pinch ambiguity | One hand never starts the gesture. |
| Gesture conflicts with path creation | Busy-hand gate suppresses the gesture only while a hand selects a ship/UI; routing and view changes otherwise coexist. |
| View change while routing | Release the held ship/UI, pinch both hands, then resume routing; no mode switch. Preserve the selection state and refresh it after the transition. |
| Stale route selection after view change | Refresh the active station, ray, and grab zone on transition completion. |
| Removing node handles loses precise anchors | Accepted; the gesture is spatial, not target-based. |
| Hand line readability | Tune color/width/activation pulse. |

## 8. Acceptance

- Ships grab and drop with thumb-index pinch only.
- Two-hand pinch in open space starts the universe scale on simultaneous pinch.
- One-hand pinch never scales.
- Pinching a ship or UI with one hand and space with the other does not start the gesture.
- Two-hand pinch does nothing while a hand is selecting a ship/UI; after release it starts normally.
- Route editing survives a view change; the active station/ray/grab zone refreshes after the transition.
- No node handle proxies, `UniverseNodeHandle`, node outline components, or `UniverseHandles` layer remain.
- No runtime component lookup or creation is used for this interaction.
