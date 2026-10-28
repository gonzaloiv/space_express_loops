# Space Shaping — Physics (clock + mass)

**Status:** now · space shaping · updated 2026-10-28 — the two-term clock and `mass ∝ radius³` are
landed; the creator now bakes the off-path clock level (start margin + off-path cube) on a
**proper-time** window. What remains is the POC re-bake and the headset pass.

**Related:** [cube](../../design/systems/cube.md) · [relativity](../../design/systems/relativity.md) ·
[progression](../../design/systems/progression.md) §4

## Goal

Make the model physical, then teach its time face. The creator's bakes search the sim, so land this before
baking variants.

## Confirm before coding

- **Clock coefficient — decided.** `lightSpeed` is the coefficient (`c² = 2`, toy scale); real `c` makes
  `2Φ/c²` and `v²/c²` vanish at metre/s scale. The same `c` feeds both terms.
- **Mass retune — decided.** `mass = radius³`; `sourceStrengthScale` is `5.5556` (the old nominal well at
  the old base `0.06`). The field now samples radii from `planetRadiusMin = 0.03` to
  `planetRadiusMax = 0.09`; the scale band is narrowed to `0.6–1.8` (a `0.216–5.83×` strength range).
  The catch is now the destination planet's radius: `PuzzleData.captureRadius` is baked from it, not a
  global setting.
- **Clock floor — decided.** `max(0, …)` stays. Zero is the horizon limit for a distant observer; in this
  stylized model it is a **clock stop**, and the window excludes zero.
- **Window and bake — partially done.** Seed 1 re-bakes and verifies; the POC is re-baked. `Tcross` is
  still coordinate time, so a tight window centred on it is not the nominal *proper* time — the next
  tuning pass should centre the band on the proper crossing (or the solution's time).
- **Off-path tutorial — remaining.** `StartFails` accepts an arrival **outside the window** (and rejects a
  crash), so the contract is ready. A probe found the next blocker: the bake's bare start is `0.9×
  nominal` with a `run / nominal` step budget, so it **cannot arrive** — a time-only level needs a
  per-level start margin (~`1.0`) and a fixed **off-path** cube centre (or a translatable clock target).
  The creator has neither dial yet; the probe was removed and the core suite is green.
- **Test churn — done.** Clock and mass tests added; the time thresholds and pinned seed are green.

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

## Landed (2026-10-27)

- **Two-term clock** in `TrajectoryIntegrator`: `√(max(0, 1 + 2Φ/c² − v²/c²))`, with the ship's
  velocity per step and a toy `lightSpeed` of `1.414`.
- **`mass ∝ radius³`** in `SpaceShapingSession.BuildField`; `sourceStrengthScale` retuned to `5.5556`,
  with the scale band narrowed to `0.6–1.8`.
- **`StartFails`** accepts a miss or an arrival outside the window, and rejects a crash; the time-only
  start is no longer required to miss.
- **Tests**: clock form, mass cube law, out-of-window start and crash-start cases.
- **POC re-baked** with the retuned field; the asset verifies.

## Landed (2026-10-28)

- **Per-level start margin.** `PuzzleSeed.startMargin` (0.5–1.5× nominal) is baked into
  `PuzzleData.launchSpeed`. The time seed runs at `1.05×`, so the bare start **arrives** instead of
  exhausting the `run / nominal` step budget; path levels keep the `0.9×` miss.
- **Off-path clock level.** `PuzzleSeed.offPath` makes the bake scan every source at least `0.35 ×`
  the run from the trajectory, keep a cube that contains only that planet, and require a scale whose
  proper-time shift clears the bare arrival by `1.5 ×` the window. The fixed cube is baked at the
  found planet (`zoneCentre` = the solution centre); `PuzzleData` is unchanged.
- **Proper-time window.** `PuzzleWindow` simulates the pristine field at nominal speed and centres a
  path level's window on that **proper** crossing; a clock level's window is centred on the found
  clock solution. `windowTightness` is a fraction of the proper crossing (the time seed uses
  `0.005`), so a tight window is finally reachable.
- **Tests.** `PuzzleClockBakeTests` covers the clock bake (bare arrives outside the window, the
  solution arrives inside, the cube sits off-path); `PuzzleBakeTests` covers the proper-crossing
  window, the start-margin bake, and the expanded brief.

## Exit

The clock uses the two-term form; mass is `∝ radius³`; a puzzle re-bakes and verifies with the retuned
field; an off-path level reads as a pure clock puzzle.

**Met:** clock, mass, proper-time window, re-bake, and the broader `StartFails`. **Met in code:**
the off-path clock level (start margin + off-path cube) and its EditMode coverage. **Remaining:** the
POC re-bake with the new window and the headset pass.
