# Current State — Sonarr-private fork

> **Ledger reconciled 2026-07-08.** The previous ledger claimed HEAD `c72ff6849`
> with a 7-commit patch series; that was badly stale. Verified reality below.

**Branch:** feature/complete-series-pack-support
**HEAD:** 7fe04823f (Fixed: add libicu74 to runtime Dockerfile for .NET
globalization support — 2026-06-26)
**Base:** upstream/v5-develop (Sonarr/Sonarr)
**Strategy:** Scenario C — long-term maintained patch-series fork with
periodic upstream rebases

## Patch series (15 commits ahead of upstream)

- 82c3ce1b4 New: Complete-series and multi-season pack support
- 0a6976144 Fixed: IsCompleteSeries gate, vacuous spec test, and V5 AllowMultiSeasonPacks parity
- 12d21d4c1 Maint: audit remediation — README fork guide, polyfill cleanup, font attribution, HappyEyeballs note, volta alignment, remove preview NuGet
- cf73acb7d Chore: initialize RAIDEN state — open loops and current state
- 6bfd11757 Maint: Docker hardening — jammy base, signed-by keyring, build user, hkps transport, https apt source
- 440c3a88b Fixed: extend SeasonPackUpgrade logic to IsMultiSeason and IsCompleteSeries packs in HistorySpecification and UpgradeDiskSpecification
- c72ff6849 Maint: minor cleanup — workflow version alignment, package.json 5.x, yarn audit clean (0 vulns), .audits gitignore
- 2ca8e1840 Chore: update RAIDEN state — 7-commit patch series, pre-homelab handoff
- 4958f1c92 New: runtime Dockerfile, GHA docker-publish workflow, Unraid template; Maint: RAIDEN state
- 3f1e56c21 Fixed: docker-publish workflow — add --framework net10.0, bump setup-node to v5
- 1a97aa2b7 Fixed: docker-publish workflow — two-step build/publish to avoid StyleCop flood
- 75166c0c7 Fixed: docker-publish workflow — correct build/publish pattern, disable analyzers on publish step
- f60fb4d05 Docs: add public-facing warning banner to README
- deaa4b832 Fixed: use msbuild PublishAllRids for complete artifact; ubuntu:noble base, run as root
- 7fe04823f Fixed: add libicu74 to runtime Dockerfile for .NET globalization support

(Reconciliation note 2026-07-08: the next state-reconcile commit adds a 16th
commit to this series.)

## Feature status

Complete. Decision engine is correct end-to-end:
- IsCompleteSeries and IsMultiSeason packs gate through AllowMultiSeasonPacks
- Both fields now route through SeasonPackUpgrade threshold logic in
  HistorySpecification and UpgradeDiskSpecification (shared
  SeasonPackUpgradeDecider.cs; DownloadRejectionReason.cs present)
- AllowMultiSeasonPacks present on V3 and V5 API surfaces
- Tests real and passing; solution build clean at 0 warnings

## Audit status

Audit v4.3 remediation complete for all fork-appropriate findings.
NuGet license inventory complete (.audits/nuget-licenses.md).
Remaining findings are upstream responsibility or explicitly deferred.
See OPEN_LOOPS.md.

## Docker packaging

GHCR publishing path is in place for homelab deployment (facts re-verified
against the repo 2026-07-08):

- Runtime Dockerfile: distribution/docker-runtime/Dockerfile — single-stage
  image on `ubuntu:noble` (24.04). The backend is published SelfContained=true
  via `dotnet msbuild -t:PublishAllRids`, so the .NET runtime is bundled in
  ./publish and no aspnet/runtime base image is used; the base supplies only
  native deps (sqlite3, libmediainfo0v5, libicu74, curl). Entrypoint
  `/app/Sonarr -nobrowser -data=/config`, web UI served from staged output.
  NOTE: **this copy currently runs as root** (no USER directive). The
  sonarr-overnight twin re-added a non-root sonarr user at 1000:1000 — see
  Twin working copies below. (The earlier ledger's `aspnet:10.0-noble` /
  `dotnet Sonarr.dll` / non-root description is superseded and was corrected
  in this reconcile.)
- Publish workflow: .github/workflows/docker-publish.yml — on push to
  feature/complete-series-pack-support, builds the frontend, publishes the
  backend, and builds and pushes ghcr.io/starlightdaemon/sonarr-private
  :latest and :feature-complete-series-pack-support.
- Unraid template: distribution/unraid/sonarr-fork.xml — CA-compatible,
  host port 8990 -> container 8989, /config /tv /downloads volumes,
  PUID/PGID/TZ env, all fields Display=always / Required=true.

GHCR package visibility (was OL-005) is RESOLVED — the package and repo were
verified public on 2026-07-02. See OPEN_LOOPS.md.

## Twin working copies

Two working copies of this fork exist side by side:
- /Users/dante/Citadel/Sonarr_SD/sonarr/           (THIS copy)
- /Users/dante/Citadel/Sonarr_SD/sonarr-overnight/ (the twin)

As of 2026-07-08 they have **diverged**. This copy sits on
feature/complete-series-pack-support @ 7fe04823f, 15 commits ahead of upstream.
The overnight twin is checked out on a `temp` branch @ 8151cefa9, **35 commits
ahead** (a rewritten/rebased version of this series sharing only base commit
82c3ce1b4, plus ~20 additional commits: CI hardening, GitHub-Actions SHA
pinning, dependency bumps (F28/FG1/FG2), further multi-season fixes, and a
non-root Dockerfile). **The overnight twin is ahead / more active.**
Consolidation is an **open operator decision** — do not merge or delete either
copy without a deliberate call.

## Next actions

1. Homelab deployment via GHCR pull — push the branch to trigger
   docker-publish.yml, then deploy the Unraid template and smoke test.
   (OL-005 package-visibility gate already cleared.)
2. Operator decision: consolidate the twin copies (see Twin working copies).
3. Upstream rebase — gated on smoke test passing.
4. Squash the feature+fix commits into one — gated on production validation
   (OL-004).
5. Fork CI (build_fork.yml) — when rebase cadence warrants it (OL-003).
6. FFprobeStatic LGPL attribution — before any public distribution (OL-002).
