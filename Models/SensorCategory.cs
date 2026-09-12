namespace SmartX.Api.Models;

/// <summary>
/// The broad class of thing a sensor node measures or controls.
/// Drives which unit, safe range and mock-seeding profile is used.
/// </summary>
public enum SensorCategory
{
    Environmental,     // e.g. soil moisture, humidity, temperature -> float
    PowerConsumption,  // e.g. smart meter wattage -> int
    Actuator           // e.g. valve / relay state -> bool
}

/// <summary>
/// The underlying CLR value type carried by a sensor's telemetry, kept explicit
/// so the ingestion endpoint can build the correctly-typed TelemetryPacket&lt;T&gt;
/// without guessing from the wire payload.
/// </summary>
public enum TelemetryValueKind
{
    Float,
    Integer,
    Boolean
}
