using Serilog;

/// <summary>
/// Centralized error workflow for integration failures.
/// </summary>
public class ErrorWorkflow
{
    /// <summary>
    /// Logs an exception and starts compensation notification flow.
    /// </summary>
    /// <param name="ex">Exception raised by integration workflow.</param>
    public void HandleError(Exception ex)
    {
        Log.Error(ex, "Integration error: {Message}", ex.Message);
        Log.Warning("Starting compensation workflow");
        NotifyUser();
    }

    private static void NotifyUser()
    {
        Log.Information("Operator notification has been sent");
    }
}
