# Transport Company — Time, Fuel + Entropy (the resources)

**Status:** design, not a plan. The **resource model** — time, fuel + entropy. The pivot is
[project direction](project-direction.md) §3; the fiction is [lore](lore.md); the campaign is
[progression](progression.md). Builds on
[relativity as gameplay](relativity-as-gameplay.md).
**Date:** 2026-10-23.

## 0. Verdict

**Yes — the resources work, and entropy is the piece that makes space-shaping
*skillful* rather than incidental.** But structure them asymmetrically:

- **Time = the window** `[Tmin, Tmax]` (pass/fail). Relativity: the ship's proper-time clock.
- **Fuel = the job's brief** — the launch fuel you are *given*; make it work, or the money does
  not add up. Fixed per job, not a lever.
- **Entropy = the score** — the Wrap's cost. Every edit burns it; the grade is the entropy you
  *did not* spend (the eco rating).
- **Reading the field is what reconciles them** — the field's natural assists are *free*, so you
  can edit little *and* arrive in the window.

The pivot to a **transport company** also *simplifies*: it drops the live economy
(the co-equal hybrid's problem) and keeps only identity, contracts, and
progression.

## 1. Why the edits cost entropy

The launch is **Coast** — an initial velocity, no engine burn (`ShipLaunch.cs`,
`LaunchModel.Coast`). In a Coast + potential-well model the ship exchanges energy with the
field as it passes: it falls into a well and comes out faster or redirected, *for free*. That
is a **gravity assist**, and it already emerges from the existing physics.

- The field's **natural assists are free**; the **edits** are what you pay for — wrapping a
  zone is the *paid* move.
- So the skillful play is to **read the field better and edit less** — get the speed you did
  not pay for.
- So entropy is not a second bar to manage; it is **the reason the field matters**.

This is the design insight: *the resource and the mechanic justify each other.*

## 2. The resources

| Resource | Basis | Role | Failure mode |
|---|---|---|---|
| **Time** | ship proper time; dilation deep in a well | the **window** `[Tmin, Tmax]` | hard fail (outside it) |
| **Fuel** | the job's **given** launch fuel | the **brief** — make it work, or no money | (the job's difficulty) |
| **Entropy** | the **Wrap's** cost — the edits | the **eco rating** (pay) | soft (lower score) |

Why asymmetric: one hard fail keeps the puzzle crisp; a second hard fail doubles the
frustration. Time is legible (a clock); entropy is a score (a grade). Fuel is the job's *given* —
not a lever. The player always knows *why* they failed (outside the window) and *why* they
scored low (edited too much).

### The trade-off
- **Edit less:** the field's natural assists (free) carry you — cheap, green.
- **Edit more:** the Wrap buys speed, but burns entropy.
- **The skill:** read the field better and edit less — get speed you did not pay for. Cheap
  *and* in the window is the "perfect run".

## 3. Concrete model (grounded in the current code)

- `fuel` = the job's **given** launch (authored per job) → the launch speed. **Narrative, not a
  stat**: the launch is fixed — a launch that would *fail* — and shaping is what makes it land.
- `entropy` = a **per-job budget** (authored). Each scale edit drains it by the **scale interval**
  (`Σ |Δscale|`, the cumulative excursion of the gesture); the grade is the entropy **left**.
  Draining it to zero is the collapse risk (§6, §7). Not yet in the code.
- `time = Σ dτ` — proper time accumulated in `TrajectoryIntegrator` (see
  [relativity as gameplay](relativity-as-gameplay.md) §3.1). Carried in `TrajectoryResult`.
- Per job: **arrive inside `[Tmin, Tmax]`** (hard) and be graded on **entropy left**
  (stars / eco rating / pay).
- Player tools: **grab inside the cube and scale** (and later resize the frame, spin, horizons)
  to reshape the path and the free velocity change.
- Both numbers are **predicted live** (the trajectory is deterministic), so the player sees the
  time and entropy cost of the current arrangement before the ship travels.

This is all pure C#, deterministic, testable — no change to the architecture.

## 4. The eco rating (the score's flavour)

Mechanically, **entropy** is the efficiency score axis: in the window is mandatory, entropy
left is the grade (§2). Presenting it as an **eco rating** is the flavour — the fiction behind
it (the 2048 stationary-state economy, "greed is eco-friendly") is in [lore](lore.md).

## 5. The pivot (elsewhere)

The transport-company pivot itself — the map, a job = a delivery, progression — is
[project direction](project-direction.md) §3, and the campaign structure is
[progression](progression.md). This doc is only the **resources** (time, fuel + entropy).

## 6. Guardrails

- **One hard fail (the window), one score (entropy left).** Do not make both fail states.
- **Entropy is a surfaced bar.** The bar drains with each edit; it *is* the eco meter — what
  remains is the order the Wrap has not yet spent. Near zero the universe visibly **collapses**
  (the stake), so "greedy is green" stays legible: waste entropy and the neighbourhood decays.
- **Both costs visible live, before the ship travels** — the prediction shows time and
  entropy, so the choice is informed, not a lottery.
- **Per job, not live-ticking.** No real-time economy running under the puzzle.
- **Plain language**: "the ship's clock" and "entropy" (the bar); "eco rating" is the flavour.
  No other physics jargon (no Schwarzschild, etc.).
- **Entropy stays one number** (the Wrap's cost). Do not add per-edit micromanagement in
  the slice.
- **Keep it simple** (the user's goal): the transport pivot should *reduce* systems
  versus the management hybrid, not add to them.

## 7. Open questions

- **The window from where?** A fixed per-job `[Tmin, Tmax]` (authored) or a function of network
  distance? Fixed-per-job is simplest and most legible.
- **Does zero entropy fail the attempt?** *Settled: no.* Entropy is a **score, not a cap** — you can
  overspend (go negative) and still pass the window, taking **1 star**. The collapse is the
  **visual/thematic stake**, not a fail state.
- **Is the launch speed a player choice?** *Settled: authored* — the scale is the only verb, so
  the score is **entropy left**.
- **Assists as an explicit tool?** Spin/horizon sources make assists deliberate;
  planets alone make them emergent. Start emergent.
- **Eco rating persistence?** A company-wide green rating is a nice meta-goal;
  per-job stars are enough for the slice.
