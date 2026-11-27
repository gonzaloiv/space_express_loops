# Project Rules

These mirror the Unity scene/prefab wiring rules and feature validation checklist in the global `AGENTS.md`.

## Plans are a morning

A plan is **one sitting of work**, not a roadmap. Keep it to what one agent can land in a morning.

- **A handful of slices, not a dozen.** Each slice says what lands and its exit — nothing more.
- **No archaeology.** Don't restate the design docs, the history, or every rejected option.
- **If it grows, split it.** A plan that needs a week is a project: break it into plans that each fit a
  morning.
- **Plans in a group share a prefix.** When several plans serve one goal, name each
  `<group>-<part>.md` so they sort together: `chapter-3-broken-bodies`, `chapter-3-map-arc`,
  `freelance-track`, `freelance-jobs`. The group is the chapter or feature, the part says what this plan
  lands. Rename a plan when it joins a group, and fix its links in the same change.
- **The board stays small.** A plan that has outgrown its shape moves to its own plan; it does not keep
  growing in place.
- **Do not close a plan on your own.** Finishing the units, passing checks, or completing a manual
  checklist is not permission to write the solution note, delete the plan, or update the board. Keep it
  in `docs/plans/in-progress/` and report what remains until the user explicitly says to close it.
- **Closing is explicit.** When the user says to close a plan, delete its file and board row; the code
  and tests are the record. Do not archive it in a `done/` folder.
- **When the user closes a plan, write the post-mortem.** Before deleting it, add a few lines to
  [`docs/lessons.md`](docs/lessons.md): what worked, what surprised us, and the rule for next time.
  Three to six bullets, one idea each, no history of the work. If a lesson changes how we build, also
  put it where it applies (this file, a design doc, a standard). This is required with or without
  `ce-compound`.
- The **plans board** ([`docs/plans/README.md`](docs/plans/README.md)) is the entry point; run
  `docs/check.ps1` after moving files.

## Reviews live in chat

A review is delivered **in the chat**, not as a file: a one-line verdict, then ranked findings with
`file:line` evidence. Do not create a `docs/reviews/` folder or review documents. When a review changes a
plan or the code, apply it in the same change and say so in the chat.

## Commits at unit and plan completion

When a plan unit's exit is met and the automated checks pass — stage only that unit's files and make a
local commit; report the hash. When the user explicitly closes the plan and every unit and exit is
implemented and the checks pass — make the plan-completion commit as the final step. Do not push; the
user still owns push and PR. If the work spans a submodule, commit the submodule first, then the parent pointer. Leave
unrelated worktree changes uncommitted.

## No internal APIs

In first-party C#:

- Do not call or instantiate `internal` methods, constructors, or types owned by another type. `internal` is assembly-wide public and makes a collaborator depend on an implementation detail.
- When a collaborator needs the behavior, move it to the owning type, add a collection extension, or pass a read-only snapshot / return an intention.
- Do not add `InternalsVisibleTo` to open up another type; fix the ownership or the boundary instead.
- Declaring a helper `internal` is acceptable only for a genuine module helper shared by a small set of sibling types in the same assembly — never as a shortcut to reach across features.
- If nothing public fits, stop and fix the design; do not widen visibility for convenience.

### Exceptions

Unity/vendor/package APIs and Editor tooling that must bind to them. Skip `**/Vendor/**`, `**/Samples/**`, and `**/Plugins/**`.

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

## Field-relative layout

- Anything placed relative to the field, the map or the seat converts through its owner's transform
  (`TransformPoint` / `TransformVector`, parent-space inputs through the local transform). Never add a
  world-axis offset to a field position.
- In the editor the `Origin` sits at the world origin, so world/parent mix-ups pass every editor check
  and break only on the headset. Test such layouts under a moved and yawed parent.
- A `LineRenderer`'s width is world metres and ignores its parent's scale; author it in metres.

## Spawned content

Content that is generated or pooled at runtime **from an authored prototype** is fine — the puzzle field's planets (`FieldPlanetSet` + `ViewPool`) and spawned ships (`SpaceShapingSpawner` + `ShipFlightPool`) are the precedent. Spawn from a serialized prototype and pool it; never invent a mesh, material, or hierarchy with no authored source.

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

## Bakes run off the editor

Seed scans and level bakes take minutes and lock Unity. Never run them in the main editor or through
the Unity MCP.

- Run them in the bake checkout (`../our_space_bake`, branch `bake`) with
  [`docs/tools/bake.ps1`](docs/tools/bake.ps1): `-Job <id> -Mode scan -Seeds <n>` to scan,
  `-Mode bake` to save the pinned seed, `-Mode answers` (no `-Job`) to backfill the stored answers.
  A fleet scan runs its probes on `-Workers` tasks (default: cores − 1); a single-ship scan stays
  sequential. Batch mode calls `PuzzleBatchBake`, which uses the synchronous bake paths
  (`FleetSearchRun` needs `EditorApplication.update`, which does not tick in batch mode).
- Commit the bake code first and merge it into `bake`; the checkout only sees committed code.
- Baked assets come back through git: commit them on `bake`, then merge or cherry-pick. Never copy
  `.asset`/`.meta` files by hand.
- One Unity per checkout, one job at a time: a fleet scan already uses every core, so a second Unity
  only splits memory. Keep working in the main editor meanwhile.
- **A second bake checkout** is the fallback for two very long jobs whose seeds you already know (at
  most two: a third slows the main editor, and merging becomes the bottleneck). A normal scan does
  not need it. A batch Unity holds 3–4 GB.
  1. `git worktree add ../our_space_bake2 -b bake2 bake` (from a branch that has the bake code).
  2. Before its first open, copy `Library/` from `../our_space_bake`, so it skips most of the full
     import.
  3. Run with `docs/tools/bake.ps1 -ProjectPath ..\our_space_bake2 -Job <id> ...`.
  4. Split by job. A job's commit is its puzzle asset, its `.meta`, its answer and its contract pin,
     never `Chapters/<nn>.asset`: the chapter order is set by hand once, after both branches merge.
- The bake-heavy test suite and the shipped-asset checks for fleets run in the bake checkout too
  (`-runTests -testFilter <name>`).
- A plan that bakes says so under its title: `**Bakes:** off the editor — <which units>`.

## Tests — pick the suite, do not launch everything

Two EditMode suites, very different cost:

- `DigitalLove.Game.SpaceShaping.Tests.EditMode` — **fast** (seconds). This is the default pass.
- `DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode` — **bake-heavy**: a full solution search per test, tens of seconds in total. Run it deliberately, not on every change.

`PuzzleShipSweepTests` and `PuzzleShipPlateauTests` are `[Explicit]`: they only run when named. Keep their field/sample counts small — they are smoke checks now, not the original feasibility sweep.

Running tests through the Unity MCP drops the bridge on the domain reload, so job polling can time out or disconnect even though the run finished. Read the result in the Test Runner window; if a run stops starting at all, clear the stuck job first.

