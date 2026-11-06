# Gesture prompts — show each gesture the first time

**Status:** next · presentation · updated 2026-11-06 — an authored prompt for each gesture, shown where
the player first meets it and hidden once they do it; learned once per save. Before Validation 1.

**Related:** [core loop](../../design/core-loop.md) · [the cube](../../design/systems/cube.md) ·
[progression](../../design/systems/progression.md) · [loop completion](../in-progress/loop-completion.md) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## Goal

A fresh save opens straight in job 1 (loop completion U5) with no map, no card and no voice. Nothing
tells a first-time player to pinch inside the cube. Each gesture gets one prompt, shown at the moment
the player first needs it — at the start, never as a mid-solve tip — and gone once they have done it.

## Contract

- **Four prompts, four moments:**

  | Prompt | Where it shows | Shows when | Learned when |
  |---|---|---|---|
  | **Scale** — two-hand pinch inside the cube | the puzzle cube | entering a job, until learned (job 1 on a fresh save) | the first committed pinch |
  | **Enter** — two-hand spread on the selected node | the selection cube | the map opens, until learned (first view is after job 1) | the first enter |
  | **Pan** — two-hand drag | the selection cube | the map is idle for `panPromptSecs`, until learned | the first pan that changes the selection |
  | **Move** — grab the cube's edges | the puzzle cube | entering a movable job (`translatable`), until learned (job 4) | the first committed move |

- **Learned once per save.** A learned gesture never prompts again; saved as a cookie per gesture
  (`gesture_learned_<scale|enter|pan|move>`) through the same `PlayerData` save as solves. The rule
  keys on the gesture, not the job number, so reordering jobs cannot break it.
- **One prompt at a time.** Enter shows first on the map; Pan only after Enter's idle wait.
- **Authored, not built.** Each prompt is an authored animated icon, a child of the cube it explains;
  runtime code only shows, hides and fades it. It never blocks input and never covers the
  field's read.
- **Not the live indicator.** `TwoHandGestureIndicator` stays as it is — it is feedback *while*
  pinching; prompts say what to do *before*.
- **States own it.** `PuzzleStartState` shows Scale/Move; `PuzzleSelectionState` shows Enter/Pan. The
  controller and the map only report the first use (an event); they do not know about prompts.

## Units

### U1 — the prompt and the learned store

- The shared terminal style (frame, monospace font asset, line-art material) unless the job card
  built it first; a `GesturePrompt` view (boot in / loop / cut off) and the four authored prompt
  objects, with their optional caption keys.
- A small learned-gesture store over `PlayerData` cookies: `IsLearned(gesture)`, `MarkLearned(gesture)`
  (persists once). EditMode test for the store.
- **Exit:** each prompt can be shown and hidden in the scene; marking one learned survives a restart.

### U2 — the puzzle prompts (Scale, Move)

- `SpaceShapingController` raises `PinchCommitted` and `CubeMoved` on the first committed edit / move
  of a run; `PuzzleStartState` shows Scale (and Move on a `translatable` job) when not learned, and
  hides + marks each on its event. Hidden on state exit and on freeze.
- **Exit:** fresh save → job 1 shows the Scale prompt; the first pinch hides it; it never returns;
  job 4 shows Move until the first move.

### U3 — the map prompts (Enter, Pan)

- `PuzzleMapView` raises `Panned` (a pan that changed the selection); `EnterRequested` already exists.
  `PuzzleSelectionState` shows Enter when not learned, then Pan after `panPromptSecs` idle when not
  learned; each hides + marks on its event.
- **Exit:** the first map view (after job 1) shows Enter on the selected node; idling shows Pan; both
  stop once used.

## Manual check (headset)

1. Fresh save: the app opens in job 1 and the Scale prompt plays on the cube; nothing else shows.
2. Pinch and release inside the cube: the prompt fades and does not come back this job or after reset.
3. Solve job 1: the map opens with the Enter prompt on the selected node (job 2).
4. Wait without moving: the Pan prompt appears; drag to another node and it fades.
5. Spread to enter: Enter is learned; reopening the map shows no prompt.
6. Reach job 4: the Move prompt shows on the cube; move it once and it is gone.
7. Quit and relaunch: no learned prompt returns.
8. The prompts never hide the start run, the planets or the TTS panel.

## Decisions

- **Look: animated icons in the terminal style (decided 2026-11-06).** One authored icon per gesture
  (two pinching hands closing/opening, a spread, a drag, a grab on an edge), looping a short animation,
  drawn as the console's phosphor line art inside its frame ([lore](../../design/world/lore.md) §The tech
  team — the console). Flat, world-space, billboarded toward the head, sized to read at bubble distance
  over passthrough. Authored sprites or sprite sheets on the prefab; no runtime textures.
- **The icon carries the meaning; the caption is optional.** At most one short monospace line per
  prompt (`> PINCH TO SCALE`), keyed `gesture_prompt_<scale|enter|pan|move>` in a localized table; the
  prompt must read without it.
- **Boot and power-off.** The frame boots in on show and cuts off like a CRT when the gesture is learned
  — the small payoff for doing it.
- **Shared with the job card.** The frame, font and line-art material are one authored style that the
  [job card](job-definition-card.md) also uses; build it once in U1.

## Out of scope

- Teaching the shrink direction (Jim's job 3 line covers it), the scale-out exit, resize.
- Hints that react to being stuck; voice for prompts.
