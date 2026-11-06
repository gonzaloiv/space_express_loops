# Map visibility — see progress on the current map

**Status:** next · presentation · updated 2026-11-06 — grow the current map: node state and stars on
the label. Same view, same culling, same pan.

**Related:** [progression](../../design/systems/progression.md) ·
[loop completion](../in-progress/loop-completion.md) · [Game.unity](../../../Assets/_Game/Game.unity)

## Goal

The current map stays: pan under the selection cube, distance culling (discovering the map is the
feel), the looped route, two-hand spread to enter. What it lacks is a read of the run — which jobs are
done and which is next. This plan adds that without changing the view.

## Contract

- **Keep:** the pan, the snap, the culling/fade, the looped route, the overview, the enter gesture.
- **Locked nodes stay as they are** (their field shows).
- **The map takes the next puzzle:** on open and on return it selects the next unsolved job —
  already true (`PuzzleMap` selects `progress.CurrentIndex`). A solve moves the selection forward;
  there is no end-of-run special case, so a following puzzle becomes the current job.
- **Stars ride the node:** a solved node shows its grade (1–3) on the authored `Stars` TMP
  (`Game.unity:3946`, currently inactive). Job 1 is `noGrade`, so it shows none — never `0/3`.
- All new visuals are authored on the prefab/scene and only shown, tinted or rotated at runtime.

## Units

### U1 — state on the node label

- The existing label ("N · name") takes the job's state tint from `SelectionCubePalette`: solved
  steady, current a slow pulse (only one), locked dim. It fades with the node's existing culling.
  `PlanetNameLabel` only writes alpha today (`PlanetNameLabel.cs:81`), so add a tint input next to
  `fadeMultiplier`; do not duplicate the state on a second label.
- A solved node shows its stars on the authored `Stars` TMP (`Game.unity:3946`); job 1 is `noGrade`
  (`PuzzleData.cs:33`) and shows none. Carry `noGrade` into `JobEntry` so `MapSelectionLabel` can
  stop rendering `0/3` for a tutorial solve (`MapSelectionLabel.cs:10`).
- Job 1 has no sun, so no label today: author one centre label for that case at the field centre,
  where the sun is expected (the diorama root), and show it only when the sun is absent —
  `ShowSunLabel` hides the label when no sun spawned (`FieldPlanetSet.cs:105`).
- **Exit:** within the culled view, a player can tell solved, current and locked nodes apart without
  selecting them; job 1 shows its number and no stars; a 1-star solve reads 1.

## Manual check (headset)

1. Fresh save: the app starts in job 1 (loop completion U5); leave it with the scale-out gesture —
   job 1 is selected, its label shows its number and no stars; nearby labels read locked.
2. Solve job 1 and return: job 2 is selected and pulses; job 1's label reads solved with no stars;
   the route and culling are unchanged.
3. Solve a graded job: its label reads solved with 1–3 stars.
4. Pan far: the culling still hides distant nodes and their labels.
5. Two-hand overview: every visible node keeps its state colour and stars.
6. Reopen after quitting: states and stars are restored from the saved solves.

## Out of scope

- A new layout, all-in-view, or point-and-pinch selection.
- Hiding locked fields; the enter crossfade; the overview gesture.
- A direction arrow to the next job and progress on the route (removed from this plan).
