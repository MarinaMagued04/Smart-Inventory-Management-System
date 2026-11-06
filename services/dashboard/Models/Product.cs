using System.Text.Json.Serialization;

namespace Dashboard.Models;

// Matches the Inventory service's Product JSON (GET /api/products).
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }

    // The Inventory service calls this field "minStockLevel"
    [JsonPropertyName("minStockLevel")]
    public int MinThreshold { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsLowStock => Quantity <= MinThreshold;
}
