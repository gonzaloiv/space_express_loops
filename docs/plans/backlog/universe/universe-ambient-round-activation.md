# Universe — Ambient Round Activation

**Status:** backlog - rounds activate existing decor planets instead of spawning new ones.
Depends on the ambient pool from [Sector streaming](universe-sector-streaming.md) and
builds on the decor slice from [Silent planets](../done/universe-silent-planets.md).

**Related:** [Infinite sector hub](universe-infinite-sector.md) · [Sector streaming](universe-sector-streaming.md) · [Universe galaxies](universe-galaxies.md)

**Wiring rule:** the authored `PlanetStore`/`ResourcePanel` components exist on the
prefab but are disabled; activation never uses `AddComponent`. Panels come from a small
authored pool.

## 1. Goal

Expose more planets than the current round needs, and let a round **activate** existing
ambient planets instead of spawning new ones.

- Ambient planets are cheap decor: no letters, no panel, no collider.
- A round selects `N` inactive ambient planets, enables their authored letter
  production, and leaves the rest as decor.
- The pool grows with the rounds.

## 2. Non-goals

- Spawning planets per round (that path stays for required content only).
- Full gameplay on ambient planets before a round activates them.
- Ambient planet persistence (deterministic from the seed; only active ids are saved).
- The galaxy grouping/transform from [Universe galaxies](universe-galaxies.md).

## 3. Decor first

Ambient planets are pre-spawned decor:

- no letters, no resource panel, no route selection, no destination ray target;
- visual rotation and the existing planet look only;
- no collider (or a non-interactive layer) until activated;
- the authored `PlanetStore`/`ResourcePanel` components exist on the prefab but are
  disabled, so activation never uses `AddComponent`.

Use the existing planet prefab as the shared visual. Keep the per-planet components
disabled for ambient planets; activation enables them.

Prefab check: `Body.prefab` is visual-only today (no `PlanetStore`/`ResourcePanel`). The
ambient prefab must be the full planet prefab with the letter components authored and
disabled. Settle this before Phase A1.

Decor readability: ambient planets use the normal planet look but no letters, no
highlight, and no rays. An activated planet gains its panel and collider, so the
transition from decor to gameplay is visible.

## 4. Activation

A round selects `N` inactive ambient planets and activates their letter production:

```text
Select N planets from the ambient registry
  -> prefer planets inside the bubble view window around the hub/room
  -> then nearest loaded systems; recenter the view on the chosen cluster
  -> avoid already-active/on-route planets and planets too close to the hub
ActivateForRound(planet, lettersPerMinute, maxLetters)
  -> enable PlanetStore and its panel
  -> PrepareStoring + ShowLetters
  -> enable the collider/layer for destination rays
  -> mark IsGameplayActive = true
```

- `PlanetBehaviour` gains `IsGameplayActive`; `PlanetSelectionRules` requires it.
- `PlanetBehaviour.Spawn` keeps the current required-content path; add a separate
  `SpawnAmbient`/`ActivateForRound` path. Do not reuse the round spawn path.
- The resource panel is pulled from a small authored panel pool, not one Canvas per
  ambient planet. Assign it on activation; return it on deactivation.
- `PlanetStore.Update` is disabled for ambient planets (component disabled), so decor
  costs no per-frame letter work.
- `ResourcePanel`/outline are inactive for decor.

## 5. Round selection

`RoundData`:

- replace `planetsToAdd` with `planetsToActivate` (keep the old field as a fallback for
  existing assets and migration);
- keep `planetSeed` for the letter rates and max letters;
- add `galaxiesToActivate`/`intergalacticAfterRound` (or a `selectionMode`) so early
  rounds use the central galaxy and later rounds can select across galaxies;
- `distanceBetweenPlanetsMultiplier` becomes legacy/unused for round spawning; the shape
  generator owns spacing;
- `shouldSpawnSpaceship`, `resetsLetters`, `hasLoseCondition` keep their meaning.

Selection rules:

- the hub is the route origin, so prefer planets inside the bubble view window around
  the **hub/room**, not an arbitrary far exploration focus;
- if the player has panned away, recenter the bubble view on the hub when the round
  starts, or select a visible cluster and recenter on it;
- if not enough planets are in the window, expand to the nearest loaded systems;
- avoid planets already active, on a route, or too close to the hub;
- early rounds select from the central gameplay galaxy; later rounds may select across
  galaxies (`galaxyId`) as the pool grows; routes can span galaxies;
- the pool grows with rounds, so exhaustion is not expected; if a round cannot find
  enough inactive planets, reuse the least-recently activated planet and reset its
  letters rather than failing the round;
- guarantee at least two selectable active planets near the hub for the current route
  flow.

The round assigns each selected planet's `lettersPerMinute` and `maxLetters` from the
round's `planetSeed`, then calls `BeginStoring` through the existing unlock step.

## 6. Identity and persistence

Ambient planet ids are deterministic:

```text
planetId = worldSeed + ":" + galaxyId + ":" + slotIndex
```

- Store the universe seed and the current focus in `GameSnapshot`.
- Store only round-activated planets (id, letters, maxLetters, active state) and the
  existing hub/ship/loop data; ambient decor is not persisted.
- On restore: regenerate the universe from the seed, stream the sectors around the
  stored focus, resolve the active ids, and activate them.
- If an active id is outside the loaded sectors, load its sector explicitly or fall back
  to the stored position.
- Keep the current snapshot fields for compatibility; add `schemaVersion`, `worldSeed`,
  `generatorVersion`, `universeFocus`, and the ambient activation fields.

**Why the generator version matters:** the universe is generated from `worldSeed`. If the
generation code changes, the same seed produces different ids/positions. A save that
lists active planet ids can then point to the wrong planet. Store `generatorVersion` and
either keep the old rules for old saves or regenerate and remap the saved active planets
by their stored position. This matters before shipping, not during the prototype.

## 7. Component map

New:

| Type | Responsibility | Assembly |
|---|---|---|
| `AmbientPlanetBehaviour` | Decor/activation state for a pooled planet. | `DigitalLove.Game.Nodes` |
| `AmbientPlanetRegistry` | Deterministic ids, sectors, loaded planets, lookup. | `DigitalLove.Game.Nodes` |
| `RoundPlanetSelector` | Pick inactive ambient planets for a round. | `DigitalLove.Game.Levels` |
| `AmbientPanelPool` | Small pool of world-space resource panels. | `DigitalLove.Game.Levels` |

Modified:

| Type | Change |
|---|---|
| `PlanetBehaviour` / `PlanetRefs` | `IsGameplayActive`, `SpawnAmbient`, `ActivateForRound`, `Deactivate`, panel assignment, collider/layer toggle. |
| `PlanetStore` / `ResourcePanel` | Enable only for active planets; panel pool. |
| `PlanetSelectionRules` | Require `IsGameplayActive`. |
| `RoundData` | `planetsToActivate`, `galaxiesToActivate`/intergalactic flag; keep `planetsToAdd` fallback; deprecate the distance multiplier. |
| `LevelLoopPlanetAvailability` | Account for ambient/active planets. |
| `GameSnapshot` | `schemaVersion`, `worldSeed`, `generatorVersion`, `universeFocus`, active ambient planet ids/letters. |
| `NewRoundState` | Round selection/activation instead of spawning new planets. |
| `LevelContainer` | Spawn/restore the ambient registry; round activation instead of planet spawn. |
| `MrukRoomLocalPlacement` | Required/round content only; ambient content does not register room occupants. |

## 8. Phases

### A1 — Ambient planets and round activation

- Add `AmbientPlanetRegistry`, `RoundPlanetSelector`, and the panel pool.
- Add `planetsToActivate`, `galaxiesToActivate`/intergalactic selection to `RoundData`
  and migrate the round assets.
- Add `PlanetBehaviour` activation/deactivation and `IsGameplayActive`.
- Update selection rules and `LevelLoopPlanetAvailability`.
- Implement the growing pool and intergalactic selection.
- Update the snapshot/restore path, including `generatorVersion`.

**Exit:** a round activates existing ambient planets instead of spawning planets; the
pool grows; later rounds can select across galaxies; letters start; routes see only
active planets; restore returns the same active set.

## 9. Test plan

1. Round activation: selects N existing planets; no new planet is spawned; letters
   start; UI appears; only active planets are selectable.
2. Selection near the hub: the round prefers visible planets; if it expands, the view
   recenters or the UI locates them.
3. Intergalactic: later rounds can select planets from more than one galaxy and routes
   still resolve.
4. Growth: the pool grows across rounds; reuse/reset is only a fallback and never fails
   a round.
5. Restore: universe seed/generator version/focus/active ids reproduce the same content
   and letters; old snapshots still load.
6. Decor cost: ambient planets have no letters/UI/colliders and no per-frame store work.

## 10. Defaults

| Parameter | Default |
|---|---|
| `planetsToActivate` | `RoundData` 2-4, growing with rounds |
| `intergalacticAfterRound` | tuned per level |
| `generatorVersion` | `1` |

## 11. Risks

| Risk | Mitigation |
|---|---|
| Ambient gameplay | Decor first; rounds activate authored letter components. No `AddComponent`. |
| Round planets | Select existing planets; early rounds central, later intergalactic; keep `planetsToAdd` fallback. |
| Pool growth | The pool grows with rounds; start with soft budgets. Reuse/reset is a fallback. |
| Panel cost | Small authored panel pool, not one Canvas per ambient planet. |
| Persistence | Seed, generator version, focus, and active ids only; ambient regenerates; snapshot migration. |
| Determinism | Save a `generatorVersion`; keep old rules or remap active planets by position. |
| Prefab gap | The ambient prefab must carry the letter components authored and disabled before A1. |
