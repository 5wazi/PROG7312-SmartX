namespace SmartX.Api.Models;

/// <summary>
/// Represents a single sensor's numeric contribution to an aggregate calculation
/// (e.g. combined wattage across two smart meters, or the delta between two
/// consecutive readings). Operators are overloaded so call sites read like the
/// domain they describe — <c>Meter3 = Meter1 + Meter2</c> — instead of exposing
/// an Add()/Subtract() method vocabulary that hides the arithmetic.
/// </summary>
public readonly struct SensorLoad : IEquatable<SensorLoad>, IComparable<SensorLoad>
{
    public string Label { get; }
    public double Value { get; }
    public string Unit { get; }

    public SensorLoad(string label, double value, string unit)
    {
        Label = label;
        Value = value;
        Unit = unit;
    }

    /// <summary>Aggregates two loads sharing the same unit (e.g. combined smart-meter wattage).</summary>
    public static SensorLoad operator +(SensorLoad a, SensorLoad b)
    {
        EnsureComparableUnits(a, b);
        return new SensorLoad($"{a.Label}+{b.Label}", a.Value + b.Value, a.Unit);
    }

    /// <summary>Computes the delta between two loads (e.g. spike detection between consecutive readings).</summary>
    public static SensorLoad operator -(SensorLoad a, SensorLoad b)
    {
        EnsureComparableUnits(a, b);
        return new SensorLoad($"{a.Label}-{b.Label}", a.Value - b.Value, a.Unit);
    }

    public static bool operator >(SensorLoad a, SensorLoad b) { EnsureComparableUnits(a, b); return a.Value > b.Value; }
    public static bool operator <(SensorLoad a, SensorLoad b) { EnsureComparableUnits(a, b); return a.Value < b.Value; }
    public static bool operator >=(SensorLoad a, SensorLoad b) { EnsureComparableUnits(a, b); return a.Value >= b.Value; }
    public static bool operator <=(SensorLoad a, SensorLoad b) { EnsureComparableUnits(a, b); return a.Value <= b.Value; }

    public static bool operator ==(SensorLoad a, SensorLoad b) => a.Unit == b.Unit && a.Value.Equals(b.Value);
    public static bool operator !=(SensorLoad a, SensorLoad b) => !(a == b);

    private static void EnsureComparableUnits(SensorLoad a, SensorLoad b)
    {
        if (!string.Equals(a.Unit, b.Unit, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Cannot combine sensor loads with mismatched units ('{a.Unit}' vs '{b.Unit}').");
        }
    }

    public bool Equals(SensorLoad other) => this == other;
    public override bool Equals(object? obj) => obj is SensorLoad other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Value, Unit);
    public int CompareTo(SensorLoad other) => Value.CompareTo(other.Value);
    public override string ToString() => $"{Label}: {Value:0.##}{Unit}";
}
