# Space Shaping — Variations (Recorded)

**Status:** backlog - design only, recorded. Aligned with the lens/entropy model
(2026-10-24): the **verb is the cube scale**, not moving sources; the **score is entropy**,
not move count; there is **one view**, not a Map/Sculpt split. This doc records the
variations that sit *after* the [lens](../in-progress/space-shaping-lens.md) gate.

**Related:** [project direction](../../design/project-direction.md) · [transport company](../../design/transport-company.md) · [progression](../../design/progression.md) · [POC](../done/space-shaping.md) · [integration](space-shaping-integration.md)

**Wiring rule:** all visuals and interaction props stay authored in the scene/prefabs
with serialized references. No runtime component lookup, mesh/material creation, or
hierarchy building.

## 1. The framing (aligned)

- **One view, one instrument.** The cube is the tool and the level. There is no Map/Sculpt
  split (see [project direction](../../design/project-direction.md) §4).
- **The verb is the scale.** The player scales zones (space + content); the field, grid and
  trajectory are derived.
- **The score is entropy.** A per-job budget drained by the scale interval; the grade is
  entropy left. No move counts.

## 2. Multiple ships / multiple destinations

There is **one shared field and one region per puzzle**: all ships read the same planet
positions. That is what makes it one puzzle.

```text
degrees of freedom = source state (positions/strengths/wells)   (few)
constraints        = sum over (ships x destinations)   (grows fast)
```

When constraints outnumber degrees of freedom the level has **no solution** — not
"hard", unsolvable — and chaos makes the search non-smooth. Everything here is about
staying off that cliff.

### 2.1 Multiple ships — simultaneous

- All ships launch into the shared field and all must succeed.
- Only viable with a hard cap (2-3 ships), generous capture radii, and few destinations
  per ship. Level generation must verify a **single** arrangement that routes every ship.
- **Authored is the friendly case.** When several routes share one bottleneck, shrinking
  that one planet fixes them all at once — the shared failure *points the player at where
  to edit*, so multi-route can be a legibility win, not a cliff. The cliff bites
  generation, where no designer guarantees the shared bottleneck exists.

### 2.2 Multiple destinations — sequential (loop route)

- A route with several stops is a sequence of **re-launched legs**: the ship relaunches at
  each stop with a new direction. A -> B -> C is then a sequence of single-leg problems
  sharing the field.
- This maps onto the existing ordered-stops model (`LoopData` / `RouteLeg`): each leg is one
  launch, independently verifiable, and `RouteLeg.positions` holds the sampled path.
- A job can be a **loop route** (A -> B -> C -> A) — see
  [project direction](../../design/project-direction.md) §3.

### 2.3 Sequential-with-lock (fallback for many ships)

If simultaneous ships prove too constrained, freeze a solved ship's recorded path so later
planet moves do not disturb it, then solve the next ship against the evolving field. Prefer
this over per-ship separate fields if you want many ships but still one world.

## 3. Level generation — editor-authored plus a seed-based creator

Forward (sources -> path) is easy and deterministic. The inverse (path -> sources) is
**underdetermined and chaotic**. Do not try to invert the trajectory map.

Two ways to make a level:

- **Editor-authored (primary).** The designer places planets and tunes the puzzle by hand.
  This is the way for now, and it lets players compare the same puzzle fairly.
- **Seed-based automatic creator (keep in mind).** A level seed carries ship count, planet
  density, and the physics variation. The creator generates candidates and verifies
  solvability.

For either path, verification is search-and-test, not inversion:

1. **Search** arrangements, forward-simulate each, and keep one that arrives within the
   window and inside the entropy budget. That is a **solved** arrangement `S`.
2. The player's **start** is `S` scrambled so that it **fails on time** (a near-miss), not
   crash. The verb is a **continuous scale**, not discrete moves, so the scramble is a
   *deformed arrangement*, not a reversed command list.
3. Verify the start: it is **not already solved**, and **at least one** edit sequence reaches
   the target. Non-uniqueness is expected and fine.

Solvability is guaranteed by construction; nothing is inverted. The entropy budget and the
star thresholds (entropy par) are set from the secured solution's cost.

## 4. Data model

- Source state is the shared field state, stored once: planet positions/strengths and any
  wells. The field and grid are derived, not stored.
- Mass is coupled to size for now (`mass ∝ radius`); a later element could decouple them (a
  small heavy planet).
- Each ship carries origin, ordered destinations, and its launch — the existing
  ordered-stops shape (`LoopData` / `RouteLeg`).
- The **definition** carries the window `[Tmin, Tmax]`, the launch speed (the narrative
  "fuel"), and the **entropy budget + entropy par** (the star target). No `solutionMoves`.
- Snapshot: puzzle id/seed + positions + ship/route list + the entropy budget/par.

## 5. Campaign menu (level select)

- A big universe — the company's network — shown as a field of **cubes, one per job**.
  The player picks a cube; selecting it **scales it up** into the play space.
- **The cube is the level.** The same cube is the puzzle's play space; the two gestures act
  on it. "The cube is the instrument" and "the cube is the level" are one object.
- Cubes are **numbered**; the number is the order. The layout gives the spatial feel; the
  numbers give the sequence. Play 1 -> N.
- **The company is lore only.** No economy, ships, or unlocks for now.

## 6. Open questions

1. **Simultaneous ship cap.** Start at 2; raise only if generation stays feasible.
2. **Region size.** How many planets per puzzle before the search space is too chaotic?
3. **Level-gen cost.** Where does the automatic creator run — authored offline, or at load?
   Keep it out of the player's frame budget.
4. **Campaign transition.** Selecting a cube scales it up; leaving scales it back. Is the
   *load* a Flow state and the scale-up a Mode? (See `_Modules/docs/standards/modes.md`.)
