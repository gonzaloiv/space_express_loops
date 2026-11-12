# Vision — "Our Space"

**Status:** design · vision — the single source of truth for the game.
**Name:** **Our Space** (your space + outer space — the universe lives in *your* room).
**Target:** Meta VR Start Developer Competition 2026 · **seated mixed reality** ·
hand-tracking first-class.
**Date:** 2026-10-23.
**Revised:** 2026-11-09.

**Related:** [core loop](core-loop.md) · [the cube](systems/cube.md) ·
[progression](systems/progression.md) · [jobs](jobs.md) ·
[lore](world/lore.md) · [glossary](glossary.md) · [plans board](../plans/README.md)

## 1. The game, in one paragraph

**Our Space** is a seated mixed-reality game where a miniature universe floats in
your real room. You run an interstellar transport company: each job is a delivery
from one star to another, with an **eco rating**. Your one tool is
**the cube** — a tangible instrument you move over the bubble. **Grab inside it with both hands and scale**:
the space stretches (a shrunk zone is a shortcut) and the planets spread and grow. So you
deform the spacetime the ship flies through until the delivery **arrives** — and cheaply.
Relativity is the language, and the cube is how you speak it.

**Current shape.** The seated slice is playable in the MR bubble: the map (the same cube picks the
job), the seven baked jobs of chapter 1, the four fleet jobs of chapter 2, the job card, Jim's panel,
the gesture prompts and the solve window. The room-scale game is removed. Build status lives on the [plans board](../plans/README.md);
this doc does not track it.

## 2. Design pillars

1. **Seated, hand-tracked, MR.** Everything interactive sits in a comfortable arm-reach
   cone at bubble distance. Controller-free core loop.
2. **The cube is the product.** The one tool, and the signature moment — "the cube
   deforms the universe". It gets the polish budget (feel, sound, haptics, the visible
   field-warp).
3. **Scale the space and the content together.** One gesture does both; the centre arrives at job 2,
   the shrink-to-clear direction at job 3, moving the cube (the region) at job 4, multi-zone at job 5,
   the field-carries-you lesson at job 6, the capstone at job 7 ([jobs](jobs.md)). The field is
   derived, never edited.
4. **One budget, one score.** Entropy is the budget: spend it all before you solve and the job
   **fails**. Entropy left is the **eco rating**; **arriving** is the goal — the launch is only a check.
   The field itself shows the spend — calm at full, strained as it drains, about to blow near empty —
   and the bar lives in it, not on a panel. The launch itself is given, not a lever.
5. **Discrete jobs.** ~1 minute each; a **10-minute first chapter** (seven jobs) and a **four-job
   fleet episode**, **eleven jobs** in the run, all baked.
   Nothing live-ticking.
6. **Relativity is the language.** The theme and the feel, not a simulation — the cube speaks it.

## 3. Mixed reality: why the bubble is the point

"Our Space" is a pun and a design brief: the universe lives in **your** space.

- **Core (keep):** MR anchoring — the bubble sits in your real room (over a desk, a
  table, in front of the sofa), placed and scaled by two hands. Passthrough + hand
  tracking is the Meta differentiator, and MRUK is already wired
  (`UniverseView.roomAnchor` / `bubbleAnchor`).
- **Legacy (cut):** room-scale *gameplay* — planets scattered around your physical
  room, walking to them. A seated player does not walk to a planet.
- **One view, one instrument: the cube.** There is no Map/Sculpt mode split. The cube
  is a tangible MR tool you **move** over the bubble; the space **inside** it scales reality/the
  field — it transforms the zone it covers ([the cube](systems/cube.md)). **The cube's size is the
  aperture:** it sets the *scope* of an edit, not its precision; the two-hand pinch sets the magnitude
  and the level sets the size. The whole map stays visible in the background, so you get overview and detail
  at once, with no transition.

So: the *room* is the backdrop and the anchor; the *bubble* is the playfield; the
*cube* is how you touch it.

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

- **In:** the **full run** — the bubble, the cube, the **map** (level selector), the **eleven-job
  run** (chapter 1's seven + chapter 2's four), the grade and player progression
  ([core loop](core-loop.md), [progression](systems/progression.md), [jobs](jobs.md)). The four fleet
  jobs are chapter 2 in v1; the 2-5 capstone stays in the store release ([chapters](chapters.md)).
- **Out of the first run:** resize (working, but the explanation is not ready), a separate tutorial flow
  (the per-job manual card is in — [jobs](jobs.md)), and the whole feel pass. **Out until proven:**
  sectors as gameplay (the ambient sector field stays the map backdrop) and runtime/procedural level
  generation.
- **After the slice: the store release** — ~€5 on the Meta store with chapters 1–2 and the freelance
  contracts, chapter 3 as the first update ([chapters](chapters.md)).
- **Technical target:** Meta Quest 3S, **72 fps**.
- **Validation first:** do not invest past the validation (§7) until it passes.

## 7. The validation

The go/no-go test. **Validation 1** is the lab (editor/play-mode) test; **Validation 2** is on device.
Validation 1 is re-run on the current **scale-the-path** loop; the old timing/window pass no longer
applies, and arrival-only is the win condition.

1. **Validation 1 — lab, in the editor, seated, hand-tracked** (no MR/bubble): does the mechanic
   survive a goal and a cost?
2. **Validation 2 — only if 1 passes:** fuse the lab puzzle into the bubble (one seated MR
   experience), add the cube, and author more jobs.

**Validation:** if the tester solves the puzzles, can say *why*, and wants the next job —
invest in integration, polish, and the identity. If they fiddle blindly, revisit the
mechanic before writing another plan.

## 8. What not to do

- No gameplay sectors or runtime level generation before the §7 validation.
- No further investment in room-scale gameplay or the legacy idle economy; freeze and
  plan the cut.
- No co-equal live-ticking economy under the puzzle.
- No interaction affordances to paper over an illegible field — fix the read.
- No discrete steps or move counts for the *player*; discreteness is for authoring only.
- No second core mechanic (entropy-as-currency, planet-drag). One verb: the scale.
- Do not pitch a systems roadmap; pitch one polished seated MR moment.

## 9. Risks (condensed)

| Risk | Current state / mitigation |
|---|---|
| Toy, not game (no stake) | **Designed, not validated.** The entropy fail, arrival goal and grade are in the loop and the panel ([core loop](core-loop.md) §3); no outside playtest is scheduled before the submission. |
| The cube is "just a group drag" | **Resolved.** The scale is a metric stretch plus content spread/growth, not a drag ([the cube](systems/cube.md)). |
| Field illegibility | **Partly open.** Lattice warp, planet size/well, the live trajectory and the straight-line baseline are in; the start run's read is not yet validated on device ([plans board](../plans/README.md)). |
| Sun reads as another planet | **Accepted.** The current `Sun.prefab` look reads as intended; no S4 look pass. |
| Sun scale is a free win | **Open.** `mass ∝ radius³` makes a grown sun strong fast; the bake must verify scalable-by-default, otherwise lock it per puzzle. The centre (job 2) is the first test; the capstone (job 7) must also hold. |
| Ship-zone legibility | **Open.** Jobs 5–7 are multi-zone, and v1 has no cue for which zone the ship is in; only the trail shows the path ([plans board](../plans/README.md) §Open tails). |
| Prediction trust | **Mitigated in the lab.** The live trajectory previews while shaping, the ship flies on release, and the straight-line baseline shows on the start check and after a miss; the start run's read is the remaining question. |
| Identity | **Resolved** by the transport-company pivot and the name "Our Space". |
| Competition scope | **Active.** The run is authored (eleven jobs, the map, Jim's panel); what remains is the road to v1 and the deliverables. Internal freeze 2026-11-13; official deadline 2026-11-18 ([plans board](../plans/README.md)). |
| Room-scale legacy drag | **Resolved.** The room view mode is removed from the code. |
| Content ceiling | **Valid, narrower.** Authored multi-ship is fine; simultaneous unscripted multi-ship is "near-chaotic". Chapter 1's seven-job spine is single-ship; the four fleet jobs are chapter 2 in v1 ([chapters](chapters.md)). |
| Entropy legibility | **In, tuning.** The in-field bar and the passthrough strain carry the spend; the budget is derived from the bake's best solution, so the 3★ band still needs a headset read ([progression](systems/progression.md) §Open questions). |
| Run length | **Measuring.** The target is a 10-minute first chapter (seven jobs); eleven jobs × ~1 min are baked; minutes per job on the headset are not yet measured ([progression](systems/progression.md) §Open questions). |
