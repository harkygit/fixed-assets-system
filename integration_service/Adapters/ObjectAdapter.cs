using System.Text.Json;
using System.Text.Json.Serialization;
using FixedAssets.Integration.DTO;
using FixedAssets.Integration.Monitoring;

namespace FixedAssets.Integration.Adapters;

/// <summary>
/// HTTP adapter for ObjectService.
/// </summary>
public sealed class ObjectAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ILogger<ObjectAdapter> _logger;

    /// <summary>
    /// Creates ObjectService adapter.
    /// </summary>
    /// <param name="httpClient">Configured ObjectService HTTP client.</param>
    /// <param name="logger">Structured logger.</param>
    public ObjectAdapter(HttpClient httpClient, ILogger<ObjectAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets all fixed assets from ObjectService.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Collection of fixed assets.</returns>
    public async Task<IReadOnlyCollection<ObjectDto>> GetObjectsAsync(CancellationToken cancellationToken)
    {
        var response = await PollyPolicies.RetryPolicy.ExecuteAsync(
            () => _httpClient.GetAsync("/objects", cancellationToken));

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var rawObjects = await JsonSerializer.DeserializeAsync<List<ObjectServiceResponse>>(
            stream,
            JsonOptions,
            cancellationToken);

        var objects = rawObjects?
            .Select(item => new ObjectDto
            {
                InventoryId = item.InventoryId,
                Name = item.Name,
                Cost = item.Cost
            })
            .ToArray() ?? Array.Empty<ObjectDto>();

        _logger.LogInformation("Loaded {AssetCount} fixed assets from ObjectService", objects.Length);
        return objects;
    }

    private sealed record ObjectServiceResponse(
        [property: JsonPropertyName("inventory_id")] string InventoryId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("cost")] decimal Cost);
}
