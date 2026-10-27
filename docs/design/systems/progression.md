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
- **A level is a cube on the map:** picking it brings the cube/lens into the play space, and its
  number/seed generates (offline) the puzzle's **own field** — the campaign map is the backdrop, the
  puzzle field is the play space.

### The learning path — the cube grows

One new idea at a time, so the player never faces two at once:

The scale has **two faces** — **space** (a shortcut / the path) and **time** (the ship's clock) —
driven by the **same factor** ([the cube](cube.md)). The player learns **space first, then time**.

1. **Jobs 1–2 — the fixed cube, one zone.** A fixed region; the only move is the **grab-inside
   scale** (the **space face**: the shortcut and the spread). One lever, one planet, a generous
   window.
2. **Job 3 — several zones.** The player places and scales **several zones**; the soft brush blends
   them, so "where do I scale" enters. The cube stays **fixed**.
3. **The time tutorial — a planetless level.** The scale still works, but with no planets only the
   **time face** shows: the grid pulses at the clock rate and the arrival slides on the window gauge.
   The player learns the clock before path and clock compete. (Recurring planetless levels are a later
   variation.)
4. **Then — the movable cube.** The region can be **moved** (translatable), so "where the cube sits"
   enters.
5. **Then — the resizable frame.** The region can be **resized** — the aperture becomes the player's
   to choose.
6. **Later puzzles — rotate.** The spin gesture (frame dragging) arrives in **later puzzles**, not
   the first 8, adding *direction* to the scale.

So the first-8 order is **scale → several zones → time → move → resize**; rotate comes later.

A **tutorial** will be needed for this, but not yet — get the loop working first
([vision](../vision.md) §6).

### The brief — the conditions a solution must meet

Each job's puzzle carries a **brief**; the per-level conditions are the source of truth here, and the
creator bakes to them ([improvements plan](../../plans/in-progress/puzzle-creator-improvements.md) Slice 4):

- **`translatable`** — the cube may be **moved** (its centre changes) or is **fixed** at home.
- **`moves`** — how many **zones** the solution scales (the cube edits `N` places, up or down).
- **`resizable`** — the frame may be **resized** (`[1×, 2×]`) or is locked.
- **`rotate`** — the spin gesture is available (later puzzles).
- **`scalable`** is **always true** — scaling the content is the verb; there is no locked-scale level.

### The puzzle map (one row per job)

What each job teaches and its variations. The counts are a **stretch goal** — prove 3–5 first.

| # | Teaches | Brief (translatable · moves · resizable · rotate) | Field (planets · layout · well) | Dials (window · entropy) | Why |
|---|---|---|---|---|---|
| 1–2 | the scale | fixed · 1 · no · no | 1 · single · medium | generous · loose→tight | first contact: one lever, one planet |
| 3 | several zones | fixed · 2+ · no · no | 2–3 · multi-well | medium · medium | "where do I scale" enters |
| time | the clock (no planets) | fixed · 1 · no · no | 0 · — · none | generous · loose | the **time face** alone: the grid pulses at the clock rate |
| 4 | move the cube | **yes** · 1+ · no · no | 3 · multi-well | medium · medium | "where the cube sits" enters |
| 5 | resize the frame | yes · 1+ · **yes** · no | 3–4 · multi-well | tight · tight | the aperture is the player's |
| 6–8 | combine | yes · 2+ · yes · no | 3–4 · mixed · deep | tight · tight | mastery: read the field, edit little |
| later | rotate | yes · 1+ · yes · **yes** | 3+ · multi-well · deep | razor · tight | direction, not just position |

**The slice.** Jobs 1–8 are the full run, ~1 minute each. The curve rises by **planet count** and
**window tightness**; rotate waits for later puzzles. The last jobs **combine** every verb the player
has learned — that is the capstone, not a locked cube.

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
4. **Well depth** — shallow → deep (the clock crawls).
5. **Layout** — single well vs multi-well; where the shortcut sits vs the natural path.
6. **Scramble** — how far the authored start sits from the solved arrangement (the difficulty dial).

Jobs 1–2 teach the scale on one planet; job 3 adds multiple scales and planet count; rotate arrives
in later puzzles. Ten jobs is the stretch goal — prove 3–5 first, then extend only if the axes hold.

## Persistence

What saves between sessions is **player progression** — the stars / cubes solved — **not** the
in-level state. The field is baked, so a run is rebuilt from its baked field rather than saved
mid-flight (the seed is authoring-only).

## Open questions

- **Star-gating?** No, in the demo — never block a run on a score. Stars feed a rating, or unlock
  *cosmetics / bonus* later.
- **Map interaction:** pick a cube directly, or only "next"? (Sequential order suggests "next".)
- **The numbers:** reset per act, or run 1 → N?
