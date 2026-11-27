# Bake construct — fan-out fleets

**Status:** next · puzzle creator · updated 2026-11-27 — two and three ships from one origin, bent by
one planet near it; a spike first, with the search as the fallback. Not started.

**Bakes:** off the editor — the 20-seed rates are scans; run them in the bake checkout.

**Related:** [plans board](../README.md) · [construct separate](bake-construct-separate.md) (must land
first) · [construct jobs](bake-construct-jobs.md) (next)

Same rule as the core: only what today's jobs use; **no anchored (`fixedMass`) planets**.

## Units

### U1 — the spike

- Shared origin, a bend planet near it but clear of the origin. A ship launches at its destination, so
  placing a destination moves its own launch: iterate — aim, simulate with the answer, move that
  destination onto the answered path's closest approach — and stop once each ship lands or after a few
  steps.
- After each move: a destination is also a planet, so the pristine run must still miss by more than
  its capture radius (`StartFails`), and the grown zone must not contain the shared origin.
- Write the convergence rate here. If it does not converge, stop: fan-out stays on the search, and
  [construct jobs](bake-construct-jobs.md) bakes only 02_06.
- Exit: a convergence rate on 20 two-ship seeds.

### U2 — two and three ships

- The same loop with a third destination; check all three paths pass the bend on the same side.
- Tune by the separate plan's scale intersection, accepted only inside the scramble bounds.
- Exit: 20 seeds against 02_04's contract (two ships) and 20 against 02_09's (three ships, Curve), the
  02_09 answers meeting `MinScaleDelta` / `MinStartDistance`. Rates here.
