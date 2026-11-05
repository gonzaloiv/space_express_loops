# Puzzle map — playable selected field

**Status:** later · flow · updated 2026-11-05 — the selected map field becomes the shared play field;
one session survives preview → play and the play-only group toggles in place.

**Related:** [puzzle map](../in-progress/puzzle-map.md) ·
[progression](../../design/systems/progression.md) · [core loop](../../design/core-loop.md) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

The map is the first screen and already shows the selected field as a full diorama, but entering a job
still hands off to a separate `SpaceShapingController` serialized in `PuzzleStartState` /
`PuzzleEditionState`. The selected field should become the play field in place: no second object, no
pose jump, no session rebuild.

## 2. The definition

- **Flow ownership.** The map owns the selected controller and puzzle and puts them into the shared
  memory/context. `PuzzleStartState` / `PuzzleEditionState` read them from that context; the separate
  serialized play controller goes away.
- **One session.** `SpaceShapingController` creates the session while the selected full play field is in
  preview and keeps it through play, so the sun/planets do not pop on enter.
- **Play-only group.** Controller, `SpaceShapingView`, spawner, lattice, ships, trajectories, zones, UI,
  and interactions are authored on the selected prototype, disabled in preview, enabled on enter, and
  disabled on exit. No runtime component creation.
- **Pose.** On enter, the field scales/moves to the play pose and the group is enabled; on exit, the
  group is disabled and the field returns to the map pose. Recover the old `BubbleScaleSession` /
  `UniverseContentRig` transition rigs (not in the current tree), rename them to fit, and use a slower
  InOutSine; if they are gone, build the helper here.
- **Return.** The existing pinch close in play returns to the map; `PuzzleEditionState` returns to
  preview instead of `HideAll`.

## 3. Slices

### S4a — flow ownership and shared session

- Put the selected controller and puzzle into the shared memory/context; `PuzzleStartState` reads them.
- Keep the session created in preview through play.
- **Exit:** entering a job uses the map's selected controller; no serialized play controller remains.

### S4b — pose and play-only group

- Author and toggle the play-only group on the selected prototype; enable it on enter, disable it on
  exit.
- Recover or build the scale/pose rig; use the slower InOutSine.
- **Exit:** the selected map field becomes the play field and returns without a second object, spawn, or
  pose jump.

## 4. Risks

- **Transition rigs.** `BubbleScaleSession` / `UniverseContentRig` are not in the current tree; recover
  and rename them, or build the helper in S4b.
- **Play-only group.** Controller, view, spawner, lattice, ships, zones, UI, and interactions must all be
  disabled in preview, including colliders; one missed component breaks the map.
- **Preview session.** Create the session once in preview and keep it through play; rebuilding it will
  pop the sun/planets.
- **One playable field.** One full play field; surrounding full fields are preview dioramas with no
  controllers or interactions.
- **Enter vs play handoff.** The map runs a 0.5 s enter zoom before the state switch; confirm the cut
  after the zoom reads as one move; tune `enterSecs`, `enterScale`, and the transition rig.
- **Scope.** Two slices; the map shell stays in [puzzle map](../in-progress/puzzle-map.md).
