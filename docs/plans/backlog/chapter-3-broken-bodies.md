# Broken bodies

**Status:** next · space shaping · updated 2026-11-27 — anchored planets and repulsors, end to end
(sim, lattice, look, bake, two-pass fleet bake, validator), with the repulsor gate on 1 Dec. Chapter
3's jobs are [chapter 3](chapter-3-jobs.md).

**Bakes:** off the editor — the U1 and U2 Creator tests bake fields
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`). The fleet's second
pass runs on the bake worker; see U1 selection.

**Related:** [plans board](../README.md) · [chapters](../../design/chapters.md#chapter-3--broken-bodies-store-release) ·
[the cube](../../design/systems/cube.md) · [glossary](../../design/glossary.md)

## Goal

Two bodies the player meets with the verbs they already have. **Anchored:** fused in place; the Wrap
cannot change it, so the line bends through its neighbours. **Repulsor:** pushes instead of pulling;
shrinking it opens a gap, growing it shoves a ship aside.

## Gate (1 Dec)

A repulsor job must bake through the new knob, classify `Clear`, draw its bump before the first pinch
in the editor, and read as a shrink/grow on device. If that does not hold, chapter 3 ships with
anchored jobs only, using **more anchors per job** for variety, and repulsors move to an update. Not a
second sun: one sun is assumed across the validator (`SunIndex`, `CheckSources`' planet count) and the
field (`FieldPlanetSet` holds one `sunPlanet`), so two suns would be a third feature.

## Units

### U1 — anchored bodies

- **Data.** `fixedMass` already means immovable end to end (`PuzzleSource`, `SourceSpec`,
  `SourceSpecs`, `SpaceZone.ApplyTo`, `SourceConstraints`); only the job-level selection is missing.
  Add an `Anchors` count to `PuzzleJobContract` and `CreateSeed`, `PuzzleSeed` and `PuzzleBrief.From`.
  `PuzzleJobSolver.Brief` and `PuzzleTestSetBuilder` derive from `CreateSeed`, so they need no
  separate change; tests that build a `PuzzleBrief` directly set the field explicitly.
- **Selection.** Add one pure `PuzzleBrokenBodyChoice` helper that takes the field's `PuzzleSource[]`
  and the chosen runs, and marks `fixedMass`/`pushes`. An anchor must sit on the chord: distance to a
  run segment < body radius × `PuzzleRunChoice.PlanetBlockBandFactor`, not an endpoint, not the sun;
  the order is deterministic. `PuzzleFieldBake.Apply` marks bodies **only when `!brief.IsMulti`**: a
  fleet's `PuzzleMultiShipSearch.Prepare` calls `Apply` too, and its pass 1 must stay pristine. Fleet:
  the ships are chosen after the field is built, so bake in two passes —
  `PuzzleMultiShipSearch.Run` probes without broken bodies, selects the ships with a public
  `PuzzleShipRescueExtensions.SelectShips(rescued, fleet, brief)` (lift `PuzzleMultiShipBake.Select`
  to that extension so both callers share it), marks anchors on those runs and repulsors off them
  (U2) on a `FleetField.WithBodies(...)` copy, probes again, and returns the pass-2 `FleetZoneResult`
  with the pass-1 ships on it (add `Ships` to the result; the probe leaves it empty). `Confirm`
  applies the bodies to the field and builds the variant with those ships through the existing
  `ConfirmZone`, which selects only when `found.Ships` is empty; do not re-select. Keep both passes
  on the bake worker; no extra main-thread search. If the second pass finds no zone, the seed fails.
- **Bake rule.** The on-chord check is geometric, shared with the validator (`IsOnChord`), so it
  works for any anchor count. Do not use `PuzzleStartFrame.SolvesInPlace` for this:
  `SpaceZone.ApplyTo` skips fixed sources, so a frame over an anchor changes nothing and the test
  cannot see it.
- **A pinch on the anchor is a refused no-op.** Define the predicate once:
  `SpaceZone.CoversScalable(SourceSpec[])` is true when any non-fixed source has
  `zone.InsideFactor(source.Home) > 0` (a partial pinch counts; containment does not).
  `SpaceShapingSession.HasScalableSource(zone)` forwards to it, and `PuzzleStartFrame.Accepts` calls
  the same predicate on the resting zone, so the runtime refusal and the bake's start-frame rule
  cannot drift. `SpaceShapingSession.PreviewZone` stores the preview with cost 0 when the predicate
  is false, so the entropy bar shows no pending spend while the player pinches an anchor.
  `SpaceShapingSession.CommitPreview` (and the `PuzzleRun` forwarder) returns a `ZoneRefusal`; on
  `NoEffect` it clears the preview, adds no command, spends nothing and launches nothing. The
  controller re-renders the committed field (`PuzzlePreview.Apply` plus the `Pending` readout),
  raises `ZoneRefusal.NoEffect` through `view.Refuse` (for the cue), and pulses
  every fixed source in the zone: `SpaceShapingSession.FixedSourcesInside(zone)` →
  `SpaceShapingView.PulseSource(index)` (passing `spawner.Zones.Danger`) →
  `FieldPlanetSet.PulseSource(index, danger)` matching `FieldPlanetBehaviour.SourceIndex` and using
  the colour for `PlayRefusal`.
  `PuzzleEditionState.OnRefused` currently early-returns for everything but `OutOfEntropy`; let
  `NoEffect` through the cooldown. `PuzzleLabPanel.RefusalText` gets a `NoEffect` line. The rule also
  covers a non-scalable (fixed) sun and a zone over empty space.
- **The validator.** `CheckSources` gains: the anchored planet count matches `contract.Anchors`;
  anchors are `fixedMass`, not `centreOnly`, not a ship endpoint, and lie on at least one stored
  ship's chord (the primary run for a single ship). Anchors count as blockers — `PuzzleRunBlockers`
  stays unchanged; the bake must still find an answer with a scalable planet. The rule is checked by
  `ShippedJobs_MeetTheContract` too.
- **The look.** `SpaceShapingSession` exposes `IsFixed(i)` (and `Pushes(i)` in U2).
  `FieldPlanetBehaviour.Spawn` takes a small `FieldPlanetLook` struct as its fifth parameter (do not
  add two flags to the existing four; the sun and endpoint call sites pass `default`), and `Spawn`
  resets the look on every pooled use. The marker is an authored child in `NodeBody`'s own
  anchor/push mark lists (separate from the endpoint ring's `marks`; empty on the sun and endpoint
  prefabs, so no null checks) and follows `SetVisible`, `SetMarksShown`, `SetFade`/`SetStateDim`
  through `ApplyTint`; it does not stay lit while the planet fades on the map.
- **Start frames.** `PuzzleStartFrame.Place`/`Accepts` rejects a frame that covers no scalable source
  (the `CoversScalable` predicate above), so an anchored movable job never starts on a no-op pinch.
- Exit: a Creator test bakes an anchored single-ship field and an anchored fleet through the
  two-pass path (keep the fleet sample small); fast tests show an anchor-only pinch is refused and
  spends nothing, a no-op start frame is rejected, and the look flags reset on a pooled `Spawn`; the
  marker shows in the editor.

### U2 — repulsors

- **Data and sim.** Add a `Pushes` count to `PuzzleJobContract`, `CreateSeed`, `PuzzleSeed` and
  `PuzzleBrief.From`, and `pushes` to `PuzzleSource`. Copy it at every hop: `SourceSpecs.From`,
  `SourceSpec` and `SourceSpec.Scaled` (which rebuilds the field on every zone edit; a missed copy
  turns the repulsor attractive mid-job), and the `FleetField` clone (add `Pushes` to the
  `FleetFieldTests` equality). `PuzzleSourceBridge` needs no change; the bake marks pushes after the
  bridge. `SpaceShapingSession.BuildField` builds a negative `Strength` from it; `SpacetimeField`
  already repels on a negative strength.
- **Selection and reach.** Extend `PuzzleBrokenBodyChoice`: choose `Pushes` planets that are off
  every selected run's chord (distance > body radius × `PlanetBlockBandFactor`), **within**
  `RepulsorReach.Radius` of at least one selected run, not endpoints, the sun, or anchors;
  deterministic. Single-ship in `Apply` with the anchors; fleet in the second pass (U1). Define one
  `RepulsorReach.Radius(SourceSpec) = PushReachFactor × InitialScale × 0.5f`, `PushReachFactor`
  starting at 2.5, shared by the bake and the validator and tuned on device. The two bounds leave a
  narrow annulus; if a seed cannot fill it, raise `PushReachFactor` before weakening "off the chord"
  — otherwise the repulsor becomes a chord blocker and teaches nothing.
- **The lattice.** `WellPull.Toward` returns zero for `Strength <= 0` and uses `Sqrt(Strength)`: use
  `|Strength|` for the depth and reverse the direction for a repulsor. `LatticeWells.changeOnly`
  draws `drawn − baseline`, so zero repulsors in the **baseline only** (both baseline writes: at rest
  and on a fresh baseline), never in the drawn copy; otherwise the repulsor shows no bump until the
  first pinch. `LatticeWells.Fill` reads `session.Pushes(i)`.
- **The role.** `PuzzleSolutionClassifier` calls a shrink `Clear` for an obstacle from a crash or the
  chord, or a source near the path; a repulsor off the chord makes a miss. Add the repulsor rule in
  `Obstacles`: a `Pushes` source is an obstacle when the run chord or the start path passes within
  `RepulsorReach.Radius` (the chord is the primary measure; the start path is already deflected by
  the repulsor). Do not use `NearAnyPath`'s `InitialScale * 0.5` body radius for repulsors. Keep the
  general `Curve` rule: any grown source is a shove; the lesson tie below is what makes it the
  repulsor.
- **The lesson is the pushed body.** Mark pushes before the zone search, then require the answer to
  act on them: for a contract with `Pushes > 0`, `PuzzleSolutionSearch` and the fleet probe accept
  only a solution whose zone covers a `pushes` source (add a zone filter to `TrySelect`/`Find`).
  `Any` is the exception — the capstone may use either body — but its repulsor still has to be
  within reach, so the field reads. A seed whose only `Curve` is a normal planet is rejected, not
  baked as 3-4.
- **The look.** A second marker in the same anchor/push mark lists, same toggle/reset rule as U1.
- **The validator.** `CheckSources`: `pushes` count matches `contract.Pushes`; a repulsor is not
  `fixedMass`, `centreOnly`, or an endpoint, is off every stored ship's chord, within
  `RepulsorReach.Radius` of at least one stored run, and disjoint from anchors. The stored answer's
  zones cover a `pushes` source unless `Mechanism == Any` (the lesson tie above). The same rule is
  checked by `ShippedJobs_MeetTheContract`.
- Exit: fast tests: a ship flying past a repulsor is pushed; editing a repulsor's zone keeps the push
  (the `Scaled` copy); `WellPullTests` gains a negative-strength case; off-chord, in-reach selection
  is deterministic; a repulsor out of reach and an answer that misses the pushed body are rejected.
  A Creator test bakes a repulsor field (through the new contract/seed knob) classified `Clear` and
  asserts the answer covers the repulsor; another rejects a `Curve` seed whose grown source is a
  normal planet. The bump draws before the first pinch in the editor.

## Manual check (headset)

1. An anchored planet reads as locked; pinching it is refused, spends nothing and launches nothing.
2. Pinching a zone over no scalable source (empty space, or a fixed body if one ships) is also
   refused; the entropy bar does not move.
3. A repulsor reads as different (look and raised lattice) before the ship flies.
4. Shrinking a repulsor opens the gap; growing one shoves the ship aside.
5. The repulsor jobs' answers edit the repulsor itself, not a neighbouring planet; the path changes
   with it.
6. A pooled planet that was an anchor or a repulsor looks normal in the next job.
7. A movable anchored job does not start on a frame that changes nothing.
8. The fleet capstone reads both bodies, and its two-pass field still solves.
9. Tune `PushReachFactor` and the repulsor strength on device; record the values.
10. Frame rate holds with both bodies in the field.
