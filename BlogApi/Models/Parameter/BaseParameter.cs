namespace BlogApi.Models.Parameter;

public class BaseParameter
{
    public string? KeySearch { get; set; }
    public long? Page { get; set; } = 1;

    public long? PageSize { get; set; } = 10;

    public string? SortName { get; set; } = "CreateAt";

    public string? SortBy { get; set; } = "DESC";
}
