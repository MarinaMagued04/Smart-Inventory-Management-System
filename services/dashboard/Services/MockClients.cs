using Dashboard.Models;

namespace Dashboard.Services;

// Fake data used when Services:UseMockData = true,
// so the dashboard can be tested before the Inventory and Alert services are ready.
public class MockInventoryClient : IInventoryClient
{
    public Task<List<Product>> GetProductsAsync(CancellationToken ct = default) =>
        Task.FromResult(MockStore.Products.ToList());

    public Task<Product?> GetProductAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(MockStore.Products.FirstOrDefault(p => p.Id == id));

    public Task<List<StockMovement>> GetMovementsAsync(int productId, int days, CancellationToken ct = default)
    {
        var from = DateTime.UtcNow.AddDays(-days);
        var list = MockStore.Movements.TryGetValue(productId, out var all)
            ? all.Where(m => m.CreatedAt >= from).ToList()
            : [];
        return Task.FromResult(list);
    }
}

public class MockAlertClient : IAlertClient
{
    public Task<List<Alert>> GetActiveAlertsAsync(CancellationToken ct = default)
    {
        lock (MockStore.Lock)
            return Task.FromResult(MockStore.Alerts.Where(a => a.Status == "active").ToList());
    }

    public Task AcknowledgeAsync(int alertId, CancellationToken ct = default)
    {
        lock (MockStore.Lock)
        {
            var alert = MockStore.Alerts.FirstOrDefault(a => a.Id == alertId);
            if (alert is not null) alert.Status = "acknowledged";
        }
        return Task.CompletedTask;
    }
}

internal static class MockHistory
{
    // Builds the history backwards from today: we know the quantity NOW,
    // so each older movement's "before" value is computed from its "after" value.
    public static List<StockMovement> Generate(
        Product product, int dailyMin, int dailyMax, int restockDaysAgo, int restockAmount, int outOfStockDays)
    {
        var rnd = new Random(product.Id * 7919); // fixed seed = same data on every run
        var now = DateTime.UtcNow;
        var movements = new List<StockMovement>();
        var after = product.Quantity;

        for (var daysAgo = 0; daysAgo < 30; daysAgo++)
        {
            if (daysAgo < outOfStockDays) continue; // no movements while sitting at zero

            var day = now.Date.AddDays(-daysAgo);
            var events = new List<(DateTime At, string Type, int Change)>();

            var sold = rnd.Next(dailyMin, dailyMax + 1);
            if (sold > 0)
            {
                // Split the day's sales into 1-2 transactions
                var first = sold > 1 && rnd.Next(2) == 0 ? rnd.Next(1, sold) : sold;
                events.Add((day.AddHours(rnd.Next(9, 14)).AddMinutes(rnd.Next(60)), "sale", -first));
                if (sold - first > 0)
                    events.Add((day.AddHours(rnd.Next(14, 20)).AddMinutes(rnd.Next(60)), "sale", -(sold - first)));
            }

            if (daysAgo == restockDaysAgo)
                events.Add((day.AddHours(8), "restock", restockAmount));

            // Walk the day's events from latest to earliest
            foreach (var e in events.Where(e => e.At <= now).OrderByDescending(e => e.At))
            {
                var change = e.Change;
                if (change > 0 && change > after) change = after; // can't restock more than we have now
                if (change == 0) continue;

                var before = after - change;
                movements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    Type = e.Type,
                    Change = change,
                    QuantityBefore = before,
                    QuantityAfter = after,
                    CreatedAt = e.At,
                });
                after = before;
            }
        }

        movements.Reverse(); // oldest first
        for (var i = 0; i < movements.Count; i++) movements[i].Id = product.Id * 1000 + i + 1;
        return movements;
    }
}

internal static class MockStore
{
    public static readonly object Lock = new();

    // Keep the fake data consistent with the generated history:
    // product "last updated" = last movement, alert "created" = when it crossed the threshold.
    static MockStore()
    {
        foreach (var p in Products)
        {
            var moves = Movements[p.Id];
            if (moves.Count > 0) p.UpdatedAt = moves[^1].CreatedAt;
        }

        foreach (var a in Alerts)
        {
            var crossing = Movements[a.ProductId].LastOrDefault(m => m.QuantityBefore > a.Threshold && m.QuantityAfter <= a.Threshold);
            if (crossing is not null) a.CreatedAt = crossing.CreatedAt;
        }
    }

    public static readonly List<Product> Products =
    [
        new() { Id = 1, Name = "Laptop Dell XPS 13", Sku = "LAP-001", Quantity = 25, MinThreshold = 5,  UpdatedAt = DateTime.UtcNow.AddHours(-2) },
        new() { Id = 2, Name = "Wireless Mouse",     Sku = "ACC-014", Quantity = 3,  MinThreshold = 10, UpdatedAt = DateTime.UtcNow.AddMinutes(-30) },
        new() { Id = 3, Name = "USB-C Cable 1m",     Sku = "ACC-021", Quantity = 0,  MinThreshold = 20, UpdatedAt = DateTime.UtcNow.AddMinutes(-5) },
        new() { Id = 4, Name = "Monitor 27 inch",    Sku = "MON-007", Quantity = 12, MinThreshold = 4,  UpdatedAt = DateTime.UtcNow.AddDays(-1) },
        new() { Id = 5, Name = "Mechanical Keyboard",Sku = "ACC-033", Quantity = 7,  MinThreshold = 8,  UpdatedAt = DateTime.UtcNow.AddHours(-6) },
    ];

    // 30 days of fake stock history per product, ending at the product's current quantity.
    public static readonly Dictionary<int, List<StockMovement>> Movements = Products.ToDictionary(
        p => p.Id,
        p => p.Id switch
        {
            1 => MockHistory.Generate(p, dailyMin: 0, dailyMax: 2, restockDaysAgo: 12, restockAmount: 20, outOfStockDays: 0),
            2 => MockHistory.Generate(p, dailyMin: 1, dailyMax: 4, restockDaysAgo: 20, restockAmount: 40, outOfStockDays: 0),
            3 => MockHistory.Generate(p, dailyMin: 3, dailyMax: 7, restockDaysAgo: 16, restockAmount: 60, outOfStockDays: 2),
            4 => MockHistory.Generate(p, dailyMin: 0, dailyMax: 1, restockDaysAgo: 25, restockAmount: 10, outOfStockDays: 0),
            _ => MockHistory.Generate(p, dailyMin: 0, dailyMax: 3, restockDaysAgo: 18, restockAmount: 25, outOfStockDays: 0),
        });

    public static readonly List<Alert> Alerts =
    [
        new() { Id = 101, ProductId = 2, ProductName = "Wireless Mouse",      Quantity = 3, Threshold = 10, CreatedAt = DateTime.UtcNow.AddMinutes(-30) },
        new() { Id = 102, ProductId = 3, ProductName = "USB-C Cable 1m",      Quantity = 0, Threshold = 20, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
        new() { Id = 103, ProductId = 5, ProductName = "Mechanical Keyboard", Quantity = 7, Threshold = 8,  CreatedAt = DateTime.UtcNow.AddHours(-6) },
    ];
}
