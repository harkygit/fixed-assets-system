using System.Diagnostics;
using Serilog;

public class PerformanceTests
{
    public async Task MeasureExecutionTime()
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        // Имитация интеграционного процесса

        await Task.Delay(2300);

        stopwatch.Stop();

        Log.Information(
            "Время выполнения: {Time} ms",
            stopwatch.ElapsedMilliseconds
        );
    }
}