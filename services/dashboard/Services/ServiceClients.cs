using System.Net.Http.Json;
using Dashboard.Models;

namespace Dashboard.Services;

public interface IInventoryClient
{
    Task<List<Product>> GetProductsAsync(CancellationToken ct = default);

    // Returns null if the product does not exist
    Task<Product?> GetProductAsync(int id, CancellationToken ct = default);

    Task<List<StockMovement>> GetMovementsAsync(int productId, int days, CancellationToken ct = default);
}

public interface IAlertClient
{
    Task<List<Alert>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task AcknowledgeAsync(int alertId, CancellationToken ct = default);
}

// Calls the real Inventory service:
//   GET /api/products
//   GET /api/products/{id}
//   GET /api/products/{id}/movements?days=30
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

    public async Task<List<StockMovement>> GetMovementsAsync(int productId, int days, CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<StockMovement>>($"api/products/{productId}/movements?days={days}", ct) ?? [];
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
