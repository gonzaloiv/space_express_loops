# Progression — how the game advances

**Status:** design · system · progression.
**Date:** 2026-10-23.
**Revised:** 2026-11-01.

**Related:** [vision](../vision.md) · [core loop](../core-loop.md) · [the cube](cube.md) ·
[lore](../world/lore.md) · [jobs](../jobs.md)

## Goal

How the game advances across jobs — and what gates it. The competition slice is the **full run** —
**3–5 jobs first**, the rest as stretch — not a single job, so progression is **in scope**.

## Inputs

The player's **solves** — completing a job opens the next, and lights its cube.

## Rules

Two things: **what advances**, and **what gates**.

### Three layers

1. **The spine (what gates).** Solve job N → job N+1 opens. A linear *order* paces the run; you
   cannot wander into job 7 at minute one.
2. **The network (what you build).** The map *is* the company: solved cubes lit, current
   highlighted, locked dark. Every solve **grows the network**.
3. **The score (what you chase).** Per-job **stars** (the entropy left / eco rating, 1–3) plus a
   running total — *mastery*, separate from *breadth*.

So progression has **two axes** — breadth (light every cube) and depth (3-star each).

### The map — numbered cubes in a big universe

The **level selector** is needed: a field of cubes, one per job.

- A **big universe** (the bounded sector field) holds the cubes, one per job.
- Each cube carries a **number** — the number *is* the order. The graph is **implied**, not drawn.
- **A level is a cube on the map:** picking it brings the cube into the play space, and its
  number/seed generates (offline) the puzzle's **own field** — the campaign map is the backdrop, the
  puzzle field is the play space.
- **Direct pick only (decided 2026-10-27):** the map is the only navigation; the result offers
  Map/Retry, not Next. Solved and current cubes are pickable; locked cubes refuse visibly.

### The learning path — the cube grows

One new lesson at a time, so the player never faces two unrelated ideas at once:

1. **Job 1 — the fixed cube, one zone.** A fixed region; the only move is the **grab-inside scale**.
   One lever, one pinch. The field has three planets so the run can be blocked, but the count is not the
   lesson ([jobs](../jobs.md) §The run).
2. **Job 2 — the centre.** The fixed cube is placed over the sun; the player scales the star with the
   same verb. It lands before the move lesson, so the sun is met with a fixed cube.
3. **Job 3 — several zones.** The level places **several fixed cubes**; the player scales each and the
   soft brush blends them. No move — "where do I scale" is the lesson on its own, learned before the
   cube can be placed.
4. **Job 4 — the cube moves.** The cube becomes **translatable**: the player chooses *where* to work,
   not just how much. "Where does the cube sit" is its own lesson, split from "several zones".
5. **Jobs 5–6 — multi-zone.** The player moves the cube and scales several zones, spreading the edit to
   spend less entropy.
6. **Job 7 — the capstone.** The cube starts over the sun; the solution scales several zones, the star
   included. The finale combines every verb, not a new control.

So the run order is **scale → centre → several zones → move → multi-zone → capstone**. **Resize is
deferred** from the slice: it works, but the explanation (size is scope, not precision) is not ready; it
is the next cut candidate.

### The several-zones rung

**Several zones is its own rung (2026-10-29).** The old rule — a fixed cube covers one region, so
`zones ≥ 2` implied `translatable` — was a limitation of the old timed win condition. With
**arrival-only**, a level can simply **place several fixed cubes**, so "several zones" (job 3) and "the
cube moves" (job 4) split into two lessons
(see [the brief](../jobs.md#the-run--the-brief)). The placed-zone condition may recur
in later jobs; for now it is job 3 only.

A **tutorial** will be needed for this, but not yet — get the loop working first
([vision](../vision.md) §6).

### The puzzle map

The **concrete run** — one row per job, with its brief fields, archetypes, field and lore skin — is in
[jobs](../jobs.md). This doc sets the advancement shape; jobs is the specific development of it.

**Multi-ship.** A job may carry more than one run; it is solved only when **every ship arrives**. The
runs are baked with a relation — **fan-out** (one origin), **converge** (one destination), or
**separate** (no shared planet) — so the shared field stays legible. Multi-ship is a variation axis,
not a mode: the same cube, entropy and arrival rules apply. The 7-job spine is single-ship; a
multi-ship job can be authored as a stretch or a later combine job.

### The company is lore only

No economy, no fleet management, no unlocks. Money is **flavour** ([lore](../world/lore.md));
progression is the **lit cubes + stars**. Reputation, fleet and systems are the wrapper, once the core
loop is fun.
The fiction for all of this — why the cubes are numbered, why the order, what the stars mean — is in
[lore §The progression](../world/lore.md).

## Feedback

- The map visibly **fills** as you go — that filling *is* the progression feel.
- Stars per job; a running total.

## Failure

None — progression never blocks a run on a score (see Open).

## Tuning — the shape

**The full run is seven cubes**, ~1 minute each. The curve rises by **planet count**, **field depth**,
and **solution cost**.

**Entropy is derived, not hand-set.** The bake finds a solution and sets the budget to **3×** its
entropy cost; stars are **1/3 and 2/3** of that budget. So 3★ means spending no more than the bake's
solution, 2★ up to twice it, otherwise 1★. The thresholds are the same shape every graded job; job 1 is
the tutorial exception (no fail, no stars). The field sets the absolute cost and how hard it is to read.

### Variation axes (enough for ~7 distinct jobs)

The verb is always the **scale**, so variety comes from the field and the brief:

1. **Planet count** — 1 → 4+ sources.
2. **Solution cost** — cheap → expensive; the bake triples it into the budget, so this sets the
   absolute bar, not the star thresholds.
3. **Well depth** — shallow → deep (a deeper well bends the path more).
4. **Well layout** — single well vs multi-well; where the shortcut sits vs the natural path.
5. **Scramble** — how far the authored start sits from the solved arrangement (the difficulty dial).
6. **Ships** — one run → several runs, all arriving.
7. **Depth** — flat disc → bulge and tilt; where the vertical structure sits.
8. **Field shape** — Ring → Spiral → Cluster; one shape per act, so the run's silhouette turns with it.
9. **Sun** — radius and mass; scalable by its centre by default, lockable per puzzle.

## Persistence

What saves between sessions is **player progression** — the stars / cubes solved — **not** the
in-level state. The field is baked, so a run is rebuilt from its baked field rather than saved
mid-flight (the seed is authoring-only).

## Open questions

- **Star-gating?** No, in the demo — never block a run on a score. Stars feed a rating, or unlock
  *cosmetics / bonus* later.
- **The numbers:** reset per act, or run 1 → N? (The slice proposes one chapter, numbered 1 → N.)
- **The rungs.** Re-analyze the learning-path order and count
  (scale → centre → several zones → move → multi-zone → capstone) after the run shape settles; the current
  order is the working shape, not final.
- **Entropy legibility.** The budget is derived from the bake's best solution; the player never sees
  that reference. Can they track spent vs remaining well enough from the readout?
- **Run length.** What set fills a **10-minute session** and stays fun — 5, 7, or 8 jobs? The current
  target is 7 × ~1 min; prove 3–5 first.
