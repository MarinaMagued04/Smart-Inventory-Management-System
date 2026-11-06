using System.Net.Http.Json;
using Dashboard.Models;

namespace Dashboard.Services;

public interface IInventoryClient
{
    Task<List<Product>> GetProductsAsync(CancellationToken ct = default);

    // Returns null if the product does not exist
    Task<Product?> GetProductAsync(int id, CancellationToken ct = default);

    // Stock movements of the last N days, oldest first.
    // Needs the product's current quantity to work out "before" and "after" for each movement.
    Task<List<StockMovement>> GetMovementsAsync(Product product, int days, CancellationToken ct = default);
}

public interface IAlertClient
{
    Task<List<Alert>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task AcknowledgeAsync(int alertId, CancellationToken ct = default);
}

// Calls the real Inventory service:
//   GET /api/products
//   GET /api/products/{id}
//   GET /api/products/{id}/transactions
public class InventoryClient(HttpClient http) : IInventoryClient
{
    public async Task<List<Product>> GetProductsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<Product>>("api/products", ct) ?? [];

    public async Task<Product?> GetProductAsync(int id, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"api/products/{id}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(ct);
    }

    public async Task<List<StockMovement>> GetMovementsAsync(Product product, int days, CancellationToken ct = default)
    {
        var transactions = await http.GetFromJsonAsync<List<InventoryTransaction>>(
            $"api/products/{product.Id}/transactions", ct) ?? [];

        return StockHistory.FromTransactions(product, transactions, DateTime.UtcNow.AddDays(-days));
    }
}

public static class StockHistory
{
    // The Inventory service stores only the type (IN/OUT) and amount of each transaction.
    // We know the quantity NOW, so walking back from the newest transaction
    // gives the quantity before and after every one of them.
    public static List<StockMovement> FromTransactions(
        Product product, IEnumerable<InventoryTransaction> transactions, DateTime fromUtc)
    {
        var movements = new List<StockMovement>();
        var after = product.Quantity;

        foreach (var t in transactions.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id))
        {
            var isIn = t.TransactionType.Equals("IN", StringComparison.OrdinalIgnoreCase);
            var change = isIn ? t.Quantity : -t.Quantity;
            var before = after - change;

            if (t.CreatedAt >= fromUtc)
            {
                movements.Add(new StockMovement
                {
                    Id = t.Id,
                    ProductId = t.ProductId,
                    Type = isIn ? "stock-in" : "stock-out",
                    Change = change,
                    QuantityBefore = before,
                    QuantityAfter = after,
                    CreatedAt = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc),
                });
            }

            after = before;
        }

        movements.Reverse(); // oldest first
        return movements;
    }
}

// Calls the real Alert service: GET /api/alerts?status=active, PATCH /api/alerts/{id}/ack
public class AlertClient(HttpClient http) : IAlertClient
{
    public async Task<List<Alert>> GetActiveAlertsAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<Alert>>("api/alerts?status=active", ct) ?? [];

    public async Task AcknowledgeAsync(int alertId, CancellationToken ct = default)
    {
        var response = await http.PatchAsync($"api/alerts/{alertId}/ack", content: null, ct);
        response.EnsureSuccessStatusCode();
    }
}
