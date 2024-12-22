using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.BaseClasses;

[ApiController]
public class BaseController<T>(ILogger<T> logger) : ControllerBase where T : BaseController<T>
{
    private ILogger<T> _logger = logger;

    protected ILogger<T> Logger => _logger ??= HttpContext.RequestServices.GetRequiredService<ILogger<T>>();
}