using SmartX.Api.Data;
using SmartX.Api.Models;

namespace SmartX.Api.Services;

/// <summary>
/// Simulates the "thousands of ESP32 chips publishing constant telemetry" the
/// brief describes. Runs continuously in the background, generating a plausible
/// reading for every registered sensor every tick, so the dashboard has data to
/// prove the ingestion pipeline and data structures work under sustained load
/// even with no physical hardware attached.
/// </summary>
public class TelemetrySeedingService : BackgroundService
{
    private static readonly Random Rng = new();
    private readonly InMemoryStore _store;
    private readonly TelemetryHistoryService _history;
    private readonly ILogger<TelemetrySeedingService> _logger;

    public TelemetrySeedingService(InMemoryStore store, TelemetryHistoryService history, ILogger<TelemetrySeedingService> logger)
    {
        _store = store;
        _history = history;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Seed a handful of demo sensors on startup so the dashboard is never empty.
        SeedDefaultSensorsIfEmpty();

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var sensor in _store.All())
            {
                var value = GenerateReading(sensor);
                var withinRange = _history.Record(sensor.Id, value, sensor.SafeMin, sensor.SafeMax);

                sensor.LastSeen = DateTimeOffset.UtcNow;
                if (withinRange)
                {
                    sensor.CurrentStreak++;
                    sensor.BestStreak = Math.Max(sensor.BestStreak, sensor.CurrentStreak);
                }
                else
                {
                    sensor.CurrentStreak = 0;
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // normal on shutdown
            }
        }
    }

    /// <summary>Produces a mock reading whose distribution matches the sensor's category, occasionally drifting out of range to exercise the anomaly path.</summary>
    private static double GenerateReading(SensorProfile sensor)
    {
        var midpoint = (sensor.SafeMin + sensor.SafeMax) / 2.0;
        var spread = (sensor.SafeMax - sensor.SafeMin) / 2.0;

        // ~12% chance of an out-of-range "anomaly" reading to prove the alerting path.
        var isAnomaly = Rng.NextDouble() < 0.12;
        var jitter = (Rng.NextDouble() * 2 - 1) * (isAnomaly ? spread * 1.6 : spread * 0.6);
        var value = midpoint + jitter;

        return sensor.ValueKind switch
        {
            TelemetryValueKind.Boolean => Rng.NextDouble() < 0.9 ? 1.0 : 0.0,
            TelemetryValueKind.Integer => Math.Round(value),
            _ => Math.Round(value, 2)
        };
    }

    private void SeedDefaultSensorsIfEmpty()
    {
        if (_store.All().Count > 0)
        {
            return;
        }

        var defaults = new[]
        {
            new SensorProfile
            {
                MacAddress = "24:6F:28:AA:01:01",
                DeploymentLocation = "Root/Facility A/Zone 1/Sub-Zone B",
                Category = SensorCategory.Environmental,
                ValueKind = TelemetryValueKind.Float,
                Unit = "%",
                SafeMin = 35,
                SafeMax = 65
            },
            new SensorProfile
            {
                MacAddress = "24:6F:28:AA:01:02",
                DeploymentLocation = "Root/Facility A/Zone 2/Sub-Zone A",
                Category = SensorCategory.PowerConsumption,
                ValueKind = TelemetryValueKind.Integer,
                Unit = "W",
                SafeMin = 400,
                SafeMax = 1200
            },
            new SensorProfile
            {
                MacAddress = "24:6F:28:AA:01:03",
                DeploymentLocation = "Root/Facility A/Zone 3/Greenhouse 1",
                Category = SensorCategory.Actuator,
                ValueKind = TelemetryValueKind.Boolean,
                Unit = "",
                SafeMin = 1,
                SafeMax = 1
            },
            new SensorProfile
            {
                MacAddress = "24:6F:28:AA:01:04",
                DeploymentLocation = "Root/Facility B/Zone 1/Sub-Zone A",
                Category = SensorCategory.PowerConsumption,
                ValueKind = TelemetryValueKind.Integer,
                Unit = "W",
                SafeMin = 200,
                SafeMax = 900
            }
        };

        foreach (var sensor in defaults)
        {
            _store.Add(sensor);
        }

        _logger.LogInformation("Seeded {Count} default Smart-X sensors", defaults.Length);
    }
}
