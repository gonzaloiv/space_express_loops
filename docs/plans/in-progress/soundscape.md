# Soundscape — the cube is not the only sound

**Status:** now · presentation · updated 2026-11-09 — a sound bed around the cube's stretch tone: a mix
pass, the field's hum, the ship's flight, and the system's ticks and transitions. Builds on the cube
sound (`CubeVoice`, `SynthVoice`, `SynthCuePlayer`); same PX synth engine, same one-voice identity.
Revised after review (2026-11-09).

**Related:** [the cube](../../design/systems/cube.md) ·
[pause](pause.md) · [plans board](../README.md) (Road step 4)

## Goal

Today you hear the music loop, Jim, the job cues and the stretch tone. While playing, the stretch is the
only thing that answers, so it sounds too loud and too alone. Give the field, the ship and the console
their own quiet voices so the stretch sits inside a machine that is already humming.

## Contract

- **The stretch stays the lead, but quieter.** Every new layer sits under it; nothing is louder than
  Jim, and the music ducks under him.
- **One tonal centre: A minor.** The cues are already C major / A minor (`CueArrive`, `CueFail`) and
  the stretch rests on E3 (`CubeVoice.baseNote` 52), the fifth of A. The hum sits on A; the engine and
  new cues stay inside A minor. The stretch's note does not change.
- **The field speaks, not the panels.** The hum and the flight come from the field; the console sounds
  are short ticks.
- **Field sources are Meta XR Audio sources; cues are 2D.** The cube, hum and ships use
  `MetaXRAudioSource` and are spatialized. The job cue voices (`Cues`) and the system voices
  (`System Cues`) stay 2D: they are the console's voice, not a place in the room (decided 2026-11-09).
  Field layers (hum, engine) route to `Scene SFX`; cues and system ticks to `SFX`. Meta XR acoustics is a
  per-source cost: it counts in the budget below.
- **Patches are data.** New cues are `SynthPatch` assets under `Features/Audio/Synth/Cues`; sustained
  layers are `SynthVoice` components tuned on the prefab.
- **The flow states trigger beats; views own their continuous voices.** Transitions and job beats come
  from states; the hum belongs to `SpaceShapingView`, the engine to `ShipTravelView`.
- **One sound per moment.** A beat has one owner and the rest stay quiet (see job entry in U4). Ticks
  never cut a cue, and a cue never cuts a tick.

## Units

### U1 — the mix pass

- The stretch stays at 0.16: with the reworked tone and the new layers under it, it sounds right
  (decided 2026-11-09).
- **Deferred to Road step 7 (voice and content):** the duck under Jim — on `MainMixer`, a Send from the
  `TTS` group into a Duck Volume on `Music` (and a lighter one on `SFX`). Tune it with Jim's real clips,
  not the TTS.
- **Exit:** the stretch leads without jumping out.

### U2 — the sustain API and the field hum

- **A generic sustain on `SynthVoice`.** Today `DriveStretch` is the only filter/LFO writer and sets
  `lfoAmp = strain * strainLfoAmp` (zero at rest), and there is no volume setter (`SynthVoice.cs:14`,
  `:62-73`). Add `BeginSustain(note, cutoff)`, `DriveSustain(note, cutoff, lfoAmp, lfoRate)` and
  `SetVolume(value)`; `BeginStretch`/`DriveStretch` become thin wrappers so `CubeVoice` is unchanged.
- **The hum:** a sustained `SynthVoice` on `Puzzle.prefab` at the field centre: A (note 33–45), a closed
  filter, a slow resting LFO. `SpaceShapingView` keys it on when the job opens and off when it closes.
- The strain the readout already pushes (`SpaceShapingView.ShowReadout` → `SetStrain`) also opens the
  hum's filter and speeds its LFO a little, so a drained budget is heard in the room.
- While a pinch is held (`SpaceShapingView.IsScaling`), the hum's volume eases down a little
  (sidechain feel) so the stretch reads as the field straining.
- **Measure here:** this is the first continuous voice. Check CPU on the Quest 3S with the hum and a long
  pinch before starting U3.
- **Exit:** a job has a quiet, constant presence; the hum changes as entropy drains; it stops on the map.

### U3 — the ship's flight

- **Only flights the player launches.** A committed pinch launches the ship; the start check that flies
  at job open (`SpaceShapingController.PlayStartCheck`) stays silent — job entry has its own sound.
  `Travel` gets a flag for it.
- **The launch cue** (a short rising sweep, in A minor) when a launched flight starts.
- **The engine** is `ShipTravelView`'s own `SynthVoice` on the ship prefab. Its pitch follows the ship's
  own speed: `MoveTo` moves the ship each frame along fixed-step trajectory samples, so the distance moved
  per second rises in a slingshot and falls in a drift. Smooth it a little and map it to pitch; no data
  from outside the view.
- Arrive: the engine falls and keys off when the ship parks (the arrive chime takes over). Crash/miss: it
  keys off with the existing shrink.
- **Exit:** every launched flight is heard from launch to end; a fast slingshot sounds faster than a slow
  drift; job open stays quiet.

### U4 — the system: ticks and transitions

- **A non-cutting mode for ticks.** `SynthCuePlayer.Play` stops every voice first
  (`SynthCuePlayer.cs:24-34`, `:67-71`). Add `PlayOver(patch)`: it plays on a free voice, or steals the
  oldest, and never stops the others. Ticks use it; transitions keep `Play`. The system player lives at
  `Features/Audio/Synth/System` with its own two voices.
- **Job entry has one owner: `PuzzleStartState`.** It plays the rising entry sweep. During entry nothing
  else adds a sound: no glitch blip (the entry glitch stays visual), no blip for Jim's panel, no blips for
  the gesture prompts (they appear together, `PuzzleEditionState.cs:68-75`), and the start-check flight
  is silent (U3).
- **Return to the map:** a falling sweep from `PuzzleCompleteState.ReturnToSelection` and
  `PuzzleEditionState`'s exit to selection.
- **The fail glitch:** a glitch blip only where the fail plays it (`PuzzleCompleteState.PlayFail`); the
  state plays it beside `Glitch()`, and `FieldGlitch` raises nothing.
- **The map:**
  - A select tick when the selected job changes while dragging: `PuzzleMapView` raises
    `SelectionChanged` where `map.Select(nearest)` succeeds (`PuzzleMapView.cs:214-215`); `SnapTo` fires
    only once per drag.
  - Typing ticks when the map's card types in (`MapSelectionReadout.Show` → `JobCardPanel.Show`):
    `JobCardTypewriter.Reveal` takes a per-line callback and `JobCardPanel` raises `LineTyped`. At most
    one tick every ~60 ms. The in-job card uses `ShowInstant` (`PuzzleEditionState.cs:80`): no typing
    there.
  - The card's open blip plays on the map only.
- `TtsPresenter` (a `_Modules` type) does not learn about game cues; Jim has no open blip.
- Views raise events; `PuzzleSelectionState` (map) and the job states play the ticks. Views do not
  reference the cue player.
- **Exit:** entering a job is one clean sweep; returning has its own; dragging across the map ticks per
  job; the map's card types with ticks; a fail has its glitch blip.

**Budget:** at most ~7 PX voices live at once (one stretch, hum, one or two engines, two job cues, two
system cues), each with a `MetaXRAudioSource`. Per-sample C# on the audio thread plus Meta XR acoustics
per source: measure on the Quest 3S after U2 and again after U3; stop sources when silent (`SynthVoice`
already does).

**Pause:** pause stops the listener (`AudioListener.pause`) and scaled time together, so a cue's
envelope (audio thread) and its note timing (`WaitForSeconds`) freeze together. Check that a cue cut by
the menu resumes in step.

## Manual check (headset)

1. Enter a job: one rising sweep, then the hum is there under Jim; no stack of blips; the start-check
   flight is silent.
2. Pinch: the stretch leads but no longer jumps out; the hum dips under it.
3. Release: the ship launches with a sweep, the engine follows its speed, and stops cleanly at the
   park / crash.
4. Drain the budget: the hum and the stretch both strain.
5. On the map: dragging the cube ticks once per job; the card types in with ticks; a tick never cuts the
   return sweep.
6. Fail a job: the glitch blip with the glitch, then Jim.
7. Open the system menu mid-cue and mid-hum: silence; close it: they resume in step.
8. No crackle or frame drop on the Quest 3S during a long pinch with a flight in progress.
9. Tuning: the overall level of the new layers — under the stretch, never louder than Jim.
