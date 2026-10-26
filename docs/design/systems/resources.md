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
| **Time** | ship proper time; dilation deep in a well | the **window** `[Tmin, Tmax]` | hard fail (outside it) |
| **Fuel** | the job's **given** launch fuel | the **brief** — make it work, or no money | (the job's difficulty) |
| **Entropy** | the **Wrap's** cost — the modifications | the **eco rating** (pay) | soft (lower score) |

- **Time = the window.** Relativity: the ship's proper-time clock ([relativity](relativity.md)).
- **Fuel = the brief.** The launch you are *given*; make it work, or the money does not add up. Not a
  lever. **Global for now** (`SpaceShapingSettings.launchSpeed`); per-job is deferred.
- **Entropy = the score.** Every **modification** burns it; the grade is the entropy you *did not*
  spend. **Retrying is free** — only modifications cost.
- **Reading the field reconciles them** — the field's natural assists are *free*, so you can edit
  little *and* arrive in the window.

Why asymmetric: one hard fail keeps the puzzle crisp; a second hard fail doubles the frustration.
Time is legible (a clock); entropy is a score (a grade). Fuel is the job's *given* — not a lever.

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
- **Zero entropy (for now):** the bar is depleted and a simple "you lose" feedback plays. The
  *visual* collapse is polish, deferred ([vision](../vision.md) §6).
- The path, the ship's clock, and the entropy cost read live on the flight
  ([core loop](../core-loop.md)).

## Failure

- **A miss or a crash** (outside `[Tmin, Tmax]`, or hitting a planet) reports a **fail**. For now
  that is all — no destroy visual; the player **keeps shaping** ([core loop](../core-loop.md) §2).
- **Soft:** spend too much entropy — a lower grade. Zero entropy does **not** fail the run on its
  own; entropy is a score, not a cap.
- **Money is flavour only** — failing costs nothing but the entropy the modifications already spent.

## Tuning

- `entropy` = a per-job **budget**. Each modification drains it; the grade is the entropy **left**.
- **The bar and the star par come from the puzzle creator:** the budget is based on the **number of
  modifications** the level's solution needs, and the creator derives it ([puzzle creator](../../plans/in-progress/space-shaping-puzzle-creator.md)).
- `fuel` = the **given** launch → the launch speed. **Narrative, not a stat**: the launch is fixed — a
  launch that would *fail* — and shaping is what makes it land. **Global for now**; per-job is
  deferred.
- `time = Σ dτ` — proper time accumulated in `TrajectoryIntegrator`, carried in `TrajectoryResult`.
- Per job: arrive inside `[Tmin, Tmax]` (hard) and be graded on entropy left (stars / eco rating).

All pure C#, deterministic, testable — no change to the architecture.

## Open questions

- **The window from where?** A fixed per-job `[Tmin, Tmax]` (authored) or a function of network
  distance? Fixed-per-job is simplest and most legible.
- **Assists as an explicit tool?** Spin/horizon sources make assists deliberate; planets alone make
  them emergent. Start emergent.
- **Eco rating persistence?** A company-wide green rating is a nice meta-goal; per-job stars are
  enough for the slice.
