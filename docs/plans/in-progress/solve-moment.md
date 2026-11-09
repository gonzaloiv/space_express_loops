# The solve moment — the arrival lands

**Status:** now · presentation · updated 2026-11-09 — the ship's arrival becomes the payoff: a burst at
the destination, the stars stamped in, then Jim's receipt. Road to v1 step 2; it closes the pitch's hero
shot.

**Related:** [pitch](../../design/pitch.md) · [core loop](../../design/core-loop.md) §3 ·
[cube sound](cube-sound.md) (the chime and the stamp ticks)

## Goal

Today a solve plays the field glitch and Jim's line, then returns to the map. The arrival itself — the
moment the stretch pays off — has no beat. Make it land, so the player wants the next job and the video
has its ending.

## Contract

- **Sequence, about 2.5 s before Jim speaks:** the ship parks short of the destination → the planet
  plays the arrival → the stars stamp in one by one → Jim's receipt → back to the map.
- **Local, not full-view.** The burst sits on the destination planet and stays small; no flash over the
  view. It uses the dust's glitch-voxel look (small additive cubes), so the field speaks one language.
- **In the field, not on a panel.** The stamp sits in the field above the destination, not on the job
  card; whatever can be part of the field is.
- **The burst replaces the solve glitch.** The glitch stays on entering a job and on a fail.
- **Stars only where graded.** Jobs 1–2 (`noGrade`) skip the stamp; the burst still plays.
- **The fail keeps its glitch.** On a fail: the glitch beat, no burst, no stars, then Jim's line.
- **The flow state sequences it.** `PuzzleCompleteState` calls burst → stamp → TTS; the views only play.

## Units

### U1 — the ship parks, the planet arrives

- **The ship stops short.** Today it flies to the clipped end of the run, then shrinks and deactivates
  (`ShipTravelView.Fly` → `Land` → `SetActive(false)`, `:85-90`), so it reads as flying into the planet.
  On an **arriving** run it should stop `parkGap` metres before the end of the path (a serialized
  distance of about one ship length, tuned on the prefab), hold there facing the destination, and stay
  active. `Completed` must fire at the parked pose — the pool only raises `Solved` from that event
  (`ShipFlightPool:58-65`), so do not keep the current `Land` + deactivate on an arrive. The shared cap
  gap (`capGap` / `capGapFraction`) stays as is, because the trajectory lines use it too.
- **Crash and miss keep today's ending.** `Travel` already receives the run's `TrajectoryResult`, so the
  view knows the outcome before it flies: arrive → park; crash or miss → the current shrink.
- **The destination plays the arrival.** `FieldPlanetBehaviour` gets an arrival pulse (a short scale and
  brightness swell of its surface); an authored burst in the `DustCube` look, on the puzzle prefab, is
  moved to the destination and played with it. `SpaceShapingView` exposes one `PlayArrival` that does both.
  `FieldPlanetSet` keeps the destination planet private (`:17,33`), so it needs one read-only accessor
  (the planet or its world pose) for that. The brightness swell belongs on `NodeBody`: it writes
  `_BaseColor` for route tint (`:77-103`), and LitRim's `_EMISSION` keyword is off on the planet
  materials, so use a rim/base swell that does not clobber the route colour.
- **Exit:** a solve parks the ship clear of the planet and the planet plays the arrival once; nothing
  plays on a fail.

### U2 — the stars stamp

- A small terminal-style line in the `UIPhosphor` look — `ECO ★★☆` — floating in the field above the
  destination, typed in one star per ~0.25 s, each with a tick ([cube sound](cube-sound.md) U3).
- An authored world-space text on the puzzle prefab, using the `UI Phosphor Screen` material so it shares
  the panels' screen look; the puzzle already carries a world-space canvas (`Puzzle.prefab`, RenderMode 2).
  Position it from the destination pose at play time rather than hard-coding a field-local point.
- The stamp must report the end of its typing (a completion callback or event) so the flow can start
  Jim's line; a hand-tuned wait in the state will drift from the art.
- The grade is `PuzzleRunResult.Grade` — the value the flow has already saved (`PuzzleEditionState:100`) —
  not `SpaceShapingController.Grade`, so the stamped stars always match the recorded ones. `PuzzleData.noGrade`
  skips the stamp; pass the flag explicitly (grade 0 is also the noGrade value).
- **The tick is an event, not a sound call:** the stamp raises one event per star; the cue player
  ([cube sound](cube-sound.md) U3) plays the tick. Until that lands, the stamp is silent.
- **Exit:** a graded solve shows its stars, one by one; `noGrade` jobs show none.

### U3 — the sequence

- `PuzzleCompleteState`: on a solve, arrival → stamp → Jim's receipt → return; on a fail, the glitch →
  Jim's line → return. The ship has already parked when `Solved` fires (it fires on flight completion),
  so the state starts with the arrival.
- Today `Enter` plays the glitch unconditionally (`:33`) and speaks immediately (`:37`). Branch the
  glitch to the fail only, and do not call `ttsPresenter.Play` until the arrival and stamp callbacks have
  both fired. On a fail, wait out the glitch (~0.45 s) before the line so Jim does not talk over it.
- Cancel the sequence in `Exit` (stop the coroutine); a late callback would otherwise return to the map a
  second time.
- Route the new calls through `SpaceShapingController` (`PuzzleCompleteState` only holds the controller).
  It is at 247/250 lines, so split it before adding `PlayArrival` / `PlayStamp`; the view has room.
- **Exit:** the solve reads as one beat, with no dead time and no overlap with Jim's voice.

**Tests:** extract the park point into `ShipRunGeometry` and cover it in `TrajectoryCapTests` (the fast
EditMode suite); the stamp gating and the state's timing are manual checks.

## Manual check (headset)

1. Solve job 1: the ship stops short of the destination and stays visible; the planet plays the
   arrival; no stars; then Jim speaks.
2. Solve job 3: the burst, then 1–3 stars stamp one by one, then Jim speaks.
3. The burst never flashes the whole view and reads as part of the field.
4. Fail a job: the glitch plays, no burst, no stars.
5. From arrival to Jim's first word feels like one beat, not a wait.
6. The return to the map still works after both a solve and a fail.
