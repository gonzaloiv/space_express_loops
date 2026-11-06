# Gesture prompts — show each gesture the first time

**Status:** now · presentation · updated 2026-11-06 — an authored prompt for each gesture, shown where
the player first meets it and hidden once they do it; learned once per save. Landed: the learned store
and its tests, the view first-use events, and the map pan watch. Next: authored panels and the state
wiring.

**Related:** [core loop](../../design/core-loop.md) · [the cube](../../design/systems/cube.md) ·
[progression](../../design/systems/progression.md) · [loop completion](loop-completion.md) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## Goal

A fresh save opens straight in job 1 (loop completion U5) with no map, no card and no voice. Nothing
tells a first-time player to pinch inside the cube. Each gesture gets one prompt, shown at the moment
the player first needs it — at the start, never as a mid-solve tip — and gone once they have done it
or its time runs out.

## Contract

- **Four prompts, four moments:**

  | Prompt | Where it shows | Shows when | Learned when |
  |---|---|---|---|
  | **Scale** — two-hand pinch inside the cube | the puzzle cube | entering a job, until learned (job 1 on a fresh save) | the first committed pinch |
  | **Enter** — two-hand spread on the selected node | the selection cube | the map opens, until learned (first view is after job 1) | `EnterRequested` — the first enter |
  | **Pan** — two-hand drag | the selection cube | the map is idle for `panPromptSecs`, until learned | `Panned` — another node gets selected, whether or not the prompt was showing |
  | **Move** — one-hand grab on the cube's frame | the puzzle cube | entering a movable job (`translatable`), until learned (job 4) | `Moved` — the grab is released after the cube moved |

  A gesture **completes at its end**: the pinch commits, the grab is released, the spread triggers
  enter, the drag lands on a new node. Starting a gesture never marks it learned.

- **Learned once per save.** A learned gesture never prompts again; saved as a cookie per gesture
  (`gesture_learned_<scale|enter|pan|move>`). Marking writes through immediately (one `Put` per first
  learn, the `CookieHelper.Save` pattern) and logs a failed save, so quitting before the next solve
  keeps it. The rule keys on the gesture, not the job number, so reordering jobs cannot break it.
- **Timed, gesture-closed panels.** Each prompt shows for at most `showSecs`. It cuts off early when
  its gesture **completes**, and only that completion marks it learned. A timeout hides it without
  learning it, so it returns at its next moment rather than lingering for the whole run or nagging
  mid-solve.
- **Independent panels.** Each prompt is its own authored panel with its own anchor. Several may be
  visible at once when their moments overlap — Scale and Move on a translatable job, or Enter and Pan
  on the map (Pan after `panPromptSecs` idle) — with anchors placed so they do not cover each other or
  the field. Each runs its own timer and closes on its own gesture; one never hides another.
- **Authored, not built.** Each prompt is an authored animated icon anchored beside the cube it
  explains, following the cube's position but **outside the scaled hierarchy**: not a child of the
  cube, and on the map not under `mapRoot` either, since `MapOverview` scales it to 0.55 during the
  overview. The cube itself carries a non-unit scale (the zone frame's `localScale` is the job's
  frame size, ~0.09; the selection cube scales to `maxZoom`/`enterScale`), which a child would
  inherit. Runtime code only shows, hides, fades and follows it. It never blocks input and never
  covers the field's read.
- **Not the live indicator.** `TwoHandGestureIndicator` stays as it is — it is feedback *while*
  pinching; prompts say what to do *before*.
- **States show and learn; panels listen and close.** The owning state triggers a prompt and also
  subscribes to the gesture source for the whole run/map: a gesture completed after the prompt's time
  has run out must still mark it learned. On the first completion the state marks + persists and hides
  the prompt. The panel subscribes to the same source in `OnEnable` (unsubscribing in `OnDisable`)
  only to cut itself off early; it does not persist. `PuzzleEditionState` owns Scale/Move,
  `PuzzleSelectionState` owns Enter/Pan. States hide their prompts on exit; the controller and the map
  only report first use.

## Units

### U1 — the prompt and the learned store

- Reuses the shared terminal style (frame, monospace font asset, line-art material) built in the
  [job card](../backlog/job-definition-card.md) U1 — land that unit first. A `GesturePrompt` view (boot in / loop /
  cut off) and the four authored prompt objects, with their optional caption keys in the global
  localization table (`gesture_prompt_<scale|enter|pan|move>`). Each prompt gets its own `showSecs`, a non-scaling
  sibling anchor beside its cube (the cube transform's scale is not 1), and a gesture source it
  subscribes to in `OnEnable` to cut itself off on completion.
- A small learned-gesture store over `PlayerData` cookies: `IsLearned(gesture)`, and `MarkLearned`
  returning whether it added the cookie. The flow persists that change immediately (the
  `CookieHelper.Save` pattern) and logs a failed `Put`; the store itself stays synchronous and
  testable. EditMode test for the store.
- **Exit:** each prompt can be shown, left to time out, and closed by its gesture in the scene;
  marking one learned survives a restart even when the run is left unsolved.

### U2 — the puzzle prompts (Scale, Move)

- `SpaceShapingView` reports first use: `PinchCommitted` after the first pinch that changes
  `Zone.Scale`, and `CubeMoved` after the first completed frame move. The move lives below the view
  (`ZoneGrabListener.Move` runs per frame): a completed grab that moved the cube raises
  `MoveCompleted`, `SpaceZoneView` forwards it as `Moved`, `ZoneCubePool` collects it, and
  `SpaceShapingView` turns the first one into `CubeMoved`.
- `PuzzleEditionState` shows Scale and Move (the latter on a `translatable` job) when not learned; both
  may show on the same job, on separate anchors. Each panel holds for `showSecs` and cuts off on its
  view event. The state also subscribes to those events for the run, marks + persists on the
  first completion (even if the panel already timed out), and hides the prompt. A timeout leaves the
  gesture unlearned until the player performs it. Prompts hide on state exit and on solve/fail, the
  same events that freeze the run.
- **Exit:** fresh save → job 1 shows the Scale prompt; the first pinch cuts it off; it never returns;
  job 4 shows Move until the first move (and Scale too if it is somehow still unlearned).

### U3 — the map prompts (Enter, Pan)

- `PuzzleMapView` raises `Panned` when a drag lands on a different node (the selection changed);
  `EnterRequested` already exists. A pan before `panPromptSecs` elapsed still marks Pan learned, so
  the prompt never shows to someone who already panned. `MapPanWatch` owns the idle timer, ticked from
  the view's existing `Update` and reset on any active gesture.
- `PuzzleSelectionState` shows Enter when not learned, then Pan after `panPromptSecs` idle when not
  learned. Both panels may be visible at once, on separate anchors; each holds for `showSecs` and cuts
  off on its map event. The state also listens to those events for the map's lifetime, marks +
  persists on the first completion (even after a timeout), and hides the prompt; a timeout leaves that
  gesture unlearned. Tune `showSecs` above `panPromptSecs` if the two should overlap on the first view.
- **Exit:** the first map view (after job 1) shows Enter on the selected node; idling adds Pan clear of
  it; spreading completes Enter, dragging completes Pan; neither returns.

## Manual check (headset)

1. Fresh save: the app opens in job 1 and the Scale prompt plays on the cube; nothing else shows.
2. Pinch and release inside the cube: the prompt cuts off and does not come back this job or after
   reset.
3. Solve job 1: the map opens with the Enter prompt on the selected node (job 2).
4. Wait without moving: the Pan prompt appears clear of Enter; drag to another node and Pan cuts off.
5. Spread to enter: Enter cuts off; reopening the map shows no prompt.
6. Learn Pan and Enter, quit before entering a job, then relaunch: neither prompt returns and job
   progress is unchanged.
7. Reach job 4: the Move prompt shows on the cube; release one move and it cuts off.
   If Scale is still unlearned, both show together, clear of each other, and close independently.
8. Quit and relaunch: no learned prompt returns.
9. The prompts never hide the start run, the planets or the TTS panel.
10. Fresh save, no pinch: the Scale prompt cuts off after `showSecs`; re-enter the job and it shows
    again, then learn it with one pinch.

## Decisions

- **Look: animated icons in the terminal style (decided 2026-11-06).** One authored icon per gesture
  (two pinching hands closing/opening, a spread, a drag, a one-hand grab on the frame), looping a
  short animation, drawn as the console's phosphor line art inside its frame
  ([lore](../../design/world/lore.md) §The tech team — the console). Flat, world-space, billboarded
  toward the head, sized to read at bubble distance over passthrough. Authored sprites or sprite
  sheets on the prefab; no runtime textures.
- **The icon carries the meaning; the caption is optional.** At most one short monospace line per
  prompt (`> PINCH TO SCALE`), keyed `gesture_prompt_<scale|enter|pan|move>` in a localized table; the
  prompt must read without it.
- **Time is a fallback; completion is the learned signal (decided 2026-11-06).** Each prompt has a
  serialized `showSecs`, long enough to read its caption. The gesture cuts it off sooner and only that
  completion marks it learned; a timeout does not, and the prompt returns next time.
- **Boot and power-off.** The frame boots in on show and cuts off like a CRT on completion or timeout.
- **Two panels may share a cube (decided 2026-11-06).** Scale/Move and Enter/Pan can be up together
  when their moments overlap; each gets its own anchor and time so they stay clear of each other.
- **Shared with the job card.** The frame, font and line-art material are one authored style, built by
  the [job card](../backlog/job-definition-card.md) U1; this plan reuses it.

## Out of scope

- Teaching the shrink direction (Jim's job 3 line covers it), the scale-out exit, resize.
- Hints that react to being stuck; voice for prompts.
