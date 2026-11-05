using Dashboard.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();

// Mock mode: run the dashboard alone with fake data (no Inventory / Alert services needed).
var useMock = builder.Configuration.GetValue<bool>("Services:UseMockData");

if (useMock)
{
    builder.Services.AddSingleton<IInventoryClient, MockInventoryClient>();
    builder.Services.AddSingleton<IAlertClient, MockAlertClient>();
}
else
{
    var inventoryUrl = builder.Configuration["Services:InventoryUrl"]
        ?? throw new InvalidOperationException("Services:InventoryUrl is not configured.");
    var alertUrl = builder.Configuration["Services:AlertUrl"]
        ?? throw new InvalidOperationException("Services:AlertUrl is not configured.");

    builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(c =>
    {
        c.BaseAddress = new Uri(inventoryUrl.TrimEnd('/') + "/");
        c.Timeout = TimeSpan.FromSeconds(5);
    });

    builder.Services.AddHttpClient<IAlertClient, AlertClient>(c =>
    {
        c.BaseAddress = new Uri(alertUrl.TrimEnd('/') + "/");
        c.Timeout = TimeSpan.FromSeconds(5);
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Prometheus: records HTTP request metrics and exposes them on /metrics
app.UseHttpMetrics();

app.MapRazorPages();
app.MapHealthChecks("/health");
app.MapMetrics();

app.Logger.LogInformation("Dashboard started. Mock data mode: {UseMock}", useMock);

app.Run();
