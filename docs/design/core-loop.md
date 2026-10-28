# Core Loop

**Status:** design · core loop — what the player actually does.
**Date:** 2026-10-23.
**Revised:** 2026-10-28.

**Related:** [vision](vision.md) · [the cube](systems/cube.md) · [resources](systems/resources.md) ·
[progression](systems/progression.md) · [glossary](glossary.md)

Three loops, nested. The product *is* the loop; everything else supports it.

## 1. Moment-to-moment (seconds)

Read the field → (move / resize the cube) → scale a zone → the ship flies the result.

- **Read:** the warped sheet, the planets, the straight-line reference.
- **Act:** grab inside the cube and pinch to scale the covered zone ([the cube](systems/cube.md)).
- **See:** the live trajectory line while shaping — it updates on every edit, before release — then the
  flight: the ship launches on each modification and flies that same trajectory. There is **no separate
  commit action** — finishing the edit *is* the commit.

Jobs 1–2 use a **fixed** cube: the only move is the grab-inside scale. Moving the cube enters at job 3
and resizing at job 5 ([progression](systems/progression.md) §The learning path).

The beat is tight: the modification *is* the check.

## 2. Session — one job (~1 minute)

Modify → the ship flies the current trajectory (a check) → keep shaping → pass.

1. **Modify:** every completed pinch launches the ship along the resulting trajectory
   ([the cube](systems/cube.md)) — the finished edit *is* the commit; there is no separate commit
   button.
2. **Check:** the flight **lands inside the window** `[Tmin, Tmax]` — that is the solve — or
   **misses / crashes**, which is only a *wrong* answer, not a fail.
3. **Keep shaping:** a miss does **not** end the job — the player edits on, and a new modification
   **interrupts** the flight. The launch is only a check; the job ends when a solve lands or the
   **committed spend reaches the entropy budget** (the fail). If the same commit both lands in the
   window and empties the budget, the solve wins. Once the budget is spent, new edits are refused;
   reset/retry stays free.
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

- **A goal:** the window `[Tmin, Tmax]` — the target you must reach.
- **A budget:** entropy — spend it all before you solve and the job **fails**; what is left is the
  score.
- **A legible read:** the field, so the player can *predict* the cube's effect.
- **A payoff:** the ship-travel moment (the demo beat).

If any of these is missing the loop reads as a toy. The validation ([vision](vision.md) §7) tests exactly
this.
