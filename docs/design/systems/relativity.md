# Relativity — the Effect Menu

**Status:** design · system · relativity.
**Date:** 2026-10-23.

**Related:** [vision](../vision.md) §4 · [resources](resources.md) · [the cube](cube.md) ·
[glossary](../glossary.md)

## Goal

Relativity is the language, not a skin. This is the menu of effects the cube instruments; each is a
real effect turned into a possible mechanic. **The test for every idea:** does it change what the
player *decides*, or only the number they see after they solve? If the latter, it is decoration.

## Honest framing

The current build is **Newtonian gravity wearing a relativity costume**:

- `SpacetimeField.Acceleration` is inverse-square, `Potential` is `-Σ strength/r`
  (`Scripts/Domain/Spacetime/SpacetimeField.cs:16-41`).
- `SpaceSource` has position, radius, strength — no spin, no time
  (`Scripts/Domain/Sources/SpaceSource.cs`).

That is fine as a base. The opportunity is to make relativity **change decisions**, not just bend
lines. Do **not** try to simulate Einstein's field equations — use a small set of well-understood
effects as clean, stylized rules. Keep the existing determinism, fixed-step integration, and
no-Unity-physics discipline.

## Inputs

The player's **modifications** — moving and scaling the cube over the sources
([the cube](cube.md)).

## Rules

| Effect | Basis | Game verb | Cost | Legibility |
|---|---|---|---|---|
| Curved space (have) | potential well / geodesic | move mass to bend the path | done | high |
| **Time dilation** | clocks slow deep in a well (and when fast) | route the ship to **spend or save ship-time**; hit the **window**; the score is **entropy left** | low | high — a clock is readable |
| **Frame dragging** | a spinning mass twists spacetime | **spinning planets swirl** the path; adds direction, not just position | low | high — visible spiral |
| **Event horizon** | a point of no return | a hazard that is a *true* fail, not a UI label | low | very high |
| Gravitational lensing | light bends around mass | route a signal/beacon around an occluder | medium | medium-high |
| Geodesic motion | freely-falling = straightest line in curved space | makes motion feel "purer"; subtle | medium | medium |
| Gravitational waves | moving masses ripple spacetime | launch-timing puzzles | high | low — likely cut |

## The three to actually build

### Ship clock / proper time (signature mechanic)
The single best relativity idea for this game: it turns geometry into **geometry + time** and makes
relativity *the* mechanic.

- A **ship clock** ticks slower the deeper the ship sits in a well (and, optionally, the faster it
  moves): `dτ = dt · f(Φ, |v|)`. Stylized: `dτ ≈ dt · sqrt(max(0, 1 - |v|²/c²)) · sqrt(1 + 2Φ/c²)`,
  or a game-tuned linear clamp `dτ ≈ dt · clamp(1 + kΦ·Φ - kv·|v|²)`.
- Track proper time by accumulating one scalar in the existing loop
  (`TrajectoryIntegrator.Integrate`) — no new math path, no allocations.
- New objective: **arrive within the window `[Tmin, Tmax]`**, then be graded on **entropy left**
  — the score is entropy, not move count ([resources](resources.md)).
- Why it works: the *shortest* path and the *cheapest-time* path differ. Diving deep past a planet
  is short but the clock crawls; swinging wide is long but the clock races — a real choice the
  player can *see* and *reason about*.

### Frame dragging (variety mechanic)
- Give `SpaceSource` a **spin** (a Vector3 angular velocity, or a scalar + axis).
- Add a tangential acceleration term: a spinning planet **swirls** the path, handed.
- The first source property that adds **direction**, not just position/scale — more variety, no new UI.
- Cheap: one tangential term in `Acceleration`; deterministic. Legible: the path visibly spirals.

### Event horizon (the missing stake)
The game's core risk is that it has **no stake**. A black hole is a horizon the ship can never
leave — a genuine fail, not a UI colour.

- Add a source kind with a horizon radius: crossing it ends the attempt.
- Cheap: reuse the crash classification in `TrajectoryIntegrator.Classify`, distinguishing
  "inescapable" from "bumped a planet".
- Visually the strongest element on the menu, and the honest source of tension.

## Feedback

- The **ship's clock** on the readout; tint by time cost.
- The **spiral** of a spun planet; the planet visibly spins.
- The **horizon** as the strongest visual.

## Failure

- **Event horizon** = a true fail (attempt over) — the stake. (Hard fail vs a soft "cannot pass"
  wall is open.)

## Tuning

- Dilation model: depth-only `dτ = dt · clamp(1 + kΦ·Φ)` (recommended) vs depth + speed.
- One or two mechanics in the competition slice: time dilation is the core bet; frame dragging is
  the second, *after* the core puzzle is proven.

## Open questions

- Time dilation from **depth only**, **speed only**, or both? Depth-only reads cleanly as "gravity
  slows time"; adding speed muddies the choice.
- Is the horizon a **hard fail** or a soft wall?
- Does spin belong on planets (a spun-up planet) or a new body (a neutron-star-like spinner)?
