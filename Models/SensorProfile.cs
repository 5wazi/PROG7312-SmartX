namespace SmartX.Api.Models;

/// <summary>
/// A registered Smart-X sensor node — the record created when a technician
/// (or the seeding service) attaches a device to the gateway.
/// </summary>
public class SensorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Device MAC address / unique hardware identifier.</summary>
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>Deployment path through the facility tree, e.g. "Facility A/Zone 1/Sub-Zone B".</summary>
    public string DeploymentLocation { get; set; } = string.Empty;

    public SensorCategory Category { get; set; }
    public TelemetryValueKind ValueKind { get; set; }
    public string Unit { get; set; } = string.Empty;

    /// <summary>Lower/upper bounds of a "healthy" reading, used for anomaly flags and streaks.</summary>
    public double SafeMin { get; set; }
    public double SafeMax { get; set; }

    public DateTimeOffset RegisteredAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSeen { get; set; }

    /// <summary>Optional config file / deployment photo / hardware log attached to this profile.</summary>
    public string? AttachmentFileName { get; set; }
    public string? AttachmentUrl { get; set; }

    /// <summary>Consecutive in-range readings — the gamified "Sensor Health Streak".</summary>
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }

    public bool IsOnline => LastSeen is not null && DateTimeOffset.UtcNow - LastSeen < TimeSpan.FromSeconds(12);
}
