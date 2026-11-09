# The first 30 seconds — the console boots

**Status:** next · presentation · updated 2026-11-09 — launch → seated bubble → title beat → the Scale
prompt, with no setup and no extra taps. Road step 5.

**Related:** [lore](../../design/world/lore.md) §The console / §In the game ·
[vision](../../design/vision.md) §2 · [pitch](../../design/pitch.md) (video 0:00–0:30) ·
[gesture prompts](../README.md) (Scale unlocks in job 1) · [plans board](../README.md)

## Goal

The judge's one-bus-stop test starts at launch, not at the first puzzle. A cold launch should read as
the company's console booting, put the bubble at seated reach, and reach job 1's first pinch fast. A
fresh save already auto-enters job 1 (`PuzzleSelectionState`), so the missing work is the boot/title
beat and the first read, not a new flow.

## Contract

- **Diegetic, no new system.** The beat is the tech team's console: thin frame, monospace, phosphor
  line art, CRT boot/cut ([lore](../../design/world/lore.md) §The tech team). No second character.
- **No new input.** Launch → the first pinch; nothing to tap, scan, or place by hand.
- **Short.** The pitch's first 10 seconds are the stretch, so the boot and title must not push the hero
  moment back.
- **Fresh save vs returning save.** Fresh: boot → job 1. Returning: boot → the map.
- **No reactive hints.** Text comes from the console or the job card, never from the player being stuck.

## Units

### U1 — the boot and the title

- The console boots with `OUR SPACE CORP · DISPATCH CONSOLE · REMOTE SEAT 2`; carry the poster line
  *"Revenue is relative."* as a console line, never spoken.
- A short `OUR SPACE` title follows, then the flow continues. Target ≤3 s to the bubble.
- **Exit:** a cold launch reads as the console, not a menu, and the title beat never replays inside a
  session.

### U2 — the bubble at seated reach

- The first bubble sits at a comfortable seated arm's reach over the player's real table/desk.
- Remove any setup prompt or hand-placement beat on the critical path; keep the same pose on a returning
  launch.
- **Exit:** a seated player can reach the cube and the field without standing, turning, or walking.

### U3 — the first read: manual + Scale prompt

- Job 1 opens; the manual's opening entry and the Scale prompt need to land together. Today job 1 has
  `noIntro`, so it skips the Edition card — decide whether the first-run manual entry is shown on entry
  or stays on the map card only.
- The Scale prompt must be visible before the hand reaches the cube.
- **Exit:** a first-timer knows to pinch inside the cube within 15 s, and the manual's *"Welcome, new
  employee"* line is seen once.

### U4 — No Jim before the first solve

- No Jim line before the first puzzle is completed; the manual and the Scale prompt carry the first
  beat.
- Jim first speaks on job 1's completion receipt, and per-job from job 2.
- **Exit:** a fresh launch reaches the first pinch with no VO; the first Jim cue is the job-1
  completion.

## Out of scope

New console art beyond the existing frame, a second speaking character, a separate tutorial flow, and
changes to job 1's bake.

## Manual check (headset)

1. Erase the save, launch: the console boot and title play, then job 1 opens.
2. The bubble is at seated reach with no scan or placement step.
3. The Scale prompt is visible before the hand reaches the cube.
4. Launch to first pinch is ≤30 s.
5. A returning save goes to the map, not job 1.
6. The boot/title never replay mid-session.
7. No Jim line plays before the first pinch or during job 1's start; the first cue is the completion.
8. Airplane-seat pass: seated, two-foot radius, no reach outside comfort.
9. Video pass: the first 10 seconds still show the stretch.
