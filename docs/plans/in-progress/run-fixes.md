# Run fixes — exit on the menu gesture, restart on fail, first pass on Jim's lines

**Status:** now · flow · updated 2026-11-11 — U1 and U4 landed with review fixes F1–F4 open (see
§Review); U2 (restart on fail) and U3 (Jim's lines) are not started. The exit is the non-dominant hand's
menu pinch, a fail restarts the job in place, and the fail and job 6–7 lines get a first pass.

**Related:** [core loop](../../design/core-loop.md) §2 · [progression](../../design/systems/progression.md) ·
[jobs](../../design/jobs.md) §The run — the lore skin · [plans board](../README.md)

## Goal

Three fixes before the competition capture: no accidental exit while shrinking, a retry that does not
go through the map, and fail lines that tell the player something.

## Contract

- **Exit is the menu gesture** — `MetaAimFlags.MenuPressed` on the **non-dominant** hand (palm up,
  pinch), not the dominant hand's universal menu. It works only in a job and returns to the map; the
  controller menu button stays wired as the fallback. The two-hand scale-out exit is removed: it fires on
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
  and `SystemGesturePokeInteractorGate` already use that Samples copy). Add `aimFlags` bindings for
  **both** hands — two `MetaAimFlagsHomeMenuInvoker` instances, one per hand, is the safe shape (one
  action with both bindings shares the invoker's single `aimFlags` field and can miss an edge); only the
  menu hand raises `MenuPressed`, so the non-dominant hand works for either dominance. Add
  `DigitalLove.HomeMenu` to the Flow asmdef and wire the invokers in `Game.unity`. `PuzzleEditionState`
  subscribes in `Enter` and unsubscribes in `Exit` (guard against a double fire if both hands report).
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
  known gap for the competition build. The detector still follows the menu hand, so the exit works.
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

Drafts for a yes (the strings land in `Levels_en`; `Levels_es` currently has translations, so replace it
with the placeholder or keep the translations). New clips are downloaded after the strings are approved.
Fail playback matches every key **containing** `FailKey`, so today a fail plays `fail` + `fail_02` +
`fail_03`; replacing the base string is not enough. Delete the extra entries in `Levels Shared Data`,
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
| 7 | manual | Shorten to "This is the one the manual was written for. There is still no office."; the "eleven years" note moves or stays, decided in the lore rethink. |

- Update [jobs](../../design/jobs.md) §The run — the lore skin: add a fail-line column (or a fail-line
  table) and the job 6/7 manual and tech-voice edits.
- **Exit:** every fail plays exactly one new line; job 6's card says what job 6 teaches.

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

U1 and U4 landed in `4c40552` (game) and `_Modules` `722b6c1`; **U2 and U3 are not started**: the fail
path still returns to the map (`PuzzleCompleteState` unchanged), `Levels_en` still has the six
"We didn't arrive." lines plus 12 `fail_0x` keys, and the glossary and progression still say
"Map/Retry". What works: the scale-out and rotation gestures and their tests are gone, the exit is
gated on pause and fires once (`exiting`), the end sequence and the map are not subscribed, the six
prompts reference `GestureLoop` assets with Unity-assigned GUIDs, and the module has no `Assets/_Game`
path.

Fixes, in order:

- **F1 — the exit listens to both hands (contract break).** `Game.unity` wires two
  `MetaAimFlagsHomeMenuInvoker`s, `Menu Gesture Left` and `Menu Gesture Right`, into
  `PuzzleEditionState.menuInvokers`. The right hand's palm pinch is the Quest universal menu for a
  right-handed player, so opening the system menu can also drop the player out of the job. Keep the
  left invoker only (left-only is the decided v1 scope) and delete the right one. Both also bind the
  same `MenuButton` action and each enables/disables it; with one invoker the double bind goes too.
- **F2 — the archive's hands never close (U4 exit not met).** All 12 `menu` frames are identical, and
  so are all 12 `pinch` frames; `grab-move` frames 0 and 1 (grip 0 vs 0.5, same position) are
  pixel-identical, so the grip does nothing anywhere in `GestureArchive`. The game's loops do animate
  (`move` frames 0/1 differ), so the fault is in the archive path: `GestureArchive.WriteFrames` →
  `SetRotations` → `RenderHands`. Find why `HandPoser.Pose`'s grip has no effect there, re-render the
  archive, and confirm the Exit prompt shows the hand closing. Add an editor-side check to the render
  command that warns when a loop's open and pinched frames come out identical.
- **F3 — the old exit art is dead.** `GesturePrompts/Art/exit/` (12 PNGs) is no longer referenced: the
  Exit prompt uses the module's `Menu` loop and `GestureMotion` has no exit motion. Delete the folder.
- **F4 — module menu names carry the game's name.** `GestureArchive` registers
  `Tools/Our Space/Render Gesture Art …` inside `_Modules`; rename to `Tools/Gesture Art/…` so another
  project does not get "Our Space" menus. The game's own command keeps its name.
- **F5 — then build U2 and U3 as written above.** Nothing in them changed; the U2 notes on
  `puzzleController.Exit()` and the restart flag still apply.

## Not in this plan

- **The ending** (Road to v1 step 3, still proposed): the lit map, total stars, the title.
- **Stars on every map node** and **a pinch to skip the end sequence** — review suggestions, no yes yet.

## Manual check (headset)

1. Job 3: shrink the blocker hard, hands well apart to close together: the job stays open.
2. In any job, the menu hand's pinch returns to the map with the exit sound (test both hand preferences);
   the controller Menu button does the same. The Exit art is left-only, so a left-dominant player sees
   the wrong hand.
3. On the map, while paused and during the end sequence, the menu gesture does nothing.
4. The Exit prompt shows the menu motion and reads at arm's length.
5. Job 2: spend the whole budget: glitch, the fail line, the job restarts with a full bar and no intro.
6. Solve after a restart: stars and Jim's receipt as usual, then the map.
7. Each job's fail plays exactly one new line; job 6's card reads the new manual line.
8. Pause (headset off) during the restart: it resumes cleanly.
9. If U4 lands: Scale, Enter, Move, Spend and Stars prompts look unchanged after the move to
   `GestureLoop`.
