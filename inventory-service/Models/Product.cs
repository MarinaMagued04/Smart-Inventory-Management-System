namespace InventoryService.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int MinStockLevel { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}