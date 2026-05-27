/// <summary>
/// Demo stock availability service used in business process examples.
/// </summary>
public class StockService
{
    /// <summary>
    /// Checks whether a requested asset quantity is available.
    /// </summary>
    /// <param name="productId">Fixed asset identifier.</param>
    /// <param name="quantity">Requested quantity.</param>
    /// <returns>True when requested quantity is available; otherwise false.</returns>
    public bool CheckStock(string productId, int quantity)
    {
        const int available = 10;
        return !string.IsNullOrWhiteSpace(productId) && available >= quantity;
    }
}
