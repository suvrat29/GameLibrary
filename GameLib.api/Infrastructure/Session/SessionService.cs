using GameLib.api.Infrastructure.Cache;
using GameLib.api.Utilities;
using GameLib.dal.Constants.Infrastructure.Cache;
using GameLib.dal.ViewModels.Infrastructure;

namespace GameLib.api.Infrastructure.Session;

public interface ISessionService
{
    /// <summary>
    /// Gets details about the current user. 
    /// </summary>
    /// <param name="skipCache">If true, skips fetching from cache as the first step.</param>
    /// <returns>User details if found.
    /// <para>
    /// null if not found.
    /// </para>
    /// </returns>
    Task<UserModel?> GetUserAsync(bool skipCache = false);
}

internal sealed class SessionService(IHttpContextAccessor context, IUserSpecificCacheService cacheService)
    : ISessionService
{
    #region Variables

    private readonly IHttpContextAccessor _context = context;
    private readonly IUserSpecificCacheService _cacheService = cacheService;

    #endregion

    #region ISessionService Implementation

    public async Task<UserModel?> GetUserAsync(bool skipCache = false)
    {
        if (Convert.ToBoolean(_context.HttpContext?.User.Identity?.IsAuthenticated))
        {
            UserModel? user = skipCache
                ? null
                : await _cacheService.GetFromCacheAsync<UserModel>(UserSpecificCacheConstants.USER_PROFILE) ?? null;

            if (user != null)
            {
                return user;
            }
            else
            {
                (string, Guid) userDetails = TokenUtilities.GetUserDetailsFromRequest(_context);

                if (!string.IsNullOrWhiteSpace(userDetails.Item1) && userDetails.Item2 != Guid.Empty)
                {
                    user = new()
                    {
                        Email = userDetails.Item1,
                        Uuid = userDetails.Item2
                    };

                    await _cacheService.SetInCacheAsync(
                        UserSpecificCacheConstants.USER_PROFILE, user);

                    return user;
                }
            }
        }

        return null;
    }

    #endregion
}