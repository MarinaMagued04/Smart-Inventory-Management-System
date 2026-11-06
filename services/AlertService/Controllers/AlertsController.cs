using AlertService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlertService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly AlertService.Services.AlertService _alertService;

    public AlertsController(AlertService.Services.AlertService alertService)
    {
        _alertService = alertService;
    }

    // GET: /api/alerts?status=active
    [HttpGet]
    public IActionResult GetAlerts([FromQuery] string? status)
    {
        var alerts = _alertService.GetAlerts(status);
        return Ok(alerts);
    }

    // PATCH: /api/alerts/{id}/ack
    [HttpPatch("{id}/ack")]
    public IActionResult AcknowledgeAlert(int id)
    {
        var success = _alertService.AcknowledgeAlert(id);

        if (!success)
            return NotFound();

        return NoContent();
    }
}