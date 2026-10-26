# The Cube (the instrument)

**Status:** design · system · cube.
**Date:** 2026-10-23.

**Related:** [vision](../vision.md) §3 · [resources](resources.md) · [relativity](relativity.md) ·
[progression](progression.md) §4 · [glossary](../glossary.md)

## Goal

One instrument, one view. The cube is a tangible MR tool you move over the bubble; it transforms the
zone it covers. There is no Map/Sculpt mode split, and the whole map stays visible in the background
— overview and detail at once, no transition. **The cube's size is the aperture:** big reads wide and
coarse, small reads narrow and fine.

## Inputs — the three gestures (taught in order)

| Gesture | Trigger | Effect | Relativity |
|---|---|---|---|
| **The scale** | a two-hand pinch **inside** the cube | scales the space *and* the content: the metric stretches (a shrunk zone is a **shortcut**) and the planets **spread** and **grow** (mass ∝ radius) | time dilation |
| **The region** *(job 3)* | grab the cube's **edges / corners** | sets *where* you work — and you can scale **several** zones; the soft brush blends them | — |
| **The rotate** *(job 4)* | spin the zone | **frame dragging** — a spinning zone swirls the path; adds direction for free | frame dragging |

**Multiple scales are the way.** The player scales **several** zones, and the soft brush blends them
into one field — the core of the verb, not a later unlock. The region's size must stay **separate
from the scale** — coupling them is the bug. Rotate unlocks at **job 4**
([progression](progression.md) §4).

## Rules

- **Soft brush.** The cube's influence falls off at the edges so neighbouring zones blend; a hard
  edge would tear the mesh.
- **The field is derived, never edited.** The player scales the space and the content; the field,
  grid and trajectory are computed from the result.
- **Planets stay the field's sources.** The *verb* changes, not the physics (`SpacetimeField`,
  `TrajectoryIntegrator` stand). Scaling the content changes the field for free through the existing
  `mass ∝ radius` rule.
- **Planets cannot fuse.** The separation constraint (`SourceConstraints`, 1.2 × the sum of radii,
  applied on every recompute) pushes any overlapping pair apart, so a zone transform can never merge
  two planets — but the constraint can *fight* the cube's intent, so the player must see the planets
  separate.
- **The verb is continuous.** The cube is apply-anywhere; no discrete steps, no move counts.
  Discreteness lives only in authoring ([vision](../vision.md) §8).

## Feedback

- The mesh **starts flat and warps only with the player's zones** — the planets' wells are not drawn,
  so any bend reads as something the player did; the lattice spans the run (sized from
  `origin`→`destination`).
- The planets visibly **spread and grow**.
- The ship **launches on each modification** and flies the current trajectory — the flight is the
  read; the trajectory visual updates at launch (no separate prediction line)
  ([core loop](../core-loop.md) §1).

## Failure

None. The tool has no fail state; the **window** is the only hard fail ([resources](resources.md)).

## Tuning

- Falloff width (soft-brush blend) — tune in the lab.
- Scale smoothing — the gesture eases toward its target so the scale, spread and warp move smoothly.

## Open questions

- **Mesh-only future:** dropping planets entirely would be a purer tool, but it discards the
  verified puzzles — **not the slice**.
- **`SourceSpec.MoveRange` is vestigial** — `SourceSpecs.From` sets it to the planet's scale, and the
  zone scale moves `Home` with the planet, so `SourceConstraints`' clamp never binds. Drop it, or give
  it a real meaning (a per-planet move limit).
- Rotate (job 4) timing, and the region/scale decoupling under multiple zones.
