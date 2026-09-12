using System.Net.Http.Json;
using SmartX.Web.Models;

namespace SmartX.Web.Services;

/// <summary>
/// The single point of contact between the MVC frontend and the SmartX.Api
/// Web API. Every controller goes through this typed client rather than
/// injecting HttpClient directly, so the base address, error handling, and
/// JSON options live in exactly one place.
/// </summary>
public class SmartXApiClient
{
    private readonly HttpClient _http;

    public SmartXApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<SensorSummaryDto>> GetSensorsAsync() =>
        await _http.GetFromJsonAsync<List<SensorSummaryDto>>("api/sensors") ?? new();

    public async Task<SensorSummaryDto?> GetSensorAsync(Guid id) =>
        await _http.GetFromJsonAsync<SensorSummaryDto>($"api/sensors/{id}");

    public async Task<(bool Success, string? Error)> RegisterSensorAsync(SensorRegistrationRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/sensors", request);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<bool> DeleteSensorAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/sensors/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Success, string? Error)> UploadAttachmentAsync(Guid sensorId, Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync($"api/sensors/{sensorId}/attachment", content);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }

    public async Task<DeploymentNodeDto?> GetDeploymentTreeAsync() =>
        await _http.GetFromJsonAsync<DeploymentNodeDto>("api/deployment/tree");

    public async Task<DeploymentValidationResult?> ValidateDeploymentPathAsync(string path)
    {
        var response = await _http.PostAsJsonAsync("api/deployment/validate", new DeploymentValidationRequest(path));
        return await response.Content.ReadFromJsonAsync<DeploymentValidationResult>();
    }

    public async Task<AggregateResult?> AggregateAsync(Guid sensorA, Guid sensorB, string operation)
    {
        var response = await _http.PostAsJsonAsync("api/telemetry/aggregate", new AggregateRequest(sensorA, sensorB, operation));
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        return await response.Content.ReadFromJsonAsync<AggregateResult>();
    }

    public async Task<DashboardStats?> GetStatsAsync() =>
        await _http.GetFromJsonAsync<DashboardStats>("api/dashboard/stats");
}
