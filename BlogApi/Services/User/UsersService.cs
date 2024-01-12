using BlogApi.Helpers.ConvertData;
using BlogApi.Models.Permission;
using BlogApi.Models.Role;
using BlogApi.Models.User;
using BlogApi.Services.Database.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;


namespace BlogApi.Services.User;

public class UsersService : IUsersService
{
    private readonly IMongoCollection<UserModel> _usersCollection;
    private readonly IMongoCollection<RoleModel> _rolesCollection;
    private readonly IMongoCollection<PermissionModel> _permissionCollection;

    public UsersService(IBlogDatabase blogDatabase)
    {
        _usersCollection = blogDatabase.UserCollection;
        _rolesCollection = blogDatabase.RoleCollection;
        _permissionCollection = blogDatabase.PermissionCollection;
    }

    public async Task<UserLookupRoles?> GetByNameAsync(string username) =>
        await UserLookupRolesAsync(Builders<UserModel>.Filter.Where(x => x.Username == username));

    public async Task<UserLookupRoles?> GetAsync(string username, string password) =>
        await UserLookupRolesAsync(Builders<UserModel>.Filter.Where(x => x.Username == username && x.Password == password));

    public async Task<List<UserLookupRoles>> GetAsync() =>
        await UsersLookupRolesAsync(Builders<UserModel>.Filter.Where(_ => true));

    public async Task<(IReadOnlyList<UserLookupRoles> Data, long TotalPages, long Count)> GetAsync(long skip, long limit, SortDefinition<UserModel> sort, FilterDefinition<UserModel> filter)
    {
        var CountFacet = AggregateFacet.Create("count",
            PipelineDefinition<UserModel, AggregateCountResult>.Create(new List<IPipelineStageDefinition> {
                PipelineStageDefinitionBuilder.Count<UserModel>()
            })
        );

        var DataFacet = AggregateFacet.Create("data",
            PipelineDefinition<UserModel, UserLookupRoles>.Create(new List<IPipelineStageDefinition> {
                PipelineStageDefinitionBuilder.Sort(sort),
                PipelineStageDefinitionBuilder.Skip<UserModel>(skip),
                PipelineStageDefinitionBuilder.Limit<UserModel>(limit),
                PipelineStageDefinitionBuilder.Lookup<UserModel, RoleModel, UserLookupRoles>(
                    foreignCollection: _rolesCollection,
                    localField: x => x.RoleIds,
                    foreignField: x => x.Id,
                    @as: x => x.Roles
                ),
                PipelineStageDefinitionBuilder.Unwind<UserLookupRoles, UserLookupRole>(x => x.Roles,
                    new AggregateUnwindOptions<UserLookupRole> {PreserveNullAndEmptyArrays = true}
                ),
                PipelineStageDefinitionBuilder.Lookup<UserLookupRole, PermissionModel, UserLookupRole>(
                    foreignCollection: _permissionCollection,
                    localField: x => x.Roles.PermissionIds,
                    foreignField: x => x.Id,
                    @as: x => x.Roles.Permissions
                ),
                PipelineStageDefinitionBuilder.Group<UserLookupRole, string, UserLookupRoles>(
                    x => x.Id,
                    g => new UserLookupRoles {
                        Id = g.Key,
                        Avatar = g.First().Avatar,
                        Username = g.First().Username,
                        Password = g.First().Password,
                        Gender = g.First().Gender,
                        Email = g.First().Email,
                        PhoneNumber = g.First().PhoneNumber,
                        // Roles = g.Select(x => x.Roles).ToList()
                        Roles = g.Select(x => x.Roles.Id).Any() ? g.Select(x => x.Roles).ToList() : g.Select(x => x.Roles).ToList()
                    }
                )
            })
        );

        var UserAggregation = await _usersCollection.Aggregate().Match(filter).Facet(CountFacet, DataFacet).ToListAsync();

        var Count = UserAggregation.First().Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()?.FirstOrDefault()?.Count ?? 0;

        var TotalPages = (long)Math.Ceiling(ConvertData.ToFloat(Count) / ConvertData.ToFloat(limit));

        var Data = UserAggregation.First().Facets.First(x => x.Name == "data")
            .Output<UserLookupRoles>();

        return (Data, TotalPages, Count);
    }

    public async Task<UserLookupRoles?> GetAsync(string id) =>
        await UserLookupRolesAsync(Builders<UserModel>.Filter.Where(x => x.Id == id));

    public async Task CreateAsync(UserModel newUser) =>
        await _usersCollection.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, UpdateDefinition<UserModel> updatedUser) =>
        await _usersCollection.UpdateOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);

    private async Task<List<UserLookupRoles>> UsersLookupRolesAsync(FilterDefinition<UserModel> filter) =>
        await _usersCollection.Aggregate().Match(filter)
        .Lookup<UserModel, RoleModel, UserLookupRoles>(
            foreignCollection: _rolesCollection,
            localField: x => x.RoleIds,
            foreignField: x => x.Id,
            @as: x => x.Roles
        )
        .Unwind(x => x.Roles, new AggregateUnwindOptions<UserLookupRole> { PreserveNullAndEmptyArrays = true })
        .Lookup<UserLookupRole, PermissionModel, UserLookupRole>(
            foreignCollection: _permissionCollection,
            localField: x => x.Roles.PermissionIds,
            foreignField: x => x.Id,
            @as: x => x.Roles.Permissions
        )
        .Group(x => x.Id, g => new UserLookupRoles
        {
            Id = g.First().Id,
            Avatar = g.First().Avatar,
            Username = g.First().Username,
            Password = g.First().Password,
            Gender = g.First().Gender,
            Email = g.First().Email,
            PhoneNumber = g.First().PhoneNumber,
            Roles = g.Select(x => x.Roles.Id).Any() ? g.Select(x => x.Roles).ToList() : new()
        }).ToListAsync();

    private async Task<UserLookupRoles?> UserLookupRolesAsync(FilterDefinition<UserModel> filter) =>
        await _usersCollection.Aggregate().Match(filter)
        .Lookup<UserModel, RoleModel, UserLookupRoles>(
            foreignCollection: _rolesCollection,
            localField: x => x.RoleIds,
            foreignField: x => x.Id,
            @as: x => x.Roles
        )
        .Unwind(x => x.Roles, new AggregateUnwindOptions<UserLookupRole> { PreserveNullAndEmptyArrays = true })
        .Lookup<UserLookupRole, PermissionModel, UserLookupRole>(
            foreignCollection: _permissionCollection,
            localField: x => x.Roles.PermissionIds,
            foreignField: x => x.Id,
            @as: x => x.Roles.Permissions
        )
        .Group(x => x.Id, g => new UserLookupRoles
        {
            Id = g.First().Id,
            Avatar = g.First().Avatar,
            Username = g.First().Username,
            Password = g.First().Password,
            Gender = g.First().Gender,
            Email = g.First().Email,
            PhoneNumber = g.First().PhoneNumber,
            Roles = g.Select(x => x.Roles.Id).Any() ? g.Select(x => x.Roles).ToList() : new()
        }).FirstOrDefaultAsync();
}