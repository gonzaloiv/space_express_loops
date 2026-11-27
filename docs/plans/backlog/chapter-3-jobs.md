# Chapter 3

**Status:** next · space shaping · updated 2026-11-26 — chapter 3's ~6 jobs on broken bodies, its
episode, lines and the release's ending. The mechanics are [broken bodies](chapter-3-broken-bodies.md).

**Bakes:** off the editor — U2 (the jobs) ([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#chapter-3--broken-bodies-store-release) ·
[lore](../../design/world/lore.md#episodes)

## Goal

A playable chapter 3 that uses anchored planets and repulsors and closes the story: ~6 jobs, the
episode opener, Jim's lines and the ending.

## Depends on

- [Broken bodies](chapter-3-broken-bodies.md): both mechanics, or anchors only if the 1 Dec gate fails.
- The [story arc](../../solutions/narrative/story-arc.md): what breaks, the episode and its clients, the
  ≤ 90-character manual rule, the card numbered by play order.

## Units

### U1 — the chapter

- `Chapters/03.asset` and its folder, and a `03` entry in `LevelSelector.asset`'s chapters (the
  selector is the only reader that discovers a chapter; the map's third system follows from the
  `03_xx` levels listed in the chapter, not from the chapter entry). `unlocksAsGroup` off, like
  chapter 2. The title and release keys derive from the chapter id
  (`chapter_03_title`, `chapter_03_manual_release`) — author them as localization values only
  (*EPISODE 3 · THE DAMAGE* / *DAMAGE CONTROL*); the client and cargo rows come from
  [chapters](../../design/chapters.md) §Chapter 3.
- Exit: `03.asset` loads and sits after chapter 2 in the selector. The third map system appears once
  U2 lists the first `03_xx` puzzle in `03.asset`'s `levels`: `RunMap.Build` groups the levels it
  reads through the selector, so the chapter entry alone adds no system.

### U2 — the jobs

- Ramp: an anchor on the line → an anchor among decoys → a repulsor to shrink → a repulsor to grow → a
  repulsor and an anchor → a capstone (both, with a fleet). If the 1 Dec gate failed: 3-1 and 3-2 as
  written; 3-3 and 3-4 become anchor jobs; 3-5 and 3-6 use more anchors per job
  ([chapters](../../design/chapters.md) §Chapter 3).
- **Contracts.** Add all six to `PuzzleJobContracts.All` as `Pending = true`, then clear it once each
  is baked and wired. Each pins the knobs the bake and validator read: `Anchors` and `Pushes`
  ([broken bodies](chapter-3-broken-bodies.md)), `PlanetCount`, `FieldShape`, `Mechanism`, `Zones`,
  `Ships`, `Relation`, `Translatable`, `MinRoutes`, `MinBlockers`, `HasSun`, `SunPlacement`,
  `SunOnChord`, `SunRule`, the `No*` flags and the scramble bounds (`MaxScaleDelta`, `MinScaleDelta`,
  `MinStartDistance`, which encode the design table's low/medium/high column). The six rows are in
  [jobs](../../design/jobs.md#the-run--the-mechanics); tune planets, zones and role from the scans. A
  repulsor contract's answer must cover the pushed body
  ([broken bodies](chapter-3-broken-bodies.md)), so a seed whose cheapest role is a normal planet is
  rejected. A fleet cannot request `Mixed` (`PuzzleBrief.SupportsFleetRole`), so the capstone asks
  `Any`.
- **Scan, pin, then bake.** `-Mode bake` bakes from the contract's `Seed`, not from a `-Mode scan`
  (`PuzzleBatchBake`, `PuzzleTestSetBuilder.FindJob`); an `Attempts` window above one scans forward
  from that seed and stops on the first pass. Scan the seeds, pin a passing one with `Attempts = 1`
  (a window above one can bake `Seed + attempt`, not the seed you pinned), then bake. A bake writes
  `Chapters/03/Job_03_xx_Puzzle.asset` and its answer only.
- **The chapter order is hand-wired.** No tool appends to a chapter's `levels`
  (`PuzzleJobChapterTests`: "Bakes no longer rewrite `Chapters/<nn>.asset`"). List the six puzzles in
  `03.asset`'s `levels` in ramp order yourself;
  `PuzzleJobChapterTests.EnabledContracts_AreListedInTheirChapter` fails until you do.
- **Sun placement is picked per job at bake** (decided 2026-11-27): neither the centred look nor the
  edge is fixed for the chapter; each contract pins `HasSun = true`, `SunPlacement`, `SunOnChord` and
  `SunRule` from the seed it bakes. A centred sun cannot meet `NoSunAnswer` in a fleet, so the capstone's
  scan decides.
- **Test cost.** `ShippedJobs_MeetTheContract` is not `[Explicit]`, but it replays each enabled job's
  stored answer and does not search (`PuzzleAnswerReplay`), so chapter 3 adds no bake time there. It
  does fail until every enabled contract has its asset and answer, which is what `Pending = true`
  holds back. The search test, `JobSet_MeetsTheContract`, is already `[Explicit]`, and
  `PuzzleJobChecks` names both. Do not move `ShippedJobs` behind `[Explicit]`: it is the only check
  that a shipped answer still lands, and replay is cheap.
- Exit: the job assets validate, sit in `Chapters/03` and are listed in `03.asset` in play order
  (extend the play-order check, which covers chapter 2 only, to chapter 3).

### U3 — lines, manual and the ending

- Jim's opener: entries `chapter_03_init_01…`, played through the fragment `chapter_03_init` (not
  `_init_01`; `PuzzleStartState` gates on `HasLines`), per-job lines, the chapter bulletins
  (`chapter_03_bulletin_01..03`), and the manual
  entries from [jobs](../../design/jobs.md#chapter-3--broken-bodies) §Chapter 3 (one rule plus one
  fine-print line, ≤ 90 characters each; the final entry carries the "eleven years" note). That table
  is the source of truth — edit it first, then the sheet.
- **The ending moves by itself.** `RunEndingState` fires on `RunProgress.IsComplete` and re-arms on
  `progress.Count`, so it plays after chapter 3 once the chapter exists. The work is the content:
  `RunEndingView`'s `jimKeys` (`run_ending_jim_01..03` fragments, whose clips are
  `run_ending_jim_01_laughing`, `run_ending_jim_02`, `run_ending_jim_03_win`) and `consoleKey`
  (`run_ending_console`, typed on screen — put the roadmap slogan *"Today the space. Tomorrow the
  time."* at its end). Regenerate all three mp3s after the rewrite; a key-stable text edit leaves the
  old clip.
- **The exit cue and the lit-map cue** start almost together: keep the lit-map cue and drop the exit
  cue on the run's last job. The exit cue plays in `PuzzleCompleteState.ReturnToSelection`; read the
  solved puzzle there with `memoryDataClient.Get<PuzzleData>()` (it is a local in `Enter`, no field
  needed) and skip the cue when `levelSelector.IsLast(puzzle)` and the ending will play
  (`!RunProgressCookies.HasSeenEnding(playerData, progress.Count)`), so a replay of the last job is
  not silent. Decide on the headset if the order reads wrong.
- `SectorWire.LineCount = 22` and `sector_wire_16..21` are chapter 3's six lines (02_08 parked
  shifted the tail down from 23); confirm the seams still match the final counts. When `03.asset`
  lands, drop `SectorWireTests.PlannedChapterThreeJobs` and read `ChapterJobs("03")`.
- Write the lines in the sheet, then pull. List every spoken key (opener, per-job, ending) and
  generate its clip; a key-stable text edit later needs the clip regenerated by hand.
- Exit: the chapter plays end to end, ending included. Freelance is [its own
  plan](freelance-jobs.md); the
  chapter does not gate on it.

## Manual check (headset)

1. Chapter 3 opens after chapter 2's last job, with its episode opener.
2. The anchor jobs are solved through the neighbours, not the anchor.
3. The repulsor jobs (if in) read before the first launch.
4. The ending plays after chapter 3, not after chapter 2; only the lit-map cue plays at the end.
5. Every new spoken line plays its clip.
6. Replaying the last job after the ending returns to the map with its exit cue, not silently.
7. Time each job; the target is ~3 min.
