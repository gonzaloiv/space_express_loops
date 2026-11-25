# Fleet rebake

**Status:** now · space shaping · updated 2026-11-25 — make the job contract, validator and bake
tooling express a fleet and chapter 2, then rebake 2-1 to 2-3 with no sun answer.
`PuzzleSunPlacement` / `OffCorridor` shipped (2026-11-20).

**Related:** [plans board](../README.md) · [fleet search](fleet-search.md) (depends on) ·
[release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

2-1 to 2-3 are rebaked through the new sun placement and validate with no sun answer. Each keeps its
`id`, lines and role; 2-4 stays the sun job.

## Why

- The current 2-1 to 2-3 fields are won by shrinking the sun.
- The contract and the tooling only know single-ship chapter 01 (2026-11-25 review): adding fleet
  contracts today would bake single-ship levels into `Chapters/01/` and overwrite chapter 2's assets.

## Units

### U1 — a fleet contract

- `PuzzleJobContract` gains `Ships`, `Relation` and `SunPlacement`, and `CreateSeed()` sets them
  (today the seed defaults to `ships = 1`, `sunPlacement = Centre`, so `PuzzleJobSeedScanner` takes the
  single-ship path).
- `PuzzleJobValidator` checks the ship count and relation.
- Its re-solve verifies **every** ship with `PuzzleVerifier.Verify` / `PuzzleShipScan.Rescues`:
  `PuzzleSolutionSearch.TrySelect` → `PuzzleZoneRun.Simulate` runs ship 0 only.
- Start frame: `CheckStartFrame` recomputes through `PuzzleStartFrame.Place`, while
  `PuzzleMultiShipBake.Frames` stores `zoneCentre` directly. Route the fleet bake through `Place`, or
  check that they agree per fleet.
- Exit: the validator rejects a fleet asset where ship 2 misses; fast suite green.

### U2 — chapter-aware tooling

- `PuzzleTestSetBuilder`: the folder, chapter asset, `JobPath` and `WireChapter` come from the job's
  chapter, not the hardcoded `Chapters/01`. `PuzzleJobBakeWindow` groups jobs by chapter.
- Saving over an asset (`PuzzleArtifactExporter`, `CopySerialized`) wipes `id`; re-apply
  `baked.id = job.Id` on every save path. Chapter 2's lines and clips are keyed `level_02_0X_*`.
- Exit: baking a chapter-2 contract writes into `Chapters/02/`, rewires only `Chapters/02.asset`, and
  keeps the id.

### U3 — rebake 2-1 to 2-3

- Add `02_01`–`02_03` to `PuzzleJobContracts.All` with `NoSunAnswer` (2-1 `OffCorridor`, 2-2/2-3
  sun-free), scan with the [fleet search](fleet-search.md), and save each over its asset.
- **Pin the seeds:** record `Seed`/`Attempts` (and `PlanetCount`, `SunOnChord`, `MinBlockers`) per
  job, so `JobSet_MeetsTheContract` and "Bake all" reproduce the rebake.
- **Sun-answer fixture:** do not move it to `02_04`. With every ship required, 2-4 is not a sun win
  ([lessons](../../lessons.md) §No sun answer), so `SunOnlyJob_SunAnswers` would go red. Use `01_02`
  (sun at centre, frame on the sun, CentredGrown) as the positive.
- **Tests:** extend `ShippedJobs_MeetTheContract` with the fleet verification rather than adding a
  second shipped-asset test (the cost stays in the Creator suite). `FleetJobsTests` (ids, ship counts
  2/2/2, sun/frame clearance) must stay green.
- Exit: the Creator suite re-solves each saved 02_01–02_03 for every ship and asserts no sun answer;
  2-1 keeps its sun; 2-4 unchanged; the fast suite is green.

If U3's fields do not bake after a day of seeds, split the layout retune into its own plan rather than
expanding this one.

## Manual check (headset)

1. 2-1 to 2-3: a cube on the sun does not solve.
2. Each is solvable within budget by working the planets.
3. 2-1 still has its sun; 2-2 and 2-3 have none.
4. Jim's lines and the cards still match the new fields.
5. 2-4 still teaches the sun with the fleet.
