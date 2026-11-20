# Entitlement check

**Status:** next · flow · updated 2026-11-19 — check the Meta entitlement at boot; a player without
it cannot play.

**Related:** [plans board](../README.md) · [release](../../design/release.md#scope)

## Goal

The paid build checks the Meta entitlement at startup, as the store requires. Entitled players see no
change; anyone else gets a short message and the app quits.

## Units

### U1 — the check

- Add an entitlement check to `MetaPlatformClient` (`_Modules/XR`): after init, call
  `Entitlements.IsUserEntitledToApplication()` and report entitled, not entitled or failed.
- Exit: compiles; called once per session.

### U2 — wire it into boot

- The boot flow runs the check during the console boot, without delaying the first 30 seconds for
  entitled players.
- Not entitled → a short console message, then quit. A failed check while offline must not lock out
  a player who owns the game: follow Meta's guidance for offline launches.
- The editor and dev builds skip the check (a build define or a serialized flag on the boot owner).
- Exit: the flow handles all three results; an EditMode test covers the decision.

### U3 — device check

- Upload to a test channel; launch with an entitled and an unentitled account.
- Exit: both behave as specified.

## Manual check (headset)

1. Entitled account: the app boots as before, with no extra wait.
2. Unentitled account: the message shows, then the app quits.
3. Airplane mode with an entitled account: the app still opens.
4. Pause and resume do not run the check again.
