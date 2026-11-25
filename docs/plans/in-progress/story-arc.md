# Story arc

**Status:** now · universe · updated 2026-11-26 — arc lines done; the card's manual texts, play-order
versions, instant contract and 60 cps typing are in. The sheet mirror and the headset pass remain.

**Related:** [plans board](../README.md) · [lore](../../design/world/lore.md#episodes) ·
[jobs](../../design/jobs.md#the-manuals-arc) · [chapter 3](../backlog/chapter-3.md) (the ending)

## Goal

The arc's revised lines play with their own audio, and the job card reads in a glance: one rule and
one line of fine print, settled in under 3 seconds, without repeating what Jim says.

## Done (2026-11-25)

What breaks, the three episode turns and the per-chapter manual register are in lore and jobs; the
chapter 1–2 seams, the bulletins and the 24-line sector wire are in the line tables. The card's eleven
manual entries are in `Levels_en` and jobs.md, the first-card version uses chapter play order, the
contract lands at once, and the manual types at 60 cps. The ending and the "eleven years" note are
chapter 3's U5.

## Why the card

Each card types JOB, CLIENT, CARGO, a heading ("MANUAL v1.0.3 (INITIAL DEPLOYMENT)") and a 92–196
character entry at 40 characters a second: 6–8 seconds before it settles, then Jim speaks 2–3 intro
lines. Job 1 is the longest, when the player knows least. Some entries repeat Jim (2-4's manual opens
with his line "Three ships. The budget did not grow."; 2-1 overlaps "two runs for the price of one").
The card also stays on screen during the run with the full entry.

## Units

### U1 — finish the arc's lines

- Clips are done (2026-11-25): `level_01_01_complete_03`, `level_01_07_complete_05_laughing` and
  `level_02_04_complete_02_confused` have new files and play the new lines. The remaining task is the
  sheet: confirm the six edited `Levels_en` lines (job 1 and 7 closes, job 7's manual, 2-4's close,
  both bulletins) and the 12 new wire lines, or the next Pull reverts them. The sheet credentials are
  not stored in the repo; re-enter them locally before a Pull, or make `Levels_en` the source.
- Exit: a Pull from the sheet changes nothing.

### U2 — one rule, one line of fine print

- Rewrite every manual entry (01_01–01_07, 02_01–02_04) to one teaching rule plus one fine-print line,
  about 90 characters at most. Keep the arc's beats: "Welcome, new employee." (job 1), "There is still
  no office." (job 7), the causality-tear fine print (job 5).
- No entry repeats a line Jim speaks on the same job.
- Update lore's job-5 fine-print quote to the new entry; the old "Spacetime elasticity is finite…
  localized causality tears" is no longer on the card.
- Record the rule in [jobs](../../design/jobs.md#the-manuals-arc) and update its manual table; the new
  chapter 2 and chapter 3 jobs follow it. Edit the sheet, then pull (re-enter the sheet credentials
  locally first).
- On screen only: no clips change.
- Exit: every entry is ≤ ~90 characters; jobs.md matches `Levels_en`. **Done 2026-11-25.**

**The texts (approved 2026-11-25; in `Levels_en` and jobs.md).** Characters in brackets; today's
entries ran 92–196.

| Job | New entry | Kept from today |
|---|---|---|
| 01_01 | Welcome, new employee. Wrap from any room. Headquarters is restructuring indefinitely. (86) | the welcome; the absent HQ (console thread) |
| 01_02 | The star bends space like any planet. Stellar mass is included in your licence. (79) | the star is a source; the licence joke |
| 01_03 | Shrink a world to widen the door. Residents are not notified. (61) | the clear lesson; residents |
| 01_04 | Where you work is a decision. Our Space Corp does not reimburse home office expenses. (85) | the console thread's "choose" beat |
| 01_05 | Multi-zone enabled. Wrap only what you need. No liability for causality tears. (78) | the tear fine print (the arc's seed) |
| 01_06 | The field is free. The Wrap is not. Use existing gravity before someone privatizes it. (86) | the line lore quotes |
| 01_07 | The manual was written for this one. Welcome, new employee. There is still no office. (85) | the welcome's return; "no office" |
| 02_01 | Two ships, one budget. Extra ships do not raise your allowance. This is a feature. (82) | the budget joke |
| 02_02 | One origin, two promises: bend near the start. Shared failures are billed twice. (80) | "One origin. Two promises." |
| 02_03 | Fix the shared approach once. Our Space Corp values every customer. Both of them. (81) | the last-two-customers joke |
| 02_04 | Every ship draws on the same Wrap. Live cargo travels at its own existential risk. (82) | the fleet budget, said without echoing Jim's `init_01` ("Three ships. The budget did not grow."); the animal |

Dropped on purpose: 01_01's full room-placement steps (the shortened rule "Wrap from any room" stays;
the gesture prompts teach the gestures), "A smaller world is a wider door" (folded into the rule),
"Shared departures are billed once", and "Efficiency is a virtue until it is a cost" (02_04 now teaches
the fleet's budget instead of repeating Jim).

### U3 — a faster card

- The release name shows only on the chapter's first card in **play order** (`x.0.1`); later jobs show
  just `MANUAL vX.0.N`. Do not key this to `AsIndex(id)`: chapter 2's new intro jobs have higher ids
  than 02_01, so the first played card is not x.0.1 today. Use `ChapterData.levels` (or an explicit
  first-card flag) and number the manual by play order, or the version runs backwards.
- `JobCardPanel` only receives `PuzzleData`; put the ordinal/first-card flag on that data at bake or
  pass it from the selection flow. Do not have the view read `Chapters` itself. **Done:** `JobCardStamp`
  carries `LevelSelector.GetLevelOrdinal` from `PuzzleSelectionState` into the run; the map views call
  the same `GetLevelOrdinal` with the selected puzzle.
- `JobCardTypewriter`: reveal the clock header, JOB, CLIENT and CARGO at once, then type only the
  manual. **Done:** the old loop typed all four lines sequentially.
- `JobCard.prefab`: `typeCharsPerSec` 40 → ~60 (tune on the headset).
- Update `PuzzleContractKeysTests` if the version moves from `AsIndex(id)` to play order; it asserts
  02_04 reads v2.0.4 today.
- Exit: every card settles in under 3 seconds in the editor; the first card of each chapter shows the
  release name and versions do not go backwards in play order; the fast suite is green.
  **Done 2026-11-25:** `PuzzleSelection` 18/18 and `SpaceShaping` 99/99 in EditMode; headset timing is
  the manual check.

### U4 — Jim's repeats (optional)

- Cut Jim's intro lines that say what the card already shows. With U2's entries no manual repeats
  Jim, so check the cargo and client rows against the intro lines. Keep 2-4's `init_02` ("The
  orchestra is in two ships. The animal is in the third."): it says which ship carries what, which
  the card does not. Each cut removes a spoken key: delete its clip and take it out of the downloader
  batch.
- Exit: no job's intro repeats its card without adding information; no stale clip for a removed key.

## Manual check (headset)

1. Job 1's card settles in under 3 seconds and still opens with "Welcome, new employee."
2. A chapter's first card shows the release name; the next job's card shows only the version, and
   versions follow play order.
3. The clock header, JOB, CLIENT and CARGO appear at once; only the manual types, and the type sound
   still ticks.
4. No card repeats what Jim says next.
5. Job 1's and job 7's closes and 2-4's close play their new clips.
6. During the run the card is still readable as a static reference.
7. Map → job feels quicker; time the map-to-play gap on two or three jobs before and after.
