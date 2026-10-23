# Universe — Silent (Non-Routable) Planets

**Status:** done - implemented and headset-validated; persistence/restore validation is deferred to a future cross-feature restart pass.

**Related:** [Bubble map drag](universe-transform.md) · [Sector field](../backlog/universe/universe-infinite-sector.md) · [Color scope](universe-color-scope.md)

**Wiring rule:** all visuals and refs stay authored in scene/prefabs. No runtime `AddComponent`, `GetComponent`, or hierarchy creation. Silent planets are instantiated from an authored prefab pool.

## 1. Decisions

| Topic | Decision |
|---|---|
| Relationship to Infinite Sector | **Separate first slice.** Unique types for now; migrate the pool/behaviour into the ambient system later. |
| Seed | One `silentPlanetSeed` per play, stored in `GameSnapshot`. Positions are regenerated from it; no position arrays are saved. |
| Spawn cadence | Once per level/room, via `SpawnIfNeeded`; not once per round. |
| Anchor | First active hub; room center when no hub exists. |
| Room visibility | Hide beyond `roomVisibleRadius = 3 m`; no fade in v1. |
| Visual source | `SilentPlanet.prefab`, derived from `Body.prefab` with Outline, ScalePunch, and colliders removed. |
| Placement | Radius-aware spacing; occupied by `MrukRoomLocalPlacement` so later gameplay planets avoid silent planets. |
| Bubble claim | Silent planets improve Bubble legibility, but Bubble is still a room-scaled copy until Infinite Sector's view window. |
| Vertical distribution | Silent planets only. Positions are uniform through the allowed room volume; gameplay and Bubble pan stay planar. |

## 2. Goal

Add **silent planets**: ambient planets that never generate letters, are not route destinations, and cannot be selected. They make the universe feel populated.

- **Gameplay planets** keep letters, routes, and their current behaviour.
- **Silent planets** are scenery/landmarks with no gameplay state.

## 3. Scope

### In scope

- Deterministic, seeded placement around the hub/room center.
- No letters, route destinations, selection, outlines, or route lines.
- Smaller, quieter visuals that do not read as interactable.
- Authored prefab pooling/reuse of the existing planet visual.
- Soft count budget, radius-aware spacing, and player-area clearance.
- Volumetric silent distribution through the room's allowed height.

### Non-goals

- Letters, rewards, lore, or interaction.
- Routes to silent planets.
- Persisting every silent position.
- The bounded sector field; [Sector field](../backlog/universe/universe-infinite-sector.md) owns that.
- Color scope; it can include silent planets later.

## 4. Model

```text
Gameplay planet = PlanetData + letters + route destination
Silent planet   = seeded position + visual variant, no gameplay data
```

- Silent planets never enter `GameSnapshot.planets`, `planetIds`, or loop destinations.
- `GameSnapshot.silentPlanetSeed` is the only saved silent state.
- Silent planets use `SilentPlanetBehaviour`, not `PlanetBehaviour`.
- Positions are content-local and follow the same Level transform as gameplay content.

## 5. Placement

- **Anchor:** first active hub; fallback `MrukRoomLocalPlacement.RoomCenterLocalPosition`.
- **Distribution:** 4 soft XZ clusters on a small ring around the anchor, with each planet taking a random Y through the room's allowed height (`TryGetHeightRange`, `verticalMargin = 0 m`). This fills the volume instead of creating stacked layers.
- **Validation:** in-room check, height range, no overlap with gameplay/hub occupants, and radius-aware silent-to-silent spacing.
- **Anchor clearance:** silent planets stay outside the hub/player area (`anchorClearance + radius`).
- **Occupancy:** silent planets register in `MrukRoomLocalPlacement` with `isProximityTarget = false`, so they block overlap but do not change gameplay proximity checks.
- **Count:** `silentPlanetCount = 36`, soft cap `48`; placement retries up to 64 times per planet to fill the cubic volume.
- **Generated once per level:** `SpawnIfNeeded(seed)`; `HideAll`/`ResetPool` clears and re-enables generation.

## 6. Visual treatment

- Smaller than gameplay planets (`0.08-0.14 m` radius).
- Spread through the room volume so the Bubble reads as a 3D cluster rather than a flat ring.
- No letter panel, selection outline, destination ray, or route slot.
- `SilentPlanet.prefab` keeps `NodeBody` and `ConstantRotation`, removes `Outline`, `ScalePunch`, and colliders.
- No tint in v1; per-cluster tint is optional later.
- Colliders: none.
- `NodeBody` exposes a seeded variant pick and renderer visibility toggle.

## 7. View behaviour

- **Room:** silent planets beyond 3 m from the camera are hidden by toggling the active renderer.
- **Bubble:** all silent planets inside the current content are visible.
- Transitions update visibility through `UniverseView.TransitionStarted` / `TransitionCompleted`.
- Selection/route systems ignore silent planets by construction: no `PlanetBehaviour`, no Rigidbody, no collider.
- Silent planets follow the Level transform, including Bubble scale and Room mapping.

## 8. Integration map

| Type | Responsibility | Change |
|---|---|---|
| `SilentPlanetsSpawner` | Seed, placement, pool, count budget, room visibility | New (`DigitalLove.Game.SilentPlanets`) |
| `SilentPlanetLayout` | Layered cluster centers across the room's Y range | New (`DigitalLove.Game.SilentPlanets`) |
| `SilentPlanetBehaviour` | Visual variant, no gameplay state, visibility | New (`DigitalLove.Game.Nodes`) |
| `SeededRandom` | Deterministic PRNG for placement and variants | New (`DigitalLove.Global`) |
| `SilentPlanet.prefab` | Stripped visual prefab | New |
| `NodeBody` | Seeded variant pick, renderer visibility | Extend |
| `MrukRoomLocalPlacement` | Candidate validation, non-proximity occupants | Extend |
| `HubsSpawner` | First active hub for the anchor | Extend |
| `GameSnapshot` | `silentPlanetSeed` | Extend |
| `LevelContainer` | Reset, spawn after gameplay content | Extend |
| `SpawnerIdSync` | Extracted ID-counter sync | New helper |
| `PlanetVisual` reuse | Replaced by `Body.prefab`-derived `SilentPlanet.prefab` | Clarified |
| Route/letter systems | Ignore silent planets | No change |

## 9. Phases

### S1 — Data and seed (done)

- `SeededRandom` and deterministic positions.
- One seed per play stored in `GameSnapshot`; no silent position entries.
- Spawner and pool skeleton.

### S2 — Placement (done)

- Cluster/scatter layout around the hub/room center.
- Radius-aware spacing against gameplay planets, hubs, and silent planets.
- Occupancy registration; content-local positions.

### S3 — Visual treatment (done)

- `SilentPlanet.prefab` derived from `Body.prefab`.
- Outline, ScalePunch, and colliders removed.
- Smaller radius range.

### S4 — View visibility (done)

- Room: hide beyond 3 m.
- Bubble: show all.
- Transition events keep visibility in sync.

### S5 — Base validation (done)

- Headset pass: spawn/placement, non-interactive visuals, gameplay regression, next-round occupancy, Room/Bubble culling, and comfort/performance passed.
- Persistence/restart validation is deferred to a future cross-feature restore/restart pass.

### S6 — Volumetric distribution (done)

- XZ clusters plus a random Y per planet fill the allowed room volume.
- `verticalMargin = 0` lets the field reach the floor/ceiling limits without changing gameplay placement.
- Gameplay planets are protected by occupancy checks and the spherical anchor clearance.
- Gameplay planets and Bubble pan remain planar.
- Persistence/restart validation remains deferred with the base feature.

## 10. Defaults

| Parameter | Default |
|---|---|
| `planetCount` | `36` |
| `planetCap` | `48` |
| `clusterCount` | `4` |
| `clusterRadius` | `0.7 m` |
| `clusterRingRadius` | `0.35 m` |
| `clusterRingJitter` | `0.25 m` |
| `minSeparation` | `0.22 m` |
| `spacingMultiplier` | `1.15` |
| `anchorClearance` | `0.3 m` |
| `minRadius` | `0.08 m` |
| `maxRadius` | `0.14 m` |
| `verticalMargin` | `0 m` |
| Y distribution | Uniform through the room's allowed height |
| `placementAttempts` | `64` |
| `roomVisibleRadius` | `3 m` |

## 11. Risks

| Risk | Mitigation |
|---|---|
| Clutter hides gameplay planets | Small radii, spacing, 3 m Room culling. |
| Players try to route to them | No collider/Rigidbody/`PlanetBehaviour`, no outline, quieter look. |
| Snapshot bloat | Only one seed is saved; positions regenerate. |
| Performance | 36-48 simple visual planets, no stores/UI/colliders; measure draw calls. |
| Player occlusion | Spherical anchor clearance, in-room placement, and gameplay occupancy checks; validate in headset. |
| Vertical spread crowds floor/ceiling | Cluster Y uses the room's allowed range with radius margins; validate in short rooms. |
| Room culling pops at 3 m | Accepted for v1; fade needs transparent materials or a future scope shader. |
| Bubble still feels redundant | Silent planets improve legibility, but a wider universe needs Infinite Sector. |
| Generation changes invalidate old layouts | Seed-only restore; bump/document generation if placement rules change. |
| Duplicate ambient systems later | Separate first slice; migrate into Infinite Sector rather than building both permanently. |

## 12. Open decisions

- Should silent planets ever become selectable (lore/color bonus), or stay pure scenery?
- Cluster tint or uniform quiet palette.
- Whether the count should grow with the universe radius in the Infinite Sector work.
- Whether Room should eventually fade instead of hide.
