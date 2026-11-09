# The Cube (the instrument)

**Status:** design · system · cube.
**Date:** 2026-10-23.
**Revised:** 2026-11-01.

**Related:** [vision](../vision.md) §3 · [core loop](../core-loop.md) · [jobs](../jobs.md) §The run — the brief ·
[glossary](../glossary.md)

## Goal

One instrument, one view. The cube is the **instrument**: a tangible MR tool you **move** over
the bubble; the space **inside** it scales reality/the field — it transforms the zone it covers.
There is no Map/Sculpt mode split, and the whole map stays visible in the background — overview and
detail at once, no transition. **The cube's size is the aperture:** it sets the *scope* of an edit
(how many planets and lattice cells one pinch touches), not its precision; the pinch sets the
magnitude. The level sets the cube's size; resizing is deferred (it works, but the explanation is not
ready).

**One gesture, two roles.** A pinch inside the cube changes the space and the bodies at once:

- it stretches the **metric** — a shrunk zone is a **shortcut**;
- it **spreads and scales** the covered planets: a grown planet is heavier (`mass ∝ radius³`) and bends
  the path more; a shrunken planet is a **smaller obstacle** and can clear a path it was blocking.

The path bends because the planets moved and changed; the field and the trajectory are computed from the
result ([core loop](../core-loop.md)). **Grow to bend, shrink to clear** — the direction of the same
gesture chooses the role. With several cubes, one can shrink an obstacle while another grows a well: a
clear and a curve in one solution.

**Honest framing.** The physics is Newtonian gravity — `SpacetimeField` is inverse-square, a source is
position + radius + strength. A **sun** is a source too: it sits at the field centre, is scalable by
its centre by default, and is never a run endpoint. Relativity is the **language and the fiction**, not
a simulation:
do not simulate Einstein's field equations. The test for any new effect is whether it changes what the
player *decides*, not the number they read after solving.

## Inputs — the two gestures (taught in order)

| Gesture | Trigger | Effect |
|---|---|---|
| **The scale** | a two-hand pinch **inside** the cube | scales the space *and* the content: the metric stretches (a shrunk zone is a **shortcut**), the planets **spread** and **scale** — a grown planet deepens its well and bends the path, a shrunken planet is a smaller obstacle and can clear it |
| **The region** *(move at job 4; resize deferred)* | grab the cube's **edges / corners** | **moves / resizes** the cube — sets *where* you work; the soft brush blends several zones |

**Multiple scales are the way.** The player scales **several** zones, and the soft brush blends them
into one field — the core of the verb, not a later unlock. One zone can clear an obstacle while another
grows a well: the same gesture with two roles. The region's size must stay **separate from the scale** —
coupling them is the bug.

**The map uses the same cube (decided 2026-11-07).** The selection map's cube is this cube. A pinch
inside it grows the selected job's diorama, and past a threshold the job is entered; shrinking does
nothing there. A one-hand grab on its frame slides it over the map (X-Z only) and snaps it to the
nearest job on release; the map stays still and the fields fade in around the cube as it travels.
A locked job lets the pinch start but stops it early and refuses. The map never teaches the move:
the next job is preselected, so moving is optional until job 4 teaches it.

**The brief selects the gestures.** A fixed level locks the move, a locked frame locks the resize, and
the scale inside the cube is always available. In the seven-job run, several zones arrive with the
movable cube ([progression](progression.md) §The several-zones rung). The game never shows a move
count.

## Rules

- **Soft brush.** The cube's influence falls off at the edges so neighbouring zones blend; a hard
  edge would tear the mesh. The falloff lives **inside** the frame — nothing outside the cube changes —
  and eases in and out (a smooth ramp with rounded corners, decided 2026-11-09), so the lattice bends
  as a gradient. The physics and the lattice share the one profile (`SpaceZone.InsideFactor`): the
  grid never shows a warp the ship does not fly through.
- **The field is derived, never edited.** The player scales the space and the content; the field,
  lattice and trajectory are computed from the result.
- **Planets stay the field's sources.** The *verb* changes, not the physics (`SpacetimeField`,
  `TrajectoryIntegrator` stand). Scaling the content changes the field for free through the existing
  `mass ∝ radius³` rule.
- **The sun is scalable by default.** It sits at the field centre and is never an endpoint. A cube
  containing its centre scales it; a level can lock it (`fixedMass`), pinning min and max scale to the
  baked radius. Job 1 has a sun the cube does not cover; the centre (job 2) makes it the lesson with the cube
  over its centre; after that it is a normal source.
- **A translatable level starts the cube at the centre.** The solution includes an off-centre zone, so the
  move is required; the player chooses where to work.
- **Planets cannot fuse.** The separation constraint (`SourceConstraints`, 1.2 × the sum of radii,
  applied on every recompute) pushes any overlapping pair apart, so a zone transform can never merge
  two planets — but the constraint can *fight* the cube's intent, so the player must see the planets
  separate.
- **The verb is continuous.** The cube is apply-anywhere; no discrete steps, no move counts.
  Discreteness lives only in authoring ([vision](../vision.md) §8).
- **No undo in the slice.** A refund would make probing free and the entropy fail unreachable; reset is
  the only way back ([core loop](../core-loop.md) §3).

## Feedback

- The mesh **starts flat and warps only with the player's zones** — the planets' wells are not drawn,
  so any local bend reads as something the player did. Entropy is read from the **in-field bar**, not a
  lattice tint: the zone warp already owns the tint. A field-wide stress (rim or pulse) is optional and
  needs its own channel; the lattice spans the run (sized from `origin`→`destination`).
- While the pinch is in progress the **frame stays fixed** and the **inner volume** scales with the
  hands: the content it covers spreads and grows, the fill follows the hand scale (the throb is only an
  accent), and the trajectory preview follows. The volume, not the frame, is the thing being scaled.
- The planets visibly **spread and scale** — grow to bend, shrink to clear; the sun scales by its centre
  unless the puzzle locks it. A locked sun holds its mass and reads as fixed, so covering it with the cube
  does not look broken.
- The cube keeps a **committed check lamp** after landing — it shows the last check's outcome until
  the next edit. A locked gesture or an over-budget scale **flashes the frame** instead of failing
  silently.
- The job opens with the **start check**: the ship flies the initial field and fails (the tutorial's
  start crashes into the blocker). The **trajectory line** previews **live while shaping**, in a
  pending state rather than the checked result, then takes its result on landing. The ship launches on
  each completed pinch after that ([core loop](../core-loop.md) §2).

## Failure

None. The tool has no fail state; the only hard fail is **spending the full entropy budget** before a
solve ([core loop](../core-loop.md) §3), and the job-1 tutorial is exempt.

## Tuning

- Falloff width (soft-brush blend) — tune in the lab.
- Scale smoothing — the gesture eases toward its target so the scale, spread and warp move smoothly.
- Entropy bar — the in-field bar's drain, band marks and last-edit segment; do not reuse the lattice tint.
- Inner volume fill — follow the hand scale; use the throb only as an accent.
- Sun radius and mass — scalable by its centre by default; a puzzle can lock it if the bake finds
  scaling trivializes the level.

## Open questions

- **Mesh-only future:** dropping planets entirely would be a purer tool, but it discards the
  verified puzzles — **not the slice**.
- Multi-zone solutions: the search and bake are a separate plan; the region/scale decoupling is
  settled — they stay separate.
- **Resize explanation:** the mechanic works, but the player needs a clear "size is scope, not
  precision" lesson before it enters the run. It is chapter 3's first lesson, with rotating the content
  inside the cube after it ([chapters](../chapters.md)).
