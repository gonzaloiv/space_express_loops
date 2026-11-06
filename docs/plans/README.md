# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Road to v1

The ordered actions to a first "final" version (decided 2026-11-06). Each step becomes a plan (a
morning) when it starts; a step that is already a plan links to it. Steps marked *(proposed)* still
need a yes.

1. **Land the current plan:** [loop completion](in-progress/loop-completion.md).
2. **[Gesture prompts](backlog/gesture-prompts.md)** — job 1 opens with nothing else to teach the pinch.
3. **[Job definition card](backlog/job-definition-card.md)** — the manual entry's only surface.
4. **Tune the run on device.** Per-job scramble and planet count; bake-check job 6's "the field
   carries you" route as the cheapest.
5. **The solve moment.** Arrival payoff at the destination (flash + sound), then the stars stamped in
   (terminal style), then Jim's receipt — what makes the player want the next job.
6. **The ending** *(proposed)*. After job 7: the whole map lit, total stars, the "Our Space" title, an
   invitation to replay for 3★.
7. **Minimum sound and haptics** *(proposed)*. Pinch grab tick, stretch tone, release, arrive chime,
   crash, fail sting, quiet ambient bed.
8. **The first 30 seconds** *(proposed)*. App open → bubble at a comfortable seated reach without
   setup → title beat → the Scale prompt.
9. **Quest 3S performance pass at 72 fps** *(proposed)*. Map dioramas, trajectory previews, lattice.
10. **Voice and content** *(proposed)*. Jim's clips, job 6's lines for the new lesson, the Spanish review.
11. **Competition deliverables.** Capture, a short trailer, the submission page.

**Out of v1:** job 8 (Mixed), resize, multi-ship jobs, in-solve tips, the playable map field, more
chapters.

## Now

Active work.

- [Loop completion — solve, fail, return](in-progress/loop-completion.md) — a job ends once on a solve or
  a fail, the solve is saved so the map fills, and job 1 starts silent. **First:** it makes the run
  playable end to end.

## Next

Queued. One plan moves to **Now** when it starts.

- [Gesture prompts — show each gesture the first time](backlog/gesture-prompts.md) — scale, enter, pan
  and move prompts on the cubes, shown where each is first needed and learned once per save.
  **First after loop completion:** job 1 opens with nothing else to teach the pinch.

- [Job definition card — the contract from job 2](backlog/job-definition-card.md) — client, cargo and the
  manual entry on a card that fades on its own; the puzzle stays live. After loop completion.

## Later

Design-only or deferred. Nothing here is scheduled.

- Nothing deferred. Help reaches the player only at the start (the card, Jim's line, the gesture
  prompts) and the end (the complete/fail message) — no in-solve hint surface.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish, playable map field.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Run authoring.** Jobs 1–7 are baked into Chapter 01 (seven jobs); job 8 is the deferred Mixed
  capstone with its own future plan. The job-card plan is above.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. Fold it into the in-puzzle read
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
record. Do not archive it in a `done/` folder.
- Run `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
