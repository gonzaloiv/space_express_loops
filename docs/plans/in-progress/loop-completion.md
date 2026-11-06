# Loop completion — solve, fail, return

**Status:** now · flow · updated 2026-11-06 — a job ends once, on a solve or a fail; the solve is saved
and the map fills; job 1 starts silent.

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
- On a solve the progress is saved: the solved cookie plus the grade (stars). On return, the solved
  node lights and the next node is current.
- A fail saves nothing. The player returns to the map and can pick the job again; Retry is a later
  call.
- From the end of the job until the return, the run is frozen: no pinch, no spend, no relaunch.
- The intro plays only the tech line (`_01`); the card owns the manual entry. Job 1 has no intro.
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
- Add the `_fail` keys for jobs 2–7 with placeholder text (one dry line each: "We didn't arrive. … Reset
  it."); the final copy is TTS S4.
- **Exit:** job 1 starts silent; job 2 plays one tech line; a solve plays the receipt, a fail plays the
  fail line; no cue overlaps the next job.

## Manual check (headset)

1. Fresh save: open job 1; no voice before the first pinch.
2. Solve job 1: the receipt plays; pinching during it does nothing.
3. Back on the map: job 1 lit, job 2 current, the route segment lit.
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
- Recordings and final copy (TTS S4).
