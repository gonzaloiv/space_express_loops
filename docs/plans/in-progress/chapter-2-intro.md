# Chapter 2 intro

**Status:** now · space shaping · updated 2026-11-25 — U1–U3 are in and 02_09/02_10 are baked
(`0b66c195`, `e890d40b`); the fast fleet tests are green. Left: the headset pass and the
`MaxScaleDelta = 0.5` scramble decision.

**Bakes:** off the editor — U2 (scan and bake 02_09, 02_10) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release) ·
[fleet rebake](../in-progress/fleet-rebake.md) ·
[fleet search](../../../Assets/_Modules/Docs~/solutions/architecture-patterns/editor-bakes-on-a-plain-snapshot.md)

## Goal

Chapter 2 opens with two jobs where one pinch on a fixed cube moves two ships: fan-out (the cube on a
planet next to the shared origin), then separate (the cube on a planet between the runs). The first
of four chapter 2 plans; the others are [fleet Clear](fleet-clear.md),
[fleet capstone](fleet-capstone.md) and [chapter 2 lines](chapter-2-lines.md).

## Depends on

[Fleet rebake](../in-progress/fleet-rebake.md) (fleet contracts, chapter-aware tooling, no sun
answer) and [fleet search](../../../Assets/_Modules/Docs~/solutions/architecture-patterns/editor-bakes-on-a-plain-snapshot.md)
(the off-thread scan).

## Ids (decided 2026-11-26)

Ids are labels; play order comes from `Chapters/02.asset`. The names keep their ids: **2-5 capstone =
02_05**, **Clear = 02_06–02_08**, and the two intros take **02_09** and **02_10**. Everything derived
from the id must follow play order instead: the card's manual version is [story arc](../../solutions/narrative/story-arc.md)
(done). `RoutePalette.StartIndex` (which palette colour a job's routes start on) stays on the id: it is
cosmetic.

## Units

**Progress (2026-11-25):** U1 and U3 are in (scramble bounds in `0b66c195`; `FixedFleetFrameTests`
green). U2 is baked in the second checkout (`our_space_bake2`, branch `bake2`, merged as `e890d40b`):
02_09 seed 7 (window 0.317) and 02_10 seed 5 (window 0.161), both `MaxScaleDelta = 0.5f`. That bound
admits the widest baked answer, so it screens nothing today; tighten it to ~0.3 and rescan if the
intro plays hard. The chapter lists 02_09, 02_10 before 02_01, and `FleetJobsTests` is 3/3 green.
`ShippedJobs_MeetTheContract` passes in `bake2`. Cards and Jim's lines are written in `Levels_en` and
the key table (Twin Moons Catering, Two Rival Bakeries); the spoken lines stay silent until their clips
are generated. Remaining: clips, the headset pass, and the 0.5 scramble decision. The first checkout
(`bake`) is untouched.

### U1 — a fixed fleet can bake

- `PuzzleMultiShipBake.ConfirmZone` ran `StartFrameClear` (the *movable* bounding-box frame) for every
  fleet. A fixed fleet ships the zone itself as the frame, and for a fan-out pair the bbox centre can
  sit on the shared origin, so every zone came back `StartCoversEndpoint`. **Superseded by
  [fleet Clear](fleet-clear.md) U2:** movable fleets now place their frame through
  `PuzzleStartFrame.Place` and `Confirm` no longer calls `StartFrameClear` for any fleet; fixed fleets
  are checked by `PuzzleVerifier.Fits` → `ContainsEndpoint`. `StartFrameClear` and its test
  `StartFrameClear_FanOutOnTheSharedOrigin_IsKeptForAFixedFleet` have no other caller: delete both.
- **Scramble bounds.** There is no scramble dial. Add to `PuzzleJobContract` three fields, each
  **`0 = off`** (the contract is a zero-defaulted struct, so existing contracts and the tests'
  object-initialised contracts keep passing): `MaxScaleDelta` (low scramble: the solution's
  `|scale − 1|` at most this), `MinScaleDelta` and `MinStartDistance` (high scramble: at least this, the
  second the distance from `frames[0]` to the solution zone on a movable job). `PuzzleJobValidator`
  checks them cheaply, with no simulation (not the 200-run `PuzzleSolveWindow`). Values set by
  playing; the other chapter 2 plans reuse them.
- **Bounds are bake-time checks.** They describe the baked answer, but `ShippedJobs_MeetTheContract`
  re-solves the asset (it stores no answer or kind) and would measure a different zone. Give
  `Validate` a `checkScramble` flag (default true; the seed scan and `JobSet_MeetsTheContract` keep
  it, `ShippedJobs_MeetTheContract` passes false), so the shipped check judges the frame and the
  ships, not a re-solved scale.
- **The fixed frame, validated.** `CheckStartFrame` returns early for fixed levels, so nothing checks
  the shipped frame today. In `PuzzleJobValidator.CheckFleet`, for `!puzzle.translatable`: a
  `PuzzleFrame` stores only centre and size (no scale), so check that the frame **solves in place**
  with `PuzzleStartFrame.SolvesInPlace(puzzle, settings, frames[0].centre, frames[0].size)` (some
  reachable scale lands every ship; the helper `SunAnswers` uses), then `ContainsEndpoint` for the
  clearance. `SolvesInPlace` runs 25 full sims, so this check is fixed-fleet-only and is not part of
  the cheap bounds. It runs wherever the validator runs: the seed scan, `JobSet_MeetsTheContract` and
  `ShippedJobs_MeetTheContract`.
- Exit, in seconds-long `Creator.Tests.EditMode` tests (the bake-heavy suite, not the default fast
  pass), with sim settings passed:
  - a tiny synthetic fan-out whose bounding-box centre covers the shared origin (or an extracted
    predicate): it fails before the fix, bakes after, and `frames[0].centre == solution.centre`. Not a
    real 60–90 s bake;
  - the same fixed fleet with its frame moved off the solution (no reachable scale solves in place)
    fails `Validate`;
  - existing contracts still validate with the new fields at 0.

### U2 — the two jobs

- Contracts in `PuzzleJobContracts.All`: `Translatable = false`, `Ships = 2`, `Relation = FanOut`
  (02_09) then `Separate` (02_10), `FieldShape = Ring`, `PlanetCount = 9` (the multi-ship minimum;
  the intro is about readability, the headset decides if it wants 10), `HasSun = true`,
  `SunPlacement = Centre`, `SunRule = NoSunAnswer` (every system keeps a star, as in
  [fleet rebake](../in-progress/fleet-rebake.md); a fixed cube on the sun must not be the answer),
  `Mechanism = Any`,
  `Zones = 1`, `MinRoutes = 1` (a fleet never counts routes; `PuzzleData.routes` stays 1),
  `NoIntro`/`NoFail`/`NoGrade = false`, and `MaxScaleDelta` set (the other two bounds off).
- **The zone covers a neighbour.** A zone enclosing the shared origin is rejected (`ContainsEndpoint`),
  so 02_09's cube sits on a planet *next to* the origin; 02_10's on a planet between the runs. Check that
  planet survives `PuzzleZoneCandidates`' 8-centre trim (ranked by ship 0's run); widen the fleet's
  candidate set if not.
- **Order.** `WireChapterSlot` replaces a job by id when the chapter already lists it and appends it
  only when absent. So the first bake appends 02_09 and 02_10 after 02_04: move them before 02_01 in
  `Chapters/02.asset` once and commit it; every later `Bake` or "Bake all" keeps the slot.
- Scan with the fleet search, bake, pin `Seed`/`Attempts`.
- Exit: both assets validate (including U1's fixed-frame check); the seed scan and
  `JobSet_MeetsTheContract` enforce the bounds, and `ShippedJobs_MeetTheContract` covers the frame and
  the ships with the bounds skipped.

### U3 — the fleet tests

- `FleetJobsTests` hardcodes four jobs and chapter 2 at global index 7, and checks only 02_01–02_04's
  flags. Make it read `Chapters/02.asset`'s levels and assert the intro jobs' flags too.
- Exit: the fast suite is green with chapter 2 at six jobs; 02_01–02_04 keep their ids, after the
  intros.

## Manual check (headset)

1. Chapter 2 opens on 02_09; one pinch on the fixed cube next to the shared origin bends both lines.
2. 02_10: one pinch on the fixed cube sitting between the runs rescues both ships.
3. The start fails on both (no job solves without an edit).
4. 2-1 to 2-4 follow in order, unchanged.
5. Time both; they should be short (~1–1.5 min).
6. The two new cards show a client, cargo and manual (Twin Moons Catering, Two Rival Bakeries); the
   spoken lines stay silent until their clips are generated.
