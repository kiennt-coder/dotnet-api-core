using System.Text.Json.Serialization;
using BlogApi.Models.Role;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using BlogApi.Models.Permission;

namespace BlogApi.Models.User;

// Thông tin người dùng
[BsonIgnoreExtraElements]
public class UserModel
{
    // Id
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // Ảnh đại diện
    public string? Avatar { get; set; }

    // Tên đăng nhập
    public string? Username { get; set; }

    // Mật khẩu
    [JsonIgnore]
    public string? Password { get; set; }

    // Giới tính
    public string? Gender { get; set; }

    // Số điện thoại
    public string? Email { get; set; }

    // Email
    public string? PhoneNumber { get; set; }

    // Danh sách id vai trò
    [JsonIgnore]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string>? RoleIds { get; set; } = new List<string>();

    // Ngày tạo
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreateAt { get; set; } = DateTime.Now;

    // Ngày cập nhật
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime UpdateAt { get; set; } = DateTime.Now;
}

public class UserLookupRoles : UserModel
{
    // Danh sách vai trò
    public List<RoleLookupPermissions> Roles { get; set; } = new();
}
public class UserLookupRole : UserModel
{
    // Danh sách vai trò
    public RoleLookupPermissions Roles { get; set; } = new RoleLookupPermissions();
}