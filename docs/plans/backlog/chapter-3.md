# Chapter 3

**Status:** next · space shaping · updated 2026-11-19 — ~6 jobs from combinations (multi-zone fleets,
then Mixed), presented as damage, with its own episode and the release's ending.

**Related:** [plans board](../README.md) · [release](../../design/release.md) ·
[lore](../../design/world/lore.md#episodes) · [chapters](../../design/chapters.md)

## Goal

A playable chapter 3 that closes the story: ~6 jobs, its episode opener, Jim's lines and the ending.
Only the lines and the manual present the fields as damage: no new physics, bake or visual work.

## Depends on

[Multi-zone fleet bake](multi-zone-fleet-bake.md) and the [story arc](story-arc.md) (what breaks, the
episode and its clients, the ending).

## Units

### U1 — the chapter

- `Chapters/03.asset` and its folder; the map gains a third system; the episode and client rows.
- Exit: an empty chapter 3 opens after chapter 2 in the editor.

### U2 — multi-zone fleet jobs

- Bake ~3 jobs: two ships and two zones, then harder, then three ships if the bake plan's U3 landed.
- Exit: the job assets validate.

### U3 — Mixed

- Bake ~3 Mixed jobs (grow one zone, shrink another); they need at least two routes and a field that
  bakes. If Mixed does not bake within a morning, chapter 3 ships with multi-zone jobs only and Mixed
  moves to the first update; record that here.
- Exit: the job assets validate, or the decision is written down.

### U4 — lines, manual and the ending

- Jim's opener and per-job lines, the manual entries for the damage framing, and the ending moved
  here (from the story arc). Regenerate the TTS clips.
- Exit: the chapter plays end to end, ending included.

## Manual check (headset)

1. Chapter 3 opens after chapter 2's last job, with its episode opener.
2. Each job reads as damage being put back; Jim plays it down.
3. Multi-zone jobs: each ship needs its own zone.
4. Mixed jobs (if in): one zone grows and another shrinks.
5. The ending plays after chapter 3, not after chapter 2.
6. Time each job; the target is ~3 min.
