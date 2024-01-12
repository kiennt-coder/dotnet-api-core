namespace BlogApi.Models.Response;

public class BaseResponse
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = BaseResponseMessage.Success();
    public dynamic? Data { get; set; }
}
