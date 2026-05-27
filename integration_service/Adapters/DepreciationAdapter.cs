using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FixedAssets.Integration.DTO;
using FixedAssets.Integration.Monitoring;

namespace FixedAssets.Integration.Adapters;

/// <summary>
/// HTTP adapter for DepreciationService.
/// </summary>
public sealed class DepreciationAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DepreciationAdapter> _logger;

    /// <summary>
    /// Creates DepreciationService adapter.
    /// </summary>
    /// <param name="httpClient">Configured DepreciationService HTTP client.</param>
    /// <param name="logger">Structured logger.</param>
    public DepreciationAdapter(HttpClient httpClient, ILogger<DepreciationAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Calculates yearly depreciation for a fixed asset.
    /// </summary>
    /// <param name="request">Depreciation calculation request.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Calculated depreciation result.</returns>
    public async Task<DepreciationDto> CalculateAsync(
        DepreciationRequest request,
        CancellationToken cancellationToken)
    {
        var payload = new DepreciationServiceRequest(request.Cost, request.UsefulLife);

        var response = await PollyPolicies.RetryPolicy.ExecuteAsync(
            () => _httpClient.PostAsJsonAsync("/depreciation/calculate", payload, cancellationToken));

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DepreciationServiceResponse>(
            cancellationToken: cancellationToken);

        var dto = new DepreciationDto
        {
            YearlyDepreciation = result?.YearlyDepreciation ?? 0
        };

        _logger.LogInformation(
            "Calculated yearly depreciation {YearlyDepreciation} for asset cost {Cost}",
            dto.YearlyDepreciation,
            request.Cost);

        return dto;
    }

    private sealed record DepreciationServiceRequest(
        [property: JsonPropertyName("cost")] decimal Cost,
        [property: JsonPropertyName("useful_life")] int UsefulLife);

    private sealed record DepreciationServiceResponse(
        [property: JsonPropertyName("yearly_depreciation")] decimal YearlyDepreciation);
}
