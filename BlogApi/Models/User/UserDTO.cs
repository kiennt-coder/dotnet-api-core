using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApi.Models.User;

public class UserDTO
{
    // Ảnh đại diện
    public string? Avatar { get; set; }

    // Tên đăng nhập
    public string? Username { get; set; }

    // Mật khẩu
    public string? Password { get; set; }

    // Giới tính
    public string? Gender { get; set; }

    // Số điện thoại
    public string? Email { get; set; }

    // Email
    public string? PhoneNumber { get; set; }

    // Quyền
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string>? RoleIds { get; set; } = new List<string>();
}
