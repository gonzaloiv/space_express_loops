# Fleet rebake

**Status:** now · space shaping · updated 2026-11-26 — make the job contract, validator and bake
tooling express a fleet and chapter 2, then rebake 2-1 to 2-3 with no sun answer.
`PuzzleSunPlacement` / `OffCorridor` shipped (2026-11-20).

**Bakes:** off the editor — U3 (scan and rebake 02_01–02_03) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) ·
[fleet search](../../../Assets/_Modules/Docs~/solutions/architecture-patterns/editor-bakes-on-a-plain-snapshot.md)
(built) ·
[release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

2-1 to 2-3 are rebaked through the new sun placement and validate with no sun answer. Each keeps its
`id`, lines, ship relation and field shape (2-1 separate · Ring, 2-2 fan-out · Spiral, 2-3
converge · Spiral); fleets bake `Mechanism = Any`, so no
solution role is promised. **Every system keeps a star** (decided 2026-11-26): 2-1 to 2-3 all have a
sun, off the ships' corridor, so the map and the field shapes read the same as chapter 1. 2-4 keeps
its sun, unchanged.

## Why

- The current 2-1 to 2-3 fields are won by shrinking the sun.
- The contract and the tooling only know single-ship chapter 01: chapter 2 is untouched by them
  today, and adding fleet contracts as-is would write `02_0x` jobs into `Chapters/01/` and rewire
  `01.asset` (`PuzzleTestSetBuilder` hardcodes both).

## Units

### U1 — a fleet contract

- `PuzzleJobContract` gains `Ships`, `Relation` and `SunPlacement`, and `CreateSeed()` sets them
  (today the seed defaults to `ships = 1`, `sunPlacement = Centre`, so `PuzzleJobSeedScanner` takes the
  single-ship path).
- `PuzzleJobValidator` checks the ship count and relation.
- **The validator verifies; it does not search.** `PuzzleJobSeedScan.Measure` validates every baked
  seed, so a probe inside `Validate` would search each field twice. For a fleet, `Validate` verifies
  the *passed* solution for every stored ship (`PuzzleVerifier.Verify` /
  `PuzzleShipScan.Rescues(zone, puzzle.ships)`); the solution kind is not checked for fleets (it comes
  from ship 0, and fleets bake `Mechanism = Any`).
- **Finding a solution for a saved fleet** belongs to `ShippedJobs_MeetTheContract` (U3), not the
  validator: sweep the zone candidates and take a zone that rescues the **stored** `puzzle.ships`.
  Not `PuzzleSolutionSearch.TrySelect` (ship 0 only; on today's 02_01 its zone does not solve the
  fleet), and not the probe's `Find()` + `ConfirmZone` (they pick the brief's best zone and re-select
  ships from the rescue list, so they check a fresh set, not the saved one).
- Start frame (safety net, not a known break; 02_01 agrees today): `CheckStartFrame` recomputes through
  `PuzzleStartFrame.Place`, while `PuzzleMultiShipBake.Frames` stores `zoneCentre`. Check that they
  agree per fleet. The routing of `PuzzleMultiShipBake.Frames` through `PuzzleStartFrame.Place` is
  owned by [fleet Clear](fleet-clear.md) U2; if a rebaked 02_01–02_03 disagrees, do that
  change here and mark it done there.
- Exit: under a test-local fleet contract, the validator rejects a solution where one ship misses and
  accepts today's 02_01 (two ships) with a zone that rescues both; fast suite green. 02_04 cannot be the
  fixture: it no longer solves under today's settings (no candidate zone rescues all three), so the
  headset check still owns it.

### U2 — chapter-aware tooling

- `PuzzleTestSetBuilder`: the folder, chapter asset and `JobPath` come from the job's chapter, not the
  hardcoded `Chapters/01`. `PuzzleJobBakeWindow` groups jobs by chapter.
- **Rewire the slot, keep the rest.** `WireChapter(LoadJobs())` replaces the chapter's whole `levels`
  list with the contracted jobs, so baking 02_01 would drop 02_04 (no contract). Replace the baked
  job in its existing slot (append only when new) and leave uncontracted levels in place.
- **Every entry point.** `BuildOne` and "Bake all" (`Build`, which today builds every enabled job and
  wires once into chapter 01) both build and rewire per chapter. `BuildMaze` keeps its meaning: a dev
  test that replaces chapter 01's list with the maze alone, so it calls an explicit replace-all, not
  the slot rewire.
- Exit: baking a chapter-2 contract writes into `Chapters/02/`, `Chapters/02.asset` still lists
  02_01–02_04 in order, `01.asset` is untouched, and
  `FleetJobsTests.ChapterTwo_IsTheFourJobsAfterChapterOne` stays green.

### U3 — rebake 2-1 to 2-3

- Add `02_01`–`02_03` to `PuzzleJobContracts.All`, scan with the
  [fleet search](../../../Assets/_Modules/Docs~/solutions/architecture-patterns/editor-bakes-on-a-plain-snapshot.md),
  and save each over its asset. The contracts carry the fleet bake's hard constraints and what the
  validator compares: `Translatable = true` (`CheckBrief`), `HasSun = true` on all three
  (`CheckSources`), `Mechanism = Any`, `Zones = 1`, `MinRoutes = 1`,
  `NoIntro`/`NoFail`/`NoGrade = false` (`FleetJobsTests` asserts them).
- **Relation and shape per job**, from today's assets and [chapters](../../design/chapters.md):
  02_01 `Relation = Separate`, `FieldShape = Ring`, `PlanetCount = 10`; 02_02 `FanOut` (shared
  origin), `Spiral`, `PlanetCount = 12`; 02_03 `Converge` (shared destination), `Spiral`,
  `PlanetCount = 12`. `Separate` is the enum's default, so an unset relation would silently bake 2-2
  as separate ships and lose its lesson and its "One origin. Two promises." line. Nothing validates
  the shape, so pin it with `PlanetCount`.
- **Sun rule:** all three use `HasSun = true`, `SunPlacement = OffCorridor`, `SunRule = NoSunAnswer`,
  as 2-1 already does. 2-2 and 2-3 were first rebaked sun-free (2026-11-26); rescan and rebake them
  with the sun: a sun on a field without a sun rule would fail `PuzzleSunRuleCheck.Matches`, and a
  centred sun is what let the old fields be won by shrinking it. If either finds no seed in the scan
  budget, record it here before falling back to sun-free.
- **MinBlockers:** 02_01 keeps 1. 02_02/03 use 0: the validator counts blockers on ship 0's chord, and
  every with-sun fan-out/converge seed baked its planet blocker on the shared run instead (8/8 seeds
  reported `blockers 0`), so `MinBlockers = 1` would reject all of them. The field still carries a
  blocker; the contract just stops rejecting it.
- **Pin the seeds:** record `Seed`/`Attempts` (and `PlanetCount`, `SunOnChord`, `MinBlockers`) per
  job, so `JobSet_MeetsTheContract` and "Bake all" reproduce the rebake.
- **Sun-answer fixtures:** both `SunOnlyJob_SunAnswers` and
  `Validator_RejectsASunAnswerUnderNoSunAnswer` use 02_01. Move both to `01_02` (sun at centre, frame
  on the sun, CentredGrown; `SunAnswers == true`), not `02_04`: with every ship required, 2-4 is not a
  sun win ([lessons](../../lessons.md) §No sun answer).
- **Tests:** extend `ShippedJobs_MeetTheContract` with the stored-ships search from U1 rather than
  adding a second shipped-asset test (the cost stays in the Creator suite). Accepted cost: the Job
  Baker's check runs both `ShippedJobs_MeetTheContract` (three stored-ships searches, tens of seconds
  each) and `JobSet_MeetsTheContract` (three fleet bakes, minutes). It runs only on demand, so the
  minutes are accepted; if they hurt, give the check a chapter-01-only option rather than dropping
  fleets from either test. `FleetJobsTests` (ids, ship counts 2/2/2, sun/frame clearance) must stay
  green.
- Exit: the Creator suite re-solves each saved 02_01–02_03 for every ship and asserts no sun answer;
  all three have a sun off the corridor; 2-4 unchanged; the fast suite is green.

If U3's fields do not bake after a day of seeds, split the layout retune into its own plan rather than
expanding this one.

## Manual check (headset)

1. 2-1 to 2-3: each has a sun off the ships' path; a cube on the sun does not solve.
2. Each is solvable within budget by working the planets.
3. On the map, every chapter 2 node sits on its star; no node floats over empty space.
4. Jim's lines and the cards still match the new fields.
5. 2-4 keeps its sun and plays as before.
