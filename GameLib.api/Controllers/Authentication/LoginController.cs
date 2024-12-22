using GameLib.api.BaseClasses;
using GameLib.dal.ViewModels.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.Controllers.Authentication;

[AllowAnonymous]
[Route("[controller]")]
public class LoginController(ILogger<LoginController> logger) : BaseController<LoginController>(logger, null)
{
    #region Routes

    #region POST Methods

    // public async Task<IActionResult> LoginAsync(LoginRequestVM credentials)
    // {
    //     
    // }

    #endregion

    #endregion
}