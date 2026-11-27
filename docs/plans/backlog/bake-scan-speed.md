# Bake — scan speed

**Status:** next · space shaping · updated 2026-11-27 — cut the scan's wall clock before chapter 3 and
the freelance bakes: measure first, then neutral fleet-probe cuts, a bounded pair filter and a leaner
scan window. No shipped job, answer or contract changes.

**Bakes:** off the editor — U1 and every exit run in the bake checkout
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [lessons](../../lessons.md) ·
[one bake path](../../solutions/bake/one-bake-path.md) (the search is the only bake path) ·
[freelance jobs](freelance-jobs.md) (the scans this serves) ·
[chapter 2 rebakes](../in-progress/chapter-2-rebakes.md) (the 200-seed runs that took hours)

## Goal

A seed scan finishes sooner for the same pass set. The search is the only bake path now
([one bake path](../../solutions/bake/one-bake-path.md)), so its cost is paid on every job: about
**2 minutes a two-ship seed on one core** ([freelance jobs](freelance-jobs.md)) and about **1 second a
failing single-ship seed**. A 200-seed fleet hunt is hours; freelance needs ~2 attempts a slot across
20 slots.

## Decisions

- **Measure before cutting.** No cut lands without a before/after wall clock on a fixed seed range, and
  the gate is that range's own output, not a feeling about the code.
- **Same answers, same passes.** A cut lands only when the fixed-seed comparison shows the same passing
  seeds, the same status and, for a pass, the same zone and rescued list. A cut that changes one seed
  is rejected, not retuned.
- **Neutral means provable.** Prune only work that cannot change the result: cached counts, the winner's
  list, a shared context. A heuristic bound is allowed to fail its measurement; it does not ship while
  it can change a seed.
- **Shipped assets are untouchable.** Contracts, chapter order, answers and saved jobs do not change.
  A rebake is out of scope; if a cut would change a shipped bake, it is rejected, not rebaked.
- **Ordering stays.** Largest size first, cheapest entropy, widest window. Speed cuts remove work the
  search would have repeated, not reorder its choice.
- **The fleet probe dominates.** It tries up to 8 centres × 3 sizes × 13 scales and simulates every
  ordered pair per zone that is not already arriving. The fix is fewer simulations and fewer lists,
  not fewer zones.
- **The gate is a re-bake, not a replay.** `ShippedJobs_MeetTheContract` replays stored answers and
  cannot see a search change; `JobSet_MeetsTheContract` (`[Explicit]`) and the fixed-seed equivalence
  check are the gate.

## Product contract

- A fleet seed's probe returns the same `FleetZoneResult` (`Found`, zone, rescued list) for that seed
  on the reference ranges and the fixed equivalence fixture.
- Cancellation and release behave as the fleet run does today: a recompile leaves no locked gate and
  no live worker. Any new per-pair or per-zone loop checks the token.
- The scan report gains per-phase timings, and a flagged debug line carries the chosen zone and
  rescued count for the equivalence fixture; the default scan output is otherwise unchanged.
- The pinned bake still measures the window at 200 samples.

## Units

### U1 — measure

- Add timings around the phases a scan already runs: planets/apply (including
  `PuzzleRunChoice.TryPick`), the `PuzzleShipScan` start cache, `Select` and its second pass,
  `HasShorter`, `Check`, `Confirm` (verify, sun, routes) and `PuzzleSolveWindow.Measure`. Keep the hook
  editor-side and optional, so the pinned bake and the Creator tests do not pay it. Report per seed and
  totals in the scan report and the `bake.ps1` log.
- Baseline in the bake checkout on fixed ranges: `01_04` (single, 64 seeds), `02_03` (two ships, 20)
  and `02_06` (three ships, 20). Record wall clock with `-Workers 1` and the default for the fleet
  ranges (`01_04` is single-core today, so it has one number), passing seeds, status histogram and the
  chosen entry's window.
- Record the equivalence fingerprint on a small fixed subset (the first 3 seeds): the chosen zone
  (centre, size, scale) and the full rescued list per seed. Dump it behind a flag, not in the default
  report.
- Fold in the board's *Parallel scan timing* tail: the two worker settings answer it, and AGENTS' bake
  section gets the measured speed-up.
- Exit: the plan carries the baseline table and the fixture fingerprints; every later unit compares
  against them.

### U2 — neutral probe cuts

- One count pass per zone. Compute `Distinct` once and stop early at `max(Ships, best.Count + 1)`, so a
  zone that cannot beat the current best stops enumerating its pairs. The comparison stays the exact
  count (`distinct <= best.Count` today, `PuzzleMultiShipProbe.cs:64`); stopping at `Ships` would make
  every qualifying zone tie and keep the first one.
- Materialise rescue lists only for the winner. `Row` keeps counts, and the full
  `List<PuzzleShipRescue>` is built only when a zone becomes the new best — a handful per seed instead
  of 8 × 3 × 13. `Confirm` still receives the full list for the chosen zone.
- Check the token between pairs and zones, so a cancel does not leave a live worker inside the new
  loops.
- Exit: on the fixed subset and the `02_03` / `02_06` ranges, the same passing seeds, the same zone and
  rescued list per seed, and a lower wall clock. `JobSet_MeetsTheContract` green.

### U3 — bound the pair loop (spike, may be rejected)

- Skip an ordered pair before simulating only when a conservative bound says the zone cannot change
  its outcome: the chord's closest approach to every source the zone scales, against the capture
  margin. Measure the bound's distribution on the 20-seed ranges first; if it cannot clear most pairs
  without coming near real rescues, reject the cut and record the ceiling.
- The bound must be conservative in the direction that keeps every arriving pair. Dropping a rescued
  run changes `Select`'s ship set, not just a count.
- Exit: if kept, the `02_03` / `02_06` ranges return the same passing seeds, zone and rescued list per
  seed with a lower wall clock. If rejected, the plan records the measured ceiling and the fleet cost
  is what U2 left.

### U4 — one context and a cheaper scan window

- Share one `PuzzleSolutionContext` across `Select`'s two searches and across `HasShorter`'s widths.
  The setup, obstacles and start simulation are mechanism-independent; swap the requested mechanism on
  a copy instead of rebuilding them three or more times per seed. Do not attempt the one-pass role
  pick: the mechanism filter changes the centre choice (single zone) and the beam trim order
  (multi-zone), so it is not the same answer.
- In a scan, measure the window at 50 samples for every found seed, then re-measure the top 8 at 200
  and choose among the re-measured. `ByUsefulness` ranks by window, so a coarse sort alone would change
  the recommended seed. The pinned bake still measures 200.
- Exit: for every shipped contract the scan reports the same chosen seed, window to the third decimal
  and status as the baseline; the reference ranges spend less on the second search and the window.
  `ShippedJobs_MeetTheContract` green in the bake checkout.

## Verification

- Fast suite: `DigitalLove.Game.SpaceShaping.Tests.EditMode`.
- Creator suite: `DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode`, plus
  `PuzzleJobBakeTests.JobSet_MeetsTheContract` by name (it is `[Explicit]`, re-bakes every enabled
  contract), and `PuzzleShipSweep` / `PuzzleShipPlateau` if the probe changed.
- Equivalence: fingerprint the fixed subset before U2 and U3 and diff after; any difference rejects the
  cut.
- Bake checkout: `bake.ps1` on the three reference jobs, then `ShippedJobs_MeetTheContract`.
- `docs/check.ps1` after the board and this file change.

## Open

- **Singles on every core.** Deferred to its own plan: single scans are ~1 s a seed, and the fix is a
  snapshot abstraction across `PuzzleSolutionSearch`, `PuzzleBeamSearch` and `PuzzleSolutionContext`
  (`FleetField` has no endpoints or frames), plus a `PuzzleSeedBatch` result type that is not
  `FleetZoneResult`. Decide after U1 shows the serial share of a single seed.
- **Fleet pass rate vs probe count.** Two ships pass often, three ships rarely
  ([lessons](../../lessons.md)). If U2 and U3 land, a wider pairing (e.g. 4 centres) may find the rare
  three-ship seed sooner; decide with the baseline, not now.
