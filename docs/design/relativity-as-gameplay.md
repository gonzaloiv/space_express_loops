# Relativity as Gameplay — Design Notes

**Status:** design ideation, not a plan. The effect menu the **cube** instruments
(see [project direction](project-direction.md) §5, §9). Feeds
[space-shaping-future](../plans/backlog/gameplay/space-shaping-future.md).
**Context:** "Our Space" — seated mixed reality, Meta VR Start Developer
Competition 2026 (see [project direction](project-direction.md)).
**Date:** 2026-10-23.

## 1. Honest framing

The current build is **Newtonian gravity wearing a relativity costume**:

- `SpacetimeField.Acceleration` is inverse-square, `Potential` is `-Σ strength/r`
  (`Scripts/Domain/Spacetime/SpacetimeField.cs:16-41`).
- `SpaceSource` has position, radius, strength — no spin, no time
  (`Scripts/Domain/Sources/SpaceSource.cs`).
- The docs say so: "the Newtonian gradient is the practical stand-in for a geodesic;
  the relativity is the metaphor and the visual" ([space-shaping](../plans/done/space-shaping.md) §4.2).

That is fine as a base. The opportunity is to make relativity **change decisions**,
not just bend lines. Do **not** try to simulate Einstein's field equations — use a
small set of well-understood effects as clean, stylized rules. Keep the existing
determinism, fixed-step integration, and no-Unity-physics discipline.

**The test for every idea below:** does it change what the player *decides*, or only
the number they see after they solve? If the latter, it is decoration.

## 2. Effect menu

Each row is a real effect turned into a possible mechanic. Cost is against the
current codebase; legibility is how well a seated first-timer reads it.

| Effect | Basis | Game verb | Cost | Legibility |
|---|---|---|---|---|
| Curved space (have) | potential well / geodesic | move mass to bend the path | done | high |
| **Time dilation** | clocks slow deep in a well (and when fast) | route the ship to **spend or save ship-time**; hit a deadline; least-proper-time score | low | high — a clock is readable |
| **Frame dragging** | a spinning mass twists spacetime | **spinning planets swirl** the path; adds direction, not just position | low | high — visible spiral |
| **Event horizon** | a point of no return | a hazard that is a *true* fail, not a UI label | low | very high |
| Gravitational lensing | light bends around mass | route a signal/beacon around an occluder | medium | medium-high |
| Geodesic motion | freely-falling = straightest line in curved space | makes motion feel "purer"; subtle | medium | medium |
| Gravitational waves | moving masses ripple spacetime | launch-timing puzzles | high | low — likely cut |

## 3. The three to actually build

### 3.1 Ship clock / proper time (signature mechanic)
The single best relativity idea for this game, because it turns geometry into
**geometry + time** and makes relativity *the* mechanic instead of the skin.

- Add a **ship clock** that ticks slower the deeper the ship sits in a well (and,
  optionally, the faster it moves): `dτ = dt · f(Φ, |v|)`.
  Stylized form: `dτ ≈ dt · sqrt(max(0, 1 - |v|²/c²)) · sqrt(1 + 2Φ/c²)`, or a
  game-tuned linear clamp `dτ ≈ dt · clamp(1 + kΦ·Φ - kv·|v|²)`.
- Track proper time by accumulating one scalar in the existing loop
  (`TrajectoryIntegrator.Integrate`, `TrajectoryIntegrator.cs:16-29`) — no new math
  path, no allocations.
- New objective: **arrive within a ship-time budget**, or **minimise ship-time**
  (the golf/par model already implied by `PuzzleData.solutionMoves`).
- Why it works as a puzzle: the *shortest* path and the *cheapest-time* path
  differ. Diving deep past a planet is short but the clock crawls; swinging wide is
  long but the clock races. That is a real choice the player can *see* (the clock)
  and *reason about* (near = slow).
- Demo value: a clock visibly slowing as the ship dives is a strong, legible
  "wow" for a competition beat.

### 3.2 Frame dragging (variety mechanic)
- Give `SpaceSource` a **spin** (a Vector3 angular velocity, or a scalar + axis).
- Add a tangential acceleration term: a spinning planet **swirls** the path around
  itself, handed (clockwise vs counter-clockwise).
- This is the first source property that adds **direction**, not just
  position/scale — it multiplies puzzle variety without new UI.
- Cheap: one tangential vector term in `Acceleration`; deterministic.
- Legible: the trajectory visibly spirals; the planet visibly spins.

### 3.3 Event horizon (the missing stake)
The game's core risk is that it has **no stake**. A black hole is a
horizon the ship can never leave — a genuine fail, not a UI colour.

- Add a source kind with a horizon radius: crossing it ends the attempt (fail).
- Cheap: reuse the crash classification in `TrajectoryIntegrator.Classify`
  (`TrajectoryIntegrator.cs:34-50`), but distinguish "inescapable" from "bumped a
  planet".
- Visually the strongest element on the menu, and the honest source of tension.

## 4. How each maps to the code

| Idea | Touch | Change |
|---|---|---|
| Time dilation | `TrajectoryIntegrator`, `TrajectoryResult` | accumulate proper time; carry it in the result |
| Frame dragging | `SpaceSource`, `SpacetimeField.Acceleration` | add `Spin`; add tangential term |
| Event horizon | `SpaceSource`, `TrajectoryIntegrator.Classify` | add horizon radius; classify as `Horizon` fail |
| Objective change | `PuzzleData`, `SpaceShapingSetup`, `SpaceShapingSession` | ship-time budget/par alongside `solutionMoves` |
| Read-out | `TrajectoryPredictionView` / UI | show the ship clock; tint by time cost |

All pure-C#, deterministic, unit-testable — consistent with the existing domain
split (`Scripts/Domain/…`) and the project's "no LINQ, no per-frame allocations"
rule.

## 5. Guardrails

- **Stylised effects, not GR simulation.** Keep fixed-step, deterministic, no
  `Rigidbody`/PhysX.
- **One or two mechanics in the competition slice.** Time dilation is the core
  bet; frame dragging is the second, *after* the core puzzle is proven.
- **Every mechanic changes a decision**, or it is a gimmick (§1).
- **Plain language in-game.** Surface it as "the ship's clock" and "spin", not
  "Schwarzschild" or "Lense-Thirring".
- **Keep the content-local, scale-independent simulation** so the bubble/scale
  gestures do not change the result.

## 6. Recommended order

1. **Ship clock + a ship-time objective** on the existing one-ship puzzle — the
   highest-value relativity step, and it also supplies the missing stake.
2. **Event horizon** as a hazard — cheap, and completes the tension.
3. **Frame dragging** — variety once the core is proven.
4. Lensing / geodesic / waves — only if the slice lands and wants more.

## 7. Open questions

- Time dilation from **depth only**, **speed only**, or both? Depth-only is simpler
  and reads cleanly as "gravity slows time"; adding speed makes the fast wide path
  also slow, which muddies the choice.
- Objective shape: **arrive at all**, **arrive within budget**, or **minimise
  ship-time**? Budget is forgiving and readable; minimise is the golf/par model.
- Does the ship clock apply to the **live prediction** too (so the player sees the
  cost before committing), or only to the launched run?
- Is the horizon a **hard fail** (attempt over) or a soft "cannot pass" wall in the
  prediction? Hard fail gives the stake; soft keeps the sandbox feel.
- Does spin belong on planets (a spun-up planet) or a new body (a neutron-star-like
  spinner)? A spun planet reuses art; a new body is a stronger landmark.
