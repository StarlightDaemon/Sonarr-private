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
    curl \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app /config

COPY --chmod=755 ./publish /app

WORKDIR /app

VOLUME ["/config"]
EXPOSE 8989

ENTRYPOINT ["/app/Sonarr", "-nobrowser", "-data=/config"]
