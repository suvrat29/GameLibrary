using GameLib.api.BaseClasses;
using GameLib.api.Infrastructure.Session;
using GameLib.dal.Constants.Database;
using GameLib.dal.Constants.HttpConstants;
using GameLib.dal.Enums;
using GameLib.dal.Models;
using GameLib.dal.ViewModels.Infrastructure;
using GameLib.dal.ViewModels.Request.StoreSource;
using GameLib.dal.ViewModels.Response.StoreSource;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Supabase.Postgrest;

namespace GameLib.api.Services.StoreSource;

public interface IStoreSourceService
{
    Task<List<StoreSourceResponseVM>> GetStoreSourcesAsync(UserModel user, int page, int limit);

    Task<Guid> InsertStoreSourceAsync(UserModel user, StoreSourceRequestVM request);

    Task<BasePatchResponse<StoreSourceResponseVM>> UpdateStoreSourceAsync(UserModel user, Guid recordId,
        JsonPatchDocument<StoreSources> patch);

    Task<bool> DeleteStoreSourceAsync(UserModel user, Guid recordId);
}

internal sealed class StoreSourceService(ILogger<StoreSourceService> logger, SupabaseClientService clientService)
    : BaseService(clientService), IStoreSourceService
{
    #region IStoreSourceService Implementation

    public async Task<List<StoreSourceResponseVM>> GetStoreSourcesAsync(UserModel user, int page, int limit)
    {
        var tableData = await _client.From<StoreSources>()
            .Where(item => item.CreatedBy == user.Uuid && item.Deleted == false)
            .Range(page, limit)
            .Limit(limit)
            .Order(item => item.Name, Constants.Ordering.Ascending)
            .Get();

        var data = tableData.Models;

        if (data.Count == 0) return new();

        return data.ConvertAll(item => new StoreSourceResponseVM
        {
            Id = item.Uuid,
            Name = item.Name,
            Icon = item.Icon,
        });
    }

    public async Task<Guid> InsertStoreSourceAsync(UserModel user, StoreSourceRequestVM request)
    {
        //TODO: Make sure that if records > 100, then this doesn't fail validation
        List<StoreSourceResponseVM> existingStoreSources = await GetStoreSourcesAsync(user, 0, 100);

        if (existingStoreSources.Any(x => x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return existingStoreSources.First(x => x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)).Id;
        }
        
        var data = new StoreSources
        {
            Name = request.Name,
            Icon = request.Icon,
            CreatedBy = user.Uuid,
            CreatedOn = DateTime.UtcNow
        };

        var response = await _client.From<StoreSources>()
            .Insert(data);
        var newStoreSource = response.Models.First();

        try
        {
            await CreateAuditRecordTrailAsync(DatabaseAuditEnums.Create, newStoreSource.Uuid, user.Uuid,
                new StoreSourceRequestVM { Name = request.Name, Icon = request.Icon });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            await _client.From<StoreSources>()
                .Where(x => x.Id == newStoreSource.Id)
                .Delete();

            throw;
        }

        return newStoreSource.Uuid;
    }

    public async Task<BasePatchResponse<StoreSourceResponseVM>> UpdateStoreSourceAsync(UserModel user, Guid recordId,
        JsonPatchDocument<StoreSources> patch)
    {
        BasePatchResponse<StoreSourceResponseVM> patchResult = new();

        if (patch.Operations.Count == 0)
        {
            patchResult.Success = false;
            patchResult.Message = PatchConstants.PATCH_EMPTY_OPERATION;
            return patchResult;
        }
        
        if (patch.Operations.All(op => op.OperationType != OperationType.Replace))
        {
            patchResult.Success = false;
            patchResult.Message = PatchConstants.PATCH_OPERATION_DISALLOWED;
            return patchResult;
        }

        var data = await _client.From<StoreSources>()
            .Where(item => item.Uuid == recordId)
            .Where(item => item.CreatedBy == user.Uuid)
            .Where(item => item.Deleted == false)
            .Single();

        if (data == null)
        {
            patchResult.Success = false;
            patchResult.Message = DatabaseOperationConstants.RECORD_UPDATE_NOT_FOUND;
            return patchResult;
        }

        StoreSourceResponseVM originalRecord = new()
        {
            Id = data.Uuid,
            Name = data.Name,
            Icon = data.Icon
        };

        StoreSources toUpdateTableRecord = data;

        patch.ApplyTo(toUpdateTableRecord);
        var updatedData = await toUpdateTableRecord.Update<StoreSources>();
        
        if (updatedData.Models.Count == 0)
        {
            patchResult.Success = false;
            patchResult.Message = PatchConstants.PATCH_FAILURE_EXCEPTION;
            return patchResult;
        }
        
        var updatedStoreSource = updatedData.Models.First();

        try
        {
            await CreateAuditRecordTrailAsync(DatabaseAuditEnums.Update, updatedStoreSource.Uuid, user.Uuid,
                new StoreSourceRequestVM { Name = updatedStoreSource.Name, Icon = updatedStoreSource.Icon });
            patchResult.Original = originalRecord;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            //calling patch.ApplyTo() mutates both toUpdateTableRecord and data variables, so the original values need to be restored here
            data.Name = originalRecord.Name;
            data.Icon = originalRecord.Icon;
            await data.Update<StoreSources>();
            
            patchResult.Success = false;
            patchResult.Message = PatchConstants.PATCH_FAILURE_EXCEPTION;
            return patchResult;
        }

        patchResult.Patched = new()
        {
            Id = updatedStoreSource.Uuid,
            Name = updatedStoreSource.Name,
            Icon = updatedStoreSource.Icon
        };
        patchResult.Success = true;
        patchResult.Message = PatchConstants.PATCH_SUCCESS;

        return patchResult;
    }

    public async Task<bool> DeleteStoreSourceAsync(UserModel user, Guid recordId)
    {
        var data = await _client.From<StoreSources>()
            .Where(item => item.Uuid == recordId)
            .Where(item =>item.CreatedBy == user.Uuid)
            .Where(item => item.Deleted == false)
            .Single();

        if (data == null) return false;

        data.Deleted = true;

        long auditTrailId;

        try
        {
            auditTrailId = await CreateAuditRecordTrailAsync(DatabaseAuditEnums.Delete, recordId, user.Uuid,
                new StoreSourceRequestVM { Name = data.Name, Icon = data.Icon });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return false;
        }

        try
        {
            var updateResult = await data.Update<StoreSources>();

            if (updateResult.Models.Count == 0)
            {
                throw new Exception(DatabaseOperationConstants.RECORD_UPDATE_FAILED);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            await _client.From<StoreSourcesAudit>()
                .Where(x => x.Id == auditTrailId)
                .Delete();

            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods

    private async Task<long> CreateAuditRecordTrailAsync(DatabaseAuditEnums operation, Guid recordId, Guid userUuid,
        StoreSourceRequestVM operationItem)
    {
        var auditObject = new StoreSourcesAudit()
        {
            StoreSourcesUuid = recordId,
            Name = operationItem.Name,
            Icon = operationItem.Icon,
            CreatedBy = userUuid,
            CreatedOn = DateTime.UtcNow
        };

        switch (operation)
        {
            case DatabaseAuditEnums.Create:
                auditObject.Modified = false;
                auditObject.Deleted = false;
                break;
            case DatabaseAuditEnums.Update:
                auditObject.Modified = true;
                auditObject.Deleted = false;
                break;
            case DatabaseAuditEnums.Delete:
                auditObject.Modified = false;
                auditObject.Deleted = true;
                break;
            default:
                throw new Exception(DatabaseOperationConstants.DISALLOWED_AUDIT_OPERATION);
        }

        var insertedRecord = await _client.From<StoreSourcesAudit>()
            .Insert(auditObject);
        return insertedRecord.Models.First().Id;
    }

    #endregion
}