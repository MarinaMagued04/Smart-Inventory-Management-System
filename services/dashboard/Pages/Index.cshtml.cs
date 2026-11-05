using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dashboard.Pages;

public class IndexModel(IInventoryClient inventory, IAlertClient alerts, ILogger<IndexModel> logger) : PageModel
{
    public List<Product> Products { get; private set; } = [];
    public List<Alert> ActiveAlerts { get; private set; } = [];

    public string? InventoryError { get; private set; }
    public string? AlertError { get; private set; }

    public int TotalProducts => Products.Count;
    public int TotalUnits => Products.Sum(p => p.Quantity);
    public int LowStockCount => Products.Count(p => p.IsLowStock);
    public int OutOfStockCount => Products.Count(p => p.Quantity == 0);

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Call both services in parallel. If one is down, the page still loads and shows a warning.
        var productsTask = LoadProductsAsync(ct);
        var alertsTask = LoadAlertsAsync(ct);
        await Task.WhenAll(productsTask, alertsTask);
    }

    // Acknowledge button in the "Active Alerts" section of the Overview page
    public async Task<IActionResult> OnPostAcknowledgeAsync(int id, CancellationToken ct)
    {
        try
        {
            await alerts.AcknowledgeAsync(id, ct);
            Message = $"Alert #{id} acknowledged.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Failed to acknowledge alert {AlertId}", id);
            Message = $"Could not acknowledge alert #{id}. Please try again.";
        }

        // Post/Redirect/Get: avoids re-submitting the form on browser refresh
        return RedirectToPage();
    }

    private async Task LoadProductsAsync(CancellationToken ct)
    {
        try
        {
            Products = (await inventory.GetProductsAsync(ct))
                .OrderBy(p => p.IsLowStock ? 0 : 1)
                .ThenBy(p => p.Name)
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Inventory service is unavailable");
            InventoryError = "Inventory service is unavailable right now.";
        }
    }

    private async Task LoadAlertsAsync(CancellationToken ct)
    {
        try
        {
            ActiveAlerts = await alerts.GetActiveAlertsAsync(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Alert service is unavailable");
            AlertError = "Alert service is unavailable right now.";
        }
    }
}
