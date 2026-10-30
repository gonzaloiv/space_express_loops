# Design

The game's design, one concern per doc. **Build status lives on the [plans board](../plans/README.md)** —
these docs describe the intended game, not what is shipped.

## Docs

| Doc | Concern |
|---|---|
| [Vision](vision.md) | The pitch, the pillars, the scope, the validation, what not to do. |
| [Core loop](core-loop.md) | What the player does: moment-to-moment, a job, the company, entropy. |
| [The cube](systems/cube.md) | The one instrument, end to end. |
| [Progression](systems/progression.md) | How the game advances: the cube map, stars, the learning path. |
| [Jobs](jobs.md) | The concrete run: one row per job — brief, field, entropy, and lore skin. |
| [Presentation](presentation.md) | Look, sound, HUD. |
| [Lore](world/lore.md) | The fiction frame. |
| [Glossary](glossary.md) | The fixed terms. |

## Shape

- **Vision** is the stable overview; the **systems/** docs are the feature units; **presentation**
  and **world/lore** are the feel and the fiction.
- The docs describe the intended game. When a plan settles a rule, the design doc follows it in the
  same change; the [plans board](../plans/README.md) owns build status.
- Each `systems/*.md` shares one template — **Goal · Inputs · Rules · Feedback · Failure · Tuning ·
  Open questions** — so they read the same way.
- Every doc's status line is `**Status:** design · <concern>.` Substantive passes add a
  `**Revised:** <yyyy-mm-dd>.` line.
