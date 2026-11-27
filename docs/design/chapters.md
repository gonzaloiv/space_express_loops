# Chapters — the store release

**Status:** design · content · chapters.
**Date:** 2026-11-07.
**Revised:** 2026-11-27.

**Related:** [jobs](jobs.md) · [progression](systems/progression.md) · [the cube](systems/cube.md) ·
[vision](vision.md) §6 · [plans board](../plans/README.md)

## Goal

The shape of the game after the competition slice: a **€4 store release** with chapters 1–3 and the
freelance contracts ([release](release.md)); rotate comes in a later update. Something in the store
sooner; real players decide what comes next. Chapters beyond 3 are ideas ([later chapters](../ideas/later-chapters.md)).

## Rules

- **One idea per chapter.** A chapter teaches one new idea at low scramble, then tests it. ~2 teaching
  jobs, the rest use it; the in-between jobs are curated from several bakes per contract row.
- **Numbers restart per chapter** (`01_01`…, `02_01`…). The map joins the chapters into one ordered
  run; a chapter opens when the previous one's last job is solved.
- **Same budget rule everywhere.** 3× the bake's solution, stars at 1/2 and 2/3 ([core loop](core-loop.md)
  §3). Difficulty comes from scramble, planet count and depth, never a tighter budget.

## Chapter 1 — the Wrap (store release)

The seven-job first chapter in [jobs](jobs.md) (scale → centre → clear → move → multi-zone → assist → capstone).
**Chapter 1 stays seven jobs** (decided 2026-11-11): chapter 2 ships in v1, and a job added behind
solved jobs would never become current, so there is no job 8. **Mixed** (shrink the blocker with one
zone, grow a well with the other) is not a lesson (decided 2026-11-25): players already try it on jobs
with more than one route, so it does not read as a new idea.

## Chapter 2 — the fleet (v1, then the store release)

Several ships share one field and one entropy budget; the job solves only when **every ship arrives**.
No new verb. The ramp restarts: low scramble, Ring shape. Fields and budgets per job are in
[jobs](jobs.md) (the source); this table keeps the lessons. **Play order** (2026-11-26): 2-1 to 2-9 in order,
with 2-8 parked (2026-11-27); the ids follow it (`02_01`…`02_09`, renumbered 2026-11-26), and the
capstone 2-9 closes the chapter.

| # | Lesson | Brief | Field | Why |
|---|---|---|---|---|
| 2-1 | **One pinch, two ships** | fixed · 2 ships · one origin | 9 + sun · Ring · low scramble | The fixed cube near the shared origin bends both paths at once. |
| 2-2 | **One zone between** | fixed · 2 ships · separate | 9 + sun · Ring · low scramble | One zone placed between two runs rescues both. |
| 2-3 | **Two ships** | movable · 2 ships · separate | 10 + sun · Ring · low scramble | One field, two runs; each ship has its own fix. |
| 2-4 | **Fan-out** | movable · 2 ships · one origin | 12 + sun · Spiral | An edit near the shared origin bends both paths. |
| 2-5 | **Converge** | movable · 2 ships · one destination | 12 + sun · Spiral | The planet by the shared destination fixes both approaches. |
| 2-6 | **Three ships** | movable · 3 ships · separate | 14 + sun · Cluster | One Wrap carries three ships; the field carries more than one ship. |
| 2-7, 2-8 | **Clear with a fleet** | movable · 2 ships | + sun · Clear answer | One obstacle in more than one ship's path: one blocker → among decoys. 2-8 is parked (2026-11-27) because it repeats 2-7; the old two-blocker variant never baked. |
| 2-9 | **Fleet capstone** | movable · 3 ships · fan-out | 12 + sun · Cluster · high scramble | Placement, shared paths and the field across every ship. |

**Every system keeps a star** (decided 2026-11-26): chapter 1 keeps the sun at the system's centre;
chapter 2 puts it at the edge, level with the planets, never the answer (`SunPlacement`, `NoSunAnswer`;
an off-corridor sun floated above the field before 2026-11-26). 02_06 moved from a centred sun to the
edge on 2026-11-27 because no valid three-ship puzzles came out centred. The map
anchors each job on its system's centre, not its star, and a starless system would read as a gap; dark
systems on purpose are an idea ([later chapters](../ideas/later-chapters.md) §Other options).

The ids follow play order (renumbered 2026-11-26; the original four are now 2-3 to 2-6)
([lessons](../lessons.md), 2026-11-27). Multi-ship bakes need ≥ 9 planets, use
one cube (fixed or movable), and can request Curve or Clear, so every fleet job is still checked by
playing it. **Slightly Used Stars Ltd.** returns at 2-4.

**The original four (now 2-3 to 2-6) moved into v1** (decided 2026-11-11, after the two-ship headset test: the fleet bakes play
better than the single-ship jobs). They ship as episode 2 of the story ([lore](world/lore.md)
§Episodes); **The Very Small Opera** takes 2-4 until a 2-5 capstone bakes. Headset pass done 2026-11-15.

## Chapter 3 — broken bodies (store release)

The damage shows in the bodies themselves (decided 2026-11-25). No new gesture: the same pinch meets
bodies that no longer behave. ~6 jobs ([broken bodies](../plans/backlog/chapter-3-broken-bodies.md) for the
mechanics, [chapter 3](../plans/backlog/chapter-3-jobs.md) for the jobs).

| Lesson | Body | Teaches | Cost |
|---|---|---|---|
| **Anchored** | a planet fused in place; the Wrap does not change it | work around what you cannot touch: bend the line with its neighbours | low–medium — `fixedMass` exists in the sim; needs the field through contract → seed → bake → validator, a refused no-op pinch, a look and a bake rule |
| **Repulsor** | a body that pushes instead of pulls; the lattice bulges up | the same verbs, read backwards: shrink it to open a gap, grow it to shove a ship aside | medium — a negative strength, the raised well (drawn from the start), a look, a role rule, bake support |

The ramp: an anchor on the line → among decoys → a repulsor to shrink → a repulsor to grow → both →
a capstone (with a fleet). If repulsors are not ready, chapter 3 ships with anchored jobs (more
anchors per job for variety) and repulsors come in an update. A second sun is not a fallback: one sun
is assumed across the validator and the field.

**Episode 3 — the damage** (named 2026-11-25; manual release *DAMAGE CONTROL*). The clients stay
absurd; the damage stays in the bodies.

| # | Lesson | Client | Cargo |
|---|---|---|---|
| 3-1 | anchor on the line | The Museum of Small Disasters | a cracked marble |
| 3-2 | anchor among decoys | Two Brothers, One Address | a shared inheritance |
| 3-3 | repulsor to shrink | The Quiet Hours Union | a night shift |
| 3-4 | repulsor to grow | The Gravity Choir | a hymn in three parts |
| 3-5 | repulsor and anchor | The Last Two Customers | the other half of the gift |
| 3-6 | capstone: both, with a fleet | The Sector Audit Office | the firm's entropy ledger |

The sector audit is the frame: Jim calls it routine; the player sees what it is. If repulsors do not
ship, 3-3 and 3-4 become anchored jobs and 3-5/3-6 use more anchors.

**The sun is picked per job at bake.** A single-ship job may keep it centred (chapter 1's rule) or put
it at the edge (chapter 2's); the contract pins `SunPlacement`, `SunOnChord` and `SunRule` from the seed,
so neither look is fixed for the chapter. A centred star cannot meet `NoSunAnswer` in a fleet.

**Future options** ([later chapters](../ideas/later-chapters.md) §Broken physics): **debris** (a broken
planet's tight cluster of fragments, each its own kind, so one pinch reshapes it unevenly) and
**inverted** bodies (grow when you shrink).

## Later — the instrument

The cube itself grows. **Resize is not a lesson yet** (2026-11-25): with a movable cube, two small
pinches do what one big one does, so a bigger cube is a shortcut the player never needs. It needs a
reason (a cost rule, or a field where the pieces must change together) before it gets a chapter.

| Lesson | Gesture | Teaches | Cost |
|---|---|---|---|
| **Resize** | grab the frame's corners (`[1×, 2×]`) | size is **scope**, not precision: a bigger cube touches more per pinch | low — the mechanic works; it needs the explanation and `resizable` rows |
| **Rotate** | rotate the content inside the cube | aim a well or open a gap by turning the covered zone | high — new gesture, code and bake support for rotated solutions |

The rotation details are to be folded in from the earlier rotation idea when this chapter is planned.

## Freelance contracts (store release)

**The store release ships 10 hand-curated contracts and the first update another 10**
([release](release.md), [freelance track](../plans/backlog/freelance-track.md),
[jobs](../plans/backlog/freelance-jobs.md)): one extra system on the map, hidden until the ending and
then fully open, ordered easy → hard, 1–3 stars, one client and cargo line each, no Jim intro. It is a
post-story track outside the run's count and ending, so coverage and the wire stay the story's. Playing
them replaces an automatic quality filter. No tiers, no daily contract.

**After the release (not scheduled):** a larger pool (~150 prebaked jobs) in tiers that follow the
lessons (Ring / Spiral / Cluster, 1–3 ships, 1–2 zones), a finer score (the exact entropy left), a
quality filter on the bake, and a daily contract ranked on Meta's leaderboards. Lore stays a pool of
absurd clients and cargo; no economy ([vision](vision.md) §5).

## Tuning

- **Price and pool size** follow measured minutes per job on the headset. Target: ~3 min a job, so the 21 story jobs run
  about an hour and the 10 launch contracts close to 15 minutes.
- **The demo** is the Meta store's own demo option; no separate build.

## Open questions

- **Chapter break on the map.** Built (2026-11-13): each chapter is its own system (a small
  spiral) side by side, with an episode title and a fainter route hop between them
  (`PuzzleMapLayout.Systems`). Kept after the headset pass (2026-11-15).
- **Cost and the cube's size.** Should an edit cost more when the zone covers more (planets, volume)?
  Decide before resize enters, since a bigger cube touches more per pinch.
- **The daily leaderboard's tie-breaker.** Entropy left will tie often on easy contracts; what breaks a
  tie without becoming a move count?
