# Progression — how the game advances

**Status:** design · system · progression.
**Date:** 2026-10-23.

**Related:** [vision](../vision.md) · [core loop](../core-loop.md) · [resources](resources.md) ·
[lore](../world/lore.md)

## Goal

How the game advances across jobs — and what gates it. The competition slice is the **full run**
(8–10 jobs), not a single job, so progression is **in scope**.

## Inputs

The player's **solves** — completing a job opens the next, and lights its cube.

## Rules

Two things: **what advances**, and **what gates**.

### Three layers

1. **The spine (what gates).** Solve job N → job N+1 opens. A linear *order* paces the run; you
   cannot wander into job 9 at minute one.
2. **The network (what you build).** The map *is* the company: solved cubes lit, current
   highlighted, locked dark. Every solve **grows the network**.
3. **The score (what you chase).** Per-job **stars** (the entropy left / eco rating, 1–3) plus a
   running total — *mastery*, separate from *breadth*.

So progression has **two axes** — breadth (light every cube) and depth (3-star each).

### The map — numbered cubes in a big universe

The **level selector** is needed: a field of cubes, one per job.

- A **big universe** (the bounded sector field) holds the cubes, one per job.
- Each cube carries a **number** — the number *is* the order. The graph is **implied**, not drawn.
- **The cube is the level:** its number/seed generates the puzzle's **own field** — the campaign map
  is the backdrop, the puzzle field is the play space.

### The learning path — the cube grows

The gestures are taught in order, so the player never faces two at once:

1. **Jobs 1–2 — the fixed cube.** A fixed region; the only move is the **grab-inside scale**. One
   lever, one planet, a generous window.
2. **Job 3 — multiple scales.** The player places and scales **several zones**; the soft brush
   blends them, so "where do I scale" enters. The cube can also **resize** (the region).
3. **Job 4+ — rotate.** The spin gesture unlocks (frame dragging), adding *direction* to the scale.

So the order is **scale → multiple scales → rotate**.

A **tutorial** will be needed for this, but not yet — get the loop working first
([vision](../vision.md) §6).

### The company is lore only

No economy, no ships, no unlocks. Money is **flavour** ([lore](../world/lore.md)); progression is
the **lit cubes + stars**. Reputation / ships / systems are the wrapper, once the core loop is fun.

## Feedback

- The map visibly **fills** as you go — that filling *is* the progression feel.
- Stars per job; a running total.

## Failure

None — progression never blocks a run on a score (see Open).

## Tuning — the 10-minute shape

About **8–10 cubes**, ~1 minute each. The curve rises by **planet count** and **window tightness**.

### Variation axes (enough for ~10 distinct jobs)

The verb is always the **scale**, so variety comes from the field and the brief:

1. **Planet count** — 1 → 4+ sources.
2. **Window tightness** — generous → razor.
3. **Entropy budget** — loose → tight (grade pressure).
4. **Launch** — the failed launch's direction/speed changes per job.
5. **Well depth** — shallow → deep (the clock crawls).
6. **Layout** — single well vs multi-well; where the shortcut sits vs the natural path.
7. **Scramble** — how far the authored start sits from the solved arrangement (the difficulty dial).

Jobs 1–2 teach the scale on one planet; job 3 adds multiple scales and planet count; job 4 adds
rotate. Ten jobs is the stretch goal — prove 3–5 first, then extend only if the axes hold.

## Persistence

What saves between sessions is **player progression** — the stars / cubes solved — **not** the
in-level state. A run is authored, so it is regenerated from its seed rather than saved mid-flight.

## Open questions

- **Star-gating?** No, in the demo — never block a run on a score. Stars feed a rating, or unlock
  *cosmetics / bonus* later.
- **Map interaction:** pick a cube directly, or only "next"? (Sequential order suggests "next".)
- **The numbers:** reset per act, or run 1 → N?
