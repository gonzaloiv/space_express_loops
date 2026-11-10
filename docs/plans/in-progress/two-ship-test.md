# Two-ship test — try multi-ship before deciding

**Status:** now · space shaping · updated 2026-11-10 — the first two-ship level played well (2026-11-10);
next, a few more two-ship levels to play (U3) before deciding on a two-ship job 8. Route colours and the
cube's keep-out moved to [transit routes](transit-routes.md).

**Related:** [chapter 2 — the fleet](../backlog/chapter-2-fleet.md) (2-1 is the same brief) ·
[puzzle creator](../../../Assets/_Game/Features/PuzzleCreator/README.md) · [plans board](../README.md)

## Goal

Multi-ship is out of v1 and untested on the headset. A test level decides whether it plays well
enough to become job 8 (two ships, separate runs) for the competition build. Mixed is dropped as job 8.

## Contract

- **The sun is never a ship's origin or destination** (the design rule for single-ship runs holds for
  every ship). The first test bake broke it; the bake must exclude centre-only sources.
- **The test does not change the run** until the headset test says yes. The level lives in
  `PuzzleCreator/Resources/TwoShips_Test_Puzzle.asset` (seed asset `PuzzleSeed_TwoShips.asset`).

## Units

### U1 — the bake keeps the sun out — code in (2026-11-10)

- Run `PuzzleShipScanTests` once the project compiles; the first run hit stale assemblies.

- `PuzzleShipScan` skips any pair whose origin or destination is a centre-only source, both in the
  pristine cache and in the rescue scan, so the probe never counts a sun run.
- Test in `PuzzleShipScanTests` (bake-heavy suite): on a field with a sun, no rescued run touches it.
- **Exit:** the test passes; a re-bake of `PuzzleSeed_TwoShips` with `hasSun` on gives two ships, none
  on the sun.

### U2 — play it — played (2026-11-10): fun, wants more levels and the fixes in transit routes

- Re-bake the test level with the sun (12 planets, Ring, `ships` 2, `Separate`, mechanism `Any`).
- Temporarily add it to `Chapters/01` as job 8 (`01_08`) for a test build; Jim and the card show
  placeholders. Remove the entry if the test says no.
- **Exit:** a decision: job 8 goes in (then the chapter 2 plan's contract work and strings follow), or
  multi-ship stays post-v1.

### U3 — more two-ship levels

- Bake a small set from `PuzzleSeed_TwoShips` (copy the seed per level so each keeps its own): two
  `Separate` (sun on), one `FanOut`, one `Converge`; 12 planets, Ring or Spiral, mechanism `Any`. Use
  *Find working seed*; save each as its own `TwoShips_*_Puzzle.asset`.
- Play them from a temporary test chapter appended after chapter 1 in `LevelSelector.asset`
  (`Chapters/99`, ids `99_01`…), so chapter 1 stays untouched; Jim and the card show placeholders.
  Remove the chapter from `LevelSelector` before the competition build.
- **Exit:** the four levels play on the headset; a yes or no on a two-ship job 8 for v1, and which
  relation it uses.

## Manual check (headset)

1. Job 8 opens with two ships and two trajectory lines; neither starts or ends at the sun.
2. A pinch launches both ships; the job solves only when both arrive.
3. One arrives, one misses: the job does not solve, and the feedback says which ship missed.
4. Ship 2's small arrival zone is fair to hit (tune if not).
5. The entropy meter reads the same with two ships.
