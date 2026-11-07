# Adds sample products + stock movements to the Inventory service.
# Run after: docker compose -f docker-compose.local.yml up -d --build
# Usage (PowerShell, from the repo root):  .\scripts\seed-data.ps1

$api = "http://localhost:5001/api/products"
$products = @(
  @{ name="Wireless Mouse";      sku="ACC-014"; minStockLevel=10; price=12.5; in=50; out=45 },
  @{ name="USB-C Cable";         sku="ACC-021"; minStockLevel=20; price=5;    in=40; out=40 },
  @{ name="Monitor 27 inch";     sku="MON-007"; minStockLevel=4;  price=200;  in=15; out=3  },
  @{ name="Mechanical Keyboard"; sku="ACC-033"; minStockLevel=8;  price=60;   in=20; out=14 },
  @{ name="Laptop Dell XPS 13";  sku="LAP-001"; minStockLevel=5;  price=1200; in=30; out=6  },
  @{ name="Webcam HD";           sku="CAM-002"; minStockLevel=6;  price=45;   in=12; out=9  }
)

foreach ($p in $products) {
  try {
    $body = @{ name=$p.name; sku=$p.sku; description=""; quantity=0; minStockLevel=$p.minStockLevel; price=$p.price } | ConvertTo-Json
    $created = Invoke-RestMethod -Method Post -Uri $api -ContentType "application/json" -Body $body
    Invoke-RestMethod -Method Post -Uri "$api/$($created.id)/stock-in"  -ContentType "application/json" -Body (@{quantity=$p.in}  | ConvertTo-Json) | Out-Null
    Invoke-RestMethod -Method Post -Uri "$api/$($created.id)/stock-out" -ContentType "application/json" -Body (@{quantity=$p.out} | ConvertTo-Json) | Out-Null
    "Added $($p.name): id $($created.id)"
  } catch {
    "Skipped $($p.name): $($_.ErrorDetails.Message)"
  }
}
