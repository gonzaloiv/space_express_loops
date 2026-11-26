# Fleet capstone

**Status:** now · space shaping · updated 2026-11-25 — 02_05's contract is in, pending the off-editor
scan; U1 is a verification, not a widening (see Progress).

**Bakes:** off the editor — U2 (scan and bake 02_05) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapter 2 intro](chapter-2-intro.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

One three-ship fan-out job that closes chapter 2's fleet lessons. The riskiest bake of the chapter:
three-ship seeds are much slower, and no three-ship field has passed a shipped re-solve yet.

## Depends on

- **[Fleet rebake](fleet-rebake.md) U4 and [edge sun](edge-sun.md) U1 before any scan:** chapter 2 suns sit at the edge,
  level with the planets, and the validator checks it; 02_06 (a4bad465, sun above the field) is rebaked.
[Chapter 2 intro](chapter-2-intro.md) (contracts pattern, scramble bounds) and
[fleet Clear](fleet-clear.md) U2, which owns routing movable fleets' `PuzzleMultiShipBake.Frames`
through `PuzzleStartFrame.Place` and the "start solves in place" failure (built and merged to `bake`
as `cd51f783`). Id: 02_05 ([chapter 2 intro](chapter-2-intro.md) §Ids).

## Progress (2026-11-25)

- **U2 contract in.** `02_05` sits in `PuzzleJobContracts.All` as `Pending` (Seed 1, bounds off), so the
  scan can probe it while `Enabled` — and every fast test — stays unchanged.
- **U1 is a verify, not a widening.** 02_04 has no contract, so `ShippedJobs_MeetTheContract` never
  touches it; the fleet-rebake decision (2026-11-26) already leaves 02_04 to the headset. A fresh bake's
  answer is inside `PuzzleFleetSolution`'s centre and size space (the untrimmed centre superset and the
  bake's three sizes plus the frame size).
- **Review → fixed (2026-11-25).** The scale claim was wrong: 13 and 32 evenly spaced grids share only
  their ends (gcd(12, 31) = 1), so the bake's exact scale is never tried and the exit rested on a
  plateau wider than one 32-step. `TryFind` now tries the probe's 13 scales first
  (`PuzzleMultiShipProbe.ScaleSamples`, built by `PuzzleZoneCandidates.LinearScales`) and the 32-scale
  grid only on a miss. The exit holds by construction, and a hit is cheaper than before.
- **Order (changed 2026-11-26):** `02_05` closes chapter 2, after the Clear jobs (02_06, 02_07): the
  difficulty climbs to the end, and its last line ("The field came back quiet") leads into chapter 3.
  Was: after `02_04`, before the Clear jobs, per
  [chapters](../../design/chapters.md) and [fleet Clear](fleet-clear.md). `WireChapterSlot` appends it
  after `02_04` on the first bake, which is the wanted slot until the Clear jobs land.
- **Remaining:** the re-check and any rebake are [chapter 2 bakes](chapter-2-bakes.md) U2. Before the edge
  sun: sync this change set to the `bake` checkout, scan `02_05` (three ships are slow; budget
  the seeds), pin the seed and set `MinScaleDelta` / `MinStartDistance` from the bake, unpin `Pending`,
  bake, then add `02_05` after `02_04` in `FleetJobsTests` (3 ships). Measure the shipped check; move the
  fleet part behind the `[Explicit]` job-set test only if it passes ~2 min.

## Units

### U1 — a three-ship re-solve

- 02_04 has no contract, so the shipped check never touches it; it stays headset-owned
  ([fleet rebake](fleet-rebake.md)). The check's job is the fresh 02_05.
- `PuzzleFleetSolution.TryFind` tries the bake's own 13 scales before its 32-scale fallback, so a fresh
  bake is re-solved at its exact scale. No other widening: the centres and sizes already cover the bake.
- Cost: measure `ShippedJobs_MeetTheContract` after 02_05 is in `Enabled`. If it exceeds ~2 min, give
  fleets their own `[Explicit]` shipped test rather than slowing every Creator run.
- Exit: the shipped check re-solves a freshly baked three-ship field.

### U2 — 2-5

- Contract: `Ships = 3`, `Relation = FanOut`, `FieldShape = Cluster`, `PlanetCount = 12`,
  `Translatable = true`, `MinRoutes = 1`, `Mechanism = Any`, `Zones = 1`, `StartRule = RunMidpoint`,
  `HasSun = true`, `SunPlacement = Centre`, `SunRule = NoSunAnswer` (every system keeps a star),
  `MinBlockers = 0` (nothing pins a shared blocker), `MinScaleDelta` and `MinStartDistance` set from the
  bake (high scramble; `MaxScaleDelta` off).
- Id 02_05; play it after 2-4 and before the Clear jobs (2-6..2-8). The chapter's last job is not an
  option once Clear follows the capstone.
- **Seed budget and fallback.** Scan ~200 seeds (a three-ship fan-out is rare). If none bakes, loosen
  `MinScaleDelta` first; if it still has nothing, ship 2-4 as the chapter close and record the gap here.
- Exit: the asset validates and re-solves; the start does not solve in place.

## Manual check (headset)

1. Three ships fan out from one origin; a cube on the sun does not solve.
2. The start does not solve in place; the first edit has to be placed.
3. It is solvable within budget; the 3★ line is reachable.
4. 2-4 still plays unchanged before it.
5. On the map, 02_05 sits after 2-4 and its node sits on the system's star.
6. Time it; target ~3–4 min.

**Settled (2026-11-26): one Wrap, many ships.** A fleet bakes one zone, so 2-4 and 2-5 teach "one Wrap
carries every ship"; no per-ship fixes (multiple zones per ship read the same as a movable cube).
`chapters.md` and `jobs.md` say so.
