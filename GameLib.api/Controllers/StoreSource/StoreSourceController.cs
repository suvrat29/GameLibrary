using System.Net;
using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.api.Services.StoreSource;
using GameLib.dal.Models;
using GameLib.dal.ViewModels.Request.StoreSource;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace GameLib.api.Controllers.StoreSource;

[Route("store-source")]
public class StoreSourceController(
    ILogger<StoreSourceController> logger,
    ISessionService sessionService,
    IStoreSourceService storeSourceService)
    : BaseController<StoreSourceController>(logger, sessionService)
{
    #region Routes

    #region GET Methods

    [HttpGet("get-sources", Name = "GetStoreSourcesAsync")]
    public Task<IActionResult> GetStoreSourcesAsync([FromQuery] int page = 0, [FromQuery] int limit = 100) =>
        CallServiceMethodAsync(() => storeSourceService.GetStoreSourcesAsync(user!, page, limit), HttpStatusCode.OK);

    #endregion

    #region POST Methods

    [HttpPost("add-store-source", Name = "AddStoreSourceAsync")]
    public Task<IActionResult> AddStoreSourceAsync(StoreSourceRequestVM request) =>
        CallServiceMethodAsync(() => storeSourceService.InsertStoreSourceAsync(user!, request), HttpStatusCode.OK);

    [HttpPost("delete-store-source", Name = "DeleteStoreSourceAsync")]
    public Task<IActionResult> DeleteStoreSourceAsync([FromQuery] Guid recordId) =>
        CallServiceMethodAsync(() => storeSourceService.DeleteStoreSourceAsync(user!, recordId), HttpStatusCode.OK);

    #endregion

    #region PATCH Methods

    [HttpPatch("update-store-source", Name = "UpdateStoreSourceAsync")]
    public Task<IActionResult> UpdateStoreSourceAsync([FromQuery] Guid recordId,
        [FromBody] JsonPatchDocument<StoreSources> request) => CallServiceMethodAsync(
        () => storeSourceService.UpdateStoreSourceAsync(user!, recordId, request), HttpStatusCode.OK);

    #endregion

    #endregion
}