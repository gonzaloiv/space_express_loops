# Universe — Galaxies, Level/Round Creation, and Two-Hand Group Transform

**Status:** follow-up to the two-views and universe-transform plans. Requires the
content-local route work (Phase A0) from [Two views](universe-two-views.md).

**Related:** [Two views](universe-two-views.md),
[Transforming the universe](universe-transform.md)

## 1. Goal

Make **galaxies** first-class content groups and let the player rotate and scale a
galaxy with two hands.

A galaxy:

- is created by the level/round spawning flow, not authored as a fixed scene group;
- contains one or more planets and may contain hubs;
- has a local position, yaw, and uniform scale under the existing `Level` root;
- can be picked with a dedicated ring handle and transformed with two hands;
- previews the target transform as a ghost, then commits on release;
- keeps its transform when the universe switches between Room and Bubble;
- is saved in the game snapshot and can be reset to its spawn pose.

Scaling a galaxy also changes the **layout** around it: shrinking pulls nearby
galaxies closer, growing pushes them farther away. The grabbed galaxy keeps its
position; neighboring galaxies move so the universe stays coherent. This is a
layout response, not just a local collision clamp.

Ships are **not** part of a galaxy's scalable content. They stay at `Level` scale;
only their station anchors and route geometry adapt after a galaxy transform.

The whole-universe transform from [Transforming the universe](universe-transform.md)
continues to exist. A galaxy transform composes underneath it: the universe moves
`Level`, while a galaxy transform changes that galaxy's local pose and the positions
of its neighbors in the layout.

## 2. Non-goals

- Moving the grabbed galaxy itself during the gesture. Its position is decided by
  level/round generation. Neighboring galaxies do move, driven by layout
  propagation.
- Scaling ships or route lines with a galaxy. Ships are `Level`-scale by design.
- Pitch/roll rotation. Galaxies rotate on Y only, like the universe.
- Non-uniform scale.
- Player-created, merged, split, or deleted galaxies.
- Nested galaxies/systems beyond one galaxy level.
- A full content ghost (UI, VFX, letters, ships) or a render-texture preview. The
  ghost uses cheap planet proxies and a bounds ring.
- Reparenting nodes between galaxies at runtime, except during spawn/restore.
- Redesigning the UI. Labels scale with the galaxy by decision; readability fixes
  are a later task.

## 3. Existing facts

- `Level` is the root containing `Hubs`, `Planets`, and `Spaceships`.
  `UniverseView` owns the `Level` transform `D`.
- Planets and hubs are currently flat under their spawner roots. Their saved
  `localPosition` is `Level`-local:
  - `PlanetData.localPosition`
  - `HubData.localPosition`
- Round flow:
  - `RoundData` is a `ScriptableObject` per round (`Assets/_Game/Features/Levels/RoundData.cs`).
  - `LevelContainer.SpawnRound` spawns hubs, planets, and ships.
  - `NewRoundState` advances the round and calls `SpawnRound`.
  - `GameSnapshot` persists planets, hubs, loops, and round index.
- Route data is already `Level`-local in the current tree:
  - `SplineLegSampler` converts sampled world positions with `contentRoot`.
  - `TravellerPathFollower` converts with `contentRoot` and `pathSpace`.
  - `LineRendererWithEdges` uses local positions.
  - This is the prerequisite that lets routes survive `Level` transforms. A galaxy
    transform still requires a route rebuild, because a galaxy is a child transform.
- `TravellerRouteRunner` already has a paused state used during universe view
  transitions.
- Interactions use the project's grabbable abstraction. The grabbables are being
  unified; this plan targets the unified API rather than one SDK-specific
  `Grabbable`. `GrabbableWrapper` is the existing example of the event pattern.
- There is no galaxy content model today. `Assets/_Game/Features/VFX/Galaxy/GalaxySpace.cs`
  is a room-wide particle shell, not a per-galaxy group, and must not be reused as
  the galaxy visual.
- `Assets/_Game/Features/Universe/` is reserved for the universe/galaxy transform
  layer from the two-views plan.

## 4. Core model

### 4.1 Hierarchy

```text
Level (UniverseView D)
  Galaxies
    Galaxy (local position, yaw, uniform scale)
      GalaxyView          visual nebula/ring, no gameplay
      GalaxyGrabHandle    ring/cage with two grab points
      Hub
      Planet
      Planet
  Hubs (legacy/fallback only)
  Planets (legacy/fallback only)
  Spaceships
  Route splines and route lines
```

Spaceships stay at `Level` level. They are not children of a galaxy, because a
route can span galaxies. Ships do not scale with a galaxy; routes and station
anchors are refreshed after a galaxy changes. Routes are rebuilt after a galaxy
changes.

### 4.2 Spaces

| Space | Meaning |
|---|---|
| World (W) | Physical room, camera, hands, tracking. Meters. |
| Level/content (C) | `Level` local space. Universe transform `D`. |
| Galaxy (G) | Galaxy local space. Galaxy transform `G` (position, yaw, uniform scale). |

```text
world = D.position + D.rotation * (D.scale * (G.position + G.rotation * (G.scale * galaxyLocal)))
```

`D` stays owned by `UniverseView`. A galaxy transform never touches `D`.

### 4.3 Galaxy data

`GalaxyData` is the saved and runtime identity:

```text
id
localPosition          Level-local position; fixed for the grabbed galaxy,
                       changed for neighbors by layout propagation
localYaw               Y rotation in degrees
localScale             uniform local scale
defaultLocalPosition   spawn pose used by Reset
defaultLocalYaw
defaultLocalScale
radius                 galaxy-local bounding radius at scale 1
transformable          false only for the migrated legacy root galaxy
roomFitEnabled         false only for the migrated legacy root galaxy
```

`PlanetData` and `HubData` gain `galaxyId`. A node's `localPosition` becomes
**galaxy-local** for new snapshots. This is a deliberate semantic change; legacy
migration is covered in §5.4. New galaxies set `transformable = true` and
`roomFitEnabled = true`; only the migrated legacy root sets them false.

Membership is fixed once a round is spawned. Nodes are not reparented while a
round is running. Galaxy positions are not immutable: neighbors can be displaced by
layout propagation (§8.5).

### 4.4 Galaxy bounds

`GalaxyBehaviour` computes a galaxy-local bounding sphere from its active members:

```text
center = average of member centers (or bounds center)
radius = max(distance(member, center) + member radius)
```

The stored `GalaxyData.radius` is the seed radius used before members exist; after
spawn, the runtime bounds take over when they are larger. Bounds are used by:

- the ghost ring and proxy placement;
- the room-fit scale limit;
- the level content bounds if the bubble radius needs to include galaxies.

## 5. Level and round creation

This is the part that makes galaxies part of the normal game flow rather than a
manually placed scene object.

### 5.1 Round data

`RoundData` gains:

```text
MinMaxInt galaxiesToAdd       // 0 means "add planets to existing galaxies"; max is exclusive
GalaxySeedData galaxySeed     // radius, spread, yaw, scale limits
```

`GalaxySeedData` (new, modeled after `PlanetSeedData`):

```text
MinMaxFloat radius            // galaxy radius at scale 1, e.g. 0.22-0.35 m
MinMaxFloat planetSpread      // fraction of radius used for planet placement
MinMaxFloat yaw               // spawn yaw range
MinMaxFloat defaultScale      // usually 1
MinMaxFloat minScale
MinMaxFloat maxScale
int maxPlanetsPerGalaxy       // capacity guard; default 6
int maxHubsPerGalaxy          // capacity guard; default 2
```

All fields are nullable class references (`MinMaxInt`/`MinMaxFloat` are classes).
The spawner must tolerate old `RoundData` assets with missing values and apply a
serialized fallback `GalaxySeedData` (assigned on the spawner or round selector),
not a runtime-created `ScriptableObject`. The first round of a level must create at
least one galaxy; if the snapshot has no galaxies, the spawner creates one even
when `galaxiesToAdd` is zero.

### 5.2 Spawn order

`LevelContainer.SpawnRoundInternal` becomes:

```text
1. SyncIdCounters
2. galaxiesSpawner.PlanRound(roundData, snapshot)
3. SpawnSpaceship (hub + ship)       // hub is assigned to a galaxy
4. SpawnPlanets                      // planets are assigned to galaxies
5. FinalizeSpawn
```

`GalaxiesSpawner.PlanRound`:

- creates `galaxiesToAdd` new galaxies and their `GalaxyData`;
- assigns the new round's hub to a galaxy;
- distributes the new planets across the new and existing galaxies;
- returns the assignments used by `HubsSpawner` and `PlanetsSpawner`.

Default distribution:

- fill new galaxies first, then existing galaxies round-robin;
- assign the new hub to the new galaxy with the fewest hubs, otherwise to the
  galaxy with the fewest hubs;
- keep at least one planet in a newly created galaxy when the round has enough
  planets; if not, allow an empty galaxy and let the next round fill it;
- respect `maxPlanetsPerGalaxy` and `maxHubsPerGalaxy`;
- if every galaxy is full, create another galaxy when room capacity allows;
  otherwise overflow the least-full galaxy and log a warning. Do not silently
  drop nodes.

The assignment must be deterministic for a given snapshot so restore does not
change membership. Prefer a seeded assignment based on round index and galaxy
order, not scene iteration order.

After all members for the round have spawned, recompute galaxy bounds and room-fit
limits. A galaxy that gained distant members may need its current scale clamped
down; do this once at the end of `SpawnRound`, not per node.

### 5.3 Placement

- `GalaxiesSpawner` places each new galaxy in `Level` space through
  `MrukRoomLocalPlacement` using the galaxy radius, and registers the galaxy volume
  as an occupant.
- The first galaxy of a level uses `GetRoomCenterLocalPosition(radius)`.
- Later galaxies use `GetValidLocalPosition(radius)`, with a fallback to the room
  center when MRUK has no valid position.
- Planets and hubs inside a galaxy are placed in **galaxy-local** space with a new
  `GalaxyPlacement` helper (a small seeded sphere/ring placement with minimum
  separation). Do not call `MrukRoomLocalPlacement` for each planet; the galaxy
  volume is the room-level occupant.
- A hub is placed near the galaxy center or on a seeded offset so it is always
  reachable and does not overlap planets. The exact offset is a tuning knob.
- Galaxy local positions are constrained so the galaxy bounds fit inside the room
  in Room state; the room-fit scale limit is computed at spawn and after an MRUK
  anchor relocation.
- After members spawn, recompute the galaxy bounds. If the runtime radius exceeds
  the seed radius registered with `MrukRoomLocalPlacement`, update the occupant
  radius (unregister/register) so later galaxies do not overlap it.
- Cap new galaxies at the room capacity (`galaxyMaxPerRoom`). If a requested galaxy
  cannot be placed, do not leave orphan planets: clamp the count and assign the
  extra planets to existing galaxies.
- Validate that the room-fit maximum scale is at least `minScale` at the chosen
  position. If not, try another position, reduce the seed radius, or skip the new
  galaxy and assign its planets to existing galaxies. Never create a galaxy that is
  clamped below its own minimum on the first frame.
- Validate the spawn layout with the same layout solver used by the gesture
  (§8.5): galaxies must be able to coexist at `minScale` without overlap and inside
  the room. Prefer another position over silently accepting an impossible layout.
- Keep the registered room occupancy in sync with the committed galaxy scale
  (`radius * localScale`), so later rounds place new galaxies against the actual
  footprint.
- `MrukRoomLocalPlacement.SyncFromSnapshot` must register galaxy volumes as the
  room occupants. Per-node positions are registered only for the migrated legacy
  root galaxy, never for new galaxy-local nodes. Registering the legacy root's full
  bounding radius would mark the whole room as occupied and prevent new galaxies
  from being placed.
- `HubsSpawner.HideAll` must stop unregistering per-hub positions for new galaxy
  children. `GalaxiesSpawner` owns room occupancy for new galaxies; keep the old
  per-node unregister path only for the legacy root.

### 5.4 Legacy snapshots

Old snapshots have no `galaxies` list and no `galaxyId` on planets/hubs. Migrate on
load:

```text
schemaVersion = 1 (or absent)
-> create one GalaxyData at local (0,0,0), yaw 0, scale 1, radius covering the
   existing Level-local content
-> set transformable = false, roomFitEnabled = false, and fix min/max scale at 1
-> assign every existing planet and hub to that galaxy
-> keep their existing localPosition values unchanged
-> schemaVersion = 2
```

Because the legacy galaxy is at identity, no coordinate conversion is required.
The legacy root galaxy bypasses room-fit clamping: its content already fits the
room at `Level` scale 1, and a room-fit limit computed from its large radius would
incorrectly shrink the whole old level. New snapshots write galaxy-local positions
and a valid `galaxyId` on every node.

### 5.5 Guarantees and validation

- Every active planet/hub has a non-empty `galaxyId` that resolves to an active
  galaxy.
- Every galaxy has a valid `GalaxyData` entry in the snapshot.
- The level always has at least one galaxy.
- The migrated legacy root galaxy is not transformable; it exists only to preserve
  old snapshots. New galaxies are always transformable.
- A round may add galaxies but never removes them.
- The universe two-hand gesture still needs at least two planets overall; the
  existing gate remains.
- Galaxy IDs are tracked by the same `IdCounter` pattern as planets/hubs, with a
  separate counter or a prefix.

## 6. Two-hand galaxy gesture

### 6.1 Dedicated galaxy handle

Use one **galaxy handle per galaxy**, not a tiny core sphere. A small core cannot
provide enough hand separation for a usable scale gesture.

`GalaxyGrabHandle`:

- sits at the galaxy center and faces the galaxy's local up;
- is a grabbable **ring around the galaxy's member elements**, not a core sphere;
- has two opposed grab nubs on the ring so two distinct hands can hold the same
  galaxy;
- has a dedicated `GalaxyHandle` layer excluded from the ship destination and UI ray
  masks; the ring is grabbed directly, not through a UI ray;
- keeps a **world-space** ring radius clamped between a minimum (usable in Bubble
  and at small galaxy scales) and a maximum (reachable in Room at large galaxy
  scales), compensating the handle's local scale from the combined
  `D.scale * G.scale`;
- does not move the grabbed galaxy. It only reports pointer events and hand poses.

At small scales the interaction ring may be larger than the galaxy itself. Keep it
visually subtle and emphasize it only on hover or first grab; it is a grab proxy,
not part of the galaxy art.

The project is unifying its grabbables, so target the unified grabbable API. If it
cannot select one ring twice at the same time, use two child grab colliders
(left/right) that both report to the same galaxy. This is a required check before
C1; the coordinator API must not depend on which shape wins. Confirm that each
pointer exposes a usable world pose; otherwise read the unified interactor/hand
transform instead.

Interaction priority: the ring must not steal a planet grab. Use nubs placed outside
the planet paths and disable the ring collider while a planet first grab is waiting
or a universe session is active.

### 6.2 Coordinator and scopes

Extend the `UniverseGrabCoordinator` from the two-views plan to own the input scope:

```text
Idle
  first planet grab          -> WaitingForPlanetPair
  first galaxy-handle grab   -> WaitingForGalaxyPair

WaitingForPlanetPair
  second distinct planet     -> UniverseSession
  release / mismatch        -> Idle

WaitingForGalaxyPair
  second distinct hand on same galaxy -> GalaxySession
  release / mismatch                 -> Idle
```

Rules:

- one hand never transforms anything; it only prompts;
- two distinct hands are required;
- two different planets start the existing universe session;
- only one transform session may be active;
- a mismatched second grab is rejected and the first grab is released;
- ship grab and path creation remain one-hand interactions;
- while a galaxy session is active, planet handles and ship/destination
  interactions are disabled;
- while a universe session is active, galaxy handles are disabled;
- while a ship is grabbed or picking a destination, galaxy handles are disabled, and
  a ship grab is ignored while a galaxy first grab is waiting.

The first-grab reservation avoids the ambiguous case of one planet plus one galaxy
handle. Only the matching second grab starts a session.

### 6.3 Session math

At the second grab, capture:

```text
G0       = galaxy local pose (position, yaw0, scale0)
a0, b0   = hand poses in world space
v0       = projectToYawPlane(b0 - a0)   // world up
d0       = |v0|
```

Use world-space hands so `minHandSeparation` is a real-world distance and the
separation ratio is independent of `D.scale`. Since both `D` and `G` are yaw-only,
a world-space yaw delta applies directly to the galaxy's local yaw.

Reject the session when `d0 < minHandSeparation` or the two hands are not distinct.

Per frame:

```text
v1     = projectToYawPlane(b1 - a1)
d1     = |v1|
raw    = d1 / max(d0, epsilon)
scale  = clamp(scale0 * pow(raw, scaleGain), minScale, maxScale)
yaw    = yaw0 + DeltaAngle(startHandYaw, currentHandYaw)
```

Rules:

- `projectToYawPlane` drops Y and keeps the horizontal hand vector; if the
  horizontal length is below a small epsilon, keep the last yaw.
- The galaxy **position stays fixed**. The hand midpoint is not a translation
  target. Only hand separation (scale) and hand-line yaw (rotation) matter.
- Scale and yaw are relative to the start pose, so the gesture is stable if the
  player moves around the room.
- Apply a dead zone to scale ratio and yaw delta.
- Rebase `d0`, `scale0`, `yaw0`, and the reference hand yaw when a limit is hit so
  reversing responds immediately.
- Preserve pointer-to-hand identity. If the projected hand vector reverses by more
  than 90 degrees in one frame (hands crossing or tracking jitter), ignore that
  frame and rebase the yaw reference instead of snapping the galaxy 180 degrees.
- Clamp to `minScale`/`maxScale`. In Room, also clamp to the galaxy's room-fit
  maximum from §8.

### 6.4 Ghost preview

`GalaxyGhostSession` owns the math. `GalaxyGhostView` renders:

- a translucent proxy sphere for each member planet and hub (sphere only, no UI/VFX);
- a bounds ring/cage at the ghost scale;
- a yaw tick or meridian so the rotation is visible;
- a subtle limit color/feedback when clamped.

The ghost root lives under `Level` (so it inherits `D`), at the galaxy's fixed
local position, with the ghost yaw and scale. The real galaxy stays at its current
transform. Proxy positions are galaxy-local member positions. Compensate the ring
line width for `D.scale * ghostScale` so the ghost stays visible in Bubble.

Do not duplicate full planet content. Do not show letters, route panels, ships, or
particles in the ghost.

### 6.5 Commit and cancel

- **Release** commits the current ghost pose.
- **Tracking loss / state switch / disable** cancels and restores the previous pose.
- If one hand releases while the other still holds, commit the current ghost; the
  remaining hand is released from the session.
- A commit with no meaningful change (inside the dead zone) is skipped.
- During the commit tween, ignore new transform input and keep routes paused.
- Tween only `localRotation` and `localScale`; position is unchanged.
- After the tween: write `GalaxyData`, save the snapshot, rebuild routes, resume
  routes, and re-enable interaction.
- Cancel resumes routes without a rebuild.

## 7. Routes during a galaxy transform

A galaxy transform changes the world positions of its members. Existing
`Level`-local route legs are stale until rebuilt.

### 7.1 Pause

`UniverseTransformGate` (shared with the universe transform) pauses the route
runner state, not just the path tween:

- `SpaceshipsSpawner.PauseAllRoutes()` -> `SpaceshipBehaviour` -> `session.TravellerLoop.Pause()`;
- `TravellerRouteRunner.Pause()` already stops the loop between legs and pauses the
  current path tween; delays do not advance while paused;
- route lines stay hidden or frozen while the gesture is active, so they do not
  visibly detach from the transforming galaxy.

Ship grab and destination picking are disabled for the whole session.

### 7.2 Rebuild

After the galaxy commit tween:

```text
for each active ship:
    rebuild route legs from the current hub and destination list
    keep the current leg index
    restart the runner at that leg
    resume
```

Rebuild all active routes, not only routes whose endpoints are in the transformed
galaxy. A route may cross galaxies, and the route container is `Level`-level.

### 7.3 Current leg and picked letters

`SplineContainerWrapper` currently resets `currentLegIndex` to `-1` in `Build`.
Add a transform-aware rebuild path:

- `Build(hub, destinations, startLegIndex)` or `RebuildPreservingLeg()`;
- expose `CurrentLegIndex`;
- preserve the current leg index when the route still has that leg;
- clamp the index if the route got shorter.

`TravellerRouteRunner` currently owns the picked-letter count inside the loop
coroutine. Move the current leg index and picked-letter count to fields and add:

```text
RestartAtLeg(int legIndex)
```

It must:

- stop the active loop coroutine and cancel the current path tween;
- restart the loop at `legIndex` without resetting already-picked letters;
- place/show the traveller at the start of the rebuilt leg;
- not pick the same planet letters twice.

The rebuilt legs use the new world positions from `NodeBody.Position`, so the ship
snaps to the new geometry. A short snap is acceptable; do not try to preserve a
normalized position along a leg whose length changed.

### 7.4 Route timing

`TravellerPathFollower` computes duration from path-space distance / `gameSpeed`.
Scaling a galaxy changes the length of rebuilt legs, so travel time changes. This
is accepted: the route is still valid, only its duration changes.

### 7.5 Interaction scale

Routes remain `Level`-local, but interaction distances around a scaled galaxy are
not:

- `RaycastHelper` currently uses `contentRoot.lossyScale` (the `Level` scale).
  Destination rays from a ship whose hub is inside a scaled galaxy need the
  combined `Level * galaxy` scale, or a maximum active galaxy scale.
- `GrabZone` line width and offsets near a hub should use the hub's galaxy scale.
- `LineRendererWithEdges` route lines and route data stay `Level`-local and continue
  to use the `Level` scale.
- `NodeBody.Radius` already uses `lossyScale`, so it includes the galaxy scale.

Prefer a small `ContentScale` resolver that returns the lossy scale of the
transform that owns the interaction (hub/galaxy for local interactions, `Level` for
route lines). Do not add a single global "max scale" if a per-node scale is
available.

### 7.6 Station-anchored visuals

A route rebuild is not enough for ships that are not running. After a galaxy
commits, call a ship-level refresh that covers every state:

- **Running** - rebuild the route and restart at the current leg (§7.2).
- **Idle** - move the grabbable, grab ghost, grab zone, and station panel to the
  hub's new world `SpawnPose`.
- **Selecting** - do the same for the station anchors and cancel/refresh any
  in-progress destination candidate; the player should not confirm a stale target.
- The ship body and traveller keep `Level` scale; only their positions are
  refreshed. See the ship-scale risk in §16.

`SpaceshipsSpawner.RefreshGrabbablesAtStation` currently only handles the running
state. Replace or extend it with `RefreshAfterContentTransform` so idle and
selecting ships do not stay at the old hub position.

## 8. Room, Bubble, and constraints

### 8.1 Preserve transforms

Galaxy local transforms are independent of `D`. Entering or leaving Bubble does not
change a galaxy's yaw or scale. The universe transform composes on top:

```text
world galaxy pose = D * G
```

The Bubble mini pose and content bounds continue to use `D` and the content bounds.
Galaxy member bounds must be included when the bubble radius is computed if a
galaxy can extend beyond the room content bounds; with the room-fit limit below,
this is normally a no-op.

### 8.2 Room-fit limit

The galaxy position is fixed, so a room-fit maximum scale can be computed once:

```text
roomMaxScale = min over room bounds axes of available half-extent / galaxy radius
maxScale = min(globalMaxScale, roomMaxScale)
```

Compute the fit in `Level` space, using the room bounds transformed into that
space, and include floor/ceiling margins. Do not mix a world-space AABB with a
`Level`-local galaxy position. Apply the same `maxScale` in both Room and Bubble so
preserving transforms cannot make a galaxy overflow the room when returning to
Room. Recompute the limit whenever a round adds members to a galaxy, and skip the
clamp for the migrated legacy root galaxy (`roomFitEnabled = false`). If the MRUK
anchor relocates, recompute the limit and clamp affected galaxies with a short
animation.

If a future feature allows galaxy translation, this static limit must become a
per-pose constraint.

### 8.3 Bedrock constraints

- No pitch/roll.
- No translation.
- Uniform scale only.
- Ghost scale is clamped before commit; the real galaxy never exceeds the limit.
- Do not allow a galaxy transform during a view transition, a round spawn, or a
  ship route commit. Gate on `UniverseView.IsTransitioning`, `IsSpawning`, and the
  transform gate.

### 8.4 Galaxy-to-galaxy overlap

Room fit only keeps a galaxy inside the room; it does not keep it away from another
galaxy. Scaling one up can overlap another, mix their planet colliders, and make
destination picking ambiguous.

Decide and implement one of:

- **Prevent overlap (recommended):** include a neighbor term in `maxScale`, using
  galaxy bounding spheres and a small margin. If the resulting max is below
  `minScale`, allow the minimum scale with a warning rather than blocking the
  gesture.
- **Allow overlap:** simpler, but accept collider/ray ambiguity and visual
  interpenetration.

The recommended approach also needs spawn-time validation: place galaxies far
enough apart that both can exist at `minScale` without overlapping.

Note that rotation does not change a bounding-sphere overlap test. If an oriented
box is used later, the constraint must run per ghost yaw.

## 9. Persistence and reset

### 9.1 Snapshot

`GameSnapshot` gains:

```text
int schemaVersion
List<GalaxyData> galaxies
```

`PlanetData` and `HubData` gain `galaxyId`.

On commit, update the `GalaxyData` current pose and call the existing
`GameSnapshot` update notification; do not write every frame. Do not persist a
ghost session.

### 9.2 Reset

`GalaxyBehaviour.ResetPose(bool instant)` restores `defaultLocalPosition`,
`defaultLocalYaw`, and `defaultLocalScale`. Position is normally unchanged, but
the reset API keeps it for completeness.

`GalaxiesSpawner.ResetAll(bool instant)` resets every galaxy. Expose a debug
`[Button]` / debug action first; a player-facing reset control is a later UI task.
Reset must also rebuild routes and resave the snapshot.

### 9.3 New level

A fresh level clears the galaxy pool and snapshot galaxies. `LevelContainer.HideAll`
and `ResetForRestart` include `GalaxiesSpawner`. `SyncIdCounters` includes galaxy
IDs.

## 10. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `GalaxyData` | Saved galaxy pose, defaults, radius. | `DigitalLove.Game.Nodes` |
| `GalaxySeedData` | Round-authorable galaxy radius/spread/yaw/scale seed. | `DigitalLove.Game.Nodes` |
| `GalaxyBehaviour` | Galaxy root, members, bounds, apply/reset pose. | `DigitalLove.Game.Nodes` |
| `GalaxyRefs` | Serialized view/handle/bounds references. | `DigitalLove.Game.Nodes` |
| `GalaxyPlacement` | Seeded galaxy-local planet/hub placement. | `DigitalLove.Game.Nodes` |
| `GalaxiesSpawner` | Round planning, galaxy pool, placement, reset, room fit. | `DigitalLove.Game.Levels` |
| `GalaxyGrabHandle` | Two-hand ring/cage handle; pointer events; no transform. | `DigitalLove.Game.Universe` |
| `GalaxyGhostSession` | Start pose, hand separation/yaw, dead zone, clamp/rebase. | `DigitalLove.Game.Universe` |
| `GalaxyGhostView` | Proxy spheres, bounds ring, yaw indicator, limit feedback. | `DigitalLove.Game.Universe` |
| `GalaxyRoomConstraint` | Room-fit max scale and anchor-relocation refit. | `DigitalLove.Game.Universe` |

Modified:

| Type | Change |
|---|---|
| `RoundData` | `galaxiesToAdd`, `galaxySeed`, defaults for old assets. |
| `GameSnapshot` | `schemaVersion`, `galaxies`, migration, update helpers. |
| `PlanetData` / `HubData` | `galaxyId`; new positions are galaxy-local. |
| `LevelContainer` | Spawn order, galaxy spawner, reset/hide, ID sync, restore. |
| `PlanetsSpawner` | Assign to galaxy; parent under galaxy; galaxy-local placement. |
| `HubsSpawner` | Assign to galaxy; galaxy-local placement. |
| `PlanetBehaviour` / `ResourcePanel` | Verify labels are parented under the planet/galaxy and keep local offsets; do not re-anchor them in world space after spawn. |
| `MrukRoomLocalPlacement` | Register galaxy volumes; keep legacy fallback. |
| `UniverseGrabCoordinator` | Add galaxy scope and first-grab reservation. |
| `UniverseTransformGate` | Extend the existing route/input gate to cover galaxy sessions as well as universe sessions. |
| `SpaceshipsSpawner` / `SpaceshipBehaviour` | Pause all routes; disable interaction; rebuild after transform. |
| `SpaceshipRouteSession` | Transform-aware rebuild. |
| `SplineContainerWrapper` | Preserve current leg index on rebuild. |
| `TravellerRouteRunner` | `RestartAtLeg`, persistent leg/picked-letter state. |
| `RaycastHelper` / `GrabZone` | Resolve the owning galaxy scale for local interactions. |
| `Game.unity` | `Galaxies` root, galaxy prefab, galaxy handle, ghost, spawner wiring; rename the existing room VFX `Galaxy` object to `GalaxyBackdrop`. |

Assembly note: follow the existing `_Game/Features` layout. Put content data and
behaviour in `DigitalLove.Game.Nodes`, spawn/round code in `DigitalLove.Game.Levels`,
and gesture/ghost/constraint code in `DigitalLove.Game.Universe`. Avoid a dependency
cycle: `Universe` may reference `Levels`, `Nodes`, `Spaceships`, and XR packages;
`Nodes` and `Levels` must not reference `Universe`.

## 11. Phases

### Phase C0 — Galaxy content and round creation

- Add `GalaxyData`, `GalaxySeedData`, `GalaxyBehaviour`, `GalaxyRefs`,
  `GalaxyPlacement`, `GalaxiesSpawner`.
- Add `galaxiesToAdd` and `galaxySeed` to `RoundData` with safe defaults.
- Add `schemaVersion`, `galaxies`, and `galaxyId` persistence, plus legacy migration.
- Change spawn order and placement in `LevelContainer`, `PlanetsSpawner`, and
  `HubsSpawner`.
- Add a placeholder galaxy visual and debug reset.
- Verify routes and path creation at default galaxy poses.

**Exit:** a fresh level and each subsequent round create/assign galaxies; a
snapshot restores the hierarchy and poses; old snapshots migrate to one galaxy;
routes work without any galaxy gesture.

### Phase C1 — Galaxy handle, ghost, and commit

- Add `GalaxyGrabHandle` with a ring/cage or two grab points; validate multi-hand
  selection.
- Extend `UniverseGrabCoordinator` with the galaxy scope and first-grab reservation.
- Add `GalaxyGhostSession` and `GalaxyGhostView`.
- Add commit/cancel, dead zone, clamps, rebase, and input gating.
- Extend `UniverseTransformGate` to pause routes and disable ship/destination
  interaction for galaxy sessions.
- Add transform-aware route rebuild and `RestartAtLeg`.
- Add station refresh for idle/selecting ships and route panels.
- Add room-fit and neighbor-overlap scale limits.

**Exit:** two hands on a galaxy handle preview yaw and scale, release commits,
routes resume at the current leg, and the universe gesture cannot run at the same
time.

### Phase C2 — Persistence, reset, and polish

- Save committed galaxy poses; add per-galaxy reset and a debug reset-all.
- Refit galaxies after an MRUK anchor relocation.
- Tune ring size, minimum world grab size, ghost proxies, commit time, gearing, and
  dead zones.
- Tune labels/panels and limit feedback. Add a reset UI later if needed.
- Add automated tests for the pure math and snapshot migration.

**Exit:** the transform is stable across Room/Bubble, room boundaries, restarts,
and resets; gesture feedback is readable; tests cover the solver and data model.

## 12. Test plan

### Creation and persistence

1. Fresh level creates at least one galaxy; every planet/hub has a valid galaxy.
2. A round with `galaxiesToAdd = 0` assigns new nodes to existing galaxies.
3. A round with `galaxiesToAdd > 0` creates new galaxies and distributes nodes.
4. New galaxies do not overlap each other and fit inside the room at minimum scale.
5. A full galaxy triggers a new galaxy when room capacity allows, otherwise the
   least-full overflow path; no node is dropped.
6. Restore from snapshot recreates the same galaxy poses and membership.
7. A legacy snapshot becomes one identity galaxy with unchanged node positions and
   no transformable galaxy handle.
8. Reset restores a galaxy's spawn yaw/scale and resaves the snapshot.

### Gesture

9. One hand on a galaxy handle only prompts; no transform occurs.
10. Two hands on one galaxy show a ghost that scales and yaws; position does not
    change.
11. Scale and yaw clamps hold; dead zone prevents jitter; rebase responds after a
    limit.
12. Release commits the ghost; tracking loss cancels.
13. Same hand twice and mismatched planet/galaxy grabs are rejected.
14. Universe planet-pair gesture and galaxy gesture cannot run simultaneously.
15. Crossing hands does not flip the galaxy 180 degrees.

### Routes and views

16. Routes pause during the gesture and resume after commit.
17. Rebuilt routes start at the current leg and do not double-pick letters.
18. Routes spanning two galaxies stay attached after one galaxy transforms.
19. Idle and selecting ships move their grabbable, ghost, grab zone, and panel to
    the new hub pose after a galaxy commit.
20. Bubble -> Room preserves galaxy yaw/scale and never exceeds the room-fit limit.
21. Ship grab and destination picking are disabled during the gesture; UI remains
    available.
22. Planet labels/panels follow and scale with their galaxy; ship route panels stay
    at ship level.

### Automated

- Ghost solve: scale ratio, yaw delta, dead zone, clamps, rebase, no translation,
  hand-crossing guard.
- Galaxy bounds, room-fit max scale, and neighbor-overlap max scale.
- Galaxy assignment: every node assigned; capacity/overflow behavior; new-round
  distribution stable.
- Snapshot round-trip and legacy migration.
- Route rebuild: leg index and picked-letter count preserved.
- No per-frame allocations and no LINQ in the new code.

## 13. Risks and decisions

| Risk / decision | Decision |
|---|---|
| Galaxy membership | Created by level/round spawning, not authored as a fixed scene group. Membership is fixed after spawn. |
| Node position semantics | New `PlanetData`/`HubData` positions are galaxy-local; legacy snapshots migrate to one identity galaxy. |
| Legacy root galaxy | The migrated root is not transformable and bypasses room-fit, staying at scale 1; it exists only to preserve old snapshots. |
| Two-hand handle | Use a ring/cage or two opposed grab points; a tiny core sphere cannot provide enough hand separation. Validate multi-hand selection; fallback to two child grab colliders. |
| Handle world size | Clamp the grab radius between a minimum world-space size (Bubble/small scales) and a maximum reachable size (Room/large scales). |
| Ghost fidelity | Proxy spheres plus a bounds ring and yaw indicator; no full content ghost. |
| Commit trigger | Commit on release; cancel on tracking loss. No threshold auto-commit. |
| Translation | Galaxies do not translate during the gesture. Position is spawn-authored. |
| Routes | Pause all routes during the gesture; rebuild all active routes after commit; restart at the current leg. |
| Route snap | A rebuilt route snaps the traveller to the new current leg start. Accepted; do not try to preserve a normalized path position across a length change. |
| Room constraint | Compute a per-galaxy room-fit max scale at spawn and apply it in both Room and Bubble; refit after anchor relocation. |
| Preserve transforms | Galaxy local yaw/scale survives Room/Bubble switches; the universe transform composes above it. |
| Labels | Planet labels/panels scale with the galaxy. Readability is accepted as a risk for now. |
| Art | `GalaxySpace` is a room shell, not a galaxy. A placeholder galaxy visual is required for C0. |
| UI scope | Ship route panels stay at ship level and are not children of a galaxy. |
| Persistence | Save on commit and reset only; no per-frame writes; no ghost persistence. |
| One active transform | Only one galaxy or universe transform session at a time. First-grab reservation prevents ambiguous pairs. |
| Content bounds | Galaxy member bounds are included in content/bubble bounds when needed; room-fit prevents overflow in normal play. |
| Performance | Ghost uses shared proxy meshes/materials; no per-frame allocations; no LINQ. |

Additional second-order risks and defaults are in §16.

## 14. Defaults

| Parameter | Default |
|---|---|
| `galaxiesToAdd` | `0-2` (0 or 1; max is exclusive) per round; first round forces at least `1` |
| `galaxyRadius` | `0.28 m` (content space) |
| `galaxyPlanetSpread` | `0.70` of radius |
| `maxPlanetsPerGalaxy` | `6` |
| `maxHubsPerGalaxy` | `2` |
| `galaxyDefaultScale` | `1.0` |
| `galaxyMinScale` | `0.25` |
| `galaxyMaxScale` | `2.0` |
| `galaxyScaleGain` | `2.0` |
| `galaxyScaleDeadZone` | `0.04` |
| `galaxyYawDeadZoneDegrees` | `2.0` |
| `galaxyCommitSecs` | `0.25 s` |
| `minHandSeparation` | `0.08 m` |
| `galaxyHandleMinWorldRadius` | `0.10 m` |
| `galaxyHandleMaxWorldRadius` | `0.35 m` |
| `galaxyRoomMargin` | `0.05 m` |
| `galaxyMaxPerRoom` | `4` |
| `routeRebuildSecs` | `0 s` (rebuild after the commit tween) |

## 15. Assumptions to confirm before C1

The second design round was not answered; these defaults are recorded so the plan
is executable, but should be confirmed before gesture implementation:

- Commit on release rather than at a scale/yaw threshold.
- Ghost uses proxy spheres and a ring, not bounds only or full content.
- Routes resume at the current leg, not the whole loop or after waiting for idle.
- The dedicated handle is a ring/cage with two grab points, not a small core
  sphere. This is a functional requirement of the scale gesture.

If any of these changes, update §6, §7, and §13 before starting C1.

## 16. Additional decisions to settle

These are second-order decisions that do not block C0 but will cause visible
problems or rework if left implicit.

| Area | Risk / decision | Recommended default |
|---|---|---|
| Galaxy capacity | Unbounded planets per galaxy grow its bounds, shrink the room-fit scale, and bloat the ghost proxy count. | Fixed capacity (`maxPlanetsPerGalaxy = 6`, `maxHubsPerGalaxy = 2`); add a galaxy when full if room allows; otherwise overflow the least-full galaxy with a warning. |
| Ship visual scale | Ships stay at `Level` scale while a galaxy scales, so they can look oversized or tiny beside its planets; route line width is also `Level`-scale. | Keep ships and route lines at `Level` scale for C1. If it reads badly, add a visual-only ship scale presenter later; do not scale path or collider data per galaxy. |
| Station refresh | Idle/selecting ships, grab ghosts, grab zones, and route panels stay at the old hub pose after a galaxy commit. | Add `RefreshAfterContentTransform` covering running, idle, and selecting states. Cancel or refresh a stale destination candidate. |
| Galaxy overlap | Scaling one galaxy can overlap a neighbor, mixing colliders and making destination rays ambiguous. | Include a neighbor term in `maxScale`; if the result is below `minScale`, clamp to `minScale` and warn. Validate at spawn that neighbors can coexist at minimum scale. |
| Handle priority | A large ring can steal planet grabs or block UI rays. | Put two nubs outside planet paths; disable the ring collider during a planet first grab or universe session; exclude the handle layer from ship and UI ray masks. |
| VFX simulation | World-space particles/trails do not rotate or scale with the galaxy root. | Use local simulation space and local-space lines/trails for galaxy visuals. Never put a world-space trail under a galaxy. |
| Hand crossing | Hands crossing can flip the yaw reference by 180 degrees. | Ignore a projected-vector reversal greater than 90 degrees in one frame and rebase the yaw reference. |
| Room-fit feasibility | A galaxy placed near a wall can have `roomMaxScale < minScale`. | Validate at placement; try another position, reduce the seed radius, or skip the new galaxy. Never create a galaxy that is below its own minimum. |
| Scene naming | The scene already has a room VFX object named `Galaxy`; new content adds `Galaxies` and `GalaxyBehaviour`. | Rename the existing VFX object to `GalaxyBackdrop` before wiring the content root. |
| Pointer pose source | A multi-select `Grabbable` may not expose two distinct hand poses. | Spike before C1. Fall back to interactor/hand transforms or two child grab colliders. |
| Failure VFX | Ship failure triggers can miss at extreme galaxy scales. | Accept for now; route arrival is animation-based. Tune only if the miss is visible. |
| Haptics and audio | There is no feedback for first grab, second grab, limit, or commit. | Add haptic/audio cues in C2. |

