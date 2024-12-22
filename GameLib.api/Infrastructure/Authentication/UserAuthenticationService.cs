using GameLib.api.BaseClasses;
using GameLib.dal.ViewModels.Infrastructure.Authentication;
using Supabase;

namespace GameLib.api.Infrastructure.Authentication;

public interface IUserAuthenticationService
{
    Task<LoginResponseVM?> LoginAsync(string username, string password);
    Task SignOutAsync();
}

internal sealed class UserAuthenticationService(ILogger<UserAuthenticationService> logger, Client client)
    : IUserAuthenticationService
{
    #region IUserAuthenticationService Implementation

    public async Task<LoginResponseVM?> LoginAsync(string username, string password)
    {
        try
        {
            var response = await client.Auth.SignIn(username, password);

            if (!string.IsNullOrWhiteSpace(response?.AccessToken) && !string.IsNullOrWhiteSpace(response.RefreshToken))
            {
                await client.Auth.SetSession(response.AccessToken, response.RefreshToken);
                
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

    public Task SignOutAsync()
    {
        return client.Auth.SignOut();
    }

    #endregion
}