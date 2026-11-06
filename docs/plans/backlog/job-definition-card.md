# Job definition card — the contract from job 2

**Status:** next · presentation · updated 2026-11-06 — client, cargo and the manual entry on an authored
card that fades on its own; job 2+ only; the puzzle stays live.

**Related:** [jobs](../../design/jobs.md) · [lore](../../design/world/lore.md) ·
[loop completion](../in-progress/loop-completion.md) · TTS panel (landed) ·
[Game.unity](../../../Assets/_Game/Game.unity)

## Goal

Each job is a contract: a client, a cargo, and the manual entry that teaches its lesson
([jobs](../../design/jobs.md) §The run — the lore skin). Loop completion cuts the intro to Jim's tech
line, so the card is now the **only** surface for the manual entry. Help reaches the player at the start (this card, Jim's line) and the end (the
complete/fail message); there is no in-solve hint.

## Contract

- **Job 2+ only.** The same `noIntro` flag that silences job 1's intro hides its card.
- **What it shows:** an **Our Space Corp.** header (authored on the card, not a localized line), then
  client, cargo and the manual entry. Jim's TTS line (`_01`) plays alongside; the card
  never speaks, and the TTS panel never reads the manual.
- **Timed fade, no input.** The card shows on `PuzzleStartState` and fades on its own once the intro
  line finishes, or after `cardSecs` when there is no line. The puzzle is live underneath; pinching
  does not wait for the card.
- **No replay** on reset or retry.
- **Keys come from the id.** `PuzzleData` derives `ClientKey`, `CargoKey` and `ManualKey`
  (`level_<id>_init_client`, `_init_cargo`, `_init_manual_entry`) beside `IntroKey` and `CompleteKey`.
  No contract asset; `jobs.md` stays the design source.
- **Placement:** authored, world-space, clear of the field, the cube and the TTS panel; no follow.
- **Terminal look.** The card is the Wrap's dispatch terminal printing the contract
  ([lore](../../design/world/lore.md) §The tech team — the console): monospace type in the UI accent, a
  thin frame, faint scanline, blinking cursor. It boots in, the lines **type in** in order (header,
  client, cargo, manual entry), and it cuts off like a CRT on its fade. Typing is part of the reading
  time: `cardSecs` starts after the last line lands.
- **Shared style.** This plan builds the frame, monospace font asset and line-art material as one
  authored terminal style; the [gesture prompts](../in-progress/gesture-prompts.md) reuse it, so this lands first.

## Units

### U1 — the card

- Build the shared terminal style (frame, monospace font asset, line-art material) as reusable authored
  assets for the gesture prompts.
- Author the card view in `Game.unity` in the terminal style (TMP header, client, cargo, manual entry);
  boot in, type the lines in with an authored `typeCharsPerSec`, then hold `cardSecs` and cut off.
- Add the derived keys on `PuzzleData`; a missing key leaves its line empty, it never blocks.
- **Exit:** job 2's card shows its client, cargo and manual entry, sits clear of the field and fades.

### U2 — the state hook

- `PuzzleStartState` shows the card with the intro (skipped on `noIntro`); the fade follows the intro
  line's completion, else `cardSecs`; `Exit` hides it.
- **Exit:** job 1 shows no card; job 2 shows it and fades after Jim's line; reset does not replay it;
  leaving the job hides it.

## Manual check (headset)

1. Open job 1: no card, no voice.
2. Open job 2: the terminal boots and types the header, client, cargo and manual entry; Jim says one
   line; the type reads at bubble distance.
3. Pinch while the card is up: the puzzle responds; the card does not block.
4. The card fades after Jim's line; it never covers the field, the cube or the TTS panel.
5. Reset job 2: the card does not return.
6. Read it at bubble distance over passthrough: size and contrast hold.

## Out of scope

- Final copy, recordings and the Spanish pass (TTS S5).
- An accept button, a held/physical card, or a manual book.
