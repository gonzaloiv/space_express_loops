# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube):
scale the space, scale the content; the ship flies the deformed spacetime.

## Now

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — in the game (one puzzle)](in-progress/space-shaping-in-game.md) | space shaping | Boot `Game.unity` into one baked puzzle: flow, result seam. **Unblocked — validation 1 passed on the Quest 3S POC (2026-10-29).** |

## Next

The runway after Now, one morning per plan. **Validation 1** (the mechanic survives a goal and a cost)
passed on the Quest 3S POC on 2026-10-29, so the integration is underway — the in-game slice is Now.
**Validation 2** is still the gate: its criteria are in [vision](../design/vision.md) §7 and it lands at the
exit of the [HUD plan](backlog/space-shaping-hud-validation.md); only after it pass do progression, the
legacy cut and the catalogue start.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — campaign map](backlog/space-shaping-campaign-map.md) | space shaping | Cube map, pick → brief → play in the same Bubble view → back; keeper moves out of `LevelContainer`. |
| [Space shaping — HUD + validation 2](backlog/space-shaping-hud-validation.md) | space shaping | Entropy, clock window, result/fail; **this exit is the gate** for the campaign. |
| [Space shaping — creator simplification](backlog/space-shaping-creator-simplify.md) | space shaping | Route-first, hand-designed bake replaces the random-field seed search. Does not block the gate. |
| [Space shaping — progression save](backlog/space-shaping-progression.md) | space shaping | Solved jobs + stars, reset, placement carve-out. After the gate. |
| [Space shaping — legacy cut](backlog/space-shaping-legacy-cut.md) | space shaping | Delete the route/round/economy flow once the keepers are out. After the gate. |
| [Space shaping — catalogue + acceptance](backlog/space-shaping-catalogue.md) | space shaping | Re-bake the catalogue with the `routes` field and author 3–5, then 8–10, jobs; then the Quest 3S acceptance pass. |

## Later

Design-only or deferred. Nothing here is scheduled.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — multi-zone solutions](backlog/space-shaping-multizone.md) | space shaping | Parked 2026-10-29: a straight shot at the destination means one zone always tunes the arrival, so no seed bakes a level that *needs* two zones. Needs a second spatial constraint (mid-run gate) or the cut. |

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion, feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

| Open tail | Lands in |
|---|---|
| Silent planets — persistence/restore validation | A future cross-feature restart pass |
| Sector field — raise `viewRadius`/aperture | Trigger: the campaign map slice → [campaign map](backlog/space-shaping-campaign-map.md) |
| Baked catalogue assets carry no `routes` field — re-bake so the movable jobs satisfy the ≥2-route gate | [catalogue](backlog/space-shaping-catalogue.md) (and the polish route count) |
| Jobs 3 and 6–8 need two zones; `01_05`–`01_08` are single-solution levels | [multi-zone](backlog/space-shaping-multizone.md) — parked until it has a second spatial constraint |
| The lens — **Undo** UI (the session `Undo` exists; no surface) | Later; not this slice — [in the game](in-progress/space-shaping-in-game.md) §4 |
| The lens — **rotate** (later puzzles) | Not scheduled; a later-puzzles plan ([cube](../design/systems/cube.md)) |

## Conventions

- Every plan starts with a status line:
  `**Status:** <state> · <area> · updated <yyyy-mm-dd> — <one-line summary>`
  where `<state>` is one of `now`, `next`, `later`.
- `<area>` is one of `space shaping`, `universe`, `presentation`, `flow`.
- Folders: `now` → `in-progress/`; `next` and `later` → `backlog/`. Move the file and update this
  board in the same change.
- **Done plans are removed.** When a plan lands, delete its file and its board row; the code and tests
  are the record. Do not archive it in a `done/` folder.
- Run `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
