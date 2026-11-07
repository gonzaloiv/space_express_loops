# Explorable map — future idea

**Status:** idea · presentation · updated 2026-11-07 — a solved counter on the map, and the puzzles
near what you have solved open up, so moving the cube around the map is part of the game.

**The idea.** The map shows a counter ("7 of 10 passed"). Instead of a single "next" job in a list,
every puzzle near a solved one is playable. With the map cube ([the cube](../design/systems/cube.md) §The map uses the same cube),
moving the cube and the cull around it reveal places the player can actually go, not only a view.

**The tension: the lessons are ordered.** Jobs 1–7 are a curriculum, one lesson each (the scale, the
sun, the other direction, the move, multi-zone…; [jobs](../design/jobs.md)). If anything nearby is
playable, a player can reach multi-zone before the move.

**Likely shape.**

- **A spine plus side puzzles.** The 7 lesson jobs stay an ordered spine through the galaxy. Optional
  side puzzles sit near spine nodes and open once a neighbour is solved; each uses only lessons the
  spine has already taught.
- **The counter counts everything** (spine and side), so "of 10" means about three extra baked
  puzzles. Cheap to show: `RunProgress.SolvedCount` already exists.
- **Moving becomes needed.** Finding side puzzles means moving the cube, so the map would teach the
  grab (a Move prompt, perhaps from the second map onwards). Today the map cube plan does not teach it
  because the next job is preselected.

**What it would take.** A proximity unlock rule in `PuzzleMap.ApplyProgress` (beside the spine's
order), the counter label, side-puzzle contracts and bakes, a Galaxy layout that places side puzzles
near their spine node, and the Move prompt on the map.

**Why not now.** The map cube lands first; it is the same under either unlock rule. Extra puzzles need
bake time (job 1 showed how narrow a contract can be).

**Related:** [the cube](../design/systems/cube.md) · [jobs](../design/jobs.md) ·
[progression](../design/systems/progression.md)
