using System.Net;
using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.api.Services.Authentication;
using GameLib.dal.ViewModels.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.Controllers.Authentication;

[Route("[controller]")]
public class TokenController(
    ILogger<TokenController> logger,
    ISessionService sessionService,
    IAuthenticationService authService)
    : BaseController<TokenController>(logger, sessionService)
{
    #region Routes

    #region POST Methods

    [HttpPost("refresh", Name = "RefreshTokenAsync")]
    public Task<IActionResult> RefreshTokenAsync(TokenRefreshRequestVM refreshToken) =>
        CallServiceMethodAsync(() => authService.RefreshTokenAsync(user!, refreshToken), HttpStatusCode.OK);

    #endregion

    #endregion
}