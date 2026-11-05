namespace Dashboard.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinThreshold { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsLowStock => Quantity <= MinThreshold;
}
