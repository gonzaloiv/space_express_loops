# TTS Voice Panel — Camera-Operator Companion

**Status:** idea · presentation · updated 2026-10-30 — recorded, design only. Not scheduled.

**Related:** [presentation](../design/presentation.md) · [jobs](../design/jobs.md)

## Goal

Turn the TTS voice card into a persistent, character-like companion that behaves like a camera operator:
it enters from the nearest view edge, stays world-anchored in the lower third while it speaks, travels
back into view when the player turns away instead of teleporting, and parks out of sight between
sessions. Speech stops while it is fully out of view and restarts the current entry when it is seen
again. This is a presentation and speech-gating change — no new dialogue, voices, or localization.

## The three states

| State | Player sees | Behaviour |
|---|---|---|
| **Dormant** | Nothing | Before the first session or when disabled |
| **Speaking** | Full card with avatar and text | Enters if needed, stays in view, speech plays |
| **Idle parked** | Nothing in view | Holds the idle expression, parked outside view and hands |

A session starts on the first `TTSHelper.Say(...)`: cancel parking → enter from the nearest edge →
speak the entries → show the idle expression → after `idleParkDelaySecs`, park outside the view and the
hand volume → stay parked until the next `Say`.

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

`SubtitlesLikeFollow.cs` (owns visibility and the panel motion — it also serves `GameEndPanel`, so its
instant `ShowInCameraView()` path must stay intact) · `TTSHelper.cs` (owns the visibility↔speech
binding) · `LocalizedTTSVoice.cs` (needs a `SetVisible(bool)` that stops and restarts the current
entry, with a generation guard against double-advance) · `StreamingAssetsSpeaker.cs` (Stop only, no
pause) · `GameStartState` / `EditionState` (drop the old `SetInFrontOfCameraOrDefault` calls).

## Non-goals

- New dialogue, voice recordings, or streaming TTS.
- Grabbing/dragging/physics on the panel; full room collision avoidance.
- Persisting panel state across sessions.
- Redesigning the game-end panel, store UI, or the card design system.

## Risks to resolve first

| Risk | Why now |
|---|---|
| Speech starts while parked or offscreen | Spatial speaker at the parked spot sweeps the room. Gate on `IsInView`; seed visibility before speaking. |
| Restart double-advance | Stopping a coroutine and restarting races the old completion callback. Generation token in `LocalizedTTSVoice`. |
| No valid parking spot | Tight rooms can invalidate every candidate. Check clearance, clamp above the floor, fall back to the last valid spot. |
| Panel collider corrupts occlusion | The panel's own trigger is hit by visibility rays. Move it to the UI layer and exclude UI from the mask. |
| Shared `SubtitlesLikeFollow` | `GameEndPanel` depends on the same component. Keep its instant path side-effect free. |

## Open questions

1. **Movement sound** — soft servo/whoosh, a UI chime, or silence? (plan: quiet servo under SFX)
2. **Physical walking** — follow with a deadzone, or only on view-direction change?
3. **Occlusion** — should a wall stop and restart the current entry? (plan: yes, with a generous fallback)
4. **Top/bottom entry** — wanted, or left/right only? This is where the camera-operator feel can become distracting.
