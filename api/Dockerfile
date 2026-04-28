# syntax=docker/dockerfile:1

# Node.js build stage for web frontend
FROM node:22-bookworm-slim@sha256:80fdb3f57c815e1b638d221f30a826823467c4a56c8f6a8d7aa091cd9b1675ea AS node-build
WORKDIR /src

# Copy web frontend sources
COPY web web/
COPY packages packages/
COPY package.json package-lock.json ./

# Build web frontend
RUN npm ci && npm run build:web

# .NET build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:f061e5a7532b36fa1d1b684857fe1f504ba92115b9934f154643266613c44c62 AS build
WORKDIR /src

# Copy csproj files and restore
COPY api/src/Relate.Smtp.Core/Relate.Smtp.Core.csproj api/src/Relate.Smtp.Core/
COPY api/src/Relate.Smtp.Infrastructure/Relate.Smtp.Infrastructure.csproj api/src/Relate.Smtp.Infrastructure/
COPY api/src/Relate.Smtp.Api/Relate.Smtp.Api.csproj api/src/Relate.Smtp.Api/
COPY api/src/Relate.Smtp.SmtpHost/Relate.Smtp.SmtpHost.csproj api/src/Relate.Smtp.SmtpHost/
COPY api/src/Relate.Smtp.Pop3Host/Relate.Smtp.Pop3Host.csproj api/src/Relate.Smtp.Pop3Host/
COPY api/src/Relate.Smtp.ImapHost/Relate.Smtp.ImapHost.csproj api/src/Relate.Smtp.ImapHost/
RUN dotnet restore api/src/Relate.Smtp.Api/Relate.Smtp.Api.csproj
RUN dotnet restore api/src/Relate.Smtp.SmtpHost/Relate.Smtp.SmtpHost.csproj
RUN dotnet restore api/src/Relate.Smtp.Pop3Host/Relate.Smtp.Pop3Host.csproj
RUN dotnet restore api/src/Relate.Smtp.ImapHost/Relate.Smtp.ImapHost.csproj

# Copy backend sources
COPY api api/

# Copy pre-built web folder from node-build stage (includes dist)
# The .csproj skips npm build if dist/index.html exists
COPY --from=node-build /src/web /src/web
COPY --from=node-build /src/packages /src/packages
COPY --from=node-build /src/package.json /src/package.json

# Build backend (API publish will detect existing web/dist and skip npm build)
WORKDIR /src/api
RUN dotnet publish src/Relate.Smtp.Api/Relate.Smtp.Api.csproj -c Release -o /app/api
RUN dotnet publish src/Relate.Smtp.SmtpHost/Relate.Smtp.SmtpHost.csproj -c Release -o /app/smtp
RUN dotnet publish src/Relate.Smtp.Pop3Host/Relate.Smtp.Pop3Host.csproj -c Release -o /app/pop3
RUN dotnet publish src/Relate.Smtp.ImapHost/Relate.Smtp.ImapHost.csproj -c Release -o /app/imap

# Build a self-contained EF Core migrations bundle. The bundle is a single
# executable that applies pending migrations against a target connection
# string passed via --connection, with no SDK or EF tools required at runtime.
ARG TARGETARCH
RUN dotnet tool install -g dotnet-ef --version 10.* \
    && case "$TARGETARCH" in \
        amd64) RID=linux-musl-x64 ;; \
        arm64) RID=linux-musl-arm64 ;; \
        *) echo "Unsupported TARGETARCH: $TARGETARCH" && exit 1 ;; \
       esac \
    && /root/.dotnet/tools/dotnet-ef migrations bundle \
        --project src/Relate.Smtp.Infrastructure \
        --startup-project src/Relate.Smtp.Api \
        --self-contained \
        -r "$RID" \
        -o /app/migrations/efbundle

# API runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:8c7671a6f0f984d0c102ee70d61e8010857de032b320561dea97cc5781aea5f8 AS api
WORKDIR /app

COPY --from=build /app/api .

# Switch to built-in non-root user (UID 1654 in .NET 8+ images)
USER app

# OCI Labels
LABEL org.opencontainers.image.title="Relate Mail API"
LABEL org.opencontainers.image.description="REST API for Relate Mail email management"
LABEL org.opencontainers.image.vendor="Relate Mail"
LABEL org.opencontainers.image.source="https://github.com/four-robots/relate-mail"

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Relate.Smtp.Api.dll"]

# SMTP runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:8c7671a6f0f984d0c102ee70d61e8010857de032b320561dea97cc5781aea5f8 AS smtp
WORKDIR /app

COPY --from=build /app/smtp .

# Switch to built-in non-root user (UID 1654 in .NET 8+ images)
USER app

# OCI Labels
LABEL org.opencontainers.image.title="Relate Mail SMTP Server"
LABEL org.opencontainers.image.description="SMTP server for receiving emails"
LABEL org.opencontainers.image.vendor="Relate Mail"
LABEL org.opencontainers.image.source="https://github.com/four-robots/relate-mail"

EXPOSE 25 587 465 8081
ENV HealthCheck__Url=http://+:8081
ENTRYPOINT ["dotnet", "Relate.Smtp.SmtpHost.dll"]

# POP3 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:8c7671a6f0f984d0c102ee70d61e8010857de032b320561dea97cc5781aea5f8 AS pop3
WORKDIR /app

COPY --from=build /app/pop3 .

# Switch to built-in non-root user (UID 1654 in .NET 8+ images)
USER app

# OCI Labels
LABEL org.opencontainers.image.title="Relate Mail POP3 Server"
LABEL org.opencontainers.image.description="POP3 server for retrieving emails"
LABEL org.opencontainers.image.vendor="Relate Mail"
LABEL org.opencontainers.image.source="https://github.com/four-robots/relate-mail"

EXPOSE 110 995 8082
ENV HealthCheck__Url=http://+:8082
ENTRYPOINT ["dotnet", "Relate.Smtp.Pop3Host.dll"]

# IMAP runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:8c7671a6f0f984d0c102ee70d61e8010857de032b320561dea97cc5781aea5f8 AS imap
WORKDIR /app

COPY --from=build /app/imap .

# Switch to built-in non-root user (UID 1654 in .NET 8+ images)
USER app

# OCI Labels
LABEL org.opencontainers.image.title="Relate Mail IMAP Server"
LABEL org.opencontainers.image.description="IMAP server for retrieving emails"
LABEL org.opencontainers.image.vendor="Relate Mail"
LABEL org.opencontainers.image.source="https://github.com/four-robots/relate-mail"

EXPOSE 143 993 8083
ENV HealthCheck__Url=http://+:8083
ENTRYPOINT ["dotnet", "Relate.Smtp.ImapHost.dll"]

# EF Core migrations runtime image. Tiny image containing just the self-contained
# migration bundle. Used by the chart's pre-install/pre-upgrade Job to bring the
# database schema up to date before the API/protocol pods start.
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine@sha256:4f08c162590324de60f31937f5b5fa9f2b5eddaa4f0aaec3c872f855bf16c36c AS migrations
WORKDIR /app

COPY --from=build /app/migrations/efbundle ./efbundle

USER app

# OCI Labels
LABEL org.opencontainers.image.title="Relate Mail Migrations"
LABEL org.opencontainers.image.description="EF Core migration bundle for the Relate Mail database schema"
LABEL org.opencontainers.image.vendor="Relate Mail"
LABEL org.opencontainers.image.source="https://github.com/four-robots/relate-mail"

ENTRYPOINT ["./efbundle"]

# Web (SPA) runtime image — separate from the API image so it can roll independently.
# nginxinc/nginx-unprivileged runs as UID 101 and listens on 8080 by default, so no
# privileged-port handling is needed in k8s/k3s.
FROM nginxinc/nginx-unprivileged:1.27-alpine@sha256:65e3e85dbaed8ba248841d9d58a899b6197106c23cb0ff1a132b7bfe0547e4c0 AS web

# Templates listed under /etc/nginx/templates/*.template are envsubst'd into
# /etc/nginx/conf.d/ on container start by the base image. We use that to inject
# ${API_BACKEND} into the proxy_pass directive.
COPY web/nginx.conf /etc/nginx/templates/default.conf.template

# Hooks under /docker-entrypoint.d/ are executed before nginx starts.
COPY web/docker-entrypoint.sh /docker-entrypoint.d/30-generate-config-json.sh

# Static assets
COPY --from=node-build /src/web/dist /usr/share/nginx/html

# Defaults; override in k8s. API_BACKEND points the /api proxy at the in-cluster
# API service. The OIDC_* vars are baked into config.json by the entrypoint hook.
ENV API_BACKEND=http://api:8080 \
    OIDC_AUTHORITY="" \
    OIDC_CLIENT_ID="" \
    OIDC_REDIRECT_URI="" \
    OIDC_SCOPE="openid profile email"

# OCI Labels
LABEL org.opencontainers.image.title="Relate Mail Web"
LABEL org.opencontainers.image.description="React + Vite SPA frontend for Relate Mail, served by nginx"
LABEL org.opencontainers.image.vendor="Relate Mail"
LABEL org.opencontainers.image.source="https://github.com/four-robots/relate-mail"

EXPOSE 8080
