# Bake — the answer is stored

**Status:** now · puzzle creator · updated 2026-11-27 — every baked job saves its answer; the shipped
check replays it instead of re-solving, and a bake no longer rewrites the chapter list. In progress:
U1–U4 implemented; the answer backfill (U3) still runs in the bake checkout.

**Bakes:** off the editor — U3 (the one-time backfill of the shipped answers).

**Related:** [plans board](../README.md) · [bake parallel](bake-parallel.md) ·
[bake construct core](../backlog/bake-construct-core.md) · [lessons 2026-11-20, seeds retune](../../lessons.md)

## Why

A shipped job stores neither its answer nor its kind, so `ShippedJobs_MeetTheContract` re-solves every
asset (slow, and the search space drifts with the settings: 02_06 broke silently). And every bake
rewrites `Chapters/<nn>.asset` through `PuzzleTestSetBuilder.WireChapterSlot`, which is why two bake
checkouts conflict and have to be split by job.

## Units

### U1 — the answer asset

- New `PuzzleAnswerAsset` (ScriptableObject, Creator data assembly): `id`, `kind`
  (`PuzzleSolutionKind`), `zones` (`PuzzleZone[]`), `seed`, and the bake's window / scale delta /
  start distance for the report. Saved at `Assets/_Game/Features/PuzzleCreator/Answers/<id>.asset`,
  outside `Resources`, so it never ships.
- `PuzzleTestSetBuilder.BuildJob` and the batch bake write it next to the job, from the
  `PuzzleSeedScanResult` they already have (`Solution`, `Kind`).
- Exit: a bake in the editor's Creator suite (one cheap chapter 1 job) writes both assets.

### U2 — the shipped check replays

- `ShippedJobs_MeetTheContract` loads the answer and validates with it
  (`PuzzleJobValidator.Validate(puzzle, answer.kind, answer.zones, …)`) instead of `TrySolve`. A job
  with no answer fails with `"<id>: no stored answer"`.
- Add a cheap replay check: the stored zones land every ship (the verifier the bake uses). If the
  settings change, every job whose answer no longer lands is named.
- With an answer stored, the scramble bounds can be checked too: drop `checkScramble: false` there.
- Exit: the shipped check runs in seconds for the stored jobs.

### U3 — backfill the shipped answers (bake checkout)

- An editor command (also callable in batch mode) re-solves each enabled shipped job once, as the test
  does today, and writes its answer. Jobs that do not solve are listed, not written.
  Code in: `PuzzleAnswerBackfill` and `docs/tools/bake.ps1 -Mode answers`.
- Run it in `bake`; commit the answers; merge back through git. **Remaining step.**
- Exit: every enabled job has an answer, except those listed as unsolvable (expected: none, since
  02_06 has no contract yet).

### U4 — bakes leave the chapter list alone

- Remove `WireChapterSlot` from the bake path; the chapter order is set by hand.
  `WireChapterLevels` stays only if the "test set" command still needs it.
- New fast check: every enabled contract's job is listed in its chapter asset, and every listed job has
  a contract or is marked authored by hand (chapter 1 tutorials, if any).
- Update the AGENTS bake section's "per job"
  notes: a job's commit is its asset, its `.meta`, its answer and its pin, never the chapter.
- Exit: two bakes on two checkouts merge with no conflict outside `PuzzleJobContracts.cs`.

## Review fixes (2026-11-27)

Applied; the U3 backfill still runs in the bake checkout. Until it runs,
`ShippedJobs_MeetTheContract` fails on every enabled job with "no stored answer".

- **Done — the backfill picks an answer that meets the contract.** `PuzzleJobSolver.TrySolveAnswer`
  walks a fleet's solved candidates (through the new `PuzzleFleetSolution.TryFind` filter) and keeps
  the first that passes `PuzzleJobValidator.Validate`, so the stored zone satisfies the scramble bounds
  (02_01/02_02, 02_09). Single-ship jobs have no scramble pins and validate the search's answer; a job
  is listed as failed only when no answer passes.
- **Done — the replay checks the solution type.** `PuzzleAnswerReplay.Lands` now takes the stored
  answer and fails when `PuzzleVerification.Kind` differs from `answer.kind`.
- **Done — the seed that baked is stored.** `PuzzleAnswerFactory.From` takes the seed;
  `PuzzleTestSetBuilder.SaveAnswer` passes `result.Seed`, the backfill passes `job.Seed`.
- **Done — minors:** `PuzzleJobChapterTests` reports an empty chapter slot instead of throwing, and
  `PuzzleBatchBake.BakeJob` checks the answer asset as well as the puzzle. A fleet is still simulated
  twice per shipped check (`CheckFleet` inside `Validate`, then the replay); that is one extra run and
  the check stays at seconds.

## Re-review (2026-11-27)

Applied. The fixed path is covered by `PuzzleJobAnswerTests`, which solves 02_01/02_02 and
01_01–01_03 and asserts each answer is its frame.

- **Done — a fixed job's answer sits on its frame.** A fixed fleet searches only
  `frames[0].centre`/`size` (`PuzzleFleetSolution.TryFind`); a fixed single-ship job searches the
  scales at its frame (`PuzzleSolutionSearch.TryFindAtFrame`). `PuzzleJobValidator.CheckFixedAnswer`
  fails any fixed answer whose centre or size differs from `frames[0]`.
- **Done — the solver returns its problems.** `PuzzleJobSolver.TrySolveAnswer` returns an
  `AnswerAttempt` carrying the validator's failures, and the backfill prints them instead of
  "no solution found".
- **Done — the fixed frame is checked once.** `TrySolveFleet` runs `CheckFixedFrame` and
  `CheckSunAnswer` once before the walk and validates candidates with the sim checks off; a movable
  fleet keeps full per-candidate validation.

Verified fine: the replay's kind check, the baked seed stored from `result.Seed`, the batch bake
checking the answer asset, and `PuzzleJobChapterTests` reporting empty slots.
