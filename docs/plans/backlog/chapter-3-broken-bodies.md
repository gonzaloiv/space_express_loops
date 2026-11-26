# Broken bodies

**Status:** next · space shaping · updated 2026-11-26 — anchored planets and repulsors, end to end
(sim, lattice, look, bake, validator), with the repulsor gate on 1 Dec. Chapter 3's jobs are
[chapter 3](chapter-3.md).

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

- **Data, every hop.** No anchor field exists today. Add it to `PuzzleJobContract` (and `CreateSeed`),
  `PuzzleSeed`, `PuzzleBrief`, the field bake (which planet becomes `fixedMass`) and the validator;
  `PuzzleJobBakeTests.Brief` must carry it too (the 2026-11-20 seed retune lost fields this way).
- **A pinch on the anchor is a refused no-op.** Today it is a *paid* no-op: cost is `|scale − reference|`
  regardless of effect (`ZoneCommandLog`), `CommitScale` always commits and launches, and
  `SpaceZone.ApplyTo` skips `FixedMass`. Drop the command when the zone changes no scalable source and
  raise `Refused` with a **new `ZoneRefusal.NoEffect`** (appended at the end; `Locked` means a
  locked frame and would mislead). `PuzzleEditionState.OnRefused` cues only `OutOfEntropy` today: add
  a `NoEffect` case (a short refusal tick plus a pulse on the anchor's marker). The pulse needs a view
  entry point: `FieldPlanetSet` only exposes `PulseEndpoint(point, danger)`, so add `PulseSource(index)`
  delegating to the planet's existing `FieldPlanetBehaviour.PlayRefusal`. No spend, no launch.
- **Bake rule (one check, the `SunAnswers` pattern).** With the lock lifted, a cube on the anchor
  solves (one `SolvesInPlace` call), and the anchor sits on the chord: the anchor is the tempting
  answer, not decoration. The zone search already skips fixed sources, so expect fewer candidates near
  the anchor: check bake yield and `MinRoutes` early.
- **The look.** Planets are procedural (`NodeBody.Init(radius, seed)`), not prefab variants: add an
  authored marker child on the field planet prefab (a bolt/ring), toggled by the flag in `Spawn`, and
  reset in `Spawn` so a pooled planet never keeps the last job's look.
- Exit: a Creator test bakes an anchored field and asserts the rule; a fast test shows a pinch on an
  anchor-only zone is refused and spends nothing; the marker shows and resets in the editor.

### U2 — repulsors

- **Data and sim.** A `pushes` flag on `PuzzleSource`, copied through `SourceSpec` **including
  `SourceSpec.Scaled`** (it rebuilds the field on every zone edit; a missed copy turns the repulsor
  attractive mid-job). `SpaceShapingSession.BuildField` builds a negative `Strength` from it.
- **The lattice.** `WellPull.Toward` returns zero for `Strength <= 0` and takes `Sqrt(Strength)`:
  use `|Strength|` for the depth and reverse the direction for a repulsor. `LatticeWells.changeOnly`
  draws only the change from the start field, so a repulsor would show **no bump until the first
  pinch**: draw repulsors always (exempt from the change-only baseline) so the tell reads before the
  flight.
- **The role.** `PuzzleSolutionClassifier` calls a shrink `Clear` only for an obstacle from a crash or
  the chord; a repulsor off the chord makes a miss, so "shrink it" classifies as `Other` and fails the
  validator. Add a repulsor-aware rule: shrinking a repulsor is `Clear` when the **start run passes
  within its push's reach** (or the start run fails near it), not within its body radius:
  `NearAnyPath` measures `InitialScale * 0.5 + epsilon`, which an off-chord repulsor never meets.
  Define the reach from the repulsor's strength (the distance where its push falls below a share of
  the ship's other forces) and keep it in one place for the bake and the validator. Growing one is a
  shove (`Curve`, or a new kind if the headset says it reads differently).
- **The look.** A second authored marker on the planet prefab, same toggle/reset rule as U1.
- Exit: fast tests: a ship flying past a repulsor is pushed; editing a repulsor's zone keeps the
  push (the `Scaled` copy); `WellPullTests` gains a negative-strength case. A Creator test bakes a
  repulsor field classified `Clear`. The bump draws before the first pinch in the editor.

## Manual check (headset)

1. An anchored planet reads as locked; pinching it is refused, spends nothing and launches nothing.
2. A repulsor reads as different (look and raised lattice) before the ship flies.
3. Shrinking a repulsor opens the gap; growing one shoves the ship aside.
4. A pooled planet that was an anchor or a repulsor looks normal in the next job.
5. Frame rate holds with both bodies in the field.
