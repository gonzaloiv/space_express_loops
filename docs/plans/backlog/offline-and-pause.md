# Offline and pause — the launch's other judge tests

**Status:** next · flow · updated 2026-11-09 — the game runs with no internet (progress kept locally,
mirrored to the cloud when online) and pauses cleanly behind the system menu. The judges' one-bus-stop
test: fast cold start, clean pause/resume.

**Related:** [the first 30 seconds](first-30-seconds.md) · [plans board](../README.md) (Road step 8,
judge tests)

## Goal

Two ways a judge loses the game before playing it. **No connection:** `SetupState`'s internet check
waits for Unity Services, then shows "internet required" and quits; saves go only to Cloud Save. **The
system menu:** nothing listens to pause, so Jim keeps talking and a ship in flight keeps flying.

## Contract

- **Local is the source of truth.** Progress is always written to the device first; the cloud is a
  mirror when it is reachable. Losing the connection never loses a solve.
- **No internet panel on the critical path.** Offline, the game starts as fast as online.
- **One client for the callers.** The flow and the prompt recorder keep one injected client; which store
  it hits is the client's business.
- **Pause freezes the moment.** With the system menu open, the voice, the sound and the ship stop; on
  resume they continue where they were. The solve is never decided while paused.

## Units

### U1 — the local client and the wrapper

- A PlayerPrefs client (JSON per type, the same `Get<T>` / `Put<T>` shape as `IRemoteClient`).
- A wrapper client: reads and writes local first; when Unity Services are ready, mirrors each `Put`
  to Cloud Save, and on launch takes the cloud copy only when it holds more progress than the local
  one.
- `Main` registers the wrapper; `PuzzleCompleteState`, `GestureLearningRecorder` and the session
  snapshot use it instead of `UnityPlayerDataClient`.
- **Exit:** solve offline, relaunch offline — the progress is there; go online — it reaches the cloud.

### U2 — launch without internet

- `InternetChecker` stops being a gate: with the local player data ready, the setup continues without
  waiting for the services timeout; the "internet required" panel leaves the critical path.
- **Exit:** airplane mode → launch → the boot plays and job 1 opens with no panel and no wait.

### U3 — pause and resume

- Subscribe to the existing `OVRSessionHelper` pause/unpause: pause the audio (voice, music, synth) and
  the game clock while the system menu is open; resume on return.
- **Exit:** open the menu mid-flight and mid-voice — both stop; close it — both continue.

## Manual check (headset)

1. Airplane mode, erase the save, launch: no panel, the boot plays, job 1 opens.
2. Solve job 1 offline, quit, relaunch offline: job 1 is solved on the map.
3. Back online: the progress reaches the cloud (the next launch online keeps it).
4. Mid-flight, open the system menu: the ship and the voice stop; close it: they continue.
5. Take the headset off mid-job and put it back on: the job is where you left it.
