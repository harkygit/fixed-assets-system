public class AsyncIntegrationService
{
    private readonly HttpClient _httpClient;

    public AsyncIntegrationService(
        HttpClient httpClient
    )
    {
        _httpClient = httpClient;
    }

    public async Task ExecuteAsync()
    {
        var stockTask =
            _httpClient.GetAsync(
                "http://localhost:5001/objects"
            );

        var depreciationTask =
            _httpClient.PostAsync(
                "http://localhost:5002/depreciation/calculate",
                null
            );

        await Task.WhenAll(
            stockTask,
            depreciationTask
        );
    }
}