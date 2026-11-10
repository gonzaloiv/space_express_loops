# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Road to v1

The ordered actions to a first "final" version (decided 2026-11-06). Each step becomes a plan (a
morning) when it starts; a step that is already a plan links to it. Steps marked *(proposed)* still
need a yes.

1. **Tune the run on device.** Per-job scramble and planet count; bake-check job 6's "the field
   carries you" route as the cheapest.
2. **The solve moment** — done (2026-11-09). Arrival payoff at the destination (flash + sound), then the stars stamped in
   (terminal style), then Jim's receipt — what makes the player want the next job.
3. **The ending** *(proposed)*. After job 7: the whole map lit, total stars, the "Our Space" title, an
   invitation to replay for 3★.
4. **Minimum sound** — done (2026-11-09); the soundscape built on it (done 2026-11-10). Pinch grab
   tick, stretch tone, release, arrive chime, crash, fail sting. No haptics: the game is hands-only.
5. **The first 30 seconds** — done (2026-11-10). App open → the console boot → the field placed at a
   comfortable seated reach → the Scale prompt. Pause and offline (the judge tests) also done 2026-11-10.
6. **Quest 3S hitches at 72 fps** *(proposed)*. Sudden frame drops, not sustained low fps: map
   dioramas, trajectory previews, lattice.
7. **Voice and content** *(proposed)*. Job 6's lines for the new lesson. Jim's complete and fail lines
   are split into one-sentence TTS entries and his clips are downloaded (done 2026-11-09). The music duck under Jim (deferred from the soundscape) is tuned with his clips. English only: Spanish is kept just to prove the localization system works.
8. **Competition deliverables — due Friday 2026-11-13; work starts Wednesday 2026-11-11.** The entry
   is frozen at the deadline. The angle, hero moment and video outline are in [pitch](../design/pitch.md);
   until Wednesday the time goes to the game.
   - **Build:** APK on a new release channel named "Competition" in the developer dashboard, plus an
     Invite URL for judges; it must stay launchable through judging.
   - **Video:** under 3 minutes, on-device footage, public on YouTube or Vimeo; the best material in
     the first seconds (judges may stop early).
   - **Form:** name, tagline (≤140 characters), track (**Gaming**), division (**New Experience**), a ≤500-word description (inspiration, how it was built, future plans), how hand
     interactions are used, target launch date, team names, emails and roles.
   - **Judge tests to pass:** fully hands-only end to end; the airplane-seat test (two-foot radius);
     the one-bus-stop test (a complete moment fast, clean pause/resume).

**Out of v1:** job 8 (Mixed), resize, multi-ship jobs, reactive in-solve tips, the playable map field,
more chapters.

## Road to the store

After v1 (decided 2026-11-07): a **~€5 Meta store release** sooner, then chapters as updates. The game
shape is in [chapters](../design/chapters.md); the demo is the store's own option.

1. **Job 8 (Mixed)** closes chapter 1 — bake it, or ship chapter 1 at seven jobs and land it before
   chapter 2 opens.
2. **Chapter 2 — the fleet.** [Plan](backlog/chapter-2-fleet.md).
3. **Freelance contracts.** ~150 prebaked jobs, tiers, the quality filter, the exact-entropy score.
4. **The daily contract.** One pool job per day, Meta platform leaderboard.
5. **Price check.** Measure minutes per job on the headset; confirm ~€5 and the pool size.
6. **Store release.** Store page, capture, the Meta demo option.
7. **Chapter 3 — the instrument** (first update): resize, then rotate.

## Now

Active work.

- [Two-ship test](in-progress/two-ship-test.md) — bake a two-ship level with the sun kept out of ship
  runs and play it before deciding on a two-ship job 8 for v1.
- [The field meter](in-progress/field-meter.md) — entropy on the grid's base edge (in); next prompts that
  teach it, the same square under the map for the chapter's stars, then one panel bank.

## Next

Nothing queued.

## Later

Design-only or deferred. Nothing here is scheduled.

- [Chapter 2 — the fleet](backlog/chapter-2-fleet.md) — job 8 (Mixed) closes chapter 1;
  chapter 2 (five jobs) brings in multi-ship. Post-v1.
- The contract card stays available during the run as a static reference; otherwise
  help reaches the player at the start (Jim's line, the gesture prompts) and the end (the complete/fail
  message) — no reactive in-solve hint surface.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish, playable map field.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **Gesture prompt icons — headset read.** Scale, Enter, Move and Exit (new 2026-11-09, shown from
  job 2) now play 12-frame line-art loops rendered from Meta's own hand mesh by
  `Tools/Our Space/Render Gesture Prompt Art` (`GesturePrompts/Editor`); re-run it after tuning a motion.
  Still open: read them in the headset (size, speed, legibility at arm's length).
- **Run authoring.** Jobs 1–7 are baked into Chapter 01 (seven jobs); job 8 is the deferred Mixed
  closer, step 1 of the road to the store. The job card landed (2026-11-07). Job 1 was rebaked (2026-11-06) with
  six planets and a sun the cube does not cover (seed 141) — a sunless four-planet field almost never
  bakes. `PuzzleJobBakeTests` and a headset read of job 1 are still open.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. Fold it into the in-puzzle read
  plan or the feel pass.
- **Resize.** Working, but the explanation (size is scope, not precision) is not ready. Deferred from the
  slice; chapter 3's first lesson ([chapters](../design/chapters.md)).
- **Capstone routes (job 8).** The capstone is deferred to its own future plan; it still needs at least
  two routes and a Mixed/InZone field that bakes. Planned in
  [chapter 2](backlog/chapter-2-fleet.md) U1; not scheduled.

## Conventions

- Every plan starts with a status line:
  `**Status:** <state> · <area> · updated <yyyy-mm-dd> — <one-line summary>`
  where `<state>` is one of `now`, `next`, `later`.
- `<area>` is one of `space shaping`, `universe`, `presentation`, `flow`.
- Folders: `now` → `in-progress/`; `next` and `later` → `backlog/`. Move the file and update this
  board in the same change.
- **Done plans are removed.** When a plan lands, add its post-mortem to [lessons](../lessons.md),
  then delete its file and its board row; the code and tests are the record. Do not archive it in a
  `done/` folder.
- Run `docs/check.ps1` after moving files — it runs `check-plans.ps1` (links, status/folder, board) and
  `check-docs.ps1` (design docs).
