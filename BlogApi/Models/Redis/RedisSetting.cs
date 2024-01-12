namespace BlogApi.Models.Redis;

public class RedisSetting
{
    public bool Enable { get; set; }
    public string ConnectionString { get; set; } = null!;
}
