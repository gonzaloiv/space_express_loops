# Chapter 3 — pinch refusal

**Status:** next · space shaping · updated 2026-11-27 — a pinch that covers no scalable source (an
anchor, a fixed sun, empty space) is refused, spends nothing and launches nothing; a movable job
never starts on such a frame. Runs alongside [anchors](chapter-3-anchors.md): it needs only
`fixedMass`, which exists. Group: [anchors](chapter-3-anchors.md) ·
[repulsors](chapter-3-repulsors.md) · [fleet bodies](chapter-3-fleet-bodies.md).

**Related:** [plans board](../README.md) · [the cube](../../design/systems/cube.md) ·
[chapter 3](chapter-3-jobs.md)

## Units

### U1 — one predicate, refused at runtime

- Define the predicate once: `SpaceZone.CoversScalable(SourceSpec[])` is true when any non-fixed
  source has `zone.InsideFactor(source.Home) > 0` (a partial pinch counts; containment does not).
  `SpaceShapingSession.HasScalableSource(zone)` forwards to it. Evaluate it against the committed
  field — drop the in-flight preview and recompute before testing — because `PreviewZone` moves
  scalable sources.
- `SpaceShapingSession.PreviewZone` stores the preview with cost 0 when the predicate is false, so the
  entropy bar shows no pending spend while the player pinches an anchor.
- `SpaceShapingSession.CommitPreview` (and the `PuzzleRun` forwarder) returns a `ZoneRefusal`; on
  `NoEffect` (a new value) it clears the preview, adds no command, spends nothing and launches nothing.
  The controller re-renders the committed field (`PuzzlePreview.Apply` plus the `Pending` readout),
  raises `ZoneRefusal.NoEffect` through `view.Refuse` (for the cue), and pulses every fixed source in
  the zone: `SpaceShapingSession.FixedSourcesInside(zone)` → `SpaceShapingView.PulseSource(index)`
  (passing `spawner.Zones.Danger`) → `FieldPlanetSet.PulseSource(index, danger)` matching
  `FieldPlanetBehaviour.SourceIndex` on a visible slot (`IsVisible`), so a hidden slot from the last
  job is skipped, and using the colour for `PlayRefusal`.
- `PuzzleEditionState.OnRefused` early-returns for everything but `OutOfEntropy`; let `NoEffect`
  through the cooldown. `PuzzleLabPanel.RefusalText` gets a `NoEffect` line.
- Exit: fast tests — a pinch over only a fixed source, and over empty space, is refused, spends
  nothing and adds no command; a pinch partly over a scalable source is accepted.

### U2 — start frames

- `PuzzleStartFrame.Accepts` requires `CoversScalable` on the resting zone, so the runtime refusal and
  the bake's start-frame rule cannot drift. `Place` still returns `preferred` when no candidate
  accepts, so `PuzzleStartFrameCheck.CheckMovable` must also reject a frame with no scalable source;
  the bake's start check then fails rather than shipping the no-op start.
- Exit: a fast test rejects a no-op start frame; `ShippedJobs_MeetTheContract` stays green (no shipped
  frame is a no-op).

## Manual check (headset)

1. Pinching an anchored planet is refused, spends nothing and launches nothing; the anchor pulses.
2. Pinching empty space is also refused; the entropy bar does not move.
3. A movable anchored job does not start on a frame that changes nothing.
