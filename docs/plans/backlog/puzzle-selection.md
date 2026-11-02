# Puzzle selection — the run carousel

**Status:** later · flow · updated 2026-11-02 — an arc of job fields, one focused, touched and amplified
into play; starts after the PoC is integrated into the main scene.

**Related:** [progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[core loop](../../design/core-loop.md) · [first-minute read](../in-progress/first-minute-read.md)

## 1. Why

The run needs a level selector, but a flat grid of full fields is noisy and expensive; a carousel keeps
one field legible while the neighbours give the run's shape. The selection must also honour the first
launch (straight to the tutorial) and return the player to the map later. The carousel is chosen over the
grid: one focus, three previews, and the same cube/amplify language as play.

## 2. Slices

### S1 — the carousel

- An authored arc: the focused field at the centre, one at each side (±~35°), the rest implied by the
  numbered cubes. Each job shows a **diorama** of its baked field (planets, sun, the cube) — pooled from
  an authored prototype, no runtime hierarchy — and a number.
- Cube states: current (highlighted), solved (lit + stars), locked (dark). Locked refuses visibly on the
  amplify; focus is allowed so the player sees what is next.
- Cap the visible previews at three; the full field is built only on entry.
- **Exit:** a 7-job run reads at a glance, the current/solved/locked states are distinct, and the preview
  cost is measured.

### S2 — focus and the first launch

- Swipe changes the centre one job at a time; touching a side cube brings it to the centre. The amplify
  is the only way to enter, so a swipe or a touch never starts a job.
- First launch skips the map and starts job 1; the map is the return state after the first solve. No
  empty map is shown.
- **Exit:** a player can reach any unlocked job by swipe or touch, and a fresh install goes straight to
  the tutorial.

### S3 — enter, return, and state

- The two-hand amplify grows the focused field/cube into the play space (an authored transition, no
  scene load). The result offers **Map/Retry**; the map is the only navigation.
- The return is the **inverse** of the amplify (pull the cube/frame apart), not the scale pinch: the same
  pinch is the play verb, so reusing it would exit by accident. A mid-play exit is a later comfort
  feature; for the slice, return only from the result.
- On a solve: light the cube, award stars, open the next job, highlight the new current. The map fills.
- **Exit:** amplify enters, the inverse returns from the result, the play pinch never exits, and the
  solved/current/locked state survives the session.

## 3. Decisions needed before code

**Settled:** the return is the **inverse** of the amplify (pull the cube/frame apart), not a held
amplify — the same two-hand vocabulary, opposite direction.

- **Preview fidelity.** A pooled diorama (recommended) vs a flat cube thumbnail; the diorama costs more
  but carries the field.
- **Locked focus.** Focus and refuse (recommended) vs skip locked in the swipe.

## 4. Out of scope

- The full campaign universe/backdrop; the run's seven jobs are the slice.
- Mid-play exit; multi-ship; the in-level state (the field is baked, the run restarts).
- Stars/economy beyond the per-job display.

## 5. Risks

- **Gesture conflict.** The amplify is the scale in play; if the exit reuses it, a scale will eventually
  exit. Keep the inverse, and only from the result for the slice.
- **Preview cost.** Three dioramas with planets and a sun are real objects; pool from an authored
  prototype and build the full field on entry, or the map costs more than the puzzle.
- **Tutorial bypass.** A first-launch flag must skip the map; after the first solve the map appears with
  job 1 lit and job 2 current.
- **Mid-run loss.** Exiting mid-shape loses committed edits (the field is baked, the run restarts);
  returning only from the result avoids the surprise.
- **Comfort.** The arc sits at a seated, arm's-length distance; the sides are tilted to the player and a
  small palm drag is enough — no large arm sweeps.
