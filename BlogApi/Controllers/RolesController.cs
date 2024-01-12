using BlogApi.Models;
using BlogApi.Models.Response;
using BlogApi.Models.Role;
using BlogApi.Services.Role;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BlogApi.Controllers
{

    [Route("v1/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ILogger<RolesController> _logger;
        private readonly IRolesService _rolesService;

        public RolesController(ILogger<RolesController> logger, IRolesService rolesService)
        {
            _logger = logger;
            _rolesService = rolesService;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse>> GetRoles([FromQuery] RoleParameter roleParameter)
        {
            // Response
            BaseResponse baseResponse = new BaseResponse();
            PaginationResponse paginationResponse = new PaginationResponse();
            // Filter
            FilterDefinitionBuilder<RoleModel> filterBuilder = Builders<RoleModel>.Filter;
            FilterDefinition<RoleModel> filter = filterBuilder.Empty;

            try
            {
                if (roleParameter is null || !roleParameter.GetType().GetProperties().Any())
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin lọc vai trò");
                    return BadRequest(baseResponse);
                }

                if (roleParameter.Page is null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.FieldRequired("Số trang");
                    return BadRequest(baseResponse);
                }

                if (roleParameter.PageSize is null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.FieldRequired("Số phần tử trang");
                    return BadRequest(baseResponse);
                }

                if (!string.IsNullOrEmpty(roleParameter.KeySearch))
                {
                    filter &= filterBuilder.Regex(x => x.Name, new BsonRegularExpression(roleParameter.KeySearch, "i"));
                }

                if (roleParameter.Page <= 0)
                {
                    roleParameter.Page = 1;
                }

                if (roleParameter.PageSize <= 0)
                {
                    roleParameter.PageSize = 10;
                }

                long skip = ((long)roleParameter.Page! - 1) * (long)roleParameter.PageSize!;
                long limit = (long)roleParameter.PageSize;

                var result = await _rolesService.GetAsync(skip, limit, filter);

                paginationResponse.List = result.Data;
                paginationResponse.TotalPages = result.TotalPages;
                paginationResponse.CurrentPage = roleParameter.Page;
                paginationResponse.Count = result.Count;

                baseResponse.Data = paginationResponse;
            }
            catch (System.Exception error)
            {
                _logger.LogError(error.Message);

                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("lấy danh sách vai trò");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<BaseResponse>> GetRoleModel(string id)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (string.IsNullOrEmpty(id))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Id vai trò");
                return BadRequest(baseResponse);
            }

            try
            {
                var role = await _rolesService.GetByIdAsync(id)!;

                if (role is null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.NotFound("Vai trò");
                    return NotFound(baseResponse);
                }

                baseResponse.Data = role;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("lấy thông tin vai trò");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse>> PostRoleModel(RoleDTO roleDTO)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (roleDTO is null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin vai trò");
                return BadRequest(baseResponse);
            }

            if (string.IsNullOrEmpty(roleDTO.Key))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Key vai trò");
                return BadRequest(baseResponse);
            }

            try
            {
                var isExists = await _rolesService.GetByKeyAsync(roleDTO.Key!);

                if (isExists != null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.Exists("Key vai trò");
                    return BadRequest(baseResponse);
                }

                RoleModel roleModel = new RoleModel
                {
                    Name = roleDTO.Name,
                    Key = roleDTO.Key,
                    Description = roleDTO.Description,
                    PermissionIds = roleDTO.PermissionIds
                };

                await _rolesService.CreateAsync(roleModel);
                var newRole = await _rolesService.GetByIdAsync(roleModel.Id!);
                baseResponse.Data = newRole;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("thêm mới vai trò");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }

        [HttpPatch("{id:length(24)}")]
        public async Task<ActionResult<BaseResponse>> PatchRoleModel(string id, RoleDTO roleDTO)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (string.IsNullOrEmpty(id))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Id vai trò");
                return BadRequest(baseResponse);
            }

            if (roleDTO is null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin vai trò");
                return BadRequest(baseResponse);
            }

            try
            {
                var role = await _rolesService.GetByIdAsync(id);

                if (role is null)
                {
                    baseResponse.Status = StatusCodes.Status404NotFound;
                    baseResponse.Message = BaseResponseMessage.NotFound("Vai trò");
                    return NotFound(baseResponse);
                }

                var updatedRole = Builders<RoleModel>.Update.Set(x => x.UpdateAt, DateTime.Now);

                if (roleDTO.Name != null)
                {
                    updatedRole = updatedRole.Set(x => x.Name, roleDTO.Name);
                }

                if (roleDTO.Key != null)
                {
                    var isExists = await _rolesService.GetByKeyAsync(roleDTO.Key!);

                    if (isExists != null)
                    {
                        baseResponse.Status = StatusCodes.Status400BadRequest;
                        baseResponse.Message = BaseResponseMessage.Exists("Key vai trò");
                        return BadRequest(baseResponse);
                    }
                    updatedRole = updatedRole.Set(x => x.Key, roleDTO.Key);
                }

                if (roleDTO.Description != null)
                {
                    updatedRole = updatedRole.Set(x => x.Description, roleDTO.Description);
                }

                if (roleDTO.PermissionIds != null)
                {
                    updatedRole = updatedRole.Set(x => x.PermissionIds, roleDTO.PermissionIds);
                }

                await _rolesService.UpdateAsync(id, updatedRole);
                var newRole = await _rolesService.GetByIdAsync(id);
                baseResponse.Data = newRole;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("cập nhật thông tin vai trò");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult<BaseResponse>> DeleteRoleModel(string id)
        {
            BaseResponse baseResponse = new BaseResponse();

            if (string.IsNullOrEmpty(id))
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Id vai trò");
                return BadRequest(baseResponse);
            }

            try
            {
                var isExists = await _rolesService.GetByIdAsync(id);

                if (isExists is null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.NotFound("Vai trò");
                    return BadRequest(baseResponse);
                }

                await _rolesService.RemoveAsync(id);
                baseResponse.Data = id;
            }
            catch (System.Exception)
            {
                baseResponse.Status = StatusCodes.Status500InternalServerError;
                baseResponse.Message = BaseResponseMessage.InternalServerError("xoá vai trò");
                return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
            }

            return Ok(baseResponse);
        }
    }
}

