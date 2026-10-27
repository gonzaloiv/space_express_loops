# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube):
scale the space, scale the content; the ship flies the deformed spacetime.

## Now

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — puzzle creator: the brief](in-progress/puzzle-creator-improvements.md) | space shaping | Complete: conditions, condition-aware bake, derived window, one best level, runtime gesture locks. The next action is Validation 1 ([vision](../design/vision.md) §7). |

## Next

The runway after Now: the **validation** (the criteria are in
[vision](../design/vision.md) §7), then — only if it passes — the fused slice (validation 2), then the
campaign.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — physics (clock + mass)](backlog/gameplay/space-shaping-physics.md) | space shaping | The two-term clock and `mass ∝ radius³`; the bakes search the sim, so land before baking variants. |
| [Space shaping — integration into the game](backlog/gameplay/space-shaping-integration.md) | space shaping | **Full replacement.** Two stages: fuse the slice (I0–I1 = validation 2), then the campaign (I2–I5). **Gated on validation 1.** |

## Later

Design-only or deferred. Nothing here is scheduled.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — feel & presentation polish](backlog/presentation/space-shaping-feel.md) | presentation | The *feel* beats after the validation: travel payoff, collapse feedback, window gauge, cargo personality. |
| [Space shaping — tech debt](backlog/gameplay/space-shaping-tech-debt.md) | space shaping | Code/authoring hygiene carried out of the creator improvements plan. |

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack, hologram
look, TTS companion.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

| Open tail | Lands in |
|---|---|
| Silent planets — persistence/restore validation | A future cross-feature restart pass |
| Sector field — raise `viewRadius`/aperture | Trigger: the campaign map slice (I1) → [integration](backlog/gameplay/space-shaping-integration.md) |
| The lens — surface the session `Undo`; **rotate** (later puzzles) | [Feel](backlog/presentation/space-shaping-feel.md) / [variations](../ideas/variations.md) |

## Conventions

- Every plan starts with a status line:
  `**Status:** <state> · <area> · updated <yyyy-mm-dd> — <one-line summary>`
  where `<state>` is one of `now`, `next`, `later`.
- `<area>` is one of `space shaping`, `universe`, `presentation`, `flow`.
- Folders: `now` → `in-progress/`; `next` and `later` → `backlog/`. Move the file and update this
  board in the same change.
- Run `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
un `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
