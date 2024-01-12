using BlogApi.Models.Role;
using BlogApi.Models.Permission;
using BlogApi.Services.Database.MongoDB;
using MongoDB.Driver;
using BlogApi.Helpers.ConvertData;
using System.Collections.Immutable;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace BlogApi.Services.Role;

public class RolesService : IRolesService
{
    private readonly IMongoCollection<RoleModel> _rolesCollection;
    private readonly IMongoCollection<PermissionModel> _permissionsCollection;

    public RolesService(IBlogDatabase blogDatabase)
    {
        _rolesCollection = blogDatabase.RoleCollection;
        _permissionsCollection = blogDatabase.PermissionCollection;
    }

    public async Task<List<RoleLookupPermissions>> GetAsync() =>
        await RolesLookupPermissions(Builders<RoleModel>.Filter.Where(_ => true));

    public async Task<(List<RoleLookupPermissions> Data, long TotalPages, long Count)> GetAsync(long skip, long limit, FilterDefinition<RoleModel> filter)
    {
        var CountFacet = AggregateFacet.Create("count",
            PipelineDefinition<RoleModel, AggregateCountResult>.Create(new[] {
                PipelineStageDefinitionBuilder.Count<RoleModel>()
            })
        );

        var DataFacet = AggregateFacet.Create("data",
            PipelineDefinition<RoleModel, RoleLookupPermissions>.Create(new List<IPipelineStageDefinition> {
                PipelineStageDefinitionBuilder.Sort(Builders<RoleModel>.Sort.Descending(x => x.CreateAt)),
                PipelineStageDefinitionBuilder.Skip<RoleModel>(skip),
                PipelineStageDefinitionBuilder.Limit<RoleModel>(limit),
                PipelineStageDefinitionBuilder.Lookup<RoleModel, PermissionModel, RoleLookupPermissions>(
                    foreignCollection: _permissionsCollection,
                    localField: x => x.PermissionIds,
                    foreignField: x => x.Id,
                    @as: x => x.Permissions
                ),
                // new BsonDocumentPipelineStageDefinition<RoleModel, RoleLookupPermissions>(
                //     new BsonDocument
                //     (
                //         "$lookup",
                //         new BsonDocument
                //         {
                //             { "from", "Permissions" },
                //             { "localField", "PermissionIds" },
                //             { "foreignField", "_id" },
                //             { "as", "Permissions" }
                //         }
                //     )
                // )
            })
        );

        var RoleAggregation = await _rolesCollection.Aggregate().Match(filter)
            .Facet(CountFacet, DataFacet).ToListAsync();

        var Count = RoleAggregation.First()
            .Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()?.FirstOrDefault()
            ?.Count ?? 0;

        var TotalPages = (long)Math.Ceiling(ConvertData.ToFloat(Count) / ConvertData.ToFloat(limit));

        var Data = RoleAggregation.First()
            .Facets.First(x => x.Name == "data")
            .Output<RoleLookupPermissions>();

        return (Data.ToImmutableList().ToList(), TotalPages, Count);
    }

    public async Task<RoleLookupPermissions?> GetByIdAsync(string id) =>
        await RoleLookupPermissions(Builders<RoleModel>.Filter.Where(x => x.Id == id));

    public async Task<RoleLookupPermissions?> GetByKeyAsync(string key) =>
        await RoleLookupPermissions(Builders<RoleModel>.Filter.Where(x => x.Key == key));

    public async Task CreateAsync(RoleModel roleModel) =>
        await _rolesCollection.InsertOneAsync(roleModel);

    public async Task UpdateAsync(string id, UpdateDefinition<RoleModel> updatedRole) =>
        await _rolesCollection.UpdateOneAsync(x => x.Id == id, updatedRole);

    public async Task RemoveAsync(string id) =>
        await _rolesCollection.DeleteOneAsync(x => x.Id == id);

    private async Task<List<RoleLookupPermissions>> RolesLookupPermissions(FilterDefinition<RoleModel> filter) =>
        await _rolesCollection.Aggregate().Match(filter)
        .Lookup<RoleModel, PermissionModel, RoleLookupPermissions>(
            foreignCollection: _permissionsCollection,
            localField: r => r.PermissionIds,
            foreignField: p => p.Id,
            @as: r => r.Permissions
        ).ToListAsync();

    private async Task<RoleLookupPermissions?> RoleLookupPermissions(FilterDefinition<RoleModel> filter) =>
        await _rolesCollection.Aggregate().Match(filter)
        .Lookup<RoleModel, PermissionModel, RoleLookupPermissions>(
            foreignCollection: _permissionsCollection,
            localField: r => r.PermissionIds,
            foreignField: p => p.Id,
            @as: r => r.Permissions
        ).FirstOrDefaultAsync();
}
