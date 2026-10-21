# Universe — Bubble Map Drag and Yaw

**Status:** in progress - implementation complete; headset validation and tuning pending.

**Related:** [Two views + interaction unify](../done/universe-views-and-interaction.md) · [Universe galaxies](../backlog/universe-galaxies.md) · [Infinite sector](../backlog/universe-infinite-sector.md) · [Color scope](../backlog/universe-color-scope.md)

**Wiring rule:** all visuals and refs stay authored in the scene/prefabs. No runtime `AddComponent`, `GetComponent`, or hierarchy creation.

## Next step

Validate in the headset: the `UniverseCenterMarker` must stay fixed while dragging, Bubble drag/yaw → Room must keep the planets centered on that marker, and returning to Bubble must preserve the map placement. The transition now tweens scale/rotation around a lerped center so the center does not drift. Then tune `translationGain`, `yawGain`, `bubbleMoveLimit`, and the settle thresholds.

## 1. Goal

Let the Bubble behave like a draggable map: two-hand pinch moves and rotates it in XZ, while the existing hand-distance mapping still controls scale and Room/Bubble commit.

The center is captured **when entering Bubble** (initial spawn or Room → Bubble), not while in Room. It stays fixed for that Bubble visit; dragging changes which content point is shown there, and Room scaling puts that same content point at the same center. The Bubble content center itself is seeded from the spawned hubs/planets, not from the room bounds.

## 2. Decision summary

| Decision | Choice |
|---|---|
| Initial view | **Bubble**, centered in front of the camera |
| First view to transform | **Bubble only** |
| Room | Bubble-view-center reference at transition time; no direct Room editing |
| Center point | Captured when entering Bubble; fixed for the visit. Dragging changes the content point shown there, not the center |
| Center source | Seeded from the spawned hubs/planets (content centroid), not room bounds |
| Room reference point | The same center point, unchanged, at transition time |
| Offset mapping | Bubble offset stored content-local, so the same planets stay in front after scaling |
| Translation axes | XZ only; Y fixed |
| Settle | Release commits; tiny movements below the dead zone revert |
| Gesture | Existing two-hand pinch: midpoint drags, hand-axis yaw, distance scales |
| Bubble edits on Room transition | Persisted through the content-local offset/yaw; the center stays fixed |
| Save data | `bubbleOffsetLocal` + `bubbleYaw` stored in `GameSnapshot`; restored after the room anchor loads |

Bubble-only is the smaller first version:

- no room-bound clamping for content that surrounds the player;
- no editing of the canonical room pose;
- the map is directly in front of the player, so drag/yaw feedback is obvious;
- a failed experiment only affects the Bubble session.

## 3. Starting point

- `UniverseView` owns the Room pose, the Bubble pose, `SetBubbleView`, and the transition tween.
- The app starts in Bubble: after the room anchor loads, the universe is centered in front of the camera.
- The Room pose is centered on the room; the player is inside the content after scaling up.
- Bubble drag/yaw and saved offsets apply to the initial Bubble too.
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
deltaMid   = currentMidpoint - startMidpoint
target     = softDeadZone(deltaMid * translationGain, translationDeadZone)
target     = ClampXZ(target, bubbleMoveLimit)
smoothed   = Lerp(smoothed, target, 1 - exp(-translationSharpness * dt))
D.position = startBubble.position + smoothed
```

- `translationGain = 1.0` for direct manipulation;
- soft dead zone removes tiny jitter without a jump when crossing the threshold;
- `bubbleMoveLimit` keeps the map within a comfortable box in front of the player;
- `translationSharpness` defaults to `18` (higher = snappier);
- Y is fixed to the canonical bubble height.

### 4.3 Yaw

```text
rawYaw    = SignedYaw(startHandAxis, currentHandAxis) * -yawGain
targetYaw = softDeadZone(rawYaw, yawDeadZone)
smoothed  = LerpAngle(smoothed, targetYaw, 1 - exp(-yawSharpness * dt))
D.rotation = Quaternion.AngleAxis(smoothed, Vector3.up) * startBubble.rotation
```

- Y-axis only; pitch and roll stay zero;
- rotation pivots on the focus point at the view center, so a planet under the center stays under the center while the universe yaws around it;
- soft dead zone avoids the 3° jump when leaving the threshold;
- `yawSharpness` defaults to `14` (higher = snappier);
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

The first version keeps the center fixed and does not clamp the Room pose to the room bounds; clamping is a later refinement if content leaves the room.

### 4.5 Room reference and mapping (decided)

The center is one **fixed world point**: the center of the player's view where the Bubble was before translation (`frontAnchor`). It does not move when the map is dragged.

```text
centerLocal       = contentCenter + offsetLocal       // content point shown at the view center
bubblePosition    = frontAnchor - bubbleYaw * (miniScale * centerLocal)
centerWorld       = captured frontAnchor when entering Bubble (stored; not recomputed per frame)
roomCenter        = centerWorld
roomPosition      = roomCenter - bubbleYaw * centerLocal
```

Dragging changes `offsetLocal` (which content point sits at the fixed center), not the center itself. Rotation keeps that same focus point at the center, so the universe turns around the planet under the view center. Scaling to Room puts that same `centerLocal` at the Room center, so the planets center on the fixed point.

Consequences:

- the center is defined on Bubble entry and stays fixed while dragging;
- the horizontal `UniverseCenterMarker` ring shows the Bubble zone at that center;
- the content point at the center in Bubble is the content point at the center in Room;
- a planet in front of the bubble view stays in front after scaling to Room;
- small Bubble drags become large Room moves (`1 / miniScale`); clamping is deferred so the center never jumps;
- `offsetLocal` and yaw persist across Room ↔ Bubble and are saved in `GameSnapshot`; they reset in `SetRoomAnchor` (new room/round) and are restored from the snapshot after the anchor loads;
- the color-scope sphere uses the same `centerWorld`, so the visual focus and the scale target agree.

**Infinite-sector coordination:** once the universe streams beyond the room (see
[Infinite sector](../backlog/universe-infinite-sector.md)), this mapping applies only
within a `roomLinkRadius` around the central galaxy. If the Bubble focus is farther
away, Bubble → Room returns to the hub/central gameplay region instead of dragging a
remote exploration focus into the room.

### 4.6 Center lifecycle

- The center is captured only when entering Bubble: initial spawn or Room → Bubble.
- Initial Bubble centers on the spawned content centroid; later Bubble visits recapture the camera-front anchor.
- While in Room the center is not recomputed.
- The horizontal `UniverseCenterMarker` ring uses `CenterWorld` and `BubbleRadiusWorld`; it is visible only in Bubble.
- Scaling Bubble → Room keeps this center fixed; returning to Bubble recaptures the front anchor and restores the saved focus.

### 4.7 Release

- Release commits the current Bubble pose.
- If the total drag is under `settleDistance` and the total yaw under `settleYaw`, restore `startBubble` instead (accidental-nudge guard).
- Routes resume after the session.

## 5. Step-by-step implementation

### Step 1 — `UniverseView` Bubble preview API (done)

Add:

- `void BeginBubblePreview()` — capture the current pose and focus point;
- `void ApplyBubblePreview(Vector3 worldOffsetFromStart, Quaternion rotation)` — move the focus by the hand offset and yaw around the fixed center; no transition;
- `void CancelBubblePreview()` — tween back to the captured Bubble pose;
- `void CommitBubblePreview()` — accept the previewed focus/yaw for this visit;
- `CenterWorld` / `BubbleRadiusWorld` for the marker and color scope;
- `SetContentCenterLocal` seeds the content centroid after spawn.

No new GameObjects; no state-machine change.

### Step 2 — Transform intent on `TwoHandGestureInput` (done)

Expose read-only values from the pinch points:

- `Vector3 Midpoint`;
- `Vector2 HandAxisXZ` normalized;
- `bool HasValidAxis` (separation above `minHandSeparation`).

The input component stays policy-free.

### Step 3 — Bubble session in `UniverseGhostController` (done)

- Start a Bubble transform session when the gesture becomes active and `universeView.IsMini`.
- Capture the fixed `CenterWorld` on Bubble entry; do not redefine it while in Room.
- While active:
  - apply midpoint drag and hand-axis yaw via `ApplyBubblePreview` (focus + rotation around the center);
  - damp translation/yaw with soft dead zones;
  - keep computing scale as today.
- On scale commit to Room: commit the focus/yaw, then transition around the same center.
- On release: commit or restore based on the settle thresholds; resume routes.
- In Room: no transform session; the gesture only scales toward Bubble as today.

### Step 4 — Bounds (done)

- Bubble offset clamps to `bubbleMoveLimit`; canonical Y is fixed.
- Yaw is continuous (no limit) in this version.
- Scale stays clamped to `MiniScale..1`.
- Room-bound clamping and `roomLinkRadius` are deferred.

### Step 5 — Feedback (done)

- Authored pinch rings and hand line are reused.
- The authored `UniverseCenterMarker` is a horizontal ring at the fixed center with radius `BubbleRadiusWorld`, so it outlines the Bubble zone; it is shown only in Bubble.
- No midpoint pivot marker or boundary sphere in this version.
- No runtime-created visuals.

### Step 6 — Validation

- Two-hand pinch in Bubble drags the map in XZ and yaws it around Y.
- Releasing commits; tiny movement restores the canonical Bubble pose.
- The center marker stays fixed while dragging and is the center after scaling to Room.
- Scale still commits Bubble → Room at the threshold; the transition opens around the center.
- Returning to Bubble preserves the saved offset/yaw.
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
| `GameSnapshot` | `hasBubbleTransform`, `bubbleOffsetLocal`, `bubbleYaw` | Extend |
| `LevelContainer` | Save on commit; restore after `SetRoomAnchor` | Extend |

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
| `translationSharpness` | `18` |
| `yawSharpness` | `14` |
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

- Room reference: the fixed player-view center (`CenterWorld`) at transition time, not the camera.
- Mapping: content-local, so the planet arrangement stays in front of the camera.
- Reference point: captured once per transition.
- Persistence: content-local offset/yaw saved in `GameSnapshot`, reset by `SetRoomAnchor`, restored after the anchor loads.

Future options:

- move/rotate Room content directly;
- add vertical movement;
- add one-hand drag mode;
- follow the player continuously instead of capturing the anchor.
