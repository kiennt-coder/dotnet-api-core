namespace BlogApi.Models.Database;

public class BlogDatabaseSetting
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string UsersCollectionName { get; set; } = null!;
    public string RolesCollectionName { get; set; } = null!;
    public string PermissionsCollectionName { get; set; } = null!;

}
