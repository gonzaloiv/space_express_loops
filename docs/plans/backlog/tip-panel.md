# Tip panel — in-world hints during the solve

**Status:** later · presentation · updated 2026-11-06 — a silent, authored side panel for short hints;
one at a time, no modal, no input block.

**Related:** [lore](../../design/world/lore.md) · [jobs](../../design/jobs.md) ·
[sys cube](../../design/systems/cube.md) · [TTS messages](../in-progress/tts-messages.md) ·
[loop completion](../in-progress/loop-completion.md)

## 1. Why

The solve has no hint surface. The lore says anonymous tip panels carry short in-world hints when the
player needs them ([lore](../../design/world/lore.md) §The tech team); the map hint is a different
surface. The tip panel teaches without a modal or a caption strip.

This is not the TTS panel. The TTS panel speaks the job's voice; the tip panel is silent unless a later
plan adds a voice to it.

## 2. The fit

- **Authored and peripheral.** A world-space panel at the side, TMP text, no runtime hierarchy, no camera
  follow.
- **Text-only, one tip at a time.** Auto-fade after `tipSecs`; no pause and no input block.
- **Authored by job and condition.** Once per run; no tip during the job intro or the complete beat.
- **One condition first.** Start with an inspector `ShowTip` for authoring; the first real condition is the
  first refused or over-budget edit, or a no-progress timer.
- **Keep the table.** Use `Levels` keys such as `level_<id>_tip_<n>`; the tip view reads them through
  `LocalizationUtil`.

## 3. Slices

### S1 — the panel

- Author the tip view; show/hide; side placement; `tipSecs` fade.
- **Exit:** a tip shows at the side and fades without covering the field.

### S2 — the cue and the trigger

- Add `TipCue` (key, condition, once) and read it from `PuzzleEditionState`/the controller. No Flow state
  and no TTS.
- **Exit:** the authored condition shows the tip once; a second pass does not.

### S3 — content and tuning

- Write job 1's scale tip, job 2's sun tip, and job 3's clear tip; tune size, contrast, and `tipSecs` at
  bubble distance.
- **Exit:** the three tips read once in place and never compete with the TTS panel.

## 4. What it touches

- `TipPanelView` (new), `TipCue` (new), `PuzzleEditionState`/`SpaceShapingController` condition.
- `Levels` keys; no `LevelData` change unless the key lives on the level.

## 5. Tests

- The tip shows once at the side on the condition; not during the intro or complete beat; it fades.
- No tip overlaps the TTS panel; no input block.
- A missing key does not show an empty panel.

## 6. Out of scope

- Voice/TTS for tips, modal hints, the attention overlay, the map hint.
- A hint system that tracks player skill.

## 7. Risks

- **Tip spam.** One per run, authored condition; no repeating timer.
- **Read vs solve.** Short, peripheral, low contrast; text is secondary to the field.
- **Two side panels.** The tip and the TTS jumbotron must not share the same arc; author separate spots.
- **Missing key.** Skip the panel; never show an empty box.
