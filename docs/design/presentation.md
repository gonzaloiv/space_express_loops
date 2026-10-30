# Presentation — look, sound, read

**Status:** design · presentation — how the game looks, sounds and reads.
**Date:** 2026-10-23.
**Revised:** 2026-10-30.

**Related:** [vision](vision.md) · [core loop](core-loop.md) · [the cube](systems/cube.md) ·
[jobs](jobs.md)

The **feel pass is last** ([vision](vision.md) §6) — this doc records the intent so the loop can be
built toward it.

## HUD / readouts

- **Over the level**, world-anchored — the readouts sit in the play space, not a screen-space overlay.
  The **lab readout panel** is the first authored version; the final HUD is not built.
- Readouts: the **result** (pending / arrived / miss / crash / out of entropy), the **entropy bar**
  (committed + pending), and the **stars** ([core loop](core-loop.md) §3). The HUD never shows a move
  count ([the cube](systems/cube.md)).
- The readout is **unchecked (pending)** while shaping. The flight is the check: the readout becomes
  **checked** on landing; a new edit cancels the pending check. A miss or crash is a check, not a fail;
  the fail is **out of entropy**.
- The trajectory is **green only when the flight arrives** — a miss or a crash reads red.
- The flight runs at **4× the simulation time** (clamped 0.8–3.5 s) and eases with a smoothstep.
- The **live trajectory line** updates on every edit and the ship flies it on release — the flight is
  the payoff, not the only reveal ([core loop](core-loop.md) §1).
- The per-job **manual and tech voice ride the TTS card** ([jobs](jobs.md)); the HUD itself stays
  world-anchored ([voice panel idea](../ideas/tts-voice-panel.md)).

## Readout look — data terminal

The lab readout borrows the **data-dashboard** language ([opencode.ai/data](https://opencode.ai/data)):
a flat `#161616` pane, hairline neutrals (`#D4D4D4` values, `#A3A3A3` labels, `#707070` faint), tabular
figures on a 0.55 em grid, and one indigo accent `#3B5CF6` for the live/pending value. Red `#FF4D40` is
reserved for a fault; `#808080` is the committed bar. Unchecked rows stay muted; a checked row takes the
accent. The reset control is `#242424` idle / `#303030` hover / accent pressed. This is the lab panel
first; the main HUD and store panels take the same tokens in a later pass.

## Art direction

- **Stylised** — not photoreal. Exact language still to be closed.
- The **hologram** look (tinted, semi-transparent, fresnel-rimmed) is a recorded option for the
  bubble ([hologram idea](../ideas/hologram-look.md)).
- The cube must read as a tangible instrument; the field must read — the mesh warp is the scale's tell.

## Audio

- **Music exists.** **New SFX still to add** — the cube's scale (the haptic "stretch" + rising pitch),
  the travel moment, the grade.

## Mixed reality

- The **bubble is present** in the slice — the playfield is anchored in the room; the room-scale
  **Room** view is **legacy (cut)** ([vision](vision.md) §3).

## Tuning

- Comfort: the arm-reach cone and the bubble distance (the numbers to hit for a seated player).

## Open questions

- Final art language and the bubble/planet/cube look.
- Final HUD placement; the readout look is settled (above).
- The SFX list (travel, scale, grade) — a later polish pass.
