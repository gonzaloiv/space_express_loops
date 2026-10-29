# Feel polish — candidates

**Status:** idea · presentation · updated 2026-10-28 — candidates, **not plans**. The decided presentation
work moved to [HUD + validation 2](../plans/backlog/space-shaping-hud-validation.md).
**Related:** [presentation](../design/presentation.md) · [the cube](../design/systems/cube.md) ·
[jobs](../design/jobs.md) · [HUD + validation 2](../plans/backlog/space-shaping-hud-validation.md)

## Why this exists

The later presentation items lost their single owner: the window gauge and the fail beat are
[HUD + validation 2](../plans/backlog/space-shaping-hud-validation.md), the
per-job manual and tech voice are [in the game](../plans/in-progress/space-shaping-in-game.md) /
[catalogue](../plans/backlog/space-shaping-catalogue.md), and the flight is already in the code
(`ShipTravelView`). What is left is undecided polish. None of it gates the mechanic, and the time already
reads numerically (`PuzzleLabPanel.cs:44-46`).

## 1. Lattice time tint

- **What:** tint the lattice cool/warm by the local potential (slower/faster clock), so the time face reads
  *where* the player scales, not only as the window numbers.
- **Cheap part:** `SpacetimeField.Potential` is public (`SpacetimeField.cs:16-27`), and
  `SpacetimeMeshView.Render` already walks every vertex (`SpacetimeMeshView.cs:47-62`).
- **Cost:** a vertex-colour shader; the feature has no custom shader today (`Lattice.mat` is a stock
  material).
- **Trigger:** validation 2 testers cannot say why the clock moved, or the numeric window is not enough.

## 2. Lattice pulse

- **What:** animate the tint at the local clock rate (spacing = space, pulse = time).
- **Why not now:** the lattice is 48 tubes, not cells (`LatticeMeshBuilder.cs:68-77`); a rate along thin
  tubes cannot be compared across the grid, and it needs an animated shader.
- **Reopen only if** the tint proves out and testers ask for rate.

## 3. Locked-gesture refusal — flash built

- **What:** flash the frame when a locked move/resize is attempted or a scale is refused at the budget.
- **Built 2026-10-28:** the frame turns red for 0.18 s (`ZoneGrabListener.Refuse`, thresholded so resting
  a hand on a locked cube does not flash). A short thud would ride the audio seam in item 5.
- **Still open:** the sound/haptic cue, and a matching flash on the entropy bar at the budget.

## 4. Haptics + pitch

- **What:** a haptic “stretch” on the cube scale; a rising pitch.
- **Why not now:** no haptic or audio code in the feature; the pillar
  ([vision](../design/vision.md) §2) but after the validation.
- **Decide first:** the pitch tracks the scale delta, not remaining entropy — entropy is the fail path.

## 5. SFX

- **What:** travel, scale and grade.
- **Why not now:** an audio pass. Music exists; the feature has no audio code. The locked sound only
  returns with item 3.

## Order

The tint first if any. The rest only on a playtest signal. Promote one to the
[plans board](../plans/README.md) when it is a morning's work.
