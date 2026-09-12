namespace SmartX.Api.Models;

/// <summary>
/// A generic, allocation-light wrapper around a single piece of telemetry.
/// <para>
/// Smart-X sensors report three fundamentally different CLR types on the same
/// wire protocol: <c>float</c> soil-moisture readings, <c>int</c> power-wattage
/// readings and <c>bool</c> valve/relay states. Rather than forcing every
/// reading through a common reference type (which would box every struct and
/// throw away the compiler's type safety), <see cref="TelemetryPacket{T}"/> is
/// constrained to <c>struct</c> so the JIT generates a specialised, non-boxed
/// implementation per value type (TelemetryPacket&lt;float&gt;,
/// TelemetryPacket&lt;int&gt;, TelemetryPacket&lt;bool&gt;, ...).
/// </para>
/// </summary>
/// <typeparam name="T">The value type carried by this packet (float, int, bool, ...).</typeparam>
public readonly struct TelemetryPacket<T> where T : struct
{
    public Guid SensorId { get; init; }
    public T Value { get; init; }
    public string Unit { get; init; }
    public DateTimeOffset Timestamp { get; init; }

    public TelemetryPacket(Guid sensorId, T value, string unit, DateTimeOffset? timestamp = null)
    {
        SensorId = sensorId;
        Value = value;
        Unit = unit;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Converts the strongly-typed value to a <see cref="double"/> for charting,
    /// aggregation and threshold comparison, without ever boxing <typeparamref name="T"/>.
    /// Boolean packets report 1.0 / 0.0 so a valve's open/closed history can still
    /// be plotted on the same sparkline component as numeric sensors.
    /// </summary>
    public double AsDouble() => Value switch
    {
        float f => f,
        int i => i,
        double d => d,
        bool b => b ? 1.0 : 0.0,
        _ => Convert.ToDouble(Value)
    };

    public override string ToString() => $"[{Timestamp:HH:mm:ss}] {SensorId:D}: {Value}{Unit}";
}
