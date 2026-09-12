using System.Collections.Concurrent;
using SmartX.Api.Models;

namespace SmartX.Api.Data;

/// <summary>
/// A simple, thread-safe in-memory repository of sensor profiles. Swapping this
/// for EF Core + SQL Server later is a drop-in change: every consumer talks to
/// this class through the same handful of methods, never to the dictionary directly.
/// </summary>
public class InMemoryStore
{
    private readonly ConcurrentDictionary<Guid, SensorProfile> _sensors = new();

    public SensorProfile Add(SensorProfile profile)
    {
        _sensors[profile.Id] = profile;
        return profile;
    }

    public SensorProfile? Get(Guid id) => _sensors.TryGetValue(id, out var s) ? s : null;

    public IReadOnlyCollection<SensorProfile> All() => _sensors.Values.ToList();

    public bool Remove(Guid id) => _sensors.TryRemove(id, out _);
}
