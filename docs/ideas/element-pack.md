# Field elements — making the field read like a galaxy

**Status:** idea · space shaping · updated 2026-11-26 — what is left of the element pack after the
store release. Not scheduled.

**Related:** [the cube](../design/systems/cube.md) · [jobs](../design/jobs.md) ·
[later chapters](later-chapters.md) (debris, broken bodies)

## Already in

More and smaller planets (just `PuzzleSource` entries), the sun as a real source at the system's centre
(`hasSun`, `fixedMass` as a per-level lock), and seeded dust drawn through the lattice warp
(`DustLayout`, `FieldDust`). Anchored bodies and repulsors are chapter 3
([broken bodies](../plans/backlog/chapter-3-broken-bodies.md)).

## Still ideas

- **Decor moons.** A small body that orbits a planet, shaped by the cube like any planet but with **no
  gravity**: a `kind` flag (source | decor) so `BuildField` skips it, a parent link to its planet, an
  orbit that is a pure function of the level clock, and its own prototype in the view. It is decor by
  necessity: see the rules below.
- **Later systems, for the field or the sector backdrop:** an asteroid belt, a nebula (billboards or
  particles, not volumetric), a station, a comet, a gate, a black hole. Each is a system, not a body, and
  needs a perf budget and a decor-or-gameplay call before art.
- **Edge suns — a recurring field variant.** Not a chapter: once introduced, some systems in any
  chapter have their star at the edge, level with the planets, instead of at the centre. The system
  reads differently and the sun is rarely on the ships' path, which varies the field without new
  physics. 02_05's first bake showed the look (2026-11-26): not ideal for a first read, much better than
  a sun above the field. Needs `PuzzleSunPlacementChoice.TryHome` limited to the planets' height band (no
  pole directions), one job and a line that introduce it, and a rule for how often it recurs. Also the
  chapter 2 fallback if a job cannot bake with a centred sun
  ([fleet rebake](../plans/in-progress/fleet-rebake.md) U4).

## Rules any element must keep

- **The player can tell what bends the path.** Decor must look weightless (material, scale, no well
  glow), or the field stops being readable at a glance.
- **A moving body cannot pull the ship.** The flight is integrated over one static field, so an
  orbiting well would make the prediction time-dependent: new physics, not data.
- **Decor stays out of planet separation.** `SourceConstraints.Separate` pushes both bodies of a pair,
  so a decor body in that list would move real planets and could break a baked solution.
- **A belt is not "more planets".** Many tiny sources fight the separation solver; it is a system.
- **Baked and authored.** Everything ships in the puzzle data and relaunches identically; visuals are
  authored prefabs, never created at runtime.
