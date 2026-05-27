using System.Text.Json;
using FixedAssets.Integration.Adapters;
using FixedAssets.Integration.DTO;
using Microsoft.Extensions.Caching.Distributed;

namespace FixedAssets.Integration;

/// <summary>
/// Coordinates the fixed assets integration workflow.
/// </summary>
public sealed class IntegrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ObjectAdapter _objectAdapter;
    private readonly DepreciationAdapter _depreciationAdapter;
    private readonly DisposalAdapter _disposalAdapter;
    private readonly IDistributedCache _cache;
    private readonly ILogger<IntegrationService> _logger;

    /// <summary>
    /// Creates integration workflow service.
    /// </summary>
    /// <param name="objectAdapter">ObjectService adapter.</param>
    /// <param name="depreciationAdapter">DepreciationService adapter.</param>
    /// <param name="disposalAdapter">DisposalService adapter.</param>
    /// <param name="cache">Redis distributed cache.</param>
    /// <param name="logger">Structured logger.</param>
    public IntegrationService(
        ObjectAdapter objectAdapter,
        DepreciationAdapter depreciationAdapter,
        DisposalAdapter disposalAdapter,
        IDistributedCache cache,
        ILogger<IntegrationService> logger)
    {
        _objectAdapter = objectAdapter;
        _depreciationAdapter = depreciationAdapter;
        _disposalAdapter = disposalAdapter;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Returns fixed assets and stores them in Redis for short-lived read optimization.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Collection of fixed assets.</returns>
    public async Task<IReadOnlyCollection<ObjectDto>> GetAssetsAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "assets:list";
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(cached))
        {
            _logger.LogInformation("Redis cache hit for {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<IReadOnlyCollection<ObjectDto>>(cached, JsonOptions)
                ?? Array.Empty<ObjectDto>();
        }

        _logger.LogInformation("Redis cache miss for {CacheKey}", cacheKey);
        var assets = await _objectAdapter.GetObjectsAsync(cancellationToken);

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(assets, JsonOptions),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            },
            cancellationToken);

        return assets;
    }

    /// <summary>
    /// Runs the integrated demo workflow: load asset, calculate depreciation and dispose asset.
    /// </summary>
    /// <param name="request">Workflow input from frontend or API client.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Aggregated workflow response.</returns>
    public async Task<FixedAssetWorkflowResponse> ExecuteFixedAssetWorkflowAsync(
        FixedAssetWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.InventoryId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DisposalReason);

        if (request.UsefulLife <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.UsefulLife), "Useful life must be positive.");
        }

        var correlationId = Guid.NewGuid().ToString("N");
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["InventoryId"] = request.InventoryId
        });

        _logger.LogInformation("Starting fixed asset workflow");

        var assets = await GetAssetsAsync(cancellationToken);
        var asset = assets.FirstOrDefault(item =>
            string.Equals(item.InventoryId, request.InventoryId, StringComparison.OrdinalIgnoreCase));

        if (asset is null)
        {
            throw new InvalidOperationException($"Fixed asset {request.InventoryId} was not found.");
        }

        var depreciation = await _depreciationAdapter.CalculateAsync(
            new DepreciationRequest
            {
                Cost = asset.Cost,
                UsefulLife = request.UsefulLife
            },
            cancellationToken);

        var disposal = await _disposalAdapter.DisposeAsync(
            new DisposalRequest
            {
                InventoryId = asset.InventoryId,
                Reason = request.DisposalReason
            },
            cancellationToken);

        _logger.LogInformation("Fixed asset workflow completed");

        return new FixedAssetWorkflowResponse
        {
            CorrelationId = correlationId,
            Asset = asset,
            Depreciation = depreciation,
            Disposal = disposal,
            Status = "Completed"
        };
    }
}
