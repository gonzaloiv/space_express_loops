# Chapter 3 — fleet bodies

**Status:** next · space shaping · updated 2026-11-27 — anchors and repulsors in a fleet, for the
capstone only: a two-pass fleet bake whose second pass scores the first pass's ships. Last of the
four broken-body plans, after [anchors](chapter-3-anchors.md) and [repulsors](chapter-3-repulsors.md);
it can slip without blocking the chapter's single-ship jobs.

**Bakes:** off the editor — the exit's Creator test and the capstone scan
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapter 3](chapter-3-jobs.md) ·
[pinch refusal](chapter-3-pinch-refusal.md)

## Why two passes

A fleet's ships are chosen after the field is built, but anchors sit on the chosen runs and repulsors
beside them. So the first pass probes the pristine field and picks the ships; the bodies are then
marked for those runs, and the second pass checks that one zone still rescues **those** ships.

## Units

### U1 — the two-pass bake

- `PuzzleMultiShipSearch.Run` probes without broken bodies, then selects the ships with a public
  `PuzzleShipRescueExtensions.SelectShips(rescued, fleet, brief)` (lift the private
  `PuzzleMultiShipBake.Select` to that extension so both callers share it).
- Mark anchors on those runs and repulsors off them through `PuzzleBrokenBodyChoice` on a
  `FleetField.WithBodies(...)` copy (copy `Pushes` in the `FleetField` clone; add it to the
  `FleetFieldTests` equality).
- **The second pass scores only the chosen ships.** Give `PuzzleMultiShipProbe` an optional ship list:
  `PuzzleShipScan` simulates only those ships' origin→destination pairs, and a zone passes when it
  rescues all of them (plateau rule unchanged). This keeps the first pass's choice and simulates a
  handful of pairs instead of every ordered pair. If no zone rescues them, the seed fails.
- Return the second pass's `FleetZoneResult` with the ships on it: add `Ships` to the result (the
  plain probe leaves it empty), and **add** a branch to `PuzzleMultiShipBake.ConfirmZone` that uses
  `found.Ships` when set and selects only when it is empty (today it always selects). `Confirm`
  applies the bodies to the field and builds the variant with those ships. Keep both passes on the
  bake worker; no extra main-thread search.
- Exit: a Creator test bakes an anchored fleet through the two-pass path (small sample); a fast test
  shows the second pass rejects a zone that rescues other pairs but not the chosen ships.

### U2 — repulsors in the fleet search

- Apply the [repulsors](chapter-3-repulsors.md) `RequiresPushes` tie to `PuzzleMultiShipProbe` and
  `PuzzleFleetSolution`, so the fleet bake and the shipped fleet re-solve cannot bypass it (`Any`
  stays the exception; the capstone asks `Any`).
- Exit: the capstone contract scans in the bake checkout with both bodies and reports passing seeds;
  `ShippedJobs_MeetTheContract` green.

## Manual check (headset)

1. The fleet capstone reads both bodies, and its two-pass field still solves.
2. Every ship in the capstone is rescued by the shipped answer.
3. Frame rate holds with both bodies and three ships.
