# TTS job intro — the contract screen

**Status:** later · presentation · updated 2026-11-02 — a field-side contract screen at job start
(manual + tech voice) that can reposition around the field; the camera-operator companion is deferred.

**Related:** [jobs](../../design/jobs.md) · [lore](../../design/world/lore.md) ·
[vision](../../design/vision.md) · [core loop](../../design/core-loop.md) ·
[TTS companion idea](../../ideas/tts-voice-panel.md)

## 1. Why

The run's TTS is the **job's intro**: the manual entry and the tech-voice line, two entries per job
([jobs](../../design/jobs.md) §The manual's arc). The POC has no TTS screen; the existing stack
(`TTSHelper`, `LocalizedTTSVoice`, `MarkdownTTSVoiceWrapper`, `StreamingAssetsSpeaker`, `VoiceId.Jim`) is
wired to the legacy flow (`GameStartState`, `EditionState`, `GameEndPanel`) and the room-scale
`SubtitlesLikeFollow` card.

The old [TTS companion idea](../../ideas/tts-voice-panel.md) — a camera-operator card that enters from the
view edge, parks out of sight, and travels back — was designed for the persistent legacy card. It does
not fit the current game: **seated MR**, one job at a time, the company is a wrapper *between* jobs, and
the card is a short intro. A card that follows or parks would cover the bubble and fight the read.

But the intro should not be a flat card floating in front of the bubble either: the field is the world,
and the contract is part of it. The better fit is a **contract screen** at the field's edge — a quiet
jumbotron/console that can move around the field without becoming a camera overlay.

## 2. The fit

- **A field fixture, not a camera overlay.** The screen is an authored object on the field's perimeter
  (or just above it) — a jumbotron/console at a readable distance, facing the player. It is not a
  screen-space card and not a companion that follows the head.
- **It can move around the field.** At job start it picks a free arc: the side nearest the player's gaze
  that does not cross the run, the cube's start, or the hands' reach. It glides there along the perimeter.
  It may **re-dock once** if the player turns far away; it never orbits continuously and never crosses the
  field centre.
- **Diegetic content.** A corporate contract board: client + cargo as a header/ticker, the manual entry as
  a corporate slide, the tech voice as a broadcast/subtitle, Jim as a small inset or voice-only. The look
  is a quiet, slightly out-of-date company display — not a flashy ad.
- **Intro only.** It powers up for the intro, plays the two entries, then powers down/hides on the first
  pinch or after the entries. It does not move or speak during the solve.
- **Visibility-gated speech.** If the screen is out of view, the speech stops; when it re-docks or the
  player looks back, the current entry resumes (a generation token guards the restart).
- **No gate on play, no replay on reset.** The puzzle is live underneath; the intro plays on job
  start/next, not on reset/retry.

## 3. Slices

### S1 — the screen

- Add an authored `ContractScreen` prefab/view to the POC scene: world-space, TMP text, a holographic/CRT
  material, Jim's idle expression or a voice-only broadcast, show/hide. No runtime hierarchy; wire it in
  the scene.
- Placement: a fixed radius on the field's perimeter or just above it, at a readable height, facing the
  player. Pick the free arc from the run/cube/hand cone; start with the side nearest the gaze.
- Add a `JobIntroData` asset per job — client, cargo, manual key, tech key — and reference it from the
  POC's puzzle set (or from `PuzzleData`). Start with job 1.
- Show on `StartPuzzle`/`NextPuzzle`; hide on the first pinch or after `ShowForSeconds`; never cover the
  run.
- **Exit:** the screen appears for job 1 at a free arc, reads the client/cargo/manual/tech text, hides on
  play, and does not cover the run.

### S2 — the voice and the re-dock

- Add `job_N_manual` and `job_N_tech` to the `Levels` table (or a `Jobs` table) so the wrapper can speak
  the job's fragment; the manual entry comes first, the tech line second.
- Wire `TTSHelper` / `LocalizedTTSVoice` / `VoiceId.Jim` in the POC. Play the two entries, stop on hide,
  and stop the previous job before the next starts.
- Visibility gate: stop when the screen is out of view, resume the same entry when it is visible again.
  Keep the skip.
- Re-dock: if the player turns far away for `reDockSecs`, glide the screen along the perimeter to the new
  free arc (peripheral path, capped speed, one re-dock). No continuous chase.
- **Exit:** job 1 speaks the manual + tech lines; looking away stops the speech and returning resumes;
  the next job never overlaps; the screen re-docks at most once.

### S3 — the seven intros

- Write the seven job intros from [jobs](../../design/jobs.md) §The run — the lore skin: client, cargo,
  manual entry, tech voice. One idea per entry; the manual arc is cheerful → bleak, the tech voice
  pride → doubt.
- **Exit:** all seven jobs have their screen + voice; the two arcs read in order; the manual's opening and
  the "eleven years" note land.

### S4 — the companion, only if wanted

- The old idea's travel/parking is **deferred**. The screen's perimeter re-dock is the light version. If
  the card should persist during play, the only version that fits is field-scoped: it stays at its
  perimeter spot, never follows the head, and returns for the next job. No servo travel, no parking spots,
  no camera-edge entry.
- **Exit:** a decision, not code, until S1–S3 have been played.

## 4. What it touches

- `SpaceShapingController` (`StartPuzzle`/`NextPuzzle`/`ResetPuzzle`): sequence the intro; extract a
  `JobIntroPresenter` if the controller would pass 250 lines.
- `ContractScreenView` (new): authored screen, show/hide, perimeter placement/re-dock, visibility gate.
- `JobIntroPresenter` (new, same GameObject): screen + voice sequencing, one job at a time.
- `TTSHelper` / `LocalizedTTSVoice` / `MarkdownTTSVoiceWrapper` (reuse): add a visibility stop/resume if
  the current API lacks it.
- `JobIntroData` (new) and the `Levels`/`Jobs` localization entries.

## 5. Tests

- The screen shows on job start/next, hides on play, and does not replay on reset.
- The screen is authored (no runtime creation), stays on the field's perimeter, and never covers the
  run/cube.
- The re-dock moves along the perimeter, not across the field, and happens at most once.
- The voice stops when the screen hides; looking away stops it; returning resumes the same entry; the next
  job does not overlap.
- The seven intros exist and their keys resolve; no empty entries.

## 6. Out of scope

- Continuous orbit/camera-follow, servo SFX, and room collision.
- New voices or recordings; the existing Jim voice.
- Redesigning the card design system.
- The level selector/map; that is a separate run-authoring plan.

## 7. Risks

- **The screen reads as a puzzle object.** Keep it flat/emissive, on the UI layer, with no collider or
  gravity, outside the cube's reach; exclude it from the visibility raycast.
- **The screen competes with the read.** Peripheral, low contrast, intro-only; no motion during the solve.
- **The re-dock distracts.** One re-dock, capped speed, peripheral path; if it reads as chasing, drop it to
  a static spot.
- **Tone.** A quiet, slightly broken corporate display, not a flashy jumbotron; the bleak tone stays.
- **Audio overlap.** Stop the previous voice before the next job; use a generation token for the
  visibility resume.
- **Localization keys.** The markdown processer sorts keys; keep the manual/tech order and test the
  fragment.
