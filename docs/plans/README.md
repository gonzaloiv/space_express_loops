# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube):
scale the space, scale the content; the ship flies the deformed spacetime.

## Now

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — physics (clock + mass)](in-progress/space-shaping-physics.md) | space shaping | Clock, mass, proper-time window, and the off-path clock bake are landed; the POC re-bake + headset pass remain. |

## Next

The runway after Now: the remaining sim work (multi-zone), the lab readout, then the
**validation** (the criteria are in [vision](../design/vision.md) §7). Only if it passes: the fused slice
(validation 2), then the campaign.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — multi-zone solutions](backlog/space-shaping-multizone.md) | space shaping | `moves = 2+` bakes for jobs 3 and 6–8; needed before the full run can be authored. |
| [Space shaping — lab readout](backlog/space-shaping-lab-readout.md) | presentation | The validation-1 readout (entropy, window, result); separate from the final HUD. |
| [Space shaping — integration into the game](backlog/space-shaping-integration.md) | space shaping | **Full replacement.** Two stages: fuse the slice (I0–I1 = validation 2), then the campaign (I2–I5). **Gated on validation 1.** |

## Later

Design-only or deferred. Nothing here is scheduled.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — feel & presentation polish](backlog/space-shaping-feel.md) | presentation | The *feel* beats after the validation: travel payoff, collapse feedback, window gauge, cargo personality. |

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack, hologram
look, TTS companion.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

| Open tail | Lands in |
|---|---|
| Silent planets — persistence/restore validation | A future cross-feature restart pass |
| Sector field — raise `viewRadius`/aperture | Trigger: the campaign map slice (I1) → [integration](backlog/space-shaping-integration.md) |
| The lens — surface the session `Undo`; **rotate** (later puzzles) | [Feel](backlog/space-shaping-feel.md) / [variations](../ideas/variations.md) |

## Conventions

- Every plan starts with a status line:
  `**Status:** <state> · <area> · updated <yyyy-mm-dd> — <one-line summary>`
  where `<state>` is one of `now`, `next`, `later`.
- `<area>` is one of `space shaping`, `universe`, `presentation`, `flow`.
- Folders: `now` → `in-progress/`; `next` and `later` → `backlog/`. Move the file and update this
  board in the same change.
- Run `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
