#!/usr/bin/env bash
set -e
ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
cd "$ROOT_DIR"

echo "Building production images and starting with docker-compose.prod.yml..."

docker-compose -f docker-compose.prod.yml build

docker-compose -f docker-compose.prod.yml up -d

echo "Production stack started. Frontend available at http://localhost:80, API at configured host and port."
