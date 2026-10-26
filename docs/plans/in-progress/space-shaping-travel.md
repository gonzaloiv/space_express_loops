# Space Shaping — Travel on Every Modification

**Status:** now · space shaping · updated 2026-10-26 — launch on every committed pinch: no commit
step, no idle trigger, no prediction line.

**Related:** [core loop](../../design/core-loop.md) §1–§2 · [resources](../../design/systems/resources.md) ·
[the cube](../../design/systems/cube.md) · [puzzle creator](space-shaping-puzzle-creator.md)

**Wiring rule:** authored objects + serialized refs only; no runtime component lookup, mesh, or
asset creation. Feedback is an authored object in the lab scene.

## 1. Goal

The design loop has **no commit** ([core loop](../../design/core-loop.md) §2): every modification
launches the ship along the resulting trajectory. The lab currently launches on **solved-and-idle**
(`ShipTravelView` + a `0.5s` trigger) and draws a separate **prediction line**. Replace both.

## 2. The change

- **On each committed pinch** (one modification), the ship launches and flies the **current
  trajectory** — the launch is a *trajectory check*, not a hard attempt.
- **Pass:** the flight lands inside `[Tmin, Tmax]` → the job is passed (graded on entropy left).
- **Fail:** the flight misses **or** crashes into a planet → reported as a fail on the HUD. For now
  that is all — no destroy visual.
- **Keep shaping freely:** a fail does not end the job; the player edits on. A **new modification
  interrupts** an in-flight run and launches the new one.
- **Entropy is charged per modification, not per launch** — it persists across launches.
- **Restart:** each launch starts from the origin.
- **No prediction line:** the flight *is* the reveal; the trajectory visual updates **at launch**.

## 3. Scope

- `SpaceShapingState.LateUpdate`: launch on the committed pinch (`wasScaling` → `CommitPreview`),
  not on solved-and-idle. Remove `travelIdleSecs` (dead).
- `ShipTravelView`: launch per modification; a new launch **interrupts** the current flight; restart
  from the origin.
- **Remove the prediction:** delete `TrajectoryPredictionView` and the **Predict** button/toggle
  (`predictButton`, `TogglePrediction`, `view.SetPredictionVisible`).
- **Fail / crash:** report the outcome on the HUD; no destroy visual for now.
- **HUD:** pass / fail / grade on `ShapingReadout`.

## 4. Decisions

| Decision | Call |
|---|---|
| Launch timing | **Immediately** on the committed pinch. |
| Interrupt | A new modification **cancels** the in-flight run. |
| Restart | Each launch **starts from the origin**. |
| A "modification" | **One committed pinch** — the entropy unit. |
| Fail | A window **miss** or a planet **crash**. |
| Destroy | **None for now** — a completed run that misses is the fail. |
| Entropy | Charged **per modification**; persists across launches (failing drains the bar). |
| After a fail | **Keep shaping** — the launch is only a check. |
| Surfacing | Pass / fail / grade on the **HUD**. |
| Prediction | **Removed** — update the trajectory visual at launch only. |

## 5. Deferred

- The destroy / explosion visual and the collapse ([feel](../backlog/presentation/space-shaping-feel.md)).
- Flight-duration tuning (`travelSecs`) now that it fires on every edit.

## 6. Tests

- EditMode: a committed pinch produces a launch; a miss and a crash both report a fail; entropy is
  charged once per modification, not per launch; a new launch interrupts the previous flight.
