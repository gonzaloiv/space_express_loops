# The Cube (the instrument)

**Status:** design · system · cube.
**Date:** 2026-10-23.
**Revised:** 2026-10-28.

**Related:** [vision](../vision.md) §3 · [resources](resources.md) · [relativity](relativity.md) ·
[progression](progression.md) §The brief · [glossary](../glossary.md)

## Goal

One instrument, one view. The cube is the **lens**: a tangible MR tool you **move and resize** over
the bubble; the space **inside** it scales reality/the field — it transforms the zone it covers.
There is no Map/Sculpt mode split, and the whole map stays visible in the background — overview and
detail at once, no transition. **The cube's size is the aperture:** big reads wide and coarse, small
reads narrow and fine. The scale has **two faces** — **space** (a shortcut / the path) and **time**
(the ship's clock) — driven by the **same factor** ([relativity](relativity.md)). It scales the space
and the content.

## Inputs — the three gestures (taught in order)

| Gesture | Trigger | Effect | Relativity |
|---|---|---|---|
| **The scale** | a two-hand pinch **inside** the cube | scales the space *and* the content: the metric stretches (a shrunk zone is a **shortcut**), the planets **spread** and **grow** (`mass ∝ radius³`) — a grown planet deepens its well, which bends the path and slows the clock | time dilation |
| **The region** *(move at job 3, resize at job 5)* | grab the cube's **edges / corners** | **moves / resizes** the cube (the lens) — sets *where* you work — and from job 3 you can scale **several** zones; the soft brush blends them | — |
| **The rotate** *(later puzzles)* | spin the zone | **frame dragging** — a spinning zone swirls the path; adds direction for free | frame dragging |

**Multiple scales are the way.** The player scales **several** zones, and the soft brush blends them
into one field — the core of the verb, not a later unlock. The region's size must stay **separate
from the scale** — coupling them is the bug. Rotate arrives in **later puzzles**
([progression](progression.md) §The learning path).

**The brief selects the gestures.** A fixed level locks the move, a locked frame locks the resize, and
the scale inside the cube is always available. **Several zones require a movable cube:** a fixed cube
covers one region, so `moves ≥ 2` implies `translatable` ([progression](progression.md) §The brief).
The game never shows a move count; the lab readout may show a command count as a diagnostic.

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
- **The time face reads on the same lattice** — the zone is **tinted cool/warm** (slower / faster clock);
  the pulse at the local clock rate is a [candidate](../../ideas/feel-polish.md) (the lattice is a
  wireframe). The window gauge is the payoff: the projected arrival slides as the player scales
  ([relativity](relativity.md)).
- The ship **launches on each completed pinch** and flies the current trajectory. The **live trajectory
  line** is the prediction while shaping — it updates on every edit, before release — and the flight
  repeats it ([core loop](../core-loop.md) §1).

## Failure

None. The tool has no fail state; the only hard fail is **spending the full entropy budget** before a
solve ([resources](resources.md)).

## Tuning

- Falloff width (soft-brush blend) — tune in the lab.
- Scale smoothing — the gesture eases toward its target so the scale, spread and warp move smoothly.

## Open questions

- **Mesh-only future:** dropping planets entirely would be a purer tool, but it discards the
  verified puzzles — **not the slice**.
- Rotate (later puzzles) timing.
- Multi-zone solutions: the search and bake are the
  [multi-zone plan](../../plans/in-progress/space-shaping-multizone.md); the region/scale decoupling is
  settled — they stay separate.
