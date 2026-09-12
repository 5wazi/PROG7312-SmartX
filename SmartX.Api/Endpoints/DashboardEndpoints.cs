using SmartX.Api.Data;

namespace SmartX.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/stats", (InMemoryStore store) =>
        {
            var sensors = store.All();
            var online = sensors.Count(s => s.IsOnline);
            var bestStreak = sensors.Count > 0 ? sensors.Max(s => s.BestStreak) : 0;
            var avgStreak = sensors.Count > 0 ? Math.Round(sensors.Average(s => s.CurrentStreak), 1) : 0;

            return Results.Ok(new
            {
                totalSensors = sensors.Count,
                onlineSensors = online,
                offlineSensors = sensors.Count - online,
                bestStreakEver = bestStreak,
                averageCurrentStreak = avgStreak
            });
        });
    }
}
