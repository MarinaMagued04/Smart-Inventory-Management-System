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

Inventory service (Ouda's `inventory-service`):
- `GET /api/products` → `[{ id, name, sku, quantity, minStockLevel, createdAt, ... }]`
- `GET /api/products/{id}` → one product (404 if not found)
- `GET /api/products/{id}/transactions` → `[{ id, productId, transactionType, quantity, createdAt }]`
  - `transactionType` is `"IN"` or `"OUT"`, `quantity` is always positive.
  - The dashboard works out the quantity before and after each transaction itself,
    starting from the product's current quantity and walking back.
  - It shows the last 30 days.

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
