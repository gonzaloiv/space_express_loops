# Interaction Unification — Plan Insert

**Status:** U0 ship prefab updated; U1 code added; planet/scene wiring still required.

**Related:** [Two views](universe-two-views.md) · [Transforming the universe](universe-transform.md)

## 1. Goal

Use one interaction path for every hand grab:

- **thumb-index pinch** (`HandGrabInteractable`, `GrabTypeFlags.Pinch`);
- the same `Grabbable` / `PointableElement` pointer-event flow;
- no touch-grab/pinch mix for the primary interactions.

Ships and planet handles should feel like the same kind of object to the hand.

## 1.1 Wiring rules

All interaction setup lives in prefabs and scenes:

- no runtime `AddComponent`, `InjectOptional...`, or runtime-created interaction objects;
- no `FindObjectsByType`/`FindObjectsOfType`/`GetComponent` lookups for these dependencies;
- no null checks on serialized references — missing wiring is a broken prefab/scene and should fail loudly;
- no disabling unused components — remove them from the prefab;
- shared references (`Grabbable` proxy, ghost view, controller) are serialized in the scene/prefab;
- the ghost is a scene/prefab object with its LineRenderers, not a runtime-created hierarchy;
- remove `TouchHandGrabInteractable` from the ship prefab, not just disable it;
- keep `TouchHandGrabRelease` only if another interaction still needs touch grab; otherwise remove it from the ship path.

The current runtime auto-wiring in `LevelContainer`/`UniverseGhostController` is provisional and must be replaced by this scene/prefab wiring before this step is considered complete.

## 2. Current state

### Ships

The ship prefab currently has both:

- `TouchHandGrabInteractable` (max 1);
- `HandGrabInteractable` (max -1).

Both point to the ship’s `Grabbable`, and `GrabbableWrapper` listens to `Grabbable.WhenPointerEventRaised`. In practice the ship accepts both touch and pinch. The touch component will be deleted from the prefab.

### Planets

`UniverseGhostController` currently polls `HandRef`:

- thumb-tip/index-tip pinch strength;
- pinch midpoint;
- nearest planet by distance.

This works, but it is a parallel interaction path. It does not use hover/selection, Oculus grab scoring, or the same pointer events as the ship.

## 3. Unified model

### 3.1 Pinch-only component

Configure `HandGrabInteractable` directly in the prefab/scene:

- `_supportedGrabTypes = GrabTypeFlags.Pinch`;
- pinch rules that require thumb + index;
- palm rules disabled;
- no movement provider (the pointable element decides what moves).

No runtime profile/injection component is needed. The same serialized configuration is used on ships and planet handles.

### 3.2 Ships

- Keep the ship `Grabbable`.
- Keep one `HandGrabInteractable` configured for pinch only.
- **Delete** `TouchHandGrabInteractable` from the ship prefab.
- `GrabbableWrapper` stays unchanged: it already reacts to `Select` / `Unselect` / `Cancel`.
- `TouchHandGrabRelease` remains available for any other touch-grabbable content, but is no longer required for ships. If nothing else uses touch grab, remove it from the ship path entirely.

### 3.3 Planet handles

Chosen model for U1:

- Each planet template has an authored `UniverseHandle` proxy child with its own kinematic Rigidbody, handle collider, `Grabbable`, pre-added `GrabFreeTransformer`, pinch-only `HandGrabInteractable`, and `PlanetUniverseHandle`.
- The proxy moves with the hand; the planet visual does not move.
- `HandGrabInteractable` allows only one selecting interactor per planet, so two hands must use two planets.
- `PlanetUniverseHandle` reports `IsSelected`, `InteractorId`, and `GrabPose` from the proxy transform.
- `UniverseGhostController` reads the selected handles from the planet pool; no `HandRef` polling.

A shared `Grabbable` proxy remains a possible later optimization, but the per-planet proxy is simpler to validate and keeps each planet’s selection state explicit. All components are authored in the planet template, not added at runtime.

### 3.4 Interaction arbitration

Introduce a shared, explicit state:

```text
Idle
  ├─ ship pinch selected ──> ShipGrab (one hand)
  └─ first planet selected ──> WaitingForSecondPlanet
        ├─ second distinct planet selected ──> UniverseScale (two hands)
        └─ release ──> Idle
```

Rules:

- a ship grab is always one hand;
- a single planet never scales;
- two distinct planets + two hands scale;
- if a hand is already selecting a ship, planet handles ignore that hand;
- mode/state changes force-release the active session.

Use pointer-event identifiers to distinguish hands/interactors rather than polling raw hand joints.

### 3.5 Priority and layers

- Planet handles live on a dedicated layer excluded from the ship destination ray.
- Hand pinch must beat hand ray for planets and ships.
- UI ray/poke remains available.
- In Path mode, planet handles are inactive; in Universe interaction, ship grab is inactive.

## 3.6 Scene/prefab wiring checklist

| Asset | Change |
|---|---|
| `Spaceship (1).prefab` | Remove `TouchHandGrabInteractable`; configure `HandGrabInteractable` for pinch only; keep the `Grabbable` and `GrabbableWrapper` references. |
| `Game.unity` / planet template | Add a preconfigured `UniverseHandle` proxy child under the planet: kinematic Rigidbody, handle `SphereCollider` on the `UniverseHandles` layer, `Grabbable`, pre-added `GrabFreeTransformer`, pinch-only `HandGrabInteractable`, and `PlanetUniverseHandle`. Wire `PlanetBehaviour.universeHandle` to it. |
| `Game.unity` | Add the authored `UniverseGhostView` hierarchy; serialize it on `UniverseGhostController`. |
| `Game.unity` / `Level` | Add `UniverseView` and `UniverseGhostController` to the scene object; remove runtime auto-add. |
| Layers | Add the `UniverseHandles` layer and exclude it from the ship destination ray mask. |
| Input | Keep the hand-ray/poke path for UI; ensure pinch wins on ship/planet interaction layers. |

## 4. Migration phases

### Phase U0 — Ship pinch only (prefab edit)

- Configure the ship `HandGrabInteractable` for pinch-only in `Spaceship (1).prefab`.
- Remove the `TouchHandGrabInteractable` component from the ship prefab.
- Keep the ship `Grabbable` and `GrabbableWrapper` serialized references intact.
- Verify grab/drop and route editing in the headset.

**Exit:** ships are grabbed with thumb-index pinch only; path editing is unchanged; no runtime touch/pinch configuration.

### Phase U1 — Planet handles through the same component (prefab/scene edit)

- Add an `UniverseHandle` proxy child to the planet template with its own kinematic Rigidbody.
- Add the handle collider on the `UniverseHandles` layer.
- Add `Grabbable`, a pre-added `GrabFreeTransformer`, a pinch-only `HandGrabInteractable`, and `PlanetUniverseHandle` to the proxy.
- Wire `PlanetBehaviour.universeHandle` to the proxy handle.
- Give `UniverseGhostController` serialized references to the planet spawner, spaceship spawner, ghost, and view.
- Replace `HandRef` polling with the proxy selection state.
- Remove any now-unused runtime setup code.

**Exit:** the two-planet scale gesture uses the same pinch component and pointer-event flow as ships, with all wiring authored in prefabs/scene.

### Phase U2 — Arbitration and cleanup

- Add the shared `GrabArbiter`/state machine.
- Gate ship vs planet interactions by mode.
- Remove obsolete touch-specific code and unused components.
- Tune hover feedback and priorities in the prefab.

## 5. Risks

| Risk | Mitigation |
|---|---|
| Prefab wiring drift / missing references | Keep references serialized and validate in the scene; no runtime discovery or injection. |
| SDK auto-adds `GrabFreeTransformer` to the proxy | Pre-add the transformer in the prefab and wire it to the `Grabbable` so the SDK does not add it at runtime. |
| Pinch grab point differs from raw pinch midpoint | Tune grab point and scoring in the prefab; compare with the current hand midpoint behavior. |
| Removing touch changes discoverability | Add hover/outline feedback and keep pinch consistent across ships and planets. |
| Ray beats pinch | Exclude planet/ship layers from the ray in the relevant mode; verify interactor priority. |
| Two hands select the same planet | Set `HandGrabInteractable` max selecting interactors to `1`; require two distinct planet handles. |
| Handle colliders interfere with destination ray | Dedicated handle layer excluded from the route ray mask. |
| Ghost built at runtime | Author the ghost hierarchy in the scene/prefab and serialize it on the coordinator. |

## 6. Acceptance

- Ships and planets both respond to thumb-index pinch.
- A single pinch on a planet never scales the universe.
- Two distinct planet pinches drive the ghost scale.
- Ship grab/drop still works exactly as before in Path mode.
- No hand-ray/pinch ambiguity in either mode.
- Touch grab is gone from the primary interaction path.
- The ship prefab no longer contains `TouchHandGrabInteractable`.
- No interaction component, proxy, or ghost is created at runtime for this feature.
- No unused component is left disabled; it is removed from the prefab.
