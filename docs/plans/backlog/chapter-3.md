# Chapter 3

**Status:** next · space shaping · updated 2026-11-25 — ~6 jobs on broken bodies: anchored planets
the Wrap cannot change, then repulsors that push instead of pull; its episode and the release's
ending.

**Bakes:** off the editor — U4 (the jobs) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-3--broken-bodies-store-release) ·
[lore](../../design/world/lore.md#episodes) · [the cube](../../design/systems/cube.md)

## Goal

A playable chapter 3 that teaches two new bodies with the verbs the player already has, and closes the
story. **Anchored:** a planet fused in place; the Wrap does not change it, so the player bends the line
with its neighbours. **Repulsor:** a body that pushes; shrinking it opens a gap, growing it shoves a
ship aside. Both read as the damage: spacetime is torn and some bodies no longer behave.

## Depends on

The [story arc](../in-progress/story-arc.md) (what breaks, chapter 3's episode and clients). The
anchored body is the guaranteed visible tell; pushing bodies are the second if the gate passes.

## Notes

- **Anchored is nearly free.** `PuzzleSource.fixedMass` exists: `SourceSpecs` clamps it to its start
  scale and the zone search skips it. Today only an unscalable sun uses it.
- **Repulsors are a sign.** `SpacetimeField` already sums `Strength` with its sign; the session builds
  it at `SpaceShapingSession` (`mass * SourceStrengthScale`). `WellPull` skips `Strength <= 0`, so the
  lattice draws nothing for them yet.
- **Gate:** if repulsors do not fly and draw correctly in the editor by 1 Dec, chapter 3 ships with
  anchored jobs only (plus a second sun for variety) and repulsors move to an update. Record that here.

## Units

### U1 — the chapter

- `Chapters/03.asset` and its folder; `chapter_03_title` / `chapter_03_manual_release`
  (*EPISODE 3 · THE DAMAGE* / *DAMAGE CONTROL*); the map gains a third system; the episode and client
  rows from [chapters](../../design/chapters.md) §Chapter 3.
- Exit: an empty chapter 3 opens after chapter 2 in the editor.

### U2 — anchored bodies

- A visual for an anchored planet (authored on the planet prefab, shown by its flag) and a pinch on it
  that visibly does nothing (no cost spent).
- Bake rule: the brief can ask for an anchor on the line, and the cheapest answer without the lock
  would have scaled it (otherwise the anchor is decoration).
- Exit: a Creator test bakes an anchored field and asserts the rule; the visual shows in the editor.

### U3 — repulsors

- A `pushes` flag on `PuzzleSource`; the session and the bake build a negative `Strength` from it.
- The lattice: `WellPull` draws a repulsor as a raised bump (the sign flips the shift); a distinct
  planet look, authored on the prefab.
- Bake: the brief can ask for a repulsor on or near the line, with a Clear-style answer (shrink it) or
  a shove answer (grow it).
- Exit: a fast test flies a ship past a repulsor and asserts the push; a Creator test bakes a repulsor
  field; the bump draws in the editor.

### U4 — the jobs

- Bake ~6 as a ramp: an anchor on the line → an anchor among decoys → a repulsor to shrink → a repulsor
  to grow → a repulsor and an anchor → a capstone (both, with a fleet or a second sun).
- Exit: the job assets validate and are in `Chapters/03`.

### U5 — lines, manual and the ending

- Jim's opener, per-job lines, the chapter bulletin (`chapter_03_bulletin_01..03`), the manual entries
  for both bodies (one rule plus one fine-print line, ≤90 characters; the final entry is
  *"Welcome, new employee. No new employee in eleven years. There is still no office."*), and the
  ending: today's ending (Jim closing the episode) moves after chapter 3 or
  is rewritten; freelance plays as the epilogue, Jim's job board after the story. The final manual
  entry carries the "eleven years" note moved from job 7 (*DAMAGE CONTROL*); the roadmap slogan
  *"Today the space. Tomorrow the time."* moves here from job 7. Re-check the sector wire tail against
  the final job count. Regenerate the TTS clips. If the ending keys change, update `RunEndingView`'s
  serialized keys in `Game.unity`.
- Exit: the chapter plays end to end, ending included; [freelance](freelance.md) opens after it.

## Manual check (headset)

1. Chapter 3 opens after chapter 2's last job, with its episode opener.
2. An anchored planet reads as locked; pinching it changes nothing and spends nothing.
3. The anchor jobs are solved through the neighbours, not the anchor.
4. A repulsor reads as different (look and raised lattice) before the ship flies.
5. Shrinking a repulsor opens the gap; growing one shoves the ship aside.
6. The ending plays after chapter 3, not after chapter 2; the exit cue and the lit-map cue do not
   clash (they start almost together today; keep one if they do).
7. Time each job; the target is ~3 min; check frame rate with both new bodies in the field.
