using Serilog;

/// <summary>
/// Demonstrates the high-level fixed asset business process.
/// </summary>
public class BusinessProcessService
{
    private readonly StockService _stockService;

    /// <summary>
    /// Creates business process service.
    /// </summary>
    /// <param name="stockService">Stock availability service.</param>
    public BusinessProcessService(StockService stockService)
    {
        _stockService = stockService;
    }

    /// <summary>
    /// Executes demo business process with structured logging.
    /// </summary>
    public void ExecuteProcess()
    {
        Log.Information("Start fixed asset disposal business scenario");

        const string productId = "OS001";
        const int quantity = 1;

        var inStock = _stockService.CheckStock(productId, quantity);

        if (!inStock)
        {
            Log.Error("Fixed asset {ProductId} is not available", productId);
            return;
        }

        Log.Information("Fixed asset {ProductId} availability confirmed", productId);
        Log.Information("Depreciation calculation step completed for {ProductId}", productId);
        Log.Information("Fixed asset {ProductId} disposal step completed", productId);
        Log.Information("Business scenario completed");
    }
}
