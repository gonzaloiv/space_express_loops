# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube): scale the space, scale the content;
the ship flies the deformed spacetime.

## Road to v1

The ordered actions to a first "final" version (decided 2026-11-06). Each step becomes a plan (a
morning) when it starts; a step that is already a plan links to it. Steps marked *(proposed)* still
need a yes.

1. **Tune the run on device.** Per-job scramble and planet count, and the 3★ margin from the cold
   playtest (the margin landed at 1.5×, judge run closed 2026-11-11). Job 6's lesson is read from the
   docs, not a bake check. The stuck-player skip landed (2026-11-11): one fail opens the next job;
   the playtest may still revise it.
2. **The solve moment** — done (2026-11-09). Arrival payoff at the destination (flash + sound), then the stars stamped in
   (terminal style), then Jim's receipt — what makes the player want the next job.
3. **The ending** — done (2026-11-11). After the run completes (every job solved, whatever the count):
   the whole map lit, the total stars, the "OUR SPACE" title, Jim closing the episode, more contracts
   in the making, an invitation to replay for 3★.
   - **The fleet episode** — [plan](in-progress/fleet-episode.md) — the two- and three-ship jobs join v1
     as chapter 2, episode 2 of the story (decided 2026-11-11 after the two-ship headset test); lines
     approved.
4. **Minimum sound** — done (2026-11-09); the soundscape built on it (done 2026-11-10). Pinch grab
   tick, stretch tone, release, arrive chime, crash, fail sting. No haptics: the game is hands-only.
5. **The first 30 seconds** — done (2026-11-10). App open → the console boot → the field placed at a
   comfortable seated reach → the Scale prompt. Pause and offline (the judge tests) also done 2026-11-10.
6. **Quest 3S hitches at 72 fps** *(proposed)*. Sudden frame drops, not sustained low fps: map
   dioramas, trajectory previews, lattice.
7. **Voice and content** *(proposed)*. Job 6's lines for the new lesson. Jim's complete and fail lines
   are split into one-sentence TTS entries and his clips are downloaded (done 2026-11-09). The music duck under Jim (deferred from the soundscape) is tuned with his clips. English only: Spanish is kept just to prove the localization system works.
8. **Competition deliverables — internal freeze Friday 2026-11-13; official deadline 2026-11-18,
   12:00 PM PT; work starts Wednesday 2026-11-11.** The entry is frozen at the internal freeze. The angle, hero moment and video outline are in [pitch](../design/pitch.md);
   until Wednesday the time goes to the game.
   - **Build:** APK on a new release channel named "Competition" in the developer dashboard, plus an
     Invite URL for judges; it must stay launchable through judging.
   - **Video:** under 3 minutes, on-device footage, public on YouTube or Vimeo; the best material in
     the first seconds (judges may stop early).
   - **Form:** name, tagline (≤140 characters), track (**Gaming**), division (**New Experience**), a ≤500-word description (inspiration, how it was built, future plans), how hand
     interactions are used, target launch date, team names, emails and roles.
   - **Judge tests to pass:** fully hands-only end to end; the airplane-seat test (two-foot radius);
     the one-bus-stop test (a complete moment fast, clean pause/resume).

**Out of v1:** Mixed (no job 8), resize, the fleet capstone (2-5), reactive in-solve tips, the playable map
field, chapters beyond 2.

## Road to the store

After v1 (decided 2026-11-07): a **~€5 Meta store release** sooner, then chapters as updates. The game
shape is in [chapters](../design/chapters.md); the demo is the store's own option.

1. **Chapter 1 stays seven jobs** (decided 2026-11-11): no job 8; Mixed moves to a later chapter.
2. **Chapter 2 — the fleet.** 2-1 to 2-4 ship in v1 ([fleet episode](in-progress/fleet-episode.md)); the
   store adds the 2-5 capstone ([design](../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release)).
3. **Freelance contracts.** ~150 prebaked jobs, tiers, the quality filter, the exact-entropy score.
4. **The daily contract.** One pool job per day, Meta platform leaderboard.
5. **Price check.** Measure minutes per job on the headset; confirm ~€5 and the pool size.
6. **Store release.** Store page, capture, the Meta demo option.
7. **Chapter 3 — the instrument** (first update): resize, then rotate.

## Now

Active work.

- [Fleet episode](in-progress/fleet-episode.md) — started 2026-11-11: four multi-ship jobs join v1 as
  chapter 2, episode 2; lines approved, job 8 dropped from v1.
- [Hand-tracking gate](in-progress/hand-tracking-gate.md) — started 2026-11-12: the boot holds while
hand tracking is off and waits briefly for both hands; the scene config and headset proof are open.

Closed 2026-11-11: the ending, endpoint shapes, the stuck-player skip and the field-wells spike
([lessons](../lessons.md)).

## Next

Nothing queued.

## Later

Design-only or deferred. Nothing here is scheduled.

- [Chapter 2's capstone](../design/chapters.md#chapter-2--the-fleet-v1-then-the-store-release) — the
  2-5 fleet capstone. Post-v1. Mixed (the old job 8) waits for a later chapter.
- The contract card stays available during the run as a static reference; otherwise
  help reaches the player at the start (Jim's line, the gesture prompts) and the end (the complete/fail
  message) — no reactive in-solve hint surface.

Candidate ideas (not plans) live in [docs/ideas](../ideas/README.md): variations, element pack,
hologram look, TTS companion (deferred), feel polish, playable map field.

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

- **TTS clips — resolved 2026-11-12.** The fail, job 6 intro and ending clips are in
  `Assets/StreamingAssets` (chapter 2 too); the remaining audio tail is the music duck under Jim.
- **Job 7's manual shortening (from run fixes).** Waits on the lore rethink: does the "eleven years"
  note move or stay.
- **Field wells: kept (2026-11-11, headset).** The change-only wells are on in `Puzzle.prefab`
  (committed). Left to do: describe them in [the cube](../design/systems/cube.md) §Feedback (the lattice
  starts flat; a grown planet's well opens, a shrunk one flattens) and decide whether `LatticeWells`
  keeps its toggle as a tuning block. The lattice still runs 48 points a line, uncommitted
  (`LatticeMeshBuilder.SamplesPerLine`); with the line smoothing in, it can go back to 32.
- **Endpoint rings (from endpoint shapes).** Second try (2026-11-11): a small ring *under* the planet
  (a `Base` child at -0.7 body radius, planet-sized, not spinning) instead of around it, and the
  endpoint bodies spin like every planet again (`ConstantRotation`). Third pass: one ring for every
  endpoint, smaller (the filled band at ×0.7, 0.21–0.35 of the body radius), so origin and destination
  no longer differ by shape — only the route colour and the line's direction tell them apart, and the
  interchange keeps its markers. `OpenRing.asset` is now unused. Update
  [the cube](../design/systems/cube.md) §Feedback ("filled/open ring around the planet") once settled.
- **The ending on the headset.** The exit cue and the lit-map cue start almost together; keep one if
  they clash.
- **Route end fallback.** `MapRouteLine.EndIndex` returns `-1` when no job is `Current` or solved
  (unreachable today); make it safe if that can ever happen.
- **Gesture prompt icons — headset read.** Scale, Enter, Move, Exit, Spend and Stars (the last two new
  2026-11-10, with the field meter) play 12-frame line-art loops rendered from Meta's own hand mesh by
  `Tools/Our Space/Render Gesture Prompt Art` (`GesturePrompts/Editor`); re-run it after tuning a motion.
  Still open: read them in the headset (size, speed, legibility at arm's length).
- **Run authoring.** Jobs 1–7 are baked into Chapter 01 (seven jobs, final). The job card landed (2026-11-07). Job 1 was rebaked (2026-11-06) with
  six planets and a sun the cube does not cover (seed 141) — a sunless four-planet field almost never
  bakes. `PuzzleJobBakeTests` and a headset read of job 1 are still open.
- **Ship-zone cue.** The ship has a trail; there is no zone indicator. No plan owns it yet.
- **Resize.** Working, but the explanation (size is scope, not precision) is not ready. Deferred from the
  slice; chapter 3's first lesson ([chapters](../design/chapters.md)).
- **Mixed.** Dropped from chapter 1 (no job 8, [chapters](../design/chapters.md)); a later chapter's
  job. It still needs at least two routes and a Mixed field that bakes; not scheduled.

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
