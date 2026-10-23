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
from one star to another, with a **deadline** and a **fuel grade**. Your one verb is
**shaping spacetime** — moving and scaling the masses in the bubble so the ship's
curved path delivers the mail on time and without wasting fuel. Relativity is the
language: a ship's clock slows deep in a gravity well, and passing near mass gives
you speed you did not pay for.

## 2. Where the project is

| Layer | State | Evidence |
|---|---|---|
| Legacy room-scale loop (planets in your room, letters → money → rounds, grab-a-ship → route) | Built, in `Game.unity`; room-scale gameplay is **legacy** | `Flow/EditionState/EditionState.cs`, `Features/Nodes`, `Features/Spaceships`, `LevelContainer.cs:17-18,198-213` |
| Room ↔ Bubble scale transition | Done, validated in headset; **the signature interaction** | `Features/Universe/View/UniverseView.cs`, [universe-views-and-interaction](../plans/done/universe-views-and-interaction.md) |
| Space Shaping physics puzzle | POC in a lab scene, not fused with the bubble | `Features/SpaceShaping/Scenes/SpaceShapingPOC.unity`, `Puzzles/SamplePuzzle.asset` |
| Map/Sculpt apertures over one frame | Design only | [space-shaping-future](../plans/backlog/gameplay/space-shaping-future.md) §2 |
| Space Shaping → game integration | Not started | [space-shaping-integration](../plans/backlog/gameplay/space-shaping-integration.md) |
| Sector generation | Pure math done (G0) | [universe-sector-generation](../plans/in-progress/universe-sector-generation.md) |

**Summary:** the strong interaction (a bubble in front of you, scaled by two hands)
already works. The gravity puzzle exists beside it but is not fused with it. The
room-scale game is dead weight for this target. The project is short on **one fused,
playable seated slice**, not on parts.

## 3. The pivot: transport company

From an idle "routes and rounds" loop to an **interstellar transport company**:

- **Map = the company's network.** Star systems, route lines, distances (the
  "commercial interstellar route map" is the campaign view).
- **A job = one delivery** across the network (A → B, or A → B → C), with a
  deadline and a fuel grade.
- **The verb = shaping spacetime** to make the delivery work efficiently.
- **Progression = the company:** unlock systems, buy ships, earn reputation —
  *between* jobs, never ticking live under the puzzle.

This drops the idle economy (the old loop's main problem for a puzzle) and keeps
exactly what makes the fantasy work: mail/express, a map, a fleet, a reputation.

## 4. Mixed reality: why the bubble is the point

"Our Space" is a pun and a design brief: the universe lives in **your** space.

- **Core (keep):** MR anchoring — the bubble sits in your real room (over a desk, a
  table, in front of the sofa), placed and scaled by two hands. Passthrough +
  hand tracking is the Meta differentiator, and MRUK is already wired
  (`UniverseView.roomAnchor` / `bubbleAnchor`).
- **Legacy (cut):** room-scale *gameplay* — planets scattered around your physical
  room, walking to them, room-anchored content placement
  (`MrukRoomLocalPlacement`, `LevelContainer` room pose). A seated player does not
  walk to a planet.
- **The two views that survive** are **Map (wide) and Sculpt (narrow) apertures of
  the bubble**, not Room vs Bubble.

So: the *room* is the backdrop and the anchor; the *bubble* is the playfield.

## 5. Design pillars

1. **Seated, hand-tracked, MR.** Everything interactive sits in a comfortable
   arm-reach cone at bubble distance. Controller-free core loop.
2. **The bubble is the product.** The Map ↔ Sculpt aperture transition is the
   signature moment and gets the polish budget (feel, sound, haptics, the visible
   field-warp).
3. **One verb: shape spacetime.** Move/scale masses (later: spin, horizons); the
   field is derived, never edited.
4. **Two resources, one hard fail.** Time is the deadline; fuel is the grade.
5. **Discrete jobs.** 1–3 minutes each; a career of them; nothing live-ticking.
6. **Relativity is the language.** Not a skin — it is how the two resources work.

## 6. The two resources: time + fuel

Structured **asymmetrically** so the puzzle stays crisp:

| Resource | Physical basis | Role | Failure |
|---|---|---|---|
| **Time** | ship proper time; slowed deep in a well | the **deadline** | hard fail (late) |
| **Fuel** | launch energy `k · launchSpeed²` | the **efficiency grade** (eco rating, pay) | soft (lower score) |

**Why fuel is the missing piece.** The launch is Coast — an initial velocity, no
burn (`ShipLaunch.cs`). In a Coast + well model the ship *exchanges energy with the
field for free*: it falls into a well and comes out faster or redirected. That is a
**gravity assist**, and it already emerges from the current physics. Without a fuel
cost there is no reason to ever launch slow — so the field would be decoration. With
fuel, the skillful play is to **launch cheap and let the field give you the speed
you did not pay for**. The resource and the mechanic justify each other.

**The trade-off.** Fast but thirsty (launch fast) vs cheap but slow (launch slow and
sculpt the field to gain speed and cut the path). Cheap *and* on time is the perfect
run. Both costs are shown in the prediction **before** commit, so the choice is
informed, not a lottery.

**The ethic framing.** Fuel as an **eco rating** makes it a *green transport
company*: the point is not wasting energy. Distinctive, wholesome, competition-ready.

## 7. Why management is the frame, not a co-equal system

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

## 8. Relativity's role

- **Ship clock / proper time** = the *time* resource. Accumulate `dτ` in
  `TrajectoryIntegrator`; rate falls with depth (and optionally speed). A clock
  visibly slowing on a dive is a great demo beat.
- **Gravity assist** = the *fuel-free* speed, already emergent from Coast + wells.
  This is what makes sculpting skillful.
- **Later:** frame dragging (spinning planets swirl the path — adds *direction* to
  the puzzle) and event horizons (a true fail state).
- Full details: [relativity as gameplay](relativity-as-gameplay.md).

## 9. Risks (condensed)

| Risk | Status / mitigation |
|---|---|
| Toy, not game (no stake) | **Addressed** by deadline + fuel grade (§6). |
| Field illegibility | Make Map *read the field* (accel arrows, straight-line reference, desire line); the bubble already shows the warped sheet. |
| Identity | **Resolved** by the transport-company pivot and the name "Our Space". |
| Competition scope | Keep one fused seated slice; no sectors/multi-ship/generation until proven. |
| Room-scale legacy drag | Freeze room-scale gameplay; keep only MR bubble anchoring. |
| Content ceiling | Multi-ship is "near-chaotic"; do not promise it. |

## 10. Next step + decision gate

The immediate next step is the
[space-shaping test slice](../plans/in-progress/space-shaping-test-slice.md): the
smallest change set (ship clock + deadline + HUD + a few puzzles) to test whether
space-time editing is a *game* — **built in the POC lab scene, not by integrating
into the game or fusing into the bubble.**

1. **Test in the lab, seated, hand-tracked** (no MR/bubble): does the mechanic
   survive a goal and a cost?
2. **Only if the gate passes:** fuse the lab puzzle into the bubble (one seated MR
   experience), add fuel/eco grading and the network map, and author more jobs.

**Gate:** if the tester solves the puzzles, can say *why*, and wants the next job —
invest in integration, polish, and the identity. If they fiddle blindly, revisit the
mechanic before writing another plan.

## 11. What not to do

- No sectors, multi-ship, or level generation before the §10 gate.
- No further investment in room-scale gameplay or the legacy idle economy; freeze
  and plan the cut.
- No co-equal live-ticking economy under the puzzle.
- No interaction affordances to paper over an illegible field — fix the read.
- Do not pitch a systems roadmap; pitch one polished seated MR moment.

## 12. Related documents

- [Relativity as gameplay](relativity-as-gameplay.md) — the effect menu; the ship
  clock, frame dragging, horizons.
- [Transport company: time + fuel](transport-company.md) — the two-resource model in
  detail.
- [Lore](lore.md) — the fiction frame: the 2048 stationary-state economy, "the Wrap",
  "revenue is relative", greed-is-eco-friendly.
- [Plans](../plans/README.md) — the engineering plans (space shaping, sectors,
  integration).
