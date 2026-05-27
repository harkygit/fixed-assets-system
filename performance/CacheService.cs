using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class CacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(
        IDistributedCache cache
    )
    {
        _cache = cache;
    }

    public async Task<string> GetProducts()
    {
        var cached =
            await _cache.GetStringAsync(
                "products"
            );

        if (cached != null)
        {
            return cached;
        }

        // Имитация получения данных

        var products =
            "[{\"id\":1,\"name\":\"Ноутбук\"}]";

        await _cache.SetStringAsync(

            "products",

            JsonSerializer.Serialize(products),

            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromMinutes(5)
            }
        );

        return products;
    }
}