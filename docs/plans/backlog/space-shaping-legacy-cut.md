# Space Shaping — Legacy Cut

**Status:** next · space shaping · updated 2026-10-28 — delete the route/round/economy flow and everything it
owns. Deletion only: keepers are already out, so this plan should not invent behaviour.

**Related:** [progression](space-shaping-progression.md) · [campaign map](space-shaping-campaign-map.md) ·
[in the game](space-shaping-in-game.md) · [jobs](../../design/jobs.md) · [vision](../../design/vision.md)

**Rule:** delete, do not disable. No `.meta` orphaning, no empty asmdefs, no dead serialized refs left in
the scene, no commented-out code.

## 1. Preconditions

Nothing here is safe before the keepers have moved:

| Owner | Keeper | Moved by |
|---|---|---|
| `LevelContainer` | `SectorFieldSpawner` spawn/reset, bubble restore | [campaign map](space-shaping-campaign-map.md) §M1 |
| `GameStartState` | boot into the map | [campaign map](space-shaping-campaign-map.md) §M3 |
| `GameSnapshot` | `bubbleOffsetLocal` / `bubbleYawDegrees`, `silentPlanetSeed` | [progression](space-shaping-progression.md) §P3 |
| `UniverseView` | bubble mode, room anchor, restore — **keep**, do not delete with the level container | — |
| `Decoration` | shared planet view + prefab (`FieldPlanetBehaviour`, `FieldPlanet.prefab`) — already in `Nodes` | done 2026-10-28 |
| `Spaceships` | `ShipVisual.prefab` copy — already out (2026-10-28) | done 2026-10-28 |

## 2. Slices

### C1 — Code and asmdefs

- Flow/panels: `GameStartState` (resume), `EditionState` (+ its asmdef), `NewRoundState`, `GameEndState`,
  `GameEndPanel`, `RoundSelector`, `LevelContainer`, `LevelViewModeFanout`, `SpawnerIdSync`,
  `LevelLoopPlanetAvailability`, `RoundData`.
- Content/economy: `PlanetsSpawner`, `PlanetStore`, `ResourcePanel`, `StorePanel`, `PlanetRouteColorSync`,
  round/chapter configuration that only they read.
- Route/ship: hubs/stations, ship grab, destination selection, `SplineContainerWrapper`,
  `SplineLegSampler`, `TravellerRouteRunner`, `SpaceshipRouteSession`, and the `Spaceships` feature's
  route/selector/states scripts (`Spaceships/Scripts` — 29 files today).
- Persistence: the route/round/economy fields of `GameSnapshot` and its store/cookie helpers.
- Remove the asmdefs this empties. Keep TTS, analytics, setup/splash, XR, puzzle creator.
- **Exit:** solution compiles with none of the above; EditMode suite green; no asmdef references a removed
  assembly.

### C2 — Scene, prefabs, assets, localization

- `Game.unity`: delete the legacy route/hub/round objects, the start/end panels and their wiring.
- Delete the spaceship/hub/planet-spawner prefabs and the `Resources/Chapters/*` assets that only the
  round flow read; delete route/resource/store panel prefabs.
- Remove the route/resource/store localization keys (all locales) and the legacy start/end panel keys.
- **Exit:** `Game.unity` contains no legacy route/hub/economy object; setup/splash still boots; boot → map →
  pick → solve is unchanged.

### C3 — Sweep

- No disabled leftovers, no empty folders/asmdefs, no dead serialized refs, no `.meta` without an asset.
- Update docs links that pointed at removed features (plans board rows, related links).
- **Exit:** `docs/check.ps1` green; Unity console clean on boot; project search finds no legacy type names.

## 3. Out of scope

- Any behaviour change to the puzzle, map or save.
- Final art/audio polish.
- Deleting `_Ignore` or vendor/sample trees.

## 4. Risk

Cutting before plans 1–4 break the keepers (the field, the placement, the boot path). If a keeper is still
inside a legacy type when C1 starts, stop and move it first.
