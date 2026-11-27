# Chapter 2 — playtest fixes

**Status:** now · space shaping · updated 2026-11-27 — the non-bake fixes from the 2026-11-27 headset
pass: one ring per route on shared endpoints, Jim names the edge sun, 02_08 parked. Code, prefab, lines
and docs are in, and the fast suites pass 120/120; the 02_01 clip and the headset checks remain. The
bakes moved to [chapter 2 rebakes](chapter-2-rebakes.md).

**Related:** [plans board](../README.md) · [chapter 2 rebakes](chapter-2-rebakes.md) ·
[lessons](../../lessons.md) · [story arc](../../solutions/narrative/story-arc.md) ·
[freelance jobs](../backlog/freelance-jobs.md)

Ids are today's play-order ids (after the 2026-11-26 renumber).

## What the playtest found

- **Shared endpoint:** 02_05's two converging ships share one destination, and 02_01, 02_04 and 02_09
  share an origin. The shared point was grey with one tiny coloured pip per route; the player did not
  see the separate colours.
- **Edge sun:** the only mention was 02_01's card fine print. Jim never said it.
- **Repetition:** 02_07 and 02_08 had the same contract (Clear, one blocker shared by both runs).
- 02_06 cannot be solved and 02_09 plays like the Clear jobs: both are
  [chapter 2 rebakes](chapter-2-rebakes.md).

## Units

### U1 — one ring per route on a shared endpoint

**Code in (2026-11-27):** `InterchangePlanetBehaviour` stacks rings (`routeRing`, `ringPrototype`,
`ringGap`), `RouteEndpoints.RingCount`, `EndpointPools.ringColors`, the `SetMarksShown` hook in
`FieldPlanetBehaviour`, and `RouteEndpointsTests` renamed.

**Done (2026-11-27):** `Interchange.prefab` now has `routeRing` on the existing `Ring`, an inactive
`Ring Prototype` copy as `ringPrototype`, `ringGap = 0.06`, and no `markerPrototype`/`markerRadius`/
`markerScale` or leftover `Marker` object. The fast `SpaceShaping.Tests.EditMode` +
`PuzzleSelection.Tests.EditMode` suites pass 120/120.

**Left:** the headset check (and an editor look at the stacked rings).


### U2 — Jim names the edge sun

**Lines in (2026-11-27):** `level_02_01_init_01` now names the edge sun, and 02_01's manual fine print
no longer repeats it ([jobs](../../design/jobs.md) row 2-1 mirrors it). `Levels_es` has no 02_01
entries.

**Left:**

- Regenerate the `level_02_01_init_01` clip with the `Downloader - OS` batch (same key; the mp3 sits in
  `Assets/StreamingAssets/`), and mirror the strings to the line sheet if it is still authoritative.
- Exit: the clip plays the new line; the card does not repeat it.

### U3 — park 02_08

**Done (2026-11-27):** 02_08 is out of `Chapters/02.asset`, its contract is `Pending` with a "parked"
comment, its asset and lines stay for a freelance job; `FleetJobsTests`, `SectorWireTests` and
`SectorWire.LineCount` (22) follow eight chapter 2 jobs; release, chapters, jobs, vision, progression
and the story-arc note say 21 story jobs. The wire text shifts with the slot: the parked job's beat
(`sector_wire_15`, "TRANSPORT FIRMS LEFT: 1") is gone and the later keys moved down one, so the
warranty line closes chapter 2 and the chapter 3 lines start at the first chapter 3 solve. The fast
suites pass.

**Left:** the headset check. Persistence needs no migration: `RunProgress` is indexed by global job
position and this is pre-release.

## Manual check (headset)

1. 02_05: the shared destination shows two rings in the two ships' colours.
2. Shared origins (02_01, 02_04, 02_09) show one ring per route; three rings read and do not clutter.
3. On a map preview, the endpoint rings hide as before.
4. 02_01: Jim mentions the edge sun; the card does not repeat it.
5. Chapter 2 has eight jobs; 02_08 no longer appears; the wire and coverage counts match.

## Review applied (2026-11-27)

- **Wire countdown:** `sector_wire_13` now reads "TRANSPORT FIRMS LEFT: 1", so the 3 → 2 → 1 line
  still lands after the parked job's beat was removed. Mirror it to the line sheet when that sync runs.
- **Refusal pulse:** `FieldPlanetBehaviour.PulseRefusal` now calls `OnTintApplied(1, danger)` each
  frame, so every stacked ring pulses with the top ring instead of keeping its route colour.
- **Rename:** finished — `InterchangePlanetBehaviour.SetPips` is `SetRings`, its `EndpointPools`
  caller follows, and `RouteEndpoints.RouteAt`/`ColorAt` take `route` instead of `pip`.
- **Left to the user:** commit the prefab wiring, the wire tables in `Global_en` / `Global Shared Data`
  and this plan.

Verified fine: the prefab wiring (`routeRing`, inactive `Ring Prototype`, `Marker` gone), rings stacked
under the scaled root so `ringGap` follows the planet radius, tint and fade repainting every ring after
`NodeBody`, the map preview hiding all rings, the 02_01 line and card matching `jobs.md`, and 02_08's
removal (chapter list, `Pending` contract, data-driven tests, wire keys shifted with no gap).
