# Space Shaping — a more realistic field

**Status:** now · space shaping · updated 2026-11-02 — S1, S2, S2.5 and S3 are in. Next: S4 — the headset
read and cost pass. Field elements from the [element pack](../../ideas/element-pack.md).

**Related:** [the cube](../../design/systems/cube.md)

## 1. Why

The field is planets on a wireframe: a thin horizontal disc with no centre and no scale cues. It reads as
a diagram, not a place. Realism here is **structure, not decoration** — a galaxy has a massive centre and
a shape (a disc, a bulge, a halo or an ellipsoid), and those are what the eye reads as *space*. Two moves
give the field that structure:

- **depth** — a disc with a bulge, a true 3D cluster, and a small tilt stop the field reading as one flat
  sheet;
- **mass** — the sun is a source at the centre, so the path bends around it.

The dust is out of this plan: it was the biggest perf and read risk (transparency overdraw, decor mistaken
for a source). Planet rings are out for now too: their plan was removed, so rings wait until the current
field passes the headset. The three-ring sun graphic was tried on 2026-10-30 and removed: it read as
another object among the planets. The right sun is **real mass with a symbolic look** — a source, marked by
a reference.

## 2. Slices

### S1 — a field with depth

**State: done.** `SectorShapeData` gained `tilt`/`bulge` (both default 0), `SectorShape` bulges the
innermost ring and the spiral's inner half, samples Cluster on a sphere when `bulge > 0`, and rotates the
finished positions by a seeded pitch/yaw/roll. `PuzzleFieldBuilder` is the only opt-in, and the depth knobs
live in `SpaceShapingSettings` (`fieldThickness` 0.08, `fieldTilt` 12°, `fieldBulge` 0.8) so the ambient
sector field keeps flat defaults and a look iteration needs no recompile. The layout's rotation is returned
but not consumed, so planets are tilted and the cube stays world-axis-aligned. Covered by
`SectorShapeTests`. The anchor now comes from the seed's `hasSun` gate over the settings radius (S2.5).

**Open:** with a single ring (`ringCount == 1`) `FillRing` bulges that ring at the outer radius, so a small
field domes its rim instead of its centre; the test only checks `maxAbsY`, so it passes either way. Force
≥2 rings or split the single-ring case before calling the read proven.

- **The field was one thin horizontal disc before S1.** `SectorShape` spreads X/Z across the radius and uses Y
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
- **The sun's radius is an S1 input.** The generator's anchor clearance is the sun's radius, taken from the
  settings and gated by the seed's `hasSun` (S2.5). Otherwise the discs are tuned against a planet-sized
  hole and every baked level moves when the sun lands.
- **Exit:** a level has a 3D centre, Cluster has comparable vertical and horizontal extent, the disc is
  tilted off the XZ plane, and the run and cube still read in the headset.

### S2 — the sun as a source

**State: done.** `PuzzleFieldBake.WithSun` appends one `Sun` source at the origin (`centreOnly`, fixed or
scalable), `Sun.prefab` is authored and wired as `sunPrototype`, the builder feeds the sun radius into
`anchorRadius`, and `PuzzleRunChoice.RunIsBlocked` blocks the star's band. The seed decides presence and
the lock; the radius comes from the settings.

- **One sun per puzzle, created with the field.** It is a `PuzzleFieldPlanet`/`PuzzleSource` entry flagged
  as the sun; `PuzzleFieldBuilder`/`PuzzleFieldGenerator` create it at the centre. The seed decides
  `hasSun` and the lock (S2.5); the settings own the radius. Nothing is pooled from another level.
- **The sun's radius is the generator's anchor clearance.** Feed it into the layout's `AnchorRadius` so the
  first `SourceConstraints.Separate` pass never moves a baked planet. Before S2, `PuzzleFieldBuilder`
  hard-coded `anchorScale = 1`, which was only a planet-sized hole.
- **Scalable by its centre first.** The sun is a source; by default a cube containing its centre scales
  it. `fixedMass` is the per-puzzle lock and the fallback if the bake finds scaling a free win. Shipped:
  `PuzzleSeed.sunScalable` defaults true, and the seed can lock it.
- Its look is a **reference, not a body**: flat and unlit in this pass, so it *annotates* a mass instead
  of pretending to be a star. A dedicated `Sun.prefab` (a planet copy with its own `Sun.mat`, wired as
  `sunPrototype`) keeps the colour tunable without touching planets. S4 gives it a controlled corona — a
  glow, not a bigger planet; the removed rings failed because they read as *another object among the
  planets*, and a glow must not.
- **The sun is never a run endpoint.** It is appended to `puzzle.sources` outside the endpoint list, and
  `PuzzleRunChoice` runs on the planet list, so the sun is not in the endpoint pairs or the cube range.
- It sits in the bulge from S1, so the centre already has depth before the mass is added.
- **Exit:** a level with a star — the path bends around it, the baked solution avoids it, the planets sit
  outside its separation radius, a cube over its centre scales it (a locked star holds its size), and it
  reads as a real mass rather than another planet.

### S2.5 — per-seed sun control

**State: done.** `PuzzleSeed` owns `hasSun` (bake it or not) and `sunScalable` (the lock); the radius
lives once in `SpaceShapingSettings`. `PuzzleBrief.From` resolves presence into a radius (`hasSun ?
settings.sunRadius : 0`), so `PuzzleFieldBake` and `PuzzleFieldBuilder` share one rule: no sun means no
anchor hole, a sun means the hole is exactly its radius. `SpaceShapingSettings.sunScalable` is gone — the
lock is per seed. The seed asset is migrated to `hasSun` on, scalable.

- **The seed owns presence, the settings own size.** `PuzzleFieldBake.WithSun` reads the resolved radius
  from the brief instead of the settings, so job 1 bakes no sun and job 2 bakes one at the shared radius.
- **The lock stays per seed.** `sunScalable` (default true) is the per-puzzle lock; it is the only sun
  value a seed carries besides `hasSun`.
- **The run needs it.** Job 1 has no sun; job 2 puts the sun over the centre; later jobs can lock it. The
  radius is a field-scale value, so it stays global with the rest of the field settings.
- **Exit:** `hasSun` off bakes no sun and leaves no anchor hole; on bakes exactly one at the settings
  radius; `sunScalable` on/off changes the cube's ability to scale it.

### S3 — a scalable sun

**State: done.** Centre containment shipped as `centreOnly` rather than `enclosureRequired`:
`PuzzleZoneCandidates.CoversSource` needs enclosure only for planets and plain containment for the sun,
and `Centres`/pair midpoints skip a fixed source. `SourceSpecs.From` pins a fixed source's min/max and
gives a scalable one the normal factors. The per-seed toggle landed in S2.5, and the runtime lock is real:
`FixedMass` travels on `SourceSpec`, `SpaceZone.ApplyTo` skips a fixed source, and separation pushes only
the other body. The look pass that proves the read is S4.

- **Toggle per seed.** `sunScalable` on `PuzzleSeed` defaults **true** in the design.
  `PuzzleFieldBake.WithSun` sets `fixedMass = !seed.sunScalable`; a seed can lock the sun. The radius is
  the settings' single value.
- **Centre containment, not enclosure.** Give the baked source an `enclosureRequired` flag (planet vs
  sun) — shipped as `centreOnly`. `PuzzleZoneCandidates.CoversSource` requires enclosure for planets and
  only `zone.Contains(home)` for the sun; `Centres` includes the sun's home when scalable and skips it
  when fixed. The cube stays planet-sized, so the bake search gains one centre, not a star-sized range.
- **Scale bounds.** `SourceSpecs.From` gives a scalable sun the normal min/max factors; the fixed path
  keeps `MinScale == MaxScale`.
- **Run choice.** The sun is still never an endpoint. If a run through the centre should count as blocked,
  pass the sun radius into `PuzzleRunChoice.RunIsBlocked`; otherwise the planet-only blocker rule stays.
  Shipped: the radius is passed and the star's band blocks a run.
- **The lock holds at runtime.** `FixedMass` travels on `SourceSpec`; `SpaceZone.ApplyTo` skips a fixed
  source, and `SourceConstraints.Separate` pushes only the other body, so a locked sun keeps its size and
  its centre while the planets it meets move out. `SourceSpecTests` covers both.
- **View.** `SpaceShapingSpawner` already spawns the sun through `sunPrototype`; a scalable sun scales
  through `FieldPlanetBehaviour`/`NodeBody` like a planet, so only the spawn route stays separate.
- **Exit:** a cube over the sun's centre changes its mass without a star-sized cube; the bake still
  verifies; a fixed sun still clamps; the search stays in budget.

### S4 — look pass

**State: in progress.** The corona is authored and wired on `Sun.prefab`: a sphere child (no collider,
renderer on for the authored preview; `SunBehaviour` turns it off until the sun is visible) with
`SunCorona.mat`, and `SunBehaviour` on the root driving its size, brightness and visibility. Tuning lives
on the component (`coronaRadiusFactor`, `maxCoronaDiameter`, `baseIntensity`, `intensityBoost`) and the
material (`_CoronaColor`, `_RimPower`, `_RimBias`); the depth knobs are live in the settings. Remaining:
the headset read, the tuning that follows it, and the Quest cost measurement.

- **A corona, not a bigger body.** Keep the physical radius and add an authored additive **rim sphere** on
  `Sun.prefab` (no billboard, so it is stereo-safe) at `coronaRadiusFactor` × the radius: warm white,
  radial rim falloff, no collider, renderer off in the prefab. The star reads big without touching
  `CaptureRadius`, the field radius or the hand. The removed three-ring graphic stays removed — this is a
  glow, not an object.
- **One behaviour owns it.** `SunBehaviour` on `Sun.prefab` reads `FieldPlanetBehaviour.Radius`, caps the
  corona's **world** diameter at `maxCoronaDiameter` and drives `_Intensity` with a
  `MaterialPropertyBlock` — never `renderer.material`, which instantiates. `NodeBody` scales the sun's
  **root**, not the body mesh, so the corona (a child) inherits that scale: the behaviour divides the
  wanted world diameter by the radius to get the local scale. It also owns visibility through
  `FieldPlanetBehaviour.IsVisible`: `NodeBody.SetVisibility` toggles only the active variant renderer, so a
  corona child would otherwise survive `Initialize`.
- **Brightness first when it scales.** The change is carried in emission intensity, so the star brightens
  rather than swallowing the innermost planets. The cube still gates on `centreOnly`, so the corona never
  changes the hit area; a fixed sun keeps its corona unchanged.
- **Leave the scale range alone.** The normal 1.8× stays, and the sizes stay as they are; the corona is
  presentation and adds no rule. Revisit a narrower range only if the bake measures the star as a free win.
- **Mass is the only strength knob**, and only if the headset says the star does not dominate: tune the
  per-source mass (density) and let the bake re-verify. No size change.
- **Guards.** The corona must not occlude the innermost planets, hide the trajectory, or read as a
  scalable surface; one transparent layer; deterministic on relaunch.
- **Exit:** at a glance the star is the centre; the corona never swallows a planet or the path; scaling
  brightens it without a giant bloom; a locked sun holds; the cost is measured.

## 3. Decisions

1. **Structure is the realism, and it comes first.** Dimensionality follows the shape: Ring/Spiral are thin
   discs with a central bulge, Cluster is a 3D ellipsoid, and the layout may be tilted off the XZ plane. A
   galaxy is a disc with a spheroidal centre and a halo, not one flat layer.
2. **One sun per seed, sized before the planets.** The seed owns whether the sun exists (`hasSun`) and
   whether it is locked (`sunScalable`); the settings own every body size — the sun's radius and the
   planets' range. That radius sets the generator's anchor clearance so no baked planet ever moves in the
   first separation pass, and the planets stay outside it.
3. **The sun is scalable by default; a seed can lock it.** `PuzzleSeed.sunScalable` (default true) turns on
   centre containment (a cube over the sun's centre scales it; only planets must be enclosed). Fixed mass
   is the per-seed lock and the fallback if the bake finds scaling a free win. Scaling stays: it is job 2's
   reveal and job 7's capstone, and the corona brightens with the mass rather than growing to swallow the
   field.
4. **The sun is a source first.** The mass is the real thing; the visual is only a reference marker laid
   over it. The first pass uses a dedicated `Sun.prefab`/`Sun.mat`; the marker is the look pass, and the
   marker is a **corona** around the mass, not a bigger mass. The removed sun ring graphic stays removed —
   it read as an object beside the mass.
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
10. **Ergonomics beats literal scale.** The field is a toy orrery, not a scale model: body sizes are chosen
    for the hand and the field, so the star does not need a galaxy's radius ratio. It reads as the centre
    through position, mass and material, not width.

### Seed vs settings

The seed is the recipe; the settings are the field's scale. This is the ownership the slices above imply,
field by field:

| Field | Lives in | Role |
|---|---|---|
| `seed` | seed | Bake search input; not a design value |
| `planetCount` | seed | Sources in the field |
| `fieldShape` | seed | Ring/Spiral/Cluster; `Auto` is for experiments |
| `hasSun`, `sunScalable` | seed | Sun presence and lock; the radius is not here |
| `translatable`, `zones`, `ships`, `relation`, `resizable` | seed | The brief; `relation` only when `ships > 1` |
| `sunRadius`, `planetRadiusMin/Max` | settings | One size scale for every puzzle |
| `fieldThickness`, `fieldTilt`, `fieldBulge` | settings | The field's depth; the look pass tunes these live |
| `runDistance`, `separationScale`, `endpointClearance`, ... | settings | The rest of the field's scale |
| `entropyBudget`, `routes`, `sources`, `frames` | baked field | Outputs; never authored |

**No seed carries a size.** A job that needs a different star or planet size is a settings change, not a
seed value — that is what keeps a planet the same size in every puzzle.

## 4. Tests

- `SectorShapeTests` (done): Cluster has comparable vertical and horizontal extent; the tilt is
  deterministic for a seed; Ring/Spiral still read as thin discs with a bulge; a larger `AnchorRadius`
  keeps every planet outside it. The tilt test asserts determinism and a non-identity rotation, not the
  plane normal — the tilted read still needs a headset/creator check.
- `SourceSpecTests` (done): a fixed source pins min and max to its initial scale; a scalable sun gets the
  normal factors; a fixed source is skipped by a zone and never moved by separation.
- `PuzzleSeed`/`PuzzleFieldTests` (done): `hasSun` off bakes no sun and leaves no anchor hole; on bakes
  exactly one at the settings radius; `sunScalable` off pins the sun, on gives it the normal factors; the
  builder passes the settings' depth knobs.
- `PuzzleFieldTests`: the sun is in the baked sources exactly once and is never an endpoint; a zone over
  its centre is not a candidate while fixed, but is one when scalable; a zone containing a planet and the
  sun still needs only the planet's enclosure; a bake/session test that the path bends and the solution
  still arrives.

## 5. Out of scope

- Screen-space flares, point lights and lighting rigs, ongoing orbits, per-level dust, a
  globular-cluster population, and planet rings (deferred until the current field passes the headset).

## 6. Risks

- **Sun clearance.** If the sun's radius is not fed into the generator's anchor clearance, planets spawn
  inside it and the first separation pass moves baked positions. This is the quiet failure to guard.
- **One global sun size.** The radius is shared by every level; a job that needs a different star size
  would need the seed to own the radius again. Field scale is global too, so this is deliberate, not a
  gap — revisit only if a job's read demands its own star.
- **Sun scale is a free win.** `mass ∝ radius³` means a 1.8x star is ~5.8x mass; the bake may find it
  trivial. The design default is scalable; the bake must verify it does not trivialize the level. If it
  does, lock the sun per puzzle rather than changing the default.
- **Search cost.** Adding the sun's centre to the zone candidates is one extra centre, not a new size
  range; measure the bake.
- **Shared generator.** A 3D `SectorShape` changes the ambient sector field too unless the new knobs
  default to flat; the defaults are the guard.
- **Read.** The first pass shows the sun through `Sun.prefab` with a flat colour; the marker is the look
  pass. A tilted field also overlaps in the creator's top-down preview.
- **Field tilt.** A tilted disc can make the run harder to read and the cube gesture harder to aim; keep
  the tilt small, or gate it per level, until it proves out in the headset.
