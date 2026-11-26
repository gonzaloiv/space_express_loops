# Plans — the board

What is being built: **Now → Next → Later**. Each plan's `**Status:**` line matches its row here.
Store release: €4, 6 Dec, ~23 story jobs + 10 freelance at launch (10 more in the first update); scope
and schedule in [release](../design/release.md). Post-mortems in [lessons](../lessons.md).

## Now

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

- [Boot on one line](backlog/boot-one-line.md) — only if there is time.
- Ideas (not plans): [docs/ideas](../ideas/README.md).

## Open tails

One line each: what is left and where it lands.

- **Field wells** (on in `Puzzle.prefab`): describe them in [the cube](../design/systems/cube.md)
  §Feedback; decide whether `LatticeWells` keeps its toggle; `LatticeMeshBuilder.SamplesPerLine` back
  from 48 to 32 (uncommitted).
- **Endpoint rings** (one small ring under every endpoint; `OpenRing.asset` unused): update
  [the cube](../design/systems/cube.md) §Feedback once settled; delete `OpenRing.asset`.
- **Route end fallback:** make `MapRouteLine.EndIndex` safe when it returns `-1` (unreachable today).
- **Gesture prompt icons:** re-run `Tools/Our Space/Render Gesture Prompt Art` after tuning a motion.
- **Job 1 on the headset:** rebaked 2026-11-06 (seed 141), still needs a headset read.
- **Story arc and job card on the headset** (closed 2026-11-26,
  [note](../solutions/narrative/story-arc.md)): new cards settle under 3 s, versions follow play order,
  the three new clips play; then push the edited `Levels_en` lines and the 12 wire lines to the sheet.
- **Fuel clips** (2026-11-26): `level_01_01_complete_04_win` and `level_01_02_complete_03` now say
  the launch is the only fuel; regenerate both mp3s (the old audio still says "No fuel burned") and push
  the two lines and job 1's manual entry to the sheet.
- **Chapter 2 clips and cards** (lines closed 2026-11-26, [note](../solutions/narrative/story-arc.md)):
  run the `Downloader - OS` batch for the 30 `level_02_05`–`02_10` keys already in its list; mirror
  the chapter 2 lines and wire to the sheet; rewrite 02_02's manual (it repeats 02_09's "bend near the
  start") and 02_01's (it still introduces fleets); run `SectorWireTests`.
- **Jim's repeats (optional):** cut intro lines that say what the card shows; each cut deletes a
  clip and its downloader key. Keep 2-4's `init_02` (which ship carries what).

## Conventions

- Status line: `**Status:** <now|next|later> · <space shaping|universe|presentation|flow> · updated
  <yyyy-mm-dd> — <summary>`.
- Names: plans that serve one goal share a prefix, `<group>-<part>.md` ([AGENTS](../../AGENTS.md#plans-are-a-morning)).
- Folders: `now` → `in-progress/`; `next`/`later` → `backlog/`. **Only the user closes a plan.**
  When they say to close it, follow [AGENTS](../../AGENTS.md) (post-mortem in lessons, then delete).
  Run `docs/check.ps1` after moving files.
