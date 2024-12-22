using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.dal.Models;
using GameLib.dal.ViewModels.Infrastructure;
using GameLib.dal.ViewModels.Request;
using GameLib.dal.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supabase;

namespace GameLib.api.Controllers;

[Authorize]
[Route("[controller]")]
public class TestController(ILogger<TestController> logger, ISessionService sessionService, Client client)
    : BaseController<TestController>(logger)
{
    [HttpGet("{id}", Name = "GetTestByIdAsync")]
    public async Task<IActionResult> GetAsync(long id)
    {
        UserModel? user = sessionService.GetUser();

        if (user == null)
        {
            return Unauthorized();
        }

        var tableData = await client.From<TestTable>()
            .Where(t => t.Id == id)
            .Get();

        var data = tableData.Models.FirstOrDefault();

        if (data == null)
        {
            return NotFound();
        }

        var response = new TestResponse
        {
            Id = data.Id,
            Name = data.Name,
            Description = data.Description,
            ReadTime = data.ReadTime,
            CreatedAt = data.CreatedAt,
        };

        return Ok(response);
    }

    [HttpPost(Name = "PostTestDataAsync")]
    public async Task<IActionResult> PostAsync(CreateTestRequest request)
    {
        UserModel? user = sessionService.GetUser();

        if (user == null)
        {
            return Unauthorized();
        }

        var data = new TestTable
        {
            Name = request.Name,
            Description = request.Description,
            ReadTime = request.ReadTime,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Uuid,
        };

        var response = await client.From<TestTable>().Insert(data);

        var newNewsLetter = response.Models.First();

        return Ok(newNewsLetter.Id);
    }

    [HttpDelete("{id}", Name = "DeleteTestByIdAsync")]
    public async Task<IActionResult> DeleteAsync(long id)
    {
        UserModel? user = sessionService.GetUser();

        if (user == null)
        {
            return Unauthorized();
        }

        await client
            .From<TestTable>()
            .Where(t => t.Id == id)
            .Delete();

        return NoContent();
    }
}