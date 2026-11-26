# Plans — the board

What is being built: **Now → Next → Later**. Each plan's `**Status:**` line matches its row here.
Store release: €4, 6 Dec, 22 story jobs + 10 freelance at launch (10 more in the first update); scope
and schedule in [release](../design/release.md). Post-mortems in [lessons](../lessons.md).

## Now

Chapter 2 was renumbered to play order on 2026-11-26 (mapping in
[chapter 2 bakes](in-progress/chapter-2-bakes.md)); the Now plans' notes use the old ids.

- **26 Nov:** [edge sun](in-progress/edge-sun.md) (built; Unity tests, commit and merge into both bake
  checkouts left) — before any further chapter 2 scan
- **26–28 Nov:** [chapter 2 bakes](in-progress/chapter-2-bakes.md) — every chapter 2 scan and bake, one
  list, after edge sun U1
- **25–27 Nov:** [fleet rebake](in-progress/fleet-rebake.md) (contracts in; bakes in chapter 2 bakes; headset pass left)
- **25–30 Nov:** chapter 2 to ten jobs: [intro](in-progress/chapter-2-intro.md) (code in) →
  [fleet Clear](in-progress/fleet-clear.md) (U1–U2 in) · [capstone](in-progress/fleet-capstone.md)
  (contract in); their bakes are in chapter 2 bakes

## Next

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
  re-solve found no zone that rescues all three ships under today's physics: if it cannot be solved,
  rebake it with a contract (edge sun) or drop it.
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
  and job 1's manual entry, chapter 2's cards and lines, and the wire (one line dropped, 23 keys).
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
