# Space Shaping — Progression Save

**Status:** next · space shaping · updated 2026-10-28 — solved jobs + stars, current/unlocked, reset, and the
placement carve-out off the legacy `GameSnapshot`. Starts only after validation 2 passes.

**Related:** [HUD + validation 2](space-shaping-hud-validation.md) · [campaign map](space-shaping-campaign-map.md) ·
[legacy cut](space-shaping-legacy-cut.md) · [progression](../../design/systems/progression.md) ·
[jobs](../../design/jobs.md) · [cube](../../design/systems/cube.md)

## 1. Context

The save is **player progression only**: which jobs are solved and with how many stars. The puzzle field is
baked, so a run is always rebuilt from the baked puzzle — never resumed mid-flight. Never a mid-level
snapshot.

## 2. Slices

### P1 — Record and write

- One `LevelCompleteCookie` per job; add `stars` (1–3) to `LevelCompleteCookieMetadata`; `hasBeenCompleted`
  stays the unlock flag; `score` stays generic.
- `LevelSelector` supplies the current/following id from the cookies; the catalogue supplies the order.
- Write on solve **and** on app pause through `PlayerData`; replay keeps the best stars.
- **Exit:** solve a job → cookie written with stars and `hasBeenCompleted`; a worse replay does not lower
  the stars; app pause mid-map persists.

### P2 — Restore on boot

- Boot rebuilds the campaign field and the map from the cookies; nothing in-level is restored.
- **Exit:** solve → quit → relaunch → the map shows solved, current and locked correctly, with stars.

### P3 — Reset and the legacy cookie

- **Reset** clears the puzzle completion cookies and keeps the bubble placement.
- First boot **ignores the legacy `GameSnapshot`** — no migration, reset-to-fresh.
- **Keepers inside `GameSnapshot`** — `bubbleOffsetLocal`, `bubbleYawDegrees` and `silentPlanetSeed` are
  not legacy route data. Move them to the placement/campaign snapshot (`PlacementStore`) **before** the
  cookie is dropped, so the [cut](space-shaping-legacy-cut.md) can delete the rest.
- **Exit:** reset → map and stars cleared, bubble stays where the player put it; a stale legacy cookie on
  disk is ignored and a fresh save is written.

## 3. Contract

| Item | Value |
|---|---|
| Persistence | Solved job ids + stars; current/unlocked. Never in-level state. |
| Record | `LevelCompleteCookie` per job; `stars` in the metadata; best-stars wins. |
| Reset | Clears completion cookies; keeps bubble placement. |
| Legacy | `GameSnapshot` ignored on first boot; keepers carved out first. |

## 4. Tests

- Cookie round trip: stars and `hasBeenCompleted` survive a reload; best-stars merge on replay.
- `LevelSelector` current id across solved/locked mixes, including an all-solved chapter.
- Reset clears completions only; placement fields untouched.

## 5. Out of scope

- Deleting the legacy persistence fields and assets — [legacy cut](space-shaping-legacy-cut.md).
- Cloud save, accounts, analytics of progression events.
- Mid-level resume — explicitly refused.
