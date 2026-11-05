using Dashboard.Models;
using Dashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dashboard.Pages;

public class AlertsModel(IAlertClient alerts, ILogger<AlertsModel> logger) : PageModel
{
    public List<Alert> ActiveAlerts { get; private set; } = [];
    public string? Error { get; private set; }

    [TempData]
    public string? Message { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            ActiveAlerts = (await alerts.GetActiveAlertsAsync(ct))
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Alert service is unavailable");
            Error = "Alert service is unavailable right now.";
        }
    }

    public async Task<IActionResult> OnPostAcknowledgeAsync(int id, CancellationToken ct)
    {
        try
        {
            await alerts.AcknowledgeAsync(id, ct);
            Message = $"Alert #{id} acknowledged.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Failed to acknowledge alert {AlertId}", id);
            Message = $"Could not acknowledge alert #{id}. Please try again.";
        }

        // Post/Redirect/Get: avoids re-submitting the form on browser refresh
        return RedirectToPage();
    }
}
