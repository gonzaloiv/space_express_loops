# Broken bodies

**Status:** next · space shaping · updated 2026-11-26 — anchored planets and repulsors, end to end
(sim, lattice, look, bake, validator), with the repulsor gate on 1 Dec. Chapter 3's jobs are
[chapter 3](chapter-3-jobs.md).

**Bakes:** off the editor — the U1 and U2 Creator tests bake fields
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [chapters](../../design/chapters.md#chapter-3--broken-bodies-store-release) ·
[the cube](../../design/systems/cube.md) · [glossary](../../design/glossary.md)

## Goal

Two bodies the player meets with the verbs they already have. **Anchored:** fused in place; the Wrap
cannot change it, so the line bends through its neighbours. **Repulsor:** pushes instead of pulling;
shrinking it opens a gap, growing it shoves a ship aside.

## Gate (1 Dec)

If repulsors do not fly, draw and bake by 1 Dec, chapter 3 ships with anchored jobs only, using **more
anchors per job** for variety, and repulsors move to an update. Not a second sun: one sun is assumed
across the validator (`SunIndex`, `CheckSources`' planet count) and the field (`FieldPlanetSet` holds
one `sunPlanet`), so two suns would be a third feature.

## Units

### U1 — anchored bodies

- **Data.** `fixedMass` already means immovable end to end (`PuzzleSource`, `SourceSpec`, `SourceSpecs`,
  `SpaceZone.ApplyTo`, `SourceConstraints`); only the job-level selection is missing. Add an `Anchors`
  count to `PuzzleJobContract` and `CreateSeed`, `PuzzleSeed` and `PuzzleBrief.From`. The bake tests'
  `Brief` helper derives from those, so it needs no separate change.
- **Bake rule (one check, the `SunAnswers` pattern).** The anchor must sit on the chord, so mark the
  chosen planets in `PuzzleFieldBake.Apply` after the run pick and before `BakeField`, so the zone
  search sees them. With the lock lifted, a cube on the anchor solves: `PuzzleStartFrame.SolvesInPlace`
  skips `FixedMass`, so run it against an unlocked source copy, not the baked puzzle. The zone search
  already skips fixed sources, so the anchor answer never appears among candidates: expect fewer
  centres near the anchor and check bake yield and `MinRoutes` early.
- **A pinch on the anchor is a refused no-op.** Today it is a *paid* no-op: cost is
  `|scale − reference|` regardless of effect (`ZoneCommandLog`), `CommitScale` always commits and
  launches, and `SpaceZone.ApplyTo` skips `FixedMass`. Put the "the zone changes no scalable source"
  predicate on `SpaceShapingSession` before `CommitPreview` so it is testable; when it fails, drop the
  command, raise `ZoneRefusal.NoEffect` (appended at the end; `Locked` means a locked frame and would
  mislead), spend nothing, launch nothing, and re-render the committed field so the stopped preview
  does not stay on screen. The rule also covers a non-scalable sun, which is already `fixedMass`.
- **The pulse.** `Refused` carries only the reason, so the controller (which holds the run) raises the
  pulse: add `FieldPlanetSet.PulseSource(index)` delegating to the planet's existing
  `FieldPlanetBehaviour.PlayRefusal`. The planet pool is ordered by spawn slot, not source index, so
  match on `FieldPlanetBehaviour.SourceIndex`. `PuzzleEditionState.OnRefused` cues only `OutOfEntropy`
  today: add a `NoEffect` case (a short refusal tick; the pulse comes from the controller).
- **The validator.** Add an anchor rule to `CheckSources`: the anchored planet count matches the
  contract and anchors are not ship endpoints. `SunIndex` matches `centreOnly`, so anchors cannot be
  mistaken for the sun.
- **The look.** Planets are procedural (`NodeBody.Init(radius, seed)`), not prefab variants: add an
  authored marker child on the field planet prefab, toggled by the flag in `Spawn` and reset in `Spawn`
  so a pooled planet never keeps the last job's look.
- Exit: a Creator test bakes an anchored field and asserts the rule; a fast test shows a pinch on an
  anchor-only zone is refused and spends nothing; the marker shows and resets in the editor.

### U2 — repulsors

- **Data and sim.** Add a `Pushes` count to `PuzzleJobContract` and `CreateSeed`, `PuzzleSeed` and
  `PuzzleBrief.From` as well as the `pushes` flag on `PuzzleSource` — without the job-level knob the
  bake cannot be asked for a repulsor. Copy it at every hop: `SourceSpecs.From`, `SourceSpec` and
  `SourceSpec.Scaled` (which rebuilds the field on every zone edit; a missed copy turns the repulsor
  attractive mid-job). `SpaceShapingSession.BuildField` builds a negative `Strength` from it;
  `SpacetimeField` already repels on a negative strength.
- **The lattice.** `WellPull.Toward` already returns zero for `Strength <= 0` and uses
  `Sqrt(Strength)`: use `|Strength|` for the depth and reverse the direction for a repulsor.
  `LatticeWells.changeOnly` draws only the change from the start field, so a repulsor would show **no
  bump until the first pinch**: draw repulsors always by zeroing them in both the drawn and the
  baseline copies (the baseline is also copied at rest and on a fresh baseline) so the tell reads
  before the flight.
- **The role.** `PuzzleSolutionClassifier` calls a shrink `Clear` only for an obstacle from a crash or
  the chord; a repulsor off the chord makes a miss, so "shrink it" classifies as `Other` and fails the
  validator. Add the repulsor rule in `Obstacles`, which already receives the start run: a shrink is
  `Clear` when the start run passes within the repulsor's push reach (or fails near it), not within
  its body radius — `NearAnyPath` measures `InitialScale * 0.5 + epsilon`, which an off-chord repulsor
  never meets. Define the reach from the repulsor's strength (the distance where its push falls below
  a share of the ship's other forces) and keep it in one place for the bake and the validator. Growing
  one is a shove and already classifies as `Curve`.
- **The look.** A second authored marker on the planet prefab, same toggle/reset rule as U1.
- Exit: fast tests: a ship flying past a repulsor is pushed; editing a repulsor's zone keeps the
  push (the `Scaled` copy); `WellPullTests` gains a negative-strength case. A Creator test bakes a
  repulsor field (through the new contract/seed knob) classified `Clear`. The bump draws before the
  first pinch in the editor.

## Manual check (headset)

1. An anchored planet reads as locked; pinching it is refused, spends nothing and launches nothing.
2. A repulsor reads as different (look and raised lattice) before the ship flies.
3. Shrinking a repulsor opens the gap; growing one shoves the ship aside.
4. A pooled planet that was an anchor or a repulsor looks normal in the next job.
5. Frame rate holds with both bodies in the field.
