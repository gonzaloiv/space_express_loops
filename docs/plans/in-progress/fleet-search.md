# Fleet search

**Status:** now · space shaping · updated 2026-11-26 — run the multi-ship seed search on a cancellable
background task, used by both the creator window and the Job Baker, so a fleet seed hunt never
freezes the editor.

**Related:** [plans board](../README.md) · [fleet rebake](fleet-rebake.md) (uses it) ·
[release](../../design/release.md)

## Goal

A fleet seed search that keeps the editor responsive, shows progress and cancels within a frame or
two. Split out of the fleet rebake (2026-11-25 review): each two-ship seed costs 60–90 s, and a
50-minute run through the editor bridge froze the editor.

## Notes

- **The probe path is thread-clean:** no `Debug`, `Time`, `Random` or Unity objects in
  `PuzzleShipScan`, `SpaceShapingSetup`, `SpaceShapingSession` or the sim. Only the inputs need
  copying.
- **The snapshot is wider than sources.** The probe also reads `PuzzleZoneCandidates.Centres`,
  `Sizes` and `CoversSource`, and `SpaceShapingSetup.Build` needs the ships (for `MaxSteps` via
  `ShipSpecs.From`), `captureRadius` and `entropyBudget`. Leaving out the ships changes the step
  budget and the result will not match today's bake.

## Units

### U1 — the snapshot and the seam

- A plain `FleetField` copied at bake start: `PuzzleSource[]`, ships, capture radius, entropy budget,
  cube range, origin/destination, settings, and whatever `PuzzleZoneCandidates` reads. The copy and
  `PuzzleFieldBake.Apply` stay on the main thread (`PuzzleData` is a `ScriptableObject`).
- **Every reader takes the snapshot:** `PuzzleMultiShipProbe`, `PuzzleShipScan`,
  `PuzzleZoneCandidates` and **`PuzzleSunShortcut`** (it takes a `PuzzleData` and is built whenever
  `SunRule == NoSunAnswer`, which is every job this search exists for).
- **One setup path.** `SpaceShapingSetup.Build` is the only place that resolves the capture radius and
  the `MaxSteps` budget. Add a plain-value `Build` overload in the runtime assembly and have the
  `PuzzleData` overload delegate to it; do not rebuild the formula in the creator assembly.
- Split `PuzzleMultiShipBake` at a public seam: the task returns zone + rescued runs; the main thread
  calls a public "confirm this zone" entry (Select → `Rescues` → `StartFrameClear` →
  `Variant`/`Verify`). `BakeField`, `Select` and `Confirm` are private today. The seam hands back (or
  rebuilds) the `PuzzleShipScan` the confirm step needs for `Rescues`.
- Exit: a test in `Creator.Tests.EditMode` (the fast suite cannot see the Editor-only probe) shows the
  snapshot path finds the same zone as the synchronous bake on a tiny synthetic field, in seconds and
  not `[Explicit]`.

### U2 — the task

- `PuzzleMultiShipProbe.Find` runs on a `Task` over the `FleetField`, never on the editor thread and
  never through the bridge.
- Cancellation checked **per zone** (between candidate centres is ≤ 8 steps of ~312 zones each, so it
  can still hang for seconds). Progress through a thread-safe counter.
- A named driver (e.g. `FleetSearchRun`) with `Start` / `Cancel` / a completion result, and a
  **process-wide** gate: one search at a time across both windows, not per window. Exceptions and
  failures are marshalled back to the main thread as a result.
- `EditorApplication.update -= Poll` on complete, cancel, `OnDisable` and domain reload (a recompile
  orphans the task, and the window's fields do not survive it); cancel the token on reload.
- Exit: a search starts, shows per-seed progress, cancels cleanly, and survives a recompile without
  a stuck poll.

### U3 — both windows use it

- **Job Baker:** `PuzzleJobSeedScan.Scan` (up to 1000 seeds, blocking, a modal progress bar inside
  the loop) becomes two-phase for fleet contracts: start the driver, poll it, and store the entries in
  `PuzzleJobBakeWindow` on completion. This is the window the rebake actually uses.
- **Measuring stays on the main thread.** After each *baked* seed, `PuzzleJobSeedScan.Measure` runs
  the validator (including `SunAnswers`, 25× `SimulateAll`) and `PuzzleSolveWindow.Measure`
  (200× `SimulateAll`). That is a short hitch per baked seed, not a freeze, and is accepted; move it
  into the task only if it hurts in use.
- **Creator window:** a new button for the fleet search; `FindSeed`'s `IsMulti` branch
  (`MultiSeedAttempts = 12`) is rerouted to the task or removed, so the synchronous button stays
  single-ship only.
- Exit: neither window can start a blocking multi-ship search; the editor stays responsive through a
  multi-seed scan in the Job Baker.

## Manual check (editor)

1. Job Baker: scan a fleet contract; the editor stays responsive and progress moves.
2. Cancel mid-seed; it stops within a second or two.
3. Recompile during a scan; no errors, no stuck progress, a new scan starts.
4. Creator window: the single-ship "Find working seed" still works; multi goes through the task.
