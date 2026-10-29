# Space Shaping — HUD + Validation 2

**Status:** next · space shaping · updated 2026-10-28 — the over-level HUD (entropy, clock window,
result/fail) and then validation 2. **Its exit is the gate** for progression, the legacy cut and the
catalogue.

**Related:** [in the game](../in-progress/space-shaping-in-game.md) · [campaign map](space-shaping-campaign-map.md) ·
[progression](space-shaping-progression.md) · [legacy cut](space-shaping-legacy-cut.md) ·
[vision](../../design/vision.md) · [core loop](../../design/core-loop.md) ·
[resources](../../design/systems/resources.md) · [feel polish](../../ideas/feel-polish.md)

**Wiring rule:** the HUD is authored in the scene over the level and subscribes to the readout — no
runtime UI construction, no `GetComponent` wiring.

## 1. Context

The loop needs its surface before it can be validated: the player must see what a scale costs, when the
ship arrives relative to the window, and whether the job is solved, missed or failed. The lab readout
(landed 2026-10-28) already exposes the seam — a read-only `PuzzleReadout` plus one `ReadoutChanged` event.

## 2. Slices

### H1 — Entropy and clock band

- Entropy left / budget as the primary bar, with the grade it currently projects.
- The window `[Tmin, Tmax]` drawn as a **band on the ship clock**, so a miss reads as “early/late”, not as
  a number.
- The HUD sits **over the level** (world-anchored), not in a screen-space panel.
- **Exit:** shaping updates both surfaces live; a miss visibly lands the arrival outside the band; no
  per-frame allocation (the readout is event-driven, not polled).

### H2 — Result and fail surface

- Result panel shows solved + stars, or the miss, or the fail; offers **Map / Retry** (no Next — navigation
  is direct pick only).
- The result is a **panel inside `SpaceShapingState`**, not a separate Flow state.
- Fail only on committed spend reaching the budget; a miss keeps shaping and the panel is dismissible.
- Locked-gesture refusal (a locked cube or a locked condition) is a
  [feel polish](../../ideas/feel-polish.md) candidate, not built here.
- **Exit:** solve → stars → Retry resets clean, Map returns to the map; miss → keep shaping; fail → panel
  with Map/Retry only.

### H3 — Validation 2

- Run the one seated MR path end to end: boot → map → pick → shape/fly/check in the same Bubble view →
  solve → back to the map.
- Fix what it exposes inside this plan's scope; anything outside it goes back to its plan.
- **Exit:** the path completes in the headset with a scored puzzle; the result is recorded against the
  [vision](../../design/vision.md) §7 criteria; frame timing sampled on device.

## 3. Gate

Validation 2 is the gate for the rest of the integration. If it fails, plans 4–6 do not start; the failure
goes back into [in the game](../in-progress/space-shaping-in-game.md) or [campaign map](space-shaping-campaign-map.md),
whichever owns it.

## 4. Out of scope

- Save, stars persistence, legacy reset — [progression](space-shaping-progression.md).
- Deleting the legacy flow — [legacy cut](space-shaping-legacy-cut.md).
- Feel polish, final art/audio — [feel polish](../../ideas/feel-polish.md).
- Undo UI: the session call exists, the surface is not in this slice.
