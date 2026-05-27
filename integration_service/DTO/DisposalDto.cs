namespace FixedAssets.Integration.DTO;

/// <summary>
/// Request for fixed asset disposal.
/// </summary>
public sealed record DisposalRequest
{
    /// <summary>
    /// Unique inventory identifier of the fixed asset.
    /// </summary>
    public required string InventoryId { get; init; }

    /// <summary>
    /// Business reason for disposal.
    /// </summary>
    public required string Reason { get; init; }
}

/// <summary>
/// Response returned after disposal operation.
/// </summary>
public sealed record DisposalDto
{
    /// <summary>
    /// Disposal service status message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Disposal reason accepted by the service.
    /// </summary>
    public required string Reason { get; init; }
}
