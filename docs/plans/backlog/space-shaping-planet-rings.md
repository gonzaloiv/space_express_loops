# Space Shaping — rings on some planets

**Status:** next · space shaping · updated 2026-10-30 — rings on a subset of planets, authored on the
prototype and chosen at puzzle creation. Split out of
[a more realistic field](../in-progress/space-shaping-realistic-field.md), which lands the field's depth
first.

**Related:** [the cube](../../design/systems/cube.md) · [presentation](../../design/presentation.md)

## 1. Why

Rings are the cheapest scale cue: they give a planet character without adding a body, a mass, or a light.
They sit on a real planet, so unlike dust they cannot be mistaken for another well. They are a separate
plan so the field's structure (disc, bulge, tilt) lands first and the rings land on a field worth dressing.

## 2. Slices

### S1 — rings on some planets

- Rings are **authored children of the planet prototype**, not runtime geometry. One ring child under the
  `Body` in `FieldPlanet.prefab`, authored so it clears the surface at the planet's base scale; the view
  turns it on for the chosen planets. No mesh, material, or hierarchy is created in code.
- **Rings are assigned at puzzle creation, not at load.** The creator's planet list carries a ring flag
  (`PuzzlePlanetListAsset` → `PuzzleFieldPlanet`); `PuzzleFieldGenerator` sets it deterministically from
  the field seed, or the creator window toggles it per planet. `PuzzleSourceBridge.ToSources` copies it
  into `PuzzleSource`, and `PuzzleFieldBake.Apply` writes it into the baked `PuzzleData` with the rest of
  the sources. The runtime only reads the baked flag, so a relaunch draws the same rings at the same
  planets.
- **It follows its planet.** The ring scales with the body (`NodeBody.SetRadius` scales only the `body`
  transform today) and hides with it (`NodeBody.SetVisible` only toggles the active renderer). Origin and
  destination derive from `FieldPlanet.prefab`, so their ring override is removed: those are markers, not
  field bodies.
- **Keep the ring out of the outline hierarchy.** `Outline` caches only the children active at `Awake`, so
  a ring authored under the outlined body will not get a consistent outline. Author it as a sibling, or
  accept a neutral ring.
- **Exit:** a baked level shows rings on the chosen subset, identical on relaunch; rings scale and hide
  with their planet; origin and destination markers have no ring; a test pins the subset selection and the
  visibility pairing.

### S2 — look pass

- Tune the share of planets with rings, the ring radius and thickness, and the material; measure the cost
  on Quest — transparency overdraw is the risk.

## 3. Decisions

1. **A ring is attached decor, not a new field element.** It never joins `SourceConstraints`, never enters
   `SpacetimeField`, and never changes the bake. It is shaped with its planet like any other child.
2. **Ring choice is made at creation and baked, not rolled at load.** It travels the creator path
   (`PuzzleFieldPlanet` → `PuzzleSource` → `PuzzleData`) with the sources, so a level relaunches
   identically; the element pack's no-runtime-randomization rule holds.
3. **Some planets, not all.** Variety is the point; a ring on every planet reads as a new body type, not
   as character.

## 4. Tests

- `RingSelectionTests`: `PuzzleFieldGenerator` picks the same rings for the same seed; `PuzzleSourceBridge`
  and `PuzzleFieldBake` carry the flag into `PuzzleData.sources`; ring visibility follows the planet's
  `SetVisible`; origin and destination never show a ring.

## 5. Out of scope

- Ring shadows, ring collision, ring variants on the route game's `Body.prefab`, and moons or other decor
  bodies.

## 6. Risks

- **Perf.** A transparent ring material on a mobile VR GPU — measure both eyes.
- **Read.** A ring must not make a planet look like a different kind of body; it is scale and character,
  not a second well.
- **Prefab inheritance.** Origin and destination inherit `FieldPlanet.prefab`; remove the ring override in
  both or the markers grow rings.
- **Outline.** The cached active-child set at `Awake` means an outlined ring will be inconsistent; prefer
  a sibling root.
