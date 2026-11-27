# Bake construct — wire it in and compare

**Status:** now · puzzle creator · updated 2026-11-27 — the `Generator` seam is in: a contract can ask
for the constructor, the scan and builder dispatch to it, retries re-seed, a Construct fleet is
unsupported, and the scratch comparison contracts write outside Resources; the timing comparison is
next.

**Bakes:** off the editor — the timing comparison.

**Related:** [plans board](../README.md) · [construct single](../in-progress/bake-construct-single.md)
(must land first) · [construct separate](bake-construct-separate.md) (in progress)

## Units

### U1 — the `Generator` seam

- **In.** `PuzzleJobContract.Generator` (`Search` default, `Construct`).
- **In.** `PuzzleSeedScanner.Find(job, seed, settings, attempts, lesson = null)` builds the request
  (`Contract`, `Brief`, `Seed`, `Field`, `Sim`, `Lesson`), dispatches on the generator, and maps the
  constructor's `PuzzleBakeResult` back to a `PuzzleSeedScanResult`. The lesson comes from
  `PuzzleLessonChoice.For(brief)` (Clear/Curve; null for Any/Mixed); an explicit lesson overrides it
  for tests. `PuzzleJobSeedScan.Probe` and `PuzzleTestSetBuilder.FindJob` pass the job.
- **In.** `Attempts` means construct retries at the same seed. `PuzzleConstructBake.Bake` re-builds
  with `request.WithAttempt(i)` for each attempt and keeps one local decoy move inside the attempt;
  the scan passes `job.Attempts`, while the search path keeps its adjacent-seed loop.
- **In.** A Construct fleet reports `UnsupportedMechanism`: `ScanResults` stays sequential for
  Construct, `ScanFleetAsync` returns unsupported entries, and the fleet path never falls through to
  the search. The fleet branch is [separate](bake-construct-separate.md).
- **In.** The scan reads the constructor's measured window and contract result (`Window`,
  `ContractChecked`, `Problems` on `PuzzleSeedScanResult`) instead of validating and measuring a second
  time.
- **In — miss status.** A `PuzzleConstruct.Build` that returns null reports `NoRun`; a field that
  builds but has no answer reports `NoSolution`, so the scan keeps the two failures distinct.
- **In — scratch contracts.** `01_01C`/`01_03C` live in `PuzzleConstructScratch` (`Pending`,
  `Construct`, 4 retries). `PuzzleJobContracts.ById`/`Has` see them; `Enabled`, the chapter, the job
  window and the shipped checks skip them. Their field and answer write under
  `Assets/_Game/Features/PuzzleCreator/Scratch`, not Resources or `Answers/`, and `bake.ps1` is
  unchanged.
- Exit: `bake.ps1 -Job 01_01C -Mode scan` times the constructor and `-Mode bake` writes the scratch
  field and answer. The constructed field uses the same variant step, export and answer asset as a
  searched one.

### U2 — search vs construct

- Time and yield for `01_03` and `01_01`, search vs construct, in the bake checkout; write both here.
  Time the scan's wall clock; the construct now reports its own window and validator result, so the
  scan does not charge it twice.
- `01_03`'s construct yield depends on [single](../in-progress/bake-construct-single.md)'s Clear recipe
  producing a field with 2 routes; the shipped `01_03` is `SingleRoute` under the current count. The
  single plan records Clear 12/20 (3 `SingleRoute`, 4 `NoRun`, 1 `Unverified`) and Curve 11/20, so the
  comparison can run.
- Exit: the numbers decide whether chapter 3 and the freelance jobs default to `Construct`. The shipped
  `01_03` and `01_01` stay as they are: this is a comparison, not a rebake.

## State

- **U1 seam in.** `PuzzleGenerator`, `PuzzleJobContract.Generator`, `PuzzleSeedScanner.Find(job, ...)`,
  retries in `PuzzleConstructBake.Bake`, fleet `UnsupportedMechanism`, the scan's single measurement,
  and the `PuzzleConstructScratch` contracts. `PuzzleConstructTests` + `PuzzleSeedScannerTests`: 14/14
  green (four new: lesson seam, fleet unsupported, per-attempt re-seed, scratch reachability).
- **Clear is unblocked.** [single](../in-progress/bake-construct-single.md) records Clear 12/20 on
  `01_03` and Curve 11/20 on `01_01`; the 3 `SingleRoute`, 4 `NoRun`, 1 `Unverified` split is the
  construct's current yield.
- **Next:** U2 — the bake checkout only sees committed code; once this is merged into `bake`, run
  `bake.ps1 -Job 01_01C` / `01_03C` and write the numbers here.

## Review (2026-11-27)

- **Fixed — lesson.** `PuzzleLessonChoice.For(brief)` selects the Clear/Curve recipe; an explicit
  lesson still overrides it. Any/Mixed have no recipe yet, so they report `NoSolution`.
- **Fixed — attempts.** `PuzzleConstructBake.Bake` re-builds per attempt (`WithAttempt`) instead of
  moving a decoy on one field; `Probe` passes `job.Attempts`.
- **Fixed — fleet.** A Construct fleet reports `UnsupportedMechanism` in both scan paths instead of
  searching; the window shows the entries.
- **Fixed — double measurement.** `PuzzleSeedScanResult` carries `Window`, `ContractChecked` and
  `Problems`; `EntryFor` uses them for construct results.
- **Fixed — contract field premise.** The `Find(job, ...)` overload exists for `request.Contract`, not
  for `MinBlockers`/the scramble bounds (those stay validator-only).
- **Fixed — status.** A null `Build` is `NoRun`; a built field with no answer is `NoSolution`.
- **Fixed — comparison copies.** `PuzzleConstructScratch` holds the two pending contracts; a scratch
  job's field and answer write under `PuzzleCreator/Scratch`, and `bake.ps1` needs no switch.

## Review — scratch folders (2026-11-27)

- **Fixed — scratch detection.** `PuzzleTestSetBuilder` keys the scratch folder on
  `PuzzleConstructScratch.Has(job.Id)`, not `Pending`, so a parked `Pending` job (02_08) keeps its
  chapter path.
- **Fixed — scratch answers.** The constructed field and its answer both write under
  `PuzzleConstructScratch.Folder` (`Scratch/`), and `PuzzleBatchBake.BakeJob` reads the answer there;
  `Answers/` stays one file per shipped job.
- **Fixed — state and link.** The links point at [single](../in-progress/bake-construct-single.md),
  which records 12/20 Clear and 11/20 Curve; U2 is next in the bake checkout.
