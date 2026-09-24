# Placement Tracker — SINGLE container (Vue frontend + ASP.NET Core API + PostgreSQL 16 + NGINX).
#
# One image / one container serves the whole stack:
#   NGINX :8080  ->  /            serves the Vue production bundle (static files)
#                 ->  /api/...     proxies to the API on 127.0.0.1:5038
#   API           ->  PostgreSQL on 127.0.0.1:5432 (same version, same schema/logic)
#   Data persists in the existing `pgdata` volume at /var/lib/postgresql/data.
#
# Notes (Docker architecture only — no app code is changed):
# - Frontend is built with the same `npm ci && npm run build`; the resulting
#   dist/ bundle is served by NGINX instead of `vite preview` (identical UI).
# - Backend runs on 127.0.0.1:5038 internally (was :8080 in its own container).
#   This change is REQUIRED: two processes cannot share :8080 in one net
#   namespace. :5038 matches the README's local-dev API port. Public entry
#   stays http://localhost:8080.

# ---------- Stage 1: build the Vue frontend (unchanged build) ----------
FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ---------- Stage 2: publish the ASP.NET Core API (unchanged build) ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY backend/*.csproj ./
RUN dotnet restore
COPY backend/ ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- Stage 3: single runtime (dotnet + nginx + postgres 16) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app

ENV DEBIAN_FRONTEND=noninteractive \
    ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://127.0.0.1:5038 \
    PGDATA=/var/lib/postgresql/data

# NGINX + PostgreSQL 16 (via the official PGDG repo: bookworm ships PG15,
# and we keep PG16 to match the previous postgres:16 service exactly).
RUN apt-get update \
 && apt-get install -y --no-install-recommends ca-certificates gnupg wget \
 && wget -qO- https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/pgdg.gpg \
 && echo "deb [signed-by=/usr/share/keyrings/pgdg.gpg] https://apt.postgresql.org/pub/repos/apt bookworm-pgdg main" > /etc/apt/sources.list.d/pgdg.list \
 && apt-get update \
 && apt-get install -y --no-install-recommends nginx postgresql-16 \
 && sed -i 's/^# *en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen && locale-gen \
 && rm -f /etc/nginx/sites-enabled/default \
 && rm -rf /var/lib/apt/lists/* \
 && mkdir -p /var/run/postgresql /var/log/postgresql \
 && chown postgres:postgres /var/run/postgresql /var/log/postgresql

# Published API (from stage 2, unmodified code).
COPY --from=backend-build /app/publish/ /app/backend/
# Vue production bundle (from stage 1, unmodified code) served by NGINX.
COPY --from=frontend-build /app/dist/ /usr/share/nginx/html/
# NGINX single-container reverse proxy + static hosting.
COPY nginx/docker.conf /etc/nginx/conf.d/default.conf
# Entrypoint: init/starts postgres, starts the API, then runs NGINX.
COPY docker-entrypoint.sh /usr/local/bin/docker-entrypoint.sh
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

EXPOSE 8080
VOLUME /var/lib/postgresql/data
ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
