using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlogApi.Models.Permission;

public class PermissionModel
{
    // Id
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    // Tên Quyền
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

    // Ngày tạo
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreateAt { get; set; } = DateTime.Now;

    // Ngày cập nhật
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime UpdateAt { get; set; } = DateTime.Now;
}
