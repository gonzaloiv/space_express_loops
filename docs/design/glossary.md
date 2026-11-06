# Glossary — the fixed terms

**Status:** design · glossary — the one place terms are defined.
**Date:** 2026-10-23.
**Revised:** 2026-11-07.

**Related:** [vision](vision.md) · [the cube](systems/cube.md) · [core loop](core-loop.md)

One term per idea. In-game copy uses the plain terms; the physics names stay internal.
**No other physics jargon** (no Schwarzschild, Lense-Thirring, etc.).

Use the **first name** in each row in design prose; the parenthesised aliases are for code, fiction, or
the UI. Do not introduce a third word for the same idea.

| Term | Means |
|---|---|
| **Our Space** | The game's name (your space + outer space). |
| **the cube (the lens)** | The one instrument: the cube frame plus the grab-inside scale, taken together. Use **cube** in design prose; **lens** is the concept alias, not a second object. |
| **zone** | The space inside the cube that a scale is applied to; the unit the scale acts on. |
| **zones** | The brief field: how many zones the solution scales (1–3). Not a move count. |
| **moves** | The player moving the cube; the `translatable` permission, not a count. The brief field for the zone count is **zones**. |
| **the scale** | The verb: a two-hand pinch *inside* the cube, scaling the space and the content. |
| **the region** | The cube's frame (edges/corners) — moving / resizing the cube; set separately from the scale. A translatable level starts it at the field centre. |
| **the aperture** | The cube's size — how much of the field one edit covers. Scope, not precision; the pinch sets magnitude. The level sets it; resize is deferred. |
| **space face** | The scale read as **space** — the metric stretch that is a **shortcut** and bends the path. |
| **the Wrap** | The in-fiction system that wraps spacetime (the tech team's product). |
| **shaping** | The in-game verb for using the Wrap; the design verb is **the scale**. |
| **entropy** | The bar: the Wrap's cost. Every committed edit burns it; the grade is what is left, awarded only on a solve, and spending the budget before a solve **fails the job**. The field carries the state: it strains as entropy drains and looks about to blow near empty. |
| **eco rating (stars)** | The grade from entropy left, shown as **1–3 stars** and awarded only on a solve. One concept: use **eco rating** in prose, **stars** for the display. |
| **the arrival** | The solve: the ship reaching the destination. The goal of every job. |
| **fuel** | The launch you are *given* (the brief) — not a player lever. **Global** — one speed, the same for every job. |
| **the field** | The derived acceleration/potential the ship flies through. |
| **the lattice (the grid)** | The visual spacetime mesh the player's zones warp; flat until a zone is applied. Use **lattice** in design prose; **grid** is the code name. |
| **straight-line reference** | The origin→destination line a flat field would produce; the baseline the player compares the warped trajectory against. |
| **well** | A planet's gravity well. A bigger planet makes a deeper well (`mass ∝ radius³`) and bends the path more. A smaller one is a smaller obstacle on the line. |
| **obstacle** | A body on the origin→destination line. Shrinking it shrinks its collision radius, so the path can clear. |
| **curve / clear** | The two roles of the scale: **grow to curve** (a well bends the path), **shrink to clear** (a smaller obstacle leaves the line). A multi-zone solution can mix them. |
| **readout** | The derived surface for one run: result, entropy and stars. The final bar lives **in the field**, not a panel; the lab panel is a diagnostic. |
| **bubble** | The playfield: the miniature universe anchored in the room. |
| **job (level, contract)** | One delivery — a cube on the map; ~1 minute each, **3–5 first**, 7 in the run. **Contract** is the fiction word. |
| **multi-ship** | A job with more than one run; it is solved only when **every ship arrives**. The runs relate as fan-out, converge, or separate. |
| **the seed** | The authoring recipe for a job — its field, brief conditions and dials; the **level** is the baked result. |
| **the map (level selector)** | The field of numbered cubes, one per job; solving lights them. **Direct pick only**; the result offers Map/Retry. |
| **the sun** | The source at the field centre. Scalable by its centre by default; a puzzle can lock it to fixed mass. Job 1 has a sun the cube does not cover; the centre job makes it the lesson. Never a run endpoint. |
| **the company** | The lore wrapper (identity, contracts); flavour only, no economy. |
| **the console** | The company-issued Wrap client: a portable dispatch terminal that projects the bubble into the player's room and carries the contract map between jobs. It explains MR anchoring in-fiction; furniture and the map, not a second interactive system. |
| **Room / Bubble** | The **bubble** is the play view (the miniature universe anchored in the room); **Room** is the legacy room-scale view, **cut** ([vision](vision.md) §3). |
