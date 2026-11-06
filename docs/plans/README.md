# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Now

Active work.

- [Loop completion — solve, fail, return](in-progress/loop-completion.md) — a job ends once on a solve or
  a fail, the solve is saved so the map fills, and job 1 starts silent. **First:** it unblocks
  Validation 1.

- [TTS messages — the jumbotron panel](in-progress/tts-messages.md) — one state-launched TTS panel (the
  jumbotron, with Jim's face) for the job intro and complete message; legacy TTS content removed.

## Next

Queued. One plan moves to **Now** when it starts.

- [Job definition card — the contract from job 2](backlog/job-definition-card.md) — client, cargo and the
  manual entry on a card that fades on its own; the puzzle stays live. After loop completion, before
  Validation 1.

## Later

Design-only or deferred. Nothing here is scheduled.

- Nothing deferred. Help reaches the player only at the start (the card, Jim's line) and the end (the
  complete/fail message) — no in-solve hint surface.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish, playable map field.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Re-run Validation 1 on the current loop.** The gate for the slice; runs once
  [loop completion](in-progress/loop-completion.md) lands. The test steps still need a home.
- **Run authoring.** Jobs 1–7 are baked into Chapter 01 (seven jobs); job 8 is the deferred Mixed
  capstone with its own future plan. The TTS and job-card plans are above.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. Fold it into the in-puzzle read
  plan (after Validation 1) or the feel pass.
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
