using BlogApi.Models.User;
using BlogApi.Models.Role;
using BlogApi.Models.Permission;
using MongoDB.Driver;

namespace BlogApi.Services.Database.MongoDB;

public interface IBlogDatabase
{
    IMongoClient MongoClient { get; }
    IMongoDatabase MongoDatabase { get; }
    IMongoCollection<UserModel> UserCollection { get; }
    IMongoCollection<RoleModel> RoleCollection { get; }
    IMongoCollection<PermissionModel> PermissionCollection { get; }
}
