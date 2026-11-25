# Story arc

**Status:** now · universe · updated 2026-11-25 — one line from episode 1 to episode 3: what breaks,
each episode's turn, and episodes 1–2's lines revised to lead there. The ending is written with
[chapter 3](../backlog/chapter-3.md) U5, once its jobs exist.

**Related:** [plans board](../README.md) · [release](../../design/release.md#story-arc) ·
[lore](../../design/world/lore.md) · [jobs](../../design/jobs.md#the-manuals-arc)

**Closes:** [plans board](../README.md) §Open tails — job 7's "eleven years" note (move or stay).

## Goal

The three episodes read as one story that builds to the realisation that, if something breaks the
universe, there may be no good option left. Chapter 3's lines can be written from this plan.

## Units

### U1 — settle what breaks

- Decide what breaks and the player's part in it, within chapter 3's broken-bodies frame (the scramble
  is spacetime torn; some bodies no longer behave — anchored, repulsor). Decide whether the Wraps mend
  the tear, only route the delivery around it, or both; that answer sets the ending. Keep the cause
  ambiguous: whether the player's own Wraps contributed stays unspoken, like the bleak reading.
- Name the visible tell — the one thing the player sees that Jim cannot explain away
  ([lore](../../design/world/lore.md#episodes) still lists this as open). Use chapter 3's bodies as the
  candidate: the anchored planet is guaranteed (it ships even if the repulsor gate fails); the
  repulsor's raised lattice is the second if the gate passes. If neither is the tell, say what is and
  where it shows.
- Decide job 7's "eleven years" note: it stays at job 7 (and chapter 3 does not repeat the number), or
  it moves to chapter 3 and job 7's manual entry shortens.
- Exit: lore §Episodes' "Still open" sentence and episode-3 row updated; lore §Canon gains one entry
  (what breaks + the visible tell); the "eleven years" decision recorded in lore/jobs.

### U2 — each episode's turn

- For episodes 1 and 2: what changes for Jim and the firm, and the one beat that hands over to the
  next episode. Episode 3's turn ends the release: Jim out of contracts, the tech team promising more,
  the realisation unspoken ([lore](../../design/world/lore.md#episodes)); chapter 3 U5 writes the ending
  from that beat.
- Name chapter 3's episode title, manual release name, and its client/cargo list — the release doc's
  open question, and the chapter keys chapter 3 U1 wires.
- State the manual's register per chapter (warm → corporate-bleak → denial/liability), so chapter 1's
  existing entries are not re-litigated; Jim's pride → doubt carries all three.
- Exit: lore §Episodes is a three-turn table, including episode 3's ending beat and the hand-off to
  freelance; the release doc's open question is closed; [jobs](../../design/jobs.md#the-manuals-arc)
  records the per-chapter register.

### U3 — revise episodes 1–2's lines

- Change only the beats the arc needs: job 1's receipt (Jim's introduction — chapter 1 has no opener and
  job 1 is `noIntro`), job 7's close (episode 1 → 2), chapter 2's opener, 2-4's close (episode 2 → 3),
  the manual fine print that foreshadows the damage (tears; bodies that no longer behave), and the
  chapter 1–2 bulletins. Say which lines stay.
- Decide "Today the space. Tomorrow the time." at job 7's close: keep it as the tech team's roadmap
  slogan (and move it), or replace it with the episode-2 hand-off.
- Decide the sector wire against the new job count. Today `SectorWire.LineCount = 12` holds on the
  chapter-2 line after job 11: either keep one line per solved job (author the chapter 2–3 lines and
  bump the count and test; 24 keys for 23 jobs plus the boot line if the repulsor gate leaves the
  count at 23) or state that it holds and put chapter 3's frame in the bulletins. Do not leave it
  silently holding on the chapter-2 hook.
- Confirm the line source before editing: [jobs](../../design/jobs.md) says `Levels_en` is synced from
  the line spreadsheet. Update the live source, not a copy, or the next sync overwrites the pass.
- List each changed **spoken** key with its line. On-screen-only strings (client, cargo, manual, title,
  bulletin) are not TTS. Add the keys to the `Downloader - OS` batch; rename or delete stale clips when
  an expression suffix changes.
- Exit: lines in `Levels_en` (and `Global_en` plus its shared table for the wire); every changed spoken
  key has a clip in `StreamingAssets`; no stale clip for a removed key; `SectorWire.LineCount`/test
  updated if the wire grew; the downloader key list ready.

## Hand-off

- Chapter 3's opener, per-job lines, manual entries and ending are chapter-3 U5, written from U2's
  turn. If the ending keys change, update `RunEndingView`'s serialized `jimKeys`/`consoleKey` in
  `Game.unity`.
- Chapter 2's new jobs' lines are chapter-2 U4, written from U2's episode-2 turn. This plan owns the
  episode turns and the chapter 1–2 seams they land on.
- English only; `Levels_es` is an empty stub. The new wire lines are checked with chapter 3's jobs.

## Manual check (headset)

1. Job 1's receipt (Jim's introduction) and job 7's close point forward, not just at the Wrap working.
2. Chapter 2's opener follows from job 7's close.
3. Chapter 1's and 2's bulletins still type once, and point at their episode.
4. 2-4's close points at chapter 3's damage (bodies that no longer behave).
5. Every changed spoken line plays its clip (no missing TTS, no stale line from a renamed key).
