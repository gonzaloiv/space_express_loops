# Bake construct — fleets with separate ships

**Status:** next · puzzle creator · updated 2026-11-27 — two and three ships from different origins,
sharing one blocker (Clear) or one bend (Curve). Not started.

**Bakes:** off the editor — the 20-seed rates are scans; run them in the bake checkout.

**Related:** [plans board](../README.md) · [construct wire](bake-construct-wire.md) (must land first) ·
[construct fan-out](bake-construct-fanout.md) (next)

Same rule as the core: only what today's jobs use; **no anchored (`fixedMass`) planets**.

## How the shared planet shapes the build

- **The shared planet comes first.** Place the blocker or bend, then each ship's endpoints so every
  chord crosses its block band (`radius × 2`, `PuzzleRunChoice.PlanetBlockBandFactor`, made public in
  the core).
- **Tuning is an intersection.** N ships have N landing-scale ranges: sweep the scale, intersect the
  ranges, take the middle, and accept it only inside the contract's scramble bounds.
- **Decoys clear every ship.** The core's decoy step rejects spots near any ship's start or answered
  path; a failed verify moves the decoy nearest the failing run.
- **Build, then verify** on the main thread with the core's gate, plus `PuzzleJobValidator`'s fleet
  checks (every ship lands, relation, ship count).

## Units

### U1 — shared blocker (Clear)

- The blocker sits where the chords cross; it blocks every start; the answer is one cube shrinking it
  (02_07's lesson, built on purpose).
- Exit: 20 seeds against 02_07's contract (two ships) and 20 against a test-local three-ship copy of it
  (`Ships = 3`; 02_08 is a parked two-ship job, so it is not that fixture). Rates here.

### U2 — shared bend (Curve)

- A **scalable** blocker near each chord so every start fails, and one bend planet in the region all
  chords pass; the answer grows it.
- **Shrinking must not solve:** sweep each blocker's shrink on its own and reject the seed if that
  lands every ship, as in [construct single](bake-construct-single.md) U2.
- Exit: 20 seeds against 02_03's contract (two ships; assert the kind is Curve) and 20 against 02_06's
  (three ships, Curve). Rates here.
