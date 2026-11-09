# Soundscape — the cube is not the only sound

**Status:** next · presentation · updated 2026-11-09 — a sound bed around the cube's stretch tone: a mix
pass, the field's hum, the ship's flight, and the system's ticks and transitions. Follows
[cube sound](../in-progress/cube-sound.md); same PX synth engine, same one-voice identity.

**Related:** [cube sound](../in-progress/cube-sound.md) · [solve moment](../in-progress/solve-moment.md) ·
[pause](../in-progress/pause.md) · [plans board](../README.md) (Road step 4)

## Goal

Today you hear the music loop, Jim, the job cues and the stretch tone. While playing, the stretch is the
only thing that answers, so it sounds too loud and too alone. Give the field, the ship and the console
their own quiet voices so the stretch sits inside a machine that is already humming.

## Contract

- **The stretch stays the lead, but quieter.** Every new layer sits under it; nothing is louder than
  Jim, and the music ducks under him.
- **One key.** Tonal layers are in A, like the stretch (`CubeVoice.baseNote` 57), so they blend instead
  of clashing.
- **The field speaks, not the panels.** The hum and the flight come from the field, as 3D sources; the
  console sounds are short, dry ticks.
- **Patches are data.** New cues are `SynthPatch` assets under `Features/Audio/Synth/Cues`; sustained
  layers are `SynthVoice` components tuned on the prefab.
- **The flow states trigger beats; views own their continuous voices.** As in cube sound: transitions
  and job beats come from states, while the hum and the engine belong to their views.
- **System sounds have their own cue player.** `SynthCuePlayer` cuts the previous cue, so a typing tick
  would cut the arrive chime. UI ticks go through a second player (`Features/Cues/System`) with its own
  two voices.

## Units

### U1 — the mix pass

- Lower the stretch: `SynthVoice.volume` on `Cube.prefab` 0.22 → ~0.12 (tune by ear after U2).
- Duck under Jim: on `MainMixer`, a Send from the `TTS` group into a Duck Volume on the `Music` group
  (and a lighter duck on `SFX`). Authored in the mixer; no code.
- **Exit:** with Jim speaking over the music and a pinch, his words stay clear and the stretch no longer
  jumps out.

### U2 — the field hum

- A sustained `SynthVoice` on `Puzzle.prefab` (3D, at the field centre): a low A (note 33–45), a closed
  filter, a slow LFO. `SpaceShapingView` keys it on when the job opens and off when it closes.
- The strain the readout already pushes (`SpaceShapingView.ShowReadout` → `SetStrain`) also opens the
  hum's filter and speeds its LFO a little, so a drained budget is heard in the room, not only in the
  cube.
- While a pinch is held, the hum dips slightly (sidechain feel) so the stretch reads as the field
  straining.
- **Exit:** a job has a quiet, constant presence; the hum changes as entropy drains; it stops on the map.

### U3 — the ship's flight

- A launch cue (a short rising sweep) when each flight starts, and a soft engine tone on the ship prefab
  (3D `SynthVoice`) while it flies, its pitch following the ship's speed along the path
  (`ShipTravelView.FlyAlong`).
- Arrive: the engine falls and keys off when the ship parks (the arrive chime takes over). Crash/miss: it
  keys off with the existing shrink.
- **Exit:** every flight is heard from launch to end; a fast slingshot sounds faster than a slow drift.

### U4 — the system: ticks, UI and transitions

- **UI enter:** a short open blip when a console window appears — the job card (`JobCardPanel.Show`),
  Jim's panel (`TtsPresenter.Play`), a gesture prompt (`GesturePromptPanel.Show`).
- **Typing:** a quiet tick per line from `JobCardTypewriter`, at most one every ~60 ms.
- **The glitch:** a blip on `FieldGlitch.Play`, either the existing
  `Audio/SFX/657809__the-sacha-rush__tech-glitch-2-blip.wav` or a noisy synth patch.
- **The map:** a snap tick when the map cube settles on a job (`MapCube.SnapTo`).
- **View transitions:** map → job, a rising sweep on entry (`PuzzleSelectionState.EnterJob`); job → map,
  a falling sweep on return (`PuzzleCompleteState.ReturnToSelection`, `PuzzleEditionState` exit to
  selection). States trigger these through the system cue player.
- Views raise events (`Opened`, `LineTyped`, `Snapped`); a state or a small scene-wired listener plays
  the cue. Views do not reference the cue player.
- **Exit:** opening the map, entering a job, cards typing and returning all have their own short sound.

**Budget:** at most ~7 PX voices live at once (one stretch, hum, one or two engines, two job cues, two
system cues). It's per-sample C# on the audio thread: measure on the Quest 3S after U3, and stop sources
when they're silent (`SynthVoice` already does).

## Manual check (headset)

1. Enter a job: transition sweep, card blip and typing, then the hum is there under Jim.
2. Jim speaks over the music: the music ducks and his words are clear.
3. Pinch: the stretch leads but no longer jumps out; the hum dips under it.
4. Release: the ship launches with a sweep, the engine follows its speed, and stops cleanly at the
   park / crash.
5. Drain the budget: the hum and the stretch both strain.
6. Return to the map: the falling sweep plays, the hum stops, and the map cube ticks when it snaps.
7. No crackle or frame drop on the Quest 3S during a long pinch with a flight in progress.
8. Tuning: the overall level of the new layers — under the stretch, never louder than Jim.
