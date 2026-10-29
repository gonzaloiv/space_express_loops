# Space Shaping — Shape the Path

**Status:** later · space shaping · updated 2026-10-29 — the field becomes a maze and the trajectory must be
molded between planets with the cube; the time window is retired and arrival replaces it. Supersedes
the parked multi-zone plan.

**Related:** [vision](../../design/vision.md) §4/§7 · [the cube](../../design/systems/cube.md) ·
[relativity](../../design/systems/relativity.md) · [jobs](../../design/jobs.md)

## 1. Why

Today the generator clears a straight lane: `PuzzleRunChoice.CorridorIsClear` rejects any origin/destination
pair whose direct segment passes near a planet, and `corridorClearance` keeps it wide
(`PuzzleRunChoice.cs:47-62`). The bare run therefore always *reaches* the destination and only misses on
**time**, so every level is a 1° timing nudge and one zone always tunes it — the reason multi-zone was
parked. The feedback is a clock number, not a bend.

**The change:** the field is a **maze** — planets sit across the route — and the player **molds the
trajectory between them** with the cube. Arrival replaces the window as the target, so the visible weave is
the feedback. **Entropy stays the edit budget** — how much the player may warp the puzzle space — and the
maze supplies the difficulty.

**Validation.** Validation 1 passed on the *timing* mechanic (2026-10-29). This changes the core mechanic, so
**Validation 1 must be re-run** on the maze before integration continues; treat this plan as pre-validation.

## 2. Slices

### S1 — Maze field

- Invert generation: drop `CorridorIsClear`; require the run to be **blocked** (planets across the direct
  segment). Pick origin/destination so the straight shot hits a planet, not the clearest pair.
- Re-tune `corridorClearance`, `separationScale` and `planetCount` for a maze, not a lane.
- **Exit:** a baked field where the bare straight run **crashes**; the direct line intersects at least one
  planet.

### S2 — Arrival, no window

- Acceptance is `Arrive` (spatial) — drop `tmin`/`tmax`, `PuzzleWindow`, the clock search
  (`PuzzleClockSearch`, `offPath`, `ClockCentres`) and the 1°-nudge knobs (`applyRandomAngle`,
  `startMargin`).
- `SpaceShapingSession.IsSolved` → arrival only; remove the clock from the integrator (`Dilation`,
  `TrajectoryResult.Times`/`ShipTime`) and the readouts (`ShipClockView`, the window label,
  `PuzzleArrival`).
- **Exit:** a maze level bakes where the bare shot fails (crash) and a molded run arrives; no clock anywhere
  in the bake, runtime or UI; EditMode green.

### S3 — Legibility + POC

- Make each zone's bend **legible** under gravity-global (§3): the bend is diffuse, so show *which* planet
  or zone the ship is near — lattice warp at the zone, tint, trail — rather than changing the physics.
- Prove it in `SpaceShapingPOC.unity`.
- **Exit:** the ship visibly weaves and each zone reads as its own bend; console clean.

## 3. Decisions that block code

- **Zone verb — decided (2026-10-29): gravity-global.** Keep the physics: scaling a planet's mass
  (`mass ∝ radius³`) bends the path *everywhere* via `SpacetimeField.Acceleration` (inverse-square, summed
  over all sources), and the clock came from the same potential. `SpaceZone.StepDivisor` (the position-gated
  metric) stays a secondary effect, not the primary verb. Consequence: a zone's bend is diffuse, so
  legibility must come from *showing* which planet/zone the ship is near, not from changing the physics.
- **Difficulty — resolved:** entropy is the **edit budget** — the total amount the player may warp the puzzle
  space. The maze is fixed; the player spends entropy editing space to route the ship through it. Fail = the
  budget is exhausted before arrival (as today). No window needed.

## 4. Tests

- Field: the straight run crashes on a maze seed; the run is blocked.
- Bake: arrival-only acceptance; deterministic.
- Retire: no `tmin`/`tmax`/`ShipTime` in runtime or creator; the clock tests are deleted.

## 5. Out of scope

- Several zones (parked 2026-10-29, at the author's request).
- The no-commit / persistent-zone input model (raised 2026-10-29, parked).
- Rotate, multi-ship, runtime generation.
- HUD/result panel — a separate plan.

## 6. Risks

- **Validation:** the mechanic changed, so Validation 1's pass no longer applies; re-run before integrating.
- **Bake cost:** longer runs × more steps; the seed scan is offline but may take minutes.
- **Maze quality:** a field blocked enough to weave but solvable within the zone budget is a real search —
  it may need a new generation pass, not just an inverted rule.
- **Rebake:** every baked asset is invalidated (schema + field change).
