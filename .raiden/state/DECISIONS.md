# Decisions

## D-001

- Date: 2026-07-09
- Status: Active
- Decision: sonarr-overnight is enrolled as a ledger-form RAIDEN Instance per
  Raiden-ops:OPS-D-002 — state continuity and fleet registry visibility only;
  no managed Writ, no baseline, no git hook, no installed Edict version.
- Rationale: this repo had a self-invented, unregistered `.raiden/state/`
  (only `CURRENT_STATE.md` and `OPEN_LOOPS.md`, both stale). The diagnosis of
  that rot was unregistration, not lightness — an unregistered ledger is
  invisible to every fleet mechanism, while a registered one is swept like
  everything else. Ledger form gives this repo registry visibility and state
  discipline without imposing managed-law machinery it does not need.
- Implementation note: upgrade to a full instance (Writ + baseline + hook +
  installed Edict version) is the normal install (`AGENT_INSTALL.md`) laid
  over this already-present state layer. It is not implied by this
  enrollment and is a future operator decision.
