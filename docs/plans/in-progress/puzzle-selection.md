# Puzzle selection — the run carousel

**Status:** now · flow · updated 2026-11-03 — an arc of job fields, one focused, touched and
amplified into play; starts after the PoC is integrated into the main scene.

**Ready:** S1/S2 code is in `Assets/_Game/Features/PuzzleSelection`; the 7-job S1 exit waits on the run
assets, and S3 waits on the completion/result path in flow (owned separately). The scale-out return from
edition to selection is wired.

**Related:** [progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[core loop](../../design/core-loop.md) · [Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

The run needs a level selector, but a flat grid of full fields is noisy and expensive; a carousel keeps
one field legible while the neighbours give the run's shape. The selection must also honour the first
launch (straight to the tutorial) and return the player to the map later. The carousel is chosen over the
grid: one focus, three previews, and the same cube/amplify language as play.

## 2. The definition

**One source of truth.** The run is still `LevelSelector` → `ChapterData` → `PuzzleData[]`. Selection
does not invent a second level list.

**The snapshot.** `PuzzleSelectionState` builds one `JobEntry` per level:

- `index`, `puzzle`, `state` (`Locked` / `Current` / `Solved`), `stars`.
- The current index and solved set come from the existing `PlayerData` level cookies: a level with a
  `LevelCompleteCookie` is solved, and its `score` is the stars shown. Selection reads the snapshot and
  never owns or writes progression data.
- Stars come from the same cookie metadata, not from the puzzle asset.

**The session.** `RunCarousel` is a plain C# class, not a MonoBehaviour: it holds the snapshots, the
focused index, `Move(delta)`, `Focus(index)`, `CanEnter`, and returns the selected `PuzzleData`. It has
no view, no flow, and no data client.

**The view.** `RunCarouselView` is a MonoBehaviour with an authored arc (centre and one at each side,
about ±35°), three pooled diorama slots, and a number per slot. It receives snapshots and raises
`FocusRequested(int)` and `EnterRequested`; it never reads `LevelSelector`, never changes flow, and never
enters a job itself. Locked feedback (the visible refusal) stays in the view.

**The orchestrator.** `PuzzleSelectionState` is the only thing that joins data, flow, and view:

1. `Enter` reads the run-progress snapshot and `LevelSelector`, builds the `RunCarousel`, and either
   skips the map on first launch or shows the view.
2. `FocusRequested` moves the session; `EnterRequested` checks `CanEnter`.
3. On enter it puts the selected `PuzzleData` into `MemoryDataClient` and routes to `PuzzleStart`.
4. `Exit` unsubscribes and hides the view.

Swiping and touching never start a job; only the amplify enters.

**The shared gesture.** The scale in/out gesture controller is a serialized scene reference on both
`PuzzleSelectionState` and `PuzzleEditionState`, so each state can subscribe to the same gesture while
it is active. Selection consumes it for the amplify/focus feedback and the arc rotation; edition
consumes it for the play scale. Subscribe in `Enter`, unsubscribe in `Exit`; do not wire it in `Awake`
and do not find a second instance at runtime. The view reads the gesture state the state passes in, not
the controller itself.

**Rotation.** The same gesture can drive the arc: derive `yaw` from the line between the two pinch
points (`atan2(right.x - left.x, right.z - left.z)`), unwrap the delta, and map it to a step index
(`round(delta / stepAngle)`). The view snaps to `-index * stepAngle`; focus changes only when the
snapped index changes. Ignore rotation when the horizontal hand separation is too small or the vector is
mostly vertical, and use a dead zone plus hysteresis so the arc does not jitter around a step. Rotation
is a focus verb only; it never enters a job.

**Swipe.** Swipe is a **one-hand pinch-drag**, using the shared gesture's `IsSingleHandPinching` state:
take `DragPoint - DragStartPoint`, project it onto the arc's local right axis, and require horizontal
dominance (about 1.5× the vertical component) plus a minimum drag (about 3 cm). Commit **one step per
pinch** on release, reset on pinch end, and cancel the swipe when the second hand pinches (that is
rotation/amplify, not swipe). A pinch that starts on a side cube is a cube touch, not a swipe; the
gesture input's busy-interactor gate suppresses the swipe. The arc may follow the drag a little, then
snap.

**Open palm (deferred).** A palm wave is possible, but it is a different tracker: read the palm joint
position, gate on an open hand (no index pinch), measure lateral displacement and speed over a short
window, and fire one step with a cooldown. It is more prone to false positives when reaching for a cube,
so keep pinch-drag as the slice default and add palm wave only if the pinch feels heavy on device.

**The transition.** The gesture model records the start and end point and time of each gesture, with a
short velocity window (about 0.1 s) at release. A step commits on distance **or** flick speed; the
direction comes from the signed horizontal delta (or the sign of the yaw delta for rotation). The view
snaps with a duration derived from that release speed, clamped (roughly 0.08–0.35 s): a slow drag settles
gently, a fast flick arrives quickly. Exactly one step per gesture; speed changes the transition, never
the count.

**The diorama.** `PuzzleDioramaView` renders only the baked field's planets, sun, and cube from an
authored lightweight prototype. It has no lattice, ship, trajectory, entropy bar, or
`SpaceShapingController`. Three slots, pooled, unused slots hidden. The centre shows the full field; the
sides may show a reduced source set. This is the recommended preview fidelity.

It reuses the play field's data and pooling rather than a second renderer: `SpaceShapingSetup` +
`SpaceShapingSession` turn the `PuzzleData` into the same source layout, `FieldPlanetSet` spawns the
sun/endpoints/sources, and `ViewPool` pools the planets and cubes. Each slot wires the run's
`SpaceShapingSettings` asset (the one the play field uses); previews pass no route colours. It does not
reuse `SpaceZoneView` or `ZoneCubePool`, because those carry grab, prediction, and scaling behaviour the
preview must not have. Bounds stay local to the preview because the cube frames are part of its framing
and `FieldFrame` measures only the run.

**First launch.** The flow decides the starting job. When it says the run is fresh, put job 0 and route
straight to `PuzzleStart` without showing an empty map; after the first solve the map is the return
state, with job 1 solved and job 2 current.

## 3. Current Game.unity state (audited 2026-11-03)

- `PuzzleSelectionState` already exists and is attached to the `GameStart` GameObject
  (`Game.unity:3058`, component `883682824`). Its `nextState` is `PuzzleStart` (`:3091`) and its
  `levelSelector` is the run asset (`:3092`).
- It is a stub: `Enter` always takes global index 0, puts it in `MemoryDataClient`, and immediately
  routes to `PuzzleStart`. There is no carousel, no first-launch branch, and no completion state.
- Flow order is `Setup → PuzzleSelection → PuzzleStart → PuzzleEdition → PuzzleComplete`
  (`Main.states`, `Game.unity:1406`). `PuzzleStartState` calls `SpaceShapingController.Enter(data)`.
  `PuzzleEditionState` now routes back to selection when the two-hand scale-out crosses its threshold;
  `PuzzleCompleteState` is still empty. The result path does not exist yet, so S3 is not testable.
- The run has one level: `Assets/_Game/Resources/Chapters/01.asset` → `01/01_01.asset`. Its
  `PuzzleData.id` is blank and its `prefab` is unset. There is no run-progress source in the scene yet,
  so solved/current/locked are placeholders until flow supplies them.
- The shared gesture is wired on both `PuzzleSelectionState` and `PuzzleEditionState`, pointing at the
  same `Main/GestureInput` scene instance. Each state subscribes in `Enter` and unsubscribes in `Exit`.
- The scene labels are stale: `GameStart` hosts `PuzzleSelectionState`; `PuzzleStart` still says
  `NewRoundState`, `PuzzleComplete` says `GameEndState`, and `Puzzle` says `SpaceShapingState`. The
  script GUIDs are correct; the names are leftovers from the rename. Rename the selection GameObject to
  `PuzzleSelection` in the same scene save.
- `Assets/_Game/Features/Puzzles/Puzzle.prefab` is the full play field (controller, view, spawner,
  lattice, entropy). Do not use it as a preview: a preview slot would pay for the whole puzzle.

## 4. Slices

### S1 — the carousel

- An authored arc: the focused field at the centre, one at each side (±~35°), the rest implied by the
  numbered cubes. Each job shows a **diorama** of its baked field (planets, sun, the cube) — pooled from
  an authored prototype, no runtime hierarchy — and a number.
- Cube states: current (highlighted), solved (lit + stars), locked (dark). Locked refuses visibly on the
  amplify; focus is allowed so the player sees what is next.
- Cap the visible previews at three; the full field is built only on entry.
- **Exit:** a 7-job run reads at a glance, the current/solved/locked states are distinct, and the preview
  cost is measured.

### S2 — focus, rotation, and the first launch

- Swipe changes the centre one job at a time; touching a side cube brings it to the centre. The amplify
  is the only way to enter, so a swipe or a touch never starts a job. Swipe is a one-hand pinch-drag for
  the slice; an open-palm wave is deferred.
- The snap transition follows the gesture: record start/end, use the release speed to select a fast or
  gentle transition (clamped), and still move exactly one job.
- **Rotation:** twisting the two pinch points rotates the arc by one step per `stepAngle` and snaps to
  the nearest job. Rotation changes focus only; it never enters. Ignore near-vertical or too-close hands,
  and use hysteresis so a held twist does not oscillate between two jobs.
- First launch skips the map and starts job 1; the map is the return state after the first solve. No
  empty map is shown.
- **Exit:** a player can reach any unlocked job by swipe, touch, or twist, and a fresh install goes
  straight to the tutorial.

### S3 — enter, return, and state

- The two-hand amplify grows the focused field/cube into the play space (an authored transition, no
  scene load). The result offers **Map/Retry**; the map is also reachable from edition.
- Scale out (two hands closer, the inverse of the amplify) returns to selection from edition. Its
  `ScaleOutMinDistance` / `ScaleOutMaxRatio` live on the shared `TwoHandGestureInput`; the amplify
  direction never exits, so play scaling does not trigger it. The result path can add a second exit later.
- Edition reuses the selection's two-hand yaw detector (`TwoHandYawGesture` in `DigitalLove.Input`) to
  rotate the puzzle around its measured field centre. Selection keeps using it for focus; edition sends
  the yaw delta to `SpaceShapingController.Rotate`.
- On a solve: light the cube, award stars, open the next job, highlight the new current. The map fills.
- **Exit:** amplify enters, the inverse returns from the result, the play pinch never exits, and the
  solved/current/locked state survives the session.

## 5. Where the code lives

- **Orchestrator:** stays in `Assets/_Game/Flow/PuzzleSelectionState/`. The flow state assembly should
  stay thin: state, data reads, routing, and view calls only.
- **Selection feature:** `Assets/_Game/Features/PuzzleSelection/Scripts/` with its own
  `DigitalLove.Game.PuzzleSelection.asmdef`. It holds `RunCarousel`, `JobEntry`, `RunCarouselView`, and
  the diorama views/products. It references `DigitalLove.Game.SpaceShaping.Domain`,
  `DigitalLove.Game.SpaceShaping.Views` (the reusable field pool), `DigitalLove.Casual.Levels`,
  `DigitalLove.Input` (the shared gesture), `DigitalLove.Game.Nodes`, `Oculus.Interaction`, and
  `Unity.TextMeshPro`.
- **`PuzzleData`:** keep it where it is, in
  `Assets/_Game/Features/Puzzles/Domain/Scripts/Data/PuzzleData.cs` and
  `DigitalLove.Game.SpaceShaping.Domain`. It already sits with `PuzzleFrame`, `PuzzleShip`,
  `PuzzleSource`, and the settings; moving it alone would split that cluster, and the selection
  assembly can reference the domain directly. A later mechanical
  `SpaceShaping.Domain → Puzzles.Domain` rename is a separate cleanup, not this plan.

**Gesture wiring.** The shared scale gesture controller stays in the scene, owned by the flow/prefab
composition. `PuzzleSelectionState` and `PuzzleEditionState` each hold a serialized reference to it and
listen only while entered. Do not inject it, resolve it by type, or create a second instance for
selection.

**Reuse audit (2026-11-03).** Before adding a selection-side implementation, use the existing owner:

- **Progress:** `PlayerData.GetLevelCompleteCookies()`, `LevelCompleteCookie.GetByLevelId` /
  `HasLevelIdLevelCompleteCookie`, and `LevelSelector.GetFollowingLevel` (the same rule the levels panel
  uses). Do not add a second current-index rule.
- **Field data:** `SpaceShapingSetup`, `SpaceShapingSession`, `PuzzleSimSettings`, and
  `FieldPlanetSet`; the preview is the baked field, not a re-authored copy.
- **Pooling:** `ViewPool<T>` is the one place pooled views are instantiated. Do not hand-roll slot
  lists or `Instantiate` loops.
- **Gesture:** `TwoHandGestureInput` already publishes `TwoHandGestureState`, pinch points, start points,
  and distance. `TwoHandYawGesture` owns the shared yaw math/validity; `RunCarouselGesture` uses it for
  focus and `PuzzleEditionState` uses it to rotate the puzzle. Neither reads Oculus hands itself.
- **Touch:** `RunSlotTouch` follows the existing `Grabbable.WhenPointerEventRaised` /
  `PointerEventType.Select` pattern used by `PuzzleLabButton` and `GrabbableWrapper`; there is no shared
  relay, so do not add a second interaction system.
- **Not reused on purpose:** `SpaceShapingController` / `Puzzle.prefab` (full play field), `SpaceZoneView`
  / `ZoneCubePool` (grab + prediction), and `LevelItemData` / `LevelItemStatus` (2D levels-panel UI
  status, not the run state machine).

## 6. Decisions

**Settled:** the return is the **inverse** of the amplify — two hands closer, not the amplify direction —
so the same two-hand vocabulary exits without firing during play scaling.

**Settled:** preview fidelity is the pooled lightweight diorama (not a flat cube thumbnail), and locked
pods focus and refuse rather than being skipped by the swipe.

**Settled:** progression and persistence belong to flow. Selection receives a run-progress snapshot
(current index + solved flags) and never owns that state.

**Settled:** the scale in/out gesture controller is one scene instance referenced by both states; each
state subscribes while entered and unsubscribes on exit.

**Settled for the slice:** swipe is pinch-drag. Open-palm wave stays a later alternative, because it
needs its own open-hand gate and is easy to trigger while reaching for a cube.

Still open:

- **Preview source cap.** How many sources a side diorama shows (full field vs a reduced set), and
  whether the centre stays full. This sets the S1 cost measurement.
- **Amplify handoff.** Whether the centre diorama morphs into the play field or zooms/fades into the
  already-authored play field. For the first cut, prefer the handoff: the play field is a separate
  prefab and a planet-by-planet morph is a second system.
- **Gesture arbitration.** Scale and rotation read the same two-hand gesture. Pick one channel per
  gesture (first threshold crossed wins) or separate the thresholds; do not let a twist during the
  amplify change focus.
- **Swipe direction.** Swipe right = previous or next? Pick the “push the arc” mapping (drag left moves
  focus right) or the “page” mapping (swipe right = next) once on device.
- **Transition speed.** How release speed maps to tween duration (linear, eased, or stepped) and where
  the min/max clamps sit. Start with a clamped inverse mapping and tune on device.
- **Step angle.** Match the authored arc (±35°) or use a smaller step. The step sets how far the hands
  must twist per job; start at the arc angle and tune on device.

## 7. Out of scope

- The full campaign universe/backdrop; the run's seven jobs are the slice.
- Mid-play exit; multi-ship; the in-level state (the field is baked, the run restarts).
- Stars/economy beyond the per-job display.
- Free-spin/inertia on the arc; rotation snaps to a job and stops.
- Moving or renaming `PuzzleData` and the space-shaping domain assembly.

## 8. Risks

- **No run assets.** Only `01_01` exists and its id is blank, so no level cookie can match it. Until the
  run has seven valid assets with ids, the carousel can only show one job and cannot distinguish solved
  from locked.
- **S3 depends on flow.** `PuzzleEditionState` and `PuzzleCompleteState` are still thin. The solve
  signal, result panel, and route back to selection are being built separately; S1/S2 do not wait on
  them, but S3 cannot be accepted before they land.
- **The selection is still a stub.** `Enter` routes immediately; adding a view without removing that
  route would flash the carousel and start job 0.
- **Preview cost.** Three dioramas with planets and a sun are real objects; the current baked field has
  13 sources. The session build is data-only, so pool the views from a lightweight prototype, cap the
  sides, and measure before treating S1 as done.
- **Gesture conflict.** The amplify is the scale in play; the exit uses the inverse (hands closer), with
  a distance + ratio threshold so an ordinary play scale does not exit. Tune both thresholds in the
  headset before treating the exit as final.
- **Rotation stability.** Two-hand yaw is noisy when the hands are close or nearly vertical. Gate on the
  horizontal separation, unwrap the angle, and keep hysteresis around the step; otherwise the arc
  flickers between jobs or jumps two at once.
- **Scale/rotation arbitration.** A two-hand gesture can change distance and yaw at the same time. Lock
  the gesture to the first channel that crosses its threshold, or a twist during the amplify will change
  focus just before entry.
- **Swipe vs cube touch.** A pinch that starts over a side cube must focus that cube, not step the arc.
  Gate the swipe on the gesture's busy-interactor flag and require the press to start off the cubes.
- **One step per pinch.** Without a per-pinch lock and a small cooldown, a long drag steps several jobs
  and overshoots. Commit once on release (or on the first threshold crossing), then wait for pinch end.
- **Velocity spikes.** A single noisy frame can make the snap too fast or trigger a flick. Use a short
  averaged window, a minimum flick speed, and a clamped duration; never let speed add a second step.
- **First-launch bypass.** A first-launch flag must skip the map; after the first solve the map appears
  with job 1 lit and job 2 current.
- **Mid-run loss.** Exiting mid-shape loses committed edits (the field is baked, the run restarts);
  returning only from the result avoids the surprise.
- **Comfort.** The arc sits at a seated, arm's-length distance; the sides are tilted to the player and a
  small palm drag is enough — no large arm sweeps.
