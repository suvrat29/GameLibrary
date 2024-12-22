using GameLib.api.BaseClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.Controllers.Authentication;

[AllowAnonymous]
[Route("[controller]")]
public class LoginController(ILogger<LoginController> logger) : BaseController<LoginController>(logger)
{
    #region Routes

    #region POST Methods

    // public async Task<IActionResult> LoginAsync(LoginRequest credentials)
    // {
    //     
    // }

    #endregion

    #endregion
}