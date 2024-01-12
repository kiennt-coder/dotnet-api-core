
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApi.Models.Permission;

public class PermissionDTO
{
    // Tên quyền
    public string? Name { get; set; }

    // Key
    public string? Key { get; set; }

    // Thứ tự sắp xếp
    public int? Order { get; set; }

    public bool? IsGroup { get; set; } = false;

    // Mô tả
    public string? Description { get; set; }

    // Id cha
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }
}
