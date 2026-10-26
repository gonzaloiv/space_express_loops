# Core Loop

**Status:** design · core loop — what the player actually does.
**Date:** 2026-10-23.

**Related:** [vision](vision.md) · [the cube](systems/cube.md) · [resources](systems/resources.md) ·
[progression](systems/progression.md) · [glossary](glossary.md)

Three loops, nested. The product *is* the loop; everything else supports it.

## 1. Moment-to-moment (seconds)

Read the field → move the cube → scale a zone → the ship flies the result.

- **Read:** the warped sheet, the planets, the straight-line reference.
- **Act:** grab inside the cube and pinch to scale the covered zone ([the cube](systems/cube.md)).
- **See:** the flight — the ship launches on each modification and flies the current trajectory, and
  the trajectory visual updates at launch. There is **no commit step** and **no prediction line**.

The beat is tight: the modification *is* the check.

## 2. Session — one job (~1 minute)

Modify → the ship flies the current trajectory (a check) → keep shaping → pass.

1. **Modify:** every committed pinch launches the ship along the resulting trajectory
   ([the cube](systems/cube.md)) — no separate commit.
2. **Check:** the flight **lands inside the window** `[Tmin, Tmax]` (pass) or **misses / crashes**
   (fail).
3. **Keep shaping:** a fail does **not** end the job — the player edits on, and a new modification
   **interrupts** the flight. The launch is only a trajectory check.
4. **Pass:** a landing in the window passes the job, graded on **entropy left** → 1–3 stars
   ([resources](systems/resources.md)).

**Entropy is charged per modification, not per launch** — it persists across launches, so failing
drains the bar.

## 3. Meta — the company (~10 minutes)

Solve jobs in order; the map fills; stars accumulate ([progression](systems/progression.md)).

- The map is a field of **numbered cubes**, one per job; solving lights them.
- The company (identity, contracts) is a wrapper *between* jobs — never live-ticking under the
  puzzle ([vision](vision.md) §5).

## 4. What the loop must do

- **A goal:** the window `[Tmin, Tmax]` — one hard fail.
- **A cost:** entropy — one score.
- **A legible read:** the field, so the player can *predict* the cube's effect.
- **A payoff:** the ship-travel moment (the demo beat).

If any of these is missing the loop reads as a toy. The gate ([vision](vision.md) §7) tests exactly
this.
