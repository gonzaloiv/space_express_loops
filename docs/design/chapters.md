# Chapters — the store release

**Status:** design · content · chapters.
**Date:** 2026-11-07.

**Related:** [jobs](jobs.md) · [progression](systems/progression.md) · [the cube](systems/cube.md) ·
[vision](vision.md) §6 · [plans board](../plans/README.md) §Road to the store

## Goal

The shape of the game after the competition slice: a **~€5 store release** with chapters 1–2 and the
freelance contracts, then chapter 3 as the first update. Something in the store sooner; real players
decide what comes next. Chapters beyond 3 are ideas ([later chapters](../ideas/later-chapters.md)).

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
zone, grow a well with the other) becomes a later chapter's job instead.

## Chapter 2 — the fleet (v1, then the store release)

Several ships share one field and one entropy budget; the job solves only when **every ship arrives**.
No new verb. The ramp restarts: low scramble, Ring shape.

| # | Lesson | Brief | Field | Why |
|---|---|---|---|---|
| 2-1 | **Two ships** | movable · 2 ships · separate | 10 + sun · Ring · low scramble | One field, two runs; each ship has its own fix. |
| 2-2 | **Fan-out** | movable · 2 ships · one origin | 10–12 · Spiral | An edit near the shared origin bends both paths. |
| 2-3 | **Converge** | movable · 2 ships · one destination | 12 · Spiral | The planet by the shared destination fixes both approaches. |
| 2-4 | **Three ships** | movable · 3 ships · separate | 12 · Cluster | Three fixes on one budget; the field carries more than one ship. |
| 2-5 | **Fleet capstone** | movable · 3 ships · fan-out | 12 · Cluster · high scramble | Placement, shared paths and the field across every ship. |

Multi-ship bakes need ≥ 9 planets, use one movable cube, and cannot request a role, so every fleet job
is checked by playing it. **Slightly Used Stars Ltd.** returns at 2-2.

**2-1 to 2-4 move into v1** (decided 2026-11-11, after the two-ship headset test: the fleet bakes play
better than the single-ship jobs). They ship as episode 2 of the story ([lore](world/lore.md)
§Episodes); **The Very Small Opera** takes 2-4 until a 2-5 capstone bakes. Headset pass:
[freeze day](../plans/backlog/freeze-day.md).

## Chapter 3 — the instrument (first update)

The cube itself grows: two new gestures, one at a time.

| Lesson | Gesture | Teaches | Cost |
|---|---|---|---|
| **Resize** | grab the frame's corners (`[1×, 2×]`) | size is **scope**, not precision: a bigger cube touches more per pinch | low — the mechanic works; it needs the explanation and `resizable` rows |
| **Rotate** | rotate the content inside the cube | aim a well or open a gap by turning the covered zone | high — new gesture, code and bake support for rotated solutions |

Resize first: cheaper, and it already exists ([the cube](systems/cube.md) §Open questions). The rotation
details are to be folded in from the earlier rotation idea when this chapter is planned.

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
- **Chapter break on the map.** Being tested (2026-11-13): each chapter is its own system (a small
  spiral) side by side, with an episode title and a fainter route hop between them
  ([sector wire](../plans/in-progress/sector-wire.md) U3). Keep it if it reads on the headset.
- **Chapter 3's pricing.** A free update (reviews) or a price step?
- **Cost and the cube's size.** Should an edit cost more when the zone covers more (planets, volume)?
  Decide before resize enters, since a bigger cube touches more per pinch.
- **The daily leaderboard's tie-breaker.** Entropy left will tie often on easy contracts; what breaks a
  tie without becoming a move count?
