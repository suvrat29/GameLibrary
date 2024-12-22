using GameLib.api.Infrastructure.Cache;
using GameLib.dal.Constants.Infrastructure.Cache;
using Supabase.Gotrue;
using Client = Supabase.Client;

namespace GameLib.api.Infrastructure.Session;

internal sealed class SupabaseClientService(
    ILogger<SupabaseClientService> logger,
    Client supabaseClient,
    IUserSpecificCacheService cacheService)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly IUserSpecificCacheService _cacheService = cacheService;

    public Client Client => _supabaseClient;

    public async Task InitializeAsync()
    {
        if (_supabaseClient.Auth.CurrentSession == null || _supabaseClient.Auth.CurrentUser == null)
        {
            Supabase.Gotrue.Session? existingSession =
                await _cacheService.GetFromCacheAsync<Supabase.Gotrue.Session?>(
                    UserSpecificCacheConstants.USER_AUTH_SESSION);

            if (existingSession is null)
            {
                await _supabaseClient.InitializeAsync();
                logger.LogInformation("No cached session found. Initialized new session.");
            }

            if (!string.IsNullOrWhiteSpace(existingSession?.AccessToken) &&
                !string.IsNullOrWhiteSpace(existingSession.RefreshToken))
            {
                await _supabaseClient.Auth.SetSession(existingSession.AccessToken, existingSession.RefreshToken);
                logger.LogInformation("Session restored from cache.");
            }
        }
    }

    public void AddAuthStateChangedListener()
    {
        _supabaseClient.Auth.AddStateChangedListener(async (sender, changed) =>
        {
            logger.LogInformation($"Auth state changed to: {changed}");
            switch (changed)
            {
                case Constants.AuthState.SignedIn:
                    logger.LogInformation("User signed in");
                    var session = _supabaseClient.Auth.CurrentSession;
                    if (session != null)
                    {
                        await _cacheService.SetInCacheAsync(UserSpecificCacheConstants.USER_AUTH_SESSION, session);
                    }

                    break;
                case Constants.AuthState.SignedOut:
                    logger.LogInformation("User signed out");
                    if (_supabaseClient.Auth.CurrentSession != null)
                    {
                        await _cacheService.RemoveFromCacheAsync(UserSpecificCacheConstants.USER_AUTH_SESSION);
                    }

                    break;
                case Constants.AuthState.UserUpdated: logger.LogInformation("User updated"); break;
                case Constants.AuthState.PasswordRecovery: logger.LogInformation("Password recovery"); break;
                case Constants.AuthState.TokenRefreshed: logger.LogInformation("Token refreshed"); break;
            }
        });
    }
}