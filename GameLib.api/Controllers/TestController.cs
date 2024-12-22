using System.Net;
using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.api.Services;
using GameLib.dal.ViewModels.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.Controllers;

[Authorize]
[Route("[controller]")]
public class TestController(ILogger<TestController> logger, ISessionService sessionService, ITestService testService)
    : BaseController<TestController>(logger, sessionService)
{
    #region Variables

    private readonly ITestService _testService = testService;

    #endregion

    #region Routes

    #region GET Methods

    [HttpGet("{id}", Name = "GetTestByIdAsync")]
    public Task<IActionResult> GetAsync(Guid id) =>
        CallServiceMethodAsync(() => _testService.GetItemByIdAsync(user!, id), HttpStatusCode.OK);

    #endregion

    #region POST Methods

    [HttpPost(Name = "PostTestDataAsync")]
    public Task<IActionResult> PostAsync(CreateTestRequest request) =>
        CallServiceMethodAsync(() => _testService.CreateItemAsync(user!, request), HttpStatusCode.Created);

    #endregion

    #region DELETE Methods

    [HttpDelete("{id}", Name = "DeleteTestByIdAsync")]
    public Task<IActionResult> DeleteAsync(Guid id) =>
        CallServiceMethodAsync(() => _testService.DeleteItemByIdAsync(user!, id));

    #endregion

    #endregion
}