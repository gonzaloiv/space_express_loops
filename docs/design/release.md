# Release — the first paid store version

**Status:** design · release.
**Date:** 2026-11-18.
**Revised:** 2026-11-26.

**Related:** [chapters](chapters.md) · [jobs](jobs.md) · [lore](world/lore.md) ·
[plans board](../plans/README.md)

## Goal

A **€4 Meta store release on 2026-12-06**, about **40–45 minutes** of story. The harder new jobs
carry most of that time. A set of freelance contracts comes after it as a bonus: **10 at launch, 10 in the first update**
(decided 2026-11-26). **Decided 2026-11-18:** ~21 story jobs plus a freelance bonus is the content for
€4. The daily contract is left out. Today the 11
jobs play in 15–20 minutes. **Updated 2026-11-27:** 21 story jobs (chapter 2 at eight after 02_08 was
parked, chapter 3 on anchored bodies and repulsors).

## Where we are

Status and dates live on the [plans board](../plans/README.md). Two facts this definition depends
on: store validation **passed** (2026-11-25), so the release is a build upload with no review wait and
anything late ships as an update; and the Meta Platform is init only (`MetaPlatformClient`), with **no
entitlement check yet**.

## Content: the story first, then the freelance contracts

**The story is the game: about +11 jobs, for 21 in total.** These are not taught yet:

| New job | What it asks | Cost |
|---|---|---|
| Fixed-cube fleet (chapter 2 intro) | One pinch on a fixed cube moves two ships | Low: the multi-ship bake already ships a fixed frame |
| Fleet capstone (2-9) | 3 ships, fan-out, high scramble | Low once the sun answer is fixed |
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
  cube, one zone per ship is what the player already does in 2-3; the rule only lives in the
  validator. Mixed (grow one, shrink another) is something players already try on jobs with more
  than one route, so it does not read as a new idea. Resize too: with a movable cube, two small
  pinches do what one big one does, so the bigger cube is a shortcut, not a lesson.

**Freelance: 10 contracts at launch, 10 in the first update, opened after the story ends.** This is a
light version of the [chapters](chapters.md) §Freelance design, using what the story already has:

- Prebaked like the story jobs, then curated by playing each one; playing them replaces the automatic
  quality filter.
- Graded with the same 1–3 stars; the exact-entropy score waits.
- One client and cargo line per job; no Jim intro.
- One extra system on the map, hidden until the ending is seen, with the whole chapter open at once and
  ordered easy → hard; no tiers and no job board. It is a post-story track, not part of the run: the
  ending, the coverage count and the wire stay the story's.

**Time estimate.** Today's ~1.5 min per job is the average of mostly teaching jobs. The new jobs are
harder (capstones, Clear with a fleet, broken bodies), so they should take longer: estimate ~3 min each
(the two fixed-fleet intro jobs are shorter).

| Part | Jobs | Estimate |
|---|---|---|
| Current story | 11 | 15–20 min |
| New story jobs | ~12 | ~30 min |
| Freelance at launch (for players who play it) | 10 | ~15 min (no intro or Jim lines: ~1.5 min each, solving only) |

The story alone could reach 40–45 min, so freelance is a bonus rather than needed to hit the target.
Without an intro, a freelance job is mostly solving time, so they stay short.
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
  2-9's close, the manual entries, the bulletins, and the sector wire.
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
- 10 freelance contracts at launch, 10 in the first update
- Entitlement check
- Store page and the Meta demo option
- Release build

**Out:** the daily contract and leaderboards, freelance tiers and scores, resize and rotate, debris
and inverted bodies ([later chapters](../ideas/later-chapters.md)), and chapters beyond 3.

## Schedule

On the [plans board](../plans/README.md) (Now → Next). The fixed date is the release, 6 Dec; freelance
ships in two batches, 10 at launch and 10 in the first update.

## Open questions

- **Chapter 3's episode.** Closed 2026-11-25: *3 — the damage*; its clients and cargo are in
  [chapters](chapters.md) §Chapter 3.
- **Freelance on the map.** Closed 2026-11-26: one extra system, hidden until the ending, the whole
  chapter open at once, as a post-story track outside the run's count and ending
  ([track](../plans/backlog/freelance-track.md), [jobs](../plans/backlog/freelance-jobs.md)).
