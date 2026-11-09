# The first 30 seconds — the console boots

**Status:** now · presentation · updated 2026-11-09 — launch → the console boot → the field placed
where the player actually is → the first pinch, with no setup and no extra taps. Road step 5.
U1 (the boot) and U2 (`Features/Placement`, `SeatPlacement`) are in, pending the headset pass; U3 is open.

**Related:** [lore](../../design/world/lore.md) §The console / §In the game ·
[vision](../../design/vision.md) §2 · [pitch](../../design/pitch.md) ·
[pause](pause.md) and [offline](../backlog/offline.md) (the launch's other judge tests) · [plans board](../README.md)

## Goal

The judge's one-bus-stop test starts at launch, not at the first puzzle. A cold launch should read as
the company's console booting, put the field at seated reach in front of the player — even when the
headset was lying on the desk at launch — and reach job 1's first pinch fast.

**What already runs at launch** (`SetupState`, the scene's initial state): an internet check, a device
check, `OVRSpaceSetup` (places the origin in front of the head right away), the player-data check, then
`AnimatorSplashScreen` — authored, but `skip: true`. A fresh save then auto-enters job 1
(`PuzzleSelectionState`); a returning save opens the map. The origin is placed again at every job start
(`PuzzleStartState`) and on a system recenter.

## Contract

- **Diegetic, no new system.** The boot is the tech team's console: thin frame, monospace, phosphor
  line art, CRT boot/cut ([lore](../../design/world/lore.md) §The tech team). No second character.
- **No new input.** Launch → the first pinch; nothing to tap, scan, or place by hand.
- **Short.** The judge plays from a cold launch, so the boot must not delay the first pinch (the video
  is edited and can open on the stretch either way).
- **Straight to play.** Fresh save: boot → job 1, no card. Returning save: boot → the map.
- **No reactive hints.** Text comes from the console or the prompts, never from the player being stuck.

## Units

### U1 — the boot

- A console boot view, played as the content of the existing splash (turn `skip` off), with its own
  small controller that types the lines and reports when done.
- Lines: `OUR SPACE CORP · DISPATCH CONSOLE · REMOTE SEAT 2`, then *"Revenue is relative."*, then the
  `OUR SPACE` title. **On a fresh save only**, add `WELCOME, NEW EMPLOYEE.` — the manual's opening line,
  seen once with no card *(proposed)*.
- Target ≤3 s, never spoken, once per launch.
- **Exit:** a cold launch reads as the console, not a menu; the welcome line shows only on a fresh save.

### U2 — placed where the player is

- **Wait for a player, then place.** Today the origin is placed as soon as setup runs, so a headset
  lying on the desk at launch puts the field wherever the headset was pointing. Place only once the
  headset is worn (user present) and the head pose has settled; the boot plays in front of the head at
  that moment, and the field is placed as the boot ends.
- **Head for direction and distance, hands for height.** Forward from the head's yaw (ignoring pitch)
  at seated reach. When the hands are tracked and resting in front, set the field just above them — the
  hands find the desk; otherwise fall back to a fixed offset below the head. Clamp the height to a
  comfortable band.
- **Re-place on the right events only:** putting the headset back on after taking it off, and a system
  recenter. A job start keeps the current placement (today it re-places every job).
- **Exit:** launch with the headset on the desk, put it on — the field appears at seated reach in front
  of you; hands on the desk put it just above them.

### U3 — the first read

- Job 1 opens with no card; the Scale prompt is visible before the hand reaches the cube.
- Job 1's unused tech-voice line ("One pinch. You'll be fine.") leaves [jobs](../../design/jobs.md)
  *(proposed)*; Jim's first line stays job 1's completion receipt (already true: job 1 is `noIntro`).
- **Exit:** a first-timer pinches inside the cube within 15 s of the boot ending.

## Out of scope

New console art beyond the existing frame, a second speaking character, a separate tutorial flow,
table detection from the room scan, and changes to job 1's bake.

## Manual check (headset)

1. Erase the save, launch with the headset on: the boot shows the welcome line, then job 1 opens.
2. Launch with the headset lying on the desk, then put it on: the field appears in front of you at
   seated reach, not where the headset was pointing.
3. Hands resting on the desk at placement: the field sits just above them.
4. The Scale prompt is visible before the hand reaches the cube.
5. Launch to first pinch is ≤30 s.
6. A returning save shows no welcome line and goes to the map.
7. Take the headset off and on mid-job: the field re-places in front of you; a job start alone does not
   move it.
8. No Jim line plays before job 1's completion.
9. Airplane-seat pass: seated, two-foot radius, no reach outside comfort.
