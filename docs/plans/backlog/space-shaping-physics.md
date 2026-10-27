# Space Shaping — Physics (clock + mass)

**Status:** next · space shaping · updated 2026-10-27 — the sim's physics: the two-term clock and
`mass ∝ radius³`, plus the off-path time tutorial. Split out of
[the creator plan](../in-progress/puzzle-creator-improvements.md) so each plan is a morning.

**Related:** [creator plan](../in-progress/puzzle-creator-improvements.md) ·
[cube](../../design/systems/cube.md) · [relativity](../../design/systems/relativity.md) ·
[progression](../../design/systems/progression.md) §4

## Goal

Make the model physical, then teach its time face. The creator's bakes search the sim, so land this before
baking variants.

## The changes

- **Two-term clock.** Replace the tuned clamp in `TrajectoryIntegrator` with the weak-field form
  `dτ = dt · √(max(0, 1 + 2Φ/c² − v²/c²))` — GR depth + SR speed. `Φ` and `v` are already per step; no
  new data, no new path.
- **`mass ∝ radius³`.** Replace the `mass ∝ radius` coupling (constant density), so a grown planet gains
  mass the way a real one would. Retune the field strength (`sourceStrengthScale`).
- **No separate scale term.** Time comes from `Φ` (mass) and `v`; the fictional `g(scale)` is dropped.
- **The time tutorial is an off-path planet.** A planet **off the trajectory** isolates the clock: the
  scale changes its mass and `dτ`, without bending the path
  ([progression](../../design/systems/progression.md) §4).

**Why here:** these are the mechanic's physics; keep it offline and EditMode-tested (determinism).

## Exit

The clock uses the two-term form; mass is `∝ radius³`; a puzzle re-bakes and verifies with the retuned
field; an off-path level reads as a pure clock puzzle.
