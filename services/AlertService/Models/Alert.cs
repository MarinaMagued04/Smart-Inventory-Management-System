namespace AlertService.Models;

public class Alert
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int Threshold { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = "active";
}