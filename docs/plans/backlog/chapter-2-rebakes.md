# Chapter 2 — rebakes

**Status:** next · space shaping · updated 2026-11-27 — 02_09 must be solved by a bend and 02_06 must
be solvable; both rebake after the bake overhaul, with the constructor if it is ready. Not started.

**Bakes:** off the editor — U2 (02_09) and U3 (02_06)
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapter 2 playtest fixes](../in-progress/chapter-2-playtest.md) ·
[bake answer](../in-progress/bake-answer.md) · [bake parallel](../in-progress/bake-parallel.md) ·
[bake construct fleet](bake-construct-fleet.md)

## Why

The 2026-11-27 headset pass: 02_06 (three ships, formerly 02_04) has no answer under today's physics,
and 02_09 (`Mechanism = Any`) plays out like the Clear jobs before it.

## Depends on

[Bake answer](../in-progress/bake-answer.md) and [bake parallel](../in-progress/bake-parallel.md): one
checkout, every core, bakes that write only their job and its answer. If
[bake construct fleet](bake-construct-fleet.md) has landed, bake both with `Generator = Construct`;
otherwise use the search.

## State on `our-space` (2026-11-27)

- 02_09's contract is already `Mechanism = Curve` and enabled, so `ShippedJobs_MeetTheContract` may be
  red on it until U2.
- 02_06's first contract is in, `Pending = true`, `Seed = 0`: 3 ships, `Separate`, `Cluster`,
  `PlanetCount = 14`, edge sun (`OffCorridor`, `NoSunAnswer`), `Mechanism = Curve`. A centred sun yields
  no valid puzzles for a three-ship field, so 02_06 moves to the edge like the rest of chapter 2
  (`chapters.md` and `jobs.md` already say so).
- `PuzzleBrief.SupportsFleetRole` accepts `Curve`: the probe searches role-agnostically and
  `PuzzleMultiShipBake.Reject` filters the confirmed answer, as for Clear fleets.

## Units

### U1 — check 02_09 as shipped

- Seed 9's documented answer is a curve (0.40 scale delta at a 0.183 start). Run
  `ShippedJobs_MeetTheContract`; once [bake answer](../in-progress/bake-answer.md) has stored 02_09's answer, this is a
  replay. If green, nothing to bake: skip U2.
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

## Manual check (headset)

1. 02_06 can be solved by bending the paths; time it.
2. 02_09 is solved by a bend, not by clearing a blocker, and feels different from 02_07.
3. Both suns sit level with their planets, beside the system.
