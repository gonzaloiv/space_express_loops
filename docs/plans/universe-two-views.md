# Universe — Two Views + Scale Plan

**Status:** first implementation step. Scale only; no rotation or translation.

**Related:** [Transforming the universe](universe-transform.md) · [Interaction unification](interaction-unification.md)

**Wiring rule:** components, references, and the ghost are authored in the scene/prefabs. Do not use runtime `AddComponent`, injection, or disabling of unused components. The current runtime auto-add/creation is provisional and will be removed., [Galaxies and level/round creation](universe-galaxies.md)

## 1. Goal

Introduce two presentation states for the existing `Level` root:

1. **Room** — what the game does today: content at room scale, normal routes.
2. **Bubble** — a miniature of the same content floating in front of the player.

The state is switched by the **two-hand scale gesture**:

- grab two planets with two hands;
- bring the hands **closer** to go toward Bubble;
- spread the hands **farther** to go toward Room.

While the hands move, a **boundary ghost** scales in real time as the preview. When the ghost crosses an enter/exit threshold, the real space animates to the other state.

There is no user-driven rotation or translation yet.

**Routes keep running in both states.** Ships continue along their routes and route lines/travellers scale with the universe. Path creation (ship grab/drop and destination picking) also stays enabled in Bubble; it needs scale-aware interaction distances.

The purpose is to validate:

- the two poses and their scale,
- the two-hand scale gesture and threshold commit,
- the boundary ghost,
- routes and UI continuing to work in Bubble,
- the front-of-camera bubble placement.

## 2. Non-goals

- User-driven rotation or translation of the universe.
- Continuous move/rotate/scale of the universe.
- A content ghost or render-texture preview (boundary ghost only).
- Persisting the view across app sessions.
- Redesigning the UI for the Bubble (keep the existing UI for now; redesign later).

## 3. Existing facts

- `Level` is the root containing `Hubs`, `Planets`, and `Spaceships`.
- `LevelContainer.SetRoomBasedPose()` places `Level` at the MRUK room anchor with scale `1`.
- Planets and hubs are placed in `Level` local space through `MrukRoomLocalPlacement`.
- Route splines, line renderers, and traveller paths are **world-space**:
  - `SplineContainer.EvaluatePosition()` returns world positions.
  - `TravellerPathFollower` uses a world-space `DOPath`.
  - Route line renderers use `useWorldSpace = true`.
  - This data becomes stale whenever `Level` moves, rotates, or scales.
- Therefore **content-local route/line/traveller data is required in this step**, not deferred. It is the foundation that lets routes continue normally in Bubble.
- Route completion is **animation-based**: `TravellerPathFollower` raises completion when the path tween ends, and `TravellerRouteRunner` then picks up letters. Physics triggers only add early collision/failure VFX; they are not the arrival test. This means routes do not need distance-based detection to work at mini scale; the trigger/VFX path may need tuning later.

## 4. Core model

### 4.1 Spaces and display transform

| Space | Meaning |
|---|---|
| **World space (W)** | Physical room, camera, hands, tracking. Meters. |
| **Content space (C)** | `Level` local space. Planets, hubs, ships, saved positions. |
| **Display transform `D`** | `Level` transform: position, rotation, uniform scale `s`. |

```text
world = D.position + D.rotation * (D.scale * contentLocal)
```

Use uniform scale only.

### 4.2 Content bounds and the bubble

`B_c` is a content-space sphere around the content:

- center captured from the MRUK room center at the room pose,
- radius from the room bounds (half-diagonal plus margin),
- used only to compute the mini scale and the bubble boundary.

World bubble:

```text
B_w.center = D.TransformPoint(B_c.center)
B_w.radius = B_c.radius * D.scale
```

### 4.3 The two settled poses

| Pose | Meaning |
|---|---|
| `D_room` | `scale = 1`, position/rotation from the MRUK room anchor. |
| `D_mini` | `scale = miniBubbleRadius / B_c.radius`, content center at a bubble anchor in front of the camera. |

Mini pose:

```text
s_mini = miniBubbleRadius / B_c.radius
bubbleAnchor = camera.position + camera.forward * bubbleDistance + camera.up * bubbleHeightOffset
rotation = yaw-only room rotation
D_mini.position = bubbleAnchor - rotation * (s_mini * B_c.center)
D_mini.rotation = rotation
D_mini.scale = s_mini
```

The mini pose is always recomputed **in front of the player** when entering Bubble. There is no seat memory and no user-driven rotation/translation in this step. The only translation in the system is the canonical transition between `D_room` and `D_mini`.

**Rotation is Y-axis only.** Both `D_room` and `D_mini` keep pitch and roll at zero (yaw-only); never feed the room anchor's pitched/rolled euler angles directly into the view.

### 4.4 View states

```text
Room ──ghost crosses exitRoomScale──> Transitioning ──> Bubble
Bubble ──ghost crosses enterRoomScale──> Transitioning ──> Room
```

`D` only moves between `D_room` and `D_mini`. The two-hand gesture drives the **ghost scale**, not `D` directly; `D` changes only when a threshold commits and the transition animation runs.

## 5. Transition behavior

### 5.1 Two-hand scale gesture

The view switch is the scale system:

- pinch/ grab **two planets** with two hands;
- bring the hands closer to shrink the ghost toward Bubble;
- spread the hands farther to grow the ghost toward Room.

One-hand vs two-hand clarity:

```text
Idle
  └─ first planet grab ──> WaitingForSecondHandle (highlight + prompt)
        ├─ second planet grab ──> TwoHandScaleActive (ghost scales)
        └─ release ──> Idle (no transform)
```

- A single planet grab never scales the universe; it only shows “grab a second planet”.
- Ship grab and path creation stay one-hand interactions and are not affected by a single planet grab.
- Two distinct planets and two distinct hands are required.
- A debug `[Button]` toggle can remain for development, but it is not the intended switch.

### 5.2 Ghost scale and threshold commit

While `TwoHandScaleActive`:

```text
ghostScale = clamp(startScale * pow(handDistance / startHandDistance, scaleGain), sMin, sMax)
```

- the ghost center is the **midpoint of the two grabbing hands**;
- recommended `scaleGain = 2.5`; the hands cannot span mini→room 1:1, so gearing gives them leverage;
- dead zone under ~3–5% to avoid jitter;
- rebase the reference when a limit is hit so reversing responds immediately;
- the ghost boundary scales in real time;
- the real `D` does **not** change yet.

Commit:

- growing past `enterRoomScale` (for example `0.60`) animates `D` to `D_room`;
- shrinking past `exitRoomScale` (for example `0.45`) animates `D` to `D_mini`;
- hysteresis prevents flicker;
- during the commit animation, end the scale session and ignore new scale input;
- after the animation, snap to the exact pose and raise `ViewChanged`.

Transition animation:

- tween position and uniform scale over about `0.6 s` (`InOutSine`);
- rotation stays constant: both `D_room` and `D_mini` share the same Y rotation.

### 5.3 Bubble placement

Always place the Bubble **in front of the current player pose**:

- `bubbleAnchor` is derived from the camera position and forward direction every time Bubble is entered;
- no `lastMiniPose` restore;
- no user-driven translation or rotation;
- a small dedicated placement helper is acceptable if it keeps this logic isolated and testable.

This is intentionally simple: the bubble is placed in front when entering Bubble and stays fixed while the player moves. It is recomputed in front the next time Bubble is entered. A remembered seat can be revisited only if a later interaction explicitly needs it.

### 5.4 Route/traveller handling

Routes and path creation **keep running in the settled Bubble and Room states**:

- travellers continue moving along their routes; completion is animation-based, so no distance-based arrival detection is needed;
- route lines remain visible and scale with the universe;
- route panels and grab zones remain visible for testing;
- path creation (ship grab/drop, destination ray) stays enabled in Bubble.

During the active scale gesture and the commit animation, pause route progression and ignore path-creation input; resume once the view is settled. This avoids moving ships/routes while the root is changing.

To make this work, route data must be content-local and interaction distances must be scale-aware:

- `RouteLeg.positions` stores `Level`-local positions;
- `SplineLegSampler` converts sampled world positions with `ContentRoot.InverseTransformPoint`;
- `LineRendererWithEdges` uses `useWorldSpace = false` and local positions;
- `TravellerPathFollower` uses `DOLocalPath` relative to a stable path space;
- APIs that need world positions (grab zone, ray, panel anchors) convert local → world at the call site;
- `RaycastHelper` ray length and `GrabbableWrapper`/grab-zone sizes must scale with `D.scale` (or be computed in content space) so path creation still works at mini scale;
- physics triggers remain for early collision/failure VFX; they are not the arrival test, so missing them at tiny scale does not break route completion.

Because the path, line, and ship transforms all live under `Level`, the entire route continues correctly at any view scale.

### 5.5 New-round spawning

If a round spawns while the view is Bubble:

- temporarily apply `D_room`,
- run the existing placement/spawn code,
- restore the previous view.

Wrap this in `UniverseView.WithRoomPose(Action)` so the temporary pose cannot leak.

## 6. Bubble boundary ghost

The boundary is both the bubble visual and the scale preview:

- scene-level object, not under `Level`;
- follows the current `D`/ghost center and radius;
- constant world-space line thickness;
- during `TwoHandScaleActive`, it scales in real time with the hand separation;
- at the threshold it represents the target state; on commit it eases to the real `B_w`;
- fades out when the camera is inside the bubble;
- in Room state it is hidden once the transition completes.

A wireframe/translucent sphere is enough for this step. Do not build a content ghost or render-texture preview.

## 7. Component map

Prototype note: `UniverseView` currently lives in `DigitalLove.Game.Levels` so the two-view foundation can share the existing Level assembly. Move it to a dedicated `DigitalLove.Game.Universe` asmdef when the planet handles and scale coordinator arrive in Phase A2. It must be authored on the `Level` scene object; runtime auto-add is temporary and will be removed.

New under `Assets/_Game/Features/Universe/` with asmdef `DigitalLove.Game.Universe`:

| Type | Responsibility |
|---|---|
| `UniverseView` | Owns `D`, `D_room`, `D_mini`, view state, room anchor, transitions, `WithRoomPose`, events. |
| `UniverseBubble` | Content-space bounds and world bubble center/radius. |
| `UniverseBubbleView` | Boundary ghost visual and fade. |
| `UniverseBubblePlacement` | Computes the bubble pose **always in front of the current player**. |
| `PlanetUniverseHandle` | Per-planet pinch handle for the two-hand scale gesture; does not move the planet. |
| `UniverseGhostController` | Owns the one-hand/two-hand state machine, scale session, ghost preview, thresholds, route pause, and commit. References `UniverseView` and the ghost visual through serialized fields. |
| `UniverseGhostView` | Pure authored ghost visual: three LineRenderers in the scene/prefab; show/hide, position, scale, width. No runtime creation. |
| `UniverseViewDebug` | Optional `[Button]`/temporary toggle for development. |

Modified:

- `LevelContainer` — delegate room pose to `UniverseView`; temporary room pose for spawn.
- `RouteLeg`, `SplineLegSampler`, `SplineContainerWrapper`, `SplineContainerExtensions` — content-local route data.
- `LineRendererWithEdges`, `GrabZone` — local-space route lines and refresh.
- `TravellerPathFollower`, `TravellerRouteRunner`, `TravellerBehaviour` — local-space path following.
- `SpaceshipRouteSession`, `SpaceshipDestinationSelection` — local ↔ world conversion at interaction boundaries.
- `RaycastHelper`, `GrabbableWrapper`, `DestinationSelector` — scale-aware ray length and grab-zone sizes for path creation in Bubble.
- `Game.unity` — `UniverseView`, planet handles, and boundary ghost wiring.
- `RoutePanel` / `ResourcePanel` — keep visible in Bubble for testing; redesign later.

## 8. Phases

### Phase A0 — Content-local routes and scale-aware interactions (prerequisite)

- Convert `RouteLeg.positions` to `Level`-local space.
- Update `SplineLegSampler`, `SplineContainerWrapper`, `LineRendererWithEdges`, and `TravellerPathFollower`.
- Convert back to world at the ray/grab-zone/panel boundaries.
- Make `RaycastHelper` ray length and `GrabbableWrapper`/grab-zone sizes scale with `D.scale`.
- Verify normal gameplay at room scale is unchanged.

**Exit:** moving/scaling `Level` keeps route lines, travellers, collisions, and path creation working. This is what allows routes and path creation to continue in Bubble.

### Phase A1 — View poses and transition

- Implement `UniverseView` with `D_room`, `D_mini`, and the animated transition.
- Add `UniverseView` to the `Level` object in `Game.unity` and serialize its references; remove the runtime auto-add.
- Wire `LevelContainer.SetRoomBasedPose` to `UniverseView.SetRoomAnchor`.
- Gate all view math on MRUK room bounds/anchor readiness.
- Add an optional debug toggle to validate the poses before the gesture exists.
- Add `WithRoomPose` for new-round spawning; expose an `IsSpawning` gate.
- Validate with an active route while switching Room ↔ Bubble.

**Exit:** the universe can move between Room and Bubble, routes keep running in both, and the Room pose restores exactly.

### Phase A2 — Two-hand scale gesture and ghost commit

- Add a preconfigured `PlanetUniverseHandle` child to the planet template with a scale-friendly collider on a dedicated handle layer, excluded from the destination ray mask.
- Add the shared `Grabbable` proxy and the `UniverseGhostView` hierarchy to the scene.
- Add `UniverseGhostController` to `Level` in the scene with serialized references to `UniverseView`, the ghost visual, planets/spaceships spawners, and both hands; remove runtime creation.
- Implement the one-hand/two-hand state machine and scale session inside the controller.
- Scale the boundary ghost in real time from hand separation, centered on the hand midpoint.
- Commit to Room/Bubble at the thresholds with hysteresis and the A1 transition.
- Gate the gesture while spawning and until two planets exist.
- Add first-grab/second-grab and limit feedback.

**Exit:** grabbing two planets and moving them closer/farther switches the universe between Bubble and Room, with the ghost previewing the target state.

### Phase A3 — Readability and polish

- Keep all existing UI visible in Bubble; do not hide panels.
- Tune mini radius, distance, height, transition time, gearing.
- Tune the minimum handle size for tiny planets.
- Note UI redesign and physics-trigger VFX tuning as later tasks.

**Exit:** the gesture feels understandable, the Bubble reads clearly, and routes/UI/path creation remain present.

## 9. Test plan

### Manual

1. One planet grab: no scale, shows “grab a second planet”.
2. Two planet grabs: ghost scales in real time with hand separation.
3. Hands closer past `exitRoomScale`: commit animates to Bubble.
4. Hands farther past `enterRoomScale`: commit animates to Room.
5. Hysteresis prevents flicker around the threshold.
6. With a route running: travellers keep moving and route lines stay attached in Bubble.
7. During the scale gesture/commit, route progression pauses; it resumes when settled.
8. Path creation still works in Bubble (ship grab/drop, destination ray) outside the active gesture.
9. Route panels and grab zones remain visible in Bubble.
10. Start a new round while in Bubble: content spawns correctly and returns to the bubble view.
11. Move the player, enter Bubble: the mini view is in front of the player.
12. Check that the boundary ghost scales and fades correctly.
13. Same planet twice / same hand twice is rejected.

### Automated

- `UniverseView` mini pose math:
  - `D_mini` maps `B_c.center` to the bubble anchor,
  - scale equals `miniBubbleRadius / B_c.radius`,
  - room pose restores scale `1`.
- `UniverseBubblePlacement` always returns a pose in front of the camera.
- `UniverseScaleSession` gearing, dead zone, clamp, rebase, and hysteresis.
- Route positions remain content-local round-trip stable (`local -> world -> local`).

## 10. Risks and decisions

| Risk / decision | Decision |
|---|---|
| Routes in Bubble | Routes **continue** when settled. Content-local data; pause progression only during the active scale gesture/commit. |
| Route arrival at mini scale | Completion is animation-based, so no distance-based detection is needed; physics triggers are only for early collision/VFX. |
| Mini scale too small/large | Compute from room bounds; tune `miniBubbleRadius`. |
| UI readability in Bubble | Keep the existing UI visible; redesign is a later task. No hiding panels for now. |
| Path creation in Bubble | Keep enabled; make ray length and grab-zone sizes scale-aware. |
| Rotation / translation | No user-driven rotation or translation. Rotation is **Y-axis only** (yaw); pitch/roll stay zero. |
| One hand vs two hands | Single planet grab = waiting/prompt only; two planets + two hands = scale. Ship grab stays one-hand. |
| Hand separation range | Gearing (`scaleGain`) plus threshold commit; explain in the gesture UI. |
| Ghost fidelity | Boundary ghost only; content ghost/render preview are not planned. |
| Bubble placement | Always recompute in front of the current player; dedicated placement script is fine. |
| Tiny planets | Try the minimum handle world radius and evaluate the feel. |
| Route panels/grab zones | Keep them visible for testing. |

### Additional risks and decisions

- **Room-scale reachability of two planets.** Accepted for now. Use the planets; no fallback UI or empty-space gesture yet. Revisit only if testing shows it is unusable.
- **Runner-level route pause.** Adopt an explicit pause state on the route runner, not just the tween. Make it reusable for the future real pause button.
- **Mini-scale failure detection.** Test first. Arrival is animation-based; if wrong-planet failure or VFX misses too much at mini scale, add a scale-independent proximity check later.
- **MRUK readiness.** Hard prerequisite: no view calculation or transition until MRUK room bounds/anchor are ready. On anchor relocation/app resume, reset to Room or recompute the mini pose.
- **Ghost-to-commit continuity.** The ghost center is the **midpoint of the two grabbing hands**. On commit, tween from the ghost pose to the target pose (always-in-front Bubble or Room) so the recenter is part of the transition rather than a jump.
- **Mini-scale path creation.** Test first. Keep scale-aware ray length and grab-zone sizing; add minimum world-space ship/grab proxies only if testing shows they are needed.
- **Handle collider vs destination ray.** Use a dedicated layer for planet handles and exclude it from the ship’s destination ray mask. This is feasible and required.
- **Fewer than two planets.** The content guarantees at least two planets; gate the scale gesture until two handles exist.
- **New-round spawn.** Do not allow a view transition while spawning. Gate the gesture with an `IsSpawning` state and keep `WithRoomPose` scoped to the whole spawn.
- **Bubble placement collision.** Use the MRUK room/anchor components to check for walls, furniture, and other room elements; clamp or offset the bubble anchor so it does not intersect them.

## 11. Defaults

| Parameter | Default |
|---|---|
| `miniBubbleRadius` | `0.22 m` |
| `bubbleDistance` | `0.45 m` |
| `bubbleHeightOffset` | `-0.10 m` |
| `roomTransitionSecs` | `0.6 s` |
| `minMiniScale` | `0.02` |
| `scaleGain` | `2.5` |
| `enterRoomScale` | `0.60` |
| `exitRoomScale` | `0.45` |
| `minHandSeparation` | `0.08 m` |
| `minHandleWorldRadius` | `0.022 m` |

## 12. Out of scope

- User-driven rotation or translation.
- Pitch/roll rotation (rotation is Y-axis only).
- Continuous transform of the universe.
- Content ghost or render-texture preview.
- Persistent view state.
- UI redesign for Bubble (keep the existing UI for now).
