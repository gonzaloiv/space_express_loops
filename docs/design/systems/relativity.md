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
| **Time dilation** | clocks slow deep in a well and when fast; the scale deepens the well | route the ship to **spend or save ship-time**; hit the **window**; the score is **entropy left** | low | high — a clock is readable |
| **Frame dragging** | a spinning mass twists spacetime | **spinning planets swirl** the path; adds direction, not just position | low | high — visible spiral |
| **Event horizon** | a point of no return | a hazard — a launch that crosses it is a *wrong* check (it costs an edit), not a run-ending fail | low | very high |
| Gravitational lensing | light bends around mass | route a signal/beacon around an occluder | medium | medium-high |
| Geodesic motion | freely-falling = straightest line in curved space | makes motion feel "purer"; subtle | medium | medium |
| Gravitational waves | moving masses ripple spacetime | launch-timing puzzles | high | low — likely cut |

## The three to actually build

### Ship clock / proper time (signature mechanic)
The single best relativity idea for this game: it turns geometry into **geometry + time** and makes
relativity *the* mechanic.

- A **ship clock** ticks slower the deeper the ship sits in a well **and the faster it moves**:
  `dτ = dt · √(max(0, 1 + 2Φ/c² − v²/c²))` (**two-term**: GR depth + SR speed). There is **no separate
  scale term** — the scale changes the clock **through the field**: growing a planet deepens its well
  (`mass ∝ radius³`). The launch is fixed, but the ship's speed along the path still varies with the
  wells.
- Track proper time by accumulating one scalar in the existing loop
  (`TrajectoryIntegrator.Integrate`) — no new math path, no allocations.
- New objective: **arrive within the window `[Tmin, Tmax]`**, then be graded on **entropy left**
  — the score is entropy, not move count ([resources](resources.md)).
- Why it works: the *shortest* path and the *cheapest-time* path differ. Diving deep past a planet
  is short but the clock crawls; swinging wide is long but the clock races — a real choice the
  player can *see* and *reason about*.
- **One verb, two faces.** The scale changes the metric (**space**: a shortcut / the path) and the
  content (**mass**: the planets spread and grow), and the mass deepens the wells — which is the
  **time** face. Same factor, two readings ([the cube](cube.md) §Feedback). A level with a planet
  **off the trajectory** isolates the time face: the scale changes its mass and the clock, but not the
  path (the ship never passes near it) — the **time tutorial**.

### Frame dragging (variety mechanic)
- Give `SpaceSource` a **spin** (a Vector3 angular velocity, or a scalar + axis).
- Add a tangential acceleration term: a spinning planet **swirls** the path, handed.
- The first source property that adds **direction**, not just position/scale — more variety, no new UI.
- Cheap: one tangential term in `Acceleration`; deterministic. Legible: the path visibly spirals.

### Event horizon (a hazard, not the stake)
The stake is the **entropy budget** ([resources](resources.md)) — the run ends when the budget does,
not when the ship dies. A black hole is then a strong *hazard*: a horizon the ship can never leave,
so a launch that crosses it is a **wrong check** that costs an edit.

- Add a source kind with a horizon radius: crossing it wastes the launch (and its entropy).
- Cheap: reuse the crash classification in `TrajectoryIntegrator.Classify`, distinguishing
  "inescapable" from "bumped a planet".
- Visually the strongest element on the menu; it raises the cost of a bad read without adding a
  second fail state.

## Feedback

- The **ship's clock** on the readout; tint by time cost.
- The **spiral** of a spun planet; the planet visibly spins.
- The **horizon** as the strongest visual.

## Failure

- **The one hard fail is entropy** ([resources](resources.md)); a horizon is a **hazard** — a wrong
  check that costs an edit, not a run-ending fail. (Whether crossing it ends the launch outright or
  is a soft "cannot pass" wall is open.)

## Tuning

- Dilation model: **two-term** — `dτ = dt · √(max(0, 1 + 2Φ/c² − v²/c²))` (GR depth + SR speed); no separate scale term.
- One mechanic in the first 8: time dilation is the core bet. Frame dragging (rotate) is the
  second, for **later puzzles**, after the core puzzle is proven.

## Open questions

- Does crossing the horizon end the launch outright, or is it a soft "cannot pass" wall? (Either
  way it is a hazard, not the run-ending fail — that is entropy.)
- Does spin belong on planets (a spun-up planet) or a new body (a neutron-star-like spinner)?
