# Chapter 2 lines

**Status:** next · universe · updated 2026-11-26 — cards, Jim's lines, clips and the sector wire for
chapter 2's six new jobs.

**Related:** [plans board](../README.md) · [story arc](../../solutions/narrative/story-arc.md) ·
[jobs](../../design/jobs.md) · [lore](../../design/world/lore.md#episodes)

## Goal

Every new chapter 2 job has its card, Jim's lines and clips, and the sector wire still reads in order
with ten chapter 2 jobs.

## Depends on

The jobs from [chapter 2 intro](chapter-2-intro.md), [fleet Clear](fleet-clear.md) and
[fleet capstone](fleet-capstone.md) (ids in [chapter 2 intro](chapter-2-intro.md) §Ids), and [story arc](../../solutions/narrative/story-arc.md)
(done: the ≤ 90-character manual rule, the card numbered by play order).

## Units

### U1 — cards and lines

- For each new id: client, cargo, a manual entry (one rule + one fine-print line, ≤ ~90 characters),
  Jim's `init_NN` / `complete_NN` / `fail` lines, following the episode-2 turn in lore.
- Write them in the line spreadsheet, then pull into `Levels_en`.
- Exit: every new job's keys resolve; jobs.md lists the new cards.

### U2 — clips

- List every new spoken key (about 30), add them to the `Downloader - OS` batch, generate the mp3s
  into `StreamingAssets`.
- Exit: every spoken key has a clip; no stale clip.

### U3 — the sector wire

- `SectorWire` is keyed by solved count (`LineCount = 24`); six new chapter 2 jobs shift keys 7–17.
  Re-author the middle lines so each still lands in its chapter (chapter 2's lines while chapter 2
  plays, chapter 3's frame after), and update `SectorWireTests`.
- Exit: the wire reads in order across 7 + 10 + chapter 3's jobs; the fast suite is green.

## Manual check (headset)

1. Every new card settles quickly and does not repeat Jim.
2. Every new spoken line plays its clip.
3. The wire's line after each solve fits the chapter being played.
