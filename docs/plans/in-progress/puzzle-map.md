# Puzzle map — the run as a global field

**Status:** now · flow · updated 2026-11-04 — a global map of puzzle fields; the selected field is
centered as a diorama, only nearby planets inside a view radius are visible, and the selection is read by
tint. S4 replaces the selected diorama with the shared full play field.

**Ready:** S1 landed in `PuzzleMap` / `PuzzleMapLayout` / `RunJobs` (27/27 fast tests). Decisions:
keep the four-slice order, selected diorama first, radius/planets-only surroundings, selection tint only,
`_App` FieldContent, recovered/renamed transition rigs, and reused `RunSlotView`/gesture. S2 uses the
diorama; S4 is the play-field handoff. First launch keeps the skip; the map is the return state.

**Related:** [progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[core loop](../../design/core-loop.md) · [Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

The carousel does not fit after testing. The run should feel like one global place: the selected puzzle
field is centered, only nearby planets are visible, and everything outside a view radius is culled. The
run order stays linear; the map organization does not. The selected field should become the play field,
not a preview that hands off to a second object.

## 2. The definition

- **Order vs map.** The run order is `LevelSelector` → `PuzzleData[]`. The map positions are a
  deterministic galaxy cluster, the same every run. The selected index follows the run; the map pans to
  the selected puzzle's fixed position. Surrounding planets are map neighbours, not run neighbours.
- **Map data.** `PuzzleMap` holds entries (index, puzzle, map position, state, stars), the selected
  index, `SelectNext(delta)`, `CanEnter`, and a culling query within `ViewRadius`. The layout is a
  deterministic spiral/cluster with one position per puzzle; no separate decoration items. `state` feeds
  `CanEnter`; `stars` is not shown per field: the map reads by selection tint only.
- **Shared field content.** Extract the sun/planet view into an `_App` Feature assembly,
  `DigitalLove.Game.SpaceShaping.FieldContent` (`FieldContentView`, `FieldPlanetSet`, the planet/sun
  prototypes, and the small pool helper if the dependency stays clean). The map and the play field both
  use it; it depends on the domain and Nodes, never on selection or Flow.
- **Map view.** `PuzzleMapView` pans to the selected map position. The selected field is the
  `PuzzleDioramaView` galaxy (sun + planets) with its labels hidden and the selection shown by tint.
  Surrounding fields contribute only their planets that fall inside the view radius, pooled and culled;
  they are not full field views. The full play prototype is S4.
- **Selected field (S4).** The play-only group is authored on the selected prototype: controller,
  `SpaceShapingView`, spawner, lattice, ships, trajectories, zones, UI, and interactions. S4 replaces the
  map shell’s diorama with this full play prototype; on enter, the field scales/moves to the play pose and
  the group is enabled; on exit, the group is disabled and the field returns to the map pose. No second
  object and no runtime component creation.
- **Session (S4).** `SpaceShapingController` creates the session while the selected full play field is in
  preview and keeps it through play, so the sun/planets do not pop on enter.
- **Gesture.** One-hand pinch-drag steps the run order (next/previous, discrete, one step per pinch, the
  same swipe language as the carousel). The map pans to the selected puzzle's fixed position. Two-hand
  scale-in enters the selected puzzle; scale-out returns. The shared `TwoHandGestureInput` and indicator
  stay.
- **Selection color.** The selected field's cube or planets change color from the palette
  (`ColorValue`) so the center reads without UI. There are no per-field number/stars/state markers; if
  `RunSlotView` is reused, hide its labels and state objects.
- **Culling.** All puzzles exist, but only the selected field and the surrounding planets inside the view
  radius are visible; the rest are pooled/culled. There is no manual free pan; the map pans to the
  selected puzzle. Locked puzzles are visible but not enterable; only the centered unlocked puzzle can
  enter.
- **Flow ownership.** The map owns the selected field and puts its controller and puzzle into the shared
  memory/context. `PuzzleStartState` / `PuzzleEditionState` read them from that context; the separate
  serialized play controller goes away.
- **First launch.** Keep `skipMapOnFirstLaunch`: a fresh install goes to the first job; the map is the
  return state after the first solve.
- **Retire carousel.** `RunCarousel`, `RunCarouselView`, the arc, and the carousel scene wiring are
  retired. `RunSlotView` survives as the map field host (labels hidden); `RunCarouselGesture` /
  `CarouselTwoHandGesture` / `ScaleOutGesture` survive as the map gesture.

## 3. Before starting

- **Layout.** Deterministic galaxy cluster; tune density and radius on device. Start compact.
- **Surrounding visibility.** Only nearby planets inside the view radius; start with a small radius and
  cap the visible planet count. The selected field is the only full field view.
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

- **Landed:** `PuzzleMap`, `PuzzleMapLayout`, `RunJobs`; 27/27 fast tests pass.
- Add `PuzzleMapLayout` (deterministic galaxy cluster, one fixed position per puzzle) and `PuzzleMap`
  (run order, selected index, `SelectNext`, `CanEnter`, culling query).
- Tests: fixed positions, run order independent of map order, selected state, culling at the radius.
- **Exit:** a headless map selects the expected puzzle for an index and lists only the planets inside the
  radius.

### S2 — shared field content and map view

- Extract `FieldContentView` / `FieldPlanetSet` / the pool helper into the `_App` Feature
  `DigitalLove.Game.SpaceShaping.FieldContent`; make `PuzzleDioramaView` a thin map wrapper over it.
- Add `PuzzleMapView`: pan to the selected map position; the selected `PuzzleDioramaView` galaxy
  (labels hidden, selection tint); surrounding planets pooled inside the radius; cull the rest. Do not
  bring the full play prototype into S2; S4 replaces the selected diorama.
- **Exit:** the scene shows the centered selected galaxy and only nearby planets; stepping the order pans
  the map and swaps the visible set; no carousel slots.

### S3 — swipe order, scale-in, and indicator

- One-hand swipe steps the run order and pans the map to the selected position; two-hand scale-in
  enters; scale-out returns; locked refusal; indicator.
- Replace `RunCarouselView`/`RunCarousel` in `PuzzleSelectionState` with `PuzzleMapView`/`PuzzleMap`;
  `RunSlotView` stays as the field host; the carousel gesture drives the map. Keep the existing play
  route until S4.
- Reuse the swipe detector, `CarouselTwoHandGesture`, and `ScaleOutGesture`.
- **Exit:** swipe steps the run order, scale-in opens the centered puzzle, scale-out returns.

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
- **One full field.** S4 has one full play field in preview; surrounding fields must contribute only
  planets, not full controllers.
- **Selection tint only.** State/stars are data, not map markers; the run may read as less “filled” than
  the carousel. Confirm the tint is enough before the map becomes the return state.
- **Culling and layout.** Cap the radius and planet count; the surrounding planets and the selected
  galaxy must still read as one universe of galaxies at the radius.
- **Transition rigs.** `BubbleScaleSession` / `UniverseContentRig` are not in the current tree; recover
  and rename them, or build the helper in S4.
- **Carousel migration.** `RunSlotView` / `RunCarouselGesture` are reused; `RunCarouselView` /
  `RunCarousel` retire. Remove the carousel state wiring and tests in the same change.
- **Locked selection.** Visible but refused; confirm the centered locked preview feels intentional.
- **Scope.** Four slices; do not restore the old sector field.
