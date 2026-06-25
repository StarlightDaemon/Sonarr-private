# Open Loops — Sonarr-private fork

| ID | Status | Area | Finding | Detail |
|----|--------|------|---------|--------|
| OL-001 | Open | Compliance | ImageSharp Split License | SixLabors.ImageSharp 3.1.12 uses the Six Labors Split License. Apache-2.0 tier applies while the fork remains GPL-3.0 open-source. Revisit before any commercialization or closed-source distribution. Switch to SkiaSharp (MIT) if OSS status ever changes. |
| OL-002 | Deferred | Compliance | FFprobeStatic LGPL attribution | Openur.FFprobeStatic ships FFmpeg binaries with no LGPL-2.1 documentation in the package. Add FFmpeg attribution to ThirdPartyNotices before any public distribution. Deferred: private fork, low risk. |
| OL-003 | Deferred | CI | Fork CI workflow | build_fork.yml not yet created. Phase 7 from the feature completion plan. Create when upstream rebase cadence increases or before any public distribution. |
