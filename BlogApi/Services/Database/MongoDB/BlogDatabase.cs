using BlogApi.Models.Database;
using BlogApi.Models.User;
using BlogApi.Models.Role;
using BlogApi.Models.Permission;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BlogApi.Services.Database.MongoDB;

public class BlogDatabase : IBlogDatabase
{
    private readonly IMongoClient _mongoClient;
    public IMongoClient MongoClient { get { return _mongoClient; } }
    private readonly IMongoDatabase _mongoDatabase;
    public IMongoDatabase MongoDatabase { get { return _mongoDatabase; } }
    private readonly IMongoCollection<UserModel> _userCollection;
    public IMongoCollection<UserModel> UserCollection { get { return _userCollection; } }
    private readonly IMongoCollection<RoleModel> _roleCollection;
    public IMongoCollection<RoleModel> RoleCollection { get { return _roleCollection; } }
    private readonly IMongoCollection<PermissionModel> _permissionCollection;
    public IMongoCollection<PermissionModel> PermissionCollection { get { return _permissionCollection; } }

    public BlogDatabase(IOptions<BlogDatabaseSetting> blogDatabaseSetting)
    {
        _mongoClient = new MongoClient(blogDatabaseSetting.Value.ConnectionString);
        _mongoDatabase = _mongoClient.GetDatabase(blogDatabaseSetting.Value.DatabaseName);
        _userCollection = _mongoDatabase.GetCollection<UserModel>(blogDatabaseSetting.Value.UsersCollectionName);
        _roleCollection = _mongoDatabase.GetCollection<RoleModel>(blogDatabaseSetting.Value.RolesCollectionName);
        _permissionCollection = _mongoDatabase.GetCollection<PermissionModel>(blogDatabaseSetting.Value.PermissionsCollectionName);
    }
}
