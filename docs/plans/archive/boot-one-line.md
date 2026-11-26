# Boot on one line

**Status:** archived · presentation · updated 2026-11-26 — the console boot types every line through one
`TMP_Text` with localized strings, instead of one GameObject per line.

**Related:** [plans board](../README.md)

## Goal

The boot card shows one line at a time in the same place, but each line is its own `TMP_Text`
GameObject with hard-coded English (header, Link/Hands/Seat/Ledger rows, four hand-tracking hints,
Cargo/Manual Rule/Manual): 13 objects for one line. Deleting lines from the Boot Card instance left a
`TerminalCursor` pointing at nothing and threw every frame (fixed 2026-11-20 by removing the
component). One line fed from string keys removes that class of wiring bug and makes the boot
translatable.

## Contract

- The boot looks and sounds the same: header, checklist rows typed up to their last word then "OK",
  looping hints while a step waits, title lines (fresh-only lines stay fresh-only), CRT cut-off.
- Every boot string lives in a Localization string table; none is typed into a scene object.
- The Boot Card keeps one text line (and its cursor, if wanted); the per-line GameObjects are deleted.

## Units

### U1 — strings

- Move each boot line into a string table entry (English as today).
- Exit: every current line has a key.

### U2 — one line

- `BootChecklist` rows, `ConsoleHint` lines and `ConsoleBootSplashScreen` title lines hold localized
  strings instead of `TMP_Text`; the boot sets the shared line's text, then `ConsoleScreen.Type`s it.
  Keep the row's "pending up to the last word" rule on the string.
- If a line needs its own style (the header), carry it with the entry, not a second object.
- Delete the per-line GameObjects from the Boot Card; wire the one line.
- Exit: compiles, fast suite green, no missing references in `Game.unity`.

## Manual check (headset)

1. Fresh install: header, rows typed with "OK", title lines including fresh-only ones, cut-off.
2. Boot with hand tracking off: the hints loop until hands are tracked, then the boot resumes.
3. Second launch: fresh-only lines are skipped.
4. No exceptions in the headset log through the boot.
