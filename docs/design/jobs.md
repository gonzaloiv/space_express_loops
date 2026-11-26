# Jobs — the run, one delivery at a time

**Status:** design · content · jobs.
**Date:** 2026-10-27.
**Revised:** 2026-11-26.

**Related:** [progression](systems/progression.md) · [lore](world/lore.md) ·
[the cube](systems/cube.md) · [glossary](glossary.md)

## Goal

The **concrete run**: the jobs the player actually plays, one row each — their brief, field and entropy
scale (the shape [progression](systems/progression.md) sets), plus their **lore skin**. Progression is
the general system; this is the specific development of it. Chapter 1's seven jobs are baked; chapter 2's
ten are being (re)baked with the sun at the edge ([plans board](../plans/README.md)). This table is
the source for every job's field and budget; [chapters](chapters.md) keeps the lessons.
This doc is **chapters 1–2**; chapter 3 is in [chapters](chapters.md).

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
| intro 1 (02_09) | one pinch, two ships | Any | fixed · 1 zone · 2 ships, one origin | Ring | 9 + sun · multi | low | the fixed cube near the shared origin bends both paths at once |
| intro 2 (02_10) | one zone between | Any | fixed · 1 zone · 2 ships, separate | Ring | 9 + sun · multi | low | one zone placed between two runs rescues both |
| 2-1 | two ships | Any | yes · 1 zone · 2 ships, separate | Ring | 10 + sun · multi | low (budget 0.30) | one field, two runs; each ship has its own fix |
| 2-2 | fan-out | Any | yes · 1 zone · 2 ships, one origin | Spiral | 12 + sun · multi | medium (budget 1.20) | an edit near the shared origin bends both paths |
| 2-3 | converge | Any | yes · 1 zone · 2 ships, one destination | Spiral | 12 + sun · multi | medium (budget 1.50) | the planet by the shared destination fixes both approaches |
| 2-4 | three ships | Any | yes · 1 zone · 3 ships, separate | Cluster | 14 + sun · multi | medium (budget 1.50) | one Wrap carries three ships; the field carries more than one ship |
| 2-5 | fleet capstone | Any | yes · 1 zone · 3 ships, one origin | Cluster | 12 + sun · multi · deep | high (budget 1.20) | placement, shared paths and the field across every ship |
| 2-6 | Clear with a fleet | Clear | yes · 1 zone · 2 ships, separate | Ring | 10 + sun · multi | medium | one blocker shared by both runs; shrinking it once frees the fleet |
| 2-7 | Clear among decoys | Clear | yes · 1 zone · 2 ships, separate | Cluster | 12 + sun · multi | medium | the shared blocker hides among planets that only look involved |
| 2-8 | two shared blockers | Clear | yes · 1 zone · 2 ships, separate | Spiral | 12 + sun · multi | medium | two shared blockers, two edits on one budget |

Fleet jobs request **Any**, except the Clear fleets (2-6 to 2-8), which ask for **Clear**; the rest are checked by playing them.

**Chapter 1 is seven jobs (2026-11-05); the run is twenty-three (2026-11-25): 1 scale, 2 centre,
3 clear, 4 move, 5 multi-zone, 6 the field carries you, 7 capstone `Curve`, then chapter 2's ten
fleet jobs.**
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
[field elements](../ideas/element-pack.md) (dust is in; moons are still an idea), so the playfield reads as a populated system
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
| intro 1–2 (02_09, 02_10), 2-1 – 2-4 | yes | yes | yes |

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

**The slice.** Jobs 1–7 are chapter 1's seven-job run; chapter 2 adds ten fleet jobs; ~1–3 minutes
each.

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

Each job is a **contract**: a client, a cargo, and the manual entry that teaches its lesson. The lines
reach the player through three channels, each with one job ([lore](world/lore.md)):

- **The manual** (tech team, on the card, never spoken): one rule and one fine-print line, then a
  corporate disclaimer. It carries the danger and the company's indifference ("No liability for causality
  tears"). Its label is a release: `MANUAL v1.0.3 (INITIAL DEPLOYMENT)`, chapter.0.patch (the job's
  place in the chapter) plus the chapter's release name (`chapter_NN_manual_release`).
- **The intro** (Jim, spoken, two or three short lines): the client and his mood, never the mechanic.
- **The receipt** (Jim, spoken, after a solve): the delivery, then a line about the world or the money.
  The main lore channel: the player is relaxed and listening.
- **The fail** (Jim, spoken): one short joke; the player wants the retry.

The exact strings live in `Levels_en` (synced from the line spreadsheet); this table keeps the
contract, not the dialogue.

| # | Client | Cargo | Manual entry |
|---|---|---|---|
| 1 | Blödmeyer & Sons (no sons) | a pallet of refurbished 2019 copper valves | "Welcome, new employee. One push at launch, then the field steers. Wrap from any room." |
| 2 | The Lighthouse Keepers' Union | one bulb | "The star bends space like any planet. Stellar mass is included in your licence." |
| 3 | Strait & Narrow Ltd. | a decommissioned school bus (unoccupied) | "Shrink a world to widen the door. Residents are not notified." |
| 4 | Madame Fink's Discount Gravity | a piano | "Where you work is a decision. Our Space Corp does not reimburse home office expenses." |
| 5 | The Third Best Bakery | a wedding cake, still warm | "Multi-zone enabled. Wrap only what you need. No liability for causality tears." |
| 6 | Nobody & Co. | a pre-paid 2038 funeral urn | "The field is free. The Wrap is not. Use existing gravity before someone privatizes it." |
| 7 | The Inheritance of Mr. Fink | the sector's final paper birth registry | "The manual was written for this one. Welcome, new employee. There is still no office." |

### Chapter 2 — the fleet

Episode 2: the Wrap works, so the firm sells more ships on the same entropy, sold as green. The clients
stay absurd; the greed is in Jim's opener and receipts, and the manual's fine print
([lore](world/lore.md) §Episodes). Jim's opener ("Good news. Clients love our green initiative." /
"Bad news. …") plays once before the chapter's first job (02_09).

| # | Client | Cargo | Manual entry |
|---|---|---|---|
| intro 1 (02_09) | Twin Moons Catering | two dinners, one kitchen | "Two ships, one origin; bend near the start. Out here the star sits at the edge." |
| intro 2 (02_10) | Two Rival Bakeries | two cakes, one feud | "One cube between two runs. Both ships share the bend; the feud is not our problem." |
| 2-1 | Halfmoon & Halfmoon | two identical parcels | "Two ships, one budget. Extra ships do not raise your allowance. This is a feature." |
| 2-2 | Slightly Used Stars Ltd. | two secondhand fusion cores (slightly dimmed) | "One origin, two promises: bend near the start. Shared failures are billed twice." |
| 2-3 | The Last Two Customers | two halves of a gift | "Fix the shared approach once. Our Space Corp values every customer. Both of them." |
| 2-4 | The Very Small Opera | the orchestra, one live animal | "Every ship draws on the same Wrap. Live cargo travels at its own existential risk." |
| 2-5 | The Committee for Three Suns | three commemorative suns | "One Wrap carries three ships. Extra ships are a promise, not a budget line." |
| 2-6 | The Bureau of Shared Walls | two doors, one frame | "Both runs cross the same blocker. Shrink it once; the budget stays one number." |
| 2-7 | The Lost and Found Office | a crate of unclaimed umbrellas | "A blocker can hide among planets that only look involved. Read the runs." |
| 2-8 | The Two Shadows Company | two shadows, one lamp | "Two blockers, two edits, one budget. Growing a clear planet is not help." |

### Chapter 3 — broken bodies

Episode 3: the damage shows in the bodies, and the sector audit is the frame ([lore](world/lore.md) §Episodes).
The manual's register is denial and liability (*DAMAGE CONTROL*): it states the rule for the broken body and
bills the consequence to the employee. The final entry carries the eleven-years note (see §The manual's arc).

| # | Client | Cargo | Manual entry |
|---|---|---|---|
| 3-1 | The Museum of Small Disasters | a cracked marble | "An anchored body ignores the Wrap. Route around it; the audit is not your concern." |
| 3-2 | Two Brothers, One Address | a shared inheritance | "Only one body here is fused. The others are decoys; the budget is not." |
| 3-3 | The Quiet Hours Union | a night shift | "A repulsor pushes, not pulls. Shrink it to open the gap. All pushes are billed." |
| 3-4 | The Gravity Choir | a hymn in three parts | "Grow a repulsor to shove a ship off its line. The push is not free." |
| 3-5 | The Last Two Customers | the other half of the gift | "Two damaged bodies, two edits. One will not move; the other must be paid for." |
| 3-6 | The Sector Audit Office | the firm's entropy ledger | "Welcome, new employee. No new employee in eleven years. There is still no office." |

## The manual's arc

**The card's behaviour** (what types and how fast, the patch number by play order, the release name on
a chapter's first card) is specified in [story arc](../solutions/narrative/story-arc.md) §The job card;
this section owns what the entries say.

The job card is the company's **onboarding manual for new employees** ([lore](world/lore.md) §The tech
team): dry, corporate, shipped in releases and patched after every delivery. The manual entry is read
on the card: the selection map shows the contract before entry, and a smaller copy stays during the
run. It is never spoken. Each entry teaches **one rule** and adds **one line of fine print** — about 90
characters at most, and never a repeat of Jim's line on the same job — so it carries the teaching *and*
the fiction. It opens warm and ends corporate-bleak; the "new employee" it keeps addressing is the
player, who arrived in a kitchen instead of an office.

**One register per chapter.** Chapter 1 opens warm and turns corporate-bleak by job 7; chapter 2 is
complicit (the fleet, sold as green); chapter 3 is denial and liability (*DAMAGE CONTROL*). Do not
replay the whole cheerful → complicit → denial arc inside chapter 1.

Two beats to keep:

- The opening line: *"Welcome, new employee."*
- A late note: *No new employee in eleven years.* It is chapter 3's final entry (*"Welcome, new
  employee. No new employee in eleven years. There is still no office."*); no other line uses that
  number.

**Two arcs:** Jim goes pride → doubt; the **manual** goes cheerful → complicit → denial. The **client
names stay absurd** — the tonal relief.

**The console thread.** Remote work lives in the manual and Jim's lines, never the client or cargo. It
escalates once per act and never explains itself:

- **Job 1 (establish):** the manual: *"Wrap from any room."* (the same entry teaches the launch: one
  push, then the field steers; that is why the Wrap saves fuel)
- **Job 4 (choose):** the manual: *"Where you work is a decision… does not reimburse home office
  expenses."*; Jim adds the flat pitch, *"It works everywhere."*
- **Job 7 (absent):** the manual's entry ends *"There is still no office."* (the "eleven years" note
  moves to chapter 3), and Jim's receipt closes *"The console stays where it is."* The roadmap slogan
  moves to the ending.

**Keys.** `level_{job}_init_client` / `_init_cargo` / `_init_manual_entry` (on screen);
`level_{job}_init_NN`, `_complete_NN` and `_fail` (spoken, played in key order). A spoken key may end in
Jim's expression (`level_01_02_init_02_annoyed`; `win`, `laughing`, `angry`, `confused`, `annoyed`,
`surprised`; none = idle). A chapter's opener is `chapter_NN_init_NN…`, the same pattern; it plays
once, before the chapter's first job ([lore](world/lore.md) §Episodes). Chapter keys also carry the
map title (`chapter_NN_title`) and the manual release (`chapter_NN_manual_release`).

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
