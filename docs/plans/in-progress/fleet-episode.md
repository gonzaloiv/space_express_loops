# Fleet episode — the two- and three-ship jobs join v1 as episode 2

**Status:** now · flow · updated 2026-11-11 — the two-ship test said yes (the new bakes play better
than the single-ship ones): four multi-ship jobs go into v1 as chapter 2, told as episode 2 of the story.
The line drafts are approved; job 8 is dropped from v1. U1 and the English strings are in
(`FleetJobsTests`); review fixes F1–F3 (§Review), the clips and the headset pass are open.

**Related:** [chapters](../../design/chapters.md) §Chapter 2 · [lore](../../design/world/lore.md)
§Episodes · [jobs](../../design/jobs.md) §The run — the lore skin ·
[plans board](../README.md)

## Goal

The fleet puzzles are the best levels we have. Put them in the competition build as a second episode,
after chapter 1's seven jobs, without changing chapter 1's existing jobs.

## Contract

- **Chapter 2 in v1, four jobs**, ids `02_01`…`02_04`, after chapter 1 in `LevelSelector`. The map is
  one ordered run, so solving job 7 makes `02_01` current; the stuck-player skip also opens it after one
  fail on `01_07` (same rule as every job). No new verb; the fleet is the lesson.
- **The order follows the chapter 2 design** (2-1 to 2-4; the 2-5 capstone is not baked and stays out):

  | Job | Asset | Relation | Why here |
  |---|---|---|---|
  | 02_01 | `TwoShips_C` | separate | the cheapest bake (budget 0.3): two ships, one easy fix each |
  | 02_02 | `TwoShips_FanOut` | one origin | an edit near the shared origin bends both paths |
  | 02_03 | `TwoShips_Converge` | one destination | the planet by the shared destination fixes both |
  | 02_04 | `ThreeShips_A` | separate, 3 ships | three fixes on one budget |

  `TwoShips_Test` and `TwoShips_B` stay test assets. Swap one in if a headset play says a job is off.
- **Job 8 is dropped** (decided 2026-11-11). A job added behind solved jobs never becomes current, so
  `01_08` could only ship before `02_01`, and Mixed does not bake in time. Chapter 1 stays seven jobs
  for good; Mixed becomes a later chapter's job. Remove `01_08` from `PuzzleJobContracts` (it is
  `Pending`) in U1.
- **The skip opens the episode too** (decided 2026-11-11): one fail on `01_07` opens `02_01`, as on any
  other job (the stuck-player skip chains on purpose).
- **Route colours restart per chapter** (accepted 2026-11-11): `RoutePalette.StartIndex` is
  chapter-blind, so `02_01`–`02_04` reuse chapter 1's first colours. Each job's own routes stay
  distinct; no change.
- **An episode, not just more jobs.** Chapter 2 says one thing about the world: the Wrap works, so the
  firm gets greedy. More ships on the same entropy, sold as green. Jim books it; he is not sure he
  should. The reading stays ambiguous ([lore](../../design/world/lore.md) §The bleak reading).
- Same rules as every job: one budget for all ships, the job solves when every ship arrives, a fail
  restarts in place, stars at the same limits.

## Units

### U1 — the jobs in the run

- Move the four puzzles into `Resources/Chapters/02/` as `Job_02_0N_Puzzle.asset` with the asset
  database (GUIDs and `.meta` kept), set each `id`, and author `Chapters/02.asset` (`ChapterData`, id
  `02`) in `LevelSelector` after `01`.
- Check what keys off the job count or chapter 1: the map's layout and route (`RunMap` uses
  `levels.Count`, and the scene already has 10 route segments, so 11 nodes need no scene rework), the
  card labels (`JobCardPanel` shows client/cargo/manual only; `MANUAL r{ManualRevision}` comes from the
  id, so chapter 2 reads r1–r4), the route colours (`RoutePalette.StartIndex` is chapter-blind and the
  palette has only four entries, so chapter 2 reuses chapter 1's colours — expected, not a bug), and
  the job-open skip (`PuzzleCompleteState` opens the next job on a fail).
  No test assumes seven jobs; the ending's tests already cover 7 and 11.
- The bake tests cannot cover chapter 2 as they stand: `PuzzleJobBakeTests` iterates
  `PuzzleJobContracts.Enabled` (01_01–01_07) through the single-ship `PuzzleSeedScanner`,
  `PuzzleJobContract` has no ships/relation, `PuzzleJobValidator` checks one chord, and
  `PuzzleTestSetBuilder` writes every enabled contract into `Chapters/01`. Keep `02_0N` out of
  `Enabled` (or extend the contract, builder and validator for multi-ship and per-chapter folders)
  and add a fast check that loads the four `Chapters/02` jobs and asserts every ship endpoint stays
  out of all frames and no sun sits on any ship's path.
- The sun rule is endpoints only: `PuzzleMultiShipBake.Select` takes runs without a sun check, and a
  sun on the chord is a fair blocker. `FleetJobsTests` asserts no ship starts or ends on the sun; 02_04's
  first ship passes 0.068 from a 0.06-radius sun, which is legal.
- **Exit:** after job 7 the map shows four new nodes; each job plays with its ship colours and solves
  only when every ship arrives.

### U2 — episode 2's lines *(approved 2026-11-11)*

One short sentence per TTS entry, few commas, no dashes. Clients stay absurd; the greed is in Jim's
lines and the manual, never in the client.

| Job | Client | Cargo | Intro (Jim) | Fail | Manual entry |
|---|---|---|---|---|---|
| 02_01 | Halfmoon & Halfmoon | two identical parcels | "I sold two runs for the price of one." | "One of them got lost." | "Two ships. One Wrap. One budget." |
| 02_02 | Slightly Used Stars Ltd. | two refurbished suns | "They are back. They want both orders today." | "Slightly Used Stars is unhappy again." | "One origin. Two promises. Bend near the start and both feel it." |
| 02_03 | The Last Two Customers | two halves of a gift | "Two ships. One door." | "The gift is still in pieces." | "Many routes may share one end. Fix the approach once." |
| 02_04 | The Very Small Opera | the orchestra, one live animal | "Three ships. The budget did not grow." | "The opera starts without them." | "Efficiency is a virtue until it is a cost." |

- Complete lines (Jim), one per job: "Both arrived. Same cost. Nobody asked how." / "They paid. They
  did not say thank you." / "One door. Two deliveries. The ledger smiles." / "Three for the price of
  one. I am not sure that is good news."
- Keys in `Levels Shared Data` (`level_02_0N_init_client`, `_init_cargo`,
  `_init_manual_entry`, `_init_01`, `_complete`, `_fail`), `Levels_en` values, `Levels_es` English
  placeholders, and the clips downloaded (with run fixes F8's seven). The downloader's key list in
  `Assets/_Game/Features/Audio/Downloader - OS.unity` is a hand-kept array and needs the 12 spoken
  keys (client, cargo and manual are on-screen text and filtered out).
  Cut line: if clips miss the 2026-11-13 freeze, chapter 2 ships silent and the lines land after.
- [jobs](../../design/jobs.md) §The run — the lore skin gets a chapter 2 table.
- `FleetJobsTests.ChapterTwoLines_ResolveInEnglish` checks all 24 keys resolve in `Levels_en` with a
  value, so a hand-edited table cannot ship empty.
- **Exit:** every chapter 2 job has an intro, a complete and a fail line that play; the cards read.

## Review — 2026-11-11

U1 and U2's strings are in and right: the four puzzles moved into `Chapters/02` with their GUIDs (`02_04`
is the old `ThreeShips_A`), ids `02_01`–`02_04`, `Chapters/02.asset` after `01` in `LevelSelector`,
`01_08` gone from `PuzzleJobContracts`, 24 keys in `Levels Shared Data` with English values and
placeholders, the 12 spoken keys in the downloader, and the chapter 2 table in jobs. `FleetJobsTests`
checks the order, the ship counts, the endpoints and the English values; the fast suite passes. Before
the commit:

- **F1 — done.** The ending's three Jim clips (`run_ending_jim_01`–`03`) are in the downloader list with
  chapter 2's spoken keys.
- **F2 — done.** `FleetJobsTests` asserts no ship starts or ends on the sun; a sun on the chord is
  allowed, so a legal rebake is not rejected.
- *Second look (2026-11-11):* F1 and F2 verified; a fourth test checks every chapter 2 line resolves in
  `Levels_en` (hence the test asmdef's new `Unity.Localization` reference); the fast suite passes
  (127/127). The commit also takes that asmdef change; `Assets/AddressableAssetsData/link.xml` is a
  build artifact, not this unit's.
- **F3 — the commit is all uncommitted and shares files.** `LevelSelector`, the localization tables,
  `Downloader - OS.unity` and the new chapter folder are this unit's; `Game.unity`, `Puzzle.prefab` and
  the lattice mesh hold other tests. Stage the fleet files only; the seed assets in
  `PuzzleCreator/Data` (untracked) go with them so the levels can be rebaked.
- **Decision still open:** 02_01's budget is 0.3 (a tiny fix); keep it as the gentle first fleet job
  or swap in `TwoShips_B` after the headset play.
- **Build check.** The dev APK installs and launches; the logged session shows `HMDUnmounted` and the
  app pausing, with no Unity exception and no crash record. The black frame is the paused app until a
  worn pass, so the launch check stays manual.

## Not in this plan

- The 2-5 fleet capstone: not baked. Mixed (the old job 8) moves to a later chapter.
- A chapter break on the map (a gap or label): one continuous map for now.
- The ending: landed (2026-11-11); it already counts any number of jobs.

## Manual check (headset)

1. Solve job 7: the map opens 02_01; chapter 1's nodes are unchanged.
2. 02_01: two ships, two route colours, one budget; one ship arriving does not solve it.
3. 02_02: an edit near the shared origin moves both lines.
4. 02_03: both lines end at the same planet in their own colours.
5. 02_04: three ships read apart; the meter reads the same as with one ship.
6. A fail in any chapter 2 job restarts it in place.
7. Each job's card, intro, fail and complete lines read and play.
8. With the stuck-player skip, one fail on 01_07 opens 02_01; confirm that is the intended way into
   the episode.
9. At 11 nodes the map still reads: dioramas do not collide, and the cube snaps to the intended node.
10. Decision: is 02_01 too easy at budget 0.3, or is that right for the episode's first job?
11. Launch the built APK while wearing the headset: boot splash → job 1, no black frame.
