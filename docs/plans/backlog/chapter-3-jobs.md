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

- `Chapters/03.asset` and its folder, and a `03` entry in `LevelSelector.asset`'s chapters (the map's
  third system follows from chapter sizes, but the chapter is not discovered without the selector
  entry). `unlocksAsGroup` off, like chapter 2. The title and release keys derive from the chapter id
  (`chapter_03_title`, `chapter_03_manual_release`) — author them as localization values only
  (*EPISODE 3 · THE DAMAGE* / *DAMAGE CONTROL*); the client and cargo rows come from
  [chapters](../../design/chapters.md) §Chapter 3.
- Exit: `03.asset` loads and sits after chapter 2 in the selector; the third map system appears once
  U2 adds the first `03_xx` level (systems are built from chapter sizes, not the selector).

### U2 — the jobs

- Ramp: an anchor on the line → an anchor among decoys → a repulsor to shrink → a repulsor to grow → a
  repulsor and an anchor → a capstone (both, with a fleet). If the 1 Dec gate failed: 3-1 and 3-2 as
  written; 3-3 and 3-4 become anchor jobs; 3-5 and 3-6 use more anchors per job
  ([chapters](../../design/chapters.md) §Chapter 3).
- Bake in ramp order: `PuzzleTestSetBuilder` appends a new id to the chapter's level list, so bake 3-1
  through 3-6 in that order (rebaking an existing id keeps its slot).
- Contracts in `PuzzleJobContracts.All` with pinned seeds, like chapter 2's plans.
- **Sun placement per job** ([edge sun](../in-progress/edge-sun.md)): single-ship jobs keep the centred
  sun with `NoSunAnswer` (as chapter 1); the fleet capstone needs `OffCorridor`, because a centred sun
  cannot meet `NoSunAnswer` in a fleet. Every contract also sets `HasSun = true`.
- **Test cost.** `ShippedJobs_MeetTheContract` is not `[Explicit]` and re-solves every *enabled*
  contract, so chapter 3 adds six full searches in the bake checkout on top of chapter 2's nine (02_06–02_07
  are baked; 02_08 ships only if a seed meets two shared blockers). If the Creator suite passes ~2 min, move it behind
  `[Explicit]`; `PuzzleJobChecks` names the test explicitly, so the Job Baker still runs it.
- Exit: the job assets validate and are in `Chapters/03` in play order.

### U3 — lines, manual and the ending

- Jim's opener `chapter_03_init` (a fragment, not `_init_01`; `PuzzleStartState` gates on
  `HasLines`), per-job lines, the chapter bulletins (`chapter_03_bulletin_01..03`), and the manual
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
  cue on the run's last job. The exit cue plays in `PuzzleCompleteState.ReturnToSelection`; keep the
  solved puzzle in a field (it is a local in `Enter`) and skip the cue when
  `levelSelector.IsLast(puzzle)` and the ending will play
  (`!RunProgressCookies.HasSeenEnding(playerData, progress.Count)`), so a replay of the last job is
  not silent. Decide on the headset if the order reads wrong.
- `SectorWire.LineCount = 24` and `sector_wire_18..23` already assume six chapter 3 jobs; confirm the
  seams still match the final counts.
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
