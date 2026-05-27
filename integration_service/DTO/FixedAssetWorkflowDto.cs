namespace FixedAssets.Integration.DTO;

/// <summary>
/// Request that starts the integrated fixed asset disposal workflow.
/// </summary>
public sealed record FixedAssetWorkflowRequest
{
    /// <summary>
    /// Unique inventory identifier of the fixed asset.
    /// </summary>
    public required string InventoryId { get; init; }

    /// <summary>
    /// Useful life in years for straight-line depreciation.
    /// </summary>
    public int UsefulLife { get; init; } = 5;

    /// <summary>
    /// Business reason for disposal.
    /// </summary>
    public required string DisposalReason { get; init; }
}

/// <summary>
/// Response that aggregates ObjectService, DepreciationService and DisposalService results.
/// </summary>
public sealed record FixedAssetWorkflowResponse
{
    /// <summary>
    /// Workflow correlation identifier for logs and diagnostics.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Asset selected for the workflow.
    /// </summary>
    public required ObjectDto Asset { get; init; }

    /// <summary>
    /// Calculated depreciation result.
    /// </summary>
    public required DepreciationDto Depreciation { get; init; }

    /// <summary>
    /// Disposal operation result.
    /// </summary>
    public required DisposalDto Disposal { get; init; }

    /// <summary>
    /// Final workflow status.
    /// </summary>
    public required string Status { get; init; }
}
