# Local Live State

Repo-local continuity state lives here.

This is a **ledger-form** RAIDEN Instance (`.raiden/instance/metadata.json`,
`instance_form_type: "ledger"`) — state continuity and fleet registry
visibility only. There is no managed Writ, no baseline, no git hook, and no
installed Edict version to read. Reading order:

1. `CURRENT_STATE.md` — what is true right now.
2. `OPEN_LOOPS.md` — durable work items, each with a Gate and a Success
   Condition.
3. `DECISIONS.md` — the append-only decision record for this repo.
4. `WORK_LOG.md` — append-only session history; volatile counts live here,
   dated.
