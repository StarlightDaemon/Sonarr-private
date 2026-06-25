# Current State — Sonarr-private fork

**Branch:** feature/complete-series-pack-support
**HEAD:** c72ff6849
**Base:** upstream/v5-develop (Sonarr/Sonarr)
**Strategy:** Scenario C — long-term maintained patch-series fork with
periodic upstream rebases

## Patch series (7 commits ahead of upstream)

- 82c3ce1b4 New: Complete-series and multi-season pack support
- 0a6976144 Fixed: IsCompleteSeries gate, vacuous spec test, V5 AllowMultiSeasonPacks parity
- 12d21d4c1 Maint: audit remediation — README, polyfills, fonts, volta, preview NuGet
- cf73acb7d Chore: initialize RAIDEN state
- 6bfd11757 Maint: Docker hardening — jammy, signed-by, build user, hkps, https
- 440c3a88b Fixed: SeasonPackUpgrade logic for IsMultiSeason and IsCompleteSeries
- c72ff6849 Maint: minor cleanup — workflow version, package.json 5.x, yarn 0 vulns, gitignore

## Feature status

Complete. Decision engine is correct end-to-end:
- IsCompleteSeries and IsMultiSeason packs gate through AllowMultiSeasonPacks
- Both fields now route through SeasonPackUpgrade threshold logic in
  HistorySpecification and UpgradeDiskSpecification
- AllowMultiSeasonPacks present on V3 and V5 API surfaces
- Tests real and passing; solution build clean at 0 warnings

## Audit status

Audit v4.3 remediation complete for all fork-appropriate findings.
NuGet license inventory complete (.audits/nuget-licenses.md).
Remaining findings are upstream responsibility or explicitly deferred.
See OPEN_LOOPS.md.

## Docker packaging

GHCR publishing path is in place for homelab deployment:

- Runtime Dockerfile: distribution/docker-runtime/Dockerfile — single-stage
  image on mcr.microsoft.com/dotnet/aspnet:10.0-noble. (.NET 10 ships no
  bookworm/slim aspnet tag; noble is the GA, apt-based, glibc Debian-family
  substitute.) Non-root sonarr user at 1000:1000, entrypoint
  `dotnet Sonarr.dll -nobrowser -data=/config`, web UI served from /app/UI.
- Publish workflow: .github/workflows/docker-publish.yml — on push to
  feature/complete-series-pack-support, builds the frontend, publishes the
  backend (linux-x64, framework-dependent) to ./publish, stages _output/UI
  into ./publish/UI, then builds and pushes
  ghcr.io/starlightdaemon/sonarr-private :latest and
  :feature-complete-series-pack-support.
- Unraid template: distribution/unraid/sonarr-fork.xml — CA-compatible,
  host port 8990 -> container 8989, /config /tv /downloads volumes,
  PUID/PGID/TZ env, all fields Display=always / Required=true.

First-push gate: the GHCR package defaults to private on a private repo and
must be made public (or Unraid given registry creds) before it can be pulled
— tracked as OL-005.

## Next actions

1. Homelab deployment via GHCR pull (next immediate step) — push the branch
   to trigger docker-publish.yml, resolve OL-005 (package visibility), then
   deploy the Unraid template and smoke test.
2. Upstream rebase — gated on smoke test passing.
3. Squash 82c3ce1b4 and 0a6976144 into one commit — gated on smoke test
   passing / production validation.
4. Fork CI (build_fork.yml) — when rebase cadence warrants it.
5. FFprobeStatic LGPL attribution — before any public distribution.
