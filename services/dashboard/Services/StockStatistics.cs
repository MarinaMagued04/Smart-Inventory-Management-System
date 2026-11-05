using Dashboard.Models;

namespace Dashboard.Services;

public record DailyStock(DateOnly Date, int EndOfDayQuantity, int UnitsOut, int UnitsIn);

public class StockReport
{
    public int Days { get; init; }
    public List<DailyStock> Daily { get; init; } = [];

    public int QuantityAtStart { get; init; }
    public int TotalOut { get; init; }
    public int TotalIn { get; init; }

    // Average units going out per day over the last 7 days (used for the forecast)
    public double AvgDailyOutLast7 { get; init; }

    // null = can't estimate (no consumption) or already out of stock
    public double? DaysUntilOutOfStock { get; init; }
    public DateOnly? EstimatedOutOfStockDate { get; init; }

    // When the product last dropped to/below its threshold, and when it hit zero
    public DateTime? BecameLowStockAt { get; init; }
    public DateTime? BecameOutOfStockAt { get; init; }

    public DailyStock? WorstDay => Daily.Where(d => d.UnitsOut > 0).MaxBy(d => d.UnitsOut);
}

public static class StockStatistics
{
    // Builds daily statistics from the raw movements (all dates are UTC).
    public static StockReport Build(Product product, IReadOnlyList<StockMovement> movements, int days, DateTime nowUtc)
    {
        var ordered = movements.OrderBy(m => m.CreatedAt).ToList();
        var today = DateOnly.FromDateTime(nowUtc);
        var firstDay = today.AddDays(-(days - 1));

        // Quantity at the start of the window = "before" of the first movement, or the current quantity if nothing moved
        var startQty = ordered.Count > 0 ? ordered[0].QuantityBefore : product.Quantity;

        var byDay = ordered.GroupBy(m => DateOnly.FromDateTime(m.CreatedAt)).ToDictionary(g => g.Key, g => g.ToList());

        var daily = new List<DailyStock>(days);
        var qty = startQty;
        for (var d = firstDay; d <= today; d = d.AddDays(1))
        {
            int unitsOut = 0, unitsIn = 0;
            if (byDay.TryGetValue(d, out var dayMoves))
            {
                unitsOut = dayMoves.Where(m => m.Change < 0).Sum(m => -m.Change);
                unitsIn = dayMoves.Where(m => m.Change > 0).Sum(m => m.Change);
                qty = dayMoves[^1].QuantityAfter;
            }
            daily.Add(new DailyStock(d, qty, unitsOut, unitsIn));
        }

        var last7 = daily.TakeLast(7).ToList();
        var avg7 = last7.Count == 0 ? 0 : last7.Sum(d => d.UnitsOut) / (double)last7.Count;

        double? daysLeft = null;
        DateOnly? outDate = null;
        if (product.Quantity > 0 && avg7 > 0)
        {
            daysLeft = product.Quantity / avg7;
            outDate = today.AddDays((int)Math.Floor(daysLeft.Value));
        }

        return new StockReport
        {
            Days = days,
            Daily = daily,
            QuantityAtStart = startQty,
            TotalOut = daily.Sum(d => d.UnitsOut),
            TotalIn = daily.Sum(d => d.UnitsIn),
            AvgDailyOutLast7 = avg7,
            DaysUntilOutOfStock = daysLeft,
            EstimatedOutOfStockDate = outDate,
            BecameLowStockAt = product.IsLowStock
                ? LastCrossing(ordered, m => m.QuantityBefore > product.MinThreshold && m.QuantityAfter <= product.MinThreshold)
                : null,
            BecameOutOfStockAt = product.Quantity == 0
                ? LastCrossing(ordered, m => m.QuantityBefore > 0 && m.QuantityAfter == 0)
                : null,
        };
    }

    private static DateTime? LastCrossing(List<StockMovement> ordered, Func<StockMovement, bool> crossed)
        => ordered.LastOrDefault(crossed)?.CreatedAt;
}
