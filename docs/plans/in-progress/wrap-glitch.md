# Wrap glitch — the projection strains

**Status:** now · presentation · updated 2026-11-07 — short, small glitches on the console panels and
the field that read as the experimental Wrap straining; the field's dust follows Field Elements tier 3.
U1 (panel glitch) and U2 (field beats) land; U3 (dust) remains.

**Related:** [field elements](../../ideas/element-pack.md) · [lore](../../design/world/lore.md) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## Goal

The Wrap is new and "can literally destroy the universe" ([lore](../../design/world/lore.md)). Let the
projection show it: the console panels tear for a moment now and then, and the field glitches at its
beats. Small and short, so it reads as an event, never as noise over the puzzle.

## Contract

- **Comfort first.** No flashing or flicker over large parts of the view; no glitch longer than ~0.15 s;
  no full-screen post-processing.
- **Not during the solve.** The field never glitches continuously while the player works; field
  glitches happen only at beats (enter, fail, solve).
- **Panels glitch on their own clock.** Every panel drawn with the `UIPhosphor` materials tears a few
  horizontal bands sideways for ~0.1 s every few seconds; all panels share the clock, so the console
  glitches as one screen.
- **Dust is Field Elements tier 3,** not a particle system: deterministic, weightless-looking, shaped
  by the cube.

## Units

### U1 — panel glitch

- `UIPhosphor` gains a band tear: interval, duration, strength and band count as material properties.
- The job card's background uses `UI Phosphor Screen`, so the cards share the panels' screen and glitch.
- **Exit:** Jim's panel and the cards tear briefly every few seconds; text stays readable.

### U2 — field glitch beats

- A stutter of the field (three ~0.08 s bursts: a ~1.5 cm jump, lattice bands tearing sideways, dropping and flaring) on entering a job, a fail and the
  solve. `FieldGlitch` on the puzzle prefab; `PuzzleStartState` and `PuzzleCompleteState` play it.
- **Exit:** each beat glitches once, briefly; nothing glitches mid-solve.

### U3 — dust (Field Elements tier 3)

- Decide veil vs points per [field elements](../../ideas/element-pack.md) §3, then build it in the
  warped space so the cube shapes it.
- **Exit:** empty space reads as space; nothing reads as a source.

## Manual check (headset)

1. Jim's panel and the job card tear briefly every few seconds; the text stays readable.
2. The tear is small and short; it never flashes the view.
3. Enter, fail and solve each glitch the field once; during the solve nothing glitches.
4. Dust drifts with the cube's warp and never looks like a planet.
5. No frame drop on the Quest 3S.
