# Lessons

Short post-mortems, written when a plan closes (rule in [AGENTS.md](../AGENTS.md) §Plans are a
morning). Newest first. Each entry: what worked, what surprised us, the rule for next time. One idea per
bullet; the code and tests are the record of what was built.

An entry marked *(in progress)* was written while its plan was still open. Finish it when the plan
closes.

## 2026-11-10 — Soundscape *(in progress)*

- **Worked:** one owner per beat. Job entry used to stack a sweep, a glitch blip, a panel blip and
  prompt blips; giving the moment to `PuzzleStartState` alone made it read as one sound.
- **Worked:** ticks and transitions play differently. Transitions cut (`Play`), ticks never do
  (`PlayOver`), so a map tick cannot kill the return sweep.
- **Surprised:** the shared synth carried this game's tuning. `SynthVoice` had cube-only stretch
  methods and the cube's timbre as defaults, so another project could not use it as is.
- **Next time:** a `_Modules` type exposes verbs; the game's tuning lives in data assets the game owns
  (here, `SynthPatch` tones under `Features/Audio/Synth/Tones`).
- **Next time:** measure the first continuous voice on the Quest 3S before adding the second. Each
  synth voice is per-sample C# plus a Meta XR source.

## 2026-11-10 — The first 30 seconds *(in progress)*

- **Worked:** the boot is the loader. Showing each setup step as it runs (`LINK`, `SEAT`, `LEDGER`)
  turns the wait into content, and a slow step is visible instead of a frozen screen.
- **Worked:** no-op virtuals on `BaseSplashScreen` let `SetupState` report steps without changing the
  other splashes.
- **Surprised:** placing at setup used wherever the headset was pointing, often at the desk. Placement
  has to wait until the headset is worn and the head pose has settled.
- **Surprised:** putting the headset on restarted the placement wait and dropped setup's callback,
  so the boot hung on `> SEAT`. A step that can be restarted by a system event keeps its caller's
  callback outside the coroutine.
- **Next time:** treat launch as the first moment of play. The judge's clock starts at the app icon,
  not at job 1.

## 2026-11-10 — Pause *(in progress)*

- **Surprised:** `Time.timeScale = 0` stops much less than it looks. Input, the map's pinch-to-enter,
  the puzzle preview and the TTS chain all run on unscaled time and kept going behind the menu.
- **Next time:** when adding pause, list every `Time.unscaled*` reader first and decide for each one:
  gate it, or let it run as cosmetic.
- **Next time:** a system that owns a timeline (TTS) handles its own pause and replay; a global pause
  flag cannot rewind a line.
