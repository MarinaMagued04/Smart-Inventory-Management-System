namespace Dashboard.Models;

// One change in a product's quantity, as the dashboard uses it.
// Built from the Inventory service's transactions (see InventoryTransaction).
public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    // "stock-in" or "stock-out"
    public string Type { get; set; } = string.Empty;

    // Signed change: negative = stock went down, positive = stock went up
    public int Change { get; set; }

    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Raw transaction JSON from the Inventory service (GET /api/products/{id}/transactions).
public class InventoryTransaction
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    // "IN" or "OUT"
    public string TransactionType { get; set; } = string.Empty;

    // Always positive; the direction comes from TransactionType
    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; }
}
