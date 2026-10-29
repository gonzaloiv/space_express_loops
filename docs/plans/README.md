# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

_Empty._

## Next

_Empty._

## Later

Design-only or deferred. Nothing here is scheduled.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — shape the path](backlog/space-shaping-shape-the-path.md) | space shaping | **Direction 2026-10-29:** the field becomes a maze and the trajectory is molded between planets with the cube; the time window is retired and arrival replaces it. Pre-validation (re-runs Validation 1). |

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion, feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

_Empty._

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
