# Universe — Two Views + Always-Available Two-Hand Scale

**Status:** in progress - view foundation implemented; replacing node handles with the space gesture.

**Related:** [Transforming the universe](../backlog/universe-transform.md) · [Interaction unification](interaction-unification.md) · [Galaxies and level/round creation](../backlog/universe-galaxies.md)

**Wiring rule:** components, references, and feedback visuals are authored in the scene/prefabs. Do not use runtime `AddComponent`, injection, `FindObjectsByType`, `GetComponent`, or disabled unused components. Remove unused components.

## Next step (implementation order)

The two-view foundation is in place: `UniverseView` applies `D` to `Level`, Room/Bubble poses and transitions work, routes are content-local, and scale feedback is the authored hand line. Remaining:

1. **Land the global gesture** — add `TwoHandGestureInput` and rewire `UniverseGhostController` to use its two pinch points. This is shared with the interaction-unification plan.
2. **Delete the node handles** — remove `UniverseNodeHandle`, planet/hub proxies, their outline components, and the `UniverseHandles` layer.
3. **Preserve route selection across Room ↔ Bubble** — do not cancel `SpaceshipSelectingState`; refresh the active station, destination ray, and grab zone after the transition.
4. **Scene wiring** — add `TwoHandGestureInput` on the `Universe` root, remove node proxies, keep the authored hand line.
5. **Tune the feel** — gain, thresholds, simultaneous-pinch activation, line feedback; test both directions in the headset.
6. **Defer the bubble boundary visual** until the gesture feels right; the hand line is the current feedback.

## 1. Goal

Keep the two presentation states:

1. **Room** — room-scale content, normal routes.
2. **Bubble** — a miniature of the same content in front of the player.

Switch between them with an **always-available two-hand pinch gesture**:

- pinch with both hands anywhere in open space;
- hold briefly so the gesture is intentional;
- bring the hands closer to shrink toward Bubble;
- spread the hands farther to grow toward Room;
- when the implied scale crosses a threshold, animate the real content to the other state.

Planets and hubs are no longer grab handles. They remain normal world objects for routes and gameplay.

## 2. Non-goals

- Two-hand move/rotation of the universe (future).
- Continuous live scaling of the content.
- Content ghost or render-texture preview.
- Persisting the view across app sessions.
- Redesigning Bubble UI (keep existing UI for now).

## 3. Existing facts

- `Level` is the content root containing `Hubs`, `Planets`, and `Spaceships`.
- `LevelContainer.SetRoomBasedPose()` places `Level` at the MRUK room anchor.
- Route/traveller data has been converted to content-local space so it follows `Level`.
- `SpaceshipBehaviour` is pinch-only (`HandGrabInteractable`); ships remain one-hand interactions.
- `UniverseView` is on an unscaled `Universe` root and serializes `contentRoot` to `Level`.
- The current node-handle proxies (`UniverseNodeHandle`) are provisional and will be removed.

## 4. Core model

### 4.1 Spaces

| Space | Meaning |
|---|---|
| World (W) | Physical room, camera, hands. |
| Content (C) | `Level` local space. |
| Display transform `D` | `Level` position/rotation/uniform scale. |

`D` is applied by `UniverseView` to `contentRoot` (`Level`).

### 4.2 Content bounds and bubble

`B_c` is the content-space sphere around the room content:

- center captured from the MRUK room center at the room pose;
- radius from the room bounds half-diagonal plus margin;
- used for mini scale and future bubble boundaries.

```text
B_w.center = D.TransformPoint(B_c.center)
B_w.radius = B_c.radius * D.scale
```

### 4.3 Two settled poses

```text
D_room: scale = 1, room anchor pose
D_mini: scale = miniBubbleRadius / B_c.radius
        content center at bubble anchor in front of camera
```

Mini rotation is Y-axis only and the bubble is recomputed in front of the player when entering Bubble. The only translation is the canonical transition between the two poses.

### 4.4 View states

```text
Room ──gesture crosses exitRoomScale──> Transitioning ──> Bubble
Bubble ──gesture crosses enterRoomScale──> Transitioning ──> Room
```

The gesture drives a **preview scale**, not `D` directly.

## 5. Always-available two-hand gesture

### 5.1 Input

`TwoHandGestureInput`:

- serialized `HandRef` left/right;
- reads thumb-tip/index-tip pinch strength and midpoint;
- activates when both hands pinch at the same time; an optional `dwellSecs` debounce defaults to `0` and can be raised to about `0.1 s` if tracking flickers;
- gates activation when either hand is selecting something else:
  - `HandGrabInteractor` selecting a ship or handle;
  - `TouchHandGrabInteractor` selecting touch content;
  - hand ray/poke selecting UI;
- serialized interactor references; no `FindObjectsByType` or `GetComponent`;
- one hand pinching alone never starts the gesture;
- **no route-editing mode gate:** route editing and view changes are not mutually exclusive. The gesture is suppressed only while a hand is selecting a ship or UI; both hands free means the gesture is available, even during a route-editing session.

### 5.2 Session

On activation:

```text
startScale = universeView.Scale
startHandDistance = |pinchRight - pinchLeft|
midpoint = (pinchLeft + pinchRight) / 2
```

While active:

- pause route progression;
- show the authored hand-to-hand line;
- compute preview scale;
- no `D` change until commit.

On release/cancel:

- hide the line;
- resume routes;
- end the session.

### 5.3 Scale mapping

```text
raw = handDistance / max(startHandDistance, epsilon)
previewScale = clamp(startScale * pow(raw, scaleGain), miniScale, 1)
```

- `scaleGain` default `2.5`;
- dead zone under ~3-5%;
- rebase the reference at limits;
- release grace ~`0.15 s` for pinch jitter;
- hand tracking loss ends the session safely.

### 5.4 Threshold commit

- In Room: commit to Bubble when `previewScale <= exitRoomScale` (default `0.45`).
- In Bubble: commit to Room when `previewScale >= enterRoomScale` (default `0.60`).
- Hysteresis prevents flicker.
- During the commit animation, end input and ignore new gestures.
- Tween `D` over `roomTransitionSecs` (default `0.6 s`) and snap to the exact target pose.

## 6. Feedback

- **Hand-to-hand line**: authored `LineRenderer`, shown while the gesture is active; endpoints are the pinch points.
- **Activation**: line appears when both hands pinch simultaneously; the optional debounce only prevents tracking flicker.
- **Limit feedback**: optional haptic/line pulse when a scale limit is hit.
- No planet/hub outlines for this gesture.
- No sphere ghost; it was removed because it obscured the scene and read as a giant outline.

## 7. Routes and path creation

- Routes continue in settled Room and Bubble.
- Pause route progression during the active gesture and commit animation.
- Route editing remains available in both Room and Bubble; no mode toggle separates routing from view changes.
- The two-hand gesture is suppressed while a hand is selecting a ship or UI. Release the held object, then pinch both hands to change view; route editing can resume immediately after.
- A view change does not cancel an active route-selection state; refresh the station, destination ray, and grab zone after the transition.
- Keep scale-aware ray length and grab-zone sizing so routing works in Bubble.
- Route completion is animation-based; physics triggers are only early collision/VFX.

## 8. Component map

| Type | Responsibility |
|---|---|
| `UniverseView` | Applies `D` to serialized `contentRoot` (`Level`); room/mini poses, transitions, `WithRoomPose`. |
| `TwoHandGestureInput` | Serialized hand/interactor refs, simultaneous pinch detection, optional debounce, busy-hand gate. |
| `UniverseGhostController` | Gesture state machine, scale mapping, thresholds, route pause, commit. References `UniverseView`, `TwoHandGestureInput`, `UniverseGhostView`, `SpaceshipsSpawner`. |
| `UniverseGhostView` | Authored hand-line visual; show/hide/set endpoints. No runtime creation. |
| `UniverseBubble` | Content bounds and bubble radius; future boundary view. |
| `UniverseBubbleView` | Deferred; not part of this step. |

Modified:

- `LevelContainer` — room pose, route pause, spawn with room pose.
- Route/line/traveller data — content-local (done).
- `RaycastHelper`, `GrabbableWrapper`, `DestinationSelector` — scale-aware path interactions.
- `Game.unity` — `Universe` root, `UniverseView`, `UniverseGhostController`, `TwoHandGestureInput`, authored hand line.
- `Spaceship (1).prefab` — pinch-only ship.

## 9. Remove with this substitution

Delete, do not disable:

- `UniverseNodeHandle` and all planet/hub `UniverseHandle` proxies.
- `PlanetBehaviour.universeHandle`, `HubBehaviour.universeHandle`, `IRouteWorldNode.UniverseHandle`.
- Per-node `Grabbable`, `GrabFreeTransformer`, `HandGrabInteractable`, proxy Rigidbody/colliders.
- `GrabbablePointerEventTypeOutline` on node proxies.
- `UniverseHandles` layer and handle colliders.
- First-hand ghost hint and sphere ghost visual.

## 10. Phases

### Phase A0 — Content-local routes (done)

- Route/line/traveller data in `Level` space.
- Scale-aware ray length and grab-zone sizing.

### Phase A1 — Two views (done)

- `UniverseView` on unscaled `Universe` root, `contentRoot = Level`.
- Room/Bubble transitions and route pause.

### Phase A2 — Replace node handles with the space gesture (current)

- Add authored `TwoHandGestureInput` with serialized hand/interactor refs.
- Add simultaneous two-hand pinch activation, optional debounce, busy-hand gate, release grace.
- Rewire `UniverseGhostController` to use gesture pinch points.
- Remove node handle proxies and per-node handle fields/layer/outlines.
- Keep the hand-to-hand line as the only scale feedback.

**Exit:** pinch both hands anywhere in open space; line appears; hands closer/farther commit the view.

### Phase A3 — Tuning

- Tune optional debounce, grace, gain, thresholds, line width/color.
- Verify no activation while grabbing ships or UI.
- Verify routes and path creation remain usable in Bubble.

## 11. Test plan

1. One-hand pinch: no gesture, no line.
2. Both-hand pinch in open space: line appears on simultaneous pinch.
3. Hands closer → commit to Bubble; farther → commit to Room.
4. Hysteresis prevents flicker at the threshold.
5. While a hand is selecting a ship/UI, two-hand pinch does not start the gesture; after release it does.
6. Pinch a ship with one hand and the other in space: gesture does not start.
7. Pinch UI with both hands: gesture does not start.
8. Release one hand: line hides; no stuck session.
9. Routes pause during gesture/commit and resume after.
10. Path creation still works in Bubble.
11. Change view while selecting a destination: the selection survives and the station/ray/grab zone refresh.
12. No node handle proxies, `UniverseHandles` layer, or node outlines remain.

## 12. Risks and decisions

| Decision | Choice |
|---|---|
| Handle source | Always-available two-hand pinch; no planet/hub targets. |
| Activation | Both hands pinching at the same time; optional debounce. |
| Busy hands | Do not start if either hand is selecting a ship/UI. |
| Feedback | Hand-to-hand line + optional limit pulse; no sphere/outline. |
| Scale mapping | Geared `scaleGain = 2.5` + threshold commit. |
| Route editing + view | Coexist. No mode gate; the gesture is suppressed only while a hand is selecting a ship/UI. |
| Routes during gesture | Pause progression; resume settled. |
| Bubble placement | In front of the player on entry; fixed while there. |
| Rotation | Y-axis only when added later. |

## 13. Defaults

| Parameter | Default |
|---|---|
| `miniBubbleRadius` | `0.22 m` |
| `bubbleDistance` | `0.45 m` |
| `bubbleHeightOffset` | `-0.10 m` |
| `roomTransitionSecs` | `0.6 s` |
| `scaleGain` | `2.5` |
| `enterRoomScale` | `0.60` |
| `exitRoomScale` | `0.45` |
| `dwellSecs` | `0 s` (optional debounce) |
| `releaseGraceSecs` | `0.15 s` |
| `minHandSeparation` | `0.06 m` |

## 14. Out of scope

- Two-hand translation/rotation.
- Continuous transform.
- Content ghost/render preview.
- Bubble boundary visual.
- Persistent view state.
- UI redesign.
