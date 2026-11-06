# TTS messages — the jumbotron panel

**Status:** now · presentation · updated 2026-11-06 — S1–S3 landed and most of S4; open: the last
cleanup items in S4 and the headset check. The intro/complete/fail hooks are in
[loop completion](loop-completion.md) U3; content is S5.

**Related:** [jobs](../../design/jobs.md) · [lore](../../design/world/lore.md) ·
[loop completion](loop-completion.md) · [job definition card](../backlog/job-definition-card.md) ·
[TTS companion idea](../../ideas/tts-voice-panel.md) · [Game.unity](../../../Assets/_Game/Game.unity)

## Goal

Jim is the only voice: one line at the start of a job and one message at the end. The jumbotron (the
authored `Features/TTSVoice` panel with Jim's face) shows it at the side while the player keeps playing.
The manual entry is on the [job card](../backlog/job-definition-card.md), never spoken.

## Contract

- **What plays.** Intro: the tech line `<intro>_01` only; job 1 has none (`noIntro`). End: `_complete`
  on a solve, `_fail` on a fail. Both are launched by states — owned by loop completion U3.
- **Never a gate.** No skip button, no input. The panel sits at its authored side spot; the puzzle is
  live under the intro; a pinch never stops the line. `Stop` on state exit is the only interruption.
- **Readable when text-only.** A line stays up for a reading time that scales with its length, or for
  the clip's length when a clip exists — whichever is longer.
- **Clips are optional per line.** A clip named after the key plays if it exists; otherwise the line is
  text-only. No global toggle.
- **One line at a time;** stale callbacks are dropped (`TtsPresenter` generation token).

## Slices

### S1–S2 — landed

The panel is the jumbotron; `TtsPresenter` sits on it and drives ordered keys with no overlap; the 49
legacy mp3s and the legacy `welcome_` / `_init_0N_*` keys are gone; all four states hold the presenter.

### S3 — reading timer

- Replace the flat `minEntrySecs` in `LocalizedTTSVoice` with a reading time:
  `max(minEntrySecs, baseSecs + secsPerChar × length)`, all three authored on the voice. The clip, when
  present, sets the floor.
- **Exit:** job 1's three-sentence receipt stays up long enough to read at bubble distance; a one-word
  line still holds `minEntrySecs`.

### S4 — no skip, and the cleanup

- **No skip.** Remove the skip button and its panel from `Features/TTSVoice`, and the skip path in
  `LocalizedTTSVoice` (`SetupSkipButton`, `skipButtonClick`, `HasSkipButton`) and the wrapper's
  `SkipButtonClick`.
- **No `useClips`.** Remove the flag from `StreamingAssetsSpeaker`; a missing file already falls back
  (`File.Exists` on desktop, a failed request on Android) and the line stays text-only.
- **Dead code.** `TtsPresenter.IsPlaying` (no callers); the empty `PuzzleStartState.Exit`; the
  wrapper's `SwapPanel`, `RestorePanel`, `SpeakAt`, `SpeakRandom` and `LocalizedTTSVoice`'s swapped-panel
  path and `SetPanelWorldPosition` — no callers outside the TTS module. Check other projects that share
  `_Modules/TTS` before deleting the public ones.
- **Wiring rule.** Drop the null checks on `LocalizedTTSVoice`'s serialized `panel`; a missing
  reference is a broken prefab.
- **Exit:** the panel has no button; a line plays and fades with no input; the module compiles with no
  unused members; the scene has no dangling skip references.
- **Landed (2026-11-06):** the skip button and path, `useClips`, `IsPlaying`, the panel-swap / position
  APIs and the `panel` null check are gone. **Still open:**
  - Remove the empty `PuzzleStartState.Exit` override.
  - Drop the null checks on `LocalizedTTSVoice`'s serialized `id` (`Id != null`).
  - `MarkdownTTSVoiceWrapper`: wire `LocalizedTTSVoice` as a `[SerializeField]` instead of the lazy
    `GetComponent`, and drop the `jumpEntries != null` check (it is serialized; assign it).
  - Leave the sample scene (`Samples/StreamingAssetsSpeaker`) alone.

### S5 — content (later)

- Write the intro lines (jobs 2–7), job 1's first message, and the complete/fail lines for jobs 2–7;
  record clips named by key; fill both locales.
- **Exit:** the content pass is its own authoring job.

## Manual check (headset)

1. Open job 2: Jim's one line shows at the side; there is no button.
2. Pinch while it shows: the puzzle responds; the line keeps going.
3. Solve job 1: the receipt stays up long enough to read all three sentences.
4. A line with no clip shows text for its reading time; a line with a clip plays the clip.
5. Leave a job mid-line: the voice stops; nothing carries into the map or the next job.

## Out of scope

- The hooks (intro / complete / fail, `noIntro`) — loop completion U3.
- The manual entry on screen — the job card.
- Follow, parking, re-dock, visibility gating, resume; the camera-operator companion.

## Risks

- **Shared module.** `_Modules/TTS` may serve other projects; delete public members only after checking.
- **Substring keys.** `GetKeysThatContain` is a substring match; pass full keys so `_init_client` /
  `_init_cargo` are never spoken.
- **Competing with the read.** Side, low contrast, no motion during the solve; the face is a small inset.
