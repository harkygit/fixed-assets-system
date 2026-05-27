using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FixedAssets.Integration.DTO;
using FixedAssets.Integration.Monitoring;

namespace FixedAssets.Integration.Adapters;

/// <summary>
/// HTTP adapter for DisposalService.
/// </summary>
public sealed class DisposalAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DisposalAdapter> _logger;

    /// <summary>
    /// Creates DisposalService adapter.
    /// </summary>
    /// <param name="httpClient">Configured DisposalService HTTP client.</param>
    /// <param name="logger">Structured logger.</param>
    public DisposalAdapter(HttpClient httpClient, ILogger<DisposalAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Sends disposal command to DisposalService.
    /// </summary>
    /// <param name="request">Disposal command.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Disposal operation result.</returns>
    public async Task<DisposalDto> DisposeAsync(DisposalRequest request, CancellationToken cancellationToken)
    {
        var payload = new DisposalServiceRequest(request.InventoryId, request.Reason);

        var response = await PollyPolicies.CircuitBreakerPolicy.ExecuteAsync(
            () => _httpClient.PostAsJsonAsync("/disposal", payload, cancellationToken));

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DisposalServiceResponse>(
            cancellationToken: cancellationToken);

        var dto = new DisposalDto
        {
            Message = result?.Message ?? $"Asset {request.InventoryId} disposed",
            Reason = result?.Reason ?? request.Reason
        };

        _logger.LogInformation("Disposed fixed asset {InventoryId}", request.InventoryId);
        return dto;
    }

    private sealed record DisposalServiceRequest(
        [property: JsonPropertyName("inventory_id")] string InventoryId,
        [property: JsonPropertyName("reason")] string Reason);

    private sealed record DisposalServiceResponse(
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("reason")] string Reason);
}
