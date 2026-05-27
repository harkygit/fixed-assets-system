using Serilog;

/// <summary>
/// Demonstrates Saga orchestration and compensation for fixed asset operations.
/// </summary>
public class SagaOrchestrator
{
    /// <summary>
    /// Executes a demo Saga and starts compensation on failure.
    /// </summary>
    public void ExecuteSaga()
    {
        try
        {
            Log.Information("Saga started for fixed asset disposal");

            const bool depreciationSuccess = true;
            if (!depreciationSuccess)
            {
                throw new InvalidOperationException("Depreciation calculation failed");
            }

            Log.Information("Saga completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Saga failed: {Message}", ex.Message);
            Compensate();
        }
    }

    private static void Compensate()
    {
        Log.Warning("Starting Saga compensation");
        Log.Warning("Fixed asset operation changes were reverted");
    }
}
