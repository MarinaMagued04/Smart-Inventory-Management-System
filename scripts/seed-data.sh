#!/usr/bin/env bash
# Adds sample products + stock movements to the Inventory service.
# Run after: docker compose -f docker-compose.local.yml up -d --build
# Usage (Linux / WSL / macOS, from the repo root):  bash scripts/seed-data.sh

API=http://localhost:5001/api/products

add() {
  id=$(curl -s -X POST "$API" -H "Content-Type: application/json" \
    -d "{\"name\":\"$1\",\"sku\":\"$2\",\"description\":\"\",\"quantity\":0,\"minStockLevel\":$3,\"price\":$4}" \
    | sed -nE 's/.*"id":([0-9]+).*/\1/p')
  if [ -z "$id" ]; then echo "Skipped $1 (already exists?)"; return; fi
  curl -s -X POST "$API/$id/stock-in"  -H "Content-Type: application/json" -d "{\"quantity\":$5}" > /dev/null
  curl -s -X POST "$API/$id/stock-out" -H "Content-Type: application/json" -d "{\"quantity\":$6}" > /dev/null
  echo "Added $1: id $id"
}

add "Wireless Mouse"      ACC-014 10 12.5 50 45
add "USB-C Cable"         ACC-021 20 5    40 40
add "Monitor 27 inch"     MON-007 4  200  15 3
add "Mechanical Keyboard" ACC-033 8  60   20 14
add "Laptop Dell XPS 13"  LAP-001 5  1200 30 6
add "Webcam HD"           CAM-002 6  45   12 9
