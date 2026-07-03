# Sonarr-private fork — runtime image
#
# Single-stage runtime image. The application (self-contained backend produced
# by `dotnet msbuild -t:PublishAllRids` plus the compiled web UI) is built
# separately by the docker-publish.yml GitHub Actions workflow and staged at
# ./publish; this Dockerfile only copies that staged output in and configures
# the runtime.
#
# Base image note: the backend is published SelfContained=true, so the .NET
# runtime is bundled in ./publish and no aspnet/runtime base image is required.
# A plain Ubuntu 24.04 "noble" base supplies only the native dependencies.
FROM ubuntu:noble

RUN apt-get update && apt-get install -y \
    sqlite3 \
    libmediainfo0v5 \
    libicu74 \
    curl \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app /config

RUN userdel -r ubuntu || true && \
    groupadd --gid 1000 sonarr && \
    useradd --uid 1000 --gid sonarr --no-create-home --shell /usr/sbin/nologin sonarr && \
    chown sonarr:sonarr /config

COPY --chmod=755 ./publish /app

WORKDIR /app

VOLUME ["/config"]
EXPOSE 8989

# Container runs as fixed UID/GID 1000:1000 (matching Unraid template PUID/PGID defaults).
# Pre-existing /config data written by older root-running images requires one-time
# host-side chown: chown -R 1000:1000 /path/to/config
USER sonarr

ENTRYPOINT ["/app/Sonarr", "-nobrowser", "-data=/config"]
