using SmartX.Api.Data;
using SmartX.Api.Endpoints;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Services -----------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<TelemetryHistoryService>();
builder.Services.AddSingleton<DeploymentTreeService>();
builder.Services.AddHostedService<TelemetrySeedingService>();

var app = builder.Build();

// --- Middleware -----------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles(); // serves /uploads/* attachments from wwwroot

app.MapGet("/", () => Results.Ok(new
{
    service = "Smart-X Data Ingestion & Validation Gateway",
    status = "online",
    pillars = new[]
    {
        new { name = "Sensor Data Ingestion & Telemetry", enabled = true },
        new { name = "Real-Time Command Stream & History", enabled = false },
        new { name = "Network Topology & Mesh Routing", enabled = false }
    }
}));

app.MapSensorEndpoints();
app.MapTelemetryEndpoints();
app.MapDeploymentEndpoints();
app.MapDashboardEndpoints();

app.Run();
