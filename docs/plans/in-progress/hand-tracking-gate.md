# Hand-tracking gate

**Status:** now · flow · updated 2026-11-12 — U1, U2 and U2b are in: the hand hint is typed by the boot
console (`ConsoleHint`), the card panel is gone. The headset proof (U3) is open.

**Related:** [pitch](../../design/pitch.md) · [plans board](../README.md)

## Goal

No judge reaches job 1 with hand tracking off or fewer than two hands. The seat step already waits
for the headset; add a hard stop for hand tracking and a steady-window wait for both hands, with one
instruction, before the seat step places the field.

## Contract

- **When:** in the editor, skip. On device, wait for the headset to be worn
  (`OVRManager.instance.isUserPresent`, matching `SeatPlacement`); the step must not complete while
  the headset is off.
- **Check, hard:** `OVRPlugin.GetHandTrackingEnabled()`. Hand tracking off holds the boot until it is
  turned on.
- **Check, soft:** both `Hand.IsConnected` / `IsTrackedDataValid` (the core is a two-hand pinch).
  Continue once both hands have tracked together for a short steady window (~0.3 s, so a one-frame
  flicker does not toggle the hint). One hand missing or untracked, or flaky tracking in dim light,
  keeps the hint up; the gate never passes the step. The first job's pinch needs both hands, so a
  one-hand judge must not reach it.
- **Placement:** the step runs in `CheckDevice`, after the AR-support branch and before
  `RunStep(SetupStep.Space, ...)`, so both hands are already tracked when `SeatPlacement` runs. The
  gate proves tracking, not rest; `SeatPlacement`'s own rest test still picks the desk height or falls
  back to the default drop.
- **Hint, on the console (2026-11-12, replaces the card panel):** the instruction is typed by the
  boot console itself, in its own style and place, so nothing overlaps the boot lines. While hands
  are missing, the console types four hint lines in turn and loops them:
  `> HAND TRACKING NEEDED`, `> PUT THE CONTROLLERS DOWN`,
  `> SETTINGS > MOVEMENT TRACKING > HAND TRACKING`, `> HOLD BOTH HANDS IN FRONT OF YOU` (a judge
  holding controllers has hand tracking on but no tracked hands). When both hands are steady the
  console returns to the `> HANDS ........ OK` row and the boot carries on. No button: the setting
  lives in the system menu. The old `HandTrackingRequiredPanel` card goes.
- **Module boundary:** `SetupState` and `SetupStep` live in `_Modules/Casual` and stay game-agnostic.
  The module gains one generic optional step that runs before the space setup: a small `SetupCheck`
  base (`Setup(Action)`) with `XRSpaceSetup : SetupCheck` so the slot is not typed as a space setup.
  It is shown as `SetupStep.Input`, appended at the end of the enum as value 3 so the existing
  serialized rows do not renumber. The game supplies the hand gate for that slot.
- **Boot:** the game's checklist labels that step `HANDS`, between `LINK` and `SEAT`.
- **Scene config:** `requestScenePermissionOnStartup = 0` (no MRUK/scene anchors) and
  `controllerDrivenHandPosesType = None` (hands are camera-only). No multimodal for the freeze
  (`launchSimultaneousHandsControllersOnStartup` stays off, no manifest change): the system switches
  back to hands once the controllers are put down, and the hint says so.
- **No runtime objects:** the hint lines and the row are authored console lines in `Game.unity`.

## Units

### U1 — the check and the step

- Add `HandTrackingGate` in `Placement.Domain`: a small stateful rule fed each frame with
  (enabled, left tracked, right tracked, `deltaSecs`) that reports *waiting* or *ready* — ready once
  both hands stay tracked for the steady window; never ready while hand tracking is off, and the
  window restarts when either hand is missing. The view feeds `Time.unscaledDeltaTime`, so a paused
  boot still counts. Its EditMode tests in `Placement.Tests.EditMode` cover the steady window, a
  flicker that resets it, one hand never passing, "off never passes", and off-again mid-window.
- Add `HandTrackingSetup : SetupCheck` in `Placement.Views` (hands + panel references). Keep `done` in
  a field and restart only the wait on `HMDMounted`/re-wear (the `SeatPlacement.pendingPlaced` trap);
  tick the gate, hide the panel, then call `done` once. It calls `done` immediately in the editor and
  waits for `isUserPresent` on device.
- In `_Modules/Casual`: append `SetupStep.Input = 3` (the existing `Services`/`Space`/`PlayerData`
  values stay) and give `SetupState` one optional serialized `SetupCheck` input step that runs in
  `CheckDevice`, after the AR-support branch and before `RunStep(SetupStep.Space, ...)`; empty skips
  it. No game names in the module.
- Exit: the editor boot still completes with the gate wired; a device with hand tracking off stays on
  `> HANDS`; turning it on with both hands tracked continues; one untracked hand does not continue; a
  device that is not worn does not complete the step; taking the headset off and on again does not
  drop the callback or hang the row.

### U2 — the panel, the row and the config

Landed with a card panel; U2b replaces the card with the console hint.

- Add the `HANDS` row to `BootChecklist` in `Game.unity` between `LINK` and `SEAT`, and a
  non-interactive `HandTrackingRequiredPanel` (a `CardPanel` + `Canvas`, no `BtnGroupPanel`;
  `SubtitlesLikeFollow` with the boot console's 0.5 m distance, 0.1 m drop and `levelWithHorizon` on);
  add `hand_tracking_required_title` and `hand_tracking_required_body` to the `Global` table.
- Set the two `OVRManager` fields above; wire the checker, the panel and the row in `Game.unity`.
  The scene's `spaceSetup` is already `SeatPlacement`, not `MRUKSpaceSetup`, so scene permission off
  does not leave the MRUK permission loop running.
- Exit: controllers on the desk and hand tracking off shows the panel; turning hand tracking on with
  both hands in view hides it and the boot continues; no scene-permission dialog.

### U2b — the hint on the console, and fixes from the review (2026-11-12)

- **The hint moves to the console.** Remove `HandTrackingRequiredPanel` (script, card, canvas and its
  `SubtitlesLikeFollow`) from `Game.unity`. `HandTrackingSetup` reports when the hint is due and when
  it clears (an event or a state the boot reads); `ConsoleBootSplashScreen` types the authored hint
  lines with `screen.Type` while it is due, looping them with the usual line hold, and goes back to the
  `HANDS` row when it clears. The hint lines are console `TMP_Text` rows styled like the others; the
  `hand_tracking_required_*` keys become one key per line. Check the assembly direction before wiring
  (UI reads Placement, or the setup exposes a module-level event the splash can take).
- **No flash.** The hint is due only while hands are *missing* (tracking off or a hand untracked),
  never during the steady window, and only once they have been missing for a short delay (~0.5 s), so
  boot's first frames and a one-frame flicker never type it. `HandTrackingGate.Tick` returns a state
  (`Missing` / `Settling` / `Ready`) plus whether the hint is due, instead of a bool. Tests: both
  hands from the start never make the hint due; a flicker shorter than the delay does not; missing
  past the delay does.
- **Off-head.** While the headset is off the gate resets and the hint is not due; the console keeps
  re-placing each line until the seat step (`PlaceEachLine`), so the hint appears in view once worn.
- **Module stays invisible when unused.** `SetupState` runs the `Input` step (begin/end reported to
  the splash) only when `inputCheck` is assigned; empty goes straight to the space step, so other
  projects' splashes never see the new step. Commit the submodule first, then the parent pointer.
- **One `IsWorn`.** `SeatPlacement` and `HandTrackingSetup` share one headset-worn check (one owner in
  `Placement`), not two copies.
- Exit: the fast EditMode suite passes, including the new gate tests; the editor boot still
  completes with the gate wired; no card or canvas is left for the hint in `Game.unity`.

### U3 — the headset proof

- Run the manual list below on the Quest 3S with hand tracking on and off; record the first-frame
  timing and the `HANDS` row read at arm's length.
- Update the [plans board](../README.md); report the headset result in chat (reviews live in chat).
- Exit: the list passes; the board closes this plan.

## Not in this plan

- A controller fallback; the entry stays hands-only.
- Simultaneous hands and controllers (multimodal); after the freeze if controllers on the desk get in
  the way.
- Mid-job hand-loss recovery; the gate is at boot only.
- The 2ft/seat tuning (UX finding 5) and the music duck.
- A hand-tracking tutorial beyond the console hint.

## Manual check (headset)

1. Controllers on the desk, hand tracking off: the console types the hand hint lines and the boot
   holds; the field is not placed.
2. Turn hand tracking on in Settings > Movement Tracking with both hands in front of you: the hint
   stops and the console types `> HANDS ........ OK`.
3. Rest both hands at desk height: no hint, then the rows read `LINK → HANDS → SEAT → LEDGER`;
   the field places at the resting-hand height.
4. Hold the controllers with hand tracking on: the hint tells you to put them down; put them on
   the desk: the hands take over, the hint stops and the boot continues; no
   controller-driven hand pose appears.
5. No scene-permission dialog at launch.
6. Cover both hands before the gate completes: the hint returns; uncover: it stops and the boot
   resumes.
7. Keep one hand out of view (only one tracked): the hint keeps looping and the boot does not continue;
   bring both hands in: it continues.
8. Hands flickering in dim light: the hint does not start and stop; the steady window holds.
9. Look up, down and to the side while the hint types: each new line lands at eye level ahead, never
   overhead.
10. Put the headset down and back on during the gate: the boot resumes without a stuck row.
11. Take the headset off before both hands track: the gate does not complete; put it on again and
    track both hands: it continues.
12. The `HANDS` row is legible at arm's length and in the right visual order; the hint's settings
    path matches the headset's menu.
13. Launch the built APK: no black frame, no stuck `> HANDS` with hands tracked.
14. Launch with both hands already in view: no hint line is typed, not even the first characters.
15. The hint lines read in the console's style (font, colour, typing sound); no card or second panel
    appears.
