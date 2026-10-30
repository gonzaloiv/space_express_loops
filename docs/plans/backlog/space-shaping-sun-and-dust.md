# Space Shaping — the sun and the dust

**Status:** next · space shaping · updated 2026-10-30 — give the field a star with real mass and a
digital-noise dust layer, so it reads as a galaxy. The field-element ladder's tiers 2–3
([element pack](../../ideas/element-pack.md)).

**Related:** [the cube](../../design/systems/cube.md) · [presentation](../../design/presentation.md)

## 1. Why

The field is planets on a wireframe: it reads as a diagram, not a place. Two elements fix that, and they
belong together — the **sun** gives it mass and a light source, the **dust** gives it volume. The
three-ring sun graphic was tried on 2026-10-30 and removed: it read as another object among the planets.
The right sun is **real mass with a symbolic look** — a source, marked by a reference.

## 2. Slices

### S1 — the sun as a source

- The sun is a **source** at the field's centre, and the generator places the planets around it. Nothing
  else needs building: `BuildField` sums it, `TrajectoryIntegrator.Classify` makes it a crash sphere,
  `SourceConstraints.Separate` pushes the planets clear, and the bake already searches and verifies every
  source — so solutions route around the star and a scaleable star is considered, not a free win.
- Its look is a **reference, not a star**: a flat, diagrammatic marker in the readout's own language —
  unlit, thin, no volume — so it *annotates* a mass instead of pretending to be a body. The exact form
  (a reticle, a glyph, a labelled node) is a look-pass call. The removed rings failed because they read as
  *another object among the planets*; a reference must not.
- **Exit:** a level with a star — the path bends around it, the baked solution avoids it, the planets sit
  outside its separation radius, and the marker reads as an annotation rather than a body.

### S2 — the dust

- `LatticeDustView`: an authored voxel-speck mesh — positions quantized so they read as pixels, a minority
  smeared into short axis-aligned streaks — warped by the lattice warp delegate, the same shape as
  `SpacetimeMeshView.WarpVertices`.
- Its material comes from the vendor `Dissolve` / `TV Glitch` shadergraphs, which is where the
  digital-noise read comes from. Vendor assets are read-only; we author our own material.
- Placed per level at the puzzle's `zoneCentre`, so it does not drift as the frame moves; the speck
  placement is a fixed seed in the builder.
- **Exit:** tests pin the speck count, the volume bounds and seed determinism; in the headset the dust
  fills the field, warps with the cube, and never reads as a body.

### S3 — look pass

- Tune the star's radius and mass (a per-source mass multiplier if a readable size needs a stronger
  well), the dust's density, and both materials; measure the cost on Quest — shadergraph plus
  transparency is the overdraw risk.

## 3. Decisions

1. **The sun is a source first.** The mass is the real thing; the visual is only a reference marker laid
   over it. The removed ring graphic stays removed — it read as an object, and a marker must not.
2. **The sun is scaleable.** The cube scales any source and the bake already searches all of them, so this
   is a designed lever rather than a hole. If it proves a free win, revisit with a fixed-mass flag.
3. **Placement is the centre.** A star belongs at the centre of its system, so the field generator's
   shapes (Ring/Spiral/Cluster) wrap it — a generator change, not just a data point.
4. **Density is a dial.** `mass ∝ radius³` makes a big body light for its size; a per-source mass
   multiplier is the honest knob if the star must dominate.
5. **The dust's look rides the vendor shaders.** No new shader: the dissolve's noise amount can be
   animated, so the flicker is a look-pass knob rather than new code.
6. **Dust is weightless.** It is not a source and never joins `SourceConstraints.Separate` — that
   constraint pushes both bodies of a pair apart, so decor in that list would displace planets and could
   break a baked solution.

## 4. Tests

- `DustMeshTests`: speck count, volume bounds, seed determinism.
- A bake/session test that the sun is a crash sphere and that the baked solution still arrives.

## 5. Out of scope

- Sun emission and lighting rigs, ongoing orbits, per-level dust data, a fixed-mass rule.

## 6. Risks

- **Perf.** A shadergraph material plus transparency on a mobile VR GPU — measure both eyes.
- **Read.** The dust must not compete with the planets; the star must not become the puzzle's answer.
- **Generator.** A star at the centre reshapes the field shapes; this is the largest change in the plan.
