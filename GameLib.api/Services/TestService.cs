using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.dal.Models;
using GameLib.dal.ViewModels.Infrastructure;
using GameLib.dal.ViewModels.Request;
using GameLib.dal.ViewModels.Response;

namespace GameLib.api.Services;

public interface ITestService
{
    Task<TestResponse?> GetItemByIdAsync(UserModel user, Guid id);
    Task<Guid?> CreateItemAsync(UserModel user, CreateTestRequest request);
    Task DeleteItemByIdAsync(UserModel user, Guid id);
}

internal sealed class TestService(SupabaseClientService clientService) : BaseService(clientService), ITestService
{
    #region ITestService Implementation

    public async Task<TestResponse?> GetItemByIdAsync(UserModel user, Guid id)
    {
        var tableData = await _client.From<TestTable>()
            .Where(t => t.Uuid == id)
            .Get();

        var data = tableData.Models.FirstOrDefault();

        if (data == null) return null;

        return new()
        {
            Id = data.Uuid,
            Name = data.Name,
            Description = data.Description,
            ReadTime = data.ReadTime,
            CreatedAt = data.CreatedAt,
        };
    }

    public async Task<Guid?> CreateItemAsync(UserModel user, CreateTestRequest request)
    {
        var data = new TestTable
        {
            Name = request.Name,
            Description = request.Description,
            ReadTime = request.ReadTime,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Uuid,
        };

        var response = await _client.From<TestTable>().Insert(data);

        var newNewsLetter = response.Models.First();
        return newNewsLetter.Uuid;
    }

    public Task DeleteItemByIdAsync(UserModel user, Guid id)
    {
        return _client
            .From<TestTable>()
            .Where(t => t.Uuid == id)
            .Delete();
    }

    #endregion
}