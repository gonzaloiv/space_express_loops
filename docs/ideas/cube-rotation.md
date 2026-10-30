# Cube rotation — future idea

**Status:** idea · space shaping · updated 2026-11-01 — rotating the cube/region, not in the current run.

**Why not now.** The region stays world-axis-aligned: the cube's handles, the lattice, and the
axis-aligned frame preview all assume it. A rotated frame would complicate the gesture, the zone
containment test, and the baked geometry for a verb the player has not asked for.

**What it would take.** Orientation in the zone transform, a rotated frame preview, and a bake that
accepts oriented zones. Worth it only if a puzzle needs a frame that is not axis-aligned — for example,
a tilted field where the player wants to work along the tilt.

**Related:** [the cube](../design/systems/cube.md) ·
[a more realistic field](../plans/in-progress/space-shaping-realistic-field.md)
