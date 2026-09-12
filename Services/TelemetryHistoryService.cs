using System.Collections.Concurrent;
using SmartX.Api.Models;

namespace SmartX.Api.Services;

/// <summary>
/// Buffers each sensor's raw readings in a small jagged array of fixed-size
/// batches before promoting them into an optimised <see cref="List{T}"/> that
/// the API and dashboard actually query.
/// <para>
/// A jagged array (<c>double[][]</c>, not a rectangular <c>double[,]</c>) is
/// used deliberately for the staging buffer: each of a sensor's recent batches
/// only needs to be as wide as the readings it has actually received, and
/// jagged arrays let each row be allocated and re-filled independently, which
/// is exactly the "sequential historical batches of raw telemetry" shape the
/// brief describes. Keeping the raw batches around (rather than discarding
/// them once flushed) also lets the dashboard cheaply compute an in-batch
/// spike range (see <see cref="GetCurrentBatchRange"/>) without re-scanning
/// the whole history.
/// </para>
/// </summary>
public class TelemetryHistoryService
{
    private const int BatchSize = 8;      // raw readings per batch
    private const int JaggedDepth = 4;    // number of rolling batches kept per sensor
    private const int MaxHistoryLength = 200;

    private sealed class SensorBuffer
    {
        public readonly double[][] Batches = new double[JaggedDepth][];
        public int ActiveBatch;
        public int Cursor;
        public readonly List<TelemetryReadingDto> History = new();

        public SensorBuffer()
        {
            for (var i = 0; i < JaggedDepth; i++)
            {
                Batches[i] = new double[BatchSize];
            }
        }
    }

    private readonly ConcurrentDictionary<Guid, SensorBuffer> _buffers = new();

    /// <summary>Records a new reading: stages it in the jagged raw batch, then appends it to the List&lt;T&gt; history.</summary>
    public bool Record(Guid sensorId, double value, double safeMin, double safeMax)
    {
        var buffer = _buffers.GetOrAdd(sensorId, _ => new SensorBuffer());
        bool withinSafeRange;

        lock (buffer)
        {
            // Stage the raw value into the currently-active jagged batch.
            buffer.Batches[buffer.ActiveBatch][buffer.Cursor] = value;
            buffer.Cursor++;

            withinSafeRange = value >= safeMin && value <= safeMax;

            // Promote into the optimised List<T> that the rest of the app queries.
            buffer.History.Add(new TelemetryReadingDto(DateTimeOffset.UtcNow, value, withinSafeRange));
            if (buffer.History.Count > MaxHistoryLength)
            {
                buffer.History.RemoveAt(0);
            }

            // Batch full — roll to the next jagged slot (overwriting the oldest raw batch).
            if (buffer.Cursor >= BatchSize)
            {
                buffer.Cursor = 0;
                buffer.ActiveBatch = (buffer.ActiveBatch + 1) % JaggedDepth;
            }
        }

        return withinSafeRange;
    }

    public List<double> GetRecentValues(Guid sensorId, int count = 40)
    {
        if (!_buffers.TryGetValue(sensorId, out var buffer))
        {
            return new List<double>();
        }

        lock (buffer)
        {
            return buffer.History.TakeLast(count).Select(r => r.Value).ToList();
        }
    }

    public List<TelemetryReadingDto> GetHistory(Guid sensorId)
    {
        if (!_buffers.TryGetValue(sensorId, out var buffer))
        {
            return new List<TelemetryReadingDto>();
        }

        lock (buffer)
        {
            return new List<TelemetryReadingDto>(buffer.History);
        }
    }

    /// <summary>
    /// Returns the max-minus-min spread of whatever has been staged into the
    /// sensor's currently-filling jagged batch — a cheap, allocation-light way
    /// to flag an in-progress spike before the batch is even complete.
    /// </summary>
    public double GetCurrentBatchRange(Guid sensorId)
    {
        if (!_buffers.TryGetValue(sensorId, out var buffer))
        {
            return 0;
        }

        lock (buffer)
        {
            if (buffer.Cursor == 0)
            {
                return 0;
            }

            var filled = buffer.Batches[buffer.ActiveBatch].Take(buffer.Cursor).ToArray();
            return filled.Max() - filled.Min();
        }
    }

    /// <summary>Exposes the raw jagged batches directly, for diagnostics / teaching purposes.</summary>
    public double[][] GetRawBatches(Guid sensorId) =>
        _buffers.TryGetValue(sensorId, out var buffer) ? buffer.Batches : Array.Empty<double[]>();
}
