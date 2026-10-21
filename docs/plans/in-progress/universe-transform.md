# Universe — Bubble Map Drag and Yaw

**Status:** in progress - Bubble preview, gesture axis, and drag/yaw session implemented; validation pending.

**Related:** [Two views + interaction unify](../done/universe-views-and-interaction.md) · [Universe galaxies](../backlog/universe-galaxies.md) · [Infinite sector](../backlog/universe-infinite-sector.md) · [Color scope](../backlog/universe-color-scope.md)

**Wiring rule:** all visuals and refs stay authored in the scene/prefabs. No runtime `AddComponent`, `GetComponent`, or hierarchy creation.

## Next step

Implement the bubble-view-center Room reference: `UniverseView` stores the Bubble offset/yaw in content-local space, uses the same `BubbleCenterWorld` as the color scope, applies it when building the Room pose, and clamps it to the room bounds. Then validate Bubble drag/yaw → Room continuity and tune `translationGain`, `yawGain`, `bubbleMoveLimit`, and the settle thresholds.

## 1. Goal

Let the Bubble behave like a draggable map: two-hand pinch moves and rotates it in XZ, while the existing hand-distance mapping still controls scale and Room/Bubble commit.

Room is anchored to the **bubble's content/view center** at the moment of the Bubble → Room transition, not to the camera. That is the point the bubble is currently looking at, so the map arrangement the player dragged in Bubble carries into Room. Y stays at the room pose height.

## 2. Decision summary

| Decision | Choice |
|---|---|
| First view to transform | **Bubble only** |
| Room | Bubble-view-center reference at transition time; no direct Room editing |
| Room reference point | The bubble's content/view center (`BubbleCenterWorld`) projected to Room Y, captured once per transition |
| Offset mapping | Bubble offset stored content-local, so the same planets stay in front after scaling |
| Translation axes | XZ only; Y fixed |
| Settle | Release commits; tiny movements below the dead zone revert |
| Gesture | Existing two-hand pinch: midpoint drags, hand-axis yaw, distance scales |
| Bubble edits on Room transition | Persisted through the content-local offset/yaw, clamped to the room bounds |

Bubble-only is the smaller first version:

- no room-bound clamping for content that surrounds the player;
- no editing of the canonical room pose;
- the map is directly in front of the player, so drag/yaw feedback is obvious;
- a failed experiment only affects the Bubble session.

## 3. Starting point

- `UniverseView` owns the Room pose, the Bubble pose, `SetBubbleView`, and the transition tween.
- The Room pose is centered on the room; the player is inside the content.
- The Bubble is placed in front of the player and stays fixed while there.
- `TwoHandGestureInput` exposes left/right pinch points, `Midpoint`, `HandDistance`, activation, and per-hand pinch state.
- `UniverseGhostController` maps hand distance to preview scale and commits at `exitRoomScale` / `enterRoomScale`.
- Routes are content-local and pause through `SpaceshipsSpawner.SetViewTransition`.
- Authorized feedback: per-hand pinch rings and the hand-to-hand line.

## 4. Bubble transform model

### 4.1 Session start

On gesture activation in Bubble:

```text
startPose     = universeView.CurrentPose
startBubble   = universeView.BubblePose
startMidpoint = gestureInput.Midpoint
startHandAxis = NormalizeXZ(rightPinch - leftPinch)
```

Reject when the hand separation is below `minHandSeparation`.

### 4.2 Translation

```text
deltaMid = currentMidpoint - startMidpoint
offset   = ClampXZ(Geared(deltaMid, translationGain), bubbleMoveLimit)
D.position = startBubble.position + offset
```

- `translationGain = 1.0` for direct manipulation;
- dead zone `0.01 m`;
- `bubbleMoveLimit` keeps the map within a comfortable box in front of the player;
- Y is fixed to the canonical bubble height.

### 4.3 Yaw

```text
deltaYaw = SignedYaw(startHandAxis, currentHandAxis)
D.rotation = Quaternion.AngleAxis(ClampYaw(deltaYaw * yawGain), Vector3.up) * startBubble.rotation
```

- Y-axis only; pitch and roll stay zero;
- `yawGain = 1.0`, dead zone `3°`;
- no snapping or fixed steps in the first version.

### 4.4 Scale and Room commit

Scale keeps the existing behavior:

```text
ghostScale = clamp(startScale * pow(handDistance / startHandDistance, scaleGain), miniScale, 1)
```

- in Bubble, spreading both hands to `enterRoomScale` commits to Room;
- the Bubble offset is converted to content-local space and applied to the Room pose;
- Room captures the bubble's view-center XZ at commit time, so the same planets stay in front after scaling;
- going back to Bubble uses the same content-local offset and yaw, so the map does not reset.

The offset is clamped to the room bounds so a small Bubble drag cannot push the room content through a wall.

### 4.5 Room reference and mapping (decided)

Room uses the **bubble's view center** at the transition moment, not the camera. This is the same point the color-scope sphere uses:

```text
bubbleCenterWorld = contentRoot.TransformPoint(contentCenter)
roomCenterY       = roomPose.position.y + roomPose.rotation * contentCenter  (world Y)
roomCenter        = (bubbleCenterWorld.x, roomCenterY, bubbleCenterWorld.z)
roomPosition      = roomCenter - bubbleYaw * contentCenter
```

The Bubble drag offset is stored in **content-local** space:

```text
offsetLocal = Inverse(bubbleYaw) * (bubbleCenterWorld - frontAnchor) / miniScale
```

Bubble reuses the same `offsetLocal` and `bubbleYaw`:

```text
bubblePosition = frontAnchor - bubbleYaw * (contentCenter * miniScale)
                 + bubbleYaw * (offsetLocal * miniScale)
```

Consequences:

- the map center the player is looking at in Bubble becomes the Room content center;
- a planet in front of the bubble view stays in front after scaling to Room;
- small Bubble drags become large Room moves (`1 / miniScale`), so the room clamp is required;
- the reference point is captured **once per transition**, not every frame; walking around the room does not drag the content;
- `offsetLocal` and yaw persist across Room ↔ Bubble within the round and reset in `SetRoomAnchor` (new room/round);
- the color-scope sphere and the Room reference use the same `bubbleCenterWorld`, so the visual focus and the scale target agree.

**Infinite-sector coordination:** once the universe streams beyond the room (see
[Infinite sector](../backlog/universe-infinite-sector.md)), this mapping applies only
within a `roomLinkRadius` around the central galaxy. If the Bubble focus is farther
away, Bubble → Room returns to the hub/central gameplay region instead of dragging a
remote exploration focus into the room.

### 4.6 Release

- Release commits the current Bubble pose.
- If the total drag is under `settleDistance` and the total yaw under `settleYaw`, restore `startBubble` instead (accidental-nudge guard).
- Routes resume after the session.

## 5. Step-by-step implementation

### Step 1 — `UniverseView` Bubble preview API (done)

Add:

- `void BeginBubblePreview()` — capture `BubblePose` and current pose;
- `void ApplyBubblePreview(Vector3 position, Quaternion rotation)` — apply XZ position and Y yaw without starting a transition;
- `void CancelBubblePreview()` — tween back to the captured Bubble pose;
- `void CommitBubblePreview()` — accept the previewed pose as the current Bubble pose for this visit;
- `BubblePose` getter.

No new GameObjects; no state-machine change.

### Step 2 — Transform intent on `TwoHandGestureInput` (done)

Expose read-only values from the pinch points:

- `Vector3 Midpoint`;
- `Vector2 HandAxisXZ` normalized;
- `bool HasValidAxis` (separation above `minHandSeparation`).

The input component stays policy-free.

### Step 3 — Bubble session in `UniverseGhostController` (done)

- Start a Bubble transform session when the gesture becomes active and `universeView.IsMini`.
- While active:
  - apply midpoint drag and hand-axis yaw via `ApplyBubblePreview`;
  - keep computing scale as today.
- On scale commit to Room: `CommitBubblePreview()` (or discard) then run the existing transition.
- On release: commit or restore based on the settle thresholds; resume routes.
- In Room: no transform session; the gesture only scales toward Bubble as today.

### Step 4 — Bounds

- Clamp the Bubble center to a box in front of the player (`bubbleMoveLimit`).
- Keep the canonical Y.
- Optional yaw limit; default continuous.
- Keep scale clamped to `MiniScale..1`.

### Step 5 — Feedback

- Reuse the authored pinch rings and hand line.
- Optional authored pivot marker at the midpoint while dragging; no boundary sphere.
- No new runtime-created visuals.

### Step 6 — Validation

- Two-hand pinch in Bubble drags the map in XZ and yaws it around Y.
- Releasing commits; tiny movement restores the canonical Bubble pose.
- Scale still commits Bubble → Room at the threshold.
- Re-entering Bubble resets to canonical front-of-player placement.
- Routes pause during the session and resume after.
- Routes and travellers remain attached to the transformed content.
- Path creation still works in Bubble.
- No activation while a ship or UI is selected.

## 6. Component map

| Type | Responsibility | Change |
|---|---|---|
| `UniverseView` | Bubble pose, transitions, preview API | Extend |
| `TwoHandGestureInput` | Midpoint and XZ hand axis | Extend |
| `UniverseGhostController` | Bubble session, drag/yaw mapping, settle | Extend |
| `UniverseGhostView` | Rings, line, optional pivot marker | Extend only if needed |
| `SpaceshipsSpawner` | Route pause/resume and station refresh | Existing API |

No planet/hub handles, no boundary ghost, no runtime-created visuals.

## 7. Defaults

| Parameter | Default |
|---|---|
| `translationGain` | `1.0` |
| `yawGain` | `1.0` |
| `translationDeadZone` | `0.01 m` |
| `yawDeadZone` | `3°` |
| `minHandSeparation` | `0.06 m` |
| `settleDistance` | `0.02 m` |
| `settleYaw` | `3°` |
| `bubbleMoveLimit` | `0.35 m` half-extent in front of the player |
| `roomLinkRadius` | `5 m` around the central galaxy |
| `scaleGain` | `2.5` |
| `enterRoomScale` | `0.60` |
| `exitRoomScale` | `0.45` |

## 8. Risks

| Risk | Mitigation |
|---|---|
| Drag and scale fight each other | Use midpoint for drag and distance for scale; both are independent. |
| Bubble pushed out of reach | Clamp to `bubbleMoveLimit`; reset on re-entry. |
| Disorientation | Yaw only around Y; no pitch/roll; keep the map at chest height. |
| Far Bubble focus mapped into Room | Limit the mapping to `roomLinkRadius`; farther commits return to the central hub. |
| Bubble edits lost on invalid focus | Reset to the central Bubble pose when the focus is outside the room-linked region. |
| Gesture conflicts with routing | Session pauses routes and uses the existing busy-hand gate. |
| Accidental nudges | Release settle thresholds restore the canonical pose. |

## 9. Open decisions

None. Decided for this version:

- Room reference: the bubble's view center (`BubbleCenterWorld`) at transition time, not the camera.
- Mapping: content-local, so the planet arrangement stays in front of the camera.
- Reference point: captured once per transition.
- Persistence: within the round, reset by `SetRoomAnchor`.

Future options:

- move/rotate Room content directly;
- add vertical movement;
- add one-hand drag mode;
- follow the player continuously instead of capturing the anchor.
