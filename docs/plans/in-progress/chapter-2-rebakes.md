# Chapter 2 — rebakes

**Status:** now · space shaping · updated 2026-11-27 — 02_09 must be solved by a bend and 02_06 must
be solvable; both rebaked after the bake overhaul, with the search (the constructor is not ready).
U1–U4 done; the headset check remains.

**Bakes:** off the editor — U2 (02_09) and U3 (02_06)
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapter 2 playtest fixes](chapter-2-playtest.md) ·
[lessons](../../lessons.md) ·
[bake construct jobs](bake-construct-jobs.md)

## Why

The 2026-11-27 headset pass: 02_06 (three ships, formerly 02_04) has no answer under today's physics,
and 02_09 (`Mechanism = Any`) plays out like the Clear jobs before it.

## Depends on

The bake overhaul (closed 2026-11-27, [lessons](../../lessons.md)): one
checkout, every core, bakes that write only their job and its answer. If
[bake construct jobs](bake-construct-jobs.md) has landed, bake both with `Generator = Construct`;
otherwise use the search (the constructor is not built as of 2026-11-27); that field stays live until
[construct jobs](bake-construct-jobs.md) U2 re-bakes both with `Construct` and re-pins
them.

## Before U1

Done 2026-11-27: the bake answer/parallel code and the playtest leftovers are committed, `bake` is
fast-forwarded, and the backfill wrote 13 answers. 02_09 was the one failure (`no solved zone`), so
U1 was red and U2 ran.

## State on `our-space` (2026-11-27)

- 02_09: `Curve`, seed 183, answer stored (0.50 scale delta at a 0.241 start). The edge sun sits at
  y 0.299, level with the planets, and horizontal 0.350, beyond the 0.338 rim.
- 02_06: `Pending` cleared, seed 86, 3 ships `Separate` with their own origins, 14 planets, edge sun.
  The answer is a 0.40 scale delta at a 0.269 start; the sun sits at y -0.312 (the planet band's low
  edge) and horizontal 0.360, beyond the 0.285 rim.
- `PuzzleBrief.SupportsFleetRole` accepts `Curve`, as for Clear fleets.

## Units

### U1 — check 02_09 as shipped

- The shipped field (seed 9) was documented as a curve, but the backfill could not solve it. Run
  `ShippedJobs_MeetTheContract`; it is a replay once the answer is stored. If green, skip U2.
- Exit: green, or a red result that names why.

### U2 — rebake 02_09 (only if U1 is red)

- Scan ~200 seeds, up to ~400 (three ships are rare). If nothing passes, loosen `MinScaleDelta` /
  `MinStartDistance` before giving up, and record the gap here.
- Pin `Seed` (`Attempts = 1`), bake, and read the asset: the `centreOnly` sun sits level with the
  planets and outside their rim.
- Exit: the scan entry meets the contract with `Curve`; asset, answer and pin committed.

### U3 — bake 02_06

- Scan with the same budget. If none bakes, stop and report: dropping 02_06 is then the user's call
  (chapter 2 at seven jobs, the story at 20, `SectorWire.LineCount` 21).
- Pin the seed, clear `Pending`, bake, read the asset as in U2.
- Exit: the scan entry meets the contract; the shipped check passes on it.

### U4 — order and tests

- `Chapters/02.asset` lists 02_01, 02_02, 02_03, 02_04, 02_05, 02_06, 02_07, 02_09 (set by hand; after
  bake answer U4 a bake no longer rewrites it). The 02_06 slot keeps its guid, so the order should not
  move; check it.
- Run `ShippedJobs_MeetTheContract`, `FleetJobsTests` and `SectorWireTests`.
- Exit: all green; the board says what baked.

## State

- **U1 — red.** `PuzzleAnswerBackfill` on the shipped 02_09 asset reports `no answer meets the
  contract: no solved zone`; the other 13 enabled jobs got answers. The shipped field has no fleet
  answer under today's physics, so U2 runs.
- **U2 — done.** The 200-seed scan (workers 31) found four passing seeds: 183 (window 0.246,
  scale 0.50, start 0.241), 158 (0.241, 0.40, 0.244), 33 (0.171, 0.50, 0.236), 86 (0.141, 0.70,
  0.224). Seed 183 is pinned, baked and committed; the asset's sun sits at y 0.299 (level) and
  horizontal 0.350, beyond the planets' 0.338 rim.
- **U3 — done.** The 200-seed scan found 33 passing seeds; seed 86 was widest (window 0.397, scale
  0.40, start 0.269). `Pending` cleared, baked and committed; the asset's sun sits at y -0.312 (the
  band's low edge) and horizontal 0.360, beyond the 0.285 rim.
- **U4 — done.** The chapter order is unchanged (02_01–02_07, 02_09) and 02_06 keeps guid `80ba`.
  `ShippedJobs_MeetTheContract` (9 ms), `FleetJobsTests` (4) and `SectorWireTests` (5) pass in
  `bake`.

## Manual check (headset)

1. 02_06 can be solved by bending the paths; time it.
2. 02_09 is solved by a bend, not by clearing a blocker, and feels different from 02_07.
3. Both suns sit level with their planets, beside the system.
4. The suns sit at the edge of the planets' height band (02_06 at the low edge, 02_09 at the high
   edge); check they read beside the system, not below or above it. If either reads wrong, prefer the
   middle band height in `PuzzleSunPlacementChoice.TryHome` and re-bake.

## Review applied (2026-11-27)

Both rebakes are in `our-space` (02_09 `b934ce64`, 02_06 `7ce5f818` + `0e2de3c4`), each with a `Curve`
answer and its pin; 02_06 kept its guid, so the chapter slot did not move.

- **Done — U4 is recorded.** The three checks ran in `bake` with both new assets:
  `ShippedJobs_MeetTheContract` (9 ms), `FleetJobsTests` (4/4) and `SectorWireTests` (5/5). State
  carries the numbers, and U3 no longer says "baking next".
- **Done — the sun height band is in the headset check.** The middle-band preference in
  `PuzzleSunPlacementChoice.TryHome` waits for that read, as the review asked.
- **Done — bake commits stay scoped.** The bake commits staged only the job assets, the stored
  answers and the pins; the bake checkout's mp3 `.meta`, `OculusProjectConfig`,
  `EditorBuildSettings` and `_Modules` changes were left out.
