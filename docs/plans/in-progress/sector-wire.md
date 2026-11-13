# Sector wire

**Status:** now · presentation · updated 2026-11-13 — the world's news and the firm's reach on the
console: a wire line that moves with each first solve, and the sector coverage count on the map.

**Related:** [lore](../../design/world/lore.md) §In the game · [plans board](../README.md)

## Goal

The lore reaches the player as independent lines, never exposition: the fixed pie, the fuel cap and
the firm's greed arrive as news, and the lit map reads as the firm's reach.

## Contract

- **One ordered wire.** `sector_wire_00`…`sector_wire_11` (Global table), one line per solved count:
  after N solved jobs the wire reads line N (past the end, the last line). The order is the story; no
  speaker, no TTS.
- **The boot** types the current line after the checklist, before the title.
- **The map** shows `SECTOR COVERAGE 07/11` (`map_coverage`) and the current wire line in the field.
  The first return after a new solve types both; any other return shows them at once. The last seen
  count lives in a `sector_coverage_seen` cookie.
- The count and the total come from the run (`RunProgress`), so a new chapter grows the total.

## Units

### U1 — domain, strings and tests

- `RunProgress.SolvedCount`, `SectorWire.Key(solved)`, the seen-coverage cookie in
  `RunProgressCookies`; EditMode tests for each.
- The 13 Global keys with English values.
- Exit: the fast suite passes.

### U2 — the views and the wiring

- `SectorReadout` (map): coverage + wire `TMP_Text`, typed or instant. `PuzzleSelectionState` shows it
  with the map and hides it on exit. `ConsoleBootSplashScreen` types the wire line.
- Author the two map texts in the field (near the map's edge, in the meter label's style) and one boot
  console line in `Game.unity`; wire them.
- Exit: in the editor, a solve returns to the map with the count and the line typed; the boot types the
  line.

### U3 — chapters as systems on the map *(a test)*

- `PuzzleMapLayout.Systems(sizes)`: one spiral per chapter at the galaxy's density, side by side with a
  gap, centred; one chapter stays the plain galaxy. `RunMap.Build` groups jobs by chapter id into
  `PuzzleMap.Chapters`; the route's hop into a new chapter is fainter (`MapRouteLine.HopAlpha`).
- `MapChapterTitles`: one title per chapter (`chapter_NN_title`, Levels table) pooled from an authored
  label under the map root, placed beside its system. `PuzzleSelectionState` shows and hides it.
- Author the title prototype under the map root (console style, `LookAtCameraOnY`) and wire it.
- Exit: the two chapters read as two places with their titles; if not, revert to one galaxy.

### U4 — the episode bulletin

- Once a chapter's first job is solved, the first return to the map types the episode's bulletin on
  the console (`chapter_NN_bulletin_01…`, Levels table) before the map appears; once per chapter
  (`chapter_bulletin_seen_NN` cookie). Chapter 1 frames the world after job 1 (never in the first 30
  seconds); chapter 2 opens with Jim and gets its bulletin after 2-1.
- `RunProgressCookies.DueBulletinChapter` / `MarkBulletinSeen`, `LocalizationUtil.GetValuesInKeyOrder`,
  `PuzzleSelectionState` sequences it through `ConsoleScreen.PlayMessage`.
- Exit: the fast suite passes; after job 1 the bulletin types, then the map with coverage and wire.

## Manual check (headset)

1. Fresh save: the boot types `> WIRE: FUEL CAP HOLDS FOR THE 12TH YEAR` before the title.
2. Solve job 1: back on the map, `SECTOR COVERAGE 01/11` and the next wire line type in.
3. Leave and re-enter the map without a new solve: both show at once, nothing retypes.
4. Fail a job: the count does not move and the line does not change.
5. After job 7, the wire turns to the green multi-ship runs as chapter 2 opens.
6. The texts read at arm's length and sit in the field, not over the selected diorama or the card.
7. Relaunch: the boot shows the line for the current count.
8. The ending: `11/11` and the time-wrap line.
9. The map shows two clusters: chapter 1's seven jobs and chapter 2's four, with a clear gap.
10. Each cluster carries its title (`EPISODE 1 · THE WRAP`, `EPISODE 2 · THE FLEET`), facing you.
11. The route's hop from 01_07 to 02_01 is fainter than the route inside a chapter.
12. Sliding the cube across the gap snaps to the other cluster's nodes; the fade follows the cube.
13. The ending's lit map still fits the view with both clusters.
14. Decision: keep the systems, tune the gap and title placement, or go back to one galaxy.
15. After job 1's receipt, the console types `> 2048. NOBODY IS BORN. NOTHING GROWS.` and two more
    lines before the map; it never plays again, not even after a replay of job 1.
16. Chapter 2: Jim's opener before 2-1, then the fleet bulletin on the return after 2-1.
