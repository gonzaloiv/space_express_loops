# Playable map field — idea

**Status:** idea · flow · updated 2026-11-06 — the selected map node becomes the play field in place.

**Why:** no second object, no pose jump; the map's "scale the space" moment; the vision's one view with
no transition ([vision](../design/vision.md) §3).

**Why not now:** the map read must land and Validation 1 is the gate
([vision](../design/vision.md) §6–7). The work is a project, not a morning: the play-only group on the
node, the transition rig, the return soft-reset, and the neighbours.

**When promoted, decide:**

- The selected prototype carries the full play rig, or the existing play prefab is posed to match.
- Neighbours visible behind play or hidden; the map root must not scale the selected node.
- Start check: build the session on enter, or split `Prepare` / `Start` and share it.
- After a solve: soft-reset to preview, light the node, move the selection to the next job.

**Risks:** play-only group completeness, prefab cost at 72 fps, the mutated field on return, and the
missing transition rigs (`BubbleScaleSession` / `UniverseContentRig`).
