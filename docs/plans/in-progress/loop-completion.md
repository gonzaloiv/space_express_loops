# Loop completion — solve, fail, return

**Status:** now · flow · updated 2026-11-06 — U1–U3 landed; open: lock the cube's zone gesture while
frozen, log a failed save, restart persistence (U4), and a fresh save starting straight in job 1 (U5).

**Related:** [core loop](../../design/core-loop.md) · [progression](../../design/systems/progression.md) ·
[TTS messages](tts-messages.md) · [vision](../../design/vision.md) §7

## Goal

Make the run playable end to end so Validation 1 can run. Today a solve is not saved (the map never
fills), a spent budget leaves the player stuck, the puzzle stays live during the receipt, and job 1
opens with Jim talking.

## Contract

- A job ends **once**: on a solve, or on a fail (budget spent and the ship did not arrive). Job 1
  never fails and has **infinite spend**: `NoFail` removes the budget cap (`IsOverBudget` is false), so
  nothing clamps or refuses an edit; the player keeps shaping until the solve.
- On a solve the progress is saved: the solved cookie plus the grade (stars). On return, the next job
  is selected (`PuzzleMap` selects `progress.CurrentIndex`). Per-node state on the map is
  [map visibility](../backlog/map-visibility.md), not this plan.
- A fail saves nothing. The player returns to the map and can pick the job again; Retry is a later
  call.
- From the end of the job until the return, the run is frozen: no pinch, no spend, no relaunch.
- The intro plays only the tech line (`_01`); the card owns the manual entry. Job 1 has no intro.
- A fresh save (nothing solved, index 0) opens job 1 straight from selection; the map shows after its
  solve, and on every later entry.
- The complete and fail cues are Jim's. A missing key is **skipped** (`TtsPresenter` completes the
  sequence next frame), so a job without its `_complete` returns to the map at once; accepted until the
  content pass. Only an empty key list waits `emptyCueSecs`.

## Units

### U1 — end the job once

- `SpaceShapingController` raises `Failed` when a checked flight leaves the session failed
  (`IsFailed`), mirroring `Solved`, fired once per run.
- `PuzzleEditionState` routes both to the complete state and passes the outcome on.
- The controller **freezes** the run on either outcome: gestures are ignored until `Exit`.
- **Exit:** a forced over-budget job 2 ends and leaves the edition state; job 1 never fails and spends
  without limit; a pinch after the solve does nothing.

### U2 — save the solve

- On a solve, write `new LevelCompleteCookie(PuzzleData.id)` with the session `Grade()` as its score —
  the module standard; `PuzzleData.id` is `01_01`, so the cookie key is `level_complete_01_01`, the key
  `RunProgressCookies.Build` already reads. The **writer is new**: add it beside `Build` in
  `RunProgressCookies`.
- `PlayerData.AddCookie` never replaces, so a re-solve updates the existing cookie's score when the
  new grade is higher (best grade wins). `noGrade` jobs (1, 2) save score 0.
- Persist through `UnityPlayerDataClient.Put`, as `CookieHelper` does.
- A fail writes nothing.
- **Exit:** solve job 1 → the map shows job 1 solved and job 2 current; the progress survives a
  restart; EditMode test for the cookie write and the best-grade rule.
- **Risk:** `Main` starts from a fresh `PlayerData` unless `forceLocalPlayerData` is set
  ([Main.cs](../../../Assets/_Modules/Casual/Flow/Main.cs)); confirm the load path before trusting the
  restart check.

### U3 — the cues

- The complete state plays `_complete` on a solve and `_fail` on a fail, then returns to the map.
- `PuzzleStartState` plays only `<intro>_01`, and nothing when the puzzle opts out of the intro.
- `noIntro` follows the `noFail` chain: `PuzzleJobContract.NoIntro` → `PuzzleSeed.noIntro` →
  `PuzzleBrief.NoIntro` → `PuzzleData.noIntro`, with a `PuzzleJobValidator` check against the contract.
  Job 1 sets it; rebake Chapter 01. Not inferred from `noFail` or the index.
- The `_complete` and `_fail` keys for jobs 2–7 are in `Levels` (en + es, first pass); the final copy
  and recordings are TTS S5.
- **Exit:** job 1 starts silent; job 2 plays one tech line; a solve plays the receipt, a fail plays the
  fail line; no cue overlaps the next job.

### U4 — close the gaps (open)

Review of the landed U1–U3 (2026-11-06):

- **Freeze the cube too.** The controller ignores input once frozen, but the zone gesture lives on the
  view (`SpaceShapingView` → `spawner.Zones`): a pinch during the receipt still scales the cube's inner
  volume and then does nothing. On freeze, lock the zones (the same path as a locked gesture, no
  refusal flash) or hide the cube; unlock on `Exit`.
- **Log a failed save.** `PuzzleCompleteState.SaveSolve` is `async void`; catch and log a failed
  `UnityPlayerDataClient.Put` so a lost save is visible. The in-memory cookie still makes the map
  correct for this session.
- **Confirm restart persistence.** `Main` still seeds a fresh `PlayerData` unless
  `forceLocalPlayerData` is set; find where the saved data loads back on launch, and fix it if nothing
  does. Manual step 7 decides it.
- **Exit:** pinching during the receipt shows no cube response; a forced `Put` failure logs once;
  manual step 7 passes on device.

### U5 — first session goes straight into job 1

- On a fresh save (`RunProgress.IsFresh`: nothing solved, job 1 current), the **first** entry into
  `PuzzleSelectionState` in an app session skips the map and enters job 1 directly — the player meets
  the cube before the map.
- Only that first entry: leaving job 1 with the scale-out gesture, or returning after its solve, shows
  the map. So the map is first seen with job 2 selected, and its two-hand spread to enter is the verb
  job 1 just taught.
- The decision lives in `PuzzleSelectionState` (flow), reusing `EnterJob`; a session flag (not saved)
  marks that the auto-enter already ran. The debug `selectRandom` / `puzzleToSet` paths keep priority.
- **Exit:** fresh save → app start lands in job 1 with no map; scale-out from job 1 shows the map;
  solve job 1 → the map shows with job 2 selected; a save with job 1 solved starts on the map.

## Manual check (headset)

1. Fresh save: the app starts straight in job 1 (no map); no voice before the first pinch.
2. Solve job 1: the receipt plays; pinching during it does nothing — the cube does not respond.
3. Back on the map: job 2 is selected; pan to job 1 and the selection cube reads solved.
4. Open job 2: one tech line plays; the puzzle is live under it.
5. Spend the full budget without arriving: the fail line plays and the map returns; job 2 is still
   current.
6. Solve job 3 (the first graded job) with a low grade, then again with a higher one: the map keeps
   the higher grade.
7. Quit and relaunch: the map shows the same progress.

## Out of scope

- The result readout/grade card on complete (the reward look); the stars show on the map.
- Retry from the complete state; the map stays the only navigation.
- The in-field entropy bar and the read pass — the next plan, after Validation 1.
- Recordings and final copy (TTS S5); the reading timer, skip removal and TTS cleanup (TTS S3–S4).
