# Project Direction — "Our Space"

**Status:** design direction (single source of truth).
**Name:** **Our Space** (your space + outer space — the universe lives in *your*
room).
**Target:** Meta VR Start Developer Competition 2026 · **seated mixed reality** ·
hand-tracking first-class.
**Date:** 2026-10-23.

## 1. The game, in one paragraph

**Our Space** is a seated mixed-reality game where a miniature universe floats in
your real room. You run an interstellar transport company: each job is a delivery
from one star to another, with a **window** and an **eco grade**. Your one tool is
**the cube** — a tangible instrument you move over the bubble. **Grab inside it and scale**:
the space stretches (a shrunk zone is a shortcut) and the planets spread and grow. Both
change **time**, the ship's clock, so you deform the spacetime the ship flies through until
the delivery lands on time and cheaply. Relativity is the language, and the cube is how you
speak it.

## 2. Where the project is

| Layer | State | Evidence |
|---|---|---|
| Legacy room-scale loop (planets in your room, letters → money → rounds, grab-a-ship → route) | Built, in `Game.unity`; room-scale gameplay is **legacy** | `Flow/EditionState/EditionState.cs`, `Features/Nodes`, `Features/Spaceships`, `LevelContainer.cs:17-18,198-213` |
| Room ↔ Bubble scale transition | Done, validated in headset; **the signature interaction** | `Features/Universe/View/UniverseView.cs`, [universe-views-and-interaction](../plans/done/universe-views-and-interaction.md) |
| Space Shaping physics puzzle | Lab POC: planets as sources, deterministic trajectory, ship clock + deadline; verified puzzles | `Features/SpaceShaping/Scenes/SpaceShapingPOC.unity`, [lens plan](../plans/in-progress/space-shaping-lens.md) |
| **The cube (zone transform)** | **New — the single tool; not built** | this doc |
| Space Shaping → game integration | Not started | [space-shaping-integration](../plans/backlog/gameplay/space-shaping-integration.md) |
| Sector generation | Done (G0/G1, device pass) | [universe-sector-generation](../plans/done/universe-sector-generation.md) |

**Summary:** the strong interaction (a bubble in front of you, scaled by two hands)
already works. The gravity puzzle works in the lab. The room-scale game is dead
weight for this target. What is missing is **one fused, playable seated slice** — and
**one instrument** to play it with.

## 3. The pivot: transport company

From an idle "routes and rounds" loop to an **interstellar transport company**:

- **A job = one delivery** from one star to another, with a **window** and an **eco grade**; a job
  can also be a **multi-leg / loop route** (A → B → C, sequential relaunches on one shared field —
  [space-shaping-future](../plans/backlog/gameplay/space-shaping-future.md) §3.2).
- **The verb = shaping spacetime** to make the delivery work efficiently.
- **Progression = the company:** unlock systems, buy ships, earn reputation —
  *between* jobs, never ticking live under the puzzle.

This drops the idle economy (the old loop's main problem for a puzzle) and keeps
exactly what makes the fantasy work: mail/express, a fleet, a reputation.

## 4. Mixed reality: why the bubble is the point

"Our Space" is a pun and a design brief: the universe lives in **your** space.

- **Core (keep):** MR anchoring — the bubble sits in your real room (over a desk, a
  table, in front of the sofa), placed and scaled by two hands. Passthrough + hand
  tracking is the Meta differentiator, and MRUK is already wired
  (`UniverseView.roomAnchor` / `bubbleAnchor`).
- **Legacy (cut):** room-scale *gameplay* — planets scattered around your physical
  room, walking to them. A seated player does not walk to a planet.
- **One view, one instrument: the cube.** There is no Map/Sculpt mode split. The cube
  is a tangible MR tool you move over the bubble; it transforms the zone it covers.
  **The cube's size is the aperture** — a big cube reads wide and coarse, a small cube
  reads narrow and fine. The whole map stays visible in the background, so you get
  overview and detail at once, with no transition.

So: the *room* is the backdrop and the anchor; the *bubble* is the playfield; the
*cube* is how you touch it.

## 5. The cube: the one instrument

The cube has **three gestures**, taught in order (scale → region → rotate):

| Gesture | Trigger | Effect | Relativity |
|---|---|---|---|
| **The scale** *(L0)* | a two-hand pinch **inside** the cube | scales the space *and* the content: the metric stretches (a shrunk zone is a **shortcut**) and the planets **spread** and **grow** (mass ∝ radius) | time dilation |
| **The region** *(job 3)* | grab the cube's **edges / corners** | sets *where* you work — and you can scale **several** zones; the soft brush blends them | — |
| **The rotate** *(job 4)* | spin the zone | **frame dragging** — a spinning zone swirls the path; adds direction for free | frame dragging |

**Multiple scales are the way.** The player scales **several** zones, and the soft brush blends
them into one field — that is the core of the verb, not a later unlock. The region's size must stay
**separate from the scale** — coupling them is the bug. **Rotate** (frame dragging) unlocks at
**job 4** (see [progression](progression.md) §4).

- **Soft brush.** The cube's influence falls off at the edges so neighbouring zones
  blend; a hard edge would tear the mesh.
- **The field is derived, never edited.** The player scales the space and the content; the
  field, grid and trajectory are computed from the result.
- **Planets stay the field's sources.** The *verb* changes, not the physics
  (`SpacetimeField`, `TrajectoryIntegrator` stand). Scaling the content changes the field
  for free through the existing `mass ∝ radius` rule. The lab's puzzles are **re-authored**
  for the new verb — they were verified under the old move verb.
- **Planets cannot fuse.** The existing separation constraint (`SourceConstraints`,
  1.2 × the sum of radii, applied on every recompute) pushes any overlapping pair apart,
  so a zone transform can never merge two planets — but the constraint can *fight* the
  cube's intent, so the player must see the planets separate.
- A future, purer "mesh-only" version would drop planets entirely; it is a bigger
  rewrite (it discards the 4 verified puzzles) and is **not** the slice.

## 6. Design pillars

1. **Seated, hand-tracked, MR.** Everything interactive sits in a comfortable arm-reach
   cone at bubble distance. Controller-free core loop.
2. **The cube is the product.** The one tool, and the signature moment — "the cube
   deforms the universe". It gets the polish budget (feel, sound, haptics, the visible
   field-warp).
3. **Scale the space and the content together.** One gesture does both; the region is the
   second gesture (job 3), rotate the third (job 4). The field is derived, never edited.
4. **One hard fail, one score.** Time is the window; entropy is the eco grade (the fuel is the
   job's given launch).
5. **Discrete jobs.** ~1 minute each; a **10-minute experience**, about **8–10 levels**.
   Nothing live-ticking.
6. **Relativity is the language.** Not a skin — the cube instruments it.

## 7. The resources: time + fuel + entropy

| Resource | Basis | Role | Failure |
|---|---|---|---|
| **Time** | ship proper time; slowed deep in a well | the **window** `[Tmin, Tmax]` | hard fail (outside it) |
| **Fuel** | the job's **given** launch fuel | the **brief** — make it work, or no money | (the job's difficulty) |
| **Entropy** | the **Wrap's** cost — the edits | the **eco rating** (pay) | soft (lower score) |

**The job hands you the fuel.** "Make it work with this launch fuel, or the money does not add
up." It is fixed per job — a constraint, not a lever.

**Entropy is what you spend.** The Wrap costs entropy: every edit burns it, and the grade is the
entropy you *did not* spend. So the skillful play is to **read the field better and edit less** —
the field's natural assists (falling into a well, slingshotting) are *free*. The greedy play is
the green play ([lore](lore.md)).

**The collapse is the core tension.** As entropy drains the universe visibly **collapses** — the
Wrap spends the *last remaining order* in the neighbourhood. It is the game's central stake: the
cheaper you are, the less of the universe you burn, and the longer it holds together
([lore](lore.md) §2).

**The trade-off.** Edit less (free assists, green) vs edit more (the Wrap buys speed, but burns
entropy). Cheap *and* in the window is the perfect run. Both costs are shown in the prediction
**before the ship travels**.

> **Not in the code yet.** The edits are free in the lab, so the slice scores on **ship-time
> only** ([lens plan](../plans/in-progress/space-shaping-lens.md) §7). Entropy is added once the
> core loop holds.

## 8. Why management is the frame, not a co-equal system

The hybrid (live economy + sculpting) breaks for a specific reason:

- **The constraint cliff.** The docs already name it — degrees of freedom are few
  (source state), while constraints grow as *(ships × destinations)*; when
  constraints outnumber degrees of freedom the level is **unsolvable, not hard**
  ([space-shaping-future](../plans/backlog/gameplay/space-shaping-future.md) §3). A
  management game is *many persistent routes sharing one field* — the worst case.
  Moving one planet can break every route; the only clean resolutions (re-solve,
  auto-resolve, or freeze solved routes) all collapse to **one active delivery at a
  time**.
- **The rhythm clash.** An ambient economy ticking while you deliberately sculpt a
  chaotic field is two hard things at opposite tempos — brutal for a seated,
  first-time demo.

**Conclusion:** solve **one job at a time**; lock or decoratively persist solved
routes; keep the economy *between* jobs. Then the management layer is a wrapper
(identity, progression, contracts) and the sculpting stays solvable.

## 9. Relativity's role

- **Ship clock / proper time** = the *time* resource. Accumulate `dτ` in
  `TrajectoryIntegrator`; the rate falls with depth. Both gestures move it — the **space**
  scale changes travel time, the **content** scale deepens the well — and a clock visibly
  slowing on a dive is a great demo beat.
- **Frame dragging** = the *free* direction change, via a spinning zone — the rotate gesture,
  unlocked at **job 4**.
- **Later:** event horizons (a true fail state).
- Full details: [relativity as gameplay](relativity-as-gameplay.md).

**The guardrail this breaks (deliberate).** v1's simulation was **scale-invariant** —
zooming never changed the result, so the prediction could be trusted across views. This
direction makes **scale physical**: transforming a zone changes what happens inside it.
That is the point of the mechanic. Cost: the prediction must be re-earned (show the
result before the ship travels), and the player must always be able to tell which zone the ship is
in — the ship is always at **map size**; the *zone* changes, never the ship.

## 10. Player verb is continuous; authoring is discrete

- **Player:** the cube is **continuous** and apply-anywhere. No discrete steps, no move
  counts.
- **Score:** a **continuous** value — entropy left (once it exists) — **bucketed to
  stars** (1–3). A coarse readout over a free tool. Move-count scoring is dropped.
- **Authoring:** the only place discreteness lives. Take a **solved arrangement** and
  **deform** it to make the start (the verb is a continuous scale, not reversed moves). The
  solved arrangement is a guaranteed solution; its entropy cost sets the star bar; the size of
  the deformation is the difficulty dial. **The start should fail on time, not crash** — a
  near-miss over the window teaches the goal ("get there *on time*") where a crash reads as
  "broken".

Pipeline: [space-shaping-future](../plans/backlog/gameplay/space-shaping-future.md) §3
(search-and-test, never inversion).

## 11. Risks (condensed)

| Risk | Status / mitigation |
|---|---|
| Toy, not game (no stake) | **Addressed** by the window + entropy grade (§7). |
| The cube is "just a group drag" | Resolved: the cube scale is a **metric stretch** (a shortcut), not a content move — see [lens plan](../plans/in-progress/space-shaping-lens.md) §3. |
| Field illegibility | Make the map *read the field* (straight-line reference, accel arrows); the bubble already shows the warped sheet. |
| Ship-zone legibility | The ship is map-size; show which zone it is in (tint, trail). |
| Prediction trust | Scale is now physical; show the result live, before the ship travels. |
| Identity | **Resolved** by the transport-company pivot and the name "Our Space". |
| Competition scope | Keep one fused seated slice; no sectors/multi-ship/generation until proven. |
| Room-scale legacy drag | Freeze room-scale gameplay; keep only MR bubble anchoring. |
| Content ceiling | Simultaneous multi-ship is "near-chaotic" for generation; authored multi-route is fine (a shared bottleneck is a legibility win). |

## 12. Next step + decision gate

The immediate next step is the
[space-shaping lens plan](../plans/in-progress/space-shaping-lens.md): test whether the
**lens** — scale the space, scale the content — is a *game*, **built in the POC lab scene**
(the harness carries over from the parked test slice), then the game-feel pass (ship travel +
flight + grade), before fusing into the bubble.

1. **Test in the lab, seated, hand-tracked** (no MR/bubble): does the mechanic survive a
   goal and a cost?
2. **Only if the gate passes:** fuse the lab puzzle into the bubble (one seated MR
   experience), add the cube, and author more jobs (entropy/eco grading once it exists).

**Gate:** if the tester solves the puzzles, can say *why*, and wants the next job —
invest in integration, polish, and the identity. If they fiddle blindly, revisit the
mechanic before writing another plan.

## 13. What not to do

- No sectors, multi-ship, or level generation before the §12 gate.
- No further investment in room-scale gameplay or the legacy idle economy; freeze and
  plan the cut.
- No co-equal live-ticking economy under the puzzle.
- No interaction affordances to paper over an illegible field — fix the read.
- No discrete steps or move counts for the *player*; discreteness is for authoring only.
- No second core mechanic (entropy-as-currency, planet-drag). One verb: the scale.
- Do not pitch a systems roadmap; pitch one polished seated MR moment.

## 14. Related documents

- [Relativity as gameplay](relativity-as-gameplay.md) — the effect menu; the ship
  clock, frame dragging, horizons (the cube instruments these).
- [Transport company: time, fuel + entropy](transport-company.md) — the resource model in
  detail.
- [Progression](progression.md) — how the game advances: the numbered-cube network + stars.
- [Lore](lore.md) — the fiction frame: the 2048 stationary-state economy, "the Wrap",
  "revenue is relative", greed-is-eco-friendly.
- [Plans](../plans/README.md) — the engineering plans (space shaping, sectors,
  integration).
