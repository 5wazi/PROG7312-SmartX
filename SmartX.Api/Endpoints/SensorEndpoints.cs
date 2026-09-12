using SmartX.Api.Data;
using SmartX.Api.Models;
using SmartX.Api.Services;
using SmartX.Api.Validation;

namespace SmartX.Api.Endpoints;

public static class SensorEndpoints
{
    public static void MapSensorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sensors").WithTags("Sensors");

        group.MapGet("/", (InMemoryStore store, TelemetryHistoryService history) =>
        {
            var result = store.All()
                .OrderBy(s => s.DeploymentLocation)
                .Select(s => ToSummary(s, history))
                .ToList();
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", (Guid id, InMemoryStore store, TelemetryHistoryService history) =>
        {
            var sensor = store.Get(id);
            return sensor is null ? Results.NotFound() : Results.Ok(ToSummary(sensor, history));
        });

        group.MapPost("/", (SensorRegistrationRequest request, InMemoryStore store, DeploymentTreeService tree) =>
        {
            if (string.IsNullOrWhiteSpace(request.MacAddress))
            {
                return Results.BadRequest("MAC address / unique identifier is required.");
            }

            // Recursively validate the deployment path against the facility tree
            // before the sensor is allowed onto the gateway.
            var validation = DeploymentValidator.Validate(tree.Root, request.DeploymentLocation);
            if (!validation.IsValid)
            {
                return Results.BadRequest(validation.Message);
            }

            var profile = new SensorProfile
            {
                MacAddress = request.MacAddress,
                DeploymentLocation = request.DeploymentLocation,
                Category = request.Category,
                ValueKind = request.ValueKind,
                Unit = request.Unit,
                SafeMin = request.SafeMin,
                SafeMax = request.SafeMax
            };

            store.Add(profile);
            return Results.Created($"/api/sensors/{profile.Id}", ToSummary(profile, null));
        });

        group.MapDelete("/{id:guid}", (Guid id, InMemoryStore store) =>
            store.Remove(id) ? Results.NoContent() : Results.NotFound());

        // Optimised multipart file uploader for device config files, deployment
        // photos, or hardware log files attached to a specific sensor profile.
        group.MapPost("/{id:guid}/attachment", async (Guid id, HttpRequest request, InMemoryStore store, IWebHostEnvironment env) =>
        {
            var sensor = store.Get(id);
            if (sensor is null)
            {
                return Results.NotFound();
            }

            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Expected multipart/form-data.");
            }

            var form = await request.ReadFormAsync();
            var file = form.Files["file"];
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest("No file supplied under form field 'file'.");
            }

            const long maxBytes = 15 * 1024 * 1024; // 15 MB cap
            if (file.Length > maxBytes)
            {
                return Results.BadRequest("File exceeds the 15 MB attachment limit.");
            }

            var uploadsRoot = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var safeFileName = $"{sensor.Id}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsRoot, safeFileName);

            await using (var stream = File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            sensor.AttachmentFileName = file.FileName;
            sensor.AttachmentUrl = $"/uploads/{safeFileName}";

            return Results.Ok(ToSummary(sensor, null));
        }).DisableAntiforgery();
    }

    private static SensorSummaryDto ToSummary(SensorProfile s, TelemetryHistoryService? history) => new(
        s.Id,
        s.MacAddress,
        s.DeploymentLocation,
        s.Category,
        s.Unit,
        s.SafeMin,
        s.SafeMax,
        s.IsOnline,
        s.CurrentStreak,
        s.BestStreak,
        s.AttachmentFileName,
        s.AttachmentUrl,
        history?.GetRecentValues(s.Id) ?? new List<double>());
}
