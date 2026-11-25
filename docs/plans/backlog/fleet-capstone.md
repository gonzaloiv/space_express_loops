# Fleet capstone

**Status:** next · space shaping · updated 2026-11-26 — 2-5: three ships, fan-out, the chapter's
hardest field, and a three-ship re-solve the shipped check can trust.

**Bakes:** off the editor — U1 (the three-ship re-solve check) and U2 (bake 02_05) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapter 2 intro](chapter-2-intro.md) ·
[chapters](../../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)

## Goal

One three-ship fan-out job that closes chapter 2's fleet lessons. The riskiest bake of the chapter:
three-ship seeds are much slower, and 02_04 already fails today's shipped re-solve.

## Depends on

[Chapter 2 intro](chapter-2-intro.md) (contracts pattern, scramble bounds) and
[fleet Clear](fleet-clear.md) U2, which owns routing movable fleets' `PuzzleMultiShipBake.Frames`
through `PuzzleStartFrame.Place` and the "start solves in place" failure. Without it a 02_05 bake can
pass `StartFrameClear` and still fail `CheckStartFrame`. Id: 02_05
([chapter 2 intro](chapter-2-intro.md) §Ids).

## Units

### U1 — a three-ship re-solve

- `PuzzleFleetSolution.TryFind` finds no single zone for 02_04 under today's settings (the physics
  changed on 2026-11-15, after 02_04's bake). Decide first whether 02_04 still plays (see
  [fleet rebake](../in-progress/fleet-rebake.md)); then widen the shipped search to what the player can
  do (the bake's own candidate set and scales), not more.
- Cost: with ten fleet contracts `ShippedJobs_MeetTheContract` runs for minutes. If it passes ~2 min,
  move the fleet part behind the `[Explicit]` job-set test.
- Exit: the shipped check re-solves a freshly baked three-ship field.

### U2 — 2-5

- Contract: `Ships = 3`, `Relation = FanOut`, `FieldShape = Cluster`, `PlanetCount = 12`,
  `Translatable = true`, `MinRoutes = 1`, `HasSun` and `SunRule` pinned (sun present with
  `NoSunAnswer`, or sun-free with `Any`), `MinScaleDelta` and `MinStartDistance` set (high scramble; `MaxScaleDelta` off).
- Id 02_05; play it after 2-4 or as the chapter's last job (decide before baking).
- Exit: the asset validates and re-solves; the start does not solve in place.

## Manual check (headset)

1. Three ships fan out from one origin; a cube on the sun does not solve.
2. It is solvable within budget; the 3★ line is reachable.
3. Time it; target ~3–4 min.
