# Plans — the board

What is being built: **Now → Next → Later**. Each plan's `**Status:**` line matches its row here.
Store release: €4, 6 Dec, 21 story jobs + 10 freelance at launch (10 more in the first update); scope
and schedule in [release](../design/release.md). Post-mortems in [lessons](../lessons.md).

## Now

- **27 Nov:** [chapter 2 playtest fixes](in-progress/chapter-2-playtest.md) — code, lines and docs in;
  the Interchange prefab wiring, the 02_01 clip and the test runs remain
- **27 Nov:** [chapter 2 rebakes](in-progress/chapter-2-rebakes.md) — 02_09 as a bend, 02_06
  solvable; starting with the answer backfill and the 02_09 replay
- **27 Nov, today:** [construct core](in-progress/bake-construct-core.md) — U1–U2 in: the plain
  skeleton and the whole gate; the Clear/Curve recipes are the single plan
- **27 Nov:** [construct wire](in-progress/bake-construct-wire.md) — U1 seam in (generator, retries,
  fleet unsupported, single measurement); miss status and the timing comparison are open
- **27 Nov:** [construct separate](in-progress/bake-construct-separate.md) — U1 in (fleet plumbing,
  scratch fixtures, 7 fast tests); the Clear/Curve spikes and rates are next in the bake checkout
- **27 Nov:** [construct jobs](in-progress/bake-construct-jobs.md) — U1 in and merged into `bake`; U2
  proved negative (fleet Curve 0/20), so 02_06/02_09 stay search-baked and 02_09C waits on fan-out
- **27 Nov:** [construct fan-out](in-progress/bake-construct-fanout.md) — code in: the picker, aim,
  lesson and 02_04C/02_09C fixtures; the fast run awaits the Test Runner (MCP bridge dropped), and the
  spike and 20-seed rates are next in the bake checkout

## Next

- **27 Nov:** build the answer first, six plans: core (in progress) →
  [single](in-progress/bake-construct-single.md) (in progress) → [wire](in-progress/bake-construct-wire.md) (in progress) →
  [separate](in-progress/bake-construct-separate.md) (in progress) →
  [fan-out](in-progress/bake-construct-fanout.md) (in progress)
- **Jobs:** [construct jobs](in-progress/bake-construct-jobs.md) U2 proved the constructor on the
  scratch copies; the fleet recipes' yield is the separate plan's next pass
- **After the bake overhaul:** [chapter 2 rebakes](in-progress/chapter-2-rebakes.md) — moved to Now;
  the answer backfill runs first
- **27 Nov–1 Dec, alongside chapter 2:** [broken bodies](backlog/chapter-3-broken-bodies.md) (anchors, then
  repulsors; gate 1 Dec)
- **Before 1 Dec:** [map arc](backlog/chapter-3-map-arc.md) — chapter systems on an arc around the seat, so every
  job stays in reach once chapter 3 adds the third
- **1–3 Dec:** [chapter 3](backlog/chapter-3-jobs.md) (the jobs, lines and the ending)
- **3 Dec:** [freelance track](backlog/freelance-track.md) — the post-story map system, hidden until the
  ending
- **27 Nov–5 Dec:** [freelance jobs](backlog/freelance-jobs.md) — 10 contracts at launch, 10 in the first
  update; rows and bakes start once chapter 2's bakes free a checkout
- **5 Dec:** the entitlement check (`MetaPlatformClient` is init only), timing pass, build, upload

## Later

- Ideas (not plans): [docs/ideas](../ideas/README.md).

## Archive

Plans parked for after the release, kept so they are not lost (`archive/`).

- [Boot on one line](archive/boot-one-line.md) — the console boot on one text, not one object per line.

## Open tails

One line each: what is left and where it lands.

- **Field wells** (on in `Puzzle.prefab`): describe them in [the cube](../design/systems/cube.md)
  §Feedback; decide whether `LatticeWells` keeps its toggle; `LatticeMeshBuilder.SamplesPerLine` back
  from 48 to 32.
- **Endpoint rings** (one small ring under every endpoint): describe them in
  [the cube](../design/systems/cube.md) §Feedback; delete the unused `OpenRing.asset` (nothing
  references it).
- **Job 1 and 02_06 on the headset:** job 1 was rebaked 2026-11-06 (seed 141) and never read on the
  headset. 02_06 (the three-ship job, formerly 02_04) is the only chapter 2 job with no contract, so no check covers it, and the shipped
  re-solve found no zone that rescues all three ships under today's physics; the headset pass
  (2026-11-27) confirmed it cannot be solved. Its rebake is
  [chapter 2 rebakes](in-progress/chapter-2-rebakes.md) U3.
- **Parallel scan timing** (bake parallel closed 2026-11-27, [lessons](../lessons.md)): time ~24 seeds
  of a two-ship job with `-Workers 1` and the default in the bake checkout, compare both reports, and
  note the speed-up in AGENTS' bake section.
- **Chapter 2 headset pass** (plans closed 2026-11-27, [lessons](../lessons.md)): every edge sun level
  with its planets and none an answer; the map cube over each system's centre with no jump on entry;
  the intros' `MaxScaleDelta = 0.5` (tighten to ~0.3 and rescan if they play hard); time the capstone
  (~3–4 min). Optional: count blockers on any ship's run for `Any` fleets and put 02_04/02_05 back to
  `MinBlockers = 1` if the shipped fields pass.
- **Story arc and job card on the headset** (closed 2026-11-26,
  [note](../solutions/narrative/story-arc.md)): new cards settle under 3 s, versions follow play order,
  the regenerated clips play.
- **Chapter 2 clips:** one is missing, `level_02_02_init_02`; regenerate it with the `Downloader - OS`
  batch (the other 24 new clips are in, renamed with the ids on 2026-11-26).
- **Chapter 2 manual entries:** 02_04's repeats 02_01's "bend near the start" (02_04 is the movable
  fan-out: teach finding the shared start), and 02_03's ("Two ships, one budget…") still introduces
  fleets, which 02_01 now does.
- **The sheet mirror:** push every `Levels_en` and `Global_en` change since 2026-11-25 to the line
  spreadsheet before the next sync, or the sync overwrites them: the story-arc lines, the two fuel lines
  and job 1's manual entry, chapter 2's cards and lines, and the wire (two lines dropped since 2026-11-25, 22 keys).
- **Jim's repeats (optional):** cut intro lines that say what the card shows; each cut deletes a
  clip and its downloader key. Keep 2-6's `init_02` (which ship carries what).

## Conventions

- Status line: `**Status:** <now|next|later|archived> · <space shaping|universe|presentation|flow> · updated
  <yyyy-mm-dd> — <summary>`.
- Names: plans that serve one goal share a prefix, `<group>-<part>.md` ([AGENTS](../../AGENTS.md#plans-are-a-morning)).
- Folders: `now` → `in-progress/`; `next`/`later` → `backlog/`; `archived` → `archive/` (parked for
  later, not closed). **Only the user closes a plan.**
  When they say to close it, follow [AGENTS](../../AGENTS.md) (post-mortem in lessons, then delete).
  Run `docs/check.ps1` after moving files.
after moving files.
