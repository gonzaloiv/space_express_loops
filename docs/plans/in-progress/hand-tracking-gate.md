# Hand-tracking gate

**Status:** now · flow · updated 2026-11-12 — the boot holds while hand tracking is off and waits
for both hands; the panel keeps the instruction up so there is no silent dead end. The step is a
generic slot in the setup module. The scene config and headset proof are open.

**Related:** [Review](../../reviews/review.md) finding 10 · [pitch](../../design/pitch.md) ·
[plans board](../README.md)

## Goal

No judge reaches job 1 with hand tracking off or fewer than two hands. The boot already waits for the
headset; add a hard stop for hand tracking and a steady-window wait for both hands, with one
instruction, before the field is placed.

## Contract

- **When:** in the editor, skip. On device, wait for the headset to be worn
  (`OVRManager.instance.isUserPresent`, matching `SeatPlacement`); the step must not complete while
  the headset is off.
- **Check, hard:** `OVRPlugin.GetHandTrackingEnabled()`. Hand tracking off holds the boot until it is
  turned on.
- **Check, soft:** both `Hand.IsConnected` / `IsTrackedDataValid` (the core is a two-hand pinch).
  Continue once both hands have tracked together for a short steady window (~0.3 s, so a one-frame
  flicker does not toggle the panel). One hand raised, or flaky tracking in dim light, keeps the panel
  up; the gate never passes the step. The first job's pinch needs both hands, so a one-hand judge
  must not reach it.
- **Placement:** the step runs in `CheckDevice`, after the AR-support branch and before
  `RunStep(SetupStep.Space, ...)`, so `SeatPlacement` sees resting hands and places the field at the
  desk height.
- **Panel:** a title-only, non-interactive panel (no button; the setting lives in the system menu):
  *"HAND TRACKING NEEDED — Turn on hand tracking in Settings > Movement Tracking, then hold both
  hands in front of you."* It hides automatically when both hands are steady.
- **Module boundary:** `SetupState` and `SetupStep` live in `_Modules/Casual` and stay game-agnostic.
  The module gains one generic optional step that runs before the space setup (an `XRSpaceSetup`
  slot, shown as `SetupStep.Input`, appended at the end of the enum as value 3 so the existing
  serialized rows do not renumber). The game supplies the hand gate for that slot.
- **Boot:** the game's checklist labels that step `HANDS`, between `LINK` and `SEAT`.
- **Scene config:** `requestScenePermissionOnStartup = 0` (no MRUK/scene anchors),
  `controllerDrivenHandPosesType = None` (hands are camera-only), and
  `launchSimultaneousHandsControllersOnStartup = 1` (controllers can sit on the desk without taking
  the hands away). Simultaneous hands and controllers relies on multimodal support, which may need a
  manifest or feature flag on the Quest 3S; without it the setting does nothing, so the headset proof
  covers it.
- **No runtime objects:** the panel and the row are authored; the setup references them in the scene.

## Units

### U1 — the check and the step

- Add `HandTrackingGate` in `Placement.Domain`: a small stateful rule fed each frame with
  (enabled, left tracked, right tracked, delta time) that reports *waiting* or *ready* — ready once
  both hands stay tracked for the steady window; never ready while hand tracking is off or either
  hand is missing. Its EditMode tests in `Placement.Tests.EditMode` cover the steady window, a
  flicker that resets it, one hand never passing, and "off never passes".
- Add `HandTrackingSetup : XRSpaceSetup` in `Placement.Views` (hands + panel references); it ticks the
  gate once the headset is worn and calls `done` when the gate is ready.
- In `_Modules/Casual`: append `SetupStep.Input = 3` (the existing `Services`/`Space`/`PlayerData`
  values stay) and give `SetupState` one optional serialized `XRSpaceSetup` input step that runs in
  `CheckDevice`, after the AR-support branch and before `RunStep(SetupStep.Space, ...)`; empty skips
  it. No game names in the module.
- Exit: the editor is unaffected; a device with hand tracking off stays on `> HANDS`; turning it on
  with both hands tracked continues; one raised hand does not continue; a device that is not worn
  does not complete the step.

### U2 — the panel, the row and the config

- Add the `HANDS` row to `BootChecklist` in `Game.unity` between `LINK` and `SEAT`, and a title-only
  `HandTrackingRequiredPanel` with the copy; add `hand_tracking_required` to the `Global` table.
- Set the three `OVRManager` fields above; wire the checker, the panel and the row in `Game.unity`.
- Exit: controllers on the desk and hand tracking off shows the panel; turning hand tracking on with
  both hands in view hides it and the boot continues; no scene-permission dialog.

### U3 — the headset proof

- Run the manual list below on the Quest 3S with hand tracking on and off; record the first-frame
  timing and the `HANDS` row read at arm's length.
- Update [Review](../../reviews/review.md) finding 10 and the [plans board](../README.md).
- Exit: the list passes; the board closes this plan.

## Not in this plan

- A controller fallback; the entry stays hands-only.
- Mid-job hand-loss recovery; the gate is at boot only.
- The 2ft/seat tuning (UX finding 11) and the music duck.
- A hand-tracking tutorial beyond the one panel.

## Manual check (headset)

1. Controllers on the desk, hand tracking off: the boot holds on the hand-tracking panel; the field is
   not placed.
2. Turn hand tracking on in Settings > Movement Tracking with both hands in view: the panel hides and
   the boot continues.
3. Both hands in view: the panel hides, then the rows read `LINK → HANDS → SEAT → LEDGER`; the field
   places at the resting-hand height.
4. Controllers connected on the desk with hands tracked: the hands drive the boot; no
   controller-driven pose appears.
5. No scene-permission dialog at launch.
6. Cover both hands during the gate: the panel returns; uncover: it hides and the boot resumes.
7. Raise only one hand: the panel stays, and the boot does not continue; raise both hands: it
   continues.
8. Hands flickering in dim light: the panel does not blink on and off; the steady window holds.
9. Hold the controllers, then put them down: the hands take over within a second (multimodal works
   on the 3S).
10. Put the headset down and back on during the gate: the boot resumes without a stuck row.
11. Take the headset off before both hands track: the gate does not complete; put it on again and
    track both hands: it continues.
12. The `HANDS` row is legible at arm's length and in the right visual order.
13. Launch the built APK: no black frame, no stuck `> HANDS` with hands tracked.
