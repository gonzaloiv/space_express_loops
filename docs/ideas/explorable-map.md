# Explorable map — future idea

**Status:** idea · presentation · updated 2026-11-26 — the puzzles near what you have solved open up,
so moving the cube around the map is part of the game.

**The idea.** Instead of a single "next" job, every puzzle near a solved one is playable. With the
map cube ([the cube](../design/systems/cube.md) §The map uses the same cube), moving the cube and the cull around it reveal places the player can actually go, not only a view.

**The tension: the lessons are ordered.** The 23 story jobs are a curriculum across three chapters
([jobs](../design/jobs.md), [chapters](../design/chapters.md)). If anything nearby is playable, a
player can reach the fleet or the broken bodies before the move.

**Likely shape.**

- **A spine plus side puzzles.** The story jobs stay an ordered spine through the galaxy. Optional
  side puzzles sit near spine nodes and open once a neighbour is solved; each uses only lessons the
  spine has already taught.
- **The side puzzles may already exist.** The 20 freelance contracts ([freelance](../plans/backlog/freelance.md))
  open after the story today; placing them beside the spine nodes that teach their lesson is the
  cheapest version. The map already shows a per-chapter tally (`map_chapter_tally`).
- **Moving becomes needed.** Finding side puzzles means moving the cube, so the map would teach the
  grab (a Move prompt, perhaps from the second map onwards). Today the map does not teach it, because the next
  job is preselected.

**What it would take.** A proximity unlock rule beside the spine's order (where the map applies
progress today), a Galaxy layout that places the freelance (or new side) puzzles near their spine node, and
the Move prompt on the map.

**Why not now.** The store release (6 Dec) ships the ordered run; this changes the unlock rule and the
map layout, a project for an update after real players have played the spine.

**Related:** [the cube](../design/systems/cube.md) · [jobs](../design/jobs.md) ·
[progression](../design/systems/progression.md)
