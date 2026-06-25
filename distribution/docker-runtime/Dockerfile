# Sonarr-private fork — runtime image
#
# Single-stage runtime image. The application (backend `dotnet publish` output
# plus the compiled web UI) is built separately by the docker-publish.yml
# GitHub Actions workflow and staged at ./publish; this Dockerfile only copies
# that staged output in and configures the runtime.
#
# Base image note: .NET 10 does NOT ship a "10.0-bookworm-slim" aspnet tag.
# Microsoft did not publish a Debian bookworm (or GA slim) aspnet image for
# .NET 10 — the only GA, apt-based, glibc Debian-family runtime image is
# Ubuntu 24.04 "noble" (trixie-slim exists only as a preview tag). 10.0-noble
# is therefore the correct substitute and is used here.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble

# Runtime dependencies:
#   sqlite3         - Sonarr's embedded database engine
#   libmediainfo0v5 - native media analysis library (Ubuntu universe, enabled
#                     by default on the noble base image)
#   curl            - health checks / diagnostics
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        sqlite3 \
        libmediainfo0v5 \
        curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Non-root runtime user at 1000:1000 (matches the Unraid PUID/PGID defaults).
# Ubuntu 24.04 ships a default "ubuntu" user/group already occupying UID/GID
# 1000, so it is removed first to free those IDs for the sonarr account.
RUN userdel -r ubuntu 2>/dev/null || true; \
    groupdel ubuntu 2>/dev/null || true; \
    groupadd -g 1000 sonarr && \
    useradd -u 1000 -g sonarr -d /config -s /usr/sbin/nologin sonarr && \
    mkdir -p /app /config && \
    chown -R sonarr:sonarr /app /config

# Application binaries + web UI, staged at ./publish by the workflow.
COPY --chown=sonarr:sonarr ./publish /app

USER sonarr
WORKDIR /app

VOLUME /config
EXPOSE 8989

# AssemblyName is "Sonarr" for non-Windows runtimes (see Sonarr.Console.csproj),
# so the published entry assembly is Sonarr.dll.
ENTRYPOINT ["dotnet", "Sonarr.dll", "-nobrowser", "-data=/config"]
