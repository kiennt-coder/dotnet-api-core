using System.Security.Claims;
using BlogApi.Helpers.HashPassword;
using BlogApi.Models.Auth;
using BlogApi.Models.Response;
using BlogApi.Models.User;
using BlogApi.Services.Auth;
using BlogApi.Services.Redis;
using BlogApi.Services.Role;
using BlogApi.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BlogApi.Controllers;

[Route("v1/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IRolesService _rolesService;
    private readonly IAuthJWTService _authJWTService;
    private readonly AuthJWTSetting _authJwtSetting;
    private readonly IRedisService _redisService;
    private readonly IHashPassword _hashPassword;
    public AuthController(IUsersService usersService, IRolesService rolesService, IAuthJWTService authJWTService, IOptions<AuthJWTSetting> authJWTSetting, IHashPassword hashPassword, IRedisService redisService)
    {
        _usersService = usersService;
        _rolesService = rolesService;
        _authJwtSetting = authJWTSetting.Value;
        _authJWTService = authJWTService;
        _hashPassword = hashPassword;
        _redisService = redisService;
    }

    // POST: api/Users/Login
    [HttpPost("Login")]
    public async Task<ActionResult<BaseResponse>> Login(UserLogin userLogin)
    {
        BaseResponse baseResponse = new BaseResponse();

        // Nếu tên đăng nhập null hoặc '', trả về lỗi
        if (string.IsNullOrEmpty(userLogin.Username))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Tên đăng nhập");
            return BadRequest(baseResponse);
        }

        // Nếu mật khẩu null hoặc '', trả về lỗi
        if (string.IsNullOrEmpty(userLogin.Password))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Mật khẩu");
            return BadRequest(baseResponse);
        }

        try
        {
            // Tìm người dùng theo tên đăng nhập
            var userFinded = await _usersService.GetByNameAsync(userLogin.Username);

            // Nếu không tìm thấy người dùng theo tên đăng nhập, trả về lỗi
            if (userFinded is null)
            {
                baseResponse.Status = StatusCodes.Status404NotFound;
                baseResponse.Message = BaseResponseMessage.NotFound("Tên đăng nhập");
                return NotFound(baseResponse);
            }

            // Nếu không tìm thấy người dùng theo mật khẩu, trả về lỗi
            if (!_hashPassword.Validate(userFinded.Password!, userLogin.Password))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.Wrong("Mật khẩu");
                return BadRequest(baseResponse);
            }

            // Tạo token, refresh token
            (string Token, string RefreshToken) = _authJWTService.Login(userFinded);
            var cacheKey = userFinded.Id;
            string cacheValue = RefreshToken;
            await _redisService.SetCacheResponseAsync(cacheKey!, cacheValue, TimeSpan.FromDays(_authJwtSetting.RefreshTokenExpire));

            baseResponse.Data = new
            {
                User = userFinded,
                Auth = new
                {
                    Token,
                    RefreshToken
                }
            };

        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("đăng nhập");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            //throw;
        }


        return Ok(baseResponse);
    }

    [HttpPost("Register")]
    public async Task<ActionResult<BaseResponse>> Register(UserLogin userLogin)
    {
        BaseResponse baseResponse = new BaseResponse();

        // Nếu tên đăng nhập null hoặc '', trả về lỗi
        if (string.IsNullOrEmpty(userLogin.Username))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Tên đăng nhập");
            return BadRequest(baseResponse);
        }

        // Nếu mật khẩu null hoặc '', trả về lỗi
        if (string.IsNullOrEmpty(userLogin.Password))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Mật khẩu");
            return BadRequest(baseResponse);
        }

        try
        {
            // Tìm người dùng theo tên đăng nhập
            var userFinded = await _usersService.GetByNameAsync(userLogin.Username);

            // Nếu không tìm thấy người dùng theo tên đăng nhập, trả về lỗi
            if (userFinded != null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.Exists("Tên đăng nhập");
                return BadRequest(baseResponse);
            }

            // Mã hoá mật khẩu
            var hashPassword = _hashPassword.Hash(userLogin.Password);

            // Thêm quyền người dùng
            var roles = await _rolesService.GetAsync();
            var userRole = roles.Find(x => x.Key == "user");
            List<string> RoleIds = new List<string>();

            if (userRole != null && userRole.Id != null)
            {
                RoleIds.Add(userRole.Id);
            }

            UserModel userModel = new UserModel
            {
                Username = userLogin.Username,
                Password = hashPassword,
                RoleIds = RoleIds
            };

            await _usersService.CreateAsync(userModel);
            var registedUser = await _usersService.GetAsync(userModel.Id!);
            baseResponse.Data = registedUser;
        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("đăng ký");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }

        return Ok(baseResponse);
    }

    [HttpPost("RefreshToken")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> GetRefreshToken(string id)
    {
        BaseResponse baseResponse = new BaseResponse();

        var identity = HttpContext.User.Identity as ClaimsIdentity;
        if (identity is null)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("lấy thông tin người dùng trong token");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }
        IEnumerable<Claim> claims = identity.Claims;
        bool isClaimExists = claims.Any(c => c.Type == "Id" && c.Value == id);
        if (!isClaimExists)
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.Failure("Xác thực thông tin");
            return BadRequest(baseResponse);
        }

        try
        {
            // Tìm người dùng theo id
            var userFinded = await _usersService.GetAsync(id);

            // Nếu tài khoản không tồn tại, trả về lỗi
            if (userFinded is null)
            {
                baseResponse.Status = StatusCodes.Status404NotFound;
                baseResponse.Message = BaseResponseMessage.NotFound("Tài khoản");
                return NotFound(baseResponse);
            }

            var auth = _authJWTService.Login(userFinded);
            baseResponse.Data = new
            {
                User = userFinded,
                Auth = auth
            };
        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("làm mới token");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }
        return Ok(baseResponse);
    }
}
