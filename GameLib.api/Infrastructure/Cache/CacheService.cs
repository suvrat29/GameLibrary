using GameLib.api.Utilities;
using GameLib.dal.Constants.Infrastructure.Cache;

namespace GameLib.api.Infrastructure.Cache;

/// <summary>
/// User specific interface used to fetch data from redis which pertains to the user.
/// </summary>
public interface IUserSpecificCacheService
{
    /// <summary>
    /// Gets a user specific cache item.
    /// </summary>
    /// <param name="key">User specific cache key constant.</param>
    /// <typeparam name="T">Type of the data to be fetched.</typeparam>
    /// <returns>
    /// <para>
    /// </para>
    /// </returns>
    Task<T?> GetFromCacheAsync<T>(string key);

    /// <summary>
    /// Sets an item in cache against a specific user.
    /// </summary>
    /// <param name="key">User specific cache key constant.</param>
    /// <param name="value">Data to store in the cache.</param>
    /// <param name="expiresIn">An optional expiration value. Defaults to the configured value if null.</param>
    /// <typeparam name="T">Type of the data to be stored.</typeparam>
    Task SetInCacheAsync<T>(string key, T value, long? expiresIn = null);

    /// <summary>
    /// Removes a user specific cache item.
    /// </summary>
    /// <param name="key">User specific cache key constant.</param>
    Task RemoveFromCacheAsync(string key);

    bool IsServiceAvailable();
}

internal sealed class CacheService : IUserSpecificCacheService
{
    #region Variables

    private readonly IRedisCacheService? _redisCache;

    #endregion

    #region Properties

    private readonly string USER_CACHE_KEY = "";

    #endregion

    #region Constructor

    public CacheService(IHttpContextAccessor context, IRedisCacheService redisCache)
    {
        if (context.HttpContext?.User.Identity?.IsAuthenticated ?? false)
        {
            Guid userUuid = TokenUtilities.GetUserUuidFromRequest(context);

            if (userUuid != Guid.Empty)
            {
                USER_CACHE_KEY = $"{CacheConstants.USER_CACHE_KEY_PREFIX}{userUuid.ToString()}";
                _redisCache = redisCache;
            }
        }
    }

    public CacheService(IRedisCacheService redisCache, Guid userUuid, bool serviceOverride = false)
    {
        if (userUuid != Guid.Empty && serviceOverride)
        {
            USER_CACHE_KEY = $"{CacheConstants.USER_CACHE_KEY_PREFIX}{userUuid.ToString()}";
            _redisCache = redisCache;
        }
    }

    #endregion

    #region IUserSpecificCacheService Implementation

    public async Task<T?> GetFromCacheAsync<T>(string key)
    {
        if (IsUserSpecificOperationAllowed())
        {
            return await _redisCache!.GetAsync<T>($"{USER_CACHE_KEY}_{key}");
        }
        else
        {
            return default;
        }
    }

    public async Task SetInCacheAsync<T>(string key, T value, long? expiresIn = null)
    {
        if (IsUserSpecificOperationAllowed())
        {
            if (expiresIn.HasValue)
            {
                await _redisCache!.SetAsync($"{USER_CACHE_KEY}_{key}", value, expiresIn.Value);
            }
            else
            {
                await _redisCache!.SetAsync($"{USER_CACHE_KEY}_{key}", value);
            }
        }
        else
        {
            throw new Exception(CacheConstants.USER_CACHE_UNAUTHORIZED_MESSAGE);
        }
    }

    public async Task RemoveFromCacheAsync(string key)
    {
        if (IsUserSpecificOperationAllowed())
        {
            await _redisCache!.RemoveAsync($"{USER_CACHE_KEY}_{key}");
        }
        else
        {
            throw new Exception(CacheConstants.USER_CACHE_UNAUTHORIZED_MESSAGE);
        }
    }

    public bool IsServiceAvailable()
    {
        return _redisCache != null;
    }

    #endregion

    #region Private Methods

    private bool IsUserSpecificOperationAllowed()
    {
        return _redisCache != null && !string.IsNullOrWhiteSpace(USER_CACHE_KEY);
    }

    #endregion
}