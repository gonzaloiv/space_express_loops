# Stuck-player skip — one fail opens the next job

**Status:** now · flow · updated 2026-11-11 — one fail opens the next job, signposted on the restart
readout and the map; started 2026-11-11 after [the ending](the-ending.md) landed. U1–U4 are
implemented and tested but not committed; K1 is accepted (opening chains), K2–K5 are open (§Review),
then the headset pass. The cold playtest may still revise it.

**Related:** [progression](../../design/systems/progression.md) · [the ending](the-ending.md) ·
[plans board](../README.md)

## Goal

A judge plays once and may stop early; a player stuck on one job never sees the rest. Open the way on
without moving them.

## Contract

- **One fail opens the next job.** The restart in place still offers the retry; the map offers the way
  on. Nothing reacts mid-solve. This chains: a fail on an opened job opens the one after it, so a
  stuck player can see every job (decided 2026-11-11); only solving every job completes the run.
- **Signposted, not forced.** The restart's terminal readout says the next job is open, and the Exit
  prompt shows once so the way to the map is visible. A fail never moves the player to the map; if the
  playtest still shows people stuck, the fallback is a Retry / Map offer, not a forced map.
- **Completion is untouched.** The run is complete when every job is solved; an opened job never counts
  ([the ending](the-ending.md) relies on it).
- **The map shows the opened job.** Today one node pulses (`MapFieldRenderer` keeps one `CurrentSlot`)
  and the route line ends at the first `Current` (`MapRouteLine`). With a job opened, the pulse and the
  route's end move to the **highest** `Current` node, the newly opened one; the failed job keeps its
  pickable tint without the pulse.

## Units

### U1 — the opened store and the guard

- One `JobOpenedCookie` per opened index, id `job_opened_<levelId>` (verified: it collides with neither
  `CompletedJobsCount` nor `IsLevelCompleteCookie`). `CurrentIndex` stays derived from the solved set.
  `RunJobs.StateAt` treats an unsolved job as `Current` when it is `CurrentIndex` **or** at or below the
  highest opened index, so a run with no fail is unchanged.
- **The guard:** open only when the failed job is in the selector and has a next one —
  `LevelSelector.GetGlobalIndex` returns `-1` for an unknown level (the debug `puzzleToSet` override is
  one), so `index >= 0 && index + 1 < TotalLevelsCount`, or nothing opens.
- A `RunProgress` member answers the signpost: **the next job is opened and unsolved** (false for the
  last job).
- No `IsFresh` change: job 1 is `noFail`, so any fail comes after a solve.
- `PuzzleCompleteState` gets the run order from the `LevelSelector`, **wired in this unit** (the scene
  reference lands here, since this unit's exit needs it). On a fail it writes the open in `Enter` beside
  the solve's `pendingSave`; `Restart` awaits it with the same `saveTimeoutSecs` cap (a local write,
  with a fire-and-forget cloud mirror).
- **The restart carries the payload.** `PuzzleRestart` (written by `PuzzleCompleteState.Restart`) gains
  the signpost's next index (`-1` for none: not opened, already solved, or the last job) and
  `firstOpen` (this fail opened the job). `PuzzleCompleteState` fills both from `RunProgress`; no
  other state needs the selector.
- **One public map member:** `PuzzleMap.HighestCurrentIndex`, tested here and used by U4's views (the
  renderer and route line are `internal`; no visibility is widened for tests). With no `Current` job
  (an all-solved run, the map the ending leaves) it returns the last solved index, the fallback
  `MapRouteLine.EndIndex` uses today, so the route still shows the whole chain.
- `PuzzleRestart` (an `internal readonly struct`, one-arg constructor today) gets the two fields through
  a new constructor; the write in `PuzzleCompleteState.Restart` is updated to it.
- Tests (`Persistence.Tests.EditMode`, `PuzzleSelection.Tests.EditMode`, both fast): an opened unsolved
  job is `Current`; a locked one past it stays `Locked`; a clean run keeps only `CurrentIndex`
  `Current`; opening never marks a job solved or complete; an unknown level or the last job opens
  nothing; the signpost member is true only while the next job is opened and unsolved;
  `HighestCurrentIndex` is the opened job when jobs 4 and 5 are `Current`, `CurrentIndex` on a clean
  run, and the last solved index when every job is solved.
- **Exit:** the fast suites pass; a fail on job 4 writes `job_opened_01_05`.

### U2 — the signpost line

- `FieldResetReadout.Play(int signpostIndex)` (`-1` for none) instead of a fourth `lineKey`: the
  readout formats the line itself, since it already owns the keys and `LocalizationUtil`. `BuildText`
  appends it only when the index is set; the prefab's serialized `lineKeys` stay the three reset lines.
  The signpost key is a `const` in `FieldResetReadout` (`field_reset_job_open`), not a serialized
  field, so `Puzzle.prefab` needs no edit.
- The index reaches the view through the existing seam: `PuzzleStartState` (restart branch) **reads**
  `PuzzleRestart` (it no longer removes it; the edition state does, U3) and calls
  `SpaceShapingController.PlayEntropyReset(index)`.
- **The controller is at 238 of 250 lines:** it gains the index argument and forwards exactly one
  `Ended` event (U3), nothing else. If either pushes it past the cap, split a small readout collaborator
  off it first.
- Localization: one key in `Global Shared Data` + `Global_en` / `Global_es` (`> JOB {0} OPEN ON THE
  MAP`), formatted with `LocalizationUtil.GetValue(key, args)` and the two-digit job number.
- **Exit:** fail job 4: the readout's last line reads "JOB 05 OPEN ON THE MAP"; after solving job 5 it
  is gone.

### U3 — the Exit prompt after the readout

- **On a fail restart, the entry prompts do not show the Exit prompt** (`PuzzleEditionState.ShowPrompts`
  runs on every entry, including the restart). The edition state reads and removes `PuzzleRestart` in
  `Enter` (moved from `PuzzleStartState`; `PuzzleSelectionState.EnterJob` still clears a stale one), so
  `ShowPrompts` skips Exit for that entry.
- `FieldResetReadout` raises one `Ended` event from both `Finish` and `Cancel`; the controller forwards
  it. On it, the edition state shows the Exit prompt **once per save** (an `exit_prompt_after_fail`
  cookie beside the opened ones), only when the payload's `firstOpen` is set.
- **It bypasses `GestureLearning.IsLearned`** for that one showing: a player who once used the menu
  gesture may still not connect it with the open job, and otherwise the cookie is moot.
- No scene or prefab wiring in this unit: the controller already owns the readout, so forwarding
  `Ended` adds no reference. The only change is the subscription in `PuzzleEditionState`, `+=` in
  `Enter` and `-=` in `Exit`.
- **Exit:** first fail on job 4: the readout types, then the Exit prompt shows; second fail: no Exit
  prompt at all.

### U4 — the map shows the opened job

- `MapFieldRenderer` pulses the slot at `PuzzleMap.HighestCurrentIndex`, and `MapRouteLine` ends its
  route there (its `DrawSegment` already draws the segment into that node), so the opened job has its
  segment and its pulse; other `Current` nodes keep the plain current tint.
- Tests: the domain member is covered in U1; the views only read it, so this unit is checked on the
  headset.
- **Exit:** after a fail on job 4, the map pulses job 5 and the route reaches it; job 4 is lit and
  pickable.

## Review — 2026-11-11

U1–U4 are in (uncommitted): the guard, the opened store, the restart payload, the signpost on the
readout, the once-per-save Exit prompt after it, and the highest-`Current` pulse and route. The scene
wires the selector on `PuzzleCompleteState`; the fast suites pass (Persistence 20/20, PuzzleSelection
9/9, SpaceShaping 121/121); `OpenNext` and `HighestCurrentIndex` are tested, including the all-solved
fallback. Open:

- **K1 — accepted 2026-11-11.** A fail on an opened job opens the one after it (failing job 5,
  opened by a fail on job 4, opens job 6), so a stuck player can reach every job by failing. Kept on
  purpose: a judge sees the whole run, and the ending still waits for every job to be solved. No code
  change; the contract says so.
- **K2 — fixed 2026-11-11.** `Restart` and `ReturnWhenSaved` now check an `active` flag after the save
  await, set in `Enter` and cleared in `Exit`, as `RunEndingState` does.
- **K3 — no issue.** `ShowPrompts` gates only the Exit prompt on `restart.Value`
  (`PuzzleEditionState.cs:92`); Scale, Move and Stars still show on a restart, as the plan says.
- **K4 — closed.** `DrawSegment` disables every segment when `end < 0`, and the old `SelectedIndex`
  fallback also drew none for that case; an empty map returns before the route. No code change.
- **K5 — applied.** `JobOpenResult.Changed` is now `firstOpen` (the prompt implies it); the
  `Game.unity` `puzzleToSet` override and the other agents' hunks are still uncommitted, so the unit
  commit stays open until the scene can be staged alone.

## Manual check (headset)

1. Fail job 4 once: it restarts in place; the readout ends with "JOB 05 OPEN ON THE MAP"; then the
   Exit prompt shows (even if the menu gesture was learned).
2. Fail job 4 again: the readout still says job 5 is open; no Exit prompt.
3. On the map: job 5 pulses and the route reaches it; job 4 is lit and pickable; relaunch keeps it.
4. Solve job 5, then fail job 4: no signpost.
5. Fail the last job: no signpost, nothing opens.
6. With the debug `puzzleToSet` override set, a fail opens nothing.
7. Solve every job on a run with no skips: the ending plays once.
8. Open a job by a fail, solve every other job but leave the failed one unsolved: no ending until it is
   solved.
