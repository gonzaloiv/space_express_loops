# Core Loop

**Status:** design · core loop — what the player actually does.
**Date:** 2026-10-23.
**Revised:** 2026-11-01.

**Related:** [vision](vision.md) · [the cube](systems/cube.md) · [progression](systems/progression.md) ·
[jobs](jobs.md) · [glossary](glossary.md)

Three loops, nested. The product *is* the loop; everything else supports it.

## 1. Moment-to-moment (seconds)

Read the field → (move the cube) → scale a zone → the ship flies the result.

- **Read:** the **start run** and the bodies it hits, the lattice warp, and the planets' size — a grown
  planet is a stronger well, a shrunken one is a smaller obstacle.
- **Act:** grab inside the cube and pinch to scale the covered zone ([the cube](systems/cube.md)).
- **See:** the live trajectory line while shaping — it updates on every edit, before release — then the
  flight: the ship launches on each modification and flies that same trajectory. There is **no separate
  commit action** — finishing the edit *is* the commit.

**What the player reads.** Three cues, in order: the **straight line** from origin to destination (the
flat-field baseline) and the **bodies on it** (a body on the line is an obstacle), the **lattice warp**
(the zone the player shaped — the planets' own wells are not drawn), and **planet size** (a bigger planet
is a stronger well, `mass ∝ radius³`; a smaller one is a smaller obstacle). The same pinch has two roles:
**grow to bend, shrink to clear**. The live trajectory is the *result* of the read, not a cue: it tells
the player what will happen, while the cues let them predict it. The ship stays at map size; the zone is
what changes.

With more than one ship, **each route is a colour**: its line, baseline and endpoint planets share it,
and the map diorama uses the same colour. The colour identifies the route, not the physics — it is not a
fourth cue — and the line's end carries the outcome (arrive, crash or miss), so the result reads at the
end of the route rather than across the whole line.

Jobs 1–3 use a **fixed** cube: the only move is the grab-inside scale (job 3 adds the shrink-to-clear
direction). Moving the cube enters at job 4, multi-zone at job 5, and the capstone closes at job 7.
Resize is deferred from the slice
([progression](systems/progression.md) §The learning path).

## 2. Session — one job (~1 minute)

**Start** (the failed check) → modify → the ship flies the current trajectory (a check) → keep shaping →
arrive.

0. **Start:** the job opens with a check — the ship flies the initial field and fails (crash or miss).
   It is free (no modification, no entropy) and does not end the job; the failure is the problem to fix.
   The tutorial's start must **crash** into a visible blocker, not miss into empty space.
1. **Modify:** every completed pinch launches the ship along the resulting trajectory
   ([the cube](systems/cube.md)) — the finished edit *is* the commit; there is no separate commit
   button.
2. **Check:** the flight **arrives at the destination** — that is the solve — or **misses / crashes**,
   which is only a *wrong* answer, not a fail.
3. **Keep shaping:** a miss does **not** end the job — the player edits on, and a new modification
   **interrupts** the flight. The launch is only a check; the job ends when a solve lands or the
   **committed spend reaches the entropy budget** (the fail; job 1 is exempt). If the same commit both
   arrives and empties the budget, the solve wins. Once the budget is spent, new edits are refused;
    reset/retry stays free. Job 1 is exempt from the cap too: it has no budget, nothing clamps, and the
    player can keep shaping until the solve.
4. **Pass:** arriving passes the job, graded on **entropy left** → 1–3 stars. **Job 1 is the tutorial
   exception:** it has no entropy fail and no stars; the bar is informational ([jobs](jobs.md)).

**Entropy is charged per modification, not per launch** — it persists across launches, so failing
drains the bar.

## 3. The one resource — entropy

Entropy is the Wrap's cost and the only resource the player manages: a **budget** (the fail) and the
**score** (the grade).

- **Every completed modification burns it.** Each committed pinch costs **|new scale − the last
  committed scale for that zone|**, charged when the pinch finishes. Within one pinch the pending cost
  is free to undo: return the hands to the committed value before release and it costs nothing. After
  release, moving the zone back costs again. **There is no undo in the slice** — a refund would make
  probing free and the fail unreachable. **Reset/retry is free** and restarts the budget with the field.
  Only committed modifications cost.
- **The budget is derived by the bake.** The creator finds a solution, then sets the job's budget to
  **3× that solution's entropy**. Stars are **1/3 and 2/3 of the budget**: 3★ means spending no more
  than the bake's solution, 2★ up to twice that, otherwise 1★. The thresholds are the same shape every
  job; the field sets the absolute cost.
- **The budget is exhausted when committed spend reaches it:** new edits are refused and an unsolved
  job **fails** (job 1 is exempt). If the same commit both arrives and empties the budget, the solve wins.
- **The grade is what is left** — the eco rating, shown as **1–3 stars**, awarded only on a solve.
- **Why the edits cost and the field is free.** The launch is **Coast** — an initial velocity, no engine
  burn (`ShipLaunch.cs`, `LaunchModel.Coast`). In a Coast + well model the ship exchanges energy with
  the field as it passes: it falls into a well and comes out faster or redirected, *for free*. That is a
  **gravity assist**, and it emerges from the existing physics. The field's assists are free; the
  **modifications** are what you pay for — entropy is *the reason the field matters*.
- **The trade-off.** Edit less: the field carries you — cheap. Edit more: the Wrap buys the bend, but
  burns entropy. The perfect run is **cheap and arriving**.

**Fuel is not a lever.** The launch is **given** — one global speed, the same for every job — and lives
in the fiction as the company's cost ([lore](world/lore.md)). The readout shows the result and the stars;
the entropy bar lives **in the field**, as a **post on the grid's front-left vertical edge** (decided
2026-11-10, replacing the base-edge arms) — not in a panel; the lab panel is a diagnostic. The post is
the **remaining entropy**: spending cuts it from the top, a pinch shows its cost just below the top
before it commits (danger if it would overrun), and the last committed edit sits dimmed above. Moving
the cube and launching are visibly free. Notches at the **1/3 and 2/3** use limits carry a **star beside
each band**: a star holds while the top stays above its notch, flickers while a pinch would cost it, and
goes dark once spent past — so the player sees which star an edit is worth before releasing. The grade
itself is still awarded on a solve.

**The bar is the meter.** Entropy is read from the in-field bar, not a screen-space panel. The lattice
tint already shows the zone warp, so entropy must not reuse it; a field-wide stress (rim or pulse) is
optional and needs its own channel. It never shows a move count.

## 4. Meta — the company (~10 minutes)

Solve jobs in order; the map fills; stars accumulate ([progression](systems/progression.md)).

- The map is a field of **numbered nodes**, one per job; solving lights them.
- The company (identity, contracts) is a wrapper *between* jobs — never live-ticking under the
  puzzle ([vision](vision.md) §5).

## 5. What the loop must do

- **A goal:** the destination — **arrive**.
- **A budget:** entropy — spend it all before you solve and the job **fails** (job 1 is exempt); what is
  left is the score.
- **A legible read:** the field, so the player can *predict* the cube's effect.
- **A payoff:** the ship-travel moment (the demo beat).

If any of these is missing the loop reads as a toy. The validation ([vision](vision.md) §7) tests exactly
this.
