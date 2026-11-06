using AlertService.Models;

namespace AlertService.Services;

public class AlertService
{
    private readonly List<Alert> _alerts = new()
    {
        new Alert
        {
            Id = 1,
            ProductId = 101,
            ProductName = "Wireless Mouse",
            Quantity = 3,
            Threshold = 10,
            CreatedAt = DateTime.UtcNow,
            Status = "active"
        },
        new Alert
        {
            Id = 2,
            ProductId = 102,
            ProductName = "USB-C Cable",
            Quantity = 0,
            Threshold = 20,
            CreatedAt = DateTime.UtcNow,
            Status = "active"
        }
    };

    public List<Alert> GetAlerts(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return _alerts.ToList();

        return _alerts
            .Where(a => a.Status.Equals(
                status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Alert? GetAlertById(int id)
    {
        return _alerts.FirstOrDefault(a => a.Id == id);
    }

    public bool AcknowledgeAlert(int id)
    {
        var alert = GetAlertById(id);

        if (alert is null)
            return false;

        alert.Status = "acknowledged";
        return true;
    }
}