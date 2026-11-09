# Pause — a clean stop behind the system menu

**Status:** now · flow · updated 2026-11-09 — the game pauses cleanly behind the system menu and when
the headset comes off. The judges' one-bus-stop test (clean pause/resume). Split from
[offline](../backlog/offline.md); do this one first.

**Related:** [offline](../backlog/offline.md) · [the first 30 seconds](first-30-seconds.md) ·
[plans board](../README.md) (Road step 8, judge tests)

## Goal

Nothing listens to pause today, so Jim keeps talking, a ship in flight keeps flying and a held gesture
can commit behind the system menu.

## Contract

- **Pause stops the moment.** With the system menu open, the voice, the sound and the ship stop. On
  resume the interrupted line replays from its start and the rest continues.
- **Nothing commits behind the menu.** The solve is never decided, no job is entered or left, and no
  scale lands while paused.

## Units

### U1 — the pause controller

- `PauseController` is an app-level component (on `Main` in `Game.unity`), not a module. It listens to
  the Unity messages itself — `OnApplicationFocus` and `OnApplicationPause` — and feeds both into one
  idempotent transition. `OVRSessionHelper` stays as it is.
- Pause sets `Time.timeScale = 0` and `AudioListener.pause = true`; resume reverses both. Music and the
  synth voices freeze on the paused listener and resume.
- Ship flight is scaled time (`ShipTravelView.cs:75,97,113`), so `timeScale = 0` already keeps
  `OnFlightCompleted` from deciding the solve.
- **Exit:** open the menu mid-flight: the ship and the sound stop; close it: they continue.

### U2 — the voice replays

- Pause is TTS's own: `TtsPresenter` handles the same Unity messages, stops on pause and restarts the
  interrupted request from its first line on resume. It remembers the last `Play(...)` and replays it,
  and the receipt's `onComplete` must not fire while paused. This sidesteps the realtime/unscaled TTS
  chain (`LocalizedTTSVoice.cs:72-82`, `StreamingAssetsSpeaker.cs:49`) that would otherwise skip lines
  behind the menu.
- **Exit:** open the menu mid-line: Jim stops; close it: the line replays from its start, and a solve's
  return to the map still happens once.

### U3 — gate the unscaled clocks

- Input: `TwoHandGestureInput` handles the same Unity messages, skips its `Update` while paused and
  clears its pinch timers/edges, so a held scale-out cannot exit the job and nothing is stale at resume
  (`Time.unscaledTime` at `:158,165,169`).
- Puzzle: skip `SpaceShapingController.LateUpdate` (`:102`) while `Time.timeScale <= 0` (its preview
  clock is unscaled, `PuzzlePreview.cs:14-20`).
- Map: `PuzzleMapView.Update` ticks on `Time.unscaledDeltaTime` (`PuzzleMapView.cs:78`), so a committed
  pinch-to-enter (`MapEnter`) finishes and enters the job behind the menu. Skip it while paused.
- The other unscaled timers (`FieldGlitch`, `FieldDust`, `JobCardTypewriter`, `GesturePromptPanel`,
  `RunSlotView`, `CubeVoice`) are cosmetic and can run on.
- **Exit:** open the menu mid-scale and mid-enter: nothing commits; close it: the field and the map are
  where you left them.

## Manual check (headset)

1. Mid-flight, open the system menu: the ship and the voice stop; close it: the ship continues and
   Jim's interrupted line replays from its start.
2. Mid-scale with both hands pinched, open the system menu: nothing commits and the job does not change
   behind the menu; close it: the field is where you left it.
3. On the map, start a pinch-to-enter and open the menu: the job is not entered behind it.
4. Take the headset off mid-job and put it back on: the job is where you left it.
5. Pause during Jim's solve receipt: on resume it replays and the map opens once.
