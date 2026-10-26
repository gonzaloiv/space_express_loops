# Plans — the board

The single source of truth for what is being built: **Now → Next → Later**. Update this file when a
plan changes state; each plan carries a matching `**Status:**` line.

**Direction:** [vision](../design/vision.md) — the **lens** (the cube):
scale the space, scale the content; the ship flies the deformed spacetime.

## Now

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — puzzle creator](in-progress/space-shaping-puzzle-creator.md) | space shaping | Seed → verified `PuzzleData` variants, editor/offline. The `SectorField → SourceSpec` bridge and the field-region → puzzle mapping. |
| [Space shaping — travel on every modification](in-progress/space-shaping-travel.md) | space shaping | Replace the solved-and-idle travel trigger with travel on every modification (no commit); pass / fail / free retry. |

## Next

The runway after Now: the **device gate** (the gate criteria are in
[vision](../design/vision.md) §12), then — only if it passes — integration.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — integration into the game](backlog/gameplay/space-shaping-integration.md) | space shaping | The largest, last slice: the puzzle loop replaces the legacy route/round/economy flow. **Gated on the device gate.** |

## Later

Design-only or deferred. Nothing here is scheduled.

| Plan | Area | Why here |
|---|---|---|
| [Space shaping — feel & presentation polish](backlog/presentation/space-shaping-feel.md) | presentation | The *feel* beats after the gate: travel payoff, collapse feedback, window gauge, cargo personality. |
| [Space shaping — variations](backlog/gameplay/space-shaping-future.md) | space shaping | Recorded: multi-ship cap, loop route, level generation. After the gate. |
| [Universe — element pack](backlog/universe/universe-element-pack.md) | universe | Later ambient/decor set for the sector field: belts, moons, nebulae, stations… Not scheduled. |
| [Hologram look](backlog/presentation/hologram-look.md) | presentation | Design only; hologram pass for the bubble view, then the edition view. Independent. |
| [TTS voice panel](backlog/presentation/tts-voice-panel-personality.md) | presentation | Design only; camera-operator companion behavior for the TTS panel. |

## Open tails

Loose ends that still need a home. Close one by linking the plan that finishes it.

| Open tail | Lands in |
|---|---|
| Silent planets — persistence/restore validation | A future cross-feature restart pass |
| Silent planets — migrate the decor pool into the field's ambient sectors | [Element pack](backlog/universe/universe-element-pack.md) |
| Sector field — raise `viewRadius`/aperture | Trigger: when gameplay content moves onto the field → [integration](backlog/gameplay/space-shaping-integration.md) |
| The lens — surface the session `Undo`; **rotate** (job 4) | [Feel](backlog/presentation/space-shaping-feel.md) / [variations](backlog/gameplay/space-shaping-future.md) |

## Conventions

- Every plan starts with a status line:
  `**Status:** <state> · <area> · updated <yyyy-mm-dd> — <one-line summary>`
  where `<state>` is one of `now`, `next`, `later`.
- `<area>` is one of `space shaping`, `universe`, `presentation`, `flow`.
- Folders: `now` → `in-progress/`; `next` and `later` → `backlog/`. Move the file and update this
  board in the same change.
- Run `docs/plans/check-plans.ps1` (or `/check-plans`) after moving files — it verifies links and
  status/folder consistency.
