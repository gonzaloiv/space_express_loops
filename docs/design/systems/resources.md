# Resources — Time, Fuel + Entropy

**Status:** design · system · resources.
**Date:** 2026-10-23.

**Related:** [vision](../vision.md) · [the cube](cube.md) · [relativity](relativity.md) ·
[lore](../world/lore.md) · [glossary](../glossary.md)

## Goal

The stakes: what the player spends, what they must hit, and what they are scored on.

## Inputs

The player's **modifications** — the cube scale ([the cube](cube.md)). Reading the field is the
alternative to editing.

## Rules

**Yes — the resources work, and entropy is the piece that makes space-shaping *skillful* rather
than incidental.** Structure them asymmetrically:

| Resource | Basis | Role | Failure mode |
|---|---|---|---|
| **Time** | ship proper time; dilation deep in a well | the **window** `[Tmin, Tmax]` — the goal | none (a miss is only a wrong check) |
| **Fuel** | the job's **given** launch fuel | the **brief** — make it work, or no money | (the job's difficulty) |
| **Entropy** | the **Wrap's** cost — the modifications | the **budget** (fail) + the **eco rating** (score) | **hard fail** when it hits zero before a solve |

- **Time = the window.** Relativity: the ship's proper-time clock ([relativity](relativity.md)).
- **Fuel = the brief.** The launch you are *given*; make it work, or the money does not add up. Not a
  lever. **Global** (`SpaceShapingSettings.launchSpeed`) — the speed does not need to vary per job.
- **Entropy = the budget and the score.** Every **completed modification** burns it (the cost is the
  continuous scale delta, charged when the edit finishes); the grade is the entropy you *did not*
  spend. **Retrying is free** — only modifications cost — but **running out before you solve fails
  the job** (no budget left to find a solve).
- **Reading the field reconciles them** — the field's natural assists are *free*, so you can edit
  little *and* arrive in the window.

Why asymmetric: **one hard fail** keeps the puzzle crisp — and it is entropy, not time. A miss is
only a *wrong* answer (the launch is a check), so the player keeps shaping; the run ends only when a
solve lands or the entropy is gone. Time is legible (a clock) and is the target; entropy is a budget
*and* a score. Fuel is the job's *given* — not a lever.

### Why the modifications cost entropy

The launch is **Coast** — an initial velocity, no engine burn (`ShipLaunch.cs`, `LaunchModel.Coast`).
In a Coast + potential-well model the ship exchanges energy with the field as it passes: it falls
into a well and comes out faster or redirected, *for free*. That is a **gravity assist**, and it
already emerges from the existing physics.

- The field's **natural assists are free**; the **modifications** are what you pay for — wrapping a
  zone is the *paid* move.
- So the skillful play is to **read the field better and edit less** — get the speed you did not
  pay for.
- So entropy is not a second bar to manage; it is **the reason the field matters**.

This is the design insight: *the resource and the mechanic justify each other.*

### The trade-off

- **Edit less:** the field's natural assists (free) carry you — cheap, green.
- **Edit more:** the Wrap buys speed, but burns entropy.
- **The skill:** read the field better and edit less — get speed you did not pay for. Cheap *and*
  in the window is the "perfect run".

## Feedback

- **Entropy is a surfaced bar.** It drains with each modification; what remains is the order the
  Wrap has not yet spent.
- **Zero entropy:** the bar is depleted — the **fail**; a simple "you lose" feedback plays. The
  *visual* collapse is polish, deferred ([vision](../vision.md) §6).
- The path, the ship's clock, and the entropy cost read live on the flight
  ([core loop](../core-loop.md)).

## Failure

- **A miss or a crash** (outside `[Tmin, Tmax]`, or hitting a planet) is only a **check** — the
  launch says right/wrong. It does **not** end the job; the player **keeps shaping**
  ([core loop](../core-loop.md) §2).
- **The one hard fail:** entropy reaches **zero** before a solve — the player has no budget left to
  find one. Entropy spent is what costs; the grade is what remains.
- **Money is flavour only** — a failed job costs nothing but the entropy already spent.

## Tuning

- `entropy` = a per-job **budget**. Each modification drains it; the grade is the entropy **left**.
- **The bar and the star grade come from the puzzle creator:** the budget is set from the solution's
  **continuous cost** (`Σ|Δscale|`, not a move count), charged when each edit completes, and the
  creator derives it.
- `fuel` = the **given** launch → the launch speed. **Narrative, not a stat**: the launch is fixed — a
  launch that would *fail* — and shaping is what makes it land. **Global** — one speed, the same for
  every job.
- `time = Σ dτ` — proper time accumulated in `TrajectoryIntegrator`, carried in `TrajectoryResult`.
- **The window is derived from the run:** `Tcross = runDistance / nominalSpeed`, and the band is
  `Tcross × [1 − t, 1 + t]`; `t` is the seed's `windowTightness`. The window and the entropy budget
  therefore share the actual run, and the bake verifies reach (start outside, solution inside).
- Per job: arrive inside `[Tmin, Tmax]` (the goal) and be graded on entropy left (stars / eco rating);
  run out of entropy first and the job fails.

All pure C#, deterministic, testable — no change to the architecture.

## Open questions

- **Assists as an explicit tool?** Spin/horizon sources make assists deliberate; planets alone make
  them emergent. Start emergent.
- **Eco rating persistence?** A company-wide green rating is a nice meta-goal; per-job stars are
  enough for the slice.
