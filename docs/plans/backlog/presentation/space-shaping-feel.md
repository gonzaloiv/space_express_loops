# Space Shaping — Feel & Presentation Polish (backlog)

**Status:** later · presentation · updated 2026-10-27 — collected, design only, **saved for later**. The
*feel* beats that make the core loop land, after the validation passes and the puzzle content exists.
None of these gate the mechanic; they are the polish that turns "works" into "wow". Not current work: the
focus is **interesting puzzles**.

**Related:** [vision](../../../design/vision.md) · [lore](../../../design/world/lore.md) ·
[jobs](../../../design/jobs.md) · [progression](../../../design/systems/progression.md) ·
[presentation](../../../design/presentation.md)

## 1. The two faces (the core read)

- **Space** is the mesh warp that already works — the lattice bends where the player scales.
- **Time** reads on the **same lattice**: its cells **pulse at the local clock rate** (spacing = space,
  pulse = time), and the zone is **tinted cool/warm** (slower / faster clock)
  ([cube](../../../design/systems/cube.md)).
- The **window gauge** is the payoff: the projected arrival slides as the player scales. That is how
  "this scale bought time" becomes visible.

## 2. The flight (the payoff beat)

- The ship **flies on every modification** — the flight *is* the reveal (the prediction line is gone).
  A run that lands reads as a win; a miss reads as a failure.
- **Flight-duration tuning** (`travelSecs`): the run fires on every edit, so its length needs a pass —
  long enough to read the arc, short enough not to slow shaping down.
- A **haptic "stretch"** on the cube scale, and a rising **pitch** as entropy drains — the cube is the
  product; it should *feel* like it is pulling spacetime.

## 3. The manual (the diegetic teaching)

- The tip panel is the company's **onboarding manual for new employees** ([jobs](../../../design/jobs.md)):
  one entry per verb, dry, corporate, slightly out of date. It carries the teaching *and* the fiction —
  never a tutorial box.
- The arc: it opens warm and ends corporate-bleak; the "new employee" it keeps addressing never arrives.

## 4. The tech voice (one character)

- **One voiced character** (the tech team) comments through the run — not a cast; a colleague. The arc:
  pride → doubt ([lore](../../../design/world/lore.md) §The tech team).

## 5. The window as a diegetic gauge (+ comedy)

- The `[Tmin, Tmax]` window shown as a **green band** on the ship's clock, not two numbers.
- Lean into the absurd precision as the *customer*: "arrive between 4:02 and 4:03 — not a minute early."
  The joke is the point ([jobs](../../../design/jobs.md)).

## 6. Cargo / customer personality

- Each job gets a **reason** — the absurd client, the cargo — one line of flavour per level, drawn from
  [jobs](../../../design/jobs.md)' lore skin and [lore](../../../design/world/lore.md)'s deadpan tone.

## 7. Locked gestures (the cube says no)

- When a job **locks** a gesture (`translatable` / `resizable`), the cube **refuses it visibly** — the
  frame flashes its locked colour and a short **thud** — never silently ignore the hand. The player has to
  see *why* nothing happened ([progression](../../../design/systems/progression.md) §4).
- The **manual** has a line for it.

## 8. Entropy + collapse (one meter)

- As entropy drains, the universe visibly **wobbles / deforms** — the *world* is the meter, not a HUD
  number ([resources](../../../design/systems/resources.md)). Start as pure feedback; optionally, later,
  let the deformation push the path (a *mechanical* collapse).
- Zero entropy is the **fail**: the bar depletes and a "you lose" beat plays; the full collapse is polish.

## 9. HUD / audio

- The HUD sits **over the level** (world-anchored) — it works today; it could also ride the TTS panel
  screen ([voice panel](../../../ideas/tts-voice-panel.md)).
- **Music exists**; the **new SFX** (travel, scale, grade, collapse, locked) are the audio work to add.

## Order

**All deferred (saved for later).** Revisit once the puzzle content exists. The **two faces** (1) is the
core read and the thing to get right first; the **flight** (2) is the demo beat; the **manual** (3) and
the **tech voice** (4) carry the fiction; the **window gauge** (5) makes the fail legible; **entropy +
collapse** (8) ties the bar to the world.
