# Universe — Bubble Map Drag

**Status:** done - implementation complete; headset validation and tuning deferred.

**Post-completion:** Room/Bubble now uses authored scene anchors (`RoomAnchor`/`BubbleAnchor`) through `UniverseContentRig`. A bubble drag moves the anchor, so the center ring travels with the map instead of staying fixed.

**Superseded (view/pose model):** The view/pose model here — content reparented between anchors, `bubbleOffsetLocal`/`bubbleYaw`, `bubbleMoveLimit`, and the ring marker tied to the content center — is superseded by [View frame (bubble window)](universe-bubble-window.md), which makes a box frame `{F, viewRadius, yaw}` the source of truth over a universe fixed in content space. The gesture, input, commit, and route-pause work below survives; only the pose/scale basis and the marker shape change. This plan's deferred headset validation and tuning (`Next step`) are carried into that slice so they run once, against the frame.

**Related:** [Two views + interaction unify](universe-views-and-interaction.md) · [Space shaping](space-shaping.md) · [Sector field](../backlog/universe/universe-infinite-sector.md) · [Color scope](universe-color-scope.md)

**Wiring rule:** all visuals and refs stay authored in the scene/prefabs. No runtime `AddComponent`, `GetComponent`, or hierarchy creation.

## Next step

Validate in the headset: one-hand drag moves the map like a maps app, two-hand pinch scales without translating, the center ring reacts to drag/scale/recenter, and Room scaling keeps the planets centered on the marker. Bubble must not allow ship grabbing or route editing. Then tune `translationGain`, `translationSmoothSecs`, `recenterDistance`, `recenterHoldSecs`, `heightOffset`, and `transitionSecs`.

## 1. Goal

Let the Bubble behave like a draggable map: one-hand pinch drags it in XZ, while two-hand pinch changes only the scale and Room/Bubble commit. No in-gesture rotation.

The center is captured **when entering Bubble** (initial spawn or Room → Bubble), not while in Room. It stays fixed for that Bubble visit; dragging changes which content point is shown there, and Room scaling puts that same content point at the same center. The Bubble content center itself is seeded from the spawned hubs/planets, not from the room bounds.

## 2. Decision summary

| Decision | Choice |
|---|---|
| Initial view | **Bubble**, centered in front of the camera |
| First view to transform | **Bubble only** |
| Room | Bubble-view-center reference at transition time; no direct Room editing |
| Center point | Captured when entering Bubble and kept fixed; the map content moves inside it |
| Center source | Seeded from the spawned hubs/planets (content centroid), not room bounds |
| Room reference point | The fixed Bubble center, so Room moves/centers on it |
| Offset mapping | Bubble offset stored content-local, so the same planets stay in front after scaling |
| Translation axes | XZ only; Y fixed |
| Settle | Release commits; a tiny total movement (`settleDistance`) reverts |
| Drag | One-hand pinch **inside the bubble circle** drags the map; adding the second pinch upgrades to scale and freezes translation |
| Scale | Two-hand pinch after `dwellSecs = 0.12`; no rotation |
| Route editing | Room only. Bubble disables ship grab/route editing and blocks map drag/scale while any selection is active |
| Recenter | In Scale mode only, close both hands below `recenterDistance` and hold `recenterHoldSecs` in Bubble |
| View cooldown | Ignore gestures for `viewChangeCooldownSecs` after a Room/Bubble transition (and at start) |
| Drag/scale separation | Adding a second pinch stops translation and starts scaling; no mixed updates |
| Drag start | First movement is latched; a large tracking jump rebases instead of moving the map |
| Bubble edits on Room transition | Persisted through the content-local offset; Bubble re-entry resets to the canonical pose so the center and content stay aligned |
| Save data | `bubbleOffsetLocal` + `bubbleYaw` stored in `GameSnapshot`; restored after the room anchor loads |

Bubble-only is the smaller first version:

- no room-bound clamping for content that surrounds the player;
- no editing of the canonical room pose;
- the map is directly in front of the player, so drag feedback is obvious;
- a failed experiment only affects the Bubble session.

## 3. Starting point

- `UniverseView` owns the Room pose, the Bubble pose, `SetBubbleView`, and the transition tween.
- The app starts in Bubble: after the room anchor loads, the universe is centered in front of the camera.
- The Room pose is centered on the room; the player is inside the content after scaling up.
- Bubble drag and saved offsets apply to the initial Bubble too.
- `TwoHandGestureInput` exposes left/right pinch points, `Midpoint`, `HandDistance`, activation, and per-hand pinch state.
- `UniverseGhostController` maps hand distance to preview scale and commits at `exitRoomScale` / `enterRoomScale`.
- Routes are content-local and pause through `SpaceshipsSpawner.SetViewTransition`.
- Authorized feedback: per-hand pinch rings and the hand-to-hand line.

## 4. Bubble transform model

### 4.1 Session start

On gesture activation in Bubble:

```text
startPose     = universeView.CurrentPose
startPoint = gestureInput.DragStartPoint
```

Reject when the hand separation is below `minHandSeparation`.

### 4.2 Translation (one-hand drag)

One hand pinches empty space and drags like a maps app. Adding the second pinch freezes translation and starts scale around the same fixed center.

```text
delta      = currentDragPoint - pinchStartPoint
offset     = pinchStartOffset + delta * translationGain
offset     = ClampXZ(offset, BubbleRadiusWorld * 0.3)
smoothed   = SmoothDamp(smoothed, target, translationSmoothSecs, dt)
D.position = startBubble.position + smoothed
```

- `translationGain = 1.0` for direct map-style dragging;
- translation is measured from the pinch-start hand position and the offset of the universe at that moment, so switching hands or adding a second hand cannot jump;
- the drag keeps the original hand as its point; the second hand only starts scale;
- drag limit is `BubbleRadiusWorld * 0.3`, inside the ring; near it the drag rubber-bands;
- `translationSmoothSecs` defaults to `0.08` (lower = snappier, higher = smoother);
- Y is fixed to the canonical bubble height.

### 4.3 Rotation

No in-gesture rotation in this version. The player turns their head instead; the Bubble stays where it is in world space.

- `D.rotation` stays at the room/Bubble yaw.
- Yaw remains in the save data for compatibility but is not edited by the gesture.

### 4.4 Scale and Room commit

Scale keeps the existing behavior:

```text
ghostScale = clamp(startScale * pow(handDistance / startHandDistance, scaleGain), miniScale, 1)
```

- in Bubble, spreading both hands to `enterRoomScale` commits to Room;
- the Bubble offset stays inside the fixed Bubble center;
- Room uses the same fixed center and focus, so scaling up moves the room view around it;
- the Bubble offset is converted to content-local space and remains saved for reload;
- going back to Bubble resets the offset and yaw to the canonical camera-front pose so the center and content stay aligned.

The first version keeps the center fixed and does not clamp the Room pose to the room bounds; clamping is a later refinement if content leaves the room.

### 4.5 Room reference and mapping (decided)

The Bubble center is a **fixed world point** captured at Bubble entry. It does not move while dragging or scaling. On entry the Bubble yaw comes from the camera, so the map faces the player. Room scaling reads the **actual current Bubble rotation** and the same center/focus, so a hub on the bubble’s right stays on that same world right in Room.

```text
centerLocal    = contentCenter + offsetLocal       // content point shown at the Bubble center
bubblePosition = frontAnchor - bubbleYaw * (miniScale * centerLocal)
centerWorld    = captured frontAnchor when entering Bubble
roomPosition   = centerWorld - currentBubbleYaw * centerLocal
```

Dragging changes `offsetLocal` (which content point sits at the fixed center), not the center itself. Scaling to Room puts that same `centerLocal` at the Room center, so the planets center on the fixed point.

Consequences:

- the center is defined on Bubble entry and stays fixed while dragging;
- the horizontal `UniverseCenterMarker` ring shows the Bubble zone at that center;
- the content point at the center in Bubble is the content point at the center in Room;
- a planet in front of the bubble view stays in front after scaling to Room;
- small Bubble drags become large Room moves (`1 / miniScale`); clamping is deferred so the center never jumps;
- `offsetLocal` is saved in `GameSnapshot`; it resets on Bubble entry, in `SetRoomAnchor` (new room/round), and is restored from the snapshot after the anchor loads;
- the color-scope sphere uses the same `centerWorld`, so the visual focus and the scale target agree.

**Infinite-sector coordination:** once the universe streams beyond the room (see
[Infinite sector](../backlog/universe/universe-infinite-sector.md)), this mapping applies only
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
- Scaling under `recenterDistance` for `recenterHoldSecs` tweens the map back inside the fixed center over `transitionSecs`; there is no jump/revert tween.
- One guard: restore `startBubble` only when combined movement (`translation + 0.001 m/degree`) is under `settleDistance`; otherwise commit.
- Routes resume after the session.

## 5. Step-by-step implementation

### Step 1 — `UniverseView` Bubble preview API (done)

Add:

- `void BeginBubblePreview()` — capture the current pose and focus point;
- `void ApplyBubblePreview(Vector3 worldOffsetFromStart)` — move the focus by the hand offset around the fixed center; no transition;
- `void CancelBubblePreview()` — tween back to the captured Bubble pose;
- `void CommitBubblePreview()` — accept the previewed focus for this visit;
- `CenterWorld` / `BubbleRadiusWorld` for the marker and color scope;
- `SetContentCenterLocal` seeds the content centroid after spawn.

No new GameObjects; no state-machine change.

### Step 2 — Transform intent on `TwoHandGestureInput` (done)

Expose read-only values from the pinch points:

- `Vector3 DragPoint` / `DragStartPoint`;
- `Vector2 HandAxisXZ` normalized;
- `bool HasValidAxis` (separation above `minHandSeparation`).

The input component stays policy-free.

### Step 3 — Bubble session in `UniverseGhostController` (done)

- Start a drag session only when the one-hand pinch starts inside the bubble circle; the drag keeps that hand even after the second pinch, which upgrades to scale (`dwellSecs`) without translating.
- Capture the fixed `CenterWorld` on Bubble entry; do not redefine it while in Room.
- While active:
  - in Drag mode, apply one-hand midpoint drag via `ApplyBubblePreview` (focus moves around the fixed center);
  - accumulate per-frame deltas and damp the result;
  - keep computing scale as today.
- On scale commit to Room: commit the focus, then transition around the same center.
- On release: commit or restore based on the settle thresholds; resume routes.
- In Room: no transform session; the gesture only scales toward Bubble as today.

### Step 4 — Bounds (done)

- Bubble offset uses the ring radius as a soft limit with rubber-band resistance; canonical Y is fixed.
- No in-gesture rotation; head movement changes the view.
- Scale stays clamped to `MiniScale..1`.
- Room-bound clamping and `roomLinkRadius` are deferred.

### Step 5 — Feedback (done)

- Authored pinch rings and hand line are reused.
- The authored `UniverseCenterMarker` is a horizontal ring at the fixed center, Bubble only, with radius `BubbleRadiusWorld * ringRadiusFactor` (`0.4` default). It turns orange when route editing blocks gestures, cyan while a map drag is active, and blends toward green as scale approaches a commit threshold. Colors are applied through a material property block so the URP line shader picks them up; `heightOffset` aligns the ring with the planet plane.
- The hand line thickens with hand-distance speed (`lineMaxMultiplier`, `lineTensionSpeed`).
- No midpoint pivot marker or boundary sphere in this version.
- No runtime-created visuals.

### Step 6 — Validation

- One-hand pinch **inside the bubble circle** drags the map in XZ like grabbing the space; pinches outside do nothing. Two-hand pinch only scales.
- Closing both hands below `recenterDistance` and holding resets the map to the fixed center.
- Ship grab/route editing is disabled in Bubble and re-enabled in Room.
- Gestures are ignored for `viewChangeCooldownSecs` after a view change or at start.
- A two-hand scale never moves the Bubble center; Room centers on that same fixed point.
- Bubble yaw is captured from the camera on entry; Room reuses that yaw, so left/right/front stay consistent between Bubble and Room.
- Releasing commits; tiny movement restores the canonical Bubble pose.
- The center marker stays fixed while dragging and is the center after scaling to Room.
- Scale still commits Bubble → Room at the threshold; the transition opens around the center.
- Returning to Bubble preserves the saved offset.
- Routes pause during the session and resume after.
- Routes and travellers remain attached to the transformed content.
- Path creation still works in Bubble.
- No activation while a ship or UI is selected.

## 6. Component map

| Type | Responsibility | Change |
|---|---|---|
| `UniverseView` | Bubble pose, transitions, preview API | Extend |
| `TwoHandGestureInput` | Drag point, start point, hand pinch state | Extend |
| `UniverseGhostController` | Bubble session, drag mapping, settle | Extend |
| `UniverseGhostView` | Rings, line, optional pivot marker | Extend only if needed |
| `SpaceshipsSpawner` | Route pause/resume and station refresh | Existing API |
| `GameSnapshot` | `hasBubbleTransform`, `bubbleOffsetLocal`, `bubbleYaw` | Extend |
| `LevelContainer` | Save on commit; restore after `SetRoomAnchor` | Extend |

No planet/hub handles, no boundary ghost, no runtime-created visuals.

## 7. Defaults

| Parameter | Default |
|---|---|
| `translationGain` | `1.0` |
| `minHandSeparation` | `0.06 m` |
| `dwellSecs` | `0.12 s` before two-hand scale activates |
| `settleDistance` | `0.01 m` |
| `lineMaxMultiplier` | `2.5` |
| `lineTensionSpeed` | `0.6 m/s` |
| `bubbleMoveLimit` | `BubbleRadiusWorld * 0.3` |
| `translationSmoothSecs` | `0.08 s` (scene uses `0.1`) |
| `dragStartJumpLimit` | `0.12 m` |
| `minDragDistance` | `0.03 m` before translation starts |
| `recenterDistance` | `0.05 m` (pinch rings touch) |
| `recenterHoldSecs` | `0.35 s` |
| `viewChangeCooldownSecs` | `0.35 s` |
| `minDragDistance` | `0.03 m` before drag starts |
| `transitionSecs` | `0.45 s` |
| `roomLinkRadius` | `5 m` around the central galaxy |
| `scaleGain` | `2.5` |
| `enterRoomScale` | `0.60` |
| `exitRoomScale` | `0.45` |

## 8. Risks

| Risk | Mitigation |
|---|---|
| Drag and scale fight each other | Second pinch freezes translation; only hand distance drives scale. |
| Bubble pushed out of reach | Clamp to the ring radius; recenter resets it. |
| Disorientation | No in-gesture rotation; keep the map at chest height and let head movement change the view. |
| Far Bubble focus mapped into Room | Limit the mapping to `roomLinkRadius`; farther commits return to the central hub. |
| Bubble edits lost on invalid focus | Reset to the central Bubble pose when the focus is outside the room-linked region. |
| Gesture conflicts with routing | Session pauses routes and uses the existing busy-hand gate. |
| Accidental nudges | Release settle thresholds restore the canonical pose. |

## 9. Open decisions

None. Decided for this version:

- Room reference: the fixed Bubble center, so Room moves around it.
- Mapping: content-local, so the planet arrangement stays in front of the camera.
- Reference point: captured once per transition.
- Persistence: content-local offset saved in `GameSnapshot`, reset by `SetRoomAnchor`/Bubble entry, restored after the anchor loads. Yaw is stored for compatibility but not edited.

Future options:

- move/rotate Room content directly;
- add vertical movement;

- follow the player continuously instead of capturing the anchor.
