# Transport Company — Time + Fuel (the two resources)

**Status:** design, not a plan. The **two-resource model**. The pivot is
[project direction](project-direction.md) §3; the fiction is [lore](lore.md); the campaign is
[progression](progression.md). Builds on
[relativity as gameplay](relativity-as-gameplay.md).
**Date:** 2026-10-23.

## 0. Verdict

**Yes — two resources work, and fuel is the piece that makes space-shaping
*skillful* rather than incidental.** But structure them asymmetrically:

- **Time = the deadline** (pass/fail). Relativity: the ship's proper-time clock.
- **Fuel = the efficiency score** (a rating, not a hard fail). Physics: launch
  energy.
- **Sculpting the field is what reconciles them** — it is how you buy speed for
  free, so you can launch cheap *and* arrive fast.

The pivot to a **transport company** also *simplifies*: it drops the live economy
(the co-equal hybrid's problem) and keeps only identity, contracts, and
progression.

## 1. Why fuel is the missing piece

Right now the launch is **Coast** — an initial velocity, no engine burn
(`ShipLaunch.cs`, `LaunchModel.Coast`). In a Coast + potential-well model the ship
exchanges energy with the field as it passes: it falls into a well and comes out
faster or redirected, *for free*. That is a **gravity assist**, and it already
emerges from the existing physics.

- Without a fuel cost, there is **no reason to ever launch slow** — just launch
  fast and ignore the planets. The field becomes decoration.
- Add a fuel cost and **launching fast becomes expensive**, so the skillful play is
  to launch cheap and use the field to gain the speed you did not pay for.
- So fuel is not a second bar to manage; it is **the reason the sculpting matters**.

This is the design insight: *the resource and the mechanic justify each other.*

## 2. The two resources

| Resource | Physical basis | Role | Failure mode |
|---|---|---|---|
| **Time** | ship proper time; dilation deep in a well | the **deadline** (must arrive by T) | hard fail (late) |
| **Fuel** | launch energy, `k · launchSpeed²` | the **efficiency rating** (eco grade, pay) | soft (lower score) |

Why asymmetric: one hard fail keeps the puzzle crisp; a second hard fail doubles the
frustration. Time is legible (a clock); fuel is a score (a grade). The player always
knows *why* they failed (late) and *why* they scored low (wasteful).

### The trade-off (fast vs cheap)
- **Fast but thirsty:** launch at high speed → short trip, big fuel burn.
- **Cheap but slow:** launch slow → tiny burn, but you must sculpt the field to
  gain speed and cut the path, or you miss the deadline.
- **The skill:** use assists (fall in/out of wells, spin, horizons as slingshots)
  to get speed you did not pay for. Cheap *and* on time is the "perfect run".

## 3. Concrete model (grounded in the current code)

- `fuel = k · launchSpeed²` — spent once at launch (Coast model). One number.
- `time = Σ dτ` — proper time accumulated in `TrajectoryIntegrator` (see
  [relativity as gameplay](relativity-as-gameplay.md) §3.1). Carried in
  `TrajectoryResult`.
- Per job: **arrive before `deadline`** (hard) and be graded on **fuel used**
  (stars / eco rating / pay).
- Player tools: move/scale planets (and later spin, horizons) to reshape the path
  and the free velocity change.
- Both numbers are **predicted before commit** (the trajectory is deterministic),
  so the player can see the time and fuel cost of an arrangement before launching.

This is all pure C#, deterministic, testable — no change to the architecture.

## 4. The eco rating (the score's flavour)

Mechanically, fuel **is** the efficiency score axis: on-time is mandatory, fuel efficiency is
the grade (§2). Presenting it as an **eco rating** is the flavour — the fiction behind it (the
2048 stationary-state economy, "greed is eco-friendly") is in [lore](lore.md).

## 5. The pivot (elsewhere)

The transport-company pivot itself — the map, a job = a delivery, progression — is
[project direction](project-direction.md) §3, and the campaign structure is
[progression](progression.md). This doc is only the **two resources**.

## 6. Guardrails

- **One hard fail (time), one score (fuel).** Do not make both fail states.
- **Both costs visible before commit** — the prediction shows time and fuel, so the
  choice is informed, not a lottery.
- **Per job, not live-ticking.** No real-time economy running under the puzzle.
- **Plain language**: "the ship's clock" and "fuel/eco rating", not physics terms.
- **Fuel stays one number** (launch energy). Do not add per-burn micromanagement in
  the slice.
- **Keep it simple** (the user's goal): the transport pivot should *reduce* systems
  versus the management hybrid, not add to them.

## 7. Open questions

- **Deadline from where?** A fixed per-job time (authored) or a function of network
  distance? Fixed-per-job is simplest and most legible.
- **Does fuel ever gate?** If a job's fuel budget is a hard cap (not just a grade),
  it becomes a real constraint but risks double-fail. Recommended: grade only.
- **Is `launchSpeed` a player choice or authored?** If the player picks it, that is
  a second verb beside sculpting (aim/choose speed). If authored, sculpting is the
  only verb and fuel is fixed per job. The former is richer; the latter is simpler.
- **Assists as an explicit tool?** Spin/horizon sources make assists deliberate;
  planets alone make them emergent. Start emergent.
- **Eco rating persistence?** A company-wide green rating is a nice meta-goal;
  per-job stars are enough for the slice.
