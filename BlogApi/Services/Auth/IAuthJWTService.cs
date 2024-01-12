using BlogApi.Models.User;

namespace BlogApi.Services.Auth;

public interface IAuthJWTService
{
    Task<UserDTO> Register(UserDTO user, string role);
    (string, string) Login(UserLookupRoles user);
}
