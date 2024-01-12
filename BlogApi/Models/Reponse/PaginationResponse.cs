namespace BlogApi.Models.Response;

public class PaginationResponse
{
    public dynamic? List { get; set; }
    public long? TotalPages { get; set; } = 1;
    public long? CurrentPage { get; set; } = 1;
    public long? Count { get; set; } = 0;
}
