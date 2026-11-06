using Microsoft.EntityFrameworkCore;
using InventoryService.Data;

var builder = WebApplication.CreateBuilder(args);

// Register controller support
builder.Services.AddControllers();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Read database connection string
var connectionString =
    builder.Configuration.GetConnectionString("InventoryDb");

// Register PostgreSQL database context
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(connectionString));

// Build the application
var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Map API controllers
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "InventoryService"
}));

app.Run();