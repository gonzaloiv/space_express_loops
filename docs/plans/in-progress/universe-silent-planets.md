# Universe — Silent (Non-Routable) Planets

**Status:** in progress - implementation complete; headset validation and tuning pending.

**Related:** [Bubble map drag](universe-transform.md) · [Infinite sector](../backlog/universe/universe-infinite-sector.md) · [Universe galaxies](../backlog/universe/universe-galaxies.md) · [Color scope](../backlog/universe/universe-color-scope.md)

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

### Non-goals

- Letters, rewards, lore, or interaction.
- Routes to silent planets.
- Persisting every silent position.
- Infinite streaming; [Infinite sector](../backlog/universe/universe-infinite-sector.md) owns that.
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
- **Distribution:** 3 soft clusters on a ring around the anchor, plus jitter.
- **Validation:** in-room check, height range, no overlap with gameplay/hub occupants, and radius-aware silent-to-silent spacing.
- **Anchor clearance:** silent planets stay outside the hub/player area (`anchorClearance + radius`).
- **Occupancy:** silent planets register in `MrukRoomLocalPlacement` with `isProximityTarget = false`, so they block overlap but do not change gameplay proximity checks.
- **Count:** `silentPlanetCount = 24`, soft cap `40`.
- **Generated once per level:** `SpawnIfNeeded(seed)`; `HideAll`/`ResetPool` clears and re-enables generation.

## 6. Visual treatment

- Smaller than gameplay planets (`0.08-0.14 m` radius).
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
| `SilentPlanetsSpawner` | Seed, placement, pool, count budget, room visibility | New (`DigitalLove.Game.Levels`) |
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

### S5 — Validation (pending)

- Silent planets never generate letters or appear in routes.
- No snapshot growth beyond the seed.
- No overlap/clutter; performance within budget.
- Bubble looks populated; Room stays readable.
- Seed restore reproduces the same layout after app restart.

## 10. Defaults

| Parameter | Default |
|---|---|
| `planetCount` | `24` |
| `planetCap` | `40` |
| `clusterCount` | `3` |
| `clusterRadius` | `1.2 m` |
| `clusterRingRadius` | `1.0 m` |
| `clusterRingJitter` | `0.5 m` |
| `minSeparation` | `0.25 m` |
| `spacingMultiplier` | `1.25` |
| `anchorClearance` | `0.6 m` |
| `minRadius` | `0.08 m` |
| `maxRadius` | `0.14 m` |
| `verticalJitter` | `0.35 m` |
| `placementAttempts` | `32` |
| `roomVisibleRadius` | `3 m` |

## 11. Risks

| Risk | Mitigation |
|---|---|
| Clutter hides gameplay planets | Small radii, spacing, 3 m Room culling. |
| Players try to route to them | No collider/Rigidbody/`PlanetBehaviour`, no outline, quieter look. |
| Snapshot bloat | Only one seed is saved; positions regenerate. |
| Performance | 24-40 simple visual planets, no stores/UI/colliders; measure draw calls. |
| Player occlusion | Anchor clearance and in-room placement; validate in headset. |
| Room culling pops at 3 m | Accepted for v1; fade needs transparent materials or a future scope shader. |
| Bubble still feels redundant | Silent planets improve legibility, but a wider universe needs Infinite Sector. |
| Generation changes invalidate old layouts | Seed-only restore; bump/document generation if placement rules change. |
| Duplicate ambient systems later | Separate first slice; migrate into Infinite Sector rather than building both permanently. |

## 12. Open decisions

- Should silent planets ever become selectable (lore/color bonus), or stay pure scenery?
- Cluster tint or uniform quiet palette.
- Whether the count should grow with the universe radius in the Infinite Sector work.
- Whether Room should eventually fade instead of hide.
