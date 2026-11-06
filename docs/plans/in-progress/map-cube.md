# Map cube — select a job with the game's own cube

**Status:** now · presentation · updated 2026-11-07 — the map's selection cube becomes the edition
cube: pinch inside to scale the node's diorama and enter, grab the frame to move it over the map
(X-Z, snapping to nodes); the cull stays centred on the cube, so moving it discovers the space.

**Related:** [the cube](../../design/systems/cube.md) · [progression](../../design/systems/progression.md) ·
job card (landed) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## Goal

The map speaks the game's one verb. The selection cube is the same `Zone.prefab` the player scales in
a job: pinching inside it scales the selected node's diorama, and past a threshold the job is entered.
Grabbing its frame with one hand moves it over the map in X-Z and snaps it to the nearest node on
release. The map no longer pans under a fixed cube; the cube travels, and the cull around it reveals
the neighbourhood as it goes.

## Contract

- **Same cube, same pinch.** A `Zone.prefab` instance under `mapRoot` replaces the selection cube. A
  two-hand pinch inside scales the selected diorama (`MapZoom`) live; reaching `enterAt` starts the
  existing `MapEnter`. Releasing below it springs the scale back to 1.
- **Shrinking does nothing.** The scale envelope's floor is 1 on the map.
- **Grab to move, snap to select.** A one-hand frame grab moves the cube on the map's X-Z plane (Y
  locked). While moving, the nearest node previews (label + card, as the drag preview does now); on
  release the cube snaps to the nearest node and selects it. No endpoint keep-out on the map.
- **The cull follows the cube.** Fields fade and cull around the cube's position (today's
  `viewRadius` fade), updated while it moves, so moving is discovering.
- **Locked nodes.** The cube may snap onto a locked node. The pinch is capped at `lockedZoomLimit`
  and a release there plays the refusal; it never enters.
- **Move is not taught on the map.** The next job is preselected, so the game completes without
  moving. No prompt for the grab on the map; the Enter prompt stays (it is the Scale pinch). The Pan
  prompt goes away with the two-hand drag. Job 4 still teaches Move inside a puzzle; a player who
  already moved on the map just knows it.
- **Reach.** The map stays still. If the Galaxy layout does not fit a seated reach, shrink the layout;
  do not bring panning back.

## Units

### U1 — the zone cube on the map

- Place a `Zone.prefab` instance under `mapRoot` in `Game.unity`, sized like today's selection cube;
  wire it to `PuzzleMapView` in place of `selectionCube` / `selectionCubeRenderer`.
- `ZoneConditions` gains a planar move (Y locked) and a way to skip the endpoint keep-out; the map
  cube uses translatable + planar, not resizable.
- A small `MapCube` collaborator reads the zone: scale while pinching → zoom and enter; position while
  moving → preview + fade; `Moved` → snap + select; release → scale back to 1. Locked nodes get a
  lower scale ceiling.
- **Exit:** pinching inside the map cube scales the diorama and enters; grabbing the frame moves it in
  X-Z and it snaps to a node; the fade follows the cube.

### U2 — retire the two-hand map input

- `PuzzleSelectionState` stops forwarding `TwoHandGestureInput` to the map. Remove what nothing uses
  any more: `RunCarouselGesture` and its settings/outcome, `MapPose`'s pan and drag, `MapOverview`,
  `SelectionCubeLens` (the zone cube has its own palette), `MapPanWatch`, the map's `Panned`, and the
  Pan prompt (`GestureKind.Pan`, its content asset and scene reference). `PuzzleMapView` ends under
  250 lines.
- The Enter prompt anchors on the zone cube.
- **Exit:** the map runs only on the cube; no dead map input code; tests build.

### U3 — docs

- Update [the cube](../../design/systems/cube.md), [progression](../../design/systems/progression.md)
  and the prompt code: drop the Pan prompt (`GestureKind.Pan`, its content asset and caption), anchor Enter on the zone cube.
- **Exit:** docs describe the map as the cube; `docs/check.ps1` passes for these files.

## Manual check (headset)

1. Open the map: the selection cube looks like the puzzle cube and sits on the next job.
2. Pinch inside it and spread: the node's diorama grows inside the cube; release early and it springs
   back.
3. Pinch past the threshold: the job enters as before.
4. Pinch to shrink: nothing happens.
5. Grab the frame with one hand and move: the cube slides in X-Z only, the nearest node previews, and
   fields appear and fade around the cube as it travels.
6. Release between nodes: the cube snaps to the nearest one and selects it.
7. Move onto a locked node and pinch: the scale stops early and refuses; it never enters.
8. Every node is reachable seated without leaning; if not, note the layout scale to shrink.
9. The Enter prompt shows on the cube on the first map; no Pan or Move prompt shows on the map.
10. Solve a job and return: the cube sits on the next job again.
