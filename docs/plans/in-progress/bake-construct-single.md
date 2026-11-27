# Bake construct — one ship, Clear and Curve

**Status:** now · puzzle creator · updated 2026-11-27 — the two single-ship recipes on the
[core](../in-progress/bake-construct-core.md): a blocker to shrink, a bend to grow. U1–U2 implemented;
the 20-seed rates below; the wire comparison next.

**Bakes:** off the editor — the 20-seed rates are a Creator test loop through the constructor and gate
(bake-heavy; run in the bake checkout); the `bake.ps1` seam lands in
[wire](../in-progress/bake-construct-wire.md).

**Related:** [plans board](../README.md) · [construct core](../in-progress/bake-construct-core.md) (must land first) ·
[construct wire](../in-progress/bake-construct-wire.md) (next)

Same rule as the core: only what today's jobs use; **no anchored (`fixedMass`) planets**.

## Units

### U1 — Clear (target `01_03`)

- `01_03`: fixed, `MinRoutes = 2`, `MinBlockers = 1`, `SunRule = OutOfZone`; the only chapter 1 Clear
  job.
- Place the main blocker beside the chord (the Clear offsets), sized so its full body blocks or
  deflects the start past the capture radius; the cube encloses it (a fixed job ships the solved zone
  as its frame).
- Tune the shrink: sweep the scale between `PuzzleSimSettings.MinScaleFactor` (1 when `GrowOnly`) and
  1, take the middle of the landing range. `PuzzleSolveWindow.Measure` only returns the width around a
  given scale, so add a small range helper.
- `MinRoutes = 2`: the pair cube over the main blocker and a smaller second blocker one separation
  away is the second route. The shipped `01_03` field's stored route count is 2 but today's
  `PuzzleRouteCount` finds 1, so the construct must place the second route.
- Exit: 20 seeds of `01_03` through the core's gate; 12/20 pass (3 `SingleRoute`, 4 `NoRun`, 1
  `Unverified`); the test asserts above half.

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
  [construct separate](../in-progress/bake-construct-separate.md) U3, whose 02_06/02_03 are movable.
- **The start must crash and curve.** `NoFail = true` makes `PuzzleStartCheck` require a crash, not
  just a miss; the blocker on the chord supplies the crash, and the bend must deviate the pre-crash
  path by ≥5 % of the run or 4° (the core's gate). Keep the answer's cube clear of the sun
  (`OutOfZone`).
- Exit: 20 seeds of `01_01` through the gate; 11/20 pass (9 `NoRun`); the test asserts above half.

## Review (2026-11-27)

Both recipes are in and recorded (Clear 12/20 on `01_03`, Curve 11/20 on `01_01`).

- **The single-ship "shrinking must not solve" gate is missing.** U2 says the verdict sweeps the
  blocker's shrink and rejects a seed that lands, but `PuzzleConstructBake.ClearShortcut` only runs for
  fleets (`IsMulti`; `PuzzleFleetSolution.TryFind` needs two ships). `01_01` is `GrowOnly`, so it is
  vacuous there, but the next non-`GrowOnly` single-ship Curve contract (chapter 3, freelance) would
  bake with a Clear shortcut. Add the single-ship branch (a Clear-only `PuzzleSolutionSearch` on the
  variant) when `!GrowOnly`, with one test.
- `NotCleared` is reused for "a Clear shortcut exists"; it reads as the opposite. Add a status
  (`ClearShortcut`) at the end of the enum.
- Docs: the status line says `next` but the plan is in `in-progress/` (the docs check fails); set
  `now`.

### To do (from the review)

1. Add the single-ship Clear-shortcut gate for non-`GrowOnly` Curve contracts, with a test.
2. Add a `ClearShortcut` status at the end of `PuzzleBakeStatus` and use it instead of `NotCleared`.
3. Set the status line to `now` (the plan is in `in-progress/`); run `docs/check.ps1`.
