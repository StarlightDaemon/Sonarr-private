# Open Loops — Sonarr-private fork

| ID | Status | Gate | Area | Finding | Detail |
|----|--------|------|------|---------|--------|
| OL-001 | Open | operator | Compliance | ImageSharp Split License | SixLabors.ImageSharp 3.1.12 uses the Six Labors Split License. Apache-2.0 tier applies while the fork remains GPL-3.0 open-source. Revisit before any commercialization or closed-source distribution. Switch to SkiaSharp (MIT) if OSS status ever changes. |
| OL-002 | Deferred | operator | Compliance | FFprobeStatic LGPL attribution | Openur.FFprobeStatic ships FFmpeg binaries with no LGPL-2.1 documentation in the package. Add FFmpeg attribution to ThirdPartyNotices before any public distribution. Deferred: private fork, low risk. |
| OL-003 | Deferred | operator | CI | Fork CI workflow | build_fork.yml not yet created. Create when upstream rebase cadence increases or before any public distribution. |
| OL-004 | Deferred | operator | Git | Commit squash | Squash 82c3ce1b4 (feature) and 0a6976144 (fix) into one clean commit before the patch series grows further. Deferred until after production validation confirms the feature works (homelab GHCR deployment + smoke test, an operator action). |
| OL-005 | Resolved | none | Deployment | GHCR package visibility | Resolved 2026-07-02: verified directly that ghcr.io/starlightdaemon/sonarr-private is **public** (anonymous registry tags/list returns HTTP 200; tags: latest, feature-complete-series-pack-support) and the StarlightDaemon/Sonarr-private repository itself is public. Unraid can pull without credentials. No action remains. Re-verified 2026-07-09 against commit ad6b21a32 and .audits/STATUS.md — status unchanged. |
