# TTS Voice Panel — Camera-Operator Companion

**Status:** idea · presentation · revised 2026-11-06 — the camera-operator companion is deferred; the
current fit is the jumbotron TTS panel ([plan](../plans/in-progress/tts-messages.md)).

**Related:** [jobs](../design/jobs.md) · [TTS messages plan](../plans/in-progress/tts-messages.md)

**Current fit (2026-11-06).** The game is seated MR with a state-launched TTS panel, not a persistent
card. The [TTS messages plan](../plans/in-progress/tts-messages.md) builds the jumbotron: an authored
field-side panel with the speaker's face that stays at the side and does not follow or gate on visibility.
The travel/parking below is a later feel-pass variant, only if the panel should persist during play.

## Goal

Turn the TTS voice panel into a persistent, character-like companion that behaves like a camera operator:
it enters from the nearest view edge, stays world-anchored in the lower third while it speaks, travels
back into view when the player turns away instead of teleporting, and parks out of sight between
sessions. Speech stops while it is fully out of view and restarts the current entry when it is seen
again. This is a presentation and speech-gating change — no new dialogue, voices, or localization.

## The three states

| State | Player sees | Behaviour |
|---|---|---|
| **Dormant** | Nothing | Before the first session or when disabled |
| **Speaking** | Full panel with face and text | Enters if needed, stays in view, speech plays |
| **Idle parked** | Nothing in view | Holds the idle expression, parked outside view and hands |

A session starts on a state-launched cue (`TtsPresenter`): cancel parking → enter from the nearest edge →
speak the entries → show the idle expression → after `idleParkDelaySecs`, park outside the view and the
hand volume → stay parked until the next cue.

## The rules that matter

- **Only fully out of view triggers a move while speaking.** Start `offscreenGraceSecs`; a quick glance
  cancels it. After the grace, travel; the current entry is stopped and restarts when visible again.
- **Gate speech on visibility, not on travel completion** — the speaker is spatial and attached to the
  panel, so offscreen speech would sweep through the room.
- **The travel path stays peripheral** and never crosses the center of the view; cap angular speed and
  allow one correction hop, never retarget every frame.
- **The idle panel does not chase the view.** Looking at the parked spot does not make it flee; it
  re-parks only when the player moves away.
- **Small camera translation follows with a deadzone and no sound**; only a real travel plays the
  movement SFX.

## What it touches

`SubtitlesLikeFollow.cs` (serves the splash — give the TTS panel its own authored instance) ·
`LocalizedTTSVoice.cs` (entry text/complete port; no resume in the current plan) ·
`StreamingAssetsSpeaker.cs` (Stop only) · `VoicesPlayer.cs` / `MarkdownVoices.cs` (scene-wired; no
runtime `AddComponent`) · `PuzzleStartState` / new `PuzzleCompleteState` (launch the cues).

## Non-goals

- New dialogue, voice recordings, or streaming TTS.
- Grabbing/dragging/physics on the panel; full room collision avoidance.
- Persisting panel state across sessions.
- Redesigning the game-end panel, store UI, or the card design system.

## Risks to resolve first

| Risk | Why now |
|---|---|
| Speech starts while parked or offscreen | Spatial speaker at the parked spot sweeps the room. Gate on `IsInView`; seed visibility before speaking. |
| Restart double-advance | Stopping a coroutine and restarting races the old completion callback. A generation token is needed only if resume returns. |
| No valid parking spot | Tight rooms can invalidate every candidate. Check clearance, clamp above the floor, fall back to the last valid spot. |
| Panel collider corrupts occlusion | The panel's own trigger is hit by visibility rays. Move it to the UI layer and exclude UI from the mask. |
| Shared `SubtitlesLikeFollow` | The splash depends on the same component. Give the TTS panel its own instance and keep the splash path intact. |

## Open questions

1. **Movement sound** — soft servo/whoosh, a UI chime, or silence? (plan: quiet servo under SFX)
2. **Physical walking** — follow with a deadzone, or only on view-direction change?
3. **Occlusion** — should a wall stop and restart the current entry? (plan: yes, with a generous fallback)
4. **Top/bottom entry** — wanted, or left/right only? This is where the camera-operator feel can become distracting.
