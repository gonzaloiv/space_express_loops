# Space Shaping — Interaction and Presentation Polish

**Status:** in progress - next after the [POC](../done/space-shaping.md) (closed). Presentation
and interaction refinements, before / alongside game integration.

**Related:** [POC](../done/space-shaping.md) · [Architecture refactor](space-shaping-architecture.md) · [Integration into the game](../backlog/gameplay/space-shaping-integration.md) · [Reframing and variations](../backlog/gameplay/space-shaping-future.md) · [View frame](../done/universe-bubble-window.md)

**Wiring rule:** authored objects + serialized refs only; no runtime component lookup or asset/hierarchy creation. Deforming an authored mesh is the one open rule question (see §4).

## 1. Why

These were in the [POC](../done/space-shaping.md) S1 scope but the POC was validated and closed
without them. They are **next steps**, not POC blockers.

## 2. Scope

1. **Lattice presentation — mesh deform (DONE).** The lattice is now a deformed tube mesh
   (`LatticeMeshBuilder` builds `Meshes/LatticeMesh.asset`; `SpacetimeMeshView` deforms it
   from the field), URP **Unlit**, no shadows, 32 samples/line. Replaces the `LineRenderer`
   grid. Follow-up: retire the `LineRenderer` grid from `SpaceShapingView`, and settle the
   `meshFilter.mesh` runtime-instance question (or move to vertex-shader deformation).
2. **Move-range visualization.** Show each source's allowed range (a wire sphere or ring per
   planet) so the puzzle constraint is visible before you hit it.
3. **Ghost / pre-move preview.** Show the proposed source position (and a coarse path) during
   a drag; commit on release.
4. **Grab / scale feedback.** Haptics or highlight on grab and on reaching the scale limits.
5. **Tuning.** Thrust-to-gravity ratio, capture radius, line widths, lattice depth/displacement.

## 3. Order

1 -> 2 -> 3 -> 4, with 5 throughout.

## 4. Open questions

- **Mesh vs vertex-shader deformation** for the lattice, and whether deforming an authored
  mesh is allowed under the "no runtime asset creation" rule (it is not *creating* a mesh).
- Does the move-range visualization live in a view, or a dedicated gizmo/interaction layer?
- Ghost preview: run the full integrator, or a coarse path?
- Does the mesh lattice replace the `LineRenderer` grid entirely, or sit behind a flag?
