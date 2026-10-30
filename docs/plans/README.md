# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work. The one thing being built.

- [Multi-ship](in-progress/space-shaping-multiship.md) — bake and play a two-ship puzzle from one field
  edit; N-ready.

## Next

Queued. One plan moves to **Now** when it starts.

- [Space shaping — the sun and the dust](backlog/space-shaping-sun-and-dust.md) — a star with real mass
  (a source, marked by a reference) plus a digital-noise dust layer that warps with the grid.

## Later

Design-only or deferred. Nothing here is scheduled.

_Empty._

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion, feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Re-run Validation 1 on the maze.** The shape-the-path change retired the window/clock, so the
  timing pass no longer applies; the maze mechanic needs a fresh validation pass.

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
