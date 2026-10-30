# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work. The one thing being built.

- [Space shaping — a more realistic field](in-progress/space-shaping-realistic-field.md) — a 3D
  disc/bulge/cluster plus a star with real mass, so the field reads as a galaxy rather than a flat diagram.
  In progress — depth, the sun (per-seed presence and lock) and the runtime lock are in; next: the
  headset read and cost pass (S4).

## Next

Queued. One plan moves to **Now** when it starts.

- [First-minute read and cost](backlog/first-minute-read.md) — baked tutorial first job, straight-line
  baseline, and an in-field entropy bar. The read is the gate.

## Later

Design-only or deferred. Nothing here is scheduled.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion, feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Re-run Validation 1 on the current loop.** The gate for the slice; the
  [first-minute read plan](backlog/first-minute-read.md) §3 runs it.
- **Run authoring.** Jobs 1–5, the map/level selector, and the TTS intro (manual + tech voice) are
  designed but not built; add a plan before the 10-minute slice.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. Fold it into the first-minute read
  plan or the feel pass.
- **Resize.** Working, but the explanation (size is scope, not precision) is not ready. Deferred from the
  slice; next cut candidate.
- **Capstone routes.** The design requires at least two routes for the capstone jobs (6–7); the bake
  currently enforces only the movable single-zone case. Fix the bake before authoring them.

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
