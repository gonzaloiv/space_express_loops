# Progression — how the game advances

**Status:** design · system · progression.
**Date:** 2026-10-23.
**Revised:** 2026-11-09.

**Related:** [vision](../vision.md) · [core loop](../core-loop.md) · [the cube](cube.md) ·
[lore](../world/lore.md) · [jobs](../jobs.md)

## Goal

How the game advances across jobs — and what gates it. The competition slice is the **full run** —
the **seven jobs** of chapter 1 — not a single job, so progression is **in scope**.

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

### The map — numbered nodes in a big universe

The **level selector** is needed: a field of nodes, one per job. The **cube** on the map is the
player's instrument, used to pick a node ([the cube](cube.md) §The map uses the same cube).

- A **big universe** (the bounded sector field) holds the nodes, one per job.
- Each node carries a **number** — the number *is* the order. The graph is **implied**, not drawn.
- **A level is a node on the map:** entering it brings its field into the play space, and its
  number/seed generates (offline) the puzzle's **own field** — the campaign map is the backdrop, the
  puzzle field is the play space.
- **Direct pick only (decided 2026-10-27):** the map is the only navigation; a fail restarts the
  job and a solve returns to the map, not Next. Solved and current nodes are pickable; locked nodes
  refuse visibly.
- **The last solve closes the run (decided 2026-11-11).** With every job solved, the map plays the
  ending — the whole network lit, the total stars, the title and Jim closing the episode — then it is
  usable again. A persisted flag keeps it to once per save, and once more when a longer run is added.
- **The first session skips the map (decided 2026-11-06).** On a fresh save the game opens straight
  in job 1; the map is first seen after job 1 (or on leaving it), so the player meets the cube before
  the map, and the map's enter is the same two-hand pinch inside the same cube
  ([the cube](cube.md) §The map uses the same cube).

### The learning path — the cube grows

One new lesson at a time, so the player never faces two unrelated ideas at once:

1. **Job 1 — the fixed cube, one zone.** A fixed region; the only move is the **grab-inside scale**.
   One lever, one pinch. The field has six planets and a sun the cube does not cover, so the run can be
   blocked, but the count is not the lesson ([jobs](../jobs.md) §The run).
2. **Job 2 — the centre.** The fixed cube is placed over the sun; the player scales the star with the
   same verb. It lands before the move lesson, so the sun is met with a fixed cube.
3. **Job 3 — the other direction.** A planet blocks the line; the fixed cube sits on it, and the player
   **shrinks** it to clear the path. Same pinch as job 1, opposite role — learned before placement.
4. **Job 4 — the cube moves.** The cube becomes **translatable**: the player chooses *where* to work,
   not just how much. "Where does the cube sit" is its own lesson.
5. **Job 5 — multi-zone.** The player moves the cube and scales several zones. This is where "several
   zones" now lands; the fixed-cube rung is retired.
6. **Job 6 — the field carries you.** A deep well already sits near the line; edit where a small change
   moves the path most.
7. **Job 7 — the capstone.** The cube starts over the sun; the solution scales several zones with the star
   included. The finale combines every verb except both roles; a later chapter brings Mixed.

So the run order is **scale → centre → clear → move → multi-zone → assist → capstone**. Resize is not
in the run ([the cube](cube.md) §Open questions).

**The obstacle is job 3.** Shrinking a blocker to clear the chord is the opposite role of the curve; it
lands right after the centre, before placement, so both directions of the pinch are learned on a fixed
field. The tutorial and the centre keep the curve; the capstone (job 7) is a Curve; a clear+curve Mixed is
deferred to a later chapter.

### The several-zones rung

Retired (2026-11-05): several zones arrive with the movable cube at job 5. Why, and the later Mixed job
that could bring the fixed rung back with Mixed, is in [jobs](../jobs.md) §The run — the mechanics.

### The puzzle map

The **concrete run** — one row per job, with its brief fields, archetypes, field and lore skin — is in
[jobs](../jobs.md). This doc sets the advancement shape; jobs is the specific development of it.

**Multi-ship.** A job may carry more than one run; it is solved only when **every ship arrives**. The
runs are baked with a relation — **fan-out** (one origin), **converge** (one destination), or
**separate** (no shared planet) — so the shared field stays legible. Multi-ship is a variation axis,
not a mode: the same cube, entropy and arrival rules apply. Chapter 1's seven-job spine is
single-ship; chapter 2's four jobs are the fleet, in v1 ([chapters](../chapters.md)).

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

**The full run is eleven jobs** (chapter 1's seven + chapter 2's four), ~1 minute each. The curve rises by **planet count**, **field depth**,
and **solution cost**.

**Entropy is derived, not hand-set** — the budget and star rule is in [core loop](../core-loop.md) §3,
and which jobs fail or grade is in [jobs](../jobs.md). The field sets the absolute cost and how hard it
is to read.

### Variation axes (enough for ~11 distinct jobs)

The verb is always the **scale**, so variety comes from the field and the brief:

1. **Planet count** — 6 (tutorial) → 12 sources by act; the field grows into a little galaxy.
2. **Solution cost** — cheap → expensive; the bake triples it into the budget, so this sets the
   absolute bar, not the star thresholds.
3. **Well depth** — shallow → deep (a deeper well bends the path more).
4. **Well layout** — single well vs multi-well; where the shortcut sits vs the natural path.
5. **Scramble** — how far the authored start sits from the solved arrangement (the difficulty dial).
6. **Ships** — one run → several runs, all arriving.
7. **Depth** — flat disc → bulge and tilt; where the vertical structure sits.
8. **Field shape** — Ring → Spiral → Cluster; one shape per act, so the run's silhouette turns with it.
9. **Sun** — radius and mass; scalable by its centre by default, lockable per puzzle.
10. **Edit role** — curve (grow a well) or clear (shrink a blocker); a multi-zone job can mix one of each.

## Persistence

What saves between sessions is **player progression** — the stars / cubes solved — **not** the
in-level state. The field is baked, so a run is rebuilt from its baked field rather than saved
mid-flight (the seed is authoring-only).

## Open questions

- **Star-gating?** No, in the demo — never block a run on a score. Stars feed a rating, or unlock
  *cosmetics / bonus* later.
- **The numbers:** settled (2026-11-07) — numbers restart per chapter (`01_01`…, `02_01`…); the map
  joins the chapters into one ordered run ([chapters](../chapters.md)).
- **The rungs.** Re-analyze the learning-path order and count
  (scale → centre → clear → move → multi-zone → assist → capstone) after the run shape settles; the current
  order is the working shape, not final.
- **Entropy legibility.** The budget is derived from the bake's best solution; the player never sees
  that reference. Can they track spent vs remaining well enough from the readout?
- **Run length.** What set fills a **10-minute session** and stays fun — 7, 11, or more jobs? Chapter 1's
  seven × ~1 min are baked; the fleet adds four; measure minutes per job on the headset.
