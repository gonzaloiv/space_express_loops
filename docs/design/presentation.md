# Presentation — look, sound, read

**Status:** design · presentation — how the game looks, sounds and reads.
**Date:** 2026-10-23.

**Related:** [vision](vision.md) · [core loop](core-loop.md) · [the cube](systems/cube.md) ·
[feel plan](../plans/backlog/presentation/space-shaping-feel.md)

The **feel pass is last** ([vision](vision.md) §6) — this doc records the intent so the loop can be
built toward it.

## HUD / readouts

- **Over the level**, world-anchored — the readouts sit in the play space, not a screen-space overlay.
  This is how it works today.
- Readouts: **pass / fail**, the **ship's clock**, the **window** `[Tmin, Tmax]`, the **entropy
  bar**, and the **stars** ([resources](systems/resources.md)).
- The **trajectory line is removed** — the flight is the reveal, and the visual updates at launch.
- It could also ride the **TTS panel** screen ([voice panel plan](../plans/backlog/presentation/tts-voice-panel-personality.md)).

## Art direction

- **Stylised** — not photoreal. Exact language still to be closed.
- The **hologram** look (tinted, semi-transparent, fresnel-rimmed) is a recorded option for the
  bubble then the edition view ([hologram plan](../plans/backlog/presentation/hologram-look.md)).
- The cube must read as a tangible instrument; the field must read (the mesh warp is the scale's
  only tell — [the cube](systems/cube.md)).

## Audio

- **Music exists.**
- **New SFX still to add** — the cube's scale (the haptic "stretch" + rising pitch), the travel
  moment, the grade.

## Mixed reality

- The **bubble is present** in the slice — the playfield is anchored in the room; Room/Bubble are
  the two MR views ([vision](vision.md) §3).

## Tuning

- Comfort: the arm-reach cone and the bubble distance (the numbers to hit for a seated player).

## Open questions

- Final art language and the bubble/planet/cube look.
- Final HUD placement and whether it rides the TTS panel.
- The SFX list (travel, scale, grade, collapse).
