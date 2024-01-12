using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogApi.Models.Auth;
using BlogApi.Models.Permission;
using BlogApi.Models.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services.Auth;

public class AuthJWTService : IAuthJWTService
{
    // private readonly UserManager<UserModel> userManager;
    private readonly AuthJWTSetting _authJWTSetting;

    public AuthJWTService(IOptions<AuthJWTSetting> authJWTSetting)
    {
        // this.userManager = userManager;
        _authJWTSetting = authJWTSetting.Value;
    }

    public async Task<UserDTO> Register(UserDTO user, string role)
    {
        return user;
    }

    public (string, string) Login(UserLookupRoles user)
    {
        List<PermissionModel> permissions = user.Roles.SelectMany(r => r.Permissions).DistinctBy(p => p.Key).ToList();
        string scope = string.Join(" ", permissions.Select(p => p.Key));

        // var user = await userManager.FindByNameAsync(userModel.Username);
        var authClaims = new List<Claim> {
            new Claim("Id", user.Id!),
            new Claim(ClaimTypes.Name, user.Username!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("scope", scope),
        };
        string Token = GenerateToken(authClaims);
        string RefreshToken = GenerateRefreshToken(authClaims);
        return (Token, RefreshToken);
    }

    private string GenerateToken(IEnumerable<Claim> claims)
    {
        var authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authJWTSetting.Secret));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _authJWTSetting.Issuer,
            Audience = _authJWTSetting.Audience,
            Expires = DateTime.UtcNow.AddHours(_authJWTSetting.TokenExpire),
            SigningCredentials = new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(claims)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken(IEnumerable<Claim> claims)
    {
        var authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authJWTSetting.Secret));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _authJWTSetting.Issuer,
            Audience = _authJWTSetting.Audience,
            Expires = DateTime.UtcNow.AddDays(_authJWTSetting.RefreshTokenExpire),
            SigningCredentials = new SigningCredentials(authSignKey, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(claims)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
