using BlogApi.Models.Parameter;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApi.Models.Permission;

public class PermissionParameter : BaseParameter
{
    public bool? IsGroup { get; set; }

    // Id cha
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }
}
