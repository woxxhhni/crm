#!/usr/bin/env bash
# Advance order #5 through multiple workflow stages with notes at each stage.
# Run after seed-order-5-activities.sh or on an in-progress order on step 3+.
set -euo pipefail

ORDER_ID="${1:-5}"
API="${API_BASE:-http://localhost:40585/api/v1}"

echo "→ Login..."
TOKEN=$(curl -sf -X POST "$API/auth/login" \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@cls.local","password":"Admin@2026!"}' | jq -r '.token')

auth() { echo "Authorization: Bearer $TOKEN"; }
now() { date -u +%Y-%m-%dT%H:%M:%SZ; }

complete_step() {
  local desc="$1"
  echo "  · complete step: $desc"
  curl -sf -X PATCH "$API/orders/set-step-complete" \
    -H "$(auth)" \
    -F "OrderId=$ORDER_ID" \
    -F "ActionDate=$(now)" \
    -F "Description=$desc" >/dev/null
  sleep 0.4
}

add_note() {
  local title="$1"
  local desc="$2"
  echo "  · note: $title"
  curl -sf -X POST "$API/orders/$ORDER_ID/notes" \
    -H "$(auth)" \
    -F "ActionDate=$(now)" \
    -F "Title=$title" \
    -F "Description=$desc" >/dev/null
  sleep 0.3
}

CURRENT=$(curl -sf "$API/orders/$ORDER_ID" -H "$(auth)" | jq -r '.currentStepId')
echo "→ Order $ORDER_ID currently on step $CURRENT"

echo "→ Stage 1 — finish Order Placement (steps 3–4)..."
if [ "$CURRENT" -le 3 ]; then
  complete_step "Price and lead time agreed with factory"
fi
if [ "$CURRENT" -le 4 ]; then
  add_note "Factory order confirmation received" "Factory confirmed MOQ, unit price, and 45-day lead time."
  complete_step "Factory order confirmation received — Order Placement complete"
fi

echo "→ Stage 2 — Pre-Production (steps 5–8)..."
add_note "Pre-production sample approved" "Client signed off on pre-production sample quality and colour."
complete_step "Pre-production sample approved by client"

add_note "Raw materials confirmed" "Factory confirmed fabric stock and colour swatches match approved sample."
complete_step "Raw materials and colour swatches confirmed"

add_note "Production timeline received" "Factory provided 6-week production schedule starting next Monday."
complete_step "Production timeline received from factory"

add_note "Initial deposit paid" "30% deposit transferred — Pre-Production stage complete."
complete_step "Initial deposit payment processed"

echo "→ Stage 3 — Manufacturing (steps 9–12)..."
add_note "Raw materials procured" "Factory confirmed all raw materials ordered and inbound."
complete_step "Raw material procurement complete"

add_note "Initial production started" "Cutting and initial assembly line started at factory."
complete_step "Initial production stage underway"

add_note "Mid-production update" "50% production complete — inline QC checks passed."
complete_step "Mid-production milestone reached"

add_note "Mid-production inspection passed" "Third-party inline inspection completed with no major defects."
complete_step "Mid-production inspection passed"

echo "→ Stage 4 — Post-Production (steps 17–18)..."
add_note "Production completion notice" "Factory notified production is complete and goods ready for packing."
curl -sf -X PATCH "$API/orders/set-step" \
  -H "$(auth)" \
  -F "OrderId=$ORDER_ID" \
  -F "StepId=17" \
  -F "ActionDate=$(now)" \
  -F "Description=Production finished — moving to post-production" >/dev/null

complete_step "Production completion notice received"

add_note "Shipment logistics arranged" "Freight forwarder booked for sea freight to Montreal."
complete_step "Shipment logistics arranged"

echo ""
echo "=== Result ==="
curl -sf "$API/orders/$ORDER_ID" -H "$(auth)" | jq -r '
  "Status: \(.status)",
  "Current: \(.currentStageName) › \(.currentStepName)",
  "Step sections: \(.steps | length)",
  (.steps | group_by(.stageName) | map("\n\(.[0].stageName) (\(length) steps)") | .[])
'
echo ""
echo "✓ http://localhost:3001/panel/orders/details/$ORDER_ID"
