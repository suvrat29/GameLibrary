using GameLib.api.Infrastructure.Authentication;
using GameLib.dal.ViewModels.Infrastructure;
using GameLib.dal.ViewModels.Infrastructure.Authentication;

namespace GameLib.api.Services.Authentication;

public interface IAuthenticationService
{
    Task<LoginResponseVM?> LoginAsync(LoginRequestVM credentials);
    Task SignOutAsync(UserModel user);
}

internal sealed class AuthenticationService(IUserAuthenticationService userAuthService) : IAuthenticationService
{
    #region IAuthenticationService Implementation

    public Task<LoginResponseVM?> LoginAsync(LoginRequestVM credentials)
    {
        return userAuthService.LoginAsync(credentials.email, credentials.password);
    }

    public Task SignOutAsync(UserModel user)
    {
        return userAuthService.SignOutAsync();
    }

    #endregion
}