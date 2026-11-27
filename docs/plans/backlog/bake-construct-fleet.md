# Bake — construct the answer first (fleets)

**Status:** next · puzzle creator · updated 2026-11-27 — the constructor for two and three ships:
Separate (shared blocker, shared bend) and FanOut (one bend near a shared origin). Not started.

**Bakes:** off the editor — U3 (02_06 and 02_09 as the first real jobs, for
[chapter 2 rebakes](chapter-2-rebakes.md)).

**Related:** [plans board](../README.md) · [bake construct core](bake-construct-core.md) (must land
first)

## Why

Fleets are where search costs explode: every ship must land with one zone, so a random field almost
never has a shared answer (three ships: hundreds of seeds, and the shipped 02_06 stopped solving). With
the answer placed first, the shared planet is the point of the field, not luck.

## Units

### U1 — Separate

- **Clear:** two chords that cross (or pass within a blocker's radius of each other); the blocker sits
  at the crossing, deflecting both start paths; the answer shrinks it. This is 02_07's lesson built on
  purpose.
- **Curve:** a fixed obstacle on each chord and one bend planet between them; growing it pulls both
  routes round. Tune scale by bisection on the worst ship's miss.
- Exit: 20 seeds of each construct and pass the existing fleet validation (`CheckFleet`); rates here.

### U2 — FanOut

- Shared origin, a bend planet near it. The launch aims at each destination, so placing a destination
  moves its own launch: iterate — aim, simulate with the answer, move that destination planet onto the
  path's closest approach — and stop once each ship lands or after a few steps. Spike this first and
  write the convergence rate here; if it does not converge, fall back to the search for FanOut.
- Three ships is the same loop with a third destination; check the bend planet sits so all three paths
  pass it on the same side.
- Exit: two- and three-ship FanOut construct and pass on most of 20 seeds, or the spike's note says
  why not.

### U3 — the first real jobs

- 02_06 (three ships, Separate, Curve) and 02_09 (three ships, FanOut, Curve) with
  `Generator = Construct`, as [chapter 2 rebakes](chapter-2-rebakes.md) U2–U3. Same export, answer
  asset and per-job commit as any bake.
- Exit: both baked and passing `ShippedJobs_MeetTheContract`, or recorded as search-baked.
