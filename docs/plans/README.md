# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work. The one thing being built.

- [Space shaping — a more realistic field](in-progress/space-shaping-realistic-field.md) — a 3D
  disc/bulge/cluster plus a star with real mass, so the field reads as a galaxy rather than a flat diagram.
  Starts Mon 2026-11-02.

## Next

Queued. One plan moves to **Now** when it starts.

- [Space shaping — rings on some planets](backlog/space-shaping-planet-rings.md) — authored rings on a
  subset of planets, chosen at puzzle creation and baked with the level.

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
