---
title: Story arc across three chapters — seams, damage, and the solved-count wire
date: 2026-11-25
category: narrative
module: story
problem_type: design_pattern
applies_when:
  - Writing or revising a multi-chapter story arc
  - Inserting jobs into an ordered run whose text is keyed to solved counts
  - Changing a spoken line whose clip is prebuilt by key
  - Shortening the job card or numbering anything the player reads in order
tags:
  - story
  - lore
  - episodes
  - sector-wire
  - tts
  - localization
  - job-card
---

# Story arc across three chapters — seams, damage, and the solved-count wire

## Context

By 2026-11-25 the story was three separate ideas: the Wrap works, the firm gets greedy, then damage —
with nothing pulling the player from one chapter into the next. The arc had to settle what breaks,
give each episode a turn, and revise episodes 1–2's lines without rewriting eleven jobs. It ships as
23 story jobs across chapters 1–3 ([lore](../../design/world/lore.md) §Episodes).

## Guidance

**Put the arc on the seams, not every job.** Chapter 1 has no opener; job 1's receipt is Jim's
introduction. The four beats that carry the arc are job 1's receipt, job 7's close (episode 1 → 2),
chapter 2's opener, and 2-9's close (episode 2 → 3). 2-6's *the field came back wrong* is the
mid-chapter turn once Clear follows it; the rest of the jobs keep their lines.

**The damage is in the bodies.** The Wrap's entropy spend tears local spacetime; the tears show as
anchored planets (the Wrap cannot change them) and repulsors (they push instead of pulling). The Wrap
routes the delivery, it cannot mend a fused body. The anchored body is the guaranteed visible tell;
the repulsor's raised lattice is the second if its gate passes. Whether the player's own Wraps caused
it stays unspoken; Jim denies it.

**Give the manual one register per chapter.** Chapter 1 is warm → corporate-bleak by job 7; chapter 2
is complicit (the fleet, sold as green); chapter 3 is denial and liability (*DAMAGE CONTROL*). Do not
replay cheerful → bleak inside chapter 1. The "eleven years" note moves to chapter 3's final manual
entry; job 7 keeps the welcome and "There is still no office".

**Episode 3 is *the damage*.** Its clients stay absurd; the sector audit is the frame. The six jobs
and their clients/cargo live in [chapters](../../design/chapters.md) §Chapter 3.

**The wire is keyed to solved counts.** `SectorWire.Key(solved)` clamps to `LineCount - 1`; inserting
a job shifts every later line. Keep `SectorWire.LineCount` in step and re-check the tail when the
chapter job counts lock. The wire's 23 keys assume 22 story jobs plus the boot line (02_08 dropped, 2026-11-26).

**Only spoken keys need clips.** Client, cargo, manual, title, bulletin and wire strings are on
screen, not TTS. Changing a spoken line in place keeps its key and leaves the old mp3; list every
changed key and regenerate its clip. Renaming a key needs a new clip and the downloader list; do not
leave a stale mp3.

**Lines live in `Levels_en`; the sheet is a mirror.** If the line spreadsheet is still authoritative,
mirror the strings before the next sync or the sync will overwrite the change. The sheet credentials
are local and gitignored (`Assets/_Game/Editor/Localization/Google Sheets Service.asset`).

**The job card reads at a glance (2026-11-26).** The card sat between every map pick and the job:
6–8 seconds of typing before Jim spoke, and some manual entries repeated him. Now:

- A manual entry is one teaching rule plus one fine-print line, about 90 characters at most, and never
  repeats a line Jim speaks on the same job. New chapters follow the same rule.
- The clock, JOB, CLIENT and CARGO land at once; only the manual types (60 characters a second,
  `JobCard.prefab`).
- The manual's patch number follows the chapter's **play order**, not the job id
  (`LevelSelector.GetLevelOrdinal`, carried into the run by `JobCardStamp`), because new jobs can be
  inserted before older ids. The release name shows only on the chapter's first card. The JOB number on the card
  follows the same play order (`PuzzleData.JobNumberFor`, 2026-11-26): chapter 2 reads 02-01 to 02-09
  and since 2026-11-26 the ids match it (`02_01`…`02_09`): chapter 2 was renumbered to play order
  before release, clips and keys included.

**Chapter 2 lines (2026-11-26).** The wire is owned by chapter seams: chapter 1 keys 01–07, chapter 2
08–16 (nine jobs, the warranty line closes episode 2), chapter 3 17–22; `SectorWireTests`
(`Key_ChapterSeamsFollowThePlayOrder`) pins the seams and `LineCount = 23`. New spoken keys go straight
into the `Downloader - OS` scene's `TableDownloader.keys`, which is the batch's list. A chapter's first
card sets what later manual entries may not repeat; re-read them when the play order changes.

## Evidence

- Arc and turns: [lore](../../design/world/lore.md) §Episodes, §Canon.
- Episode and clients: [chapters](../../design/chapters.md) §Chapter 3,
  [release](../../design/release.md) §Story arc.
- Manual register and job 7 entry: [jobs](../../design/jobs.md) §The manual's arc.
- Terms: [glossary](../../design/glossary.md) (anchor, repulsor, the damage).
- Wire: `Assets/_Game/Features/PuzzleSelection/Domain/SectorWire.cs` (23 keys).
- Lines: `Assets/_Game/Resources/Localization/Levels/Levels_en.asset`;
  wire: `Assets/_Game/Resources/Localization/Global/Global_en.asset`.
- Clips: `Assets/StreamingAssets/level_01_01_complete_03.mp3`,
  `level_01_07_complete_05_laughing.mp3`, `level_02_06_complete_02_confused.mp3`.
- Card: `JobCardPanel`, `JobCardTypewriter`, `JobCardStamp` (PuzzleSelection), `PuzzleData.ManualVersionFor`,
  `LevelSelector.GetLevelOrdinal` (modules `c978074`); the entries table in
  [jobs](../../design/jobs.md) §The manual's arc.

## Verification

- `docs/check.ps1` passes; the plans board and all plan links are clean.
- `DigitalLove.Game.PuzzleSelection.Tests.EditMode` passed 18/18 when the lines and wire changed.
- The three clips were regenerated on 2026-11-25 (valid MPEG headers, new sizes).
- Card: `PuzzleSelection` 19/19 and the fast suite 99/99 EditMode passed (2026-11-26); the editor
  showed `MANUAL v2.0.1 (FLEET EXPANSION)` on chapter 2's first card and `v2.0.2` without the release.
- Closed 2026-11-26 without the headset pass or the sheet mirror; both are open tails on the
  [plans board](../../plans/README.md).
- At close, the in-progress fleet work had two red Creator tests
  (`PuzzleFleetValidationTests`, "no fleet zone"); they are unrelated to this note.
