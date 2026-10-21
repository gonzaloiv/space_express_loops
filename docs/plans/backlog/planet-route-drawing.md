# Planet Route Drawing — Planet-to-Planet Loops and Round Ship Grants

**Status:** backlog - design only. Replaces the hub/ship-grab route flow. Hubs
and old hub-based saves are not supported; an incompatible snapshot starts a
fresh game.

**Decisions confirmed:** drawing replaces stations; routes are closed loops; the
drawn stroke (simplified and smoothed) is the route; one hand draws and two hands
scale; routes are removed and redrawn, not leg-edited; rounds grant ships to new
or current planets; removing a route refunds its ship immediately; old
hub-based saves are dropped.

**Related:** [Two views + interaction unify](../done/universe-views-and-interaction.md) ·
[Transforming the universe](universe-transform.md)

**Wiring rule:** route lines, draft lines, ship badges, highlight rings, and the
optional pencil are authored in the planet prefab and scene. Do not create
renderers, labels, materials, or interaction proxies at runtime. Follow the
SerializeField wiring rules in the project `AGENTS.md`.

## 1. Goal

Replace "grab a ship at a station and point it at planets" with **drawing a route
directly from planet to planet**.

The player:

1. sees a number on every planet that still has a ship available to open a route;
2. pinches a planet and draws a line to another planet (or holds the optional
   pencil and draws with its tip);
3. repeats until the line returns to the planet where it started;
4. commits the closed loop. One ship leaves the origin planet, the number on that
   planet drops by one, and the ship runs the drawn loop forever.

A planet stays usable for route drawing while it has ships left. It can also be a
stop on routes started elsewhere at any time. The round decides whether more
ships enter the game; see §4.2.

The route is **not complete until it closes**. A route with a single destination
(A -> B) therefore requires two drawn strokes: the outbound A -> B and the return
B -> A. A route with N stops requires N + 1 strokes.

The optional pencil is a physical drawing tool. When it is enabled, the player must
grab the pencil and draw with its tip; the pencil replaces the pinch-point drawing
input and gives the player an explicit, precise drawing prop.

## 2. Non-goals

- Deleting or editing individual legs of a committed route. Remove the route and
  draw it again.
- Automatic pathfinding, obstacle avoidance, or path smoothing around planets.
  The player's drawn line is the route.
- Multiple routes merged into one runner, shared ship pools, or ship transfers
  between routes.
- A new economy for buying ships. New ships come from round data.
- Replacing the universe two-hand scale gesture. It must keep working, but not at
  the same time as a live route stroke. An open draft may scale.
- Redesigning the route panel, score UI, or planet art.
- A general-purpose drawing system outside route drawing.
- The pencil is optional and does not block the core phases.

## 3. Existing facts

- Routes are owned by `SpaceshipBehaviour` and a `TravellerRouteRunner`.
- A route is currently built by `SplineContainerWrapper.Build(hub, destinations)`
  through `SplineLegSampler`:
  - one leg per destination;
  - a final leg back to the hub;
  - a single destination produces an outbound leg and a return leg.
- `RouteLeg` already stores **content-local** `Vector3[] positions` plus the
  `pickupPlanet`. Lines and travellers already work in content-local space.
- `TravellerRouteRunner` loops the legs forever and raises completion after each
  iteration. `EditionState` turns completed loops into letters and money.
- Each ship currently spawns with a `HubBehaviour`; `SpaceshipData.hubId` and
  `LoopData.hubId` anchor it. Destination selection excludes planets already on
  the same route and requires a hub.
- `PlanetBehaviour` currently owns letter storage, route tint, and a
  `PlanetUniverseHandle` used for the two-hand universe scale gesture.
- `PlanetSelectionRules` treats a planet as globally unavailable while it is on a
  route. That is incompatible with reusable planets and is replaced for drawn
  routes by per-route duplicate checks.
- `PlanetRouteColorSync` tints each planet based on the loops that visit it. A
  reusable planet can belong to several routes, so a single planet tint cannot
  represent route membership anymore.
- `LineRendererWithEdges` and `TravellerPathFollower` already accept arbitrary
  content-local point arrays. A drawn polyline can be used directly; the spline
  generator is not required for new routes.
- The scene/prefab components are authored; runtime auto-add and runtime creation
  are not allowed.

## 4. Core model

### 4.1 Route shape

A route is a **directed closed cycle of planets**:

```text
A -> B -> C -> A
```

Rules:

- minimum two distinct planets (A -> B -> A);
- no planet repeats inside the cycle except the closing origin;
- every leg is a drawn stroke from one planet to another;
- the closing stroke must end on the route origin;
- the route cannot be committed before the closing stroke;
- a route with N stops has N + 1 legs.

The ship runs the cycle in order and picks letters at each stop, exactly like the
current loop runner. The route origin is also the route's home/panel anchor.

### 4.2 Ship budget

Ships are a **route-creation resource held by planets**. The round is the only
thing that decides whether more ships enter the game:

```text
RoundData
  shipsToAdd          // MinMaxInt, may be 0
  shipGrantMode       // where the new ships are placed

PlanetData
  shipsRemaining      // current local budget; persisted
```

- Each round adds `shipsToAdd` ships to the game, distributed across active
  planets by `shipGrantMode`.
- Drawing does not consume a ship.
- Committing a closed route consumes exactly one ship from its origin.
- Destinations do not consume ships.
- A planet with `shipsRemaining == 0` cannot start a new route, but can still be
  a destination.
- A route being deleted returns its ship to the origin.
- A ship already running a loop stays with that loop; it does not return to the
  planet each iteration. The number represents ships not yet committed to routes,
  not docked ships.
- If every planet is at zero and the round grants no ships, no new routes can be
  started. Existing routes keep running; removing a route refunds a ship. This is
  a deliberate round-economy state, not a soft-lock.
- The number is shown on the planet while it is greater than zero. At zero, the
  badge dims or hides and the planet cannot start a draft.

`shipGrantMode` is a per-round choice of target:

```text
NewPlanetsOnly          // only planets added by this round
CurrentPlanetsOnly      // only planets that existed before this round
NewPlanetsThenCurrent   // new planets first, remainder on current planets
```

Within the chosen target group, distribute deterministically and never place a
ship on an inactive planet. If the chosen group is empty, the round grants no
ships; that is an intentional round-design choice. `NewPlanetsThenCurrent` is the
default because it makes new content usable without starving the existing board.

This keeps the requirement "if the planet has a remaining ship to create a route
it shows a number, and is usable again for a path", while the round controls how
many more ships exist.

### 4.3 Route draft

Until the loop closes, the player is editing a **route draft**:

```text
RouteDraft
  originPlanetId
  planetIds[]        // visited planets in order, starts with the origin
  legs[]             // drawn strokes between consecutive planets
  color              // reserved for the draft
```

The draft:

- has exactly one active "frontier" planet: the last visited planet;
- keeps every completed leg visible;
- survives between strokes;
- can be undone one leg at a time;
- can be canceled, discarding all legs and releasing nothing (no ship was spent);
- is persisted only when it commits; an unclosed draft is not saved across app
  restarts.

### 4.4 Stroke and geometry

Each stroke is sampled from the drawing pointer into **content-local** points:

```text
pointer pose (world)
  -> contentRoot.InverseTransformPoint
  -> distance/time sampling
  -> light smoothing
  -> simplify (RDP or equal-distance decimation)
  -> trim endpoints to the planet surfaces
  -> RouteLeg.positions
```

Rules:

- the exact drawn stroke is authoritative, but it is simplified and smoothed; do
  not replace it with a canonical spline between planet centers;
- sample in content space so the line survives Room/Bubble scaling;
- minimum world stroke length (`minLegLengthWorld * contentScale`) rejects taps;
- maximum stored points per leg, for example 48; simplify longer strokes;
- after smoothing, re-trim both ends so the line does not enter a planet;
- the route line is the drawn polyline; no spline regeneration for drawn routes;
- a stroke that passes through another planet does not implicitly add it; only the
  release endpoint counts.

### 4.5 Route color

A reusable planet can be on many routes, so:

- route color lives on the **route line and route panel**, not on the planet body;
- the planet body stays neutral for drawn routes;
- the ship badge shows the budget, not route color;
- the draft line uses the next available route color, dimmed until commit;
- `PlanetRouteColorSync` is deleted; it only supported hub routes and
  whole-planet tinting.

## 5. Drawing interaction

### 5.1 Start

A stroke can start only when:

- no route draft is open, or the start planet is the draft's frontier planet;
- the planet is active and has `shipsRemaining > 0` (draft origin only);
- no universe view transition or round spawn is active;
- no active route stroke exists.

Planet hit testing uses an authored `RoutePlanetPicker` layer mask and query:

- start: the pointer must be within `startSnapRadiusWorld` of a planet;
- release: the nearest eligible planet within `snapRadiusWorld` is the target;
- planet body colliders are used; the `UniverseHandles` layer is excluded so the
  two-hand scale proxy cannot be mistaken for a route target;
- no collider or handle is created at runtime.

The start planet is the planet hit by the pinch/pointer at stroke start. It is not
"nearest planet to the hand" once the stroke has begun.

### 5.2 Draw and release

While drawing:

- sample the pointer;
- show the live stroke;
- highlight a valid planet under the pointer with a snap ring;
- show invalid feedback when the pointer is over an ineligible planet.

On release:

| Release target | Result |
|---|---|
| Valid next planet, different from the frontier | Append leg; frontier = target |
| Draft origin, with at least one leg and at least two planets | Close the route and commit |
| Draft origin, with no completed leg | Invalid self-loop; discard stroke |
| Planet already in the draft, not the origin | Invalid revisit; discard stroke |
| Empty space / world geometry / invalid planet | Discard only this stroke; keep the draft |

Appending a leg is atomic: either the whole stroke is accepted or none of it is.
The draft is never left with a partially appended stroke.

### 5.3 State machine

The rule is **one hand draws, two hands scale**:

```text
Idle
  one eligible planet pinched (one hand)       -> Drawing
  second distinct planet pinched (two hands)   -> UniverseScale
  pencil held + tip on eligible planet         -> Drawing

Drawing
  pointer released on valid next planet        -> DraftOpen
  pointer released on draft origin             -> Committing
  pointer released anywhere else               -> DraftOpen / Idle
  second distinct planet pinched (two hands)   -> cancel live stroke, UniverseScale

DraftOpen
  stroke from the frontier planet               -> Drawing
  undo                                        -> DraftOpen (leg removed) / Idle
  cancel                                      -> Idle
  second distinct planet pinched (two hands)   -> UniverseScale

Committing
  route runner created, ship consumed, saved    -> RouteRunning
```

- A single-hand pinch on a planet with ships starts a route stroke. A single-hand
  pinch on a planet without ships does not draw; it can still become the first
  half of a two-hand scale pair.
- A second hand pinching a distinct planet always wins: the live stroke is
  discarded and the existing `UniverseScale` session starts.
- An open draft is **kept** when the universe scales. Draft legs are content-local,
  so they scale and move with the planets automatically. The player can scale
  while thinking and then continue from the frontier planet.
- A live stroke is never committed or appended when the second hand takes over.

### 5.4 Cancel and undo

Route drawing needs explicit recovery:

- a small authored `RouteDraftPanel` follows the frontier planet;
- it shows the origin, the stop count, and two actions: **Undo leg** and
  **Cancel route**;
- the panel uses the existing button/panel style, is authored in the prefab, and
  is not created at runtime;
- an unclosed draft is discarded when the round ends, the level resets, the app
  closes, or the player cancels;
- do not auto-cancel on a short timeout while the player is thinking; only cancel
  on an explicit action or a lifecycle boundary.

### 5.5 Commit

On a valid closing stroke:

1. reserve one ship on the origin (`TryReserveShip`);
2. allocate the route color;
3. create the runner at the origin planet;
4. build legs from the draft;
5. save the route and the new ship count;
6. start the loop;
7. show the route panel and full-loop line;
8. update the origin badge.

If any step fails (origin lost its ship, planet disappeared, save fails), roll
back: release the reserved ship, drop the draft, and show an error state. Do not
start a runner with a partially committed route.

## 6. Planet feedback

### 6.1 Ship badge

An authored `PlanetShipBadge` under the planet prefab:

- shows the remaining ship count as a number;
- uses a small ship icon and a background plate, consistent with
  `ResourcePanel`;
- visible when `shipsRemaining > 0`;
- dimmed or hidden at zero;
- pulses on change (commit or refund);
- faces the camera using the same pattern as the existing planet labels;
- is wired through `PlanetRefs`; no runtime lookup or creation.

The badge is a separate authored object from the letters panel so both can be
visible without overlapping. The exact offset above/below the planet is a prefab
tuning value.

### 6.2 Eligibility and stroke feedback

An authored `PlanetRouteFeedback` on each planet:

| State | Feedback |
|---|---|
| Has ships, idle | subtle ring or glow; badge visible |
| Hovered as a valid start/next stop | ring highlight and snap ring |
| Current draft frontier | stronger pulse ring |
| Draft origin | distinct ring color / marker |
| Invalid target (revisit, wrong planet, no ships) | muted red or gray ring, no snap |
| No ships | badge dimmed; ring disabled |

Use the existing `Outline`/`ScalePunch` components where they already fit, but do
not overload the destination-selection outline state. Route feedback is its own
authored renderer set.

### 6.3 Route line

- Completed routes show the full closed loop.
- The active leg is highlighted while the ship travels it.
- The draft shows all appended legs plus the live stroke.
- Line width compensates for the content scale (same pattern as
  `LineRendererWithEdges`).
- Route line render order must stay above planet surfaces and below UI.
- Multiple routes may overlap; the route color and a small lateral offset per
  route are the first mitigation.

A new authored `RouteLoopView` owns:

- a full-loop `LineRenderer`;
- an active-leg `LineRenderer`;
- optional endpoint/edge markers.

`SplineContainerWrapper` becomes the route-data owner and delegates drawing to
`RouteLoopView`. Remove its `goLine`/`returnLine` serialized renderers or repurpose
them inside the view; there is no second line mode.

## 7. Ships and runners

`SpaceshipBehaviour` remains the visible ship and route runner. It gains a drawn
route entry point instead of a second runner type.

### 7.1 Drawn route mode

```text
StartDrawnRoute(RoutePlan plan)
  set origin planet
  set route color
  build legs from plan
  position ship at origin
  enter SpaceshipRunningState
```

- No destination selection, grab-me panel, station anchor, or ship-grab
  interaction is used. Those components are removed from the ship prefab.
- `SpaceshipIdleState` and `SpaceshipSelectingState` are deleted with the rest of
  the station route flow.
- `SpaceshipRouteSession` only accepts prebuilt legs from a `RoutePlan`.
- `GetGrabbableStationPosition` and the station-anchored visual refresh are
  deleted.
- `TravellerRouteRunner` is unchanged; it already loops arbitrary legs and picks
  letters.

### 7.2 Route panel

- `RoutePanel` is anchored at the route origin or the first leg midpoint, not at a
  hub.
- The panel shows the route color and route id.
- Add a remove action to the panel. Removing a route:
  - stops the runner;
  - hides the ship;
  - removes the loop from the snapshot;
  - removes the route line;
  - refunds one ship to the origin;
  - frees the route color;
  - updates the planet badge.
- Editing a drawn route = remove and redraw.

### 7.3 Route completion and score

- `TravellerRouteRunner` raises the same completion event.
- `EditionState` continues to add letters/money per completed iteration.
- The route keeps looping until removed, exactly like the current game.

### 7.4 Hubs and the old grab flow

Hubs are removed from the game, not retained as a compatibility path:

- delete `HubsSpawner`, `HubBehaviour`, `HubRefs`, `HubData`, and the hub fields
  in `LevelContainer` and `GameSnapshot`;
- delete `SpaceshipIdleState`, `SpaceshipSelectingState`,
  `SpaceshipDestinationSelection`, `DestinationSelector`, `DestinationZone`,
  `GrabbableWrapper`, `GhostBehaviour`, and `GrabZone`;
- delete `SplineLegSampler` and the hub-based `SplineContainerWrapper.Build`;
- remove `hubId`, `destinationIds`, `shouldSpawnSpaceship`, and the ship-grab
  components from data and prefabs;
- `LoopEventArgs` and `SpaceshipData` use `originPlanetId` instead of `hubId`;
- old snapshots containing hubs are detected and reset to a fresh game rather than
  migrated.

## 8. Data and persistence

### 8.1 Planet and round data

`PlanetData` gains:

```text
shipsRemaining
```

`RoundData` gains:

```text
MinMaxInt shipsToAdd
ShipGrantMode shipGrantMode
```

`ShipGrantMode`:

```text
NewPlanetsOnly
CurrentPlanetsOnly
NewPlanetsThenCurrent
```

`PlanetSeedData` does not own a permanent ship budget. The round grants ships to
planets; a fresh level's first round must grant at least one ship so the player
can draw the first route.

`PlanetsSpawner.SpawnPlanets` leaves `shipsRemaining` at its saved value. A new
`LevelContainer.GrantRoundShips(roundData, gameSnapshot)` runs after the planets
spawn and distributes `shipsToAdd` to the target group selected by
`shipGrantMode`. `NewPlanetsOnly` and `CurrentPlanetsOnly` are explicit round
choices; `NewPlanetsThenCurrent` fills the new planets first and puts the
remainder on current planets.

### 8.2 Loop data

`LoopData` gains:

```text
originPlanetId
routePlanetIds[]        // visit order, starts with origin; implicit return to origin
routeLegs[]             // drawn geometry
```

`RouteLegData`:

```text
fromPlanetId
toPlanetId
List<SerializableVector3> localPositions
```

`LoopData` no longer has `destinationIds` or `hubId`. Route identity is
`originPlanetId` plus `routePlanetIds`; one runner per loop.

### 8.3 Snapshot, reset, and persistence

- Increment the snapshot schema version, coordinated with the galaxies plan if
  both are implemented.
- **No migration.** A snapshot without the new route fields, or one containing
  hubs/legacy loops, is incompatible: clear it and start a fresh game.
- On load, a drawn route restores directly from `routeLegs`; it does not
  regenerate geometry from current planet positions.
- Missing or invalid `shipsRemaining` restores as `0`; the next round's grant
  supplies ships. Restoring a route never decrements a count.
- A committed route and the new count are saved together, once.
- Removing a route and the refunded count are saved together, once.
- Unclosed drafts are never saved.

### 8.4 Round flow

`LevelContainer.SpawnRoundInternal`:

```text
SyncIdCounters
SpawnPlanets
GrantRoundShips(roundData, gameSnapshot)   // the round decides new ships
FinalizeSpawn
```

No automatic hub + ship spawn. A round with `shipsToAdd = 0` introduces no new
ships; existing routes keep running and removing a route refunds its ship. The
first round of a level must grant at least one ship. `RespawnFromData` restores
drawn routes from their leg data and never re-grants round ships.

## 9. Interaction arbitration and gates

Route drawing shares planet pinch with the universe scale gesture, so the
arbitration is explicit.

### 9.1 Hand drawing

The hand count is the arbiter:

- **one hand on one planet** starts a route stroke if that planet has ships;
- **two hands on two distinct planets** start the existing universe scale
  session;
- a second hand taking a distinct planet cancels the live stroke before scaling
  starts;
- a single hand on a planet with no ships does not draw, but can still be the
  first hand of a scale pair;
- an open draft does not block scaling; its content-local legs scale with the
  planets;
- a universe transition, round spawn, or route commit animation disables route
  strokes.

No pair-window timer or movement threshold is used. The only remaining distance
check is the minimum stroke length that rejects accidental taps.

### 9.2 Pencil gating (optional)

When the pencil feature is enabled:

- route strokes require the pencil to be held;
- the pencil tip is the only drawing pointer;
- a planet pinch then does not draw and belongs to the universe scale flow
  again;
- a serialized `requirePencilForDrawing` flag on the route drawing controller
  selects the mode; no runtime mode discovery.

## 10. Optional pencil

A physical, authored pencil prop in the scene:

- `RoutePencil` is grabbable through the unified grabbable path;
- it has an authored tip transform and a small authored visual;
- while held, its tip drives the stroke sampler;
- the tip uses the same content-local sampling and planet snapping as hand
  drawing;
- releasing the pencil ends the stroke; the draft stays;
- the pencil can be picked up and put down; it does not teleport;
- haptics/audio on pickup, stroke start, valid snap, invalid snap, and commit are
  polish;
- the pencil is not created at runtime and has no runtime component injection.

The pencil does not add a separate route model; it is an input source for
`RouteStrokeSampler`.

## 11. Component map

New under `Assets/_Game/Features/Spaceships/Scripts/Drawing/`
(assembly `DigitalLove.Game.Spaceships`):

| Type | Responsibility |
|---|---|
| `RoutePlan` | Immutable committed route: origin, ordered planets, legs, color. |
| `RouteDraft` | Mutable draft: origin, visited planets, legs, frontier. |
| `RouteStrokeSampler` | Pointer sampling, content-local conversion, smoothing, simplification, endpoint trim. |
| `RoutePlanetPicker` | Authored layer-mask hit testing for start/end planets; excludes universe handles. |
| `RouteDrawController` | Draft state machine, validation, commit/cancel/undo, grants. |
| `RouteDraftView` | Authored draft line and frontier marker; no runtime creation. |
| `RouteLoopView` | Authored full-loop line, active-leg line, edge markers. |
| `PlanetShipBadge` | Authored badge showing remaining ships; pulse on change. |
| `PlanetRouteFeedback` | Authored eligibility/hover/snap/invalid feedback. |
| `RoutePencil` | Optional grabbable pencil and tip pose. |
| `ShipGrantMode` | Round policy for distributing new ships across planets. |

Modified:

| Type | Change |
|---|---|
| `PlanetData` | `shipsRemaining`. |
| `RoundData` | `shipsToAdd`, `shipGrantMode`; remove `shouldSpawnSpaceship`. |
| `PlanetBehaviour` / `PlanetRefs` | Ship budget, badge/feedback refs, reserve/release methods. |
| `PlanetsSpawner` | Preserve saved ship counts; reset badges. |
| `LoopData` / `GameSnapshot` | Drawn route fields, leg geometry, schema reset, atomic save. |
| `SpaceshipBehaviour` | `StartDrawnRoute(RoutePlan)`; origin planet; ship-grab mode removed. |
| `SpaceshipRouteSession` | Prebuilt legs and drawn-route origin only. |
| `SplineContainerWrapper` | `BuildFromLegs` only; full-loop support. |
| `SpaceshipsSpawner` | Spawn at planet, restore drawn routes, remove/refund, color allocation. |
| `RoutePanel` | Anchor at route, remove action. |
| `UniverseGhostController` | Route-draw arbitration: one hand draws, two hands scale, second hand cancels a live stroke. |
| `LevelContainer` | No hub spawner; grant round ships; restore drawn routes. |
| `EditionState` | Save drawn route plans on commit/remove. |
| `ISpaceshipHost` / `LoopEventArgs` | `originPlanetId` instead of `hubId`. |
| `Game.unity` / planet prefab | Ship badge, route feedback, draft panel, pencil prop, loop line renderers. |

Deleted with the hub flow:

| Type | Reason |
|---|---|
| `HubsSpawner`, `HubBehaviour`, `HubRefs`, `HubData` | No station content. |
| `SpaceshipIdleState`, `SpaceshipSelectingState`, `SpaceshipDestinationSelection` | No ship-grab/destination selection. |
| `DestinationSelector`, `DestinationZone`, `RaycastHelper`, `GrabZone`, `GhostBehaviour`, `GrabbableWrapper` | Ship-grab route creation only. |
| `PlanetRouteColorSync` | Reusable planets are not tinted by route membership. |
| `SplineLegSampler` | No generated hub-to-planet legs. |
| `PlanetSelectionRules` | Replaced by per-route draft validation. |

Assembly note: route drawing belongs to the existing `Spaceships` feature domain
because it owns the route runner and route data. Do not put it in
`DigitalLove.Game.Universe`; that assembly is reserved for universe/galaxy
transforms. `Nodes` must not reference `Spaceships`.

## 12. Phases

### Phase R0 — Route data, geometry, and restore

- Add `RoutePlan`, `RouteDraft`, `RouteLegData`, and the new snapshot fields.
- Add `SplineContainerWrapper.BuildFromLegs` and `RouteLoopView`.
- Add `StartDrawnRoute` and prebuilt-leg support to `SpaceshipRouteSession`.
- Add `shipsRemaining`, round ship grants, and reserve/release methods.
- Add the save schema gate that clears incompatible snapshots.
- Add a debug action that commits a hand-authored route from planet ids and
  sampled legs.

**Exit:** a drawn route (built from data, no gesture yet) renders its full loop,
runs its traveller, picks letters, and round-trips through the snapshot. Old
hub-based snapshots reset cleanly.

### Phase R1 — Hand-drawn drafts and commit

- Add `RouteDrawController`, `RouteStrokeSampler`, `RouteDraftView`,
  `PlanetShipBadge`, and `PlanetRouteFeedback`.
- Add the draft state machine, validation, undo, cancel, and closing commit.
- Add arbitration with `UniverseGhostController`.
- Add the route panel anchor and remove/refund action.
- Add a `RouteDraftPanel` with undo/cancel.
- Commit consumes one origin ship, saves atomically, and starts the runner.

**Exit:** the player can draw A -> B -> A and A -> B -> C -> A with the hand,
cancel or undo a draft, and see the origin badge decrement. A planet with two
ships can start two routes. A planet with zero ships can still be a stop.

### Phase R2 — Remove the station route flow

- New rounds no longer spawn a hub + ship; `RoundData.shipsToAdd` grants ships to
  the target group selected by `shipGrantMode`.
- Remove `shouldSpawnSpaceship` and the new-round ship/hub path.
- Delete the hub content, ship-grab, and destination-selection code and prefab
  components listed in §11.
- Remove `PlanetRouteColorSync`; route color lives on lines and panels.
- Replace the `RoutePanel` edit flow (`EditionState.OnLoopEditionButtonClicked`)
  with the remove-and-refund action.
- Add route removal/refund to the normal UI.
- Clear incompatible snapshots instead of migrating them.

**Exit:** all routes are drawn planet-to-planet. There is no station step, no
ship-grab interaction, and no hub content in the game flow.

### Phase R3 — Optional pencil

- Add the authored pencil prop, `RoutePencil`, tip sampling, and the
  `requirePencilForDrawing` gate.
- Add pickup/put-down behavior, haptics, and audio.
- Verify that planet pinch returns to the universe scale flow when the pencil is
  the drawing tool.

**Exit:** with the pencil feature enabled, lines are drawn only with the pencil
tip; hand pinch on planets scales the universe as before.

### Phase R4 — Polish and verification

- Tune stroke sampling, smoothing, minimum length, snap radius, and line width.
- Tune badge placement, ring feedback, and draft panel placement.
- Tune multi-route line overlap and render order.
- Add automated tests for the pure logic.
- Profile stroke sampling and snapshot size.

**Exit:** drawing is readable at Room and Bubble scale, save/restore is stable,
and the new code has no per-frame allocations or LINQ.

## 13. Test plan

### Manual

1. A planet with two ships shows `2`; committing a route changes it to `1`.
2. Drawing A -> B leaves a draft with one leg; B -> A commits and launches a ship.
3. A single stroke A -> A is rejected and leaves no draft.
4. Releasing on empty space discards only that stroke; the draft remains.
5. Releasing on an already visited planet is rejected.
6. A route with three stops requires three legs and closes on the fourth.
7. Undo removes the last leg; cancel removes the whole draft and spends no ship.
8. A planet with zero ships cannot start a draft but can be a stop.
9. A planet with one remaining ship can start a second route after the first
   commit only if it still has budget; otherwise it cannot.
10. One hand on a planet draws; two hands on two planets scale.
11. A second hand grabbing a distinct planet during a stroke cancels the stroke
    and starts scaling; an existing draft is preserved.
12. Removing a route removes its line and ship, refunds the origin ship, and frees
    its color.
13. Room <-> Bubble: the draft, completed lines, badges, and snap radius stay
    attached and readable.
14. Restoring a snapshot restores counts, routes, and leg geometry without a
    second decrement.
15. An incompatible old hub snapshot resets to a fresh game instead of restoring
    hubs or broken loops.
16. A new round grants `shipsToAdd` ships according to `shipGrantMode` and does
    not spawn a hub; a round with zero adds no ships.
17. The optional pencil draws only from its tip; releasing it ends the stroke.
18. Route lines remain legible when several routes share a planet.

### Automated

- Stroke sampler: content-local conversion, min length, point cap, endpoint trim,
  determinism for a fixed input.
- Draft validation: start eligibility, distinct endpoint, no revisit, closing
  origin, minimum two planets, N + 1 legs.
- Ship grants: distribution by mode, reserve/release, no double spend, refund on
  remove, restore does not decrement or re-grant.
- Snapshot round-trip for drawn routes; incompatible snapshot reset gate.
- Route plan -> legs -> loop view/traveller.
- Arbitration: one-hand draw, two-hand scale, second-hand live-stroke cancel,
  draft preservation.
- No per-frame allocations and no LINQ in new code.

## 14. Risks and decisions

| Risk / decision | Decision |
|---|---|
| Station vs planet origin | New routes start at planets. Hubs, ship grab, and destination selection are deleted; no hub content remains in the game flow. |
| One-stop route | A route with one destination is a two-stroke loop, A -> B -> A. A single unclosed stroke is never a route. |
| Ship meaning | A ship is a route-creation allowance added by rounds. It is consumed on commit and refunded on route removal, not on each loop. |
| Ship supply | The round decides how many new ships enter the game (`shipsToAdd`) and whether they target new planets, current planets, or both (`shipGrantMode`). If the round grants none and every planet is at zero, no new route can start until a route is removed or a later round grants ships. |
| Reusable planets | A planet can be on many routes; only the same-route duplicate rule applies. |
| Planet tint | Reusable planets are not tinted by route membership; route color lives on the line and panel. |
| Freehand geometry | The drawn polyline is the route. It is sampled in content space, smoothed, simplified, and trimmed at both endpoints. |
| Endpoint snapping | Only the release endpoint selects a planet. Passing through a planet does not add it. |
| Draft persistence | Drafts survive between strokes but are not saved across app restarts. They are canceled explicitly or at lifecycle boundaries. |
| Recovery | Undo one leg and cancel the whole draft are required. A committed route is removed and redrawn, not leg-edited. |
| Two-hand scale | One hand draws, two hands scale. A second hand on a distinct planet cancels the live stroke and starts scaling; an open draft is preserved and scales with the content. |
| Pencil | Optional. When enabled it becomes the drawing pointer and removes the planet-grab ambiguity. |
| Save size | Cap stored points per leg and simplify before saving. Do not store drafts. |
| Save compatibility | No migration. A snapshot without the new route fields, or one containing hubs, is cleared and starts a fresh game. Restoring a drawn route never decrements a count. |
| Line overlap | Color plus a small per-route lateral offset; deeper layout is future work. |
| Line crossing planets | Allowed for now; lines render above planets. Revisit if it reads badly. |
| Color exhaustion | `SpaceshipsSpawner` currently has a finite color set. Removing a route frees its color; running out is a gameplay tuning risk. |
| Route panel overlap | Anchor at the origin or first-leg midpoint; multiple routes may still collide. Tune offset in R4. |

## 15. Defaults

| Parameter | Default |
|---|---|
| `shipsToAdd` per round | `0-2`; the first round must grant at least `1` |
| `shipGrantMode` | `NewPlanetsThenCurrent` |
| `minLegLengthWorld` | `0.04 m` world |
| `maxStoredPointsPerLeg` | `48` |
| `strokeSampleDistance` | `0.01 m` content space |
| `strokeSmoothing` | 3-point moving average, one pass |
| `startSnapRadiusWorld` | `0.045 m` world, with a planet-radius floor |
| `snapRadiusWorld` | `0.035 m` world, with a planet-radius floor |
| `planetSurfaceClearance` | `1.05` x planet radius |
| `draftLineWidth` | same as route line, dimmed |
| `routeLineWidth` | existing route width |
| `routeOverlapOffset` | `0.003 m` per active route index |
| `draftPanelOffset` | above the frontier planet badge |
| `requirePencilForDrawing` | `false` until R3 |
| `routeDraftCancel` | explicit panel action only |

## 16. Open questions

1. **Should `NewPlanetsOnly` fall back to current planets when the round added no
   planets?** Default: no, it grants zero; that is an explicit round-design choice.
2. **Can two routes share the same directed legs?** Default: yes, the line offset
   and color distinguish them. If that reads badly, reject exact duplicate legs.
3. **Should the pencil be a hard requirement for all route drawing once it ships,**
   or a mode the player can choose? Default: a mode controlled by
   `requirePencilForDrawing`.
4. **Should an unclosed draft survive the round transition?** Default: no; discard
   it when the round ends or the level resets.
