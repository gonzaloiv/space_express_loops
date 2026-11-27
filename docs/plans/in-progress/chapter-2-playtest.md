# Chapter 2 — playtest fixes

**Status:** now · space shaping · updated 2026-11-27 — the non-bake fixes from the 2026-11-27 headset
pass: one ring per route on shared endpoints, Jim names the edge sun, 02_08 parked. Code, lines and
docs are in; the Interchange prefab wiring, the 02_01 clip and the test runs remain. The bakes moved to
[chapter 2 rebakes](../backlog/chapter-2-rebakes.md).

**Related:** [plans board](../README.md) · [chapter 2 rebakes](../backlog/chapter-2-rebakes.md) ·
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
  [chapter 2 rebakes](../backlog/chapter-2-rebakes.md).

## Units

### U1 — one ring per route on a shared endpoint

**Code in (2026-11-27):** `InterchangePlanetBehaviour` stacks rings (`routeRing`, `ringPrototype`,
`ringGap`), `RouteEndpoints.RingCount`, `EndpointPools.ringColors`, the `SetMarksShown` hook in
`FieldPlanetBehaviour`, and `RouteEndpointsTests` renamed.

**Left:**

- `Interchange.prefab` (`Assets/_Game/Features/Puzzles/Models/`) still serializes `markerPrototype`,
  `markerRadius` and `markerScale` and has no `routeRing` / `ringPrototype`. In the editor: assign
  `routeRing` to the existing `Ring` renderer, add one inactive copy of it as `ringPrototype`
  (`m_IsActive: 0`, the old `Marker` is the precedent), delete the `Marker` object, save. Let Unity
  write the prefab; do not hand-type references.
- Run the fast `SpaceShaping.Tests.EditMode` suite.
- Exit: in the editor, 02_05's shared destination shows two rings in the two route colours, 02_09's
  three-ship origin shows three, and a map preview hides all of them.

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
and the story-arc note say 21 story jobs.

**Left:** run the fast suite (`FleetJobsTests`, `SectorWireTests`). Persistence needs no migration:
`RunProgress` is indexed by global job position and this is pre-release.

## Manual check (headset)

1. 02_05: the shared destination shows two rings in the two ships' colours.
2. Shared origins (02_01, 02_04, 02_09) show one ring per route; three rings read and do not clutter.
3. On a map preview, the endpoint rings hide as before.
4. 02_01: Jim mentions the edge sun; the card does not repeat it.
5. Chapter 2 has eight jobs; 02_08 no longer appears; the wire and coverage counts match.
