using System.Collections.Immutable;
using BlogApi.Helpers.ConvertData;
using BlogApi.Models.Permission;
using BlogApi.Services.Database.MongoDB;
using MongoDB.Driver;

namespace BlogApi.Services.Permission;

public class PermissionsService : IPermissionsService
{
    private readonly IMongoCollection<PermissionModel> _permissionCollection;

    public PermissionsService(IBlogDatabase blogDatabase)
    {
        _permissionCollection = blogDatabase.PermissionCollection;
    }

    public async Task<List<PermissionModel>> GetAllAsync() =>
        await _permissionCollection.Find(_ => true).ToListAsync();

    public async Task<(IReadOnlyList<PermissionModel> Data, long TotalPages, long Count)> GetAsync(long skip, long limit, SortDefinition<PermissionModel> sort, FilterDefinition<PermissionModel> filter)
    {


        var CountFacet = AggregateFacet.Create("count",
            PipelineDefinition<PermissionModel, AggregateCountResult>.Create(new[] {
                PipelineStageDefinitionBuilder.Count<PermissionModel>()
            })
        );

        var DataFacet = AggregateFacet.Create("data",
            PipelineDefinition<PermissionModel, PermissionModel>.Create(new[] {
                PipelineStageDefinitionBuilder.Sort(sort),
                PipelineStageDefinitionBuilder.Skip<PermissionModel>(skip),
                PipelineStageDefinitionBuilder.Limit<PermissionModel>(limit)
            })
        );

        var PermissionAggregation = await _permissionCollection.Aggregate()
            .Match(filter).Facet(CountFacet, DataFacet).ToListAsync();

        var Count = PermissionAggregation.First()
            .Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()?.FirstOrDefault()
            ?.Count ?? 0;

        var TotalPages = (long)Math.Ceiling(
            ConvertData.ToFloat(Count) / ConvertData.ToFloat(limit)
        );

        var Data = PermissionAggregation.First()
            .Facets.First(x => x.Name == "data")
            .Output<PermissionModel>();

        return (Data, TotalPages, Count);
    }

    public async Task<PermissionModel?> GetByIdAsync(string id) =>
        await _permissionCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    public async Task<PermissionModel?> GetByKeyAsync(string key) =>
        await _permissionCollection.Find(x => x.Key == key).FirstOrDefaultAsync();

    public async Task CreateAsync(PermissionModel permissionModel) =>
        await _permissionCollection.InsertOneAsync(permissionModel);

    public async Task UpdateAsync(string id, UpdateDefinition<PermissionModel> updatedPermisison) =>
        await _permissionCollection.UpdateOneAsync(x => x.Id == id, updatedPermisison);

    public async Task UpdateManyAsync(FilterDefinition<PermissionModel> filter, UpdateDefinition<PermissionModel> updatedPermissions) =>
        await _permissionCollection.UpdateManyAsync(filter, updatedPermissions);

    public async Task RemoveAsync(string id) =>
        await _permissionCollection.DeleteOneAsync(x => x.Id == id);
}
