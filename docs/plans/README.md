# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work. The one thing being built.

- [Puzzle map — the run as a global field](in-progress/puzzle-map.md) — puzzle fields in a galaxy
  cluster; the selected field is a diorama, nearby full fields are culled by radius, the selection reads
  by a selection cube, and S4 replaces the diorama with the shared full play field.

- [Bake — role and sun alignment](in-progress/bake-role-sun-alignment.md) — make the bake produce the
  designed roles (sun-Curve, Clear, Mixed) so the 8-job set stops falling back to Clear or duplicates.

## Next

Queued. One plan moves to **Now** when it starts.

- Nothing queued.

## Later

Design-only or deferred. Nothing here is scheduled.

- [Puzzle map — selection info and onboarding](backlog/puzzle-map-info.md) — the cube carries the job
  name, state and stars, and the swipe/zoom verbs are taught in place.

- [Space shaping — the scale ghost](backlog/space-shaping-scale-ghost.md) — one scale ladder shared by
  the bake and the pinch, with a ghost preview of the value the hands are about to commit.

- [TTS job intro — the contract screen](backlog/tts-job-intro.md) — a field-side contract screen
  (manual + tech voice) that can reposition around the field; the camera-operator companion is deferred.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Re-run Validation 1 on the current loop.** The gate for the slice; the test steps need a home now
  that the first-minute read plan is gone.
- **Run authoring.** Jobs 1–8 and the TTS intro (manual + tech voice) are designed but not built. The
  TTS intro has a plan ([TTS job intro](backlog/tts-job-intro.md)); the map/level selector now has
  [Puzzle map](in-progress/puzzle-map.md). The job authoring still needs one before the 10-minute slice.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. Fold it into the first-minute read
  plan or the feel pass.
- **Resize.** Working, but the explanation (size is scope, not precision) is not ready. Deferred from the
  slice; next cut candidate.
- **Capstone routes.** The design requires at least two routes for the capstone jobs (7–8); the bake
  currently enforces only the movable single-zone case. Fold into
  [bake role and sun alignment](in-progress/bake-role-sun-alignment.md) before authoring them.

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
