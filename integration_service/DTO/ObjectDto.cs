namespace FixedAssets.Integration.DTO;

/// <summary>
/// Fixed asset object returned by ObjectService.
/// </summary>
public sealed record ObjectDto
{
    /// <summary>
    /// Unique inventory identifier of the fixed asset.
    /// </summary>
    public required string InventoryId { get; init; }

    /// <summary>
    /// Human-readable asset name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Initial accounting cost of the asset.
    /// </summary>
    public decimal Cost { get; init; }
}
