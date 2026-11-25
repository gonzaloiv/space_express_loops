# Release — the first paid store version

**Status:** design · release.
**Date:** 2026-11-18.

**Related:** [chapters](chapters.md) · [jobs](jobs.md) · [lore](world/lore.md) ·
[plans board](../plans/README.md) §Road to the store

## Goal

A **€4 Meta store release on 2026-12-06**, about **40–45 minutes** of story. The harder new jobs
carry most of that time. A small set of 20 freelance contracts comes after it as a bonus.
**Decided 2026-11-18:** ~21 story jobs plus 20 freelance contracts is the content for €4. The daily contract is left out. Today the 11
jobs play in 15–20 minutes. **Updated 2026-11-25:** ~23 story jobs (chapter 2 to ten, chapter 3 on
anchored bodies and repulsors).

## Where we are

| Item | State |
|---|---|
| Chapter 1 (7 jobs) + chapter 2 (2-1 to 2-4) | Shipped in the competition build |
| Seeds retune (01_03, 01_05, 01_07; `growOnly` on 01_01) | Done (2026-11-20) |
| Fleet jobs won by shrinking the sun (2-1 to 2-3) | **To do**: needed before new fleet jobs |
| Meta Platform | Init only (`MetaPlatformClient`); **no entitlement check yet** |
| Store validation | **Passed** (2026-11-25): the current version is ready to publish; the release is a new build upload |

## Content: the story first, then 20 freelance contracts

**The story is the game: about +12 jobs, for ~23 in total.** These are not taught yet:

| New job | What it asks | Cost |
|---|---|---|
| Fixed-cube fleet (chapter 2 intro) | One pinch on a fixed cube moves two ships | Low: the multi-ship bake already ships a fixed frame |
| Fleet capstone (2-5) | 3 ships, fan-out, high scramble | Low once the sun answer is fixed |
| Clear with a fleet | Shrink an obstacle that blocks more than one ship | Low–medium: multi-ship bakes cannot request `Clear` yet |
| Anchored bodies (chapter 3) | A planet the Wrap cannot change; bend the line with its neighbours | Low: `fixedMass` exists in the sim and the bake; needs a look and a bake rule |
| Repulsors (chapter 3) | A body that pushes; shrink it to open a gap, grow it to shove a ship | Medium: a negative strength, a raised lattice bump, a look, bake support |

- **Chapter 2 grows to 10 jobs.** Two fixed-cube fleet jobs open it, then 2-1 to 2-4, the 2-5
  capstone, and three jobs that bring Clear into the fleet.
- **A new chapter 3 has ~6 jobs on broken bodies** (decided 2026-11-25): anchored planets, then
  repulsors, then both, with its own episode and client rows. **Its fields are presented as damage**
  (decided 2026-11-18): the scramble is spacetime torn, and the player's Wraps put the route back;
  some bodies no longer behave. Jim plays it down: it's all broken, but just do your job; it probably has
  nothing to do with our machine ([lore](world/lore.md) §Episodes). If repulsors are not working in
  the editor by 1 Dec, chapter 3 ships with anchored jobs and repulsors come in an update.
- **Dropped (2026-11-25):** multi-zone fleets (`zones = ships`) and Mixed as lessons. With a movable
  cube, one zone per ship is what the player already does in 2-1; the rule only lives in the
  validator. Mixed (grow one, shrink another) is something players already try on jobs with more
  than one route, so it does not read as a new idea. Resize too: with a movable cube, two small
  pinches do what one big one does, so the bigger cube is a shortcut, not a lesson.

**Freelance: 20 contracts, opened after the story ends.** This is a light version of the
[chapters](chapters.md) §Freelance design, using what the story already has:

- Prebaked like the story jobs, then curated by playing each one. At 20 jobs, playing them replaces
  the automatic quality filter.
- Graded with the same 1–3 stars; the exact-entropy score waits.
- One client and cargo line per job; no Jim intro.
- Shown as one extra system on the map after the ending; no tiers and no job board.

**Time estimate.** Today's ~1.5 min per job is the average of mostly teaching jobs. The new jobs are
harder (capstones, Clear with a fleet, broken bodies), so they should take longer: estimate ~3 min each
(the two fixed-fleet intro jobs are shorter).

| Part | Jobs | Estimate |
|---|---|---|
| Current story | 11 | 15–20 min |
| New story jobs | ~12 | ~30 min |
| Freelance (for players who play it) | 20 | ~30 min (no intro or Jim lines: ~1.5 min each, solving only) |

The story alone could reach 40–45 min, so freelance is a bonus rather than needed to hit the target.
Without an intro, a freelance job is mostly solving time, so 20 of them stay short.
Time the first new jobs on the headset. If they run long, chapter 3 can shrink; if they run short,
freelance fills the gap.

## Story arc

**The story needs an arc across the three episodes.** Today it is a string of separate ideas: the
Wrap works (episode 1), then the firm gets greedy (episode 2), and each job adds its own beat. The
pieces are good ([lore](world/lore.md) §Episodes, [jobs](jobs.md) §The manual's arc). What's missing
is a line that pulls the player from one chapter into the next. The arc and its three turns now live in
[lore](world/lore.md) §Episodes (settled 2026-11-25): the episodes build to the realisation that if
something breaks the universe, there may be no good option left. For this release:

- **What breaks (settled 2026-11-25).** The damage is in the bodies: anchored planets and repulsors.
  The Wrap routes a delivery through it, not mend a fused body; Jim denies the cause.
- **Episodes 1–2's lines** now lead there: the job 1 receipt and job 7 close, chapter 2's opener and
  2-8's close, the manual entries, the bulletins, and the sector wire.
- **Each episode's turn** is in [lore](world/lore.md) §Episodes: episode 2 follows from episode 1 and
  sets up episode 3.
- **The ending.** Chapter 3's close is the release's ending; the current ending moves there or is
  rewritten once chapter 3's jobs exist. Freelance plays as the epilogue: Jim's job board after the
  story.
- **What stays.** Jim's pride → doubt and the manual's cheerful → complicit → denial run across all
  three episodes, not just episode 1.

## Scope

**In:**
- Chapters 1–3 (story) and Jim's lines for the new jobs
- 20 freelance contracts
- Entitlement check
- Store page and the Meta demo option
- Release build

**Out:** the daily contract and leaderboards, freelance tiers and scores, resize and rotate, debris
and inverted bodies ([later chapters](../ideas/later-chapters.md)), and chapters beyond 3.

## Schedule

Rescheduled 2026-11-25. Store validation has passed, so the release is a build upload with no review
wait; anything that is late (freelance first) ships as an update.

| Days | Lands |
|---|---|
| 25–26 Nov | The current state: fix the fleet sun answer (fleet rebake); the story arc and episodes 1–2's lines |
| 27–30 Nov | Chapter 2 to ten jobs |
| 1–3 Dec | Chapter 3: anchored bodies, repulsors and the ending |
| 3–5 Dec | Freelance: bake, play and keep 20, and the map system; entitlement check (if still missing); headset timing pass; release build; upload |
| 6 Dec | Release |

## Open questions

- **Chapter 3's episode.** Closed 2026-11-25: *3 — the damage*; its clients and cargo are in
  [chapters](chapters.md) §Chapter 3.
- **Freelance on the map.** One system after the ending, or a separate board in Jim's panel?
