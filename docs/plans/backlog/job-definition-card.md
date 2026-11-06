# Job definition card — the contract before the job

**Status:** later · presentation · updated 2026-11-06 — an authored job card (client, cargo, lesson)
shown at job start; the TTS panel speaks the job's voice.

**Related:** [jobs](../../design/jobs.md) · [lore](../../design/world/lore.md) ·
[TTS messages](../in-progress/tts-messages.md) · [Game.unity](../../../Assets/_Game/Game.unity)

## 1. Why

Each job is a contract: a client, a cargo, and the manual entry that teaches its lesson
([jobs](../../design/jobs.md) §The run — the lore skin). The definition should read as a card the player
takes, not a label on the map or a line in the TTS panel. The card carries the definition; the jumbotron
carries the voice.

## 2. The fit

- **An authored card.** World-space, held or placed at job start, with the client, cargo, and the manual
  entry. No runtime hierarchy; no camera follow.
- **One runtime contract asset.** `JobContractData` per job: client, cargo, manual key, tech key, complete
  key, keyed by `PuzzleData.id`. Keep it in the runtime assembly; the Creator contracts are Editor-only.
- **Card and voice are separate.** The card shows the definition; the TTS panel plays the manual and tech
  lines. The panel is at the side; the card is the thing the player accepts.
- **Job start only.** Show on `PuzzleStartState`; dismiss on accept or after the intro. No replay on reset
  or retry.
- **No block after accept.** The card must not cover the run, the cube, or the TTS panel once dismissed.

## 3. Slices

### S1 — the card

- Author the card view; show/hide; placement; client, cargo, and manual text.
- **Exit:** job 1's card reads client/cargo/manual and sits clear of the field.

### S2 — the data

- Add `JobContractData` and resolve it by `PuzzleData.id`; fill job 1.
- **Exit:** the card reads authored data; a missing contract fails loudly.

### S3 — accept and state hook

- Show the card on `PuzzleStartState`; accept dismisses it and starts the job; the TTS panel is launched by
  the same state or immediately after.
- **Exit:** job 1 shows the card, accept starts the job, the panel speaks, and the card hides.

## 4. What it touches

- `JobDefinitionCardView` (new), `JobContractData` (new), `PuzzleData.id`, `PuzzleStartState`.
- `Levels` keys for client/cargo/manual; `TtsPresenter` reads the manual/tech keys.

## 5. Tests

- The card shows job 1's client/cargo/manual; accept dismisses; no replay on reset.
- The card does not cover the run or the TTS panel; a missing contract fails loudly.
- The keys resolve in both locales.

## 6. Out of scope

- The map selection info, the TTS panel, the tip/attention panels.
- Final copy and audio.

## 7. Risks

- **Two job-start surfaces.** Card and TTS panel can compete; sequence them (card first, voice with or
  after it) and keep the panel at the side.
- **Duplicated copy.** `jobs.md` is the design source; the runtime asset is the authored data. Do not
  copy the table into code.
- **Creator assembly.** Keep `JobContractData` runtime-side; do not reference the Editor-only contracts.
- **Reading time.** Do not block the puzzle beyond the accept.
