using BlogApi.Models.User;
using MongoDB.Driver;

namespace BlogApi.Services.User;

public interface IUsersService
{
    Task<UserLookupRoles?> GetByNameAsync(string username);
    Task<UserLookupRoles?> GetAsync(string username, string password);
    Task<UserLookupRoles?> GetAsync(string id);
    Task<(IReadOnlyList<UserLookupRoles> Data, long TotalPages, long Count)> GetAsync(long skip, long limit, SortDefinition<UserModel> sort, FilterDefinition<UserModel> filter);
    Task<List<UserLookupRoles>> GetAsync();
    Task CreateAsync(UserModel userModel);
    Task UpdateAsync(string id, UpdateDefinition<UserModel> updatedUser);
    Task RemoveAsync(string id);
}
