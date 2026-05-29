#!/bin/bash
# Check if Docker dependencies are running
if ! docker ps | grep -q "cls_postgres_dev"; then
    echo "Starting Database and MinIO dependencies..."
    docker-compose -f docker-compose.dev.yml up -d
    echo "Waiting for services to be ready..."
    sleep 5
fi

# Kill any existing process on port 40585 to ensure a clean start
echo "Cleaning up port 40585..."
lsof -ti :40585 | xargs kill -9 2>/dev/null || true

# Cleanup function to kill the backend process on exit
cleanup() {
    echo ""
    echo "Stopping Cls API..."
    if [ -n "$API_PID" ]; then
        kill $API_PID 2>/dev/null
    fi
    exit
}

# Register the cleanup function for SIGINT (Ctrl+C) and SIGTERM
trap cleanup SIGINT SIGTERM

echo "Starting Cls API on http://localhost:40585 ..."

# Use 'dotnet run' which is the most stable and handles environment variables from launchSettings automatically.
# This prevents the 'Abort trap: 6' crashes seen with direct DLL execution.
dotnet run --project src/Cls.Api/Cls.Api.csproj --launch-profile "Dev" &
API_PID=$!

# Wait for the process to finish
wait $API_PID
