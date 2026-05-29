#!/bin/sh

# Generate runtime environment config
# This script runs at container startup to inject env vars into the app

CONFIG_FILE="/app/public/__ENV.js"

# Create the runtime config file with current environment variables
# Using NEXT_PUBLIC_ prefix for consistency with Next.js conventions
cat <<EOF > $CONFIG_FILE
window.__ENV = {
  NEXT_PUBLIC_API_BASE_URL: "${NEXT_PUBLIC_API_BASE_URL:-}",
  NEXT_PUBLIC_TIMEZONE: "${NEXT_PUBLIC_TIMEZONE:-America/Toronto}",
  NEXT_PUBLIC_RECAPTCHA_SITE_KEY: "${NEXT_PUBLIC_RECAPTCHA_SITE_KEY:-}"
};
EOF

echo "Runtime config generated at $CONFIG_FILE"

# Start the application
exec node server.js
