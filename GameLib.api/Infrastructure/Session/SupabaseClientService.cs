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
            Supabase.Gotrue.Session? existingSession = _cacheService.IsServiceAvailable()
                ? await _cacheService.GetFromCacheAsync<Supabase.Gotrue.Session?>(
                    UserSpecificCacheConstants.USER_AUTH_SESSION)
                : null;

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
        //TODO: This might be needed when being accessed in conjunction with the client library
        _supabaseClient.Auth.AddStateChangedListener((_, changed) =>
        {
            switch (changed)
            {
                case Constants.AuthState.SignedIn: logger.LogInformation("User signed in"); break;
                case Constants.AuthState.SignedOut: logger.LogInformation("User signed out"); break;
                case Constants.AuthState.UserUpdated: logger.LogInformation("User updated"); break;
                case Constants.AuthState.PasswordRecovery: logger.LogInformation("Password recovery"); break;
                case Constants.AuthState.TokenRefreshed: logger.LogInformation("Token refreshed"); break;
            }
        });
    }
}