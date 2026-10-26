---
description: Validate the plans board — links resolve and each plan's status matches its folder.
---

Run the plans checker and report the result:

!`powershell -NoProfile -ExecutionPolicy Bypass -File docs/plans/check-plans.ps1`

If it reports problems, fix them (repoint the broken link, correct the `**Status:**` line, or add
the missing row to `docs/plans/README.md`), then re-run until it prints `check-plans: OK`.
