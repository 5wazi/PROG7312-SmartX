namespace SmartX.Api.Models;

public record SensorRegistrationRequest(
    string MacAddress,
    string DeploymentLocation,
    SensorCategory Category,
    TelemetryValueKind ValueKind,
    string Unit,
    double SafeMin,
    double SafeMax);

public record TelemetryIngestRequest(
    Guid SensorId,
    double Value);

public record TelemetryReadingDto(
    DateTimeOffset Timestamp,
    double Value,
    bool WithinSafeRange);

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

public record DeploymentValidationRequest(string Path);

public record DeploymentValidationResult(bool IsValid, string Path, string Message);

public record AggregateRequest(Guid SensorAId, Guid SensorBId, string Operation); // "add" | "subtract"

public record AggregateResult(string Label, double Value, string Unit, string Expression);
