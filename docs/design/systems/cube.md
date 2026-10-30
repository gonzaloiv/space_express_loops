# The Cube (the instrument)

**Status:** design · system · cube.
**Date:** 2026-10-23.
**Revised:** 2026-10-30.

**Related:** [vision](../vision.md) §3 · [core loop](../core-loop.md) · [progression](progression.md) §The brief ·
[glossary](../glossary.md)

## Goal

One instrument, one view. The cube is the **lens**: a tangible MR tool you **move and resize** over
the bubble; the space **inside** it scales reality/the field — it transforms the zone it covers.
There is no Map/Sculpt mode split, and the whole map stays visible in the background — overview and
detail at once, no transition. **The cube's size is the aperture:** big reads wide and coarse, small
reads narrow and fine.

**One gesture, one effect.** A pinch inside the cube does two things at once, and both are *space*:

- it stretches the **metric** — a shrunk zone is a **shortcut**;
- it **spreads and grows** the covered planets (`mass ∝ radius³`), so a grown planet is heavier and
  bends the path more.

The path bends because the planets moved and grew; the field and the trajectory are computed from the
result ([core loop](../core-loop.md)).

**Honest framing.** The physics is Newtonian gravity — `SpacetimeField` is inverse-square, a source is
position + radius + strength. Relativity is the **language and the fiction**, not a simulation: do not
simulate Einstein's field equations. The test for any new effect is whether it changes what the player
*decides*, not the number they read after solving.

## Inputs — the two gestures (taught in order)

| Gesture | Trigger | Effect |
|---|---|---|
| **The scale** | a two-hand pinch **inside** the cube | scales the space *and* the content: the metric stretches (a shrunk zone is a **shortcut**), the planets **spread** and **grow** (`mass ∝ radius³`), so a grown planet deepens its well and bends the path |
| **The region** *(move at job 4, resize at job 5)* | grab the cube's **edges / corners** | **moves / resizes** the cube (the lens) — sets *where* you work; the soft brush blends several zones |

**Multiple scales are the way.** The player scales **several** zones, and the soft brush blends them
into one field — the core of the verb, not a later unlock. The region's size must stay **separate
from the scale** — coupling them is the bug.

**The brief selects the gestures.** A fixed level locks the move, a locked frame locks the resize, and
the scale inside the cube is always available. **Several zones do not require a movable cube:** a level
may place **several fixed cubes** for the player to scale, so `moves ≥ 2` holds with `translatable`
either way — the level pre-places the zones, or the player moves a single lens between them
([progression](progression.md) §The brief). The game never shows a move count; the lab readout may show
a command count as a diagnostic.

## Rules

- **Soft brush.** The cube's influence falls off at the edges so neighbouring zones blend; a hard
  edge would tear the mesh.
- **The field is derived, never edited.** The player scales the space and the content; the field,
  grid and trajectory are computed from the result.
- **Planets stay the field's sources.** The *verb* changes, not the physics (`SpacetimeField`,
  `TrajectoryIntegrator` stand). Scaling the content changes the field for free through the existing
  `mass ∝ radius³` rule.
- **Planets cannot fuse.** The separation constraint (`SourceConstraints`, 1.2 × the sum of radii,
  applied on every recompute) pushes any overlapping pair apart, so a zone transform can never merge
  two planets — but the constraint can *fight* the cube's intent, so the player must see the planets
  separate.
- **The verb is continuous.** The cube is apply-anywhere; no discrete steps, no move counts.
  Discreteness lives only in authoring ([vision](../vision.md) §8).
- **Undo refunds the last edit.** The session can unwind a committed zone; surfacing the control is
  later ([plans board](../../plans/README.md)).

## Feedback

- The mesh **starts flat and warps only with the player's zones** — the planets' wells are not drawn,
  so any bend reads as something the player did; the lattice spans the run (sized from
  `origin`→`destination`).
- The planets visibly **spread and grow**.
- The cube keeps a **committed check lamp** after landing — **green when the last check arrived**, red
  on a crash — until the next edit. A locked gesture or an over-budget scale **flashes the frame red**
  instead of failing silently.
- The ship **launches on each completed pinch** and flies the current trajectory. The **trajectory
  line** previews **live while shaping**, in a pending accent rather than the checked green/red, then
  takes its result on landing ([core loop](../core-loop.md) §1).

## Failure

None. The tool has no fail state; the only hard fail is **spending the full entropy budget** before a
solve ([core loop](../core-loop.md) §3).

## Tuning

- Falloff width (soft-brush blend) — tune in the lab.
- Scale smoothing — the gesture eases toward its target so the scale, spread and warp move smoothly.

## Open questions

- **Mesh-only future:** dropping planets entirely would be a purer tool, but it discards the
  verified puzzles — **not the slice**.
- Multi-zone solutions: the search and bake are a separate plan; the region/scale decoupling is
  settled — they stay separate.
