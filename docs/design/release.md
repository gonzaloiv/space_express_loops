# Release — the first paid store version

**Status:** design · release.
**Date:** 2026-11-18.

**Related:** [chapters](chapters.md) · [jobs](jobs.md) · [lore](world/lore.md) ·
[plans board](../plans/README.md) §Road to the store

## Goal

A **€4 Meta store release on 2026-12-06**, about **40–45 minutes** of story. The harder new jobs
carry most of that time. A small set of 20 freelance contracts comes after it as a bonus.
**Decided 2026-11-18:** ~21 story jobs plus 20 freelance contracts is the content for €4. The daily contract is left out. Today the 11
jobs play in 15–20 minutes.

## Where we are

| Item | State |
|---|---|
| Chapter 1 (7 jobs) + chapter 2 (2-1 to 2-4) | Shipped in the competition build |
| Seeds retune (01_03, 01_05, 01_07; `growOnly` on 01_01) | **To do** |
| Fleet jobs won by shrinking the sun (2-1 to 2-3) | **To do**: needed before new fleet jobs |
| Meta Platform | Init only (`MetaPlatformClient`); **no entitlement check yet** |

## Content: the story first, then 20 freelance contracts

**The story is the game: about +10 jobs, for ~21 in total.** The mechanics are enough; no new
gesture is needed. These combinations are not taught yet:

| Combination | What it asks | Cost |
|---|---|---|
| Clear with a fleet | Shrink an obstacle that blocks more than one ship | Low: existing bakes, `Clear` role |
| Fleet capstone (2-5) | 3 ships, fan-out, high scramble | Low once the sun answer is fixed |
| Multi-zone fleets (`zones = ships`) | Each ship needs its own zone | Medium: a per-ship zone search in the bake |
| Mixed | Grow one zone, shrink another | Medium: needs ≥ 2 routes and a field that bakes |
| Resize | Grab the cube's corners | High for now: works, but the explanation is not ready |

- **Chapter 2 grows to 8 jobs.** 2-5 is the fleet capstone; 2-6 to 2-8 bring Clear into the fleet.
- **A new chapter 3 has ~6 jobs built from combinations** (multi-zone fleets, then Mixed), with its
  own episode and client rows. **Its fields are presented as damage** (decided 2026-11-18): the
  scramble is spacetime torn, and the player's Wraps put it back. Jim plays it down: it's all
  broken, but just do your job; it probably has nothing to do with our machine. Only Jim's lines and
  the manual change; no new physics, bake or visual work ([lore](world/lore.md) §Episodes). The instrument chapter (resize, rotate) moves to chapter 4, the first
  update.

**Freelance: 20 contracts, opened after the story ends.** This is a light version of the
[chapters](chapters.md) §Freelance design, using what the story already has:

- Prebaked like the story jobs, then curated by playing each one. At 20 jobs, playing them replaces
  the automatic quality filter.
- Graded with the same 1–3 stars; the exact-entropy score waits.
- One client and cargo line per job; no Jim intro.
- Shown as one extra system on the map after the ending; no tiers and no job board.

**Time estimate.** Today's ~1.5 min per job is the average of mostly teaching jobs. The new jobs are
harder (capstones, multi-zone fleets, Mixed), so they should take longer: estimate ~3 min each.

| Part | Jobs | Estimate |
|---|---|---|
| Current story | 11 | 15–20 min |
| New story jobs | ~10 | ~30 min |
| Freelance (for players who play it) | 20 | ~30 min (no intro or Jim lines: ~1.5 min each, solving only) |

The story alone could reach 40–45 min, so freelance is a bonus rather than needed to hit the target.
Without an intro, a freelance job is mostly solving time, so 20 of them stay short.
Time the first new jobs on the headset. If they run long, chapter 3 can shrink; if they run short,
freelance fills the gap.

## Story arc

**The story needs an arc across the three episodes.** Today it is a string of separate ideas: the
Wrap works (episode 1), then the firm gets greedy (episode 2), and each job adds its own beat. The
pieces are good ([lore](world/lore.md) §Episodes, [jobs](jobs.md) §The manual's arc). What's missing
is a line that pulls the player from one chapter into the next. The arc now lives in
[lore](world/lore.md) §Episodes: the episodes build to the realisation that if something breaks the
universe, there may be no good option left. For this release:

- **Settle what breaks** and the player's part in it, before chapter 3's lines are written.
- **Revise episodes 1–2's lines** (Jim's openers, manual entries, the bulletins) so they lead there.
- **Each episode's turn.** What changes for Jim and the firm in each chapter, so episode 2 follows
  from episode 1 and sets up episode 3.
- **The ending.** Chapter 3's close is the release's ending; the current ending moves there or is
  rewritten. Freelance plays as the epilogue: Jim's job board after the story.
- **What stays.** Jim's pride → doubt and the manual's cheerful → bleak still run across all three
  episodes, not just episode 1.

## Scope

**In:**
- Chapters 1–3 (story) and Jim's lines for the new jobs
- 20 freelance contracts
- Entitlement check
- Store page and the Meta demo option
- Release build

**Out:** the daily contract and leaderboards, freelance tiers and scores, resize and rotate, and
chapters beyond 3.

## Schedule

| Week | Lands |
|---|---|
| 18–22 Nov | The story arc; seeds retune; fix the fleet sun answer; entitlement check; start the store page (art, text, age rating, privacy policy) |
| 23–29 Nov | Chapter 2 to 8 jobs; the multi-zone fleet bake; chapter 3 |
| 30 Nov–4 Dec | Freelance: bake, play and keep 20, and the map system; Jim's lines; headset timing pass; release build; submit |
| 6 Dec | Release |

**Risk: Meta's review time.** A submission on 4 Dec gives 2 days. Check how long review takes
now. If it takes a week or more, submit the story by ~28 Nov and ship freelance as an update.

## Open questions

- **Meta review time** for a new paid app. This decides the submission date.
- **Chapter 3's episode.** Its title, what it says, and its clients; follows from §Story arc.
- **Freelance on the map.** One system after the ending, or a separate board in Jim's panel?
