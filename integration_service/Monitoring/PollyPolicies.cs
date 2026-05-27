using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace FixedAssets.Integration.Monitoring;

/// <summary>
/// Shared resilience policies for inter-service HTTP calls.
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Exponential retry policy for transient network and HTTP errors.
    /// </summary>
    public static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(response => (int)response.StatusCode >= 500)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    /// <summary>
    /// Circuit breaker policy for protecting downstream services during repeated failures.
    /// </summary>
    public static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> CircuitBreakerPolicy =
        Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(response => (int)response.StatusCode >= 500)
            .CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(30));
}
