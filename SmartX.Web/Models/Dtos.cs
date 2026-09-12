namespace SmartX.Web.Models;

public enum SensorCategory { Environmental, PowerConsumption, Actuator }
public enum TelemetryValueKind { Float, Integer, Boolean }

public record SensorRegistrationRequest(
    string MacAddress,
    string DeploymentLocation,
    SensorCategory Category,
    TelemetryValueKind ValueKind,
    string Unit,
    double SafeMin,
    double SafeMax);

public record SensorSummaryDto(
    Guid Id,
    string MacAddress,
    string DeploymentLocation,
    SensorCategory Category,
    string Unit,
    double SafeMin,
    double SafeMax,
    bool IsOnline,
    int CurrentStreak,
    int BestStreak,
    string? AttachmentFileName,
    string? AttachmentUrl,
    List<double> RecentValues);

public record DeploymentNodeDto(string Name, List<DeploymentNodeDto> Children);

public record DeploymentValidationRequest(string Path);
public record DeploymentValidationResult(bool IsValid, string Path, string Message);

public record AggregateRequest(Guid SensorAId, Guid SensorBId, string Operation);
public record AggregateResult(string Label, double Value, string Unit, string Expression);

public record DashboardStats(
    int TotalSensors, int OnlineSensors, int OfflineSensors,
    int BestStreakEver, double AverageCurrentStreak);
