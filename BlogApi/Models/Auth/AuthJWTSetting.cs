namespace BlogApi.Models.Auth;

public class AuthJWTSetting
{
    public string Domain { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Secret { get; set; } = null!;
    public int TokenExpire { get; set; }
    public int RefreshTokenExpire { get; set; }
}
