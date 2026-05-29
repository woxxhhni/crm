#!/bin/bash

# Configuration
CONTAINER_NAME="cls_postgres_dev"
if docker ps | grep -q "cls_postgres$"; then
    CONTAINER_NAME="cls_postgres"
fi

DB_USER=${POSTGRES_USER:-"postgres"}
DB_NAME=${POSTGRES_DB:-"Cls_Db"}
DUMP_FILE="dumps/app_prod_backup_20260224_143643.sql"

echo "Checking if dump file exists..."
if [ ! -f "$DUMP_FILE" ]; then
    echo "Error: Dump file $DUMP_FILE not found."
    exit 1
fi

echo "Checking if PostgreSQL container is running..."
if ! docker ps | grep -q "$CONTAINER_NAME"; then
    echo "Error: Database container '$CONTAINER_NAME' is not running."
    echo "Please ensure your containers are up (e.g., via docker-compose up) before running this script."
    exit 1
fi

echo "Stopping API (docker container if exists)..."
docker stop cls_api > /dev/null 2>&1 || true

echo "Stopping local native API (if running)..."
lsof -ti :40585 | xargs kill -9 2>/dev/null || true
lsof -ti :8080 | xargs kill -9 2>/dev/null || true

echo "Terminating existing connections to database '$DB_NAME'..."
docker exec -i $CONTAINER_NAME psql -U $DB_USER -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME';"

echo "Dropping database '$DB_NAME'..."
docker exec -i $CONTAINER_NAME psql -U $DB_USER -d postgres -c "DROP DATABASE IF EXISTS \"$DB_NAME\";"

echo "Creating fresh database '$DB_NAME'..."
docker exec -i $CONTAINER_NAME psql -U $DB_USER -d postgres -c "CREATE DATABASE \"$DB_NAME\";"

echo "Importing $DUMP_FILE into '$DB_NAME'..."
cat "$DUMP_FILE" | docker exec -i $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME

echo "Database replacement completed successfully!"
