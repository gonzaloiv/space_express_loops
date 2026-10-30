# Space Shaping — a more realistic field

**Status:** now · space shaping · updated 2026-10-30 — starts Mon 2026-11-02: make the field read as a
galaxy — depth in every direction and mass at the centre. Field elements from the
[element pack](../../ideas/element-pack.md).

**Related:** [the cube](../../design/systems/cube.md) · [presentation](../../design/presentation.md) ·
[rings](../backlog/space-shaping-planet-rings.md)

## 1. Why

The field is planets on a wireframe: a thin horizontal disc with no centre and no scale cues. It reads as
a diagram, not a place. Realism here is **structure, not decoration** — a galaxy has a massive centre and
a shape (a disc, a bulge, a halo or an ellipsoid), and those are what the eye reads as *space*. Two moves
give the field that structure:

- **depth** — a disc with a bulge, a true 3D cluster, and a small tilt stop the field reading as one flat
  sheet;
- **mass** — the sun is a source at the centre, so the path bends around it.

The dust is out of this plan: it was the biggest perf and read risk (transparency overdraw, decor mistaken
for a source). Planet rings are also out — they have their own plan. The three-ring sun graphic was tried
on 2026-10-30 and removed: it read as another object among the planets. The right sun is **real mass with a
symbolic look** — a source, marked by a reference.

## 2. Slices

### S1 — a field with depth

- **The field is one thin horizontal disc today.** `SectorShape` spreads X/Z across the radius and uses Y
  only as a jitter (`inclination × radius`, 0.08 in `PuzzleSeed_Shape`), and the layout's random rotation
  is yaw only. Every level is a flat sheet.
- Keep the thin **disc** for Ring/Spiral — a real spiral is thin — but give the centre a 3D **bulge** (the
  sun's seat), make **Cluster** a true 3D ellipsoid instead of a flattened disc, and add a deterministic
  pitch/roll **tilt** so a level is not always edge-on to the player.
- **The generator is shared, so the change is opt-in.** `SectorShape` also feeds the ambient sector field
  (`SectorField`). Add `thickness`/`tilt`/`bulge` knobs to `SectorShapeData` that default to today's flat
  values, and set them only in `PuzzleFieldBuilder`; the decoration field keeps its current look until a
  later look pass.
- **Tilt the layout, not the cube.** Bake the tilt into the planet positions so the cube stays
  world-axis-aligned; the lattice frame already measures an AABB, and the creator preview keeps its
  top-down projection with height lines. Check the tilted field against the camera and the cube.
- **The sun's radius is an S1 input.** The generator's anchor clearance is the sun's radius, so add that
  value to the seed/settings here, before the sun source is wired in S2. Otherwise the discs are tuned
  against a planet-sized hole and every baked level moves when the sun lands.
- **Exit:** a level has a 3D centre, Cluster has comparable vertical and horizontal extent, the disc is
  tilted off the XZ plane, and the run and cube still read in the headset.

### S2 — the sun as a source

- **One sun per puzzle, created with the field.** It is a `PuzzleFieldPlanet`/`PuzzleSource` entry flagged
  as the sun; `PuzzleFieldBuilder`/`PuzzleFieldGenerator` create it at the centre. Nothing global, nothing
  pooled from another level.
- **The sun's radius is the generator's anchor clearance.** Feed it into the layout's `AnchorRadius` so the
  first `SourceConstraints.Separate` pass never moves a baked planet. Today `PuzzleFieldBuilder` hard-codes
  `anchorScale = 1`, which is only a planet-sized hole.
- **Scalable by its centre, not by enclosure.** A zone scales any source whose centre it contains; only
  planets must be enclosed by the cube. That keeps the cube range planet-sized — so the bake search does
  not explode — while the star still scales. The star's visual may poke out of the cube; that is a
  look-pass call.
- Its look is a **reference, not a star**: a flat, diagrammatic marker in the readout's own language —
  unlit, thin, no volume — so it *annotates* a mass instead of pretending to be a body. Add a serialized
  `sunPrototype` to `SpaceShapingSpawner`; the sun must not spawn through the planet pool or get a
  `PlanetNameLabel`. The removed rings failed because they read as *another object among the planets*; a
  reference must not.
- **The sun is never a run endpoint.** Filter it out of `PuzzleRunChoice`'s endpoint pairs, but include it
  in the blocking check so a run through the centre is a valid blocked run.
- It sits in the bulge from S1, so the centre already has depth before the mass is added.
- **Exit:** a level with a star — the path bends around it, the baked solution avoids it, the planets sit
  outside its separation radius, the star can be scaled by a cube over its centre, and the marker reads as
  an annotation rather than a body.

### S3 — look pass

- Tune the star's radius and mass (a per-source mass multiplier if a readable size needs a stronger well),
  the disc thickness and tilt, and the materials; measure the cost on Quest.

## 3. Decisions

1. **Structure is the realism, and it comes first.** Dimensionality follows the shape: Ring/Spiral are thin
   discs with a central bulge, Cluster is a 3D ellipsoid, and the layout may be tilted off the XZ plane. A
   galaxy is a disc with a spheroidal centre and a halo, not one flat layer.
2. **One sun per puzzle, sized before the planets.** The sun is a per-puzzle entry, and its radius sets the
   generator's anchor clearance so no baked planet ever moves in the first separation pass.
3. **The sun scales by its centre.** A zone scales any source whose centre it contains; only planets must
   be enclosed. The cube stays planet-sized, so the bake's search does not grow, and the star still scales.
   If the star proves a free win, add the fixed-mass flag.
4. **The sun is a source first.** The mass is the real thing; the visual is only a reference marker laid
   over it. The removed sun ring graphic stays removed — it read as an object beside the mass.
5. **Placement is the centre.** A star belongs at the centre of its system, so the field generator's
   shapes (Ring/Spiral/Cluster) wrap it — a generator change, not just a data point.
6. **Density is a dial.** `mass ∝ radius³` makes a big body light for its size; a per-source mass
   multiplier is the honest knob if the star must dominate.
7. **The generator change is opt-in.** New `SectorShapeData` knobs default to today's flat values so the
   ambient sector field is untouched; only `PuzzleFieldBuilder` opts into 3D.
8. **Tilt the layout, not the cube.** The tilt lives in the planet positions; the cube and its gestures
   stay world-axis-aligned.
9. **The dust is out of this plan.** It stays a tier-3 idea in the element pack; revisit it alone if the
   field still lacks volume.

## 4. Tests

- `SectorShapeTests`: Cluster has comparable vertical and horizontal extent; the tilt is deterministic for
  a seed; Ring/Spiral still read as thin discs with a bulge; a larger `AnchorRadius` keeps every planet
  outside it.
- `PuzzleFieldTests`: the sun is in the baked sources exactly once and is never an endpoint; a zone over
  its centre is a candidate; a bake/session test that the path bends and the solution still arrives.

## 5. Out of scope

- Sun emission and lighting rigs, ongoing orbits, per-level dust, a halo or globular-cluster population,
  and planet rings ([their own plan](../backlog/space-shaping-planet-rings.md)).

## 6. Risks

- **Sun clearance.** If the sun's radius is not fed into the generator's anchor clearance, planets spawn
  inside it and the first separation pass moves baked positions. This is the quiet failure to guard.
- **Sun scale is a free win.** `mass ∝ radius³` means a 1.8x star is ~5.8x mass; the bake may find it
  trivial. Keep the fixed-mass flag ready.
- **Shared generator.** A 3D `SectorShape` changes the ambient sector field too unless the new knobs
  default to flat; the defaults are the guard.
- **Read.** The sun's marker must stay an annotation; the star may poke out of the cube (accept or tune),
  and a tilted field overlaps in the creator's top-down preview.
- **Field tilt.** A tilted disc can make the run harder to read and the cube gesture harder to aim; keep
  the tilt small, or gate it per level, until it proves out in the headset.
