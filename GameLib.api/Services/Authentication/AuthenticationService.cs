using GameLib.api.Infrastructure.Authentication;
using GameLib.dal.ViewModels.Infrastructure;
using GameLib.dal.ViewModels.Infrastructure.Authentication;

namespace GameLib.api.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResponseVM?> LoginAsync(AuthenticationRequestVM credentials);
    Task SignOutAsync(UserModel user);
    Task<AuthenticationResponseVM?> RefreshTokenAsync(UserModel user, TokenRefreshRequestVM refreshToken);
}

internal sealed class AuthenticationService(IUserAuthenticationService userAuthService) : IAuthenticationService
{
    #region IAuthenticationService Implementation

    public Task<AuthenticationResponseVM?> LoginAsync(AuthenticationRequestVM credentials) =>
        userAuthService.LoginAsync(credentials.email, credentials.password);

    public Task SignOutAsync(UserModel user) => userAuthService.SignOutAsync(user.Uuid);

    public Task<AuthenticationResponseVM?> RefreshTokenAsync(UserModel user, TokenRefreshRequestVM refreshToken) =>
        userAuthService.RefreshTokenAsync(refreshToken);

    #endregion
}