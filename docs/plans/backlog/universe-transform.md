# Universe — Transform Plan

**Status:** backlog - follow-up after the [two views](../in-progress/universe-views-and-interaction.md) are working.

**Related:** [Two views + interaction unify](../in-progress/universe-views-and-interaction.md)

**Wiring rule:** ghost, handles, proxies, and coordinator references are authored in the scene/prefabs. No runtime `AddComponent`, injection, or disabled unused components.

## 1. Goal

Let the player move and rotate (Y axis only) the universe, and optionally scale it continuously, building on the two-views plan.

The two-views plan already delivers:

- Room/Bubble states and the animated transition;
- the two-hand planet scale gesture;
- the boundary ghost and threshold commit;
- content-local routes so routes and path creation keep working at any scale.

Plan B adds:

1. **Yaw rotation and translation** — first as a ghost preview with threshold commit (same model as scale), then optionally continuous.
2. **Option B: continuous similarity transform** — move + Y rotation + scale following the hands directly, if the discrete/ghost model is not enough.

## 2. Non-goals for the first transform step

- Continuously transforming the live universe under the hands.
- Moving the XR origin or the MRUK room.
- Pitch/roll rotation; the universe stays upright and rotates on Y only.
- A content ghost or render-texture preview.

## 3. Starting point from the two-views plan

- `Room` / `Bubble` states with animated transitions.
- Two-hand planet scale gesture with boundary ghost and thresholds.
- One-hand vs two-hand state machine: single planet grab only prompts; two planets scale.
- Content-local route/line/traveller data; routes and path creation work at any scale.
- Route completion is animation-based; physics triggers are only early collision/VFX.

## 4. Ghost preview for rotation and translation

Scale is already handled in the two-views plan. This plan extends the same commit model to Y rotation and translation.

### 4.1 What the player sees

- The player drives a preview (yaw and/or translation) with the hands.
- The boundary ghost shows the target position, radius, and yaw.
- The real world stays in Room or Bubble.
- When the preview crosses the commit threshold, the real space animates to the target and the ghost disappears.

### 4.2 Boundary ghost only

The boundary ghost is enough:

- it shows the target bubble position, radius, and yaw;
- it avoids duplicating content;
- it keeps the real content in known states while the gesture is active.

Do not build a content ghost or render-texture preview.

### 4.3 Gearing (why not 1:1)

Hand distance has a small range (roughly 0.1–1.2 m). Mini scale might be ~0.1 while room scale is 1.0, so a 1:1 mapping would require about 10x hand separation, which is impossible.

Gearing multiplies hand movement:

```text
ghostScale = clamp(startScale * pow(handDistance / startHandDistance, scaleGain), sMin, sMax)
```

- `scaleGain = 2.5` means a 2x hand separation gives `2^2.5 ≈ 5.7x` scale;
- a threshold commit then animates the remaining distance to the settled state;
- a dead zone and rebase-at-limit keep the gesture responsive.

Rotation and translation use the same idea: geared hand movement drives a preview, and commit happens at a threshold.

### 4.4 Bubble placement and ghost center

Keep the two-views rule: the bubble is **placed in front of the current player when entering Bubble** and stays fixed while there; no seat memory.

The ghost center is the **midpoint of the two grabbing hands**. On commit, tween from that ghost pose to the target pose (always-in-front Bubble or Room) so the recenter is part of the transition.

Wait for MRUK room bounds/anchor readiness before computing the ghost or committing. Do not allow a transition while a round is spawning; the content guarantees at least two planets.

## 5. Two-hand input

### 5.1 Planet handles

The two-views plan already adds `PlanetUniverseHandle` with a scale-friendly collider on a dedicated handle layer. Plan B reuses it for yaw and translation. The handle layer is excluded from the ship’s destination ray mask so path creation is not blocked.

### 5.2 One hand vs two hands

Same state machine as the two-views plan:

```text
Idle
  └─ first planet grab ──> WaitingForSecondHandle (prompt only)
        ├─ second planet grab ──> TwoHandActive (preview/transform)
        └─ release ──> Idle
```

- one hand never transforms the universe;
- two distinct planets and two distinct hands are required;
- ship grab and path creation stay one-hand and are not affected by a single planet grab;
- feedback: highlight the first handle, prompt for the second, show the ghost when active.

### 5.3 Coordinator

`UniverseGhostController`:

- requires two distinct planets and two distinct hands/interactors;
- rejects the same planet twice and the same hand twice;
- starts a session on the second select;
- ends on release, tracking loss, state switch, or planet hide;
- feeds hand poses into the preview/transform;
- raises feedback events (`FirstHandleGrabbed`, `SessionStarted`, `SessionEnded`, `LimitReached`).

Input gating:

- while a transform session is active, pause ship grab and destination picking;
- keep hand ray/poke available for UI;
- disable planet handles outside the transform interaction.

## 6. Option B — Continuous similarity transform

Only pursue this if the ghost/discrete commit does not provide enough control.

### 6.1 Session start

On the second planet grab:

- `a0`, `b0` = hand/grab contact poses in world space;
- `D0` = current display transform;
- `s0 = D0.scale`;
- `d0 = |b0 - a0|`, `mid0 = (a0 + b0) / 2`;
- reject if `d0 < minHandSeparation` or the handles are not distinct.

### 6.2 Per-frame solve

```text
d1    = |b1 - a1|
raw   = d1 / max(d0, epsilon)
sRaw  = s0 * pow(raw, scaleGain)
s     = clamp(sRaw, modeMinScale, modeMaxScale)

qDeltaRaw = FromToRotation(b0 - a0, b1 - a1)
qDelta    = ConstrainYaw(qDeltaRaw)          // Y-axis rotation only
mid1      = (a1 + b1) / 2

q         = ConstrainYaw(qDelta * D0.rotation)
p         = mid1 - qDelta * (s / s0) * (mid0 - D0.position)
```

Apply to `Level`:

```text
D.position = p
D.rotation = q
D.scale    = s
```

With `scaleGain = 1` and no clamps, the content points under the hands stay under the hands. With gearing/clamping, the midpoint stays anchored and the universe zooms around the hands.

### 6.3 Rotation axis

Rotation is **Y-axis only**. Ignore hand roll and vertical tilt for now; constrain the final rotation to yaw, and keep pitch/roll at zero in both states. This also keeps the room floor level and makes the ghost/discrete transition predictable.

### 6.4 Constraints

1. Clamp scale to the current state range.
2. Clamp translation so the bubble center stays in front of the camera, within reach, above the floor, and inside the room in Room state.
3. Constrain rotation to Y-axis only in both states.
4. Rebase the scale reference when a limit is hit.

### 6.5 Routes during transform

The two-views plan already makes route/line/traveller data content-local, so it is scale- and rotation-agnostic.

While a transform gesture is active (ghost or continuous):

- pause the **route runner state**, not just the path tween, so delays and legs do not advance;
- keep route lines hidden or frozen so they do not fight the changing root;
- on commit/release, resume the runner; content-local paths remain valid at the new scale and rotation.

Physics triggers are not the arrival test; route completion is animation-based, so a brief pause does not break pickup.

## 7. Component map

| Type | Responsibility | Phase |
|---|---|---|
| `UniverseGhostView` | Boundary ghost; extends from scale to yaw/translation preview. | Two views A2 / B0 |
| `UniverseGhostSession` | Start pose, hand distance, dead zone, clamp/rebase. | Two views A2 |
| `PlanetUniverseHandle` | Per-planet pinch handle; no planet movement. | Two views A2 |
| `UniverseGhostController` | Two-hand acquisition, validation, ghost preview, session lifecycle, commit. | Two views A2 |
| `UniverseTransformGate` | Pauses routes during an active transform and resumes after commit. | B0 |
| `UniverseSimilaritySolver` | Pure two-pose similarity math and tests. | Option B |
| `TwoHandUniverseSession` | Continuous session state, smoothing, limits. | Option B |
| `UniverseRouteRefactor` | Content-local route/line/traveller data. | Two views A0 |
| `UniverseStateController` | Enables/disables ship, planet, and ghost interaction. | Two views A2 / B0 |

## 8. Phases

### Phase B0 — Yaw and translation preview

- Extend the two-views ghost to preview target yaw and/or position (Y-axis rotation only).
- Commit to the real state with the same threshold/animation model.
- Pause routes during the active gesture; resume on commit/release.
- Keep the real content in Room or Bubble.

**Exit:** the player can preview and commit a yaw/position change without continuous live transform.

### Phase B1 — Option B continuous transform (optional)

- Implement the similarity solver and continuous session.
- Reuse content-local route data from the two-views plan.
- Add continuous constraints and smoothing.
- Pause routes while the transform is active.
- Keep the ghost as an optional preview/limit indicator.

**Exit:** the universe follows the hands continuously and routes resume correctly after release.

## 9. Test plan

### Yaw / translation preview

1. Preview shows target yaw and/or position on the boundary ghost.
2. Commit animates the real space to the previewed state.
3. Routes pause while the gesture is active and resume after commit.
4. Y-axis only; pitch/roll stay zero.
5. After moving around the room, the bubble is again placed in front of the player.
6. Same planet twice / same hand twice is rejected.

### Option B continuous

7. Translation/rotation/scale follow hands; rotation stays Y-axis only.
8. Routes pause while transforming and resume correctly after release.
9. Routes, lines, and travellers remain attached at the new scale/rotation.
10. Constraints keep the bubble reachable and inside the room.
11. Performance: no per-frame allocations, no LINQ, stable frame time.

## 10. Risks and decisions

| Risk / decision | Decision |
|---|---|
| Routes during transform | Routes **stop** while a transform gesture/commit is active and resume after. Content-local data makes them valid at any scale/rotation. |
| Hand separation cannot span mini→room | Geared scale (`scaleGain`) + threshold commit; explained in §4.3. |
| Ghost fidelity | Boundary ghost only; content ghost/render preview are not planned. |
| One hand vs two hands | Single planet grab only prompts; two planets + two hands transform. Ship grab stays one-hand. |
| Rotation | Y-axis only everywhere; no pitch/roll. |
| Bubble placement | Always in front of the current player; no seat memory. |
| UI | Keep visible; redesign later. |
| Tiny planets in Bubble | Minimum world-space handle collider; evaluate the feel first. |
| Disorientation | Keep transitions short and Y-only at room scale. |

The two-views plan’s additional risks also apply here, especially room-scale handle reachability, runner-level route pause, room-anchor readiness, ghost-to-commit continuity, and mini-scale path creation.

## 11. Defaults

| Parameter | Default |
|---|---|
| `scaleGain` | `2.5` |
| `enterRoomScale` | `0.60` |
| `exitRoomScale` | `0.45` |
| `minHandSeparation` | `0.08 m` |
| `minHandleWorldRadius` | `0.022 m` |
| `sMin` | `0.08` |
| `sMax` | `1.15` |
| `scaleSmoothTau` | `0.05 s` |
| `rotationSmoothTau` | `0.03 s` |

## 12. Out of scope until decided

- Continuous live scaling as the default interaction.
- Moving/rotating the room itself.
- Pitch/roll rotation; the universe stays upright and rotates on Y only.
- Non-uniform scale.
- Persisting the transform across app sessions.
- Infinite/procedural content outside the editable bubble.
