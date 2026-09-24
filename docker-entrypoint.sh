#!/bin/bash
# Placement Tracker — single-container entrypoint.
# Starts PostgreSQL 16, the ASP.NET Core API, then NGINX (foreground, PID 1).
# Database behavior is unchanged: same PG16, same DB/user/password env vars,
# same data-dir path (/var/lib/postgresql/data, persisted via the pgdata volume).
set -e

PGDATA="${PGDATA:-/var/lib/postgresql/data}"
PGBIN="/usr/lib/postgresql/16/bin"
PGUSER="${POSTGRES_USER:-postgres}"
PGPASSWORD="${POSTGRES_PASSWORD:-postgres}"
PGDB="${POSTGRES_DB:-placement_tracker}"
BACKEND_DIR="/app/backend"

mkdir -p "$PGDATA" /var/run/postgresql /var/log/postgresql
chown -R postgres:postgres "$PGDATA" /var/run/postgresql /var/log/postgresql
chmod 700 "$PGDATA"

# --- Init the cluster on first start (fresh volume) ---
if [ ! -f "$PGDATA/PG_VERSION" ]; then
  echo "[entrypoint] Initializing PostgreSQL 16 cluster in $PGDATA ..."
  runuser -u postgres -- "$PGBIN/initdb" -D "$PGDATA" -U "$PGUSER" \
    --auth-local=trust --auth-host=scram-sha-256
fi

# --- Start PostgreSQL (as the postgres OS user) ---
# A stale postmaster.pid can be left behind if a previous container was
# killed (docker rm -f); no other server can exist in a fresh container.
rm -f "$PGDATA/postmaster.pid"
echo "[entrypoint] Starting PostgreSQL ..."
runuser -u postgres -- "$PGBIN/pg_ctl" -D "$PGDATA" \
  -l /var/log/postgresql/postgresql.log -w -t 60 start

# --- Ensure role password + database exist (idempotent, safe on reuse) ---
runuser -u postgres -- psql -d postgres -v ON_ERROR_STOP=1 \
  -c "ALTER USER \"$PGUSER\" WITH PASSWORD '$PGPASSWORD';"
if ! runuser -u postgres -- psql -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$PGDB'" | grep -q 1; then
  echo "[entrypoint] Creating database $PGDB ..."
  runuser -u postgres -- psql -d postgres -v ON_ERROR_STOP=1 \
    -c "CREATE DATABASE \"$PGDB\" OWNER \"$PGUSER\";"
fi

# --- Wait until PostgreSQL accepts connections ---
echo "[entrypoint] Waiting for PostgreSQL ..."
until runuser -u postgres -- "$PGBIN/pg_isready" -h 127.0.0.1 -p 5432 -U "$PGUSER" > /dev/null 2>&1; do
  sleep 1
done
echo "[entrypoint] PostgreSQL is ready."

# --- Start the ASP.NET Core API (same app, internal port :5038) ---
export ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
export ASPNETCORE_URLS="http://127.0.0.1:5038"
export ConnectionStrings__Default="Host=127.0.0.1;Port=5432;Database=$PGDB;Username=$PGUSER;Password=$PGPASSWORD"
echo "[entrypoint] Starting API (dotnet $BACKEND_DIR/PlacementTracker.Api.dll on 127.0.0.1:5038) ..."
dotnet "$BACKEND_DIR/PlacementTracker.Api.dll" &
BACKEND_PID=$!

# --- Graceful shutdown: stop API + postgres on SIGTERM/SIGINT ---
shutdown() {
  echo "[entrypoint] Shutting down ..."
  kill "$BACKEND_PID" 2>/dev/null || true
  runuser -u postgres -- "$PGBIN/pg_ctl" -D "$PGDATA" stop -m fast 2>/dev/null || true
  exit 0
}
trap shutdown TERM INT

# --- NGINX in the foreground (single public entry point :8080) ---
echo "[entrypoint] Starting NGINX on :8080 ..."
exec nginx -g "daemon off;"
