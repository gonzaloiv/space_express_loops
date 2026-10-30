# Core Loop

**Status:** design · core loop — what the player actually does.
**Date:** 2026-10-23.
**Revised:** 2026-10-30.

**Related:** [vision](vision.md) · [the cube](systems/cube.md) · [progression](systems/progression.md) ·
[jobs](jobs.md) · [glossary](glossary.md)

Three loops, nested. The product *is* the loop; everything else supports it.

## 1. Moment-to-moment (seconds)

Read the field → (move / resize the cube) → scale a zone → the ship flies the result.

- **Read:** the warped sheet, the planets, the straight-line reference.
- **Act:** grab inside the cube and pinch to scale the covered zone ([the cube](systems/cube.md)).
- **See:** the live trajectory line while shaping — it updates on every edit, before release — then the
  flight: the ship launches on each modification and flies that same trajectory. There is **no separate
  commit action** — finishing the edit *is* the commit.

Jobs 1–2 use a **fixed** cube: the only move is the grab-inside scale. Several placed zones enter at
job 3, moving the cube at job 4, and resizing at job 5
([progression](systems/progression.md) §The learning path).

## 2. Session — one job (~1 minute)

Modify → the ship flies the current trajectory (a check) → keep shaping → arrive.

1. **Modify:** every completed pinch launches the ship along the resulting trajectory
   ([the cube](systems/cube.md)) — the finished edit *is* the commit; there is no separate commit
   button.
2. **Check:** the flight **arrives at the destination** — that is the solve — or **misses / crashes**,
   which is only a *wrong* answer, not a fail.
3. **Keep shaping:** a miss does **not** end the job — the player edits on, and a new modification
   **interrupts** the flight. The launch is only a check; the job ends when a solve lands or the
   **committed spend reaches the entropy budget** (the fail). If the same commit both arrives and
   empties the budget, the solve wins. Once the budget is spent, new edits are refused;
   reset/retry stays free.
4. **Pass:** arriving passes the job, graded on **entropy left** → 1–3 stars.

**Entropy is charged per modification, not per launch** — it persists across launches, so failing
drains the bar.

## 3. The one resource — entropy

Entropy is the Wrap's cost and the only resource the player manages: a **budget** (the fail) and the
**score** (the grade).

- **Every completed modification burns it.** The cost is the **net** scale change from the last
  committed value for that zone, charged when the edit finishes — the same cost the bake budgets, so
  returning the hands to where they started costs nothing. **Reset/retry is free** and **undo refunds**
  the last edit; only committed modifications cost.
- **The budget is exhausted when committed spend reaches it:** new edits are refused and an unsolved
  job **fails**. If the same commit both arrives and empties the budget, the solve wins.
- **The grade is what is left** — the eco rating, shown as **1–3 stars**, awarded only on a solve.
- **Why the edits cost and the field is free.** The launch is **Coast** — an initial velocity, no engine
  burn (`ShipLaunch.cs`, `LaunchModel.Coast`). In a Coast + well model the ship exchanges energy with
  the field as it passes: it falls into a well and comes out faster or redirected, *for free*. That is a
  **gravity assist**, and it emerges from the existing physics. The field's assists are free; the
  **modifications** are what you pay for — entropy is *the reason the field matters*.
- **The trade-off.** Edit less: the field carries you — cheap. Edit more: the Wrap buys the bend, but
  burns entropy. The perfect run is **cheap and arriving**.

**Fuel is not a lever.** The launch is **given** — one global speed, the same for every job — and lives
in the fiction as the company's cost ([lore](world/lore.md)). The readout shows the result, the entropy
bar (committed spend plus a pending segment for the in-progress edit), the grade, and a command count
(zones — a lab diagnostic only).

## 4. Meta — the company (~10 minutes)

Solve jobs in order; the map fills; stars accumulate ([progression](systems/progression.md)).

- The map is a field of **numbered cubes**, one per job; solving lights them.
- The company (identity, contracts) is a wrapper *between* jobs — never live-ticking under the
  puzzle ([vision](vision.md) §5).

## 5. What the loop must do

- **A goal:** the destination — **arrive**.
- **A budget:** entropy — spend it all before you solve and the job **fails**; what is left is the
  score.
- **A legible read:** the field, so the player can *predict* the cube's effect.
- **A payoff:** the ship-travel moment (the demo beat).

If any of these is missing the loop reads as a toy. The validation ([vision](vision.md) §7) tests exactly
this.
