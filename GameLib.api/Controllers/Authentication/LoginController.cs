using System.Net;
using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.api.Services.Authentication;
using GameLib.dal.ViewModels.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.Controllers.Authentication;

[Route("[controller]")]
public class LoginController(
    ILogger<LoginController> logger,
    ISessionService sessionService,
    IAuthenticationService authService)
    : BaseController<LoginController>(logger, sessionService)
{
    #region Routes

    #region POST Methods

    [AllowAnonymous]
    [HttpPost(Name = "LoginAsync")]
    public Task<IActionResult> LoginAsync(LoginRequestVM credentials) =>
        CallServiceMethodAsync(() => authService.LoginAsync(credentials), HttpStatusCode.OK, true);

    [HttpPost("logout", Name = "LogoutAsync")]
    public Task<IActionResult> LogoutAsync() => CallServiceMethodAsync(() => authService.SignOutAsync(user!));

    #endregion

    #endregion
}