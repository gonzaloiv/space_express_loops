# Chapter 3 — repulsors

**Status:** next · space shaping · updated 2026-11-27 — single-ship repulsors end to end (sim,
lattice, look, role, search tie, validator); this plan is the 1 Dec gate. After
[anchors](chapter-3-anchors.md) (it extends `PuzzleBrokenBodyChoice` and the look struct). Fleets are
[fleet bodies](chapter-3-fleet-bodies.md).

**Bakes:** off the editor — the exit's Creator tests bake fields
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) ·
[chapters](../../design/chapters.md#chapter-3--broken-bodies-store-release) ·
[chapter 3](chapter-3-jobs.md) · [the cube](../../design/systems/cube.md)

## Goal

**Repulsor:** pushes instead of pulling; shrinking it opens a gap, growing it shoves a ship aside.

## Gate (1 Dec)

A repulsor job must bake through the new knob, classify `Clear`, draw its bump before the first pinch
in the editor, and read as a shrink/grow on device. If that does not hold, chapter 3 ships with
anchored jobs only, using **more anchors per job** for variety, and repulsors move to an update. Not a
second sun: one sun is assumed across the validator (`SunIndex`, `CheckSources`' planet count) and the
field (`FieldPlanetSet` holds one `sunPlanet`), so two suns would be a third feature.

## Units

### U1 — data, sim and lattice

- **Data and sim.** Add a `Pushes` count to `PuzzleJobContract`, `CreateSeed`, `PuzzleSeed` and
  `PuzzleBrief.From`, and `pushes` to `PuzzleSource`. Copy it at every hop: `SourceSpecs.From`,
  `SourceSpec` and `SourceSpec.Scaled` (which rebuilds the field on every zone edit; a missed copy
  turns the repulsor attractive mid-job). `PuzzleSourceBridge` needs no change; the bake marks pushes
  after the bridge. `SpaceShapingSession.BuildField` builds a negative `Strength` from it;
  `SpacetimeField` already repels on a negative strength.
- **The lattice.** `WellPull.Toward` returns zero for `Strength <= 0` and uses `Sqrt(Strength)`: use
  `|Strength|` for the depth and reverse the direction for a repulsor. `LatticeWells.changeOnly`
  draws `drawn − baseline`, so zero repulsors in the **baseline only** (both baseline writes: at rest
  and on a fresh baseline), never in the drawn copy; otherwise the repulsor shows no bump until the
  first pinch. `LatticeWells.Fill` reads `session.Pushes(i)`.
- **The look.** `SpaceShapingSession.Pushes(i)`; a push flag in `FieldPlanetLook` and a second marker
  in the anchor/push mark lists, same toggle/reset rule as anchors.
- Exit: fast tests — a ship flying past a repulsor is pushed; editing a repulsor's zone keeps the push
  (the `Scaled` copy); `WellPullTests` gains a negative-strength case; the bump draws before the first
  pinch in the editor.

### U2 — choice, role and the search tie

- **Selection and reach.** Extend `PuzzleBrokenBodyChoice`: choose `Pushes` planets that are off the
  run's chord (distance > body radius × `PlanetBlockBandFactor`), **within** `RepulsorReach.Radius`
  of the run, not endpoints, the sun, or anchors; deterministic; marked in `Apply` with the anchors.
  Define one `RepulsorReach.Radius(SourceSpec) = PushReachFactor × InitialScale × 0.5f`,
  `PushReachFactor` starting at 2.5 (with `PlanetBlockBandFactor = 2` that is only a 0.5 × radius
  annulus; expect 3–3.5), shared by the bake and the validator and tuned on device. If a seed cannot
  fill the annulus, raise `PushReachFactor` before weakening "off the chord" — otherwise the repulsor
  becomes a chord blocker and teaches nothing.
- **The role.** `PuzzleSolutionClassifier` calls a shrink `Clear` for an obstacle from a crash or the
  chord, or a source near the path; a repulsor off the chord makes a miss. Add the repulsor rule in
  `Obstacles`: a `Pushes` source is an obstacle when the run chord or the start path passes within
  `RepulsorReach.Radius` (the chord is the primary measure). Do not use `NearAnyPath`'s
  `InitialScale * 0.5` body radius for repulsors. Keep the general `Curve` rule at the classifier (any
  grown source is a shove); the search tie below makes the repulsor the lesson.
- **The lesson is the pushed body.** Mark pushes before the zone search. For a contract with
  `Pushes > 0` and a non-`Any` mechanism, accept only a solution with **at least one** zone covering a
  `pushes` source (a multi-zone `Mixed` may put a normal planet in the other zone). Carry a
  `RequiresPushes` flag from the original brief into `PuzzleSolutionContext` (the internal `any` copy
  in `PuzzleSolutionSelection` keeps it), and apply it in `PuzzleSolutionSearch` (`TrySelect`,
  `TryFindAtWidth`/`HasShorter`, `TryFindAtFrame`). `Any` is the exception, but its repulsor still has
  to be within reach. A seed whose only `Curve` is a normal planet is rejected, not baked as 3-4.
- **The validator.** `CheckSources`: `pushes` count matches `contract.Pushes`; a repulsor is not
  `fixedMass`, `centreOnly`, or an endpoint, is off every stored ship's chord, within
  `RepulsorReach.Radius` of at least one stored run, and disjoint from anchors. At least one stored
  answer zone covers a `pushes` source unless `Mechanism == Any`. Checked by
  `ShippedJobs_MeetTheContract` too.
- Exit: fast tests — off-chord, in-reach selection is deterministic; a repulsor out of reach and an
  answer that misses the pushed body are rejected. Creator tests: a repulsor field baked through the
  contract/seed knob classifies `Clear` and its answer covers the repulsor; a `Curve` seed whose grown
  source is a normal planet is rejected. The gate is then read on device.

## Manual check (headset)

1. A repulsor reads as different (look and raised lattice) before the ship flies.
2. Shrinking a repulsor opens the gap; growing one shoves the ship aside.
3. The repulsor jobs' answers edit the repulsor itself, not a neighbouring planet.
4. A pooled planet that was a repulsor looks normal in the next job.
5. Tune `PushReachFactor` and the repulsor strength on device; record the values.
6. Frame rate holds with a repulsor and an anchor in the field.
