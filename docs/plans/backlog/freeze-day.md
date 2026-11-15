# Freeze day

**Status:** next · flow · updated 2026-11-15 — the headset pass is done (2026-11-15, its fixes are
committed); left: the frozen build on the "Competition" channel.

**Related:** [Review](../../reviews/review.md) findings 4, 5 and 9 · [plans board](../README.md)

## Goal

The entry freezes: the APK on the "Competition" channel has passed the headset checks and launches
from the invite URL.

## Units

### U2 — the build

- Build the APK, upload it to the "Competition" channel, install from the invite URL and launch.
- Exit: the frozen build runs from the invite, end to end on hands only.

## Cut for the freeze

- The score on the map (running total, `BEST`) and the two "current" jobs after a fail.
- The in-run reset, stars on job 2, chapter 2's doc wording, job 5's second route.

## Manual check (headset)

1. The invite-URL install launches straight into the boot, no black frame.
2. A full hands-only path: boot → job 1 → map → job 2, with a pause and resume on the way.
