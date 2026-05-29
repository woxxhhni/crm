#!/usr/bin/env bash
# Seeds a demo order with multi-step tracking history for local development.
# Requires: backend running on :40585, Postgres, jq, curl
set -euo pipefail

API="${API_BASE:-http://localhost:40585/api/v1}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@cls.local}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-Admin@2026!}"

echo "→ Logging in as admin..."
TOKEN=$(curl -sf -X POST "$API/auth/login" \
  -H 'Content-Type: application/json' \
  -d "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}" \
  | jq -r '.token')

auth() { echo "Authorization: Bearer $TOKEN"; }

echo "→ Creating client..."
CLIENT_ID=$(curl -sf -X POST "$API/clients" \
  -H "$(auth)" \
  -F "Name=Demo Tracking Client" \
  -F "IsActive=true" \
  | jq -r '.id')

echo "→ Creating provider..."
PROVIDER_ID=$(curl -sf -X POST "$API/providers" \
  -H "$(auth)" \
  -F "Name=Demo Tracking Provider" \
  -F "IsActive=true" \
  | jq -r '.id')

echo "→ Creating order..."
ORDER_JSON=$(curl -sf -X POST "$API/orders" \
  -H "$(auth)" \
  -F "Title=Demo Tracking Order" \
  -F "OrderDate=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
  -F "Description=Order with step history for tracking UI demo" \
  -F "BuyCurrency=USD" \
  -F "BuyAmount=1000" \
  -F "SellCurrency=EUR" \
  -F "SellAmount=900" \
  -F "ClientId=$CLIENT_ID" \
  -F "ProviderId=$PROVIDER_ID" \
  -F "employees.UserIds[0]=3")

ORDER_ID=$(echo "$ORDER_JSON" | jq -r '.id')
echo "   Order ID: $ORDER_ID"

echo "→ Adding note on step 1..."
curl -sf -X POST "$API/orders/$ORDER_ID/notes" \
  -H "$(auth)" \
  -F "ActionDate=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
  -F "Title=Kickoff note" \
  -F "Description=Specifications confirmed with client. Waiting for factory response." \
  -o /dev/null

echo "→ Completing step 1..."
curl -sf -X PATCH "$API/orders/set-step-complete" \
  -H "$(auth)" \
  -F "OrderId=$ORDER_ID" \
  -F "ActionDate=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
  -F "Description=Product specifications confirmed" \
  -o /dev/null

echo "→ Adding note on step 2..."
curl -sf -X POST "$API/orders/$ORDER_ID/notes" \
  -H "$(auth)" \
  -F "ActionDate=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
  -F "Title=PO sent" \
  -F "Description=Purchase order sent to factory. Awaiting confirmation." \
  -o /dev/null

echo ""
echo "✓ Demo order ready: http://localhost:3001/panel/orders/details/$ORDER_ID"
echo "  API detail: $API/orders/$ORDER_ID"
