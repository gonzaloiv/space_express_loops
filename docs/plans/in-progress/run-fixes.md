# Run fixes — exit on the menu gesture, restart on fail, first pass on Jim's lines

**Status:** now · flow · updated 2026-11-11 — U1 and U4 landed; review fixes F1–F4 are fixed; U2
(restart on fail) and U3 (Jim's lines) are implemented. The new fail and job-6 lines need their TTS
clips downloaded; job 7's manual shortening waits on the lore rethink.

**Related:** [core loop](../../design/core-loop.md) §2 · [progression](../../design/systems/progression.md) ·
[jobs](../../design/jobs.md) §The run — the lore skin · [plans board](../README.md)

## Goal

Three fixes before the competition capture: no accidental exit while shrinking, a retry that does not
go through the map, and fail lines that tell the player something.

## Contract

- **Exit is the left-hand menu gesture** — `MetaAimFlags.MenuPressed` on the **left** hand (palm up,
  pinch), the v1 scope decided 2026-11-11. It works only in a job and returns to the map; the controller
  menu button stays wired as the fallback. Left-dominant players see the wrong hand and cannot use the
  gesture, a known gap. The two-hand scale-out exit is removed: it fires on
  any two-hand pinch that closes 8 cm, which is exactly job 3's shrink.
- **A fail restarts the job** (decided 2026-11-11). Glitch, Jim's fail line, then the same job reopens:
  a fresh field, the budget restored, the start check. Jim's intro does not replay on a restart. A solve
  still returns to the map.
- **One fail line per job.** Short, one sentence per TTS entry, few commas, no dashes. A fail line may
  point at the read (what to look at), never at the answer. The lines here are a first pass; the wider
  lore rethink comes after.

## Units

### U1 — exit on the menu gesture

- Reuse `MetaAimFlagsHomeMenuInvoker` in `_Modules/Player/HomeMenu` and its `homeMenuRequested`; do not
  write a new detector, and do not add the Samples `MetaSystemGestureDetector` dependency (`HomeMenuInvoker`
  and `SystemGesturePokeInteractorGate` already use that Samples copy). Add one `MenuAimLeft` action bound
  to `<MetaAimHand>{LeftHand}/aimFlags`, keep the controller Menu fallback, add `DigitalLove.HomeMenu` to
  the Flow asmdef and wire the single left invoker in `Game.unity`. `PuzzleEditionState` subscribes in
  `Enter` and unsubscribes in `Exit` (guard against a double fire).
- Remove `ScaleOutGesture`, its tests, the `scaleOut*` fields on `TwoHandGestureInput` and the now-unused
  `gesture` field on `PuzzleEditionState` (the component stays for `TwoHandGestureIndicator`).
- Remove the disabled rotation gesture (`OneHandRotationGesture`, `rotationEnabled` and its fields on
  `PuzzleEditionState`, `SpaceShapingController.Rotate` if nothing else calls it). Chapter 3 rebuilds
  rotation in the cube.
- Re-render the Exit prompt loop with the menu motion now, on the game rig; U4 later lifts the same loop
  into the archive. Do not make U1 depend on U4 (`GestureMotion` only varies position and grip today, so
  the palm-up pose also needs its own hand orientation). Replace the `exit` motion's sampler or add a
  `menu` motion + content (`GestureMotion.All` has no `menu` entry yet). The loop is **left hand only**
  (decided 2026-11-11): left-handed players, whose menu hand is the right, see a mismatched prompt — a
  known gap for the competition build. The detector is left-only too, so a left-dominant player uses the
  controller menu button instead.
- Gate the handler on `Time.timeScale <= 0f` (the `TwoHandGestureInput` / `PuzzleMapView` pattern): the
  end sequence unsubscribes, but pause does not.
- Check: the menu gesture while paused or during the end sequence does nothing.
- **Exit:** a hard shrink in job 3 never leaves the job; the menu gesture always does.

### U2 — restart on fail

- `PuzzleCompleteState` fail path: after Jim's line, call `puzzleController.Exit()` (it clears `frozen`
  and the duplicate `ShipsCompleted` / `ResetRequested` subscriptions; `Enter` alone leaves the restart
  unplayable), then route to `PuzzleStartState` with the same `PuzzleData`, not to the map. Add a
  serialized `retryState` reference. A fail already saves nothing (`pendingSave` is completed).
- `PuzzleStartState` skips Jim's intro and the second `Glitch()` on a restart. Use one explicit seam:
  `PuzzleCompleteState` sets a separate restart value in memory just before the transition;
  `PuzzleStartState` reads and clears it; `PuzzleSelectionState.EnterJob` also clears it, so a stale
  value cannot skip the intro on a fresh map entry.
- Docs: [glossary](../../design/glossary.md) (the map row) and
  [progression](../../design/systems/progression.md) §The map: replace "the result offers Map/Retry" with
  "a fail restarts the job; a solve returns to the map". [core loop](../../design/core-loop.md) §2 step 3
  gets the same sentence.
- **Exit:** spend the budget in job 2: glitch, fail line, the field resets in place with a full budget.

### U3 — first pass on the lines

Drafts approved and applied 2026-11-11 (the strings land in `Levels_en`; `Levels_es` uses the English
placeholder for the changed entries). New clips are downloaded after the strings are approved.
Fail playback matches every key **containing** `FailKey`, so today a fail played `fail` + `fail_02` +
`fail_03`; the extra keys are removed. The `_fail_02` / `_fail_03` entries are deleted from `Levels Shared Data`,
their `Levels_en` / `Levels_es` values and the `StreamingAssets/*_fail_02` / `_fail_03.mp3` clips.
The fail lines are story only (decided 2026-11-11): the cargo's fate, plus job 3's blocker, which points
at the read without the answer.

| Job | Key | Line |
|---|---|---|
| 2 | fail | "The bulb is still out there." |
| 3 | fail | "Too much rock in the way." |
| 4 | fail | "The piano is lost." |
| 5 | fail | "The cake went cold." |
| 6 | fail | "The field won." |
| 7 | fail | "Mr. Fink is still waiting." |
| 6 | manual | "The field is free. The Wrap is not." |
| 6 | tech voice | "Use what is already there." |
| 7 | manual | Deferred: shortening waits on the lore rethink, which decides whether the "eleven years" note moves or stays. |

- Update [jobs](../../design/jobs.md) §The run — the lore skin: add a fail-line column (or a fail-line
  table) and the job 6/7 manual and tech-voice edits. Done 2026-11-11: the fail column and job 6 lines
  are in; job 7's manual is deferred.
- **Exit:** every fail has exactly one entry in the string table and the extra `_fail_0x` clips are gone;
  the new clips are downloaded next.

### U4 — a gesture art module for other projects

The render tool (`GesturePrompts/Editor`) poses Meta's own hand mesh and draws line-art loops; only
the motions and the cube, meter and star overlays are this game's. Lift the generic part into `_Modules`
with a minimal archive of common Quest hand gestures. If U1–U3 fill the morning, U4 moves to its own plan.

- **Module:** `_Modules/XR/GestureArt/` (the XR module already holds the hand-tracking helpers). Split it
  into a runtime asmdef (`GestureLoop`) and an `Editor/` asmdef (the rig, `HandPoser`, `LineArt`, the
  capture writer), as the project rules require. `GestureLoop` is a `ScriptableObject` (frames + frame
  time) that any project's prompt can reference.
- **The archive** (12-frame, 512 px loops, rendered once and committed): `pinch` (one hand),
  `spread` (two-hand pinch, apart), `squeeze` (two-hand pinch, together), `grab-move` (one-hand pinch
  and slide), `menu` (left palm up, pinch — the menu gesture; a right-hand copy for left-handed
  players is a follow-up). The module's render command
  re-renders them into the module's own folder.
- **The game keeps its own motions.** Scale, Enter and Move (lifted onto the cube), Spend and Stars
  (with `MeterRig`) stay in `GesturePrompts/Editor`; the rig also builds the cube and the star, so the
  optional overlay hook must cover cube, meter and star, not only the meter. The hand rotation is fixed
  per hand today; the palm-up `menu` loop needs a per-motion orientation. The game's render command
  calls the module's writer with its motions and output folder.
- `GesturePromptContent` references a `GestureLoop` instead of holding its own frames; Exit points at
  the module's `menu` loop. Re-wire the six prompt assets; the existing PNGs stay as the game's loops.
- No hard-coded `Assets/_Game` path in the module: output folders come from the caller.
- **Exit:** a fresh project can render or reuse the five archive loops with only the XR module; this
  game's six prompts look the same as before, and Exit shows the menu motion.

## Review — 2026-11-11

U1 and U4 landed in `4c40552` (game) and `_Modules` `97129df`; **U2 and U3 are implemented after
that** (working tree). What works: the scale-out and rotation gestures and their tests are gone, the
exit is gated on pause and fires once (`exiting`), the end sequence and the map are not subscribed, the
six prompts reference `GestureLoop` assets with Unity-assigned GUIDs, the module has no
`Assets/_Game` path, a fail calls `Exit()` and re-enters the same job with a one-shot restart flag, and
the old `fail_02` / `fail_03` keys and clips are gone.

Fixes, in order:

- **F1 — fixed 2026-11-11.** The exit listens to the left hand only: one `menuInvoker` field, one
  `MenuAimLeft` action, one `Menu Gesture Left` object. The right invoker, its action and its
  `MenuAimRight` reference are deleted, so the universal-menu gesture cannot drop a right-handed
  player out of the job and the duplicate `MenuButton` bind is gone. Left-dominant players use the
  controller fallback.
- **F2 — fixed 2026-11-11.** The preview-scene `SkinnedMeshRenderer` kept its first skin pose, so the
  bone grip never reached the render. `HandPoser` now sets `updateWhenOffscreen` and
  `forceMatrixRecalculationPerRender`; the archive's `pinch` and `menu` loops close, and
  `GestureArchive` warns if a loop's open and pinched frames are still identical.
- **F3 — fixed 2026-11-11.** `GesturePrompts/Art/exit/` is deleted; Exit uses the module's `Menu` loop.
- **F4 — fixed 2026-11-11.** The module menus are `Tools/Gesture Art/Render Archive` and
  `Tools/Gesture Art/Render Menu`; the game's `Tools/Our Space/Render Gesture Prompt Art` keeps its name.
- **F6 — the game points at an orphan module commit.** The game's `HEAD` records `_Modules` at
  `97129df`, which is on no branch, local or remote; the module's `our-space` branch has `722b6c1` (the
  same change, rewritten) and is one commit ahead of `origin`. A fresh clone of the game cannot check
  out its modules. Commit the module fixes (F1, F2, F4) on `our-space`, push the module, then commit the
  game with `_Modules` at that commit and push the game. Correct the `97129df` reference above.
- **F7 — a debug override sits in the scene.** `Game.unity`'s `PuzzleSelectionState.puzzleToSet` is set
  to `TwoShips_Converge_Puzzle`, which skips the map and opens that level every time. Fine for a test,
  but clear it before committing the scene; the same diff also carries a few `RectTransform` layout
  changes worth checking before they ride along.
- **F5 — done 2026-11-11.** U2: `PuzzleCompleteState` calls `puzzleController.Exit()` and re-enters
  `PuzzleStartState` via the scene-wired `retryState`; a one-shot `PuzzleRestart` value skips Jim's
  intro and the second glitch, and `PuzzleSelectionState.EnterJob` clears stale flags. U3: the six fail
  lines and job 6's manual/tech line are in `Levels_en`; `Levels_es` holds the English placeholder;
  the 12 `fail_0x` keys, their locale values and the old clips are deleted; `jobs.md` has a fail column.
  The new clips are still to download, and job 7's manual shortening is deferred to the lore rethink.

### Second review — 2026-11-11

U2 and U3 are right: the restart exits the controller before re-entering, the flag defaults to false
when absent and is cleared on both entries, and a missing clip ends the line silently, so the restart
still fires. Still open before the commit:

- **F6 and F7 are unchanged.** The game still records `_Modules` at the orphan `97129df`, and
  `puzzleToSet` still points at `TwoShips_Converge_Puzzle` in `Game.unity`. Both must be fixed in the
  commit that carries U2/U3, or the build opens a test level and the clone breaks.
- **F8 — seven lines are silent until their clips land.** No clip exists for
  `level_01_02`…`level_01_07_fail` or `level_01_06_init_01` (the old ones are deleted). The game runs,
  but a fail plays the glitch with no voice, and job 6's intro is mute. Download the seven clips before
  the capture; until then, manual-check item 7 cannot pass.
- **F9 — `jobs.md` shows job 7's manual short; the string is still the long one.** The lore-skin table
  lists "This is the one the manual was written for." while `Levels_en` still carries the four-sentence
  entry. Since the shortening is deferred, show the current string in the table (or mark it "deferred")
  so the doc matches the build.

## Not in this plan

- **The ending** (Road to v1 step 3, still proposed): the lit map, total stars, the title.
- **Stars on every map node** and **a pinch to skip the end sequence** — review suggestions, no yes yet.

## Manual check (headset)

1. Job 3: shrink the blocker hard, hands well apart to close together: the job stays open.
2. In any job, the left-hand menu pinch returns to the map with the exit sound; the controller Menu
   button does the same. A left-dominant player uses the controller (the art and detector are left-only).
3. On the map, while paused and during the end sequence, the menu gesture does nothing.
4. The Exit prompt shows the palm-up menu motion, the hand closes, and it reads at arm's length.
5. Job 2: spend the whole budget: glitch, the fail line (once its new clip is downloaded), the job
   restarts with a full bar and no intro.
6. Solve after a restart: stars and Jim's receipt as usual, then the map.
7. After the new clips download: each job's fail plays exactly one line; job 6's card reads the new
   manual line.
8. Pause (headset off) during the restart: it resumes cleanly.
9. Scale, Enter, Move, Spend and Stars prompts look unchanged after the move to `GestureLoop`.
