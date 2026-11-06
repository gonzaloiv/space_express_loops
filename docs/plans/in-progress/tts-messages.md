# TTS messages — the jumbotron panel

**Status:** now · presentation · updated 2026-11-06 — S1–S2 wired: the existing TTS panel is the
jumbotron and legacy clips are removed. S3's remainder (job 1 silent, tech line only, the fail cue) moved
to [loop completion](loop-completion.md); S4 content is pending.

**Related:** [jobs](../../design/jobs.md) · [lore](../../design/world/lore.md) ·
[vision](../../design/vision.md) · [core loop](../../design/core-loop.md) ·
[TTS companion idea](../../ideas/tts-voice-panel.md) ·
[job definition card](../backlog/job-definition-card.md) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

The run's voice is the job's intro — the manual entry and the tech line — plus the message when the job
is complete ([jobs](../../design/jobs.md) §The manual's arc). The POC has no TTS panel; the in-scene TTS
stack is legacy and unwired, and the legacy content (49 clips, the old `level_*` and `welcome_*` keys)
belongs to a different game.

The jumbotron **is** the TTS panel: an authored field-side panel with the speaker's face (Jim). It is not
a camera overlay and not a companion that follows the head. It sits at the side, reads at distance, and
never covers the run. The panel does not follow, park, or re-dock in this plan; the camera-operator
companion stays deferred.

## 2. The fit

- **The jumbotron is the TTS panel.** An authored panel in the scene: TMP text, Jim's face, show/hide,
  fixed side placement. Reuse the in-scene TTS module and the authored panel pattern; do not share the
  splash's `SubtitlesLikeFollow` object.
- **One line at a time.** The manual entry, then the tech line; the complete message is its own cue. The
  skip button ends the current cue.
- **States launch the cues.** `PuzzleStartState` launches the intro; a new `PuzzleCompleteState` launches
  the complete message and returns to the map. No controller-owned TTS; no presentation Flow state beyond
  the complete state.
- **Keep the `Levels` table.** The authored keys are `level_<id>_init_client`, `_cargo`, `_manual_entry`,
  `_01` (the tech line), and a new `_complete`. Remove the 49 legacy mp3s.
- **System and panel first.** Final copy, keys, and recordings are a later content pass. Placeholders are
  text-only; `LocalizedTTSVoice` gets a minimum reading time instead of blocking the line.
- **No resume, no gate.** The voice keeps playing while the player pinches; the panel stays at the side.
  There is no visibility stop/resume, and `Stop` on state exit is enough.
- **Puzzle live underneath.** The intro does not block the solve; the panel is peripheral and quiet.

## 3. Slices

### S1 — strip legacy and wire the scene

- Remove the 49 legacy mp3s; keep the `Levels` table and its authored job keys.
- Reuse the authored `Features/TTSVoice` panel (`CardPanel` + Jim) as the jumbotron; wire `TtsPresenter`
  on the same GameObject. No runtime `AddComponent`.
- Add `level_01_01_complete` as the first placeholder; jobs without a complete key fall back to a short
  wait.
- **Exit:** job 1's intro lines show on the existing panel through the TTS module; no legacy clip can play.

### S2 — the presenter and the panel

- Add `TtsPresenter` (one voice, ordered key fragments, stop on state exit, no overlap) above the TTS
  module. `LocalizedTTSVoice` gets a minimum reading time so text-only lines are readable.
- The panel stays at its authored side spot; no resume and no visibility gate. A pinch does not stop it.
- **Exit:** the intro plays manual then tech; the panel sits at the side; leaving the job stops it.

### S3 — state hooks and the complete state

- `PuzzleStartState` launches the intro on enter. Add `PuzzleCompleteState`; it launches the complete
  message and returns to the map after the cue. The edition view keeps the readout visible until then.
- No intro on reset or retry; stop the previous cue before the next; hide the panel on state exit.
- **Exit:** start and complete both come from states; returning to the map stops the voice; the next job
  never overlaps.

### S4 — content pass (later)

- Write the seven job intros and complete lines, record the clips, and fill both locales.
- **Exit:** the content pass is its own authoring job; the system/panel exit is S1–S3.

## 4. What it touches

- `PuzzleStartState`, `PuzzleEditionState`, new `PuzzleCompleteState`.
- `LocalizedTTSVoice` (minimum reading time, no resume), `StreamingAssetsSpeaker`.
- `TtsPresenter` (new, same GameObject as the panel).
- `Game.unity` jumbotron; `Levels` keys.

## 5. Tests

- The panel shows on state enter and hides on state exit; no legacy line appears.
- The intro plays manual then tech; the complete cue plays once; the next cue does not overlap.
- A pinch does not stop the panel or the voice; the player can keep playing.
- Reset/retry does not replay the intro; returning to the map stops it.
- A missing clip shows the line for its minimum reading time; a missing key does not leave the panel stuck.

## 6. Out of scope

- Follow, parking, re-dock, visibility gating, and resume.
- New voices; final lines and recordings (S4).
- The job definition card.
- The camera-operator companion.

## 7. Risks

- **Sharing the splash panel.** `SubtitlesLikeFollow` serves the splash; the TTS panel is the authored
  `Features/TTSVoice` object, not the splash card.
- **Card coupling.** The `CardPanel` on `Features/TTSVoice` is the jumbotron in this plan; no new view.
- **Substring keys.** `GetKeysThatContain` is a substring match; the presenter passes explicit key
  fragments (`_manual_entry` then `_01`), so the client/cargo keys are not spoken.
- **Audio naming.** The clip is named by the localization key; placeholders must not depend on a clip to
  advance.
- **Competing with the read.** Side, low contrast, no motion during the solve; the face is a small inset.
- **State ownership.** Stop the cue in `Exit`; a stale coroutine or clip across the map would overlap the
  next job.
