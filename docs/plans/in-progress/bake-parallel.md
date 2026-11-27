# Bake — scan in parallel

**Status:** now · puzzle creator · updated 2026-11-27 — one batch Unity scans many seeds at once on
every core, and the counts that follow the job list are derived, not typed. U1–U4 in; the timed scan
is still due in the bake checkout.

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

- `SectorWire.LineCount` is typed by hand (22); when the story's job count changes it silently shifts.
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

## State

- **U1** — the note above. Fleet probes are independent; everything that touches a `PuzzleData` or
  builds a variant stays on the main thread, and single-ship scans stay sequential.
- **U2** — `PuzzleSeedBatch` (+ `IPuzzleSeedSearch`, `PuzzleMultiShipSearch.PrepareForSeed`) runs
  up to `-Workers` fleet probes at once and confirms them in seed order; `PuzzleJobSeedScan.Scan`
  takes the worker count and keeps single-ship scans on the calling thread; a fleet bake's `Attempts`
  range (`PuzzleTestSetBuilder.FindFleetJob`) stops on the first pass. `bake.ps1` passes
  `-workers` (default cores − 1). `PuzzleSeedBatchTests` checks a two-seed batch against two
  sequential scans, the stop-on-first order and cancel-from-progress (3 tests, ~1 s, green);
  `PuzzleSeedBatchRealTests` runs two real seeds of 02_02 with 1 and 2 workers and compares the
  answer zone (87 s, bake-heavy suite, green).
  **Still due:** the timed ~24-seed two-ship scan, sequential vs parallel, in the bake checkout.
- **U3** — `SectorWireTests` reads the chapter seams from `Chapters/01.asset` and
  `Chapters/02.asset` and fails with the number to set when the job count moves; chapter 3 is still a
  named six-job term until `Chapters/03.asset` lands. `FleetJobsTests` takes its ids from
  `Chapters/02.asset` and its ship counts from `PuzzleJobContracts`.
- **U4** — AGENTS' bake section: one checkout, `-Workers`, and the second worktree kept as the
  fallback for two very long jobs.

## Review (2026-11-27)

The U1 note holds (checked: no static mutable state in the bake domain or the sim besides
`PlanetNames.instance`, which is main-thread only; the Burst code is presentation-only), and no
blockers. Review findings, and what landed:

- **Prove it on real seeds.** Done: `PuzzleSeedBatchRealTests` runs two real seeds of 02_02 through
  the batch with 2 workers and with 1, and matches `Seed`, `Found`, `Status`, `Kind` and the zone.
  Bake-heavy suite, 87 s, green.
- **The timing run** (U2 exit): still due — ~24 seeds of a two-ship job, sequential vs parallel, in
  the bake checkout; compare the two reports too.
- **Minor.** Done: `FleetJobsTests` skips (and names) a hand-authored job instead of throwing, and
  `ChapterTwo_LevelsAreInPlayOrder` guards the chapter's numeric play order. `PuzzleSeedBatch.Cancel`
  now backs the parallel scan's cancelable progress bar. (`FindFleetJob` shows no bar to cancel;
  every contract is `Attempts = 1` today.)

## Re-review (2026-11-27)

The fixes hold: `PuzzleSeedBatchRealTests` compares seed, found, status, kind and the zone between
one and two workers; `PuzzleSeedBatch.Cancel` stops the parallel scan from the progress bar;
`FleetJobsTests` notes hand-authored jobs instead of throwing and `ChapterTwo_LevelsAreInPlayOrder`
guards the order.

- Minor: `PuzzleSeedBatchRealTests` adds 87 s to the bake-heavy Creator suite, which AGENTS describes
  as tens of seconds. Mark it `[Explicit]`, like `PuzzleShipSweepTests`, and run it by name after a
  change to the batch or the probe.
- Still due: the timed ~24-seed scan in the bake checkout.
