using System.Net;
using GameLib.api.Infrastructure.Session;
using GameLib.dal.Constants.Infrastructure.Product;
using GameLib.dal.ViewModels.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.BaseClasses;

[ApiController]
public class BaseController<T>(ILogger<T> logger, ISessionService sessionService)
    : ControllerBase where T : BaseController<T>
{
    #region Variables

    private ILogger<T> _logger = logger;
    private ISessionService _sessionService = sessionService;
    protected UserModel? user;

    #endregion

    #region Executor Methods

    public async Task<IActionResult> CallServiceMethodAsync(Func<Task> method,
        bool anonymousCall = false)
    {
        bool isErrored = false;

        if (!anonymousCall && user == null)
        {
            user = await FetchUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }
        }

        try
        {
            await method();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            isErrored = true;
        }

        if (isErrored)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        return Ok();
    }

    public async Task<IActionResult> CallServiceMethodAsync<U>(Func<Task<U>> method, HttpStatusCode expectedStatusCode,
        bool anonymousCall = false)
    {
        bool isErrored = false;
        U? result = default;

        if (!anonymousCall && user == null)
        {
            user = await FetchUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }
        }

        try
        {
            result = await method();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            isErrored = true;
        }

        if (isErrored)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        switch (expectedStatusCode)
        {
            case HttpStatusCode.OK:
                if (result is null) return NotFound();
                return Ok(result);
            case HttpStatusCode.Created:
                if (result is null) return StatusCode((int)HttpStatusCode.InternalServerError);
                return Created(ProductConstants.BaseCreatedResponse, result);
            case HttpStatusCode.Accepted:
                return StatusCode((int)HttpStatusCode.Accepted);
            case HttpStatusCode.NoContent:
                return NoContent();
            default:
                return Ok(result);
        }
    }

    #endregion

    #region Helper Methods

    private Task<UserModel?> FetchUserAsync() => _sessionService.GetUserAsync();

    #endregion
}