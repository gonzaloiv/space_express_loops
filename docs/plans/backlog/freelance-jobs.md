# Freelance — the jobs

**Status:** next · space shaping · updated 2026-11-27 — 10 contracts at launch, the other 10 in the first
update; baked and played on the headset. Follows the bake overhaul: one checkout on every core, a bake
writes its job and answer, the chapter order is set by hand.

**Bakes:** off the editor — U3 and U5 (a seed scan and a bake per slot, ~2 attempts each)
([AGENTS](../../../AGENTS.md#bakes-run-off-the-editor), `docs/tools/bake.ps1`).

**Related:** [plans board](../README.md) · [track](freelance-track.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#freelance-contracts-store-release)

## Goal

Ten contracts on the post-story system at launch, the other ten in the first update: 1–3 stars, one
client and cargo line, no Jim. The story's epilogue, in two batches.

## Decisions

- **Launch scope, decided now.** Ten contracts (slots `90_01`–`90_10`, the easy half) ship at launch;
  slots `90_11`–`90_20` (the hard half) ship in the first update. This is the fallback taken now, not a
  risk left in the plan. The update appends the ten to the same chapter, so no code changes.
- **Its own chapter id: `90`, not `04`.** Saves, cookies and the card key off the job id
  (`PuzzleJobContracts.ChapterOf`, `PuzzleData.GetChapterId`, which must parse as a number); `04_xx`
  would collide with a later story chapter 4 ([later chapters](../../ideas/later-chapters.md)) in
  players' saves.
- **Sun placement:** single-ship contracts keep the centred sun with `NoSunAnswer`; fleet contracts use
  `OffCorridor` ([chapters](../../design/chapters.md)), since a centred sun cannot meet
  `NoSunAnswer` in a fleet.
- **The bakes do not wait for the track.** Contract rows and bakes need no map code: commit the rows
  (U1), merge them into the bake checkout and start U3; the [track](freelance-track.md) is built
  alongside. One checkout is enough: a fleet scan runs on every core (`bake.ps1 -Workers`, default
  cores − 1). A fleet seed takes ~43 s of CPU with two ships and ~2.2 min with three; 200
  three-ship seeds take ~15 min on 31 workers ([scan speed](../../solutions/bake/seed-scan-timings.md)).
- **It lives on the post-story track** ([track](freelance-track.md)): hidden until the ending is seen,
  then fully open. Coverage, the wire and the ending stay the story's.
- **Order is the chapter asset, not the ids.** Slots are baked in order anyway, so `90_01`…`90_10` read
  in play order; the card shows `JOB 90-01` (`JobCardPanel.cs:98`).
- **Taught mechanics only:** Curve, Clear, single-ship two-zone, 2–3 ship fleets; broken bodies only if
  chapter 3 ships them. No multi-zone fleets (dropped, `release.md:45-49`).
- **No Jim, no manual, no clips.** No `chapter_90_init` (opener skipped, `PuzzleStartState.cs:59-64`);
  every contract sets `noIntro` (`PuzzleEditionState.cs:94-104`); no manual entry (the card hides it,
  `JobCardPanel.cs:95-104`); no complete/fail cues (the return is immediate, `TtsPresenter.cs:99-115`).
- **A bake writes the job, not the chapter.** A bake saves `Chapters/90/Job_90_xx_Puzzle.asset` and
  its answer at `PuzzleCreator/Answers/90_xx.asset`; it never touches `Chapters/90.asset`, whose
  order is set by hand. `PuzzleJobChapterTests` fails while an enabled slot is missing from it.
- **`Pending` until baked, flipped in the bake commit.** Every slot starts `Pending`, so the shipped
  check and the chapter check skip it. Clear `Pending` before `-Mode bake`, in the same commit as the
  asset, answer and pin.
- **Every slot uses the search.** One path: scan the slot's seeds, pin a passing seed, bake. There is
  no generator to choose between.

## Units

### U1 — the chapter and the slots

- `Chapters/90.asset`, empty, wired into `LevelSelector.postStoryChapters`; `chapter_90_title` for the
  map (wording with U2). Slots join it by hand as they bake (U3).
- Twenty `PuzzleJobContract` rows `90_01`…`90_20`, all `Pending` (an enabled row with no asset fails
  the shipped and chapter checks); each with its mechanic, planet count and difficulty target.
- Exit: the rows compile and the fast and Creator suites stay green with all twenty pending. (Hidden
  before the ending, 0/10 after it, is checked once the [track](freelance-track.md) lands.)

### U2 — lines

- Ten client and cargo pairs for the launch slots (the other ten with U5), in the release's voice; the
  map title. No intro lines, no manual entries, no complete/fail clips.
- Exit: every launch card reads; the keys exist before the bakes.

### U3 — bake and play the ten launch slots (off the editor)

- Per slot: `bake.ps1 -Job 90_xx -Mode scan -Seeds <n>`, pin the passing seed, clear `Pending`, then
  `-Mode bake`. Commit the job asset,
  its `.meta`, its answer and the contract pin together, then add the slot to `Chapters/90.asset` by
  hand. Play it on the headset; if it is not fun or fair, change the seed or field and bake the same
  slot again (~2 attempts a slot).
- The bake checkout needs the slot rows committed and merged first (AGENTS bake rule); fleet slots are
  the slow ones, so start their scans first.
- Exit: ten contracts that play well, `Chapters/90.asset` in play order, `ShippedJobs_MeetTheContract`
  and `PuzzleJobChapterTests` green in the bake checkout.

### U4 — the launch headset pass

- The manual check below, then the timing pass; the build and upload follow the release plan.

### U5 — the first update's ten

- Bake and play `90_11`…`90_20` the same way (clear `Pending` in each bake commit, add each to
  `Chapters/90.asset` by hand); write their ten cards. No code, no layout change: the same system grows to twenty.
- Exit: the update ships twenty contracts total; a save that played the first ten keeps its stars.

## Manual check (headset)

1. Before the ending, no fourth system, no freelance stars, coverage still reads the story's count.
2. Solve the story's last job: the ending plays once, as today.
3. On the next map, the freelance system appears with all ten open and the first contract selected.
4. No Jim line or card on entry; the map card shows client and cargo only.
5. Solving a contract returns to the map, stamps stars, and the tally survives a restart.
6. A failed contract restarts in place; no clip delays the return.
7. Coverage and the wire stay story-only after a freelance solve.
8. Time five contracts; ~1.5 min each is the target.

## Open

- **Map label.** The other titles read "EPISODE n · TITLE"; freelance is not an episode. Settle the
  wording in U2 (e.g. "THE BOARD · OPEN CONTRACTS").

