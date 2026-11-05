# Puzzle map — the run as a global field

**Status:** now · flow · updated 2026-11-05 — a global map of puzzle fields; the selected field is
centered as a diorama, nearby full fields inside a view radius are visible, and everything outside the
radius is culled. Selection is a one-hand pinch-drag that moves the map; release snaps the nearest
field's sun to the cube. Two-hand pinch spread zooms and enters. The play-field handoff is
[puzzle-map-play-field](../backlog/puzzle-map-play-field.md).

**Ready:** S1–S3 code and scene wiring landed (20/20 fast selection/persistence tests): `PuzzleMap` /
`PuzzleMapLayout` / `RunJobs`, the `FieldContent` assembly, `FieldContentView`, the `PuzzleDioramaView`
wrapper, the `PuzzleMapView` hierarchy, and the map state/gesture wiring. The carousel is retired.
Surrounding fields are full preview dioramas, culled by field center. The 2026-11-05 pass made the map a
direct one-hand pinch-drag (the map follows the hand, previews the nearest field's label and cube tint,
soft-limits the drag to the cluster, and snaps to the nearest field center on release), added a start
marker at the pinch origin of a one-hand drag, made the two-hand pinch a continuous zoom that runs a
slower enter zoom at the threshold, enlarged the map and cube, opened the cull radius, and added
selected-state chrome (cube tint, state and stars in the label). The map has an overview (pinch close),
idle field motion and a stronger refusal. The play-field handoff is
[puzzle-map-play-field](../backlog/puzzle-map-play-field.md). The map is the first screen; the
first-launch skip is removed.

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
  first and last field; it does not wrap. The map follows the hand during a drag and settles to the
  next/previous field on release. Surrounding fields are map neighbours, not run neighbours.
- **Map data.** `PuzzleMap` holds entries (index, puzzle, map position, state, stars), the selected
  index, `Select(index)`, `SelectNext(delta)`, `CanEnter`, and a culling query within `ViewRadius`. On
  release the view picks the nearest field center with `MapSnap` and centers that field's **sun** under
  the cube; the selected index can be any run order index. The layout is a deterministic spiral/cluster
  with one position per puzzle; no separate decoration items. `state` feeds `CanEnter` and the
  selection-cube tint; `stars` is shown on the selected label only. Every field's label is the index and
  sun name, and it culls/fades with the field.
- **Shared field content.** Extract the sun/planet view into an `_App` Feature assembly,
  `DigitalLove.Game.SpaceShaping.FieldContent` (`FieldContentView`, `FieldPlanetSet`, the planet/sun
  prototypes, and the small pool helper if the dependency stays clean). The map and the play field both
  use it; it depends on the domain and Nodes, never on selection or Flow.
- **Map view.** `PuzzleMapView` pans to the selected map position and centers the selected field under
  the fixed selection cube; the map offset accounts for `MapRoot` scale. The selected field is the `PuzzleDioramaView`
  galaxy (sun + planets) with its planet names hidden; it shows the index and sun name over the sun and
  breathes slowly in place. The per-sun label is culled/faded with the field's planets. A refused enter
  shakes the selected field. State comes from the cube tint and the selection label; the old marker
  element below the field is removed. Surrounding fields are full `PuzzleDioramaView` previews with the
  same index/sun label, while planet names and cubes are hidden. An authored route of eight thin dotted
  `LineRenderer` segments under `MapRoot` connects the suns in run order and closes 8→1. It uses
  `Dotted.mat` and a `ColorValue` (`Neutral`, `routeAlpha` 0.35); each segment culls/fades with the same
  near/far as the fields and follows the suns as they breathe and the map pans/scales.
  Their planets are grayed and culled individually by distance to the fixed cube, not by hiding whole
  fields. They are preview-only: no controller or interactions. The full play prototype is the
  [play-field follow-up](../backlog/puzzle-map-play-field.md).
- **Play field (follow-up).** The selected field is a preview diorama here. Turning it into the shared
  play field — flow ownership, one session, the play-only group, and the pose — is
  [puzzle-map-play-field](../backlog/puzzle-map-play-field.md).
- **Gesture.** A one-hand pinch-drag grabs the map; the map follows the hand 1:1. On release it snaps
  the nearest field's sun under the cube, whichever run-order index that is. A two-hand pinch spread is
  a continuous zoom; at the threshold it starts a slower enter zoom (field/cube to `enterScale`) over
  `enterSecs` before the state switch. Releasing cancels; a locked field clamps the spread to
  `lockedZoomLimit` (0.06) and refuses at the threshold; set it to 0 for no scale at all. A two-hand pinch close in the
  map zooms out to an overview of all fields, and release restores the map. In play, the same pinch
  close returns to the map. The two-hand gesture requires both pinches and ends as soon as one stops,
  so a single hand cannot scale the map. The shared `TwoHandGestureInput` and indicator stay.
- **Selection cube.** A separate authored selection cube stays fixed at the center, using the puzzle zone
  frame mesh and `CubeFrame` material so it matches the in-level cubes. `MapRoot` scale `1.5` and a
  `0.18` cube keep the selected preview larger than the cube. The selected field travels under it; the
  puzzle's level cube stays hidden. The cube is tinted with the shared `ColorValue` palette:
  `Destination` for current, `Success` for solved, `Danger` for locked. The cube tint and the label
  above the cube carry the state and star score; every field's per-sun label is the index and sun name,
  culled/faded with the field. The old marker element below the field is removed. Planet names and the
  level's own cubes stay hidden. The `PuzzleMapView` inspector exposes Previous/Next swipe buttons and a Show All toggle
  for Play-mode testing.
- **Culling.** All puzzle fields are present; the selected field stays full. Surrounding fields apply a
  distance fade to their planets: planets outside the view radius are culled, and planets between near
  and far are grayed. This is per planet, not whole-field hiding. The map follows the pinch and snaps
  on release; there is no free pan. Locked puzzles are visible but not enterable; only the centered
  unlocked puzzle can enter.
- **Flow ownership.** The map owns the selected field and puts its controller and puzzle into the shared
  memory/context. `PuzzleStartState` / `PuzzleEditionState` read them from that context; the separate
  serialized play controller goes away.
- **First launch.** The map is the first screen; `skipMapOnFirstLaunch` is removed. A fresh install
  starts on the map with job 1 centered and enters with the spread.
- **Retire carousel.** `RunCarousel`, `RunCarouselView`, the arc, and the carousel scene wiring are
  retired. `RunSlotView` survives as the map field host (labels hidden); `RunCarouselGesture` /
  `CarouselTwoHandGesture` / `ScaleOutGesture` survive as the map gesture.

## 3. Before starting

- **Layout.** Deterministic galaxy cluster (`Galaxy` radius `0.24`, height `0`); all fields share the
  same Y, so the pinch drag stays in X-Z. `MapRoot` scale `1.5` and a `0.18` selection cube;
  `viewRadius` `0.2` (local, ~`0.30` world) and `panSecs` `0.30`; all eight fields remain present. Tune
  density and radius on device.
- **Surrounding visibility.** Full fields inside the view radius; start with a small radius and cap the
  visible field count. Only the selected field is playable; surrounding fields are previews.
- **Locked behaviour.** Locked puzzles can be centered for preview but cannot enter.
- **Shared content.** New `_App` Feature `FieldContent` assembly; `PuzzleDioramaView` becomes a thin map
  wrapper (or is retired) over `FieldContentView`.
- **Play field (follow-up).** The map shell keeps the diorama. The shared play field, flow handoff, and
  session are [puzzle-map-play-field](../backlog/puzzle-map-play-field.md).
- **First launch.** The map is the first screen; the skip is removed.

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
  outside. Do not bring the full play prototype into S2; the play-field follow-up replaces the selected
  diorama.
- **Exit:** the scene shows the centered selected galaxy and nearby full fields; stepping the order pans
  the map and swaps the visible set; no carousel slots.

### S3 — pinch drag, nearest snap, spread, and overview

- **Landed:** `PuzzleSelectionState` uses `PuzzleMapView` / `PuzzleMap`; the carousel scene/content and
  `RunCarousel` / `RunCarouselView` are retired; `RunCarouselGesture` drives the map. 20/20 fast
  selection/persistence tests pass. The selection cube is fixed at center; every field shows its index
  and sun name over its sun, culled/faded with the field; the cube is tinted by the `ColorValue`
  palette, the label above the cube shows the state and stars, and the old marker element below the
  field is removed,
  and the inspector has Previous/Next swipe buttons.
- One-hand pinch-drag moves the map with the hand and shows a ring marker at the pinch origin, using the
  same marker as the pinch point. The label and cube tint preview the nearest field while dragging,
  unchanged fields are not re-rendered, and the drag is soft-limited to the cluster. Release snaps the
  nearest field's sun under the cube. Two-hand pinch spread is a continuous zoom; at the threshold it
  runs a slower enter zoom
  (`enterSecs` 0.5, `enterScale` 3.6) before the state switch, and release cancels. The pinch close in
  the map zooms out to the overview and release restores; the pinch close in play returns; a locked
  field refuses; the selected diorama breathes and a refused enter shakes it; indicator. The two-hand
  gesture ends when either pinch stops, so one hand cannot scale.
- The selection cube stays fixed at center; the selected field's sun moves under it; `MapRoot` scale
  `1.5` with a `0.18` cube keeps the selected preview larger than the cube. The level cubes stay hidden.
- The route is eight thin dotted `LineRenderer` segments (1→2→…→8→1) under the fields; each segment
  culls/fades with the field near/far, and `routeAlpha` / `widthMultiplier` tune it.
- Replace `RunCarouselView`/`RunCarousel` in `PuzzleSelectionState` with `PuzzleMapView`/`PuzzleMap`;
  `RunSlotView` stays as the field host; the carousel gesture drives the map. Keep the existing play
  route until the play-field follow-up.
- Reuse the pinch detector, `CarouselTwoHandGesture`, and `ScaleOutGesture`; the step and zoom live in
  `RunCarouselGesture`, the pose in `PuzzleMapView`.
- **Exit:** a pinch-drag moves the map and snaps to the nearest field, a spread zooms the centered field
  and opens it at the threshold, and the two-hand pinch close returns.

The play-field handoff — flow ownership, one session, the play-only group, and the pose — is
[puzzle-map-play-field](../backlog/puzzle-map-play-field.md). This plan ends at S3.

## 5. Reuse

- **Content:** `FieldPlanetSet`, `FieldPlanetBehaviour`, planet/sun prototypes, and the pool helper; the
  `_App` `FieldContentView` replaces the map’s duplicate preview path. `RunSlotView` stays as the map
  field host (labels hidden) and the selected diorama.
- **Gesture:** `TwoHandGestureInput` and indicator; the swipe detector, `RunCarouselGesture` /
  `CarouselTwoHandGesture`, and `ScaleOutGesture`.
- **Progress/context:** `RunProgressCookies` / `LevelSelector` for order, state, and stars. The shared
  memory/context handoff is part of the play-field follow-up.
- **Not reused:** `RunCarouselView` / `RunCarousel`; the free-pan `BubbleMapTransformSession` and
  `UniverseGhostSession` route editing; selection is discrete.

## 6. Risks

- **Assembly move.** `FieldPlanetSet` / the pool helper leave `SpaceShaping.Views` for the `_App`
  Feature; update references and keep GUIDs. The content assembly must not depend on selection or Flow.
- **Selection cube only.** The per-sun puzzle names, cube tint and state/stars label are the map's
  progression read. Confirm the names stay readable at the tighter spacing and that the run still reads
  as “filling” before the map becomes the return state.
- **Overview release.** The two-hand pinch close is the map overview; the play return belongs to the
  follow-up. The overview release must restore the base map scale.
- **First launch.** The map is now the first screen; confirm a new player can start from the map without
  a prompt (the onboarding hint is the separate info plan).
- **Sun snap vs run order.** The snap picks the nearest field and centers its sun, so the player can
  browse a solved or locked field between run steps. The run progression still comes from state and
  stars; confirm the per-sun label and tint keep the next job legible.
- **One-hand scale.** The two-hand gesture now ends when either pinch stops. Confirm a single hand can
  never scale the map, and raise `pinchStrengthThreshold` if the second hand still pinches by accident.
- **Drag vs snap.** The map follows the hand; a soft limit now resists dragging past the cluster, and
  the nearest snap plus `panSecs` decide the settle feel. Tune the limit and snap on device.
- **Culling and layout.** Tune the fade/cull radius and planet count; the grayed surrounding fields and
  the selected galaxy must still read as one universe of galaxies.
- **Carousel migration.** `RunSlotView` / `RunCarouselGesture` are reused; `RunCarouselView` /
  `RunCarousel` retire. Remove the carousel state wiring and tests in the same change.
- **Locked selection.** Tinted and labelled locked; the spread is clamped to `lockedZoomLimit` and
  refuses at the threshold. Confirm the tint reads as locked, not as an error.
- **Scope.** Three slices; the play-field handoff is its own plan. Do not restore the old sector field.
