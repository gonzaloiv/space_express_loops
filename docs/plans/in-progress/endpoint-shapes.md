# Endpoint shapes — origin, destination and interchange rings

**Status:** now · presentation · updated 2026-11-11 — split from transit routes (closed 2026-11-11),
whose route colours, line-end outcomes and keep-out landed. The fleet episode's fan-out and converge
jobs share endpoints, so this is wanted before that episode is captured. Review applied 2026-11-11:
positions come from the session, one pool per role, pips only in play.

**Related:** [fleet episode](../backlog/fleet-episode.md) · [the cube](../../design/systems/cube.md)
§Feedback · [plans board](../README.md)

## Goal

Each ship's endpoint planets already carry its route colour, and a shared endpoint takes the first
ship's colour. Origin and destination still look alike, and a planet two routes share reads as one
route's.

## Contract

- **Origin and destination differ by shape, not colour:** the origin a filled ring, the destination an
  open ring, both in the route colour. New authored art, not a tint.
- **A shared endpoint is an interchange:** a neutral ring with one small pip per route colour, like a
  station on a transit map. An endpoint that is one ship's origin and another's destination is an
  interchange too.
- **Endpoints sit where the session puts them.** Separation can push an endpoint off its authored point
  (`SourceConstraints`), so every endpoint view spawns at `session.GetPosition` / `GetScale` by its
  source index, the way `FieldPlanetSet` places planets today. `RoutePlan` supplies only role, pips and
  colour, never a position.
- **Pips only in play; notches on the map.** On the map (`PreviewSize` 0.16 m) a coloured pip would be
  1–2 mm, so the dioramas show the interchange's markers neutral: the same authored markers, one per
  route, read as a station's notch count. The ring shapes still tell origin, destination and
  interchange apart.
- **Authored, not invented.** Rings and pips are authored prototypes, pooled; runtime code places and
  tints them with property blocks, never `.material`.

## Units

### U1 — `RouteEndpoints`, the derivation

- A plain `RouteEndpoints` type built from the `RoutePlan` and the session: one entry per distinct
  endpoint with its **source index** (`session.IndexOf`, the pattern in `FieldPlanetSet`), its role
  (origin, destination, interchange) and its route colours. Endpoints match by exact `Vector3` equality
  (safe: the baked assets share identical floats, e.g. `TwoShips_FanOut`).
- It also answers **the primary destination's entry**, so the solve payoff can find its view.
- Tests (fast suite): a single ship yields one origin and one destination (the common case); two
  separate ships yield 2N endpoints; a fan-out yields one interchange with two pips; a three-route
  interchange has three pips; an endpoint that is one ship's origin and another's destination is an
  interchange; the primary destination resolves on a converge job, where it is an interchange.
- **Exit:** the fast suite passes with the cases above.

### U2 — the endpoint pools and the shapes

- **One pool per role** behind an endpoint collaborator on `FieldPlanetSet` (`ViewPool<T>` binds one
  prototype; `NodeBody` has one renderer and one route-colour slot): origin, destination and
  interchange. `FieldPlanetSet` stays under 250 lines; the collaborator owns the pools.
- **Pips** are authored markers inside the interchange prototype, placed ring-local and evenly spaced
  at runtime, capped at four (the palette size). In play they carry the route colours; on the map they
  stay neutral so the count reads as notches. The ring and marker art is authored at the sphere's
  1 m-diameter footprint (0.5 local radius), so `NodeBody.SetRadius` keeps the capture radius.
  `Origin.prefab` and `Destination.prefab` are re-authored in place (both roots already reference them;
  GUIDs stay); `Interchange.prefab` and the marker are new.
- **The prototypes travel as one group.** `Prototypes` would grow past five constructor parameters, so
  the endpoint prototypes become one serialized group, wired in both roots (`SpaceShapingSpawner` and
  `FieldContentView`).
- `SpawnSources` excludes every endpoint index; the bulk passes (`SetLabelsVisible`, `SetStateDim`,
  `SetMapFade`, `ClearMapFade`, `ShowOnlySunLabel`, `Hide`) cover the pools.
- **The refused-move pulse follows the endpoints:** `PulseEndpoint` is re-pointed at the pools, and each
  pooled view spawns with its `SourceIndex` (`FieldPlanetBehaviour`), so the pulse still finds a pushed
  endpoint.
- **`Destination` resolves through `RouteEndpoints`:** the primary ship's destination view, which is an
  interchange on a converge job, before `PlayArrival` / `PlayStamp` read its `Radius`.
- Docs: [the cube](../../design/systems/cube.md) §Feedback's route-colour paragraph gets the shapes and
  the interchange.
- **Exit:** a fan-out or converge job reads as an interchange; the solve payoff still lands on the
  primary destination; a pushed endpoint keeps its ring.

## Not in this plan

- `ContentFade` measures only the primary segment, so the lattice around ship 2's route can fade. Fix
  it here if the headset shows it.

## Manual check (headset)

1. Single-ship job: origin and destination tell apart by shape, in the route colour; nothing else
   changed.
2. Two-ship job: each ship's origin and destination read by shape and colour.
3. Fan-out and converge jobs: the shared planet shows a neutral ring with one pip per route colour.
4. Grow a planet next to an endpoint until separation pushes it: the ring moves with its planet.
5. Push the cube onto an endpoint: the refused-move pulse plays on that endpoint's ring.
6. A converge solve: the arrival flash and the stars land on the shared destination.
7. The map dioramas show the rings with neutral notches (one per route) and read the same as play.
8. Tuning: pip size and spacing on the interchange at arm's length; ring thickness against the lattice.
