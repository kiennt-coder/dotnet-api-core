using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApi.Models.Role;

public class RoleDTO
{
    // Tên vai trò
    public string? Name { get; set; }
    // Key
    public string? Key { get; set; }
    // Mô tả
    public string? Description { get; set; }
    // Danh sách id quyền
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string>? PermissionIds { get; set; }
}