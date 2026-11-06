namespace InventoryService.Models;

public class StockTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string TransactionType { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}