# Freelance — the jobs

**Status:** next · space shaping · updated 2026-11-26 — 10 contracts at launch, the other 10 in the first
update; baked and played on the headset.

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
  `OffCorridor` ([edge sun](../in-progress/edge-sun.md)), since a centred sun cannot meet
  `NoSunAnswer` in a fleet.
- **The bakes do not wait for the track.** Contract rows and bakes need no map code: commit the rows
  (U1), merge them into a free bake checkout and start U3 as soon as chapter 2's bakes free one; the
  [track](freelance-track.md) is built alongside. A one-seed fleet scan takes about 2 minutes.
- **It lives on the post-story track** ([track](freelance-track.md)): hidden until the ending is seen,
  then fully open. Coverage, the wire and the ending stay the story's.
- **Order is the chapter asset, not the ids.** Slots are baked in order anyway, so `90_01`…`90_10` read
  in play order; the card shows `JOB 90-01` (`JobCardPanel.cs:98`).
- **Taught mechanics only:** Curve, Clear, single-ship two-zone, 2–3 ship fleets; broken bodies only if
  chapter 3 ships them. No multi-zone fleets (dropped, `release.md:45-49`).
- **No Jim, no manual, no clips.** No `chapter_90_init` (opener skipped, `PuzzleStartState.cs:59-64`);
  every contract sets `noIntro` (`PuzzleEditionState.cs:94-104`); no manual entry (the card hides it,
  `JobCardPanel.cs:95-104`); no complete/fail cues (the return is immediate, `TtsPresenter.cs:99-115`).
- **The bake owns the wiring.** `Chapters/90` is the slot folder, a bake auto-appends by id, and
  `Pending` keeps unbaked slots out of `ShippedJobs_MeetTheContract`
  (`PuzzleTestSetBuilder.cs:115-143`, `PuzzleTestSetBuilder.cs:165-183`, `PuzzleJobBakeTests.cs:103-131`).

## Units

### U1 — the chapter and the slots

- `Chapters/90.asset` with the ten launch jobs in play order, wired into
  `LevelSelector.postStoryChapters`; `chapter_90_title` for the map (wording with U2).
- Twenty `PuzzleJobContract` rows `90_01`…`90_20`: the ten launch slots enabled, the ten update slots
  `Pending`; each with its mechanic, planet count and difficulty target.
- Exit: the rows compile and the chapter asset lists the ten launch slots. (Hidden before the ending,
  0/10 after it, is checked once the [track](freelance-track.md) lands.)

### U2 — lines

- Ten client and cargo pairs for the launch slots (the other ten with U5), in the release's voice; the
  map title. No intro lines, no manual entries, no complete/fail clips.
- Exit: every launch card reads; the keys exist before the bakes.

### U3 — bake and play the ten launch slots (off the editor)

- Per slot: `bake.ps1 -Job 90_xx -Mode scan -Seeds <n>`, pin the passing seed, `-Mode bake`
  (`PuzzleBatchBake.cs:60-70`). Play it on the headset; if it is not fun or fair, change the seed or
  field and bake the same slot again (~2 attempts a slot). Unpin a slot once it ships.
- The bake checkout needs the slot rows committed and merged first (AGENTS bake rule); fleet slots bake
  much more slowly, so bake them first.
- Exit: ten contracts that play well, chapter order settled, the Creator suite green in the bake
  checkout.

### U4 — the launch headset pass

- The manual check below, then the timing pass; the build and upload follow the release plan.

### U5 — the first update's ten

- Bake and play `90_11`…`90_20` the same way, unpin them, and append them to `Chapters/90.asset`; write
  their ten cards. No code, no layout change: the same system grows to twenty.
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
