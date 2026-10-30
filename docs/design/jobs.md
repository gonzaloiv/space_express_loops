# Jobs — the run, one delivery at a time

**Status:** design · content · jobs.
**Date:** 2026-10-27.
**Revised:** 2026-10-30.

**Related:** [progression](systems/progression.md) · [lore](world/lore.md) ·
[the cube](systems/cube.md) · [glossary](glossary.md)

## Goal

The **concrete run**: the jobs the player actually plays, one row each — their brief, field and dials
(the shape [progression](systems/progression.md) sets), plus their **lore skin**. Progression is the
general system; this is the specific development of it. Counts are a **stretch goal** — prove 3–5 first.

## The run — the mechanics

| # | Teaches | Brief (translatable · moves · resizable) | Field (planets · layout · well) | Dials (entropy) | Why |
|---|---|---|---|---|---|
| 1–2 | the scale | fixed · 1 · no | 1 · single · medium | loose→tight | first contact: one lever, one planet |
| 3 | several zones | fixed · 2+ · no | 2–3 · multi-well | medium | "where do I scale" enters — the level places the zones |
| 4 | the lens moves | **yes** · 1 · no | 2–3 · multi-well | medium | "where does the lens sit" enters |
| 5 | resize the frame | yes · 1+ · **yes** | 3–4 · multi-well | tight | the aperture is the player's |
| 6–7 | combine | yes · 2+ · yes | 3–4 · mixed · deep | tight | mastery: read the field, edit little — spread the edit across zones to spend less entropy |

**The several-zones rung.** A fixed cube covers one region, but a level can **place several fixed
cubes**, so `moves ≥ 2` no longer needs `translatable`. Job 3 teaches several zones on placed cubes;
job 4 adds the move; resize follows at job 5.

**The slice.** Jobs 1–7 are the full run, ~1 minute each.

## The run — the lore skin

Each job is a **contract**: a client, a cargo, and the manual entry that teaches its lesson.
One line each; never exposition ([lore](world/lore.md)).

| # | Client | Cargo | Tech voice | Manual entry |
|---|---|---|---|---|
| 1 | Blödmeyer & Sons (no sons) | a pallet of valves | "One pinch. You'll be fine." | "Welcome. Your Wrap does one thing. Do that one thing." |
| 2 | The Third Best Bakery | a wedding cake, still warm | "Same again — but the cake's on a clock." | "Most deliveries are urgent. None of them are our fault." |
| 3 | Two Guys, One Moon | spare pump parts | "Two zones now. Scale them both." | "The Wrap scales where it sits. Two of them, this time." |
| 4 | The Lighthouse Keepers' Union | one bulb | "Now move it. *Where* is a choice." | "The Wrap is yours to place. Where you work is a decision. Make it." |
| 5 | Madame Fink's Discount Gravity | a piano | "Bigger, smaller — your call." | "The aperture is yours. Size is a promise you make to yourself." |
| 6 | Nobody & Co. | a body (a funeral) | "Tight. Really tight. Sorry." | "Every edit costs. The customer only ever sees the arrival." |
| 7 | The Inheritance of Mr. Fink | the last of something | "…Good luck." | "This is the one the manual was written for." |

## The manual's arc

The tip card is the company's **onboarding manual for new employees** ([lore](world/lore.md) §The tech
team): short, dry, corporate, slightly out of date. It plays on the TTS card as the job's intro — the
manual entry and the tech-voice line, two entries keyed from the job's id (`LevelData.IntroKey`). Each
entry introduces **one idea**, so it carries the teaching *and* the fiction. It opens warm and ends
corporate-bleak; the "new employee" it keeps addressing never arrives. Two beats to keep:

- The opening line: *"Welcome, new employee."*
- A late note: *there has not been a new employee in eleven years.*

**Two arcs:** the **tech voice** goes pride → doubt; the **manual** goes cheerful → bleak. The **client
names stay absurd** — the tonal relief.

## Seeds (later)

- **Per-job launch lines.** Once the launch is per-job, each job's *given* launch becomes a beat — a
  launch that would fail, made to land.
- **The manual as a findable object.** The full manual in the office, with the entries the run skipped.
- **Client callbacks.** An absurd name returns in a later job, with a grudge. **Slightly Used Stars
  Ltd.** is the first: the move lesson outgrew its "You can move it now" contract.
- **A returning contract.** **The Very Small Opera** (a live animal — "Efficiency is a virtue until it
  is a cost") waits for a later combine job.
