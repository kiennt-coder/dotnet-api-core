using BlogApi.Models.Role;
using MongoDB.Driver;

namespace BlogApi.Services.Role;

public interface IRolesService
{
    Task<List<RoleLookupPermissions>> GetAsync();
    Task<(List<RoleLookupPermissions> Data, long TotalPages, long Count)> GetAsync(long skip, long limit, FilterDefinition<RoleModel> filter);
    Task<RoleLookupPermissions?> GetByIdAsync(string id);
    Task<RoleLookupPermissions?> GetByKeyAsync(string key);
    Task CreateAsync(RoleModel roleModel);
    Task UpdateAsync(string id, UpdateDefinition<RoleModel> updatedRole);
    Task RemoveAsync(string id);
}
