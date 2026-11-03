# Jobs — the run, one delivery at a time

**Status:** design · content · jobs.
**Date:** 2026-10-27.
**Revised:** 2026-11-02.

**Related:** [progression](systems/progression.md) · [lore](world/lore.md) ·
[the cube](systems/cube.md) · [glossary](glossary.md)

## Goal

The **concrete run**: the jobs the player actually plays, one row each — their brief, field and entropy
scale (the shape [progression](systems/progression.md) sets), plus their **lore skin**. Progression is
the general system; this is the specific development of it. Counts are a **stretch goal** — prove 3–5
first.

## The run — the mechanics

| # | Teaches | Role | Brief (translatable · zones · resizable · ships) | Shape | Field (planets + sun · wells · depth) | Entropy scale (derived) | Why |
|---|---|---|---|---|---|---|---|
| 1 | the scale | Curve | fixed · 1 zone · no · 1 ship | Ring | 3 · single · tutorial | low | first contact: a viable three-planet field, no sun, one pinch |
| 2 | the centre | Curve | fixed · 1 zone · no · 1 ship | Ring | 4–6 + sun · single · medium | medium | the fixed cube over the sun — the star is a source too |
| 3 | several zones | Curve | fixed · 2+ zones · no · 1 ship | Ring | 6–8 + sun · multi | medium | "where do I scale" enters — the fixed cubes are on planets |
| 4 | the cube moves | Curve | **yes** · 1 zone · no · 1 ship | Spiral | 8–10 + sun · multi | medium | spawns at the centre; the solution is off-centre, so the move is required |
| 5 | multi-zone intro | Curve | yes · 2+ zones · no · 1 ship | Spiral | 10–12 + sun · multi | medium | move + scale two zones — the combination enters |
| 6 | efficiency | Curve | yes · 2+ zones · no · 1 ship | Cluster | 10–12 + sun · multi | high | the cheap answer is to spread the edit across zones |
| 7 | the capstone | Curve | yes · 2+ zones · no · 1 ship | Cluster | 12 + sun · mixed · deep | high | multi-zone + the centre — starts over the sun and scales several zones |

**The role.** Each job states the role its reference solution must teach: **Curve** (a well grew),
**Clear** (an obstacle shrank), **Mixed** (both, at least two zones), or **Any** (no request). It is a
**bake request**, not a player condition. The creator searches for a matching solve and rejects the seed
when it cannot find one; it also rejects a seed when a **cheaper or equal wrong-role route** exists,
because the taught role must be the best answer. More expensive alternate routes may remain. The 7-job
spine requests **Curve**; the **Clear** obstacle and the **Mixed** clear+curve are later variations
([archetypes](#the-run--archetypes)). A `Curve`/`Mixed` seed is rejected when its start path stays under
5% of the chord or 4° off it: the start must show the field. The bake reports the reference solution's
`kind`; the player never sees the label. Multi-ship role requests are not supported yet.

**The field is a galaxy, not a handful of balls.** The planet column is the **source** count — the bodies
that bend the path. The field also carries **decor** (dust, moons, distant planets) from
[the element pack](../ideas/element-pack.md) once that lands, so the playfield reads as a populated system
without every body becoming a well. The tutorial stays sparse at three; the count rises by act, so the
galaxy grows with the run.

**The shape.** Each job states its `fieldShape`; `Auto` is for experiments only. Shape is a read axis,
not a difficulty dial — it never changes a rule — so it marks an **act**: **Ring** carries the
fixed-cube opening (1–3), **Spiral** the move act (4–5), **Cluster** the mastery act (6–7). A job may
turn the silhouette when the act turns; the new verb is still the lesson.

**The sun.** The field can place a **sun** at its centre; it is scalable by its centre by default, is
never a run endpoint, and is not counted in the planet column ([the cube](systems/cube.md)). A seed can
omit it; the level can lock it; the settings size it.

**The sun in the first jobs.** Job 1 has **no sun**: the only sources are planets. Job 2 introduces the
sun with a fixed cube over the centre, so the star is in the zone by construction. No lock; after job 2
the sun is a normal source in the later levels.

**Job 1 is a baked tutorial field.** It bakes from a normal seed — three planets, no sun, one fixed zone
— with the tutorial flags (no fail, no stars). The field has more than one planet; the lesson is the
cube's scale, not a single-source field. It still records a reference solution, so the budget stays `3×`
and the star bands keep their shape. Job 1 has **no entropy fail and no stars**: the bar is
informational, and the fail and grade rules start at job 2. The read test runs without the brief/TTS line.

**Entropy scale.** The bake sets the budget to **3×** its solution's entropy; the labels are the
absolute cost of that solution. The star thresholds stay **1/3 and 2/3** of the budget, so the grade
pressure has the same shape every graded job ([progression](systems/progression.md) §Tuning); job 1 is
informational.

**The several-zones rung** is owned by [progression](systems/progression.md) §The several-zones rung:
a fixed level places several cubes, so `zones ≥ 2` does not need a move. Job 3 teaches that; job 4
adds the move; multi-zone follows at job 5; the capstone adds the centre at job 7.

**Job 3 keeps the sun out of the zones.** The fixed cubes sit on planets; the sun is background, so the
lesson stays "where do I scale," not "what is the star."

**The move is guaranteed.** A translatable level starts the cube at the field centre; job 4's solution
includes an off-centre zone, so the player must move the cube.

**The capstone is the centre + multi-zone.** Job 7 starts the cube over the sun and the solution scales
several zones, the star included; the player must move out to the other zones. It is the only job that
combines every verb.

**Resize is deferred.** `resizable` works, but the slice does not use it: the explanation (size is scope,
not precision) is not ready. It is the next cut candidate.

**The slice.** Jobs 1–7 are the full run, ~1 minute each.

**Multi-ship.** A job may carry more than one run; it is solved only when **every ship arrives**. The
runs are baked as **fan-out**, **converge**, or **separate**, so the shared field stays legible.
Multi-ship is available now; the 7-job spine above is single-ship. A multi-ship job can be authored as
a stretch or a later combine job.

## The run — the brief

Each job's puzzle carries a **brief**; the per-level conditions are the source of truth here, and the
creator bakes to them. Three groups.

**Player conditions** — what the level allows:

| Field | Means |
|---|---|
| **`translatable`** | The cube may be **moved** (its centre changes) or is **fixed** at home. How many times the player moves it is their choice; it is not a baked count. |
| **`zones`** | How many **zones** the solution scales (1–3). **`zones ≥ 2` holds with `translatable` either way** — a **fixed** level places that many cubes, a **translatable** one has the player move one cube between zones. |
| **`resizable`** | The frame may be **resized** (`[1×, 2×]`) or is locked. Deferred from the slice until the explanation is ready. |
| **`ships` / `relation`** | How many runs the job has (1 = single-ship) and how they relate: **fan-out** (one origin), **converge** (one destination), or **separate**. |

**Bake request** — what the creator must produce:

| Field | Means |
|---|---|
| **`mechanism`** | The role the bake's reference solution must have: **`Any`** (no request), **`Curve`** (a well grew), **`Clear`** (an obstacle shrank), or **`Mixed`** (both; needs ≥2 zones). The search filters to the requested role; a seed that cannot bake it is rejected, and so is a seed whose cheapest route has the wrong role. More expensive alternate routes may remain. |
| **`minRoutes`** | Minimum distinct arriving routes the bake must offer. `0` = auto: 2 for a movable single-zone level, otherwise 1. The capstone jobs (6–7) set 2. |

**Baked results** — what the creator records, not a player condition:

| Field | Means |
|---|---|
| **`routes`** | Distinct zone sets that arrive within budget. The bake enforces `minRoutes`: a movable single-zone level and the capstone jobs (6–7) need at least two; fixed and intro levels may have one. |
| **`entropyBudget`** | **3×** the entropy of the bake's solution; the star thresholds are 1/3 and 2/3 of it. |
| **`sources` / `frames`** | The baked field (planets, sun, positions) and the cube frames the level places. |

**Invariant:** **`scalable`** is always true — the level always offers the scale. A source can be locked
(the sun), but the level still has the verb.

Every job, including the job-1 tutorial, is defined on its **seed** — the field, the brief conditions and
the entropy scale — and the **level** is the baked result of that seed. Job 1 is a normal bake with the
tutorial flags (no fail, no stars), not a hand-authored field. The seed is the recipe; the level is what
the creator bakes from it, once. Body sizes are not a seed
value: the sun's radius and the planet range live in the settings, so a planet reads the same size in
every job.

## The run — archetypes

An archetype names the lesson a job teaches. It is read from the brief and the field, so each job reads
as one lesson and the set reads as a catalogue. Archetypes stack: a job has one primary archetype and may
add a layout.

| Archetype | Brief | Lesson | Jobs |
|---|---|---|---|
| **Shortcut** | `zones 1`, fixed cube, on-path | scale one zone | 1 |
| **Centre** | `zones 1`, fixed cube over the sun | scale the star itself | 2 |
| **Several zones** | `zones ≥ 2`, fixed cubes | scale and blend the placed zones | 3 |
| **Move** | `translatable`, `zones 1` | place the cube yourself | 4 |
| **Multi-zone** | `zones ≥ 2`, `translatable` | place and blend several zones — a spread edit can spend less entropy | 5–6 |
| **Capstone** | `zones ≥ 2`, `translatable`, cube over the sun | combine the centre + several zones | 7 |
| **Obstacle** | `zones 1`, cube on a body on the line | shrink the blocker so the path clears | later |
| **Mixed** | `zones ≥ 2`, one on the blocker, one on a run planet | clear with one zone, curve with another | later |

**The obstacle is the second role.** A body on the straight line is an obstacle; reducing its scale
reduces its collision radius, so the path can pass. It is a **later variation**, not job 1: the tutorial
keeps the curve. With several cubes, one can sit on the obstacle (shrink) and one on a run planet (grow)
— a clear and a curve in one multi-zone solution. The creator tags the role so a job can request it.

**Multi-zone as an efficiency lesson.** Each committed zone edit costs its net change from that zone's
last committed scale, so a level can be authored so the **cheap** solution is to **move the cube** and
scale **several zones a little**, rather than one zone a lot — spreading the edit beats one big edit on
the grade.

**The centre is the reveal.** Job 2 **fixes** the cube over the sun, so the star is in the zone by
construction and the player scales it with the same verb as a planet. It lands before the move lesson, so
the player meets the sun with a fixed cube and only later learns to place the cube.

Shape is a second axis, from `fieldShape`: **Ring** and **Spiral** are thin discs with a central
bulge; **Cluster** is a 3D ellipsoid. The field may be tilted off the XZ plane; the cube and its
gestures stay world-axis-aligned. The run turns the shape by act (Ring → Spiral → Cluster), so a job is
e.g. *Multi-zone · Spiral*. Role is a third axis: a zone **curves** (grow a well) or **clears** (shrink a
blocker); a multi-zone job can mix one of each.

Each archetype should have **more than one route** where the brief allows. A job with one route is a
single answer, not a variation.

*Automatic labelling from the brief is possible (a pure function of the brief) and is deferred; the labels
are design-only until then.*

## The run — the lore skin

Each job is a **contract**: a client, a cargo, and the manual entry that teaches its lesson.
One line each; never exposition ([lore](world/lore.md)).

| # | Client | Cargo | Tech voice | Manual entry |
|---|---|---|---|---|
| 1 | Blödmeyer & Sons (no sons) | a pallet of valves | "One pinch. You'll be fine." | "Welcome. Your Wrap does one thing. Do that one thing." |
| 2 | The Lighthouse Keepers' Union | one bulb | "The star is a source too." | "The star is not a planet. It bends the same way." |
| 3 | Two Guys, One Moon | spare pump parts | "Two zones now. Scale them both." | "The Wrap scales where it sits. Two of them, this time." |
| 4 | Madame Fink's Discount Gravity | a piano | "Now move it. *Where* is a choice." | "The Wrap is yours to place. Where you work is a decision. Make it." |
| 5 | The Third Best Bakery | a wedding cake, still warm | "Several zones. Place it, scale them." | "The Wrap is yours to place. Use it more than once." |
| 6 | Nobody & Co. | a body (a funeral) | "Tight. Really tight. Sorry." | "Every edit costs. The customer only ever sees the arrival." |
| 7 | The Inheritance of Mr. Fink | the last of something | "…Good luck." | "This is the one the manual was written for." |

## The manual's arc

The tip card is the company's **onboarding manual for new employees** ([lore](world/lore.md) §The tech
team): short, dry, corporate, slightly out of date. It plays on the TTS card as the job's intro — the
manual entry and the tech-voice line, two entries per job. Each entry introduces **one idea**, so it
carries the teaching *and* the fiction. It opens warm and ends corporate-bleak; the "new employee" it
keeps addressing never arrives. Two beats to keep:

- The opening line: *"Welcome, new employee."*
- A late note: *there has not been a new employee in eleven years.*

**Two arcs:** the **tech voice** goes pride → doubt; the **manual** goes cheerful → bleak. The **client
names stay absurd** — the tonal relief.

## Seeds (later)

- **Per-job launch lines.** If the launch later becomes per-job (a variation, not a lever), each job's
  *given* launch becomes a beat — a launch that would fail, made to land.
- **The manual as a findable object.** The full manual in the office, with the entries the run skipped.
- **Client callbacks.** An absurd name returns in a later job, with a grudge. **Slightly Used Stars
  Ltd.** is the first: the move lesson outgrew its "You can move it now" contract.
- **A returning contract.** **The Very Small Opera** (a live animal — "Efficiency is a virtue until it
  is a cost") waits for a later combine job.

## Open questions

- **Routes as a mechanic?** If a late job (7–8) has more than one route, multiple routes stops being an
  authoring guard and becomes a principal mechanic. Decide whether the run teaches it.
- **The capstone jobs.** What jobs 5–6 vary (planet count, entropy scale), and whether job 7 combines the
  centre + multi-zone as the finale or introduces a new variation.
