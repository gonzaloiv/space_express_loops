# Puzzle map — the run as a global field

**Status:** now · flow · updated 2026-11-05 — a global map of puzzle fields; the selected field is
centered as a diorama, nearby full fields inside a view radius are visible, and everything outside the
radius is culled. Selection is swipe-only along the route tangent and clamps at the ends; two-hand spread
zooms the selected field and enters at the threshold. S4 replaces the selected diorama with the shared
full play field.

**Ready:** S1–S3 code and scene wiring landed (24/24 fast selection/persistence tests): `PuzzleMap` /
`PuzzleMapLayout` / `RunJobs`, the `FieldContent` assembly, `FieldContentView`, the `PuzzleDioramaView`
wrapper, the `PuzzleMapView` hierarchy, and the map state/gesture wiring. The carousel is retired.
Surrounding fields are full preview dioramas, culled by field center. The 2026-11-05 pass removed the
free drag, made the swipe follow the route tangent, clamped the selection instead of wrapping, made the
two-hand gesture a continuous zoom, and added selected-state chrome (cube tint, state marker, state and
stars in the label). The S4 play-field handoff is next. First launch keeps the skip; the map is the return state.

**Related:** [progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[core loop](../../design/core-loop.md) · [Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

The carousel does not fit after testing. The run should feel like one global place: the selected puzzle
field is centered, nearby full fields are visible, and every field outside a view radius is culled. The
run order stays linear; the map organization does not. The selected field should become the play field,
not a preview that hands off to a second object.

## 2. The definition

- **Order vs map.** The run order is `LevelSelector` → `PuzzleData[]`. The map positions are a
  deterministic galaxy cluster, the same every run. The selected index follows the run and clamps at the
  first and last field; it does not wrap. The map pans to the selected puzzle's fixed position along the
  route tangent. Surrounding fields are map neighbours, not run neighbours.
- **Map data.** `PuzzleMap` holds entries (index, puzzle, map position, state, stars), the selected
  index, `SelectNext(delta)`, `AdvanceDirection(index)`, `CanEnter`, and a culling query within
  `ViewRadius`. The layout is a deterministic spiral/cluster with one position per puzzle; no separate
  decoration items. `state` feeds `CanEnter` and the selection-cube tint; `stars` is shown on the
  selected label only, not per surrounding field.
- **Shared field content.** Extract the sun/planet view into an `_App` Feature assembly,
  `DigitalLove.Game.SpaceShaping.FieldContent` (`FieldContentView`, `FieldPlanetSet`, the planet/sun
  prototypes, and the small pool helper if the dependency stays clean). The map and the play field both
  use it; it depends on the domain and Nodes, never on selection or Flow.
- **Map view.** `PuzzleMapView` pans to the selected map position and centers the selected field under
  the fixed selection cube; the map offset accounts for `MapRoot` scale. The selected field is the `PuzzleDioramaView`
  galaxy (sun + planets) with its planet names hidden; it shows its authored state marker. Surrounding
  fields are full `PuzzleDioramaView` previews; their planet names, state markers, and cubes are hidden.
  Their planets are grayed and culled individually by distance to the fixed cube, not by hiding whole
  fields. They are preview-only: no controller or interactions. The full play prototype is S4.
- **Selected field (S4).** The play-only group is authored on the selected prototype: controller,
  `SpaceShapingView`, spawner, lattice, ships, trajectories, zones, UI, and interactions. S4 replaces the
  map shell’s diorama with this full play prototype; on enter, the field scales/moves to the play pose and
  the group is enabled; on exit, the group is disabled and the field returns to the map pose. No second
  object and no runtime component creation.
- **Session (S4).** `SpaceShapingController` creates the session while the selected full play field is in
  preview and keeps it through play, so the sun/planets do not pop on enter.
- **Gesture.** One-hand pinch-swipe moves the selection one run-order step. The swipe must align with the
  route tangent, so the map always pans toward the field the hand moves toward. There is no free drag and
  no nearest-field pick. The selection clamps at the first and last field instead of wrapping. Two-hand
  spread is a continuous zoom: the selected field and its cube grow with the hand separation, and
  crossing the threshold enters the selected puzzle. Releasing before the threshold cancels; a locked
  field refuses at the threshold. Scale-out in play returns to the map. The shared
  `TwoHandGestureInput` and indicator stay.
- **Selection cube.** A separate authored selection cube stays fixed at the center, using the puzzle zone
  frame mesh and `CubeFrame` material so it matches the in-level cubes. The map scale matches the
  selected preview to the cube's inner size, so the two fill each other. The selected field travels under
  it; the puzzle's level cube stays hidden. The cube is tinted by state (current, solved, locked), the
  selected field shows its authored state marker, and the label above the cube names the puzzle and its
  sun plus the state and star score. Planet names and the level's own cubes stay hidden. The
  `PuzzleMapView` inspector exposes Previous/Next swipe buttons and a Show All toggle for Play-mode testing.
- **Culling.** All puzzle fields are present; the selected field stays full. Surrounding fields apply a
  distance fade to their planets: planets outside the view radius are culled, and planets between near
  and far are grayed. This is per planet, not whole-field hiding. The map does not free-pan; it pans to
  the selected puzzle. Locked puzzles are visible but not enterable; only the centered unlocked puzzle
  can enter.
- **Flow ownership.** The map owns the selected field and puts its controller and puzzle into the shared
  memory/context. `PuzzleStartState` / `PuzzleEditionState` read them from that context; the separate
  serialized play controller goes away.
- **First launch.** Keep `skipMapOnFirstLaunch`: a fresh install goes to the first job; the map is the
  return state after the first solve.
- **Retire carousel.** `RunCarousel`, `RunCarouselView`, the arc, and the carousel scene wiring are
  retired. `RunSlotView` survives as the map field host (labels hidden); `RunCarouselGesture` /
  `CarouselTwoHandGesture` / `ScaleOutGesture` survive as the map gesture.

## 3. Before starting

- **Layout.** Deterministic galaxy cluster (`Galaxy` radius `0.35`, height `0.1`); `MapRoot` scale
  `0.9375` so the selected preview matches the `0.15` cube, `viewRadius` `0.18` (local, ~`0.17` world) and
  `panSecs` `0.32`; all eight fields remain present. Tune density and radius on device.
- **Surrounding visibility.** Full fields inside the view radius; start with a small radius and cap the
  visible field count. Only the selected field is playable; surrounding fields are previews.
- **Locked behaviour.** Locked puzzles can be centered for preview but cannot enter.
- **Shared content.** New `_App` Feature `FieldContent` assembly; `PuzzleDioramaView` becomes a thin map
  wrapper (or is retired) over `FieldContentView`.
- **Selected field.** The map shell uses the diorama. S4 reuses the existing `Puzzle.prefab` scene
  instance as the one full play field; the play-only group is authored and toggled.
- **Flow handoff (S4).** Put the selected controller and puzzle into the shared memory/context; the play
  states read them. Keep the session created in preview.
- **First launch.** Keep the skip; the map is the return state.

## 4. Slices

### S1 — map layout, order, and culling

- **Landed:** `PuzzleMap`, `PuzzleMapLayout`, `RunJobs`; fast tests pass.
- Add `PuzzleMapLayout` (deterministic galaxy cluster, one fixed position per puzzle) and `PuzzleMap`
  (run order, selected index, `SelectNext`, `CanEnter`, culling query).
- Tests: fixed positions, run order independent of map order, selected state, culling at the radius.
- **Exit:** a headless map selects the expected puzzle for an index and lists only the fields inside the
  radius.

### S2 — shared field content and map view

- **Landed:** `FieldContent` assembly (`FieldPlanetSet` / `ViewPool` / `BoundsBuilder`),
  `FieldContentView`, `PuzzleDioramaView` wrapper, and `PuzzleMapView` script; fast tests pass.
  `PuzzleMapView` is wired in `Game.unity` with `MapRoot` and `SelectedField`, and clones the selected
  field for surrounding previews. The selection cube, swipe and zoom are next.
- Extract `FieldContentView` / `FieldPlanetSet` / the pool helper into the `_App` Feature
  `DigitalLove.Game.SpaceShaping.FieldContent`; make `PuzzleDioramaView` a thin map wrapper over it.
- Add `PuzzleMapView`: pan to the selected map position; the selected `PuzzleDioramaView` galaxy
  (labels hidden, selection cube); surrounding full fields pooled inside the radius; cull every field
  outside. Do not bring the full play prototype into S2; S4 replaces the selected diorama.
- **Exit:** the scene shows the centered selected galaxy and nearby full fields; stepping the order pans
  the map and swaps the visible set; no carousel slots.

### S3 — swipe order, zoom, and indicator

- **Landed:** `PuzzleSelectionState` uses `PuzzleMapView` / `PuzzleMap`; the carousel scene/content and
  `RunCarousel` / `RunCarouselView` are retired; `RunCarouselGesture` drives the map. 24/24 fast
  selection/persistence tests pass. The selection cube is fixed at center, planet names and the level
  cubes are hidden, the selected field shows its state marker, the cube is tinted by state, the label
  shows the puzzle, sun, state and stars, and the inspector has Previous/Next swipe buttons.
- One-hand swipe steps the run order along the route tangent; there is no free drag and no nearest-field
  pick. The selection clamps at the ends. Two-hand spread is a continuous zoom and enters at the
  threshold; scale-out returns; a locked field refuses at the threshold; indicator.
- The selection cube stays fixed at center; the selected field moves under it; the map is scaled so the
  selected preview fills the cube. The level cubes stay hidden.
- Replace `RunCarouselView`/`RunCarousel` in `PuzzleSelectionState` with `PuzzleMapView`/`PuzzleMap`;
  `RunSlotView` stays as the field host; the carousel gesture drives the map. Keep the existing play
  route until S4.
- Reuse the swipe detector, `CarouselTwoHandGesture`, and `ScaleOutGesture`; the zoom lives in
  `CarouselTwoHandGesture` and `RunCarouselGesture`, the pose in `PuzzleMapView`.
- **Exit:** a swipe steps the selection in the direction of the hand and pans the map there, a spread
  zooms the centered field and opens it at the threshold, and scale-out returns.

### S4 — playable selected field

- Move Flow ownership to the map’s selected controller through the shared memory/context:
  `PuzzleStartState` reads the selected controller/puzzle; `PuzzleEditionState` enables the play-only
  group, then disables it and returns to preview instead of `HideAll`.
- Replace S2’s selected diorama with the shared full play prototype. The selected field moves/scales to
  the play pose; the shared sun/planet content is the same object; the play-only group is enabled in
  place. Reuse the existing `Puzzle.prefab` instance.
- Recover the old `BubbleScaleSession` / `UniverseContentRig` transition rigs (they are not in the current
  tree), rename them to fit, and use a slower InOutSine; if they are gone, build the helper here.
- Tests: preview shows only sun/planets; play enables each play-only feature; no interaction in preview;
  scale-out restores the map center and preview.
- **Exit:** the selected map field becomes the play field and returns without a second object, spawn, or
  pose jump.

## 5. Reuse

- **Content:** `FieldPlanetSet`, `FieldPlanetBehaviour`, planet/sun prototypes, and the pool helper; the
  `_App` `FieldContentView` replaces the map’s duplicate preview path. `RunSlotView` stays as the map
  field host (labels hidden) and the selected diorama.
- **Play:** the existing `Puzzle.prefab` instance, `SpaceShapingController` / `SpaceShapingView` /
  `SpaceShapingSpawner` as the play-only group; keep the session across preview → play.
- **Scale/pose:** recover and rename `BubbleScaleSession` / `UniverseContentRig`; keep
  `UniverseBounds` / `UniverseFocus` where the view radius or clamp still fits.
- **Gesture:** `TwoHandGestureInput` and indicator; the swipe detector, `RunCarouselGesture` /
  `CarouselTwoHandGesture`, and `ScaleOutGesture`.
- **Progress/context:** `RunProgressCookies` / `LevelSelector` for order, state, and stars; the shared
  memory/context for the selected controller and puzzle.
- **Not reused:** `RunCarouselView` / `RunCarousel`; the free-pan `BubbleMapTransformSession` and
  `UniverseGhostSession` route editing; selection is discrete.

## 6. Risks

- **Assembly move.** `FieldPlanetSet` / the pool helper leave `SpaceShaping.Views` for the `_App`
  Feature; update references and keep GUIDs. The content assembly must not depend on selection or Flow.
- **Diorama trap.** S2 uses the diorama; S4 replaces the selected field with the full play prototype.
  Do not let S2 grow a second selected-field path that S4 has to delete.
- **Flow ownership.** The map owns the selected controller; the current serialized play controller must
  go. The memory/context handoff is the real change, not a view toggle.
- **Preview session.** Create the session once in preview and keep it through play; rebuilding it will
  pop the sun/planets.
- **Play-only group.** Controller, view, spawner, lattice, ships, zones, UI, and interactions must all be
  disabled in preview, including colliders; one missed component breaks the map.
- **One playable field.** S4 has one full play field; surrounding full fields are preview dioramas with
  no controllers or interactions.
- **Selection cube only.** The selected state marker, cube tint and state/stars label are the map's
  progression read; surrounding fields are still identical previews. Confirm the run reads as “filling”
  before the map becomes the return state.
- **Swipe vs route.** The swipe must align with the route tangent; a swipe that reads as “next” on
  screen but points across the route is ignored. The map and the hint must make the valid direction
  obvious.
- **Culling and layout.** Tune the fade/cull radius and planet count; the grayed surrounding fields and
  the selected galaxy must still read as one universe of galaxies.
- **Transition rigs.** `BubbleScaleSession` / `UniverseContentRig` are not in the current tree; recover
  and rename them, or build the helper in S4.
- **Carousel migration.** `RunSlotView` / `RunCarouselGesture` are reused; `RunCarouselView` /
  `RunCarousel` retire. Remove the carousel state wiring and tests in the same change.
- **Locked selection.** Tinted and marked locked; the zoom refuses at the threshold. Confirm the tint
  reads as locked, not as an error.
- **Scope.** Four slices; do not restore the old sector field.
