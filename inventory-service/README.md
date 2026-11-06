# Smart Inventory Management: Inventory Service

A REST API for managing products and stock levels, built with
ASP.NET Core and PostgreSQL.

## Technologies

- C# / ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- PostgreSQL 17
- Swagger
- Docker

## Features

- Create, read, update and delete products
- Stock-in and stock-out operations
- Prevents negative stock
- Records every stock movement (transaction history)
- Low-stock detection
- Health check endpoint

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/products | List all products |
| POST | /api/products | Create a product |
| GET | /api/products/{id} | Get one product |
| PUT | /api/products/{id} | Update a product |
| DELETE | /api/products/{id} | Delete a product |
| GET | /api/products/low-stock | Products at or below minimum stock |
| POST | /api/products/{id}/stock-in | Add stock |
| POST | /api/products/{id}/stock-out | Remove stock |
| GET | /api/products/{id}/transactions | Stock movement history |
| GET | /health | Health check |

Stock request body: `{ "quantity": 5 }`

## Run locally

1. Install the .NET 10 SDK and Docker.
2. Start PostgreSQL:

   ```bash
   docker run --name inventory-db \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=postgres \
     -e POSTGRES_DB=inventorydb \
     -p 5432:5432 -d postgres:17
   ```

3. Apply the database migrations:

   ```bash
   dotnet ef database update
   ```

4. Start the API:

   ```bash
   dotnet run --urls http://localhost:5000
   ```

Swagger: http://localhost:5000/swagger

## Run with Docker

```bash
docker build -t inventory-service .

docker run -d --name inventory-api -p 8080:8080 \
  -e ConnectionStrings__InventoryDb="Host=YOUR_DB_HOST;Port=5432;Database=inventorydb;Username=postgres;Password=YOUR_PASSWORD" \
  inventory-service
```

Inside Docker Compose, `YOUR_DB_HOST` is the database service name, not `localhost`.
The database must be migrated before the API can serve requests.

## Integration details

| Property | Value |
|----------|-------|
| Internal port | 8080 |
| API prefix | /api/products |
| Health check | /health |
| Swagger | /swagger |