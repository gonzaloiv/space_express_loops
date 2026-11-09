# Pause — a clean stop behind the system menu

**Status:** now · flow · updated 2026-11-09 — the game pauses cleanly behind the system menu and when
the headset comes off. The judges' one-bus-stop test (clean pause/resume). Split from
[offline](../backlog/offline.md); do this one first.

**Related:** [offline](../backlog/offline.md) · [the first 30 seconds](../backlog/first-30-seconds.md) ·
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

- Focus-aware is on (`OculusProjectConfig focusAware: 1`), so the existing
  `OVRSessionHelper.hasPaused/hasUnpaused` fire behind the system menu. The helper is already on the rig
  in `Game.unity`; feed both `OnApplicationFocus` and `OnApplicationPause` into one idempotent
  transition so the menu and the headset coming off cannot double-fire. No new scene component.
- A shared `PauseController` (Global) holds the paused flag and sets `Time.timeScale = 0` and
  `AudioListener.pause = true`; resume reverses both. Music and the synth voices freeze on the paused
  listener and resume.
- Ship flight is scaled time (`ShipTravelView.cs:75,97,113`), so `timeScale = 0` already keeps
  `OnFlightCompleted` from deciding the solve.
- **Exit:** open the menu mid-flight: the ship and the sound stop; close it: they continue.

### U2 — the voice replays

- No pause-aware clock. On pause, stop the TTS presenter; on resume, restart the interrupted request
  from its first line. `TtsPresenter` remembers the last `Play(...)` and replays it, and the receipt's
  `onComplete` must not fire while paused. This sidesteps the realtime/unscaled TTS chain
  (`LocalizedTTSVoice.cs:72-82`, `StreamingAssetsSpeaker.cs:49`) that would otherwise skip lines behind
  the menu.
- **Exit:** open the menu mid-line: Jim stops; close it: the line replays from its start, and a solve's
  return to the map still happens once.

### U3 — gate the unscaled clocks

- Input: `TwoHandGestureInput` still runs at `timeScale = 0` and its dwell/grace timers use
  `Time.unscaledTime` (`TwoHandGestureInput.cs:158,165,169`), so a held scale-out can exit the job while
  paused. Gate it on the paused flag and clear its pinch timers/edges on pause, so nothing is stale when
  focus returns.
- Puzzle: skip `SpaceShapingController.LateUpdate` (`:102`) while paused (its preview clock is
  unscaled, `PuzzlePreview.cs:14-20`).
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
