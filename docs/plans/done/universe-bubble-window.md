# Universe — View Frame (Bubble Cube)

**Status:** done - implemented. Slice 1 of the sector-field split; prerequisite for color
scope. Owns frame culling (the bounded field is pre-spawned).

**Related:** [Bubble map drag](universe-transform.md) · [Sector field hub](../backlog/universe/universe-infinite-sector.md) · [Color scope](../in-progress/universe-color-scope.md)

**Wiring rule:** frame values are serialized settings on the authored `Universe` root.
No runtime `AddComponent`, `GetComponent`, or hierarchy creation. No new visual assets;
the marker stays an authored `LineRenderer`.

## 1. Model: the frame moves, the universe does not

The moved entity is the **view frame** (the window) over a universe that is fixed in
content space. The grab changes the frame, not the universe.

```text
Content space (C) = the universe, fixed
Frame             = a cube in C: center F, half-size viewRadius, yaw
Magnification m   = miniBubbleRadius / viewRadius
Bubble pose D     = anchor - rotation * (m * F)      (scale m)
Room              = the frame opened to the room bounds, m = 1
```

- `viewRadius` is the cube half-size in universe space (default `2.5 m`).
- `miniBubbleRadius` is the cube half-size on screen in world space (`0.22-0.28 m`).
- F is the only thing the grab moves; the universe stays where it is in C.
- Room = the frame opened wide (room bounds); Bubble = the frame window.

This replaces the current model where the pose is keyed to a fixed `contentCenterLocal`
(a content property) and `MiniScale` is derived from the room bounds. The window makes
the **frame** the source of truth.

### 1.1 Shape: a cube, not a sphere

Default is a **cube** (equal half-size on all three axes, yaw-aligned):

- Rooms, culling (`Renderer.bounds`), and the sector grid are all AABBs — a cube frames
  exactly and aligns with all three.
- Containment/culling is exact and cheap (`|local| <= half` per axis) instead of
  distance-to-center.
- The scope/marker/containment math is a cube SDF (`sdBox` with equal extents); the edge
  can be softened with `sdRoundBox` if the hard corners read badly, but the shape stays a
  cube.
- "Bubble" stays the mode name; the frame is the concept, and the center marker becomes a
  square outline.
- **Shape stays data.** Keep the frame half-size a `Vector3` and route containment,
  culling, and the scope mask through the frame (`Contains`, half-size) instead of
  hardcoding cube math at call sites. A sphere/ellipsoid/slab later is then a value or
  mask swap — the model, drag, persistence, and culling are unaffected.

### 1.2 Presentation constraint

With the project's single-root, no-portal rendering (no second camera, render texture,
or depth texture), the universe can only stay fixed **in content space**. The frame
moves through C; the content's *world* transform `D` is derived each frame so the frame
center lands at the bubble anchor. Keeping content literally fixed in world space would
need a portal/render-texture view, which is excluded. The gain here is the model and
semantics, not the render trick.

## 2. Why first

- Without it, out-of-frame content becomes microscopic in the bubble.
- It defines the frame center F and the frame cube that the color scope and the center
  marker reuse.
- It supersedes the whole-content mini scale currently in the transform plan and
  assumed by the color-scope plan.

## 3. Scope

- `UniverseView` exposes the frame (`F`, `viewRadius`, `yaw`) and derives `D`.
- The grab moves F; it clamps to the frame bounds, not a content offset.
- Two-hand pinch changes the aperture (`viewRadius`); Bubble → Room opens the frame to
  the room bounds.
- Room becomes "frame at room bounds, `m = 1`"; there is no shared-content-point
  invariant to maintain.
- Save data is the frame (focus + aperture + yaw), replacing
  `bubbleOffsetLocal`/`bubbleYaw`.
- No streaming: the field is bounded and pre-spawned; out-of-frame content is hidden
  (see §6.1). No round changes, no new content types.

## 4. What it simplifies

| Before | After |
|---|---|
| Grab moves the universe content | Grab moves the frame |
| Content reparented between `roomAnchor`/`bubbleAnchor` | Content stays under the level root; `D` derived from the frame |
| Clamp = content offset (`BubbleRadiusWorld * 0.3`) | Clamp = frame bound (focus clamp) |
| Room maps the bubble center to a content point | Room opens the frame; no shared-point invariant |
| Save = `bubbleOffsetLocal` + `bubbleYaw` | Save = frame focus + aperture + yaw |
| `MiniScale` from room bounds | `m` from `viewRadius` (frame aperture) |
| Ring marker + sphere scope | One frame cube drives marker, scope, and culling |

Removing the reparenting paths also removes `LevelContainer.WithRoomPose`, which only
exists to temporarily un-parent content for spawn/restore.

## 5. What it does not solve

The universe is built up front (not streamed), so the window is what makes it read as
infinite: content inside the frame cube is visible, a **margin** outside stays visible,
and beyond that it disappears. That visibility band is this slice's job — a renderer
enable/disable test against the frame cube × margin — driven by the composition root so
content systems do not reference the view. The color scope only grays content; the window
hides it.

## 6. Scope / marker / culling alignment

- **Color scope:** the scope volume becomes the frame cube; the shader mask moves from a
  sphere SDF to `sdBox` with equal extents. Update [Color scope](../in-progress/universe-color-scope.md).
- **Center marker:** `UniverseCenterMarker` draws a square at the frame center with the
  cube extents instead of a ring.
- **Culling:** content outside the cube (AABB test) is hidden/disabled; see §6.1.
- **Streaming:** not needed; the field is bounded and pre-spawned.

### 6.1 Field culling (bounded field)

The frame movement is bounded, so the universe does not stream:

- Spawn the whole bounded field once at level start (from a pool).
- Each frame, hide/disable renderers whose AABB is outside the frame cube × margin
  (`BubbleViewState.ContainsPoint`, `|local| <= half` per axis; the margin is the soft
  band from §5).
- No load/unload hysteresis, no per-frame spawn budget, no floating-origin rebase, and no
  seed regeneration on pan: content stays put and is only shown/hidden.
- Hidden renderers cost no draw calls; the field size is bounded by load time and memory,
  not by frame time.
- The sector grid is axis-aligned, so the cube test maps cleanly to sectors if a coarse
  per-sector toggle is cheaper than per-renderer.

## 7. Coordination

- [Bubble map drag](universe-transform.md): its `bubbleMoveLimit` becomes a
  frame clamp, and its `bubbleOffsetLocal`/`bubbleYaw` become frame focus + aperture.
  That plan is marked done but its view/pose model is superseded here; its deferred
  headset validation and tuning (`translationGain`, `translationSmoothSecs`,
  `recenterDistance`, `recenterHoldSecs`, `heightOffset`, `transitionSecs`) run in this
  slice, against the frame.
- [Color scope](../in-progress/universe-color-scope.md): the scope center/radius become the frame cube,
  not the content bounds.
- Streaming: not needed; the field is bounded and pre-spawned, and this slice owns frame
  culling. `UniverseFocus` is this frame.

## 8. Touch points

- `UniverseBubbleSettings`: add `viewRadius` (aperture).
- `UniverseContentRig`: `m`/cube extents from `viewRadius`; `D` keyed to F; drop the
  room-bounds-derived `contentRadius` coupling and the reparenting methods.
- `UniverseView`: seed/expose F; frame-based pan, commit, and save.
- `BubbleMapTransformSession` / `BubbleScaleSession` / `UniverseGhostSession`: frame
  center, aperture, and cube containment for the drag gate.
- `UniverseCenterMarker`: square at the frame extents.
- `BubbleViewState` / `BubbleMetrics`: frame state (center, half-size, `m`, mode).
- `LevelContainer`: remove `WithRoomPose`.
- `GameSnapshot` / `BubblePlacement`: frame focus + aperture + yaw (compat read).
- Done transform plan: update its model to the frame.

## 9. Test plan

1. Bubble shows a readable local region; planets are not microscopic.
2. One-hand grab moves the frame; the grab only starts inside the frame cube.
3. Two-hand pinch changes the aperture; Room = frame open; the transition fires once.
4. The center marker square matches the frame extents.
5. Content outside the frame is hidden (bounded field, no streaming).
6. Saved frame (focus + aperture + yaw) restores the same view.
7. No content reparenting occurs at runtime; routes/ships/round spawn are unaffected.

## 10. Defaults

| Parameter | Default |
|---|---|
| `viewRadius` (cube half-size, universe space) | `2.5 m` |
| `miniBubbleRadius` (cube half-size, world) | `0.26 m` |
| Frame shape | cube, equal half-size, yaw-aligned |
| Bubble pose | `anchor - rotation * (m * F)` |
| Room frame half-size | `roomBounds.extents` |
| Room magnification | `1` |

## 11. Risks

| Risk | Mitigation |
|---|---|
| Single-root presentation still renders outside the frame | Cull out-of-frame content by AABB (§6.1); the frame model does not remove the overflow on its own. |
| Two plans disagree on bubble scale | This plan is the single source; update the transform and color-scope plans first. |
| Frame clamp semantics change the grab feel | Re-tune the clamp and translation gain against the frame, not the content offset. |
| Window too small to read a cluster | `viewRadius` must cover the active cluster; tune per level. |
| Cube loses the round "bubble" look | Accepted for the frame metaphor; `sdRoundBox` softens the edges if needed. |
| Room fit over/under-covers | Room frame half-size uses the room AABB extents, which a cube matches exactly. |

## 12. Next steps

Ordered; each is independently verifiable. The focus model + `viewRadius` window are
already in the code, so the remaining work is:

1. **Cube marker** (in progress). Author the marker LineRenderer as a cube wireframe
   (retraced single stroke) and drop the `Euler(90,0,0)` rotation in
   `UniverseCenterMarker` so it stands upright. Clean: a mesh or ~5 renderers.
2. **Zoom** (settled). Keep two views + the commit gesture; the aperture stays a fixed
   setting. No live aperture zoom.
3. **Cube containment** (done). The drag gate uses `BubbleViewState.ContainsPoint`
   (`|local| <= half` per axis) and the marker takes the frame yaw. The drag limit still
   uses the soft radial limit — keep until the cube feel is confirmed.
4. **Snapshot the aperture** — not needed while the aperture is fixed; revisit only if
   zoom ever becomes dynamic.
5. **Window visibility (the infinite illusion).** The universe is built up front; the
   frame hides content beyond the cube × margin so it reads as infinite — a soft band,
   not a hard cut at the edge. Implement the visibility band (renderer toggle) against the
   frame, driven by the composition root so content systems do not reference the view.
   The color scope stays a separate slice. The selection marker is a smaller cube than the
   frame extents (`ringRadiusFactor`), so the window and the selection read as different
   sizes. *Done:* the decor band sits at `cube × bubbleFrameMargin` (1.0, i.e. the cube
   limits) and the drag clamp was lifted from 0.3× to 1× so the band, not the pan limit,
   is the visible boundary. Raise or drop the clamp once the universe extent is known.
6. **Remove reparenting** (done). Content stays a scene root and poses are applied as world
   transforms (`ApplyDisplayPose`); `PlayTransition` and `EndTemporaryRoomPose` no longer
   reparent. `WithRoomPose` remains — it's still needed for room-space placement, and
   removing it means spawning in content space, which is a separate placement step.
   `bubbleAnchor` is now a pose holder (written, not a parent).
7. **Color scope alignment** (separate slice). The scope mask becomes the frame cube
   (`sdBox`); update [Color scope](../in-progress/universe-color-scope.md).
