# Project Rules

These mirror the Unity scene/prefab wiring rules and feature validation checklist in the global `AGENTS.md`.

## Plans are a morning

A plan is **one sitting of work**, not a roadmap. Keep it to what one agent can land in a morning.

- **A handful of slices, not a dozen.** Each slice says what lands and its exit — nothing more.
- **No archaeology.** Don't restate the design docs, the history, or every rejected option.
- **If it grows, split it.** A plan that needs a week is a project: break it into plans that each fit a
  morning.
- **The board stays small.** A plan that has outgrown its shape moves to its own plan; it does not keep
  growing in place.
- **Done plans are removed.** When a plan lands, delete its file and its board row; the code and tests
  are the record. Do not archive it in a `done/` folder.
- The **plans board** ([`docs/plans/README.md`](docs/plans/README.md)) is the entry point; run
  `docs/check.ps1` after moving files.

## Reviews live in chat

A review is delivered **in the chat**, not as a file: a one-line verdict, then ranked findings with
`file:line` evidence. Do not create a `docs/reviews/` folder or review documents. When a review changes a
plan or the code, apply it in the same change and say so in the chat.

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

## Spawned content

Content that is generated or pooled at runtime **from an authored prototype** is fine — the sector field's silent planets (`SectorFieldSpawner` + `NodePoolExtensions.AcquireInactive`) and the puzzle field's planets (`SpaceShapingView`'s `SpaceSourceView` pool) are the precedent. Spawn from a serialized prototype and pool it; never invent a mesh, material, or hierarchy with no authored source.

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

## Explain with folders

Organize first-party code into folders that describe what it does, so the tree reads as an outline of the system. Do this everywhere it is possible.

- Group by responsibility, not by file kind: `Domain/`, `Data/`, `Views/`, `Flow/`, `Editor/`. A file's folder should tell you what it is before you open it.
- One responsibility per folder; keep related types together and unrelated ones apart.
- Prefer a descriptive folder over name prefixes or a flat dump of files.
- When moving files, move the `.meta` too; keep GUIDs (never re-create assets).
- Skip `**/Vendor/**`, `**/Samples/**`, and `**/Plugins/**`.
