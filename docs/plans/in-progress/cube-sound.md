# Cube sound — the synth voice of the Wrap

**Status:** now · presentation · updated 2026-11-09 — the cube's minimum sound set from the PX synth
engines in `_Ignore/Synths`: a live stretch tone, grab and release, and the job's cues. Road to v1 step
4; sound only — hand tracking has no haptics.

**Related:** [pitch](../../design/pitch.md) · [the cube](../../design/systems/cube.md) ·
[solve moment](solve-moment.md)

## Goal

The cube is silent, and footage without sound reads as a prototype. The Wrap is a machine; give it a
synth voice whose pitch *is* the stretch, so the pinch is heard as well as seen.

## Why the PX synths

The engines (`pxStrax`: square + triangle oscillators, a Moog-style filter, a waveshaper, an LFO and an
AR envelope) generate audio live in `OnAudioFilterRead`. That suits the stretch: the zone's scale can
drive pitch and cutoff every frame, which a clip cannot do. Every cue comes from one voice, so the game
gets one sound identity with no audio files.

What needs fixing before it ships:

- **Sample rate is hard-coded to 44100;** the Quest runs at 48000, so pitch and envelope times drift.
  Read `AudioSettings.outputSampleRate`.
- **The output loop assumes two channels.** Respect `channels`.
- **CPU:** per-sample C# on the audio thread. One stretch voice and two cue voices; measure on the
  Quest 3S.
- **Location:** the engines move out of `_Ignore` to `Assets/_Modules/Audio/Vendor/PxSynth` (vendor code,
  kept as is apart from the fixes); demos, `generator.cs` and the samples stay behind.

## Contract

- **The stretch tone follows the scale:** grow raises the pitch and opens the filter, shrink lowers it.
  It plays only while pinching.
- **The tone strains as entropy drains:** the LFO wobble deepens as the budget runs low, so the spend is
  heard (pillar 4: the field shows the spend).
- **The map cube sounds the same.** Its pinch-to-enter uses the same stretch voice, since it is the same
  cube.
- **Licence:** the PX synths are free to use.
- **The cues:** grab tick, release, arrive chime, crash, fail sting, out-of-entropy refusal, star tick.
  Short, quiet, dry; nothing louder than Jim's voice.
- **Patches are data.** Each cue is an authored patch (notes, envelope, filter), tuned in the inspector.
- **The flow states trigger the job cues;** the cube view owns only its own voice (grab, stretch,
  release).

## Units

### U1 — bring in the engine

- Move `pxStrax`, `pxLope`, `pxLFO` to the vendor module; fix the sample rate and channels.
- **Exit:** one Strax voice plays a note on the Quest with no crackle and no frame cost.

### U2 — the stretch voice

- A cube sound view on the puzzle prefab: pinch start → grab tick and a sustained voice; the zone's
  scale drives pitch and cutoff each frame; release → key off and a release blip.
- The same voice on the map cube's pinch-to-enter.
- **Exit:** pinching sounds like stretching; the tone tracks the scale both ways and wobbles near an
  empty budget; the map pinch sounds the same.

### U3 — the cues

- Authored patches for the arrive chime, crash, fail sting, refusal and star tick, played by a small cue
  player (two pooled voices). `PuzzleCompleteState` and the controller's events trigger them.
- **Exit:** every beat in the loop has its sound; the solve moment's chime and ticks play in sequence.

## Manual check (headset)

1. A pinch ticks, then a tone stretches with the scale: up when growing, down when shrinking.
2. Releasing stops the tone cleanly, with no click or hang.
3. Arrive, crash, fail and the out-of-entropy refusal each have a distinct cue.
4. The star ticks line up with the stamp.
5. No crackle and no frame drop on the Quest 3S during a long pinch.
6. Nothing masks Jim's voice; the music bed still sits under it.
