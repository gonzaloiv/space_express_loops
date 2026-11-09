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

- **The ship stops short.** Today it flies to the clipped end of the run and shrinks away there, so it
  reads as flying into the planet. It should stop before the surface — the gap covers the ship's length —
  hold there facing the destination, and stay visible through the beat.
- **The destination plays the arrival.** The planet itself reacts (a pulse of its surface plus an
  authored burst in the `DustCube` look) instead of an effect on the ship.
- **Exit:** a solve parks the ship clear of the planet and the planet plays the arrival once; nothing
  plays on a fail.

### U2 — the stars stamp

- A small terminal-style line in the `UIPhosphor` look — `ECO ★★☆` — floating in the field above the
  destination, typed in one star per ~0.25 s, each with a tick ([cube sound](cube-sound.md) U3).
- **Exit:** a graded solve shows its stars, one by one; `noGrade` jobs show none.

### U3 — the sequence

- `PuzzleCompleteState`: on a solve, burst → stamp → Jim's receipt → return; on a fail, the glitch →
  Jim's line → return.
- **Exit:** the solve reads as one beat, with no dead time and no overlap with Jim's voice.

## Manual check (headset)

1. Solve job 1: the ship stops short of the destination and stays visible; the planet plays the
   arrival; no stars; then Jim speaks.
2. Solve job 3: the burst, then 1–3 stars stamp one by one, then Jim speaks.
3. The burst never flashes the whole view and reads as part of the field.
4. Fail a job: the glitch plays, no burst, no stars.
5. From arrival to Jim's first word feels like one beat, not a wait.
6. The return to the map still works after both a solve and a fail.
