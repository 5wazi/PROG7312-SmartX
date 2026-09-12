using SmartX.Api.Data;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Endpoints;

public static class TelemetryEndpoints
{
    public static void MapTelemetryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/telemetry").WithTags("Telemetry");

        // Manual ingestion — lets a caller (or the seeding service) push a single
        // reading. Internally this is routed through TelemetryPacket<T> so the
        // strongly-typed, non-boxed wrapper is genuinely exercised end to end.
        group.MapPost("/", (TelemetryIngestRequest request, InMemoryStore store, TelemetryHistoryService history) =>
        {
            var sensor = store.Get(request.SensorId);
            if (sensor is null)
            {
                return Results.NotFound("Unknown sensor id.");
            }

            var withinRange = sensor.ValueKind switch
            {
                TelemetryValueKind.Boolean => IngestTyped(new TelemetryPacket<bool>(sensor.Id, request.Value >= 0.5, sensor.Unit), sensor, history),
                TelemetryValueKind.Integer => IngestTyped(new TelemetryPacket<int>(sensor.Id, (int)Math.Round(request.Value), sensor.Unit), sensor, history),
                _ => IngestTyped(new TelemetryPacket<float>(sensor.Id, (float)request.Value, sensor.Unit), sensor, history)
            };

            sensor.LastSeen = DateTimeOffset.UtcNow;
            sensor.CurrentStreak = withinRange ? sensor.CurrentStreak + 1 : 0;
            sensor.BestStreak = Math.Max(sensor.BestStreak, sensor.CurrentStreak);

            return Results.Ok(new { withinRange, sensor.CurrentStreak, sensor.BestStreak });
        });

        group.MapGet("/{sensorId:guid}/history", (Guid sensorId, TelemetryHistoryService history) =>
            Results.Ok(history.GetHistory(sensorId)));

        group.MapGet("/{sensorId:guid}/batch-range", (Guid sensorId, TelemetryHistoryService history) =>
            Results.Ok(new { rangeInCurrentBatch = history.GetCurrentBatchRange(sensorId) }));

        // Demonstrates operator overloading on SensorLoad: aggregates or diffs the
        // latest readings of two sensors that share the same unit, e.g.
        // Meter3 = Meter1 + Meter2.
        group.MapPost("/aggregate", (AggregateRequest request, InMemoryStore store, TelemetryHistoryService history) =>
        {
            var a = store.Get(request.SensorAId);
            var b = store.Get(request.SensorBId);
            if (a is null || b is null)
            {
                return Results.NotFound("One or both sensors were not found.");
            }

            var aLatest = history.GetRecentValues(a.Id, 1).FirstOrDefault();
            var bLatest = history.GetRecentValues(b.Id, 1).FirstOrDefault();

            var loadA = new SensorLoad(a.MacAddress, aLatest, a.Unit);
            var loadB = new SensorLoad(b.MacAddress, bLatest, b.Unit);

            try
            {
                var result = request.Operation.Equals("subtract", StringComparison.OrdinalIgnoreCase)
                    ? loadA - loadB
                    : loadA + loadB;

                var expression = request.Operation.Equals("subtract", StringComparison.OrdinalIgnoreCase)
                    ? $"{loadA} - {loadB}"
                    : $"{loadA} + {loadB}";

                return Results.Ok(new AggregateResult(result.Label, result.Value, result.Unit, expression));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });
    }

    private static bool IngestTyped<T>(TelemetryPacket<T> packet, SensorProfile sensor, TelemetryHistoryService history) where T : struct
    {
        var value = packet.AsDouble();
        return history.Record(sensor.Id, value, sensor.SafeMin, sensor.SafeMax);
    }
}
