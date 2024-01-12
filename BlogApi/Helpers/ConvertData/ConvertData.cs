using System.Text.RegularExpressions;

namespace BlogApi.Helpers.ConvertData;

public class ConvertData
{
    // Viết hoa chữ cái đầu tiên
    public static string ToUpperFirstLetter(string value) => value[0].ToString().ToUpper() + value.Substring(1);
    public static string ToUpper(string value) => value.ToUpper();

    // Viết thường chữ cái đầu tiên
    public static string ToLowerFirstLetter(string value) => value[0].ToString().ToLower() + value.Substring(1);
    public static string ToLower(string value) => value.ToLower();

    // Chuyển tiếng việt có dấu sang không dấu
    public static string ToNonAccentVietnamese(string value) =>
        new Regex("[\\p{Mn}]").Replace(value.Normalize(System.Text.NormalizationForm.FormKD), string.Empty);

    // Convert to decimal
    public static decimal ToDecimal(long number) => Convert.ToDecimal(number);

    // Convert to double
    public static float ToFloat(long number) => Convert.ToSingle(number);

    // Convert to long
    public static long ToLong(double number) => Convert.ToInt64(number);
}
