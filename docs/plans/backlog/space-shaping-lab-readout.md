# Space Shaping — Lab Readout

**Status:** next · presentation · updated 2026-10-27 — the minimal validation-1 readout: entropy, window,
ship time, result and grade. **Not** the final HUD.

**Related:** [vision](../../design/vision.md) §7 · [core loop](../../design/core-loop.md) ·
[resources](../../design/systems/resources.md) ·
[multi-zone](space-shaping-multizone.md) · [integration](space-shaping-integration.md) ·
[feel](space-shaping-feel.md)

## Goal

Make validation 1 testable in the lab. The tester must see the goal (the window), the budget (entropy), and
the result of each flight. The POC currently has **no HUD and no readout events**; the old lab HUD is gone.

## The surface

- **`PuzzleReadout`** (plain struct): `EntropySpent`, `EntropyBudget`, `EntropyLeft`, `ShipTime`, `Tmin`,
  `Tmax`, `Outcome`, `IsSolved`, `Grade`, `CommandCount`.
- **`SpaceShapingState`** exposes `Readout` and one `Checked` event (or `ReadoutChanged`). It updates on
  reset, preview, commit and flight completion.
- The readout is **derived**; it adds no domain rules. `SpaceShapingSession.Grade()` already owns the star
  buckets.
- **Timing:** the flight is the check. A miss is not a fail; entropy zero is the fail. If the run seam
  (`PuzzleRun`) has not landed, the state publishes the last `TrajectoryResult` and the session’s entropy;
  the contract is the same.
- No persistence; no map; no final HUD.

## The panel

- One authored world-space `TextMeshPro` panel in `SpaceShapingPOC.unity`, with serialized refs. No runtime
  creation.
- Lines: `ENTROPY 0.42 / 1.00`, `WINDOW 5.0 – 9.0 s`, `SHIP 7.4 s`, `RESULT: IN WINDOW / MISS / OUT OF
  ENTROPY`, `GRADE ★★☆`, `COMMANDS 2`.
- A reset button wired to `SpaceShapingState.ResetPuzzle`; authored, not created at runtime.
- Update from the state event, not a per-frame poll; cache the formatted strings so the lab does not
  allocate in the loop.
- Neutral styling only: the window gauge, diegetic entropy and collapse are the
  [feel](space-shaping-feel.md) work.

## Slices

### R1 — Readout contract

- Add `PuzzleReadout` and the state exposure/event; tests over a synthetic session.
- **Exit:** entropy, window, ship time, solved and grade match the session; a miss and an entropy-zero fail
  report the right result.

### R2 — Lab panel

- Add the authored panel and reset button to the POC; wire the serialized refs; format and update.
- **Exit:** a validation-1 tester can read the goal, cost and result while shaping; reset clears the
  readout; console clean.

## Tests

- EditMode: readout for a solved run, a miss, an over-budget fail; grade buckets; command count.
- Manual POC: edit → ship flies → readout updates; reset clears; 10 planets + lattice still run.
- No UI automation; no final-art checks.

## Out of scope

- The final world-anchored HUD and result flow — [integration](space-shaping-integration.md).
- Window gauge, diegetic entropy, collapse, manual, TTS and haptics — [feel](space-shaping-feel.md).
- Map, persistence and stars.
- The validation protocol itself (jobs, testers, metrics).
