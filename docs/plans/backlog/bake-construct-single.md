# Bake construct — one ship, Clear and Curve

**Status:** next · puzzle creator · updated 2026-11-27 — the two single-ship recipes on the
[core](../in-progress/bake-construct-core.md): a blocker to shrink, a bend to grow. Not started.

**Bakes:** off the editor — the 20-seed rates are a Creator test loop through the constructor and gate
(bake-heavy; run in the bake checkout); the `bake.ps1` seam lands in
[wire](bake-construct-wire.md).

**Related:** [plans board](../README.md) · [construct core](../in-progress/bake-construct-core.md) (must land first) ·
[construct wire](bake-construct-wire.md) (next)

Same rule as the core: only what today's jobs use; **no anchored (`fixedMass`) planets**.

## Units

### U1 — Clear (target `01_03`)

- `01_03`: fixed, `MinRoutes = 2`, `MinBlockers = 1`, `SunRule = OutOfZone`; the only chapter 1 Clear
  job.
- Place the blocker on the chord, a third to two thirds along, sized so its full body blocks or
  deflects the start past the capture radius; the cube encloses it (a fixed job ships the solved zone
  as its frame).
- Tune the shrink: sweep the scale between `PuzzleSimSettings.MinScaleFactor` (1 when `GrowOnly`) and
  1, take the middle of the landing range. `PuzzleSolveWindow.Measure` only returns the width around a
  given scale, so add a small range helper.
- `MinRoutes = 2`: the field must admit a second shrunk route (`PuzzleRouteCount`), not only the
  answer.
- Exit: 20 seeds of `01_03` through the core's gate; most pass; rate recorded here.

### U2 — Curve (target `01_01`)

- `01_01`: fixed, `GrowOnly`, `NoFail`, `SunRule = OutOfZone`, `MinRoutes = 1`; the only chapter 1
  Curve job with all four.
- Place a **scalable** blocker on or near the chord (inside the block band) so the straight shot crashes
  (`NoFail`), and a bend planet beside the chord on the side the route should pass. The answer grows the
  bend planet: sweep its scale and its offset from the chord.
- **Shrinking must not solve.** With an ordinary blocker, shrinking it might also land the ship. The
  verdict sweeps the blocker's shrink on its own and rejects the seed if any scale lands, so the job
  stays a Curve lesson without a new rule or a new kind of planet. `01_01` is both `GrowOnly` and
  fixed (the cube is on the bend), so this gate passes vacuously here; it bites in
  [construct separate](bake-construct-separate.md) U2, whose 02_06/02_03 are movable.
- **The start must crash and curve.** `NoFail = true` makes `PuzzleStartCheck` require a crash, not
  just a miss; the blocker on the chord supplies the crash, and the bend must deviate the pre-crash
  path by ≥5 % of the run or 4° (the core's gate). Keep the answer's cube clear of the sun
  (`OutOfZone`).
- Exit: 20 seeds of `01_01` through the gate; most pass; rate recorded here.
