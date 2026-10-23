# Transport Company — Time + Fuel (the two resources)

**Status:** design, not a plan. The **two-resource model**. The pivot is
[project direction](project-direction.md) §3; the fiction is [lore](lore.md); the campaign is
[progression](progression.md). Builds on
[relativity as gameplay](relativity-as-gameplay.md).
**Date:** 2026-10-23.

## 0. Verdict

**Yes — two resources work, and fuel is the piece that makes space-shaping
*skillful* rather than incidental.** But structure them asymmetrically:

- **Time = the window** `[Tmin, Tmax]` (pass/fail). Relativity: the ship's proper-time clock.
- **Fuel = the eco rating** (a score, not a hard fail). Basis: the **edits**.
- **Reading the field is what reconciles them** — the field's natural assists are *free*, so you
  can edit little *and* arrive in the window.

The pivot to a **transport company** also *simplifies*: it drops the live economy
(the co-equal hybrid's problem) and keeps only identity, contracts, and
progression.

## 1. Why the edits cost fuel

The launch is **Coast** — an initial velocity, no engine burn (`ShipLaunch.cs`,
`LaunchModel.Coast`). In a Coast + potential-well model the ship exchanges energy with the
field as it passes: it falls into a well and comes out faster or redirected, *for free*. That
is a **gravity assist**, and it already emerges from the existing physics.

- The field's **natural assists are free**; the **edits** are what you pay for — wrapping a
  zone is the *paid* move.
- So the skillful play is to **read the field better and edit less** — get the speed you did
  not pay for.
- So fuel is not a second bar to manage; it is **the reason the field matters**.

This is the design insight: *the resource and the mechanic justify each other.*

## 2. The two resources

| Resource | Basis | Role | Failure mode |
|---|---|---|---|
| **Time** | ship proper time; dilation deep in a well | the **window** `[Tmin, Tmax]` | hard fail (outside it) |
| **Fuel** | the **edits** — the work the scale does | the **eco rating** (pay) | soft (lower score) |

Why asymmetric: one hard fail keeps the puzzle crisp; a second hard fail doubles the
frustration. Time is legible (a clock); fuel is a score (a grade). The player always
knows *why* they failed (outside the window) and *why* they scored low (edited too much).

### The trade-off (fast vs cheap)
- **Fast but wasteful:** scale the space hard → a shortcut, but a big energy burn.
- **Cheap but slow:** leave the field alone → tiny burn, but you must use the field's natural
  assists to reach the window.
- **The skill:** read the field better and edit less — get speed you did not pay for. Cheap
  *and* in the window is the "perfect run".

## 3. Concrete model (grounded in the current code)

- `fuel = the edit energy` — the cost of the scale's work (one number; not yet in the code).
- `time = Σ dτ` — proper time accumulated in `TrajectoryIntegrator` (see
  [relativity as gameplay](relativity-as-gameplay.md) §3.1). Carried in
  `TrajectoryResult`.
- Per job: **arrive inside `[Tmin, Tmax]`** (hard) and be graded on **fuel used**
  (stars / eco rating / pay).
- Player tools: **grab inside the cube and scale** (and later resize the frame, spin, horizons)
  to reshape the path and the free velocity change.
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

- **One hard fail (the window), one score (fuel).** Do not make both fail states.
- **Both costs visible before commit** — the prediction shows time and fuel, so the
  choice is informed, not a lottery.
- **Per job, not live-ticking.** No real-time economy running under the puzzle.
- **Plain language**: "the ship's clock" and "fuel/eco rating", not physics terms.
- **Fuel stays one number** (the edit energy). Do not add per-edit micromanagement in
  the slice.
- **Keep it simple** (the user's goal): the transport pivot should *reduce* systems
  versus the management hybrid, not add to them.

## 7. Open questions

- **The window from where?** A fixed per-job `[Tmin, Tmax]` (authored) or a function of network
  distance? Fixed-per-job is simplest and most legible.
- **Does fuel ever gate?** If a job's fuel budget is a hard cap (not just a grade),
  it becomes a real constraint but risks double-fail. Recommended: grade only.
- **Is the launch speed a player choice?** *Settled: authored* — the scale is the only verb, so
  fuel is the **edit energy**.
- **Assists as an explicit tool?** Spin/horizon sources make assists deliberate;
  planets alone make them emergent. Start emergent.
- **Eco rating persistence?** A company-wide green rating is a nice meta-goal;
  per-job stars are enough for the slice.
