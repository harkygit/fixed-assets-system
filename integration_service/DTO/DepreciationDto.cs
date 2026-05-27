namespace FixedAssets.Integration.DTO;

/// <summary>
/// Request for straight-line depreciation calculation.
/// </summary>
public sealed record DepreciationRequest
{
    /// <summary>
    /// Initial accounting cost of the fixed asset.
    /// </summary>
    public decimal Cost { get; init; }

    /// <summary>
    /// Useful life in years.
    /// </summary>
    public int UsefulLife { get; init; }
}

/// <summary>
/// Result of depreciation calculation.
/// </summary>
public sealed record DepreciationDto
{
    /// <summary>
    /// Yearly depreciation amount.
    /// </summary>
    public decimal YearlyDepreciation { get; init; }
}
