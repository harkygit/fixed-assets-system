using Serilog;

namespace FixedAssets.Integration.Logging;

/// <summary>
/// Serilog bootstrap configuration for IntegrationService.
/// </summary>
public static class LoggerConfig
{
    /// <summary>
    /// Configures console and rolling-file structured logging.
    /// </summary>
    public static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "FixedAssetsSystem")
            .WriteTo.Console()
            .WriteTo.File(
                "logs/log.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}
