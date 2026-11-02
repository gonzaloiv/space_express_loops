# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work. The one thing being built.

- [First-minute read and cost](in-progress/first-minute-read.md) — 3-planet tutorial first job,
  straight-line baseline, and an in-field entropy bar. In progress — S1, S2, S2.5 and S3 are implemented
  (tutorial rebaked, baseline reference, unified colour set, in-field bar); run Validation 1.

## Next

Queued. One plan moves to **Now** when it starts.

- [Puzzle solution mechanism — curve or clear](backlog/puzzle-solution-mechanism.md) — the creator
  classifies a solve as a well curve or an obstacle clear, and a job requests its role.

## Later

Design-only or deferred. Nothing here is scheduled.

- [Space shaping — the scale ghost](backlog/space-shaping-scale-ghost.md) — one scale ladder shared by
  the bake and the pinch, with a ghost preview of the value the hands are about to commit.
- [Puzzle selection — the run carousel](backlog/puzzle-selection.md) — an arc of job fields, one
  focused, touched and amplified into play; starts after the PoC is in the main scene.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion, feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Re-run Validation 1 on the current loop.** The gate for the slice; the
  [first-minute read plan](in-progress/first-minute-read.md) §3 runs it.
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
