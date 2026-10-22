# Project Rules

These mirror the Unity scene/prefab wiring rules and feature validation checklist in the global `AGENTS.md`.

## SerializeField wiring

- Use serialized references for all scene, prefab, and same-GameObject collaborators.
- Do not add components at runtime or resolve them with `GetComponent`, `FindObjectsByType`, `FindObjectsOfType`, `FindFirstObjectByType`, or name-based lookups. Wire them in the prefab or scene.
- Do not null-check serialized references. They are required wiring; a missing reference is a broken prefab/scene and should fail loudly.
- Remove unused components from the prefab. Do not leave them disabled as a substitute for deletion.

## No runtime asset or hierarchy creation

- Do not create visual assets, materials, meshes, ghost hierarchies, or other presentation objects at runtime.
- Author them as prefab/scene GameObjects and reference them through serialized fields.
- Runtime code may position, scale, show, or hide authored objects; it must not invent them.
- This applies to UI, VFX, line renderers, ghost previews, and interaction proxies.

## States, modes, and sessions

Follow [`Assets/_Modules/docs/standards/modes.md`](Assets/_Modules/docs/standards/modes.md).

- **Flow state** = app/round progression, save/load, blocking gates. Owned by `_Game/Flow` and `MonoState`.
- **Mode** = persistent behaviour/presentation of one system. One owner, one enum, no parallel booleans like `isMini` + `isBubble`.
- **Session** = one interaction inside a mode. Plain class; does not own the mode.
- Do not add a Flow state for a view/camera/input mode. Promote only when it owns gameplay progression, panels, or persistence.
- Use `Mode`, `IsTransitioning`, `TransitionTarget` naming; transitions complete once and fire events once.

## Feature validation checklist

After implementing a user-facing feature, include a short manual validation checklist in the final response, written for the target device (normally the headset).

- Use a flat numbered list (`1.`, `2.`, ...), not grouped prose or checkboxes; one action or expected result per item.
- Keep it to the smallest set that proves the feature: normally 6-10 items.
- Cover only what applies: clean import/wiring, spawn/visibility, interaction/exclusions, adjacent regressions, Room/Bubble or state transitions, persistence/reset, comfort/clutter/performance.
- Include a tuning or decision item when a trade-off is still open.
- This is a manual acceptance pass, not a replacement for automated tests.
