# Puzzle map — selection info and onboarding

**Status:** later · presentation · updated 2026-11-05 — the cube carries the job's state, name and stars,
and the pinch-drag/spread verbs are taught in place.

**Related:** [progression](../../design/systems/progression.md) · [jobs](../../design/jobs.md) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

The map now shows the selected field's state (cube tint, marker, state/stars label), but the job's
identity and the two verbs (pinch-drag, spread) are still learned by trial. A first-time player sees galaxies and a cube; the
label says "Puzzle 2 · Sun". The run should read as a place with a next job, and the gestures should be
legible without a briefing.

## 2. The definition

- **Cube info.** The selected cube names the job (level name, not "Puzzle N") and shows the star score;
  locked shows the unlock condition ("solve 2"), solved shows the best score. The state marker's shape
  and tint stay the primary read; the text is secondary.
- **Onboarding.** An authored hint near the cube says "Pinch-drag the map · Spread to enter · Close to
  overview". It fades after the first successful drag, overview and zoom, persists within the run once
  seen, and can be replayed from the inspector. No modal.
- **Job brief (thin).** One line of the job's brief under the name: the goal and the sun. No new panel;
  the map stays the place. The full contract screen is the TTS job intro.
- **Locked reason.** Selecting a locked field names the prerequisite instead of only refusing.
- **Layout.** The label remains above the cube; the hint is a separate authored object near the cube, so
  the map view can show/hide it without building text at runtime.

## 3. Slices

### S1 — cube identity

- Move the label text into a small formatter: job name, sun, state, stars, and the locked prerequisite.
- Wire the level's authored name through `PuzzleData`; keep "Puzzle N" as the fallback.
- **Exit:** the selected cube reads the job, state and stars; a locked field says what unlocks it.

### S2 — first-use hint

- Author the hint near the cube and a `MapHintView` that fades after the first drag, first overview and
  first zoom; persist "seen" in the player data for the run.
- **Exit:** a fresh player sees the verbs once; returning players do not.

### S3 — job brief line

- Add the one-line brief to the selected label; no panel.
- **Exit:** the job's goal and sun are readable on the cube without leaving the map.

## 4. Out of scope

- The TTS contract screen, panels, and voice.
- Free pan, nearest-field selection, and a map zoom-out overview.

## 5. Risks

- **Label density.** The cube label is small; three lines may not fit. Prefer name + state/stars over the
  brief if space runs out.
- **Hint persistence.** A stored "seen" flag must follow the run/save, not the app install.
