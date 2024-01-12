using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using BlogApi.Models.Permission;

namespace BlogApi.Models.Role;

public class RoleModel
{
    // Id
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // Tên vai trò
    public string? Name { get; set; }

    // Key
    public string? Key { get; set; }

    // Mô tả
    public string? Description { get; set; }

    // Danh sách id quyền
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string>? PermissionIds { get; set; }

    // Ngày tạo
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreateAt { get; set; } = DateTime.Now;

    // Ngày cập nhật
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime UpdateAt { get; set; } = DateTime.Now;
}

[BsonIgnoreExtraElements]
public class RoleLookupPermissions : RoleModel
{
    // Danh sách quyền
    public List<PermissionModel> Permissions { get; set; } = new List<PermissionModel>();
}