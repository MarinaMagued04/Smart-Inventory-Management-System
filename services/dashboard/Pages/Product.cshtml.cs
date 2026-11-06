using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dashboard.Pages;

public class ProductModel(IInventoryClient inventory, ILogger<ProductModel> logger) : PageModel
{
    public const int PeriodDays = 30;

    public Product? Product { get; private set; }
    public List<StockMovement> Movements { get; private set; } = [];
    public StockReport? Report { get; private set; }
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            // Load the product first: if it doesn't exist, return 404
            // instead of failing on the movements request.
            Product = await inventory.GetProductAsync(id, ct);
            if (Product is null) return NotFound();

            Movements = (await inventory.GetMovementsAsync(Product, PeriodDays, ct))
                .OrderByDescending(m => m.CreatedAt)
                .ToList();
            Report = StockStatistics.Build(Product, Movements, PeriodDays, DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Could not load product {ProductId}", id);
            Error = "Inventory service is unavailable right now.";
        }

        return Page();
    }
}
