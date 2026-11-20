# Multi-zone fleet bake

**Status:** next · space shaping · updated 2026-11-19 — a fleet bake where each ship needs its own
zone (`zones = ships`), for chapter 3.

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[chapters](../../design/chapters.md)

## Goal

`PuzzleMultiShipBake` produces fields where each ship is rescued by a different zone and no single
cube rescues them all. Chapter 3's first jobs are built on it.

## Depends on

[Fleet sun answer](fleet-sun-answer.md): the background search and the sun rule for fleets.

## Units

### U1 — the brief

- Add the zone-per-ship requirement to the multi-ship brief and contract (`Zones` equals the ship
  count, plus an "each ship has its own zone" check).
- Exit: the validator rejects a field where one zone serves two ships.

### U2 — the per-ship zone search

- Search one zone per ship, then confirm the combined answer lands every ship within budget and that
  fewer zones do not.
- Exit: a Creator test bakes a two-ship, two-zone field.

### U3 — three ships

- The same for three ships and three zones, if the search time allows (60–90 s per seed).
- Exit: one three-ship field bakes, or a note here on why it cannot yet.

## Manual check (headset)

1. A two-zone fleet field: each zone rescues only its own ship.
2. Placing both zones solves within budget.
3. The player can tell which ship each zone is for.
