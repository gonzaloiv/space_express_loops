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

## Parameter count

In first-party C#:

- A method or constructor takes **at most 5 parameters**; optional parameters count individually.
- Group related values into a parameter object, or have the owning type construct its collaborators.
- Do not pass the same long tail of tuning values through several layers.

### Exceptions

Unity lifecycle and message methods, interface implementations, and signatures required by an external API may exceed the limit. Skip `**/Vendor/**`, `**/Samples/**`, and `**/Plugins/**`.

## Public surface

In first-party C#:

- Default to the narrowest visibility that works: `private`, then `internal`, and `public` only when another assembly, a prefab, or an external API needs the member.
- State that only drives one system stays `internal`; expose it publicly only at the assembly boundary.
- Events and read-only properties are the integration surface. Do not expose public mutable fields or setters for convenience.
- A public getter with a private setter is still public surface; budget it like any other public member.
- Do not widen visibility for tests; use `InternalsVisibleTo` only when the test assembly actually needs it.

### Exceptions

Serialized Unity fields, prefab-facing types, interface implementations, and public data types may need public visibility. Skip `**/Vendor/**`, `**/Samples/**`, and `**/Plugins/**`.

## Visibility is not encapsulation

- `internal` is assembly-wide public. Do not use it to disguise a wide interface or unfinished coupling.
- If several collaborators reach into one type, fix the ownership or pass a snapshot/command; do not hide the members as `internal` and call it encapsulated.
- Reserve `internal` for genuine module helpers shared by a small set of sibling types in the same assembly.
- If a member would be public across an assembly boundary, justify its design as if it were public.

## State ownership and snapshots

When one type owns state that several collaborators need:

- Do not let collaborators pull mutable state from the owner or call a broad set of setters.
- Pass a small read-only snapshot (mode, scale, position, flags) into the collaborator.
- Have the collaborator return intentions or actions; the owner or its coordinator applies them.
- Keep the owner as the only writer, and keep presentation feedback with the system that produces it.
- Prefer one `State`/`Snapshot` value over a row of public derived properties.

## View independence

- Content and domain systems must not reference the view that presents them.
- The composition root (level/flow) observes the view and fans presentation state into content systems.
- Feature assemblies should depend on domain modules, not on presentation assemblies.
- A content spawner exposes commands such as `SetBubbleView(bool)`; it does not subscribe to view events itself.

## Feature validation checklist

After implementing a user-facing feature, include a short manual validation checklist in the final response, written for the target device (normally the headset).

- Use a flat numbered list (`1.`, `2.`, ...), not grouped prose or checkboxes; one action or expected result per item.
- Keep it to the smallest set that proves the feature: normally 6-10 items.
- Cover only what applies: clean import/wiring, spawn/visibility, interaction/exclusions, adjacent regressions, Room/Bubble or state transitions, persistence/reset, comfort/clutter/performance.
- Include a tuning or decision item when a trade-off is still open.
- This is a manual acceptance pass, not a replacement for automated tests.

## Explain with folders

Organize first-party code into folders that describe what it does, so the tree reads as an outline of the system. Do this everywhere it is possible.

- Group by responsibility, not by file kind: `Domain/`, `Data/`, `Views/`, `Flow/`, `Editor/`. A file's folder should tell you what it is before you open it.
- One responsibility per folder; keep related types together and unrelated ones apart.
- Prefer a descriptive folder over name prefixes or a flat dump of files.
- When moving files, move the `.meta` too; keep GUIDs (never re-create assets).
- Skip `**/Vendor/**`, `**/Samples/**`, and `**/Plugins/**`.
