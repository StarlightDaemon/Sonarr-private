FROM ubuntu:jammy AS builder

ENV DEBIAN_FRONTEND noninteractive 
ENV MONO_VERSION 5.18

RUN apt-get update && \
    apt-get -y -o Dpkg::Options::="--force-confold" install --no-install-recommends \
        apt-transport-https \
        wget dirmngr gpg gpg-agent \
        # add-apt-repository for PPAs
        software-properties-common && \
    rm -rf /var/lib/apt/lists/*
	
RUN install -m 0755 -d /usr/share/keyrings && \
    gpg --no-default-keyring \
        --keyring /usr/share/keyrings/mono-official.gpg \
        --keyserver hkps://keyserver.ubuntu.com \
        --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb [signed-by=/usr/share/keyrings/mono-official.gpg] http://download.mono-project.com/repo/debian stable-jammy main" > /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && apt-get install -y \
        devscripts build-essential tofrodos \
        dh-make dh-systemd \
        cli-common-dev \
        mono-complete \
        sqlite3 libcurl4 mediainfo

RUN apt-cache policy mono-complete
RUN apt-cache policy cli-common-dev

COPY debian-start.sh /debian-start.sh
RUN  fromdos /debian-start.sh

# Volumes must be writable by UID 4021 on the host
RUN groupadd sonarrbuilder -g 4020 && \
    useradd sonarrbuilder -u 4021 -g 4020 -m -s /bin/bash
USER sonarrbuilder

WORKDIR /data
VOLUME [ "/data/sonarr_bin", "/data/build", "/data/output" ]
CMD /debian-start.sh
