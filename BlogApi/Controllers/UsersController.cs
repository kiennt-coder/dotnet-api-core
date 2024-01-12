using Microsoft.AspNetCore.Mvc;
using BlogApi.Models.Response;
using BlogApi.Models.User;
using BlogApi.Services.User;
using Microsoft.AspNetCore.Authorization;
using BlogApi.Helpers.HashPassword;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BlogApi.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IHashPassword _hashPassword;

        private readonly ILogger<UsersController> _logger;

        public UsersController(IUsersService usersService, IHashPassword hashPassword, ILogger<UsersController> logger)
        {
            _usersService = usersService;
            _hashPassword = hashPassword;
            _logger = logger;
        }


        // GET: api/Users
        [HttpGet]
        [Authorize("read:users")]
        public async Task<ActionResult<BaseResponse>> GetUsers([FromQuery] UserParameter? userParameter)
        {
            // Response
            BaseResponse baseResponse = new BaseResponse();
            PaginationResponse paginationResponse = new PaginationResponse();
            // Sort
            SortDefinitionBuilder<UserModel> sortBuilder = Builders<UserModel>.Sort;
            SortDefinition<UserModel> sort = sortBuilder.Descending(x => x.CreateAt);
            // Filter
            FilterDefinitionBuilder<UserModel> filterBuilder = Builders<UserModel>.Filter;
            FilterDefinition<UserModel> filter = filterBuilder.Empty;

            try
            {
                if (userParameter is null || !userParameter.GetType().GetProperties().Any())
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.FieldRequired("Tham số");
                    return BadRequest(baseResponse);
                }

                if (userParameter.Page is null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.FieldRequired("Số trang");
                    return BadRequest(baseResponse);
                }

                if (userParameter.PageSize is null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.FieldRequired("Số phần tử trang");
                    return BadRequest(baseResponse);
                }

                if (!string.IsNullOrEmpty(userParameter.KeySearch))
                {
                    filter &= filterBuilder.Regex(x => x.Username, new BsonRegularExpression(userParameter.KeySearch, "i")) |
                        filterBuilder.Regex(x => x.Email, new BsonRegularExpression(userParameter.KeySearch, "i"));
                }

                // Check pagination
                if (userParameter.Page <= 0)
                {
                    userParameter.Page = 1;
                }

                if (userParameter.PageSize <= 0)
                {
                    userParameter.PageSize = 10;
                }

                if (!string.IsNullOrEmpty(userParameter.SortName) && !string.IsNullOrEmpty(userParameter.SortBy))
                {
                    sort = userParameter.SortBy == "ASC" ?
                            sortBuilder.Ascending(userParameter.SortName) :
                            sortBuilder.Descending(userParameter.SortName);
                }

                long skip = ((long)userParameter.Page! - 1) * (long)userParameter.PageSize!;
                long limit = (long)userParameter.PageSize;

                var result = await _usersService.GetAsync(skip, limit, sort, filter);

                paginationResponse.List = result.Data;
                paginationResponse.TotalPages = result.TotalPages;
                paginationResponse.CurrentPage = userParameter.Page;
                paginationResponse.Count = result.Count;

                baseResponse.Data = paginationResponse;
            }
            catch (System.Exception error)
            {
                _logger.LogError(error.Message);
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("lấy danh sách người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }
            return Ok(baseResponse);
        }

        // GET: api/Users/5
        [HttpGet("{id:length(24)}")]
        [Authorize("read:user")]
        public async Task<ActionResult<BaseResponse>> GetUserModel(string id)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (string.IsNullOrEmpty(id))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Id người dùng");
                return BadRequest(baseResponse);
            }

            try
            {
                var user = await _usersService.GetAsync(id);

                if (user is null)
                {
                    baseResponse.Status = StatusCodes.Status404NotFound;
                    baseResponse.Message = BaseResponseMessage.NotFound("Người dùng");
                    return NotFound(baseResponse);
                }

                baseResponse.Data = user;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("lấy thông tin người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }

        // PATCH: api/Users/5
        [HttpPatch("{id:length(24)}")]
        [Authorize("update:user")]
        public async Task<ActionResult<BaseResponse>> PatchUserModel(string id, UserDTO userDTO)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (string.IsNullOrEmpty(id))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Id người dùng");
                return BadRequest(baseResponse);
            }

            if (userDTO is null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin người dùng");
                return BadRequest(baseResponse);
            }

            try
            {
                var user = await _usersService.GetAsync(id);

                if (user is null)
                {
                    baseResponse.Status = StatusCodes.Status404NotFound;
                    baseResponse.Message = BaseResponseMessage.NotFound("Người dùng");
                    return NotFound(baseResponse);
                }

                var updatedUser = Builders<UserModel>.Update.Set(x => x.UpdateAt, DateTime.UtcNow);

                // Gán lại thông tin cần cập nhật
                if (userDTO.Avatar != null)
                {
                    updatedUser = updatedUser.Set(x => x.Avatar, userDTO.Avatar);
                }

                if (userDTO.Username != null)
                {
                    var isExists = await _usersService.GetByNameAsync(userDTO.Username);

                    if (isExists != null)
                    {
                        baseResponse.Status = StatusCodes.Status400BadRequest;
                        baseResponse.Message = BaseResponseMessage.Exists("Tên đăng nhập");
                        return BadRequest(baseResponse);
                    }

                    updatedUser = updatedUser.Set(x => x.Username, userDTO.Username);
                }

                if (userDTO.Password != null)
                {
                    // Mã hoá mật khẩu
                    string passwordHash = _hashPassword.Hash(userDTO.Password ?? "");
                    updatedUser = updatedUser.Set(x => x.Password, passwordHash);
                }

                if (userDTO.Gender != null)
                {
                    updatedUser = updatedUser.Set(x => x.Gender, userDTO.Gender);
                }

                if (userDTO.Email != null)
                {
                    updatedUser = updatedUser.Set(x => x.Email, userDTO.Email);
                }

                if (userDTO.PhoneNumber != null)
                {
                    updatedUser = updatedUser.Set(x => x.PhoneNumber, userDTO.PhoneNumber);
                }

                if (userDTO.RoleIds != null)
                {
                    updatedUser = updatedUser.Set(x => x.RoleIds, userDTO.RoleIds);
                }

                await _usersService.UpdateAsync(id, updatedUser);

                var newUser = await _usersService.GetAsync(id);

                baseResponse.Data = newUser;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("cập nhật thông tin người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }

        // POST: api/Users
        [HttpPost]
        [Authorize("create:user")]
        public async Task<ActionResult<BaseResponse>> PostUserModel(UserDTO userDTO)
        {
            BaseResponse baseResponse = new BaseResponse();

            // Nếu tên đăng nhập null hoặc '', trả về lỗi
            if (string.IsNullOrEmpty(userDTO.Username))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Tên đăng nhập");
                return BadRequest(baseResponse);
            }

            var isExists = await _usersService.GetByNameAsync(userDTO.Username);

            if (isExists != null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.Exists("Tên đăng nhập");
                return BadRequest(baseResponse);
            }

            // Nếu mật khẩu null hoặc '', trả về lỗi
            if (string.IsNullOrEmpty(userDTO.Password))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Mật khẩu");
                return BadRequest(baseResponse);
            }

            // Mã hoá mật khẩu
            string passwordHash = _hashPassword.Hash(userDTO.Password);

            // Khởi tạo đối tượng người dùng mới
            UserModel user = new UserModel
            {
                Username = userDTO.Username,
                Password = passwordHash,
                Gender = userDTO.Gender,
                Email = userDTO.Email,
                PhoneNumber = userDTO.PhoneNumber,
                RoleIds = userDTO.RoleIds,
            };

            try
            {
                await _usersService.CreateAsync(user);
                var newUser = await _usersService.GetAsync(user.Id!);
                baseResponse.Data = newUser;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("thêm mới người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
                // throw;
            }

            return Ok(baseResponse);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id:length(24)}")]
        [Authorize("delete:user")]
        public async Task<ActionResult<BaseResponse>> DeleteUserModel(string id)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (id is null || id.Length == 0)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Id người dùng");
                return BadRequest(baseResponse);
            }

            try
            {
                var user = await _usersService.GetAsync(id);

                if (user is null)
                {
                    baseResponse.Status = StatusCodes.Status404NotFound;
                    baseResponse.Message = BaseResponseMessage.NotFound("Người dùng");
                    return NotFound(baseResponse);
                }

                await _usersService.RemoveAsync(id);
                baseResponse.Data = id;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("xoá người dùng");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }
    }
}
