# Freeze day

**Status:** next · flow · updated 2026-11-12 — no new features: the headset pass, value-only tuning
and the frozen build on the "Competition" channel.

**Related:** [Review](../../reviews/review.md) findings 4, 5 and 9 ·
[hand-tracking gate](../in-progress/hand-tracking-gate.md) · [plans board](../README.md)

## Goal

On 2026-11-13 the entry freezes: the APK on the "Competition" channel has passed the headset checks
and launches from the invite URL.

## Units

### U1 — the headset pass

- Run the hand-tracking gate's manual list (its U3) and the meter/dot checks below.
- Placement at a desk and on the lap with a 2 ft tape: field height, reach to the far endpoint, the
  boot console's height. Tune `SeatSettings` values only.
- Jim over the music: if a line is masked, lower the music under him by value; no duck system.
- Exit: both lists pass; any tuning is values only.

### U2 — the build

- Build the APK, upload it to the "Competition" channel, install from the invite URL and launch.
- Exit: the frozen build runs from the invite, end to end on hands only.

## Cut for the freeze

- The score on the map (running total, `BEST`) and the two "current" jobs after a fail.
- The in-run reset, stars on job 2, chapter 2's doc wording, the map's chapter break, job 5's second
  route.

## Manual check (headset)

1. Job 1's meter shows the entropy post, no stars or notches; its pending dot warms near the ring.
2. Job 2's meter shows no stars; overspending still fails; its dot still warms.
3. Job 3's stars are lit and flicker on a costly pinch; its pending dot stays neutral.
4. Release on job 3 still shows the outcome (arrival sphere or crash).
5. The boot has no "Revenue is relative." line.
6. On the lap with no desk, the far endpoint is reachable without leaning.
7. At a desk, the field sits at resting-hand height.
8. The boot console sits at eye level, ahead, in both seats.
9. Jim's lines are clear over the music in jobs 1 and 3 and at the ending.
10. The invite-URL install launches straight into the boot, no black frame.
11. A full hands-only path: boot → job 1 → map → job 2, with a pause and resume on the way.
