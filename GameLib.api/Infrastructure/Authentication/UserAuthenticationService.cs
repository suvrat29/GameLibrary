using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Cache;
using GameLib.api.Infrastructure.Session;
using GameLib.dal.Constants.Infrastructure.Cache;
using GameLib.dal.ViewModels.Infrastructure.Authentication;

namespace GameLib.api.Infrastructure.Authentication;

public interface IUserAuthenticationService
{
    Task<AuthenticationResponseVM?> LoginAsync(string username, string password);
    Task SignOutAsync(Guid userUuid);
    Task<AuthenticationResponseVM?> RefreshTokenAsync(TokenRefreshRequestVM refreshToken);
}

internal sealed class UserAuthenticationService(
    ILogger<UserAuthenticationService> logger,
    IRedisCacheService redisCache,
    SupabaseClientService clientService) : BaseService(clientService), IUserAuthenticationService
{
    #region IUserAuthenticationService Implementation

    public async Task<AuthenticationResponseVM?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _client.Auth.SignIn(username, password);

            if (!string.IsNullOrWhiteSpace(response?.AccessToken) &&
                !string.IsNullOrWhiteSpace(response.RefreshToken) && response.User != null)
            {
                await _client.Auth.SetSession(response.AccessToken, response.RefreshToken);

                //Since the cache service is unavailable at this point due to the request being from an unauthorized user
                //Create a new instance and store data for the authenticated user
                CacheService cache = new(redisCache, Guid.Parse(response.User.Id!), true);

                await cache.SetInCacheAsync(UserSpecificCacheConstants.USER_AUTH_SESSION, _client.Auth.CurrentSession);
                logger.LogInformation("Session stored in cache.");

                return new()
                {
                    access_token = response.AccessToken,
                    token_type = "Bearer",
                    expires_in = response.ExpiresIn,
                    expires_at = response.ExpiresAt(),
                    refresh_token = response.RefreshToken
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }

        return null;
    }

    public async Task SignOutAsync(Guid userUuid)
    {
        await _client.Auth.SignOut();
        await RemoveUserSpecificCacheItemsAsync(userUuid);
    }

    public async Task<AuthenticationResponseVM?> RefreshTokenAsync(TokenRefreshRequestVM refreshToken)
    {
        try
        {
            logger.LogInformation($"Old refresh token: {refreshToken.refresh_token}");
            await _client.Auth.RefreshToken();
            var session = _client.Auth.CurrentSession;

            if (!string.IsNullOrWhiteSpace(session?.AccessToken) && !string.IsNullOrWhiteSpace(session.RefreshToken))
            {
                logger.LogInformation($"New refresh token: {session.RefreshToken}");
                CacheService cache = new(redisCache, Guid.Parse(session.User!.Id!), true);

                await cache.SetInCacheAsync(UserSpecificCacheConstants.USER_AUTH_SESSION, session);
                logger.LogInformation("Session updated in cache.");

                return new()
                {
                    access_token = session.AccessToken,
                    token_type = "Bearer",
                    expires_in = session.ExpiresIn,
                    expires_at = session.ExpiresAt(),
                    refresh_token = session.RefreshToken
                };
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }

        return null;
    }

    #endregion

    #region Private Methods

    private async Task RemoveUserSpecificCacheItemsAsync(Guid userUuid)
    {
        //Since the cache service is unavailable at this point due to the request being from an unauthorized user
        //Create a new instance and store data for the authenticated user
        CacheService cache = new(redisCache, userUuid, true);
        var profileItemTask = cache.RemoveFromCacheAsync(UserSpecificCacheConstants.USER_PROFILE);
        var authItemTask = cache.RemoveFromCacheAsync(UserSpecificCacheConstants.USER_AUTH_SESSION);
        await Task.WhenAll(profileItemTask, authItemTask);
    }

    #endregion
}