# Space Shaping — Spike (Smallest Testable Version)

**Status:** done - passed. Found the launch-to-curvature ratio band and chose **coast**; the
POC then validated the feel.

**Related:** [POC](space-shaping.md) · [Reframing and variations](../backlog/gameplay/space-shaping-future.md) · [Integration into the game](../backlog/gameplay/space-shaping-integration.md) · [View frame (bubble window)](universe-bubble-window.md)

**Wiring rule:** Tier 0 is pure C#. Tier 1 uses authored scene objects and serialized refs only; no runtime component lookup or creation.

## 1. Goal

Find the **launch-to-curvature ratio** and decide go/no-go.

Hypothesis: moving a source (a planet) bends the ship's path, and there is a band of the
ratio where the destination stays reachable but the puzzle is not trivial. If no such
band exists, the whole reframing is in trouble — and this spike finds that out in a day.

## 2. Model note (relativity)

- Spacetime is a **scalar potential** `Φ` produced by **sources** (planets). The grid is
  **derived** from `Φ`, never edited directly.
- The ship moves through the curved space: `accel = -∇Φ`. This Newtonian gradient is the
  practical stand-in for a geodesic; the relativity is the metaphor and the visual.
- Two launch models to try in the spike (same integrator, different forcing):
  - **Coast (inertial / geodesic)** — an initial velocity only; the ship drifts and the
    space bends it. This is the "straight inertial vector" feel. **Primary.**
  - **Thrust** — a constant force in a direction; a Newtonian rocket. **Fallback.**
- Pick the launch model from the feel; it is a real fork, not a detail.

## 3. What it proves / does not prove

- **Proves:** the field model bends predictably, is deterministic, and is tunable to a
  legible puzzle.
- **Does not prove:** VR interaction, source editing, the grid visualization, the Sculpt
  frame, commands/undo, multi-ship, or scoring.

## 4. Tier 0 — pure C# (no scene, hours)

- `SpacetimeField`: one source (planet), potential from radius.
- `TrajectoryIntegrator`: fixed step, fixed count: `accel = -∇Φ` (+ thrust if enabled).
- One EditMode test asserting:
  - empty field -> the path is a straight line;
  - one planet -> the path bends toward it;
  - same input -> identical output across runs.
- No Unity objects, no scene, no VR.

## 5. Tier 1 — tiny 3D debug scene (the real feel test, ~a day)

- A new throwaway scene (not `Game.unity`). **3D**, with the planet in the A -> B plane
  first so the ratio is easy to eyeball; free it in 3D once the ratio is found.
- Origin planet `1`, destination planet `2`, one ship, one planet.
- Drag the planet with an editor handle / mouse; a `LineRenderer` redraws the path live.
- Arrive = the path passes within the capture radius; crash = it enters the planet radius.
- No hands, no lattice, no frame — just drag and a line.

**Success criterion:** there is a visible band of the launch-to-curvature ratio where the
path is clearly bendable *and* the destination is reachable by a non-obvious source
placement. If the path is either a lottery or trivially straight at every ratio, stop and
rethink the mechanic.

## 6. Parameters

| Parameter | Default |
|---|---|
| Dimension | 3D; origin at `(0,0,0)`, destination along `+X`; planet free (start in-plane) |
| `launchModel` | `Coast` (try `Thrust` too) |
| `launchSpeed` | tuned (coast) |
| `thrustAccel` | tuned (thrust model only) |
| `sourceStrength` | derived from radius |
| `captureRadius` | `1.5 x` destination radius |
| `collisionRadius` | `1.0 x` planet radius |
| `stepSecs` | `1/120 s` |
| `maxSteps` | `1200` (~10 s) |

## 7. Out of scope

Source editing, grid visualization, Sculpt frame, hands/VR, commands/undo, UI,
persistence, multi-ship, scoring.

## 8. Exit / decision

- **Pass:** a usable ratio band exists -> proceed to the [POC](space-shaping.md) S1
  (source editing + derived grid).
- **Fail:** the path is a lottery or trivial at every ratio -> stop; the mechanic needs
  rethinking before any further build.
