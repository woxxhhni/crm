#!/usr/bin/env bash
# Populate order #5 with every visible activity-log variant for UI testing.
# Requires API on :40585 and admin credentials.
set -euo pipefail

ORDER_ID="${1:-5}"
API="${API_BASE:-http://localhost:40585/api/v1}"

echo "→ Login..."
TOKEN=$(curl -sf -X POST "$API/auth/login" \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@cls.local","password":"Admin@2026!"}' | jq -r '.token')

auth() { echo "Authorization: Bearer $TOKEN"; }
now() { date -u +%Y-%m-%dT%H:%M:%SZ; }

echo "→ Order $ORDER_ID — complete current step (statuscomplete + statusforward)..."
curl -sf -X PATCH "$API/orders/set-step-complete" \
  -H "$(auth)" \
  -F "OrderId=$ORDER_ID" \
  -F "ActionDate=$(now)" \
  -F "Description=Purchase order confirmed and sent to factory" >/dev/null

sleep 1

echo "→ Step backward to previous step (statusbackward)..."
curl -sf -X PATCH "$API/orders/set-step" \
  -H "$(auth)" \
  -F "OrderId=$ORDER_ID" \
  -F "StepId=1" \
  -F "ActionDate=$(now)" \
  -F "Description=Returned to specifications — client requested changes" >/dev/null

echo "→ Activity note after step change..."
curl -sf -X POST "$API/orders/$ORDER_ID/notes" \
  -H "$(auth)" \
  -F "ActionDate=$(now)" \
  -F "Title=Client requested spec revision" \
  -F "Description=Client asked to revise product dimensions before PO can be finalized." >/dev/null

echo "→ Assign manager as second employee (employeeassigned)..."
curl -sf -X POST "$API/orders/$ORDER_ID/employees" \
  -H "$(auth)" -H 'Content-Type: application/json' \
  -d '{"userIds":[3]}' >/dev/null || true

echo "→ Suspend order (ordersuspended)..."
curl -sf -X PATCH "$API/orders/$ORDER_ID/suspend" \
  -H "$(auth)" \
  -F "ActionDate=$(now)" \
  -F "Description=Paused while waiting for client specification sign-off" >/dev/null

echo "→ Unsuspend order (orderreturnedtoprogress)..."
curl -sf -X PATCH "$API/orders/$ORDER_ID/unsuspend" \
  -H "$(auth)" \
  -F "ActionDate=$(now)" \
  -F "Description=Client approved revised specifications — resuming work" >/dev/null

echo "→ Complete step 1 again..."
curl -sf -X PATCH "$API/orders/set-step-complete" \
  -H "$(auth)" \
  -F "OrderId=$ORDER_ID" \
  -F "ActionDate=$(now)" \
  -F "Description=Revised specifications approved" >/dev/null

echo "→ Jump forward to step 3 (statusforward)..."
curl -sf -X PATCH "$API/orders/set-step" \
  -H "$(auth)" \
  -F "OrderId=$ORDER_ID" \
  -F "StepId=3" \
  -F "ActionDate=$(now)" \
  -F "Description=Skipped PO sending — PO already handled offline" >/dev/null

echo "→ Note on negotiation step..."
curl -sf -X POST "$API/orders/$ORDER_ID/notes" \
  -H "$(auth)" \
  -F "ActionDate=$(now)" \
  -F "Title=Price negotiation started" \
  -F "Description=Negotiating unit price and lead time with factory." >/dev/null

echo "→ Summary:"
curl -sf "$API/orders/$ORDER_ID" -H "$(auth)" | jq -r '
  "Status: \(.status) | Step: \(.currentStepName)",
  "Log types:",
  (.steps[]?.logs[]?.logType) as $t | $t
' | sort -u

echo ""
echo "✓ Open http://localhost:3001/panel/orders/details/$ORDER_ID"
