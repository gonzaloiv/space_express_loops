# Vision — "Our Space"

**Status:** design · vision — the single source of truth for the game.
**Name:** **Our Space** (your space + outer space — the universe lives in *your* room).
**Target:** Meta VR Start Developer Competition 2026 · **seated mixed reality** ·
hand-tracking first-class.
**Date:** 2026-10-23.
**Revised:** 2026-10-28.

**Related:** [core loop](core-loop.md) · [the cube](systems/cube.md) · [resources](systems/resources.md) ·
[relativity](systems/relativity.md) · [progression](systems/progression.md) · [presentation](presentation.md) ·
[lore](world/lore.md) · [glossary](glossary.md) · [plans board](../plans/README.md)

## 1. The game, in one paragraph

**Our Space** is a seated mixed-reality game where a miniature universe floats in
your real room. You run an interstellar transport company: each job is a delivery
from one star to another, with a **window** and an **eco rating**. Your one tool is
**the cube** — a tangible instrument you move over the bubble. **Grab inside it and scale**:
the space stretches (a shrunk zone is a shortcut) and the planets spread and grow. Both
change **time**, the ship's clock, so you deform the spacetime the ship flies through until
the delivery lands on time and cheaply. Relativity is the language, and the cube is how you
speak it.

**Current shape.** The signature interaction (a bubble in front of you, scaled by two hands)
works; the gravity puzzle works in the lab; the room-scale game is legacy. What is missing is
**one fused, playable seated slice** — and **one instrument** (the cube) to play it with.
Build status lives on the [plans board](../plans/README.md); this doc does not track it.

## 2. Design pillars

1. **Seated, hand-tracked, MR.** Everything interactive sits in a comfortable arm-reach
   cone at bubble distance. Controller-free core loop.
2. **The cube is the product.** The one tool, and the signature moment — "the cube
   deforms the universe". It gets the polish budget (feel, sound, haptics, the visible
   field-warp).
3. **Scale the space and the content together.** One gesture does both; moving the cube (the region)
   enters at job 3, resizing at job 5; rotate arrives in **later puzzles**, not the first 8. The
   field is derived, never edited.
4. **One budget, one score.** Entropy is the budget: spend it all before you solve and the job
   **fails**. Entropy left is the **eco rating**; the **window** `[Tmin, Tmax]` is the target — the launch
   is only a check. The launch itself is given, not a lever.
5. **Discrete jobs.** ~1 minute each; a **10-minute experience**, about **8–10 levels** — **3–5
   first**, extended only if the variation axes hold.
   Nothing live-ticking.
6. **Relativity is the language.** Not a skin — the cube instruments it.

## 3. Mixed reality: why the bubble is the point

"Our Space" is a pun and a design brief: the universe lives in **your** space.

- **Core (keep):** MR anchoring — the bubble sits in your real room (over a desk, a
  table, in front of the sofa), placed and scaled by two hands. Passthrough + hand
  tracking is the Meta differentiator, and MRUK is already wired
  (`UniverseView.roomAnchor` / `bubbleAnchor`).
- **Legacy (cut):** room-scale *gameplay* — planets scattered around your physical
  room, walking to them. A seated player does not walk to a planet.
- **One view, one instrument: the cube (the lens).** There is no Map/Sculpt mode split. The cube
  is a tangible MR tool you **move and resize** over the bubble; the space **inside** it scales
  reality/the field — it transforms the zone it covers ([the cube](systems/cube.md)). **The cube's
  size is the aperture** — a big cube reads wide and coarse, a small cube reads narrow and fine.
  The whole map stays visible in the background, so you get overview and detail at once, with no
  transition.

So: the *room* is the backdrop and the anchor; the *bubble* is the playfield; the
*cube* (the lens) is how you touch it.

## 4. The scale is physical (a deliberate guardrail)

v1's simulation was **scale-invariant** — zooming never changed the result, so the prediction
could be trusted across views. This direction makes **scale physical**: transforming a zone
changes what happens inside it. That is the point of the mechanic. Cost: the prediction must be
re-earned (show the result before the ship travels), and the player must always be able to tell
which zone the ship is in — the ship is always at **map size**; the *zone* changes, never the ship.

## 5. Why management is the frame, not a co-equal system

The hybrid (live economy + sculpting) breaks for a specific reason:

- **The constraint cliff.** Degrees of freedom are few (source state), while constraints grow as
  *(ships × destinations)*; when constraints outnumber degrees of freedom the level is
  **unsolvable, not hard**. A management game is *many persistent routes sharing one field* — the
  worst case. Moving one planet can break every route; the only clean resolutions (re-solve,
  auto-resolve, or freeze solved routes) all collapse to **one active delivery at a time**.
- **The rhythm clash.** An ambient economy ticking while you deliberately sculpt a
  chaotic field is two hard things at opposite tempos — brutal for a seated,
  first-time demo.

**Conclusion:** solve **one job at a time**; lock or decoratively persist solved
routes; keep the economy *between* jobs. Then the management layer is a wrapper
(identity, progression, contracts) and the sculpting stays solvable.

## 6. Scope: the competition slice

- **In:** the **full run** — the bubble, the cube, the **level selector** and **3–5 jobs first
  (8–10 as the stretch)**, the grade, and player progression ([core loop](core-loop.md),
  [progression](systems/progression.md)).
- **Out of the first 8:** rotate (arrives in **later puzzles**), a separate tutorial flow (the per-job
  manual card is in — [jobs](jobs.md)), and the whole feel pass. **Out until proven:** sectors, multi-ship,
  runtime/procedural level generation.
- **Technical target:** Meta Quest 3S, **72 fps**.
- **Validation first:** do not invest past the validation (§7) until it passes.

## 7. The validation

The go/no-go test. **Validation 1** is the lab (editor/play-mode) test; **Validation 2** is on device.

1. **Validation 1 — lab, in the editor, seated, hand-tracked** (no MR/bubble): does the mechanic
   survive a goal and a cost?
2. **Validation 2 — only if 1 passes:** fuse the lab puzzle into the bubble (one seated MR
   experience), add the cube, and author more jobs.

**Validation:** if the tester solves the puzzles, can say *why*, and wants the next job —
invest in integration, polish, and the identity. If they fiddle blindly, revisit the
mechanic before writing another plan.

## 8. What not to do

- No sectors, multi-ship, or level generation before the §7 validation.
- No further investment in room-scale gameplay or the legacy idle economy; freeze and
  plan the cut.
- No co-equal live-ticking economy under the puzzle.
- No interaction affordances to paper over an illegible field — fix the read.
- No discrete steps or move counts for the *player*; discreteness is for authoring only.
- No second core mechanic (entropy-as-currency, planet-drag). One verb: the scale.
- Do not pitch a systems roadmap; pitch one polished seated MR moment.

## 9. Risks (condensed)

| Risk | Status / mitigation |
|---|---|
| Toy, not game (no stake) | **Addressed** by the entropy budget — run out before solving and the job fails — plus the window target and the grade ([resources](systems/resources.md)). |
| The cube is "just a group drag" | Resolved: the cube scale is a **metric stretch** (a shortcut), not a drag — it spreads and grows the content too. See [the cube](systems/cube.md). |
| Field illegibility | Make the map *read the field* (straight-line reference, accel arrows); the bubble already shows the warped sheet. |
| Ship-zone legibility | The ship is map-size; show which zone it is in (tint, trail). |
| Prediction trust | Scale is now physical; show the result live, before the ship travels. |
| Identity | **Resolved** by the transport-company pivot and the name "Our Space". |
| Competition scope | Keep one fused seated slice; no sectors/multi-ship/generation until proven. |
| Room-scale legacy drag | Freeze room-scale gameplay; keep only MR bubble anchoring. |
| Content ceiling | Simultaneous multi-ship is "near-chaotic" for generation; authored multi-route is fine (a shared bottleneck is a legibility win). |
