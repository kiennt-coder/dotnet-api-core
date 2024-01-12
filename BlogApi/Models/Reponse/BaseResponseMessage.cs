using BlogApi.Helpers.ConvertData;

namespace BlogApi.Models.Response;

public class BaseResponseMessage
{
    // Message Success
    public static string Success() => "Thành công!";
    public static string Success(string value) => $"{ConvertData.ToUpperFirstLetter(value)} thành công!";

    // Message Error
    public static string Failure() => "Thất bại!";
    public static string Failure(string value) => $"{ConvertData.ToUpperFirstLetter(value)} thất bại!";
    public static string Wrong() => "Không chính xác!";
    public static string Wrong(string value) => $"{ConvertData.ToUpperFirstLetter(value)} không chính xác!";
    public static string InternalServerError() => "Lỗi hệ thống! Vui lòng liên hệ với Admin để xử lý!";
    public static string InternalServerError(string value) => $"Xảy ra lỗi trong quá trình {ConvertData.ToLowerFirstLetter(value)}!";
    public static string MinRequired(int number) => $"Không được nhỏ hơn ${number}!";
    public static string MinRequired(string value, int number) => $"{ConvertData.ToUpperFirstLetter(value)} không được nhỏ hơn ${number}!";
    public static string StringMinRequired(string value, int number) => $"{ConvertData.ToUpperFirstLetter(value)} không được nhỏ hơn ${number} ký tự!";

    // Message Auth
    public static string Unauthorized() => "Phiên đăng nhập đã hết hạn!";
    public static string Forbidden() => "Bạn không có quyền truy cập tính năng này!";


    // Message Required
    public static string FieldRequired() => "Không được để trống!";
    public static string FieldRequired(string value) => $"{ConvertData.ToUpperFirstLetter(value)} không được để trống!";

    // Message Data Type
    public static string InCorrectDataType() => "Sai kiểu dữ liệu!";
    public static string InCorrectDataType(string value) => $"{ConvertData.ToUpperFirstLetter(value)} sai kiểu dữ liệu!";

    // Message Exists
    public static string Exists() => "Đã tồn tại!";
    public static string Exists(string value) => $"{ConvertData.ToUpperFirstLetter(value)} đã tồn tại!";
    public static string NotFound() => "Không tồn tại!";
    public static string NotFound(string value) => $"{ConvertData.ToUpperFirstLetter(value)} không tồn tại!";
}
