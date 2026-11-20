# Release build

**Status:** next · flow · updated 2026-11-19 — the headset timing pass, the release build and the
submission for 2026-12-06.

**Related:** [plans board](../README.md) · [release](../../design/release.md#schedule) ·
[store page](store-page.md) · [entitlement check](entitlement-check.md)

## Goal

A release build on the store channel, submitted early enough for review to finish by 2026-12-06.

## Units

### U1 — the submission date

- Use the review time from [store page](store-page.md) U1: if review takes a week or more, submit the
  story by ~28 Nov and ship freelance as an update.
- Exit: the submission date and what it carries are in the release doc.

### U2 — timing pass

- Play the whole story on the headset from a fresh save and time each chapter. Target 40–45 minutes.
- Long → shrink chapter 3; short → freelance fills the gap.
- Exit: per-chapter times in the release doc.

### U3 — the build

- Release config: version and bundle numbers, dev-only features off, the entitlement check on, the
  right store channel. Run both test suites.
- Exit: the APK passes Meta's VRC checks locally and installs from the channel.

### U4 — submit

- Upload, fill the remaining dashboard items, submit for review.
- Exit: submitted; status checked daily until approved.

## Manual check (headset)

1. Install from the store channel on a clean headset.
2. Fresh save: boot reaches chapter 1 within the first 30 seconds.
3. Play to the ending; freelance opens after it.
4. Pause, resume and an offline launch work.
5. The judge tests still pass: hands only, two-foot radius.
