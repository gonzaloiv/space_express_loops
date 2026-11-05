# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work. The one thing being built.

- Nothing active.

## Next

Queued. One plan moves to **Now** when it starts.

- Nothing queued.

## Later

Design-only or deferred. Nothing here is scheduled.

- [Puzzle map — playable selected field](backlog/puzzle-map-play-field.md) — the selected map field
  becomes the shared play field, with one session and an in-place play-only group.

- [Puzzle map — selection info and onboarding](backlog/puzzle-map-info.md) — the cube carries the job
  name, state and stars, and the swipe/zoom verbs are taught in place.

- [TTS job intro — the contract screen](backlog/tts-job-intro.md) — a field-side contract screen
  (manual + tech voice) that can reposition around the field; the camera-operator companion is deferred.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Re-run Validation 1 on the current loop.** The gate for the slice; the test steps need a home now
  that the first-minute read plan is gone.
- **Run authoring.** Jobs 1–7 are baked into Chapter 01 (seven jobs); job 8 is the deferred Mixed
  capstone with its own future plan. The TTS intro and map plans are above.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. Fold it into the first-minute read
  plan or the feel pass.
- **Resize.** Working, but the explanation (size is scope, not precision) is not ready. Deferred from the
  slice; next cut candidate.
- **Capstone routes (job 8).** The capstone is deferred to its own future plan; it still needs at least
  two routes and a Mixed/InZone field that bakes. Not scheduled.

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
