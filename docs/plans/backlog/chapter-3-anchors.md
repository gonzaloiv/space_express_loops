# Chapter 3 — anchors

**Status:** next · space shaping · updated 2026-11-27 — anchored planets for single-ship jobs: the
contract knob, the choice, the validator and the look. First of four broken-body plans: anchors →
[pinch refusal](chapter-3-pinch-refusal.md) (in parallel) → [repulsors](chapter-3-repulsors.md) (the
1 Dec gate) → [fleet bodies](chapter-3-fleet-bodies.md). The jobs are [chapter 3](chapter-3-jobs.md).

**Bakes:** off the editor — the exit's Creator test bakes a field
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) ·
[chapters](../../design/chapters.md#chapter-3--broken-bodies-store-release) ·
[the cube](../../design/systems/cube.md) · [glossary](../../design/glossary.md)

## Goal

Two bodies the player meets with the verbs they already have; this plan lands the first.
**Anchored:** fused in place; the Wrap cannot change it, so the line bends through its neighbours.
Single-ship only; fleets get anchors in [fleet bodies](chapter-3-fleet-bodies.md).

## Units

### U1 — data and choice

- **Data.** `fixedMass` already means immovable end to end (`PuzzleSource`, `SourceSpec`,
  `SourceSpecs`, `SpaceZone.ApplyTo`, `SourceConstraints`); only the job-level selection is missing.
  Add an `Anchors` count to `PuzzleJobContract` and `CreateSeed`, `PuzzleSeed` and `PuzzleBrief.From`.
  `PuzzleJobSolver.Brief` and `PuzzleTestSetBuilder` derive from `CreateSeed`, so they need no
  separate change; tests that build a `PuzzleBrief` directly set the field explicitly.
- **Selection.** Add one pure `PuzzleBrokenBodyChoice` helper that takes the field's `PuzzleSource[]`
  and the chosen runs, and marks `fixedMass` (repulsors extend it later). An anchor must sit on the
  chord: distance to a run segment < body radius × `PuzzleRunChoice.PlanetBlockBandFactor`, not an
  endpoint, not the sun; the order is deterministic. `PuzzleFieldBake.Apply` marks bodies **only when
  `!brief.IsMulti`**: a fleet's `PuzzleMultiShipSearch.Prepare` calls `Apply` too, and its first pass
  must stay pristine (a fleet `PuzzleSeedField` is therefore body-free; pass an explicit flag if that
  preview needs them). A fleet contract with `Anchors > 0` fails as unsupported until
  [fleet bodies](chapter-3-fleet-bodies.md) lands.
- **Bake rule.** The on-chord check is geometric, shared with the validator (`IsOnChord`), so it
  works for any anchor count. Do not use `PuzzleStartFrame.SolvesInPlace` for this:
  `SpaceZone.ApplyTo` skips fixed sources, so a frame over an anchor changes nothing and the test
  cannot see it.
- Exit: fast tests for the choice (on-chord, not an endpoint or the sun, deterministic); a Creator test
  bakes an anchored single-ship field.

### U2 — validator and look

- **The validator.** `CheckSources` gains: the anchored planet count matches `contract.Anchors`;
  anchors are `fixedMass`, not `centreOnly`, not a ship endpoint, and lie on at least one stored
  ship's chord (the primary run for a single ship). Anchors count as blockers — `PuzzleRunBlockers`
  stays unchanged; the bake must still find an answer with a scalable planet. The rule is checked by
  `ShippedJobs_MeetTheContract` too.
- **The look.** `SpaceShapingSession` exposes `IsFixed(i)`. `FieldPlanetBehaviour.Spawn` takes a
  small `FieldPlanetLook` struct as its fifth parameter (do not add flags to the existing four; the sun
  and endpoint call sites pass `default`; repulsors add their flag to the same struct), and `Spawn`
  resets the look on every pooled use. The marker is an authored child in `NodeBody`'s own
  anchor/push mark lists (separate from the endpoint ring's `marks`; empty on the sun and endpoint
  prefabs, so no null checks) and follows `SetVisible`, `SetMarksShown`, `SetFade`/`SetStateDim`
  through `ApplyTint`; it does not stay lit while the planet fades on the map.
- Exit: validator fast tests (count, endpoint, off-chord rejected); the look flags reset on a pooled
  `Spawn`; the marker shows in the editor.

## Manual check (headset)

1. An anchored planet reads as locked before the first launch.
2. The anchored job is solved through the neighbours, not the anchor.
3. A pooled planet that was an anchor looks normal in the next job.
