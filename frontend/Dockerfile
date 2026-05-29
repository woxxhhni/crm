# syntax=docker/dockerfile:1.4
FROM node:20-alpine AS base

# --- Builder Stage ---
FROM base AS builder
WORKDIR /app

# Disable telemetry to slightly speed up build
ENV NEXT_TELEMETRY_DISABLED=1
# Limit Next.js build memory to prevent crashes on limited resource VMs. Adjust if necessary.
ENV NODE_OPTIONS="--max-old-space-size=1024"

# 1) Cache npm registry requests
# By using buildkit cache mounts, we keep the download cache across multiple build executions.
COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm \
    npm ci --ignore-scripts --unsafe-perm --legacy-peer-dep

# Copy all source files
COPY . .

# 2) Cache Next.js build outputs
# This is a huge optimization for rebuild speeds. The .next/cache directory will be preserved across builds.
RUN --mount=type=cache,target=/app/.next/cache \
    npm run build


# --- Runner Stage (Production) ---
FROM base AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# Set permissions for entrypoint script
COPY entrypoint.sh ./entrypoint.sh
RUN chmod +x ./entrypoint.sh

# Leverage output traces to reduce image size
COPY --from=builder /app/public ./public
# standalone directory gets generated because 'output: standalone' is in next.config.ts
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000

ENTRYPOINT ["./entrypoint.sh"]