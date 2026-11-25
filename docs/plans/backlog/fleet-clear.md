# Fleet Clear

**Status:** next · space shaping · updated 2026-11-26 — let a fleet bake ask for Clear, then three
two-ship jobs where one obstacle blocks more than one ship.

**Bakes:** off the editor — U3 (the three Clear jobs) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapter 2 intro](chapter-2-intro.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

Three chapter 2 jobs as a ramp: one obvious shared blocker → a blocker among decoys → two blockers.
Shrinking the blocker frees every ship.

## Depends on

- [Chapter 2 intro](chapter-2-intro.md): the scramble bounds, the opened fleet tests.
- [Fleet rebake](../in-progress/fleet-rebake.md) U2: the chapter-aware slot wiring that places a job
  after 2-4.
- Ids **02_06–02_08** ([chapter 2 intro](chapter-2-intro.md) §Ids). Play order follows
  [chapters](../../design/chapters.md): after 2-5 (02_05); if the capstone slips, after 2-4.

## Decisions (2026-11-26)

- **Two ships only.** The one shipped three-ship field, 02_04, no longer solves under today's physics
  (no candidate zone rescues all three), and a fresh three-ship re-solve is still unproven
  ([fleet capstone](fleet-capstone.md) U1). Every Clear job is two ships and stays in the shipped
  check. No headset-only exemptions.
- **The decoy job is judged by playing.** No contract field says "decoy"; the validator checks the
  shared blocker, the headset check judges the decoys.

## Units

### U1 — a fleet can ask for Clear

- `PuzzleSeedScanner.Find` and `PuzzleMultiShipSearch.Prepare` reject any mechanism but `Any` for a
  fleet: accept `Clear`.
- `PuzzleMultiShipBake` classifies the confirmed solution (`PuzzleSolutionClassifier`) and rejects a
  wrong role with a **new** `PuzzleBakeStatus.WrongRole`, appended at the end of the enum (not
  `WrongRoleRoute`, which is the single-ship "cheapest route has the wrong role"). Add its message to
  `PuzzleCreatorWindow`'s status text; the Job Baker's summary already prints enum names.
- `PuzzleJobValidator`: check the role for a fleet only when
  `contract.Ships > 1 && contract.Mechanism != PuzzleMechanism.Any`. `Matches(Any, kind)` means
  "not `Other`", so dropping today's `Ships <= 1` guard alone breaks
  `PuzzleFleetValidationTests.Validate_TwoShipFleet_Passes` (it passes `Other` under `Any`).
- `PuzzleFleetSolution.TryFind` takes the contract's mechanism and returns the **first** solved zone
  whose kind matches (`Any` = today's behaviour), so a Clear job that also admits a Curve zone does not
  fail its own shipped check. First, not cheapest: keeping the early exit keeps
  `ShippedJobs_MeetTheContract` from verifying every candidate. Pass `contract.Mechanism` from both
  callers (`PuzzleFleetValidationTests`, `PuzzleJobBakeTests`).
- Exit: a Creator test bakes a two-ship Clear field; a Curve answer on a Clear brief comes back
  `WrongRole`; the fleet validation tests stay green.

### U2 — the start and the shared blocker, checked

- **The start does not solve in place, asserted.** Route movable fleets' `PuzzleMultiShipBake.Frames`
  through `PuzzleStartFrame.Place`: `Variant` and `Frames` take the sim settings (they do not today),
  and `Place` is called with `PuzzleFieldBake.StartSpot(puzzle, brief.StartRule)` as the preferred
  spot, not `zoneCentre` (which only matches `RunMidpoint`; a `FieldCentre` contract would bake a
  frame the validator rejects). Drop `StartFrameClear` from `ConfirmZone` for movable fleets (it would
  contradict `Place`). `Place` falls back to the preferred spot when nothing is accepted, so add an
  explicit "start solves in place" failure to `CheckStartFrame` instead of trusting the fallback; it
  runs only with settings.
- **Shared blockers, for Clear fleets only.** `BlockerCount` measures ship 0's run (the variant pins
  `origin`/`destination` to ship 0). Keep that count as it is for single-ship jobs (`puzzle.ships` is
  null on 01_x; `Validate_TooFewBlockers_Fails` expects it) and for `Any` fleets (02_01–02_03 pin
  `MinBlockers` against it in the [fleet rebake](../in-progress/fleet-rebake.md)). Only when
  `Ships > 1 && Mechanism == Clear`: count blockers over every stored ship and require at least one on
  two or more ships' runs (the lesson itself); `MinBlockers = 2` then means two shared blockers.
- Exit: a seconds-long test in `PuzzleCreator/Tests/EditMode` (next to `PuzzleJobValidatorTests`, not
  `[Explicit]`; the validator is Editor-only, so the fast suite cannot reach it), with sim settings
  passed: a start that solves in place fails the validator; a blocker on both runs counts as shared;
  `Validate_TooFewBlockers_Fails` stays green.

### U3 — the three jobs

- Contracts as [chapter 2 intro](chapter-2-intro.md) U2, movable (`Translatable = true`,
  `MinRoutes = 1`), `Ships = 2`, `Mechanism = Clear`, `MinBlockers` 1, 1, 2; scramble bounds by playing (`MaxScaleDelta` on the first job, the minimums on the third).
- **Budget the scans.** Clear is filtered after a role-agnostic search, and the classifier returns
  `Mixed` when any source grows, so expect many `WrongRole` seeds: scan ~200 seeds per job off the
  editor. If a job has no Clear bake after ~400 seeds, ship the ones that baked and record the gap
  here.
- Exit: every Clear job that baked validates in `ShippedJobs_MeetTheContract` (three, or fewer with
  the dropped lesson recorded here); none solves in place; they sit after 02_05 (or 02_04) in
  `Chapters/02.asset`; `FleetJobsTests`' ids, ship counts and chapter count match what shipped.

## Manual check (headset)

1. The shared blocker reads before the first launch.
2. Shrinking it frees both ships; growing a planet alone does not.
3. The decoy job and the two-blocker job each add one step.
4. Time each; target ~3 min.
