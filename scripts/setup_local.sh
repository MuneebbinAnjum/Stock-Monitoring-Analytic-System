#!/usr/bin/env bash
set -e
ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
cd "$ROOT_DIR"

echo "Starting docker-compose (dev)..."
docker-compose build

docker-compose up -d

echo "Waiting for Postgres to be ready..."
until docker exec $(docker-compose ps -q postgres) pg_isready -U smas >/dev/null 2>&1; do
  sleep 1
done

echo "Applying EF migrations (requires dotnet-ef)..."
if ! command -v dotnet-ef >/dev/null 2>&1; then
  echo "dotnet-ef not found. Install with: dotnet tool install --global dotnet-ef"
else
  dotnet ef database update -p SMAS.API -s SMAS.API
fi

echo "Frontend: install deps and start dev server (in separate terminal if desired)"
cd frontend
npm ci || true

echo "Setup complete. Frontend dev: 'npm run dev' in frontend/; API at http://localhost:5000"
