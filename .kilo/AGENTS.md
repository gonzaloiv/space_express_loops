# Project Rules

These mirror the Unity scene/prefab wiring rules in the global `AGENTS.md`.

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
