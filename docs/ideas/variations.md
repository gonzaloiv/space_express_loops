# Space Shaping — Variations (Recorded)

**Status:** idea · space shaping · updated 2026-10-26 — recorded, design only. Aligned with the lens/entropy model
(2026-10-24): the **verb is the cube scale**, not moving sources; the **score is entropy**,
not move count; there is **one view**, not a Map/Sculpt split. This doc records the
variations that sit *after* the validation.

**Related:** [vision](../design/vision.md) · [resources](../design/systems/resources.md) · [progression](../design/systems/progression.md) · POC · [catalogue](../plans/backlog/space-shaping-catalogue.md)

**Wiring rule:** all visuals and interaction props stay authored in the scene/prefabs
with serialized references. No runtime component lookup, mesh/material creation, or
hierarchy building.

## 1. The framing (aligned)

- **One view, one instrument.** The cube is the tool and the level. There is no Map/Sculpt
  split (see [vision](../design/vision.md) §4).
- **The verb is the scale.** The player scales zones (space + content); the field, grid and
  trajectory are derived. The grid is the *record of the player's zones*: it starts flat and warps
  only where a zone is applied — the planets' own wells are not drawn, so a bend always reads as
  something the player did ([cube](../design/systems/cube.md)).
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
  [vision](../design/vision.md) §3.

### 2.3 Sequential-with-lock (fallback for many ships)

If simultaneous ships prove too constrained, freeze a solved ship's recorded path so later
planet moves do not disturb it, then solve the next ship against the evolving field. Prefer
this over per-ship separate fields if you want many ships but still one world.

## 3. Level generation — the puzzle generates its own field from its seed

**Decided (2026-10-26): the puzzle creates its own field from its seed.** A puzzle is a
**seed + a brief**; the field is **derived** from the seed — not hand-placed, and not a region cut
from the campaign's universe field. This is the cleanest path: `SectorField.Generate` is already a
pure, seeded, deterministic function, so the puzzle field reproduces from the seed alone, and the
campaign map's aperture / real-metre field is **not** on this critical path.

- **Seed → field.** The seed drives a puzzle-scoped `SectorFieldData` (density, shape, budgets,
  cell size) through the existing `SectorField.Generate` / `SectorShape` math, flattened to
  sources. Positions come from the seed; the puzzle authors the **editing envelope** (`MinScale`,
  `MaxScale`, `MoveRange`) and the **brief** (`[Tmin, Tmax]`, launch, entropy budget/par).
- **The designer authors the brief, not the layout.** Layout variety comes from the seed; the
  hand-tuned part is the window, the launch, and the entropy budget.
- **Editor-authored positions remain a fallback** (a fixed `sources` list) for a hand-tuned puzzle.

Forward (sources -> path) is easy and deterministic. The inverse (path -> sources) is
**underdetermined and chaotic**. Do not try to invert the trajectory map.

Verification is search-and-test, not inversion — iterate the **seed** and the scale edits:

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

- Source state is **baked into the puzzle asset** at author time (the seed generates the field
  offline); the field is derived from the stored positions, and the grid from the player's zones —
  neither is stored.
- Mass is coupled to size for now (`mass ∝ radius³`, constant density); a later element could decouple them (a
  small heavy planet).
- Each ship carries origin, ordered destinations, and its launch — the existing
  ordered-stops shape (`LoopData` / `RouteLeg`). Origin and destination are **placed in the
  generated field**, not authored.
- The **definition** carries the **seed**, the window `[Tmin, Tmax]`, the launch speed (the
  narrative "fuel"), the **editing envelope** (`MinScale`/`MaxScale`/`MoveRange`), and the
  **entropy budget + entropy par** (the star target). No `solutionMoves`.
- **No in-level snapshot.** What saves is player progression — solved jobs + stars — never the scale
  commands. A run is rebuilt from the baked field. If mid-level resume is ever added, its snapshot would
  be puzzle id + the scale commands + ship/route list + the entropy budget/par. The **seed** is
  authoring-only (not shipped to the runtime puzzle loop).

## 5. Campaign menu (level select)

- A big universe — the company's network — shown as a field of **numbered cubes, one per job**.
  The player picks a job; the map remains visible in the Bubble view while its puzzle loads.
- **The cube is the level.** The cube is the instrument in the same play space; selecting a job does not
  create a Map/Sculpt split or a scale-up transition ([vision](../design/vision.md) §3).
- Cubes are **numbered**; the number is the order. The layout gives the spatial feel; the
  numbers give the sequence. Play 1 -> N.
- **The company is lore only.** No economy, ships, or unlocks for now.

## 6. Open questions

1. **Simultaneous ship cap.** Start at 2; raise only if generation stays feasible.
2. **Region size.** How many planets per puzzle before the search space is too chaotic?
3. **Level-gen cost — decided (2026-10-27).** The creator bakes offline; nothing generates at load.
4. **Campaign handoff — proposed.** Loading a picked job is Flow (`CampaignState` → `SpaceShapingState`);
   the map and puzzle share the Bubble view, with no scale-up transition. Navigation is direct pick only
   and cube positions are authored (decided 2026-10-27). (See `_Modules/docs/standards/modes.md`.)
