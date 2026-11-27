# Bake — scan in parallel

**Status:** now · puzzle creator · updated 2026-11-27 — one batch Unity scans many seeds at once on
every core, and the counts that follow the job list are derived, not typed. Code in; the timed scan is
still due in the bake checkout.

**Bakes:** off the editor — U2's timing run.

**Related:** [plans board](../README.md) · [bake answer](bake-answer.md) ·
[fleet search](../../../Assets/_Modules/Docs~/solutions/architecture-patterns/editor-bakes-on-a-plain-snapshot.md)

## Why

`PuzzleSeedScanner.Scan` probes one seed after another, so a batch bake uses about one core. A second
worktree is the workaround, and it costs a `Library` copy, a merge and a third Unity's memory. The
fleet search already runs on a plain snapshot (`PuzzleMultiShipSearch.Prepare` → `Run` on a Task), so
the expensive part can run side by side.

## Units

### U1 — find what can run off the main thread

- Before writing code: check whether `PuzzleMultiShipSearch.Run` and the single-ship search touch
  anything thread-bound (Unity objects, Burst job scheduling in the sim, static caches). The fleet
  search lesson says the probe already runs on a Task; confirm two can run at once (no shared mutable
  state).
- Single-ship bakes (`PuzzleFieldBake.Bake`) take a `PuzzlePlanetListAsset`: if their search is not
  snapshot-based, leave them sequential (chapter 1-style jobs are fast) and parallelise fleets only.
- Exit: a short note in this plan: what runs in parallel, what stays on the main thread.

**What runs in parallel.** Only `PuzzleMultiShipSearch.Run`: it builds a `PuzzleMultiShipProbe` over
the `FleetField` plain copies and searches zones in `PuzzleShipScan`. That path touches no Unity
objects, no Burst/Jobs (the lattice jobs are presentation-only) and no static mutable state — every
cache (`PuzzleSunShortcut.bySize`, the scan's start outcomes) is per search. The sim is instance
state (`ShipLaunch`, `TrajectoryIntegrator`, `SpaceShapingSession`), so two or more probes run at once.

**What stays on the main thread.** `PuzzleArtifactFactory.PlanetsFor` and
`PuzzleMultiShipSearch.Prepare` (ScriptableObject field, `PuzzleFieldBake.Apply`), `Confirm`
(`PuzzleMultiShipBake.ConfirmZone` builds the variant), and every single-ship bake: `PuzzleFieldBake.Bake`
searches through `PuzzleData`/`PuzzleSolutionSelection` assets, not a snapshot, so those stay
sequential (chapter 1-style jobs are fast).

### U2 — a parallel scan

- `PuzzleSeedScanner` (or a new `PuzzleSeedBatch`): prepare N seeds' snapshots on the main thread,
  run their searches on up to `Environment.ProcessorCount - 1` tasks, confirm and report on the main
  thread, in seed order so the report reads the same as today. Cancel the remaining seeds as soon as
  the first passing seed is found when scanning for a bake; keep going in `-Mode scan`.
- `docs/tools/bake.ps1` gets `-Workers <n>` (default: cores − 1).
- Exit: a fast test that a two-seed batch returns the same results as two sequential scans; a timed
  scan of ~24 seeds of a two-ship job in the bake checkout, sequential vs parallel, written here.

### U3 — counts follow the job list

- `SectorWire.LineCount` is typed by hand (23); when the story's job count changes it silently shifts.
  Replace the hand-kept count with a test that compares it to the chapters' total job count + 1, so a
  change fails loudly in the fast suite with the number to set.
- `FleetJobsTests`: read ids and ship counts from `Chapters/02.asset` and the contracts, not a list in
  the test.
- Exit: dropping or adding a chapter job is the chapter asset + contract + lines; the fast suite says
  exactly what else to touch.

### U4 — one bake checkout

- Once U2 holds, retire the second worktree in AGENTS (keep the instructions as the fallback for two
  very long jobs).
- Exit: AGENTS' bake section says one checkout, `-Workers`, and when to still use a second.
