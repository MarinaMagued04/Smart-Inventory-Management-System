# Dashboard Service

Web dashboard for managers: stock overview, low-stock products, and active alerts.
Built with ASP.NET Core 8 (Razor Pages).

## Endpoints

| Path        | Description                         |
|-------------|-------------------------------------|
| `/`         | Overview: KPIs + products table     |
| `/Alerts`   | Active alerts + Acknowledge button  |
| `/Products/{id}` | Product details: 30-day stock history, daily usage charts, out-of-stock forecast |
| `/health`   | Health check (for Docker / k8s)     |
| `/metrics`  | Prometheus metrics                  |

## Configuration (environment variables)

| Variable                  | Example                  | Notes                                  |
|---------------------------|--------------------------|----------------------------------------|
| `Services__InventoryUrl`  | `http://inventory:8080`  | Inventory service base URL             |
| `Services__AlertUrl`      | `http://alert:8080`      | Alert service base URL                 |
| `Services__UseMockData`   | `true`                   | Use fake data, no other services needed|

## APIs this service depends on

Inventory service:
- `GET /api/products` → `[{ id, name, sku, quantity, minThreshold, updatedAt }]`
- `GET /api/products/{id}` → `{ id, name, sku, quantity, minThreshold, updatedAt }` (404 if not found)
- `GET /api/products/{id}/movements?days=30` → stock movements in the last N days:
  ```json
  [
    {
      "id": 1,
      "productId": 2,
      "type": "sale",            // "sale" | "restock" | "adjustment"
      "change": -5,              // negative = out, positive = in
      "quantityBefore": 8,
      "quantityAfter": 3,
      "createdAt": "2026-11-04T10:30:00Z"   // UTC
    }
  ]
  ```
  The Inventory service must save one movement row every time a product's quantity changes.

Alert service:
- `GET /api/alerts?status=active` → `[{ id, productId, productName, quantity, threshold, createdAt, status }]`
- `PATCH /api/alerts/{id}/ack`

## Run with Docker (alone, mock data)

```bash
docker build -t dashboard .
docker run -p 5000:8080 -e Services__UseMockData=true dashboard
```

Open http://localhost:5000

## Run locally without Docker

```bash
dotnet run --environment Development   # mock data is ON in Development
```
