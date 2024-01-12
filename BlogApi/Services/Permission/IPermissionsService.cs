using BlogApi.Models.Permission;
using MongoDB.Driver;

namespace BlogApi.Services.Permission;

public interface IPermissionsService
{
    Task<List<PermissionModel>> GetAllAsync();
    Task<(IReadOnlyList<PermissionModel> Data, long TotalPages, long Count)> GetAsync(long skip, long limit, SortDefinition<PermissionModel> sort, FilterDefinition<PermissionModel> filter);
    Task<PermissionModel?> GetByIdAsync(string id);
    Task<PermissionModel?> GetByKeyAsync(string key);
    Task CreateAsync(PermissionModel permissionModel);
    Task UpdateAsync(string id, UpdateDefinition<PermissionModel> permissionModel);
    Task UpdateManyAsync(FilterDefinition<PermissionModel> filter, UpdateDefinition<PermissionModel> permissions);
    Task RemoveAsync(string id);
}
