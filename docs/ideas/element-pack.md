# Field Elements — making the field read like a galaxy

**Status:** idea · space shaping · updated 2026-10-30 — how the puzzle field becomes a galaxy. Not scheduled.

**Related:** [presentation](../design/presentation.md) · [progression](../design/systems/progression.md) ·
[the cube](../design/systems/cube.md) · [jobs](../design/jobs.md)

## Goal

The puzzle field should **read as a real galaxy** — a populated, dusty, lit volume, not a few balls on a
wireframe. Every element is judged on two things: **how much galaxy it buys**, and whether it **changes
the puzzle** (a body that bends the path) or only the **read** (decor the cube still shapes).

## Solvability — the risk that comes with decor

The more the field looks like a galaxy, the easier it is to **mistake decor for a source**. The player
reads the path from the bodies that bend it, so anything weightless has to *look* weightless: a distinct
material and scale, no well glow, and a clear difference from a real source. Dust and grid highlights
carry the most risk; **decor moons** are the candidate filler — small clutter bodies that sell the
galaxy without touching the path — but they still need that distinct read, or the puzzle stops being
solvable at a glance.

Rule: **if the player cannot tell what bends the path, the element is not done.**

## Data — every element ships with the puzzle

Everything a level draws — sources, decor, the sun's position, the dust layer — is **authored into the
puzzle data** and baked with it, so a level **relaunches identically, forever**. No runtime
randomization, no generated geometry; the baked asset is the whole level.

Recommended shape: one `PuzzleSource[]` entry per body with a **`kind`** (`source` | `decor`), so decor
takes the same zone transform as a planet and `BuildField` simply skips it. The bake writes decor like
any other entry, so it is as reproducible as the sources.

## The ladder (cheapest first)

**More planets is already in** — the bake ships N sources and the view pools them — so the ladder starts
at the first *new* element. A body that bends the path is data, not code: `PuzzleSource[]` takes N
entries, each becoming `radius = scale/2`, `mass = radius³` (`SpaceShapingSession.cs:107-109`); a new
element is worth it only when it changes what the player *decides*.

**Cost axis — code/schema work, cheapest first:** bake data → a highlight in a mesh that already exists →
a new authored layer → a per-body flag plus a parent/orbit. Only tier 1 changes the physics (a new well,
so the bake verifies it like any source); tiers 2–4 are presentation only, so they cannot break
solvability by construction — only the *read* can fail (§Solvability).

### 1. A small planet — a second, static source *(approved)*

A second well beside a planet: a double-well the player has to read, authored as just another
`PuzzleSource`. Data only — the look is the existing planet prefab scaled down. **It is not a moon:** it
does not orbit and it is not attached, so the cube transforms it independently of its neighbour (scale a
zone that covers one and not the other and they separate).

### 2. The sun, in the grid — the cheapest graphic

The sun is **a graphic in the lattice style, not a tint**: authored wireframe geometry (a cage or a
burst) drawn with the same line look as the grid. The lattice is an authored tube mesh
(`LatticeMeshBuilder`, menu `DigitalLove/Space Shaping/Build Lattice Mesh`) whose vertices the view
rewrites every frame from `LatticeGrid.Node` and the warp (`SpacetimeMeshView.WarpVertices`), so a sun
graphic riding that path is shaped by the cube for free.

Cost: authored geometry plus a small view extension — the view assumes one fixed vertex layout
(`samplesPerLine = vertexCount / (3 · gridCount² · sides)`), so the sun either joins that layout or is a
sibling mesh driven by the same warp delegate. No data, no flag, no parent, no pool.

### 3. Dust — noise around the field

Dust is **noise around the field**, not a particle simulation: a procedural noise veil (or noise-scattered
points) filling the space in and around the play area, drawn in the same warped space as the lattice so
the cube **shapes it** and it adds **no well**. A **fixed seed** keeps it deterministic — the same
relaunch shows the same dust — so there is nothing to simulate, pool, or store: the layer is a shader (or
one authored mesh) plus a seed.

This is the biggest galaxy read for the cost: it is what makes empty space read as *space*.

Open: a shader veil on the existing lattice (cheapest, no new mesh) or noise-scattered points (more
depth, one authored mesh). Either way it is authored or computed — never instantiated at runtime.

### 4. Decor bodies — the moon — shaped by the cube, weightless

A **decor** body is shaped by the cube **exactly like a planet** — it moves and scales with the zone —
but contributes **no gravity**. The zone transform still applies to it; the session skips it when it
builds the field (`BuildField`, `SpaceShapingSession.cs:102-113`), so it never enters `SpacetimeField`.

**The moon is the first instance, and it is the first real code.** A moon *orbits a planet*, and a body
that moves **cannot be a source**: `TrajectoryIntegrator.Integrate` integrates one static field
(`field.Acceleration(position)`, no time term), so a moving well would make the field time-varying and
the prediction time-dependent. So the moon is decor by construction, and it needs:

- the `kind` flag from §Data (source | decor) so `BuildField` skips it;
- a **parent link** to its planet, so it rides the planet's transformed position after the cube acts;
- a **deterministic orbit** — a pure function of the level clock (no randomness), so the level relaunches
  identically;
- a decor prototype in the view, and one decision — whether decor joins `SourceConstraints.Separate`
  (probably yes, so it does not interpenetrate a planet).

Read: *planets bend, moons decorate* — a moon is never checked because it can never bend anything. That
is forced by the static field, not chosen.

### 5. Later — new systems

The rest of the pack — belt, nebula, station, comet, gate, black hole — each a system, not a body.

## Constraints

- **A belt is not "more planets".** `SourceConstraints.Separate` pushes any pair apart to
  `(scale_i + scale_j) * 0.5 * separationScale` (`SourceConstraints.cs:28`), so a scatter of many tiny
  sources fights the solver — that is a real system, not data.
- **No runtime asset creation.** Every element is an authored prefab or a visual drawn from the existing
  lattice — never geometry invented in code.
- **Determinism.** Motion is allowed only as a **pure function of the level clock** (the moon's orbit), so
  a relaunch is identical; nothing is randomized at load. A moving body is never a source — the field
  must stay static for the integrator.

## Sector-field decor (later)

The original pack: the ambient/decor set for the **sector field** backdrop, not the puzzle. Each element
gets a perf budget and a decor/gameplay classification before art. Do not start with volumetrics or
physics.

| Element | Role | Cost | Phase |
|---|---|---|---|
| Anchor planet (sun role) | Sector anchor/landmark | Low; existing planet prefab, no light | v1 (field) |
| Planets | Decor, activatable for a puzzle | Low-medium; pooled prefab | v1 (field) |
| Moons/rings | Planet variety | Low; child visuals | v1.5 |
| Asteroid belt/field | Visual ring, optional obstacle | Medium; instanced meshes | v1.5 |
| Nebula | Visual region | High if volumetric | v2 (billboards/particles only) |
| Station/outpost | Landmark; future demand layer | Medium; TBD | v2 |
| Comet/derelict | Rare event/resource | Medium; timed spawn | v2 |
| Wormhole/gate | Travel between regions | Medium; gameplay decision | v3 |
| Black hole | Hazard/landmark | Medium; VFX | v3 |

Suns are not a new element type: the anchor is a scaled planet using the existing planet
prefab/material, with no per-sun lights and no new shaders — or, cheaper still, a wireframe graphic in
the lattice (see tier 2).
