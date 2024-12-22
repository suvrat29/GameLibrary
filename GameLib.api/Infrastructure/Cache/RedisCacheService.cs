using System.Text.Json;
using GameLib.dal.Constants.Infrastructure.Product;
using StackExchange.Redis;

namespace GameLib.api.Infrastructure.Cache;

public interface IRedisCacheService
{
    /// <summary>
    /// Gets an item from redis cache.
    /// </summary>
    /// <param name="key">Cache key for the item to be fetched.</param>
    /// <typeparam name="T">Type of the item to be fetched.</typeparam>
    /// <returns>
    /// <para>The requested data from cache (if found).</para>
    /// <para>null (if not found).</para>
    /// </returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets an item in redis cache using default configured expiration time.
    /// </summary>
    /// <param name="key">Cache key for the item to be stored.</param>
    /// <param name="value">Data for the item to be stored.</param>
    /// <typeparam name="T">Type of the item to be stored.</typeparam>
    Task SetAsync<T>(string key, T value);

    /// <summary>
    /// Sets an item in redis cache using a custom expiration time.
    /// </summary>
    /// <param name="key">Cache key for the item to be stored.</param>
    /// <param name="value">Data for the item to be stored.</param>
    /// <param name="expiresIn">Expiration time (in seconds) for the item to be stored.</param>
    /// <typeparam name="T">Type of the item to be stored.</typeparam>
    Task SetAsync<T>(string key, T value, long expiresIn);

    /// <summary>
    /// Deletes an item in redis cache.
    /// </summary>
    /// <param name="key">Cache key for the item to be removed.</param>
    Task RemoveAsync(string key);
}

internal sealed class RedisCacheService(
    ILogger<RedisCacheService> logger,
    IConfiguration config,
    IConnectionMultiplexer muxer)
    : IRedisCacheService
{
    #region Variables

    private readonly IDatabase _redis = muxer.GetDatabase();

    #endregion

    #region Properties

    private readonly string PRODUCT_NAME = ProductConstants.ProductName;

    private readonly long DEFAULT_CACHE_EXPIRATION =
        config.GetSection("Cache").GetValue<long>("DefaultItemExpirySeconds");

    #endregion

    #region IRedisCacheService Implementation

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            string? cacheItem = await _redis.StringGetAsync($"{PRODUCT_NAME}_{key}");

            if (!string.IsNullOrWhiteSpace(cacheItem))
            {
                return JsonSerializer.Deserialize<T>(cacheItem);
            }

            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return default;
        }
    }

    public Task SetAsync<T>(string key, T value)
    {
        try
        {
            var setTask = _redis.StringSetAsync($"{PRODUCT_NAME}_{key}", JsonSerializer.Serialize(value));
            var expireTask =
                _redis.KeyExpireAsync($"{PRODUCT_NAME}_{key}", TimeSpan.FromSeconds(DEFAULT_CACHE_EXPIRATION));
            return Task.WhenAll(setTask, expireTask);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Task.CompletedTask;
        }
    }

    public Task SetAsync<T>(string key, T value, long expiresIn)
    {
        try
        {
            var setTask = _redis.StringSetAsync($"{PRODUCT_NAME}_{key}", JsonSerializer.Serialize(value));
            var expireTask = _redis.KeyExpireAsync($"{PRODUCT_NAME}_{key}", TimeSpan.FromSeconds(expiresIn));
            return Task.WhenAll(setTask, expireTask);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            return _redis.KeyDeleteAsync($"{PRODUCT_NAME}_{key}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Task.CompletedTask;
        }
    }

    #endregion
}