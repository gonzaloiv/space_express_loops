# The panel bank — the console on two arms from the grid

**Status:** now · presentation · updated 2026-11-10 — implemented; the console windows leave the head arc and hang,
smaller, on two arms from the grid's bottom-left-back corner: one at 45° out to the left, one straight
up. Split from [the field meter](field-meter.md). The pair draws above the field (canvases sort 30)
instead of the impossible "outside the near silhouette" test. U1's footprint and U2 are in; open: the
U3 review fixes (the layout breaks once the seat moves Origin), the headset check and the card's
text re-authoring.

**Related:** [the field meter](field-meter.md) · [lore](../../design/world/lore.md) §The console ·
[plans board](../README.md)

## Goal

The console windows sit on an arc around the head like monitors, oversized (card 38 × 30 cm, Jim
30 × 30 cm), while the field floats unattached in front of them. Reading one is a head turn up and out,
then back down to the field. Make the windows, the field and its meter read as one machine.

## Contract

- **One side, a glance.** Both windows sit on the left of the field, within about 35° of straight
  ahead; no turn to the other side.
- **Hung from the grid.** Two arms leave the grid's bottom-left-back corner: one runs up and out to
  the left at 45° in the field's vertical plane through the corner (not a 45° head turn), one rises
  straight up. Each window hangs at the end of its own arm. Arms use the grid's and the meter's line
  style and colour (`FieldMeter.mat`).
- **The pair keeps its shape.** Arm lengths are fixed; only the corner moves with the grid, so the
  pair looks the same in every job and on the map.
- **Above the field in draw order.** The pair stays on the field's left, and its canvases sort above
  the field, so no lattice line, trajectory, grown planet or solve stamp hides the text. The field may
  pass behind the console; that is the one-machine look.
- **Out of the hands' space.** Nothing below or in front of the field; the corner is behind the hands.
- **Readable by angle, not by size.** Reading distance is ~0.7–0.8 m (`SeatSettings` distance 0.45 m
  plus the field half plus the arm). Text cap height ≥ ~1.1° of view (≈1.4 cm at 0.75 m).

## Units

### U1 — smaller windows — footprint in; text open

- The card's text is ~1.3–1.5 cm cap (19–22 px at 0.001 scale, `JobCard.prefab`), near the floor, so
  a plain 60 % scale breaks the text rule. The footprint is in: card 260×230 units (26×23 cm), Jim
  333×450 (20×27 cm); the map card shares the card's prefab.
- Jim's panel text has `autoSizing: 0` (the TTS panel): check his longest line fits the 20 cm panel,
  or wrap and grow the panel's height.
- **Open:** re-author the card's text (less padding, shorter lines, tighter header) at the headset;
  the current fonts are unchanged and sit at the foot of the floor.
- **Exit:** both windows are clearly smaller in the headset and still read at their distance.

### U2 — the corner arms — in

- **Geometry.** The anchor is the grid's bottom-left-back corner: `FieldCentre + (−half, −half, +half)`
  (`ConsoleArmGeometry.Corner`). The 45° arm runs outward-left and up; the vertical arm rises from just
  outside the corner (`ConsoleArmGeometry.DiagonalEnd`/`VerticalEnd`). Arm lengths are serialized and
  fixed (0.24 m card, 0.28 m Jim). The card hangs on the 45° arm, Jim on the vertical arm. Both arm
  ends stay left of the field's centre; the pair's canvases sort above the field.
- **Frames and spaces.** `SpaceShapingView.FieldCentre` / `FieldHalf` are puzzle-local and the Puzzle
  instance is scaled 0.75 in `Game.unity`; convert with `shapingView.transform.TransformPoint` /
  `TransformVector` (as `GesturePromptPresenter.MeterFront` does), never the raw values on the Origin.
  Arm points vary per job, so runtime code sets them on authored line renderers (like
  `FieldEdgeMeterView.LayArm`); it creates nothing.
- **The map.** The corner comes from the field meter's U3 map square at rest, not from the live
  selected field (which moves with the cube, breathes, and grows 1→1.8 while pinched). That rest frame
  is new work: `MapStarMeter` lays the square from `RunSlotView.FieldCentreMap` / `FieldHalfMap`, which
  include the live zoom and breath (`MapStarMeter.cs:41`, `RunSlotView.cs:20-26,120-126`), so add a
  rest-frame accessor (or anchor to the square at its 1.8× extent) before taking the corner. Check the
  1.8× pinch preview stays right of the windows; if it can reach them, use the 1.8× extent.
- **One root, one CRT.** The arms and both windows sit under one pair root so a single
  `CrtTransition` boots and cuts the whole console. The card keeps its own `ShrinkOut` for swapping its
  content.
- **Map enter.** The pair cuts off (CRT) when the enter zoom starts and boots back with the job. The zoom
  starts in `PuzzleMapView`'s private `StartEnter`, not at `EnterRequested`: add a public
  `EnterStarted` event there and cut off on it.
- **Wiring.** `SeatPlacement` owns `ConsoleLayout` and calls `ArrangeAround` (`SeatPlacement.cs:18,103`).
  Replace that entry with a pair layout that receives the corner (and the field's right vector) as
  values in Origin space from the owners that already see both views: `PuzzleEditionState` on job
  start, `PuzzleSelectionState` on map show and whenever the map's rest frame changes. Keep
  `Features/UI/Console` free of Puzzle / PuzzleSelection references. Show and hide with the field
  (`SpaceShapingController.HideAll`, the map's `Show` / `Hide`).
- **Tests.** `ConsoleArmGeometryTests` in the fast `SpaceShaping.Tests.EditMode` suite: the corner for
  the largest and smallest job frames and the map rest frame, the 45° direction, the vertical arm, the
  window centre, and both arm ends left of the field centre.
- **Exit:** in every job and on the map, both windows hang on their arms on the left of the field,
  joined at the corner, never crossed by the field's content, and readable without turning the head to
  the other side.

### U3 — review fixes (review 2026-11-10)

Do 1 and 2 before the headset check; 1 breaks the layout on the device.

1. **Layout mixes spaces.** The owners pass the corner and rotation in Origin space
   (`SpaceShapingView.FieldCorner`/`FieldRotation`, `PuzzleMapView.FieldCorner`/`FieldRotation`), but
   `ConsoleBank.Layout` converts them as world values (`transform.InverseTransformPoint`,
   `Inverse(transform.rotation)`, `ConsoleBank.cs:38-39`). Console Bank sits at identity under
   `Origin`, and in the editor Origin is at the world origin, so the errors cancel. On the headset
   `SeatPlacement` moves and yaws Origin (`OVROriginBehaviour.Setup`), so windows and arms land offset
   by the seat pose. Treat the inputs as parent space (convert through the bank's local transform, not
   its world one). Test: `Layout` under a moved and rotated parent puts the corner where the field's is.
2. **The cards are outside the one root.** Only Jim's panel and the arms are under `Origin/Console
   Bank`; the job card is under `Origin/UI` and the map card under `PuzzleMapView`. `Boot` / `CutOff`
   scale only the bank, so on map enter the arms and Jim collapse while the card stays. `Place` also
   sets each card's `localPosition` in its own parent, which works only because those parents are at
   identity. Move the card slot under the bank (a holder for both cards); keep the card's own
   `ShrinkOut` for content swaps.
3. **The map corner is not at rest.** `PuzzleMapView.FieldCorner` says it reads at rest but uses
   `RunSlotView.FieldHalfMap` / `FieldCentreMap`, which include the zoom and the breath. Read only on
   show and selection change, so the error is small (~2 %), but the rest-frame accessor the plan asked
   for does not exist. Build it once for both this and [field-meter](field-meter.md) U4 item 5. Still
   open from U2: check the 1.8× pinch preview stays clear of the windows.
4. **Draw order does not beat depth.** The canvases sort at 30, which orders transparent queues only;
   anything that writes depth (opaque or grown planets) still hides text when in front. The contract
   change (outside the silhouette → above in draw order) stands; the headset check item 3 decides
   whether the windows need more clearance or the text a depth-ignoring material.
5. **Arm colour is hard-coded.** `armColor` is a raw `Color` applied at runtime
   (`ConsoleBank.cs:22,119-123`). Use the meter's `ColorValue` (same palette as `FieldEdgeMeter`).
6. *(Minor)* Tests cover the geometry but not `ConsoleBank.Layout`'s space conversion (item 1's test
   closes that); "both arm ends left of the field centre" is trivially true, so assert left of the
   grid's left edge instead. `ConsoleArmGeometryTests` lives in `Puzzles/Tests` while the type is in
   `UI/Console`. The map shows the pair without `Boot`; boot it there too or note it as intended.

- **Checked fine:** `SeatPlacement` no longer arranges the console (no `ConsoleLayout` callers),
  `EnterStarted` is public and wired to `CutOff`, the Console assembly references no Puzzle or
  PuzzleSelection code, the arms use `FieldMeter.mat` in local space, both windows' canvases sort 30.
- **Exit:** with Origin moved and yawed, the pair hangs from the grid's corner; one CRT boots and cuts
  arms, Jim and the card together; one rest-frame accessor serves the map corner and the map square.

## Manual check (headset)

1. Job 1: the card and Jim's call hang left of the field on their arms; reading them is a glance left,
   not a head turn.
2. Both arms leave the grid's bottom-left-back corner: one at 45°, one straight up.
3. No lattice line, planet, grown planet, trajectory or solve stamp hides either window's text, in
   any job; the field passing behind the console is expected.
4. The map: the same pair from the map square's corner, the map card on its arm; on map enter the pair
   cuts off and boots back with the job.
5. Hands never meet a window while pinching or moving the cube.
6. Jim's longest line fits his smaller panel.
7. Text reads at its distance; tuning: arm lengths, which window sits on which arm, the vertical
   panel's height.
