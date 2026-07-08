# Current State — Sonarr-private fork (overnight copy)

> **Ledger reconciled 2026-07-08.** The previous ledger claimed HEAD `c72ff6849`
> with a 7-commit patch series on feature/complete-series-pack-support; that was
> badly stale. This copy is actually checked out on the `temp` branch. Verified
> reality below.

**Branch:** temp (checked out HEAD)
**HEAD:** 8151cefa9 (Maint: bump contained-major frontend deps (FG2) —
2026-07-02)
**Also present:** feature/complete-series-pack-support @ 7fe04823f (15 commits
ahead of upstream — the pre-divergence tip, identical to the plain sonarr twin)
**Base:** upstream/v5-develop (Sonarr/Sonarr)
**Strategy:** Scenario C — long-term maintained patch-series fork with
periodic upstream rebases

## Patch series — `temp` (35 commits ahead of upstream)

`temp` shares only base commit 82c3ce1b4 with feature/complete-series-pack-support;
every commit above it was rewritten/rebased, then ~20 further commits were added.

- 82c3ce1b4 New: Complete-series and multi-season pack support
- 463c68e19 Fixed: IsCompleteSeries gate, vacuous spec test, and V5 AllowMultiSeasonPacks parity
- aeab7897c Maint: audit remediation — README fork guide, polyfill cleanup, font attribution, HappyEyeballs note, volta alignment, remove preview NuGet
- ab85357a5 Chore: initialize RAIDEN state — open loops and current state
- 33d65a432 Maint: Docker hardening — jammy base, signed-by keyring, build user, hkps transport, https apt source
- f7011f89d Fixed: extend SeasonPackUpgrade logic to IsMultiSeason and IsCompleteSeries packs in HistorySpecification and UpgradeDiskSpecification
- 37fa8f326 Maint: minor cleanup — workflow version alignment, package.json 5.x, yarn audit clean (0 vulns), .audits gitignore
- 331a9cc20 Chore: update RAIDEN state — 7-commit patch series, pre-homelab handoff
- 6c0784b91 New: runtime Dockerfile, GHA docker-publish workflow, Unraid template; Maint: RAIDEN state
- 8cf95b83e Fixed: docker-publish workflow — add --framework net10.0, bump setup-node to v5
- ccfba2c2e Fixed: docker-publish workflow — two-step build/publish to avoid StyleCop flood
- 650a0493c Fixed: docker-publish workflow — correct build/publish pattern, disable analyzers on publish step
- 73f7c9c2e Docs: add public-facing warning banner to README
- c0c8c432b Fixed: use msbuild PublishAllRids for complete artifact; ubuntu:noble base, run as root
- f26d508d4 Fixed: add libicu74 to runtime Dockerfile for .NET globalization support
- 1bd6f9096 Fixed: run build_v5.yml test CI on feature branch pushes
- 20d4d26fc Fixed: pin third-party GitHub Actions to commit SHAs
- f8c2a5a0f Fixed: add minimal top-level permissions block to build_v5.yml
- 50ded6333 Fixed: align docker-publish.yml Node version with project's volta pin
- cd4e7bf45 Fixed: stale comment in Parser.cs contradicted by AllowMultiSeasonPacks escape hatch
- f612a938e Fixed: hand-sync V5 openapi.json for historyNotUpgrade + allowMultiSeasonPacks
- 1a5c7880c Fixed: add non-root sonarr user (UID/GID 1000:1000) to runtime Dockerfile
- 4b929dc45 Fixed: deduplicate zero-count guard in UpgradeDiskSpecification
- f2b7af01e Fixed: harden DownloadedEpisodesImportService multi-season gate to also check IsCompleteSeries
- c1811259d Fixed: clarify AllowMultiSeasonPacks help text dependency on Season Pack Upgrade mode
- 9000821c1 Fixed: reject multi-season packs for scene numbered series instead of literal fallback
- ad6b21a32 Maint: mark OL-005 resolved - GHCR package and repo verified public 2026-07-02
- ccc93eda8 Maint: remove deprecated System.IO.FileSystem.AccessControl package reference
- 65ef657c2 Maint: bump patch-level dependencies (F28 batch 1, group 1)
- 3a9181ad6 Maint: bump test toolchain dependencies (F28 batch 1, group 2)
- 877bb6c76 Maint: bump FluentAssertions 7.2.2 -> 8.10.0 (F28 batch 1, group 3)
- e7c75467d Maint: bump Sentry 5.16.3 -> 6.6.0 (F28 batch 2, group 5)
- d1de12e3a Maint: bump Selenium.Support 3.141.0 -> 4.45.0, ChromeDriver 134.x -> 150.0.7871.4600
- 00adcbfce Maint: bump same-major frontend deps (FG1)
- 8151cefa9 Maint: bump contained-major frontend deps (FG2)

(Reconciliation note 2026-07-08: the next state-reconcile commit adds a 36th
commit to `temp`.)

## Feature status

Complete. Decision engine is correct end-to-end:
- IsCompleteSeries and IsMultiSeason packs gate through AllowMultiSeasonPacks
- Both fields route through SeasonPackUpgrade threshold logic in
  HistorySpecification and UpgradeDiskSpecification (shared
  SeasonPackUpgradeDecider.cs; DownloadRejectionReason.cs present)
- AllowMultiSeasonPacks present on V3 and V5 API surfaces (V5 openapi.json
  hand-synced)
- Additional post-feature fixes on `temp`: multi-season gate hardened for
  scene-numbered series and IsCompleteSeries in the import service.
- Tests real and passing (Sonarr.Core.Test: 5158 passed, 0 failed as of the
  FG2 dependency bump).

## Audit status

Audit v4.3 remediation complete for all fork-appropriate findings.
NuGet license inventory complete (.audits/nuget-licenses.md).
CI hardening added on `temp`: third-party Actions pinned to commit SHAs,
top-level permissions block on build_v5.yml. Dependency-bump campaign (F28,
FG1, FG2) in progress; several deps intentionally left-and-noted (css-loader,
postcss-loader, react-slider — see commit 8151cefa9). See OPEN_LOOPS.md.

## Docker packaging

GHCR publishing path is in place for homelab deployment (facts re-verified
against the repo 2026-07-08):

- Runtime Dockerfile: distribution/docker-runtime/Dockerfile — single-stage
  image on `ubuntu:noble` (24.04). The backend is published SelfContained=true
  via `dotnet msbuild -t:PublishAllRids`, so the .NET runtime is bundled in
  ./publish and no aspnet/runtime base image is used; the base supplies only
  native deps (sqlite3, libmediainfo0v5, libicu74, curl). Entrypoint
  `/app/Sonarr -nobrowser -data=/config`. **This copy runs non-root** as a
  dedicated sonarr user at UID/GID 1000:1000 (commit 1a5c7880c). Pre-existing
  root-written /config needs a one-time host chown to 1000:1000. (The earlier
  ledger's `aspnet:10.0-noble` / `dotnet Sonarr.dll` description is superseded
  and was corrected in this reconcile.)
- Publish workflow: .github/workflows/docker-publish.yml — on push to
  feature/complete-series-pack-support, builds the frontend, publishes the
  backend, and builds and pushes ghcr.io/starlightdaemon/sonarr-private
  :latest and :feature-complete-series-pack-support.
- Unraid template: distribution/unraid/sonarr-fork.xml — CA-compatible,
  host port 8990 -> container 8989, /config /tv /downloads volumes,
  PUID/PGID/TZ env, all fields Display=always / Required=true.

GHCR package visibility (OL-005) is RESOLVED — verified public 2026-07-02
(anonymous registry tags/list HTTP 200; tags latest,
feature-complete-series-pack-support). See OPEN_LOOPS.md.

## Twin working copies

Two working copies of this fork exist side by side:
- /Users/dante/Citadel/Sonarr_SD/sonarr-overnight/ (THIS copy)
- /Users/dante/Citadel/Sonarr_SD/sonarr/           (the twin)

As of 2026-07-08 they have **diverged**. This copy is on `temp` @ 8151cefa9,
**35 commits ahead** of upstream. The plain-sonarr twin sits on
feature/complete-series-pack-support @ 7fe04823f, only 15 commits ahead — the
pre-divergence tip. `temp` shares only base commit 82c3ce1b4 with that branch;
everything above was rewritten, plus ~20 additional commits (CI hardening,
Actions SHA pinning, dependency bumps, further multi-season fixes, non-root
Dockerfile). **This overnight copy is ahead / more active.** Consolidation is
an **open operator decision** — do not merge or delete either copy without a
deliberate call.

## Next actions

1. Homelab deployment via GHCR pull — push the branch to trigger
   docker-publish.yml, then deploy the Unraid template and smoke test.
   (OL-005 package-visibility gate already cleared.)
2. Operator decision: consolidate the twin copies (see Twin working copies),
   and decide whether `temp` should be fast-forwarded onto / replace
   feature/complete-series-pack-support before the next rebase.
3. Upstream rebase — gated on smoke test passing.
4. Squash the feature+fix commits into one — gated on production validation
   (OL-004).
5. Fork CI (build_fork.yml) — when rebase cadence warrants it (OL-003).
6. FFprobeStatic LGPL attribution — before any public distribution (OL-002).
7. Continue dependency-bump campaign; revisit the left-and-noted deps
   (css-loader, postcss-loader, react-slider).
