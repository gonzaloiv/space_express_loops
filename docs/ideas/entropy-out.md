# When the Wrap runs out

**Status:** idea · presentation · updated 2026-11-26 — the moment entropy hits zero should be an
event, not just a refusal.

**Related:** [ideas](README.md) · [core loop](../design/core-loop.md) · [glossary](../design/glossary.md)
(entropy)

## Today

The field strains as entropy drains and looks about to blow near empty. At zero, new edits are refused
(`ZoneRefusal.OutOfEntropy`, a cue in `PuzzleEditionState.OnRefused`) and an unsolved job fails. The
build-up is there; the moment itself has no payoff: the warning promises something that does not happen.

## Options (pick one, keep it short)

- **The colour drains.** The field desaturates to grey in a second or two: planets, lattice, the
  ships' lines. The Wrap is spent; space goes back to being plain. Cheap (a shared material value) and
  calm, which suits the failure screen that follows.
- **It blows.** The strain peaks, then a flash and a pop: the lattice snaps flat, the planets shudder
  back to their start scales. Pays off "about to blow", louder; needs a sound and a short VFX.
- **It freezes.** Everything stops mid-motion, the ships hang where they are, a crack runs through the
  cube. Reads as "the bill came due", fits the corporate tone.
- **Jim notices.** Any of the above plus one short line (a spoken key and its clip) — fits the
  damage arc in chapter 3 (the tears are the Wrap's spend).

## Notes

- One beat, about 1–2 s, then the existing fail flow; it must not delay a retry.
- Job 1 is exempt from failing; skip or soften the effect there.
- Authored on the prefab (material values, a VFX child), not created at runtime.
