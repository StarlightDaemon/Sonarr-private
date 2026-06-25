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

## Next actions

1. Homelab deployment and smoke testing (next immediate step)
2. Upstream rebase — after production smoke testing confirms feature works
3. Squash 82c3ce1b4 and 0a6976144 into one commit — after production validation
4. Fork CI (build_fork.yml) — when rebase cadence warrants it
5. FFprobeStatic LGPL attribution — before any public distribution
