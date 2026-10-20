# Interaction Unification — Plan Insert

**Status:** design insert between the two-views work and the transform work.

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

Preferred model:

- One shared `Grabbable` proxy in the scene for the universe-scale gesture.
- Each planet template has a preconfigured `HandGrabInteractable` child whose pointable element is the shared proxy.
- The proxy target is a dummy transform (or a no-op transformer); it never moves the planet.
- Two hands selecting two planet interactables produce two pointer events on the shared proxy.
- `UniverseGhostController` holds serialized references to the shared proxy and listens to pointer events instead of polling `HandRef`.

Per-planet `Grabbable` objects with a no-op transformer are an acceptable fallback, but a shared proxy is simpler for the two-hand case. Either way, the components are authored in the planet template/prefab, not added at runtime.

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
| `Game.unity` / planet template | Add a preconfigured `PlanetUniverseHandle` child with a handle collider, a `HandGrabInteractable`, and a reference to the shared `Grabbable` proxy. |
| `Game.unity` | Add the shared `Grabbable` proxy and the `UniverseGhostView` hierarchy; serialize them on `UniverseGhostController`. |
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

- Add the shared `Grabbable` proxy to the scene.
- Add a preconfigured `HandGrabInteractable` child to the planet template, wired to the shared proxy.
- Add the handle collider and its layer in the template.
- Give `UniverseGhostController` serialized references to the proxy and ghost.
- Replace `HandRef` polling with pointer events.
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
| Pinch grab point differs from raw pinch midpoint | Tune grab point and scoring in the prefab; compare with the current hand midpoint behavior. |
| Removing touch changes discoverability | Add hover/outline feedback and keep pinch consistent across ships and planets. |
| Ray beats pinch | Exclude planet/ship layers from the ray in the relevant mode; verify interactor priority. |
| Shared proxy sees two hands on the same planet | Reject duplicate planet selections; require distinct planets. |
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
