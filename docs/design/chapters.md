# Chapters — the store release

**Status:** design · content · chapters.
**Date:** 2026-11-07.

**Related:** [jobs](jobs.md) · [progression](systems/progression.md) · [the cube](systems/cube.md) ·
[vision](vision.md) §6 · [plans board](../plans/README.md) §Road to the store

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
No new verb. The ramp restarts: low scramble, Ring shape.

| # | Lesson | Brief | Field | Why |
|---|---|---|---|---|
| intro 1 | **One pinch, two ships** | fixed · 2 ships · one origin | Ring · low scramble | The fixed cube near the shared origin bends both paths at once. |
| intro 2 | **One zone between** | fixed · 2 ships · separate | Ring · low scramble | One zone placed between two runs rescues both. |
| 2-1 | **Two ships** | movable · 2 ships · separate | 10 + sun · Ring · low scramble | One field, two runs; each ship has its own fix. |
| 2-2 | **Fan-out** | movable · 2 ships · one origin | 10–12 · Spiral | An edit near the shared origin bends both paths. |
| 2-3 | **Converge** | movable · 2 ships · one destination | 12 · Spiral | The planet by the shared destination fixes both approaches. |
| 2-4 | **Three ships** | movable · 3 ships · separate | 12 · Cluster | Three fixes on one budget; the field carries more than one ship. |
| 2-5 | **Fleet capstone** | movable · 3 ships · fan-out | 12 · Cluster · high scramble | Placement, shared paths and the field across every ship. |
| 2-6 to 2-8 | **Clear with a fleet** | movable · 2–3 ships | Clear answer | One obstacle in more than one ship's path: one blocker → among decoys → two blockers. |

The intro jobs play before 2-1; 2-1 to 2-4 keep their ids, and new jobs take the next free ids
([chapter 2 to ten](../plans/backlog/chapter-2-to-ten.md)). Multi-ship bakes need ≥ 9 planets, use
one cube (fixed or movable), and cannot request a role yet, so every fleet job is checked by playing
it. **Slightly Used Stars Ltd.** returns at 2-2.

**2-1 to 2-4 move into v1** (decided 2026-11-11, after the two-ship headset test: the fleet bakes play
better than the single-ship jobs). They ship as episode 2 of the story ([lore](world/lore.md)
§Episodes); **The Very Small Opera** takes 2-4 until a 2-5 capstone bakes. Headset pass done 2026-11-15.

## Chapter 3 — broken bodies (store release)

The damage shows in the bodies themselves (decided 2026-11-25). No new gesture: the same pinch meets
bodies that no longer behave. ~6 jobs ([chapter 3](../plans/backlog/chapter-3.md)).

| Lesson | Body | Teaches | Cost |
|---|---|---|---|
| **Anchored** | a planet fused in place; the Wrap does not change it | work around what you cannot touch: bend the line with its neighbours | low — `fixedMass` exists in the sim and the bake; needs a look and a bake rule |
| **Repulsor** | a body that pushes instead of pulls; the lattice bulges up | the same verbs, read backwards: shrink it to open a gap, grow it to shove a ship aside | medium — a negative strength, the raised well, a look, bake support |

The ramp: an anchor on the line → among decoys → a repulsor to shrink → a repulsor to grow → both →
a capstone (with a fleet or a second sun). A second sun (another `centreOnly` source) is field
variety inside these jobs, not a lesson. If repulsors are not ready, chapter 3 ships with anchored
jobs and repulsors come in an update.

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

Optional jobs beside the chapters: graded, no story. Jim's job board in the fiction.

- **Prebaked offline**, ~150 jobs shipped as assets; the bake is editor-only and too slow for the headset.
- **Tiers follow the lessons** — Ring / Spiral / Cluster, 1 / 2 / 3 ships, 1 or 2 zones. A tier opens
  once the chapters have taught what it needs.
- **A finer score:** the exact entropy left ("94% efficient"); the chapters keep 1–3 stars.
- **A quality filter.** Reject a bake when the start barely shows the field (the existing chord check),
  the cheap and expensive routes cost about the same, there is one route, or the start is too close to
  solved. Then play a sample of every batch.
- **Lore:** a pool of absurd clients and cargo, recombined. No economy ([vision](vision.md) §5).

**The daily contract.** One freelance job per day, picked from the pool by date, the same for every
player, ranked by entropy left on Meta's platform leaderboards. Offline, it is the daily job without the
board.

## Tuning

- **Price and pool size** follow measured minutes per job on the headset. Estimate: chapters 1–2
  ~40–60 min, freelance 1 h+.
- **The demo** is the Meta store's own demo option; no separate build.

## Open questions

- **Freelance unlock.** After chapter 2 only, or each tier as soon as its lesson is taught?
- **Chapter break on the map.** Built (2026-11-13): each chapter is its own system (a small
  spiral) side by side, with an episode title and a fainter route hop between them
  (`PuzzleMapLayout.Systems`). Kept after the headset pass (2026-11-15).
- **Cost and the cube's size.** Should an edit cost more when the zone covers more (planets, volume)?
  Decide before resize enters, since a bigger cube touches more per pinch.
- **The daily leaderboard's tie-breaker.** Entropy left will tie often on easy contracts; what breaks a
  tie without becoming a move count?
