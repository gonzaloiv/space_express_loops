# Jobs — the run, one delivery at a time

**Status:** design · content · jobs.
**Date:** 2026-10-27.
**Revised:** 2026-11-13.

**Related:** [progression](systems/progression.md) · [lore](world/lore.md) ·
[the cube](systems/cube.md) · [glossary](glossary.md)

## Goal

The **concrete run**: the jobs the player actually plays, one row each — their brief, field and entropy
scale (the shape [progression](systems/progression.md) sets), plus their **lore skin**. Progression is
the general system; this is the specific development of it. All eleven v1 jobs are baked.
This doc is **chapters 1–2**; the store release's 2-5 and chapter 3 are in [chapters](chapters.md).

## The run — the mechanics

| # | Teaches | Role | Brief (translatable · zones · ships) | Shape | Field (planets + sun · wells · depth) | Entropy scale (derived) | Why |
|---|---|---|---|---|---|---|---|
| 1 | the scale | Curve | fixed · 1 zone · 1 ship | Ring | 6 + sun · single · tutorial | low | first contact: a viable six-planet field, the sun in the background, one pinch |
| 2 | the centre | Curve | fixed · 1 zone · 1 ship | Ring | 4–6 + sun · single · medium | medium | the fixed cube over the sun — the star is a source too |
| 3 | the other direction | Clear | fixed · 1 zone · 1 ship | Ring | 5–6 + sun · single · medium | medium | a planet blocks the line; shrinking it clears the path — the same pinch, the opposite role |
| 4 | the cube moves | Curve | **yes** · 1 zone · 1 ship | Spiral | 8–10 + sun · multi | medium | spawns at the centre; the solution is off-centre, so the move is required |
| 5 | multi-zone intro | Curve | yes · 2+ zones · 1 ship | Spiral | 10–12 + sun · multi | medium | move + scale two zones — the combination enters |
| 6 | the field carries you | Curve | yes · 2+ zones · 1 ship | Cluster | 10 + sun · multi · deep | high | a deep well already near the line does most of the bend; the cheap answer is to edit where a small change moves the path most |
| 7 | the capstone | Curve | yes · 2+ zones · 1 ship | Cluster | 12 + sun · multi · deep | high | multi-zone + the centre + the star + several routes — both roles deferred to a later chapter |
| 2-1 | two ships | Any | yes · 1 zone · 2 ships, separate | Ring | 12 + sun · multi | low (budget 0.30) | one field, two runs; each ship has its own fix |
| 2-2 | fan-out | Any | yes · 1 zone · 2 ships, one origin | Spiral | 12 + sun · multi | medium (budget 1.20) | an edit near the shared origin bends both paths |
| 2-3 | converge | Any | yes · 1 zone · 2 ships, one destination | Spiral | 12 + sun · multi | medium (budget 1.50) | the planet by the shared destination fixes both approaches |
| 2-4 | three ships | Any | yes · 1 zone · 3 ships, separate | Cluster | 14 + sun · multi | medium (budget 1.50) | three fixes on one budget; the field carries more than one ship |

Fleet jobs request **Any**: multi-ship bakes cannot request a role, so each is checked by playing it.

**Chapter 1 is seven jobs (2026-11-05); the run is eleven (2026-11-11): 1 scale, 2 centre, 3 clear,
4 move, 5 multi-zone, 6 the field carries you, 7 capstone `Curve`, then chapter 2's four fleet jobs.**
The fixed-cube several-zones rung is retired; "several zones" lands with the move at
job 5. There is no job 8 (decided 2026-11-11): **Mixed** moves to a later chapter, since chapter 2
ships after job 7 ([chapters](chapters.md)).

**The ramp by job.** Scramble is how far the start sits from the solved arrangement: it rises within an
act and drops when a new verb lands, so the new idea arrives readable and is then tested. `Reuses` names
the prior skill the job combines, so the ramp is cumulative, not additive.

| # | Scramble | Reuses |
|---|---|---|
| 1 | low | — (tutorial) |
| 2 | low | the scale |
| 3 | medium | the scale, reversed |
| 4 | low | the scale |
| 5 | medium | the move + several zones |
| 6 | high | multi-zone + the read (use the field's wells) |
| 7 | high | everything: centre, move, zones, star; Mixed deferred |
| 2-1 | low | the move, now for two runs |
| 2-2 | medium | the move + a shared origin |
| 2-3 | medium | the move + a shared destination |
| 2-4 | medium | the move + the read (the field carries more than one ship) |

**The role.** Each job states the role its reference solution must teach: **Curve** (a well grew),
**Clear** (an obstacle shrank), **Mixed** (both, at least two zones), or **Any** (no request). It is a
**bake request**, not a player condition. The creator searches for a matching solve and rejects the seed
when it cannot find one; it also rejects a seed when a **cheaper or equal wrong-role route** exists,
because the taught role must be the best answer. More expensive alternate routes may remain. Chapter 1's
seven-job spine requests **Curve** except job 3 (**Clear**): the capstone is a `Curve` with the centre and the star.
**Mixed** is deferred to a later chapter ([archetypes](#the-run--archetypes)). A `Curve`/`Mixed` seed is rejected when its start path stays under
5% of the chord or 4° off it: the start must show the field. The bake reports the reference solution's
`kind`; the player never sees the label. Multi-ship role requests are not supported yet.

**The field is a galaxy, not a handful of balls.** The planet column is the **source** count — the bodies
that bend the path. The field also carries **decor** (dust, moons, distant planets) from
[the element pack](../ideas/element-pack.md) once that lands, so the playfield reads as a populated system
without every body becoming a well. The tutorial stays sparse at six planets plus the sun; the count
rises by act, so the galaxy grows with the run.

**The shape.** Each job states its `fieldShape`; `Auto` is for experiments only. Shape is a read axis,
not a difficulty dial — it never changes a rule — so it marks an **act**: **Ring** carries the
fixed-cube opening (1–3), **Spiral** the move act (4–5), **Cluster** the mastery act (6–7). A job may
turn the silhouette when the act turns; the new verb is still the lesson.

**The ramp is three dials, not the budget.** The budget and star bands keep the same shape every job
([core loop](core-loop.md) §3); difficulty comes from **scramble** (how far the start
sits from the solved arrangement), **planet count**, and **depth**. Turn all three up by act: the
tutorial is nearly solved, the act ends are scrambled. Do not tighten the budget to add difficulty; that
only moves the star bar.

**The sun.** The field can place a **sun** at its centre; it is scalable by its centre by default, is
never a run endpoint, and is not counted in the planet column ([the cube](systems/cube.md)). A seed can
omit it; the level can lock it; the settings size it.

**The sun in the first jobs.** Job 1 has a sun in the field, but the cube stays off it, so the first
lesson is still a planet. Job 2 makes the star the lesson: a fixed cube over the centre, so the star is
in the zone by construction. No lock; after job 2 the sun is a normal source in the later levels. (A
sunless four-planet field almost never bakes — the run picker needs a planet blocker on the line — so
job 1 carries a sun that the cube does not cover.)

**The pressures fade in one at a time.** This doc owns the per-job flags:

| Job | Fail | Stars | Intro |
|---|---|---|---|
| 1 | no (`noFail`, unlimited spend) | no (`noGrade`) | no (`noIntro`) |
| 2 | yes | no (`noGrade`) | yes |
| 3–7 | yes | yes | yes |
| 2-1 – 2-4 | yes | yes | yes |

Job 1 bakes from a normal seed — six planets and a sun the cube does not cover, one fixed zone — and still records a reference
solution, so the budget rule holds ([core loop](core-loop.md) §3). The lesson is the cube's scale, not a
single-source field. The entropy-scale column above is the absolute cost of the bake's solution.

**The several-zones rung is retired with the old job 4 (2026-11-05).** A fixed level can place several cubes,
but the generator could not force a two-zone `Curve`, so chapter 1's seven-job run folds "several zones" into the
multi-zone move at job 5. Job 4 adds the move; job 5 introduces two zones; job 6 teaches the field to
carry the ship; the capstone adds the centre and the star at job 7. A later chapter can bring back the fixed several-zones rung
with Mixed.

**Job 3 keeps the sun out of the zones.** The blocker is a planet; the star is background, so the lesson
stays "where do I scale," not "what is the star."

**The move is guaranteed.** A translatable level starts the cube at the field centre; job 4's solution
includes an off-centre zone, so the player must move the cube.

**The capstone is the centre + multi-zone + the star.** Job 7 starts the cube over the sun; the solution
scales several zones with the star included, so the player must move out to the other zones. It combines
every verb except both roles; both roles are deferred to a later chapter.

**The slice.** Jobs 1–7 are chapter 1's seven-job run; chapter 2 adds four fleet jobs; ~1 minute each.

**Multi-ship.** Chapter 1's seven-job spine is single-ship; the rule is in
[progression](systems/progression.md) §The puzzle map; multi-ship is chapter 2
([chapters](chapters.md)).

## The run — the brief

Each job's puzzle carries a **brief**; the per-level conditions are the source of truth here, and the
creator bakes to them. Three groups.

**Player conditions** — what the level allows:

| Field | Means |
|---|---|
| **`translatable`** | The cube may be **moved** (its centre changes) or is **fixed** at home. How many times the player moves it is their choice; it is not a baked count. |
| **`zones`** | How many **zones** the solution scales (1–3). **`zones ≥ 2` holds with `translatable` either way** — a **fixed** level places that many cubes, a **translatable** one has the player move one cube between zones. |
| **`ships` / `relation`** | How many runs the job has (1 = single-ship) and how they relate: **fan-out** (one origin), **converge** (one destination), or **separate**. |

**Bake request** — what the creator must produce:

| Field | Means |
|---|---|
| **`mechanism`** | The role the bake's reference solution must have: **`Any`** (no request), **`Curve`** (a well grew), **`Clear`** (an obstacle shrank), or **`Mixed`** (both; needs ≥2 zones). The search filters to the requested role; a seed that cannot bake it is rejected, and so is a seed whose cheapest route has the wrong role. More expensive alternate routes may remain. |
| **`minRoutes`** | Minimum distinct arriving routes the bake must offer. `0` = auto: 2 for a movable single-zone level, otherwise 1. Jobs 3 and 7 set 2 (and a later Mixed job). |

**Baked results** — what the creator records, not a player condition:

| Field | Means |
|---|---|
| **`routes`** | Distinct zone sets that arrive within budget. The bake enforces `minRoutes`: a movable single-zone level, job 3, and the capstone (job 7, and a later Mixed job) need at least two; the tutorial and job 2 may have one. |
| **`entropyBudget`** | **3×** the entropy of the bake's solution; the star thresholds are 1/2 and 2/3 of it. |
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
| **Obstacle** | `zones 1`, fixed cube on a body on the line | shrink the blocker so the path clears | 3 |
| **Move** | `translatable`, `zones 1` | place the cube yourself | 4 |
| **Multi-zone** | `zones ≥ 2`, `translatable` | place and blend several zones | 5 |
| **Assist** | `zones ≥ 2`, `translatable`, a deep well near the line | edit where a small change moves the path most | 6 |
| **Capstone** | `zones ≥ 2`, `translatable`, cube over the sun | combine the centre + several zones + the star | 7 |
| **Mixed** | `zones ≥ 2`, one on the blocker, one on a run planet | clear with one zone, curve with another | 8 (future) |
| **Fleet** | `ships ≥ 2`, `translatable`, `zones 1` | one cube, one budget, several runs | 2-1, 2-4 |
| **Fan-out** | `ships 2`, one origin | edit near the shared origin to bend both | 2-2 |
| **Converge** | `ships 2`, one destination | fix the shared approach once | 2-3 |

**The obstacle is the second role.** A body on the straight line is an obstacle; reducing its scale
reduces its collision radius, so the path can pass. It is **job 3**: it lands after the centre and before
placement, so the player learns both directions of the same pinch while the field is still fixed. The
tutorial and the centre keep the curve. With several cubes, one can sit on the obstacle (shrink) and one
on a run planet (grow) — a clear and a curve in one multi-zone solution, deferred to a later chapter. The
seven-job capstone is a `Curve`. The creator tags the role so a job can request it. Job 3 requests `minRoutes: 2`: two blockers on or near the line, so
the player chooses which one to shrink.

**Job 6 — the field carries you.** Each committed zone edit costs its net change, so spreading an edit
is not cheaper on its own; what is cheaper is **using the field**. Job 6 is authored so a deep well
already sits near the line: the cheap solution nudges the path into it with a few small edits (a
gravity assist), while the expensive one fights the field with one big edit. The lesson is read, not
bookkeeping, so it needs the **3★ band** to read distinctly on the in-field bar while shaping
([core loop](core-loop.md) §3).

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

| # | Client | Cargo | Fail | Tech voice | Manual entry |
|---|---|---|---|---|---|
| 1 | Blödmeyer & Sons (no sons) | a pallet of valves | — (no fail) | — (no intro: the Scale prompt teaches it) | "Welcome. Your Wrap does one thing. Do that one thing. Set the console anywhere; there is no office." |
| 2 | The Lighthouse Keepers' Union | one bulb | "The bulb is still out there." | "The star is a source too." | "The star is not a planet. It bends the same way." |
| 3 | Strait & Narrow Ltd. | a wide load | "Too much rock in the way." | "Shrink the rock, not the cargo." | "The Wrap takes as well as gives. A smaller world is a wider door." |
| 4 | Madame Fink's Discount Gravity | a piano | "The piano is lost." | "Now move it. *Where* is a choice. Works everywhere." | "The Wrap is yours to place. Where you work is a decision. Make it." |
| 5 | The Third Best Bakery | a wedding cake, still warm | "The cake went cold." | "Several zones. Place it, scale them." | "The Wrap is yours to place. Use it more than once. Wrap only what you need; the universe is not insured." |
| 6 | Nobody & Co. | a body (a funeral) | "The field won." | "Use what is already there." | "The field is free. The Wrap is not." |
| 7 | The Inheritance of Mr. Fink | the last of something | "Mr. Fink is still waiting." | "…Good luck." | "This is the one the manual was written for. Welcome, new employee. There has not been a new employee in eleven years. There is still no office." |

### Chapter 2 — the fleet

Episode 2: the Wrap works, so the firm sells more ships on the same entropy. The clients stay absurd;
the greed is in Jim's lines and the manual ([lore](world/lore.md) §Episodes).

| # | Client | Cargo | Fail | Tech voice | Manual entry |
|---|---|---|---|---|---|
| opener | Jim, before 2-1 | — | — | "The Wrap works. So I sold more of it. Same entropy. More ships. The brochure says green. It is just cheaper. Fuel is still capped. Nobody else can do this." | — |
| 2-1 | Halfmoon & Halfmoon | two identical parcels | "One of them got lost." | "I sold two runs for the price of one." | "Two ships. One Wrap. One budget." |
| 2-2 | Slightly Used Stars Ltd. | two refurbished suns | "Slightly Used Stars is unhappy again." | "They are back. They want both orders today." | "One origin. Two promises. Bend near the start and both feel it." |
| 2-3 | The Last Two Customers | two halves of a gift | "The gift is still in pieces." | "Two ships. One door." | "Many routes may share one end. Fix the approach once." |
| 2-4 | The Very Small Opera | the orchestra, one live animal | "The opera starts without them." | "Three ships. The budget did not grow." | "Efficiency is a virtue until it is a cost." |

## The manual's arc

The job card is the company's **onboarding manual for new employees** ([lore](world/lore.md) §The tech
team): short, dry, corporate, slightly out of date. The manual entry is read on the card: the selection
map shows the contract before entry, and a smaller copy stays during the run. The TTS card plays only
Jim's tech-voice line. Each entry introduces **one idea**, so it carries the teaching *and* the
fiction. It opens warm and ends corporate-bleak; the "new employee" it keeps addressing is the player,
who arrived in a kitchen instead of an office.
Two beats to keep:

- The opening line: *"Welcome, new employee."*
- A late note: *you are the first new employee in eleven years.*

**Two arcs:** the **tech voice** goes pride → doubt; the **manual** goes cheerful → bleak. The **client
names stay absurd** — the tonal relief.

**The console thread.** Remote work lives in the manual and Jim's receipts, never the client or cargo —
the contract is the customer's, the office is Jim's, and the two never meet. It escalates once per act
and never explains itself:

- **Job 1 (establish):** the manual sets the console down — *"Set the console anywhere; there is no
  office."*
- **Job 4 (choose):** the manual already says *where you work is a decision*; the tech voice adds the
  flat pitch, *"Works everywhere."*
- **Job 7 (absent):** the manual's late note ends *"There is still no office."*, and Jim's receipt
  closes *"The console stays where it is. Today the space. Tomorrow the time."*

The exact strings live in `Levels_en` / `Levels_es` under the `init_manual_entry`, `init_01`, `fail` and
`complete` keys. A spoken line longer than one short sentence is split into numbered entries that
play in order: `init_01`, `init_02`, … (up to `_09`), and `complete`, `complete_02`, … (the
same for `fail`, which is one entry per job). A key may end in Jim's expression
(`level_01_02_init_01_surprised`; `win`, `laughing`, `angry`, `confused`, `annoyed`, `surprised`, none =
idle). A chapter's opener is `chapter_NN_init_01…`, the same pattern; it plays once, before the
chapter's first job ([lore](world/lore.md) §Episodes). Jobs 2, 3, 5 and 6 stay untouched: the thread is the
company's condition, not a running joke.

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
- **Job 6's bake.** What number would show the well-using route is the cheapest — the cheapest spend
  against the runner-up route, or the winning zone's delta? Answer it with the store's freelance quality
  filter, not now.
