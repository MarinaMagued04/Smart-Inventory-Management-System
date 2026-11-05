namespace Dashboard.Models;

// One change in a product's quantity, recorded by the Inventory service.
public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    // "sale", "restock" or "adjustment"
    public string Type { get; set; } = string.Empty;

    // Signed change: negative = stock went down, positive = stock went up
    public int Change { get; set; }

    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public DateTime CreatedAt { get; set; }
}
