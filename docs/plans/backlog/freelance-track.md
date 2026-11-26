# Freelance — the track

**Status:** next · flow · updated 2026-11-26 — the post-story track: one map system after the ending,
hidden before it; code and tests only. The map arc is [its own plan](chapter-3-map-arc.md).

**Related:** [plans board](../README.md) · [jobs](freelance-jobs.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md#freelance-contracts-store-release)

## Goal

The map can grow one extra system after the story ends without touching the run: the ending, the
coverage count and the wire stay the story's. Content and bakes live in [freelance jobs](freelance-jobs.md).

## Decisions

- **A post-story track, not chapter 4 of the run.** A new `LevelSelector.postStoryChapters` list with the
  same resolution as `modeChapters`: levels resolve by id, but the run's order, count, unlocks, ending
  and saved job count stay story-only (`LevelSelector.cs:10-12`, `LevelSelector.cs:73-84`). Appending
  the chapter to `chapters` would break the ending: it fires only when every level is solved
  (`RunEndingState.cs:35`, `RunProgress.cs:61`), and the seen flag compares totals, so a bigger run
  re-earns it (`RunProgressCookies.cs:110-133`).
- **The map composes it only after the ending.** The map is story levels plus post-story levels when
  `RunProgressCookies.HasSeenEnding` is true; before that there is no fourth system at all — no nodes,
  ring, title or card (`RunMap.cs:22-39`, `MapFieldRenderer.cs:86-99`, `MapChapterTitles.cs:28-53`,
  `MapSelectionReadout.cs:18-24`).
- **The whole chapter opens at once.** The map's post-story progress marks every job open; no
  `job_opened_*` cookies and no `unlocksAsGroup` (that flag only gates the levels panel,
  `LevelSelector.cs:35-46`; the map reads `RunProgress.IsOpened`, `RunJobs.cs:26-34`).
- **The focus marker is the chapter's first unsolved job**, not the highest current (`PuzzleMap.cs:43-59`),
  so the pulse does not sit on the last contract.
- **Story progress is untouched.** `RunEndingState`, `HasSeenEnding`, the sector readout and the wire
  keep the story build; only the map uses the composed one (`PuzzleSelectionState.cs:79-147`). Every
  completion still enters `RunEndingState`, which returns straight to the map after the ending.

## Units

### U1 — the selector track

- Add `postStoryChapters` to `LevelSelector`, resolved by id in `GetChapter`/`GetLevelData`, excluded
  from `TotalLevelsCount`, the progression order and the levels panel. No new flag on `ChapterData`
  (`ChapterData.cs:10-13`).
- Tests: the total ignores a post-story chapter; `GetChapter` and `GetLevelOrdinal` resolve its ids.

### U2 — the map composes it

- Build the map's levels as story + post-story when the ending is seen, and a map-only `RunProgress`
  over them: solved and stars from the level cookies, every post-story job open, current = its first
  unsolved. Leave `RunProgressCookies.Build` (the run) story-only.
- `PuzzleSelectionState` uses the composed progress for the map and the story progress for the bulletin,
  coverage and freshness (`PuzzleSelectionState.cs:79-147`).
- Exit: EditMode test — with a post-story fixture chapter, the map has no such jobs before
  `HasSeenEnding` and all of them current after.

### U3 — the focus and the seam

- `PuzzleMap`: when the group is open, the pulse lands on its first unsolved job; ordinary story
  signposts keep the current rule (`PuzzleMap.cs:43-59`).
- Exit: EditMode tests — the ending still fires with an unsolved post-story chapter; a solved
  post-story job records and reloads its stars; the story's coverage count is unchanged; the pulse
  test.

## Checks

1. EditMode suite green with no content assets (fixtures only).
2. The ending plays once at the story's last job, post-story chapter present and unsolved.
3. On a headset build with the real chapter, no freelance system, stars or titles appear before it.
