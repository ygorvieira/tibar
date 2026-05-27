#!/bin/sh
set -e

echo "Waiting for database at $DB_HOST:$DB_PORT..."
while ! nc -z "$DB_HOST" "$DB_PORT" 2>/dev/null; do
  sleep 1
done
echo "Database is reachable. Starting API..."

exec dotnet Tibar.API.dll
