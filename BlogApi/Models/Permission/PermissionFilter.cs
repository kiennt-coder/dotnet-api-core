using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApi.Models.Permission;

public class PermissionFilter
{
    // Tên quyền
    public string? Name { get; set; }

    public bool? IsGroup { get; set; }

    // Id cha
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }
}
