using BlogApi.Models.Permission;
using BlogApi.Models.Response;
using BlogApi.Services.Permission;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BlogApi.Controllers;

[Route("v1/[controller]")]
[ApiController]
public class PermissionsController : ControllerBase
{
    private readonly ILogger<PermissionsController> _logger;
    private readonly IPermissionsService _permissionsService;

    public PermissionsController(IPermissionsService permissionsService, ILogger<PermissionsController> logger)
    {
        _logger = logger;
        _permissionsService = permissionsService;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse>> GetPermissions([FromQuery] PermissionParameter? permissionParameter)
    {
        // Response
        BaseResponse baseResponse = new BaseResponse();
        PaginationResponse paginationResponse = new PaginationResponse();
        // Sort
        SortDefinitionBuilder<PermissionModel> sortBuilder = Builders<PermissionModel>.Sort;
        SortDefinition<PermissionModel> sort = sortBuilder.Descending(x => x.CreateAt);
        // Filter
        FilterDefinitionBuilder<PermissionModel> filterBuilder = Builders<PermissionModel>.Filter;
        FilterDefinition<PermissionModel> filter = filterBuilder.Empty;

        try
        {
            if (permissionParameter is null || !permissionParameter.GetType().GetProperties().Any())
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Tham số");
                return BadRequest(baseResponse);
            }

            if (permissionParameter.Page is null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Số trang");
                return BadRequest(baseResponse);
            }

            if (permissionParameter.PageSize is null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.FieldRequired("Số phần tử trang");
                return BadRequest(baseResponse);
            }

            if (!string.IsNullOrEmpty(permissionParameter.KeySearch))
            {
                filter &= filterBuilder.Regex(x => x.Name, new BsonRegularExpression(permissionParameter.KeySearch, "i"));
            }

            if (permissionParameter.IsGroup != null && permissionParameter.IsGroup is bool)
            {
                filter &= filterBuilder.Eq(x => x.IsGroup, permissionParameter.IsGroup);
            }

            if (!string.IsNullOrEmpty(permissionParameter.ParentId))
            {
                if (permissionParameter.ParentId == "null")
                    permissionParameter.ParentId = null;

                filter &= filterBuilder.Eq(x => x.ParentId, permissionParameter.ParentId);
            }

            // Check pagination
            if (permissionParameter.Page <= 0)
            {
                permissionParameter.Page = 1;
            }

            if (permissionParameter.PageSize <= 0)
            {
                permissionParameter.PageSize = 10;
            }

            if (!string.IsNullOrEmpty(permissionParameter.SortName) && !string.IsNullOrEmpty(permissionParameter.SortBy))
            {
                sort = permissionParameter.SortBy == "ASC" ?
                        sortBuilder.Ascending(permissionParameter.SortName) :
                        sortBuilder.Descending(permissionParameter.SortName);
            }

            long skip = ((long)permissionParameter.Page! - 1) * (long)permissionParameter.PageSize!;
            long limit = (long)permissionParameter.PageSize;

            var result = await _permissionsService.GetAsync(skip, limit, sort, filter);

            paginationResponse.List = result.Data;
            paginationResponse.TotalPages = result.TotalPages;
            paginationResponse.CurrentPage = permissionParameter.Page;
            paginationResponse.Count = result.Count;

            baseResponse.Data = paginationResponse;
        }
        catch (System.Exception error)
        {
            _logger.LogError(error.ToString());
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("lấy danh sách quyền");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }

        return Ok(baseResponse);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<BaseResponse>> GetPermissionModel(string id)
    {
        BaseResponse baseResponse = new BaseResponse();

        if (string.IsNullOrEmpty(id))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Id quyền");
            return BadRequest(baseResponse);
        }

        try
        {
            var permission = await _permissionsService.GetByIdAsync(id);
            if (permission is null)
            {
                baseResponse.Status = StatusCodes.Status404NotFound;
                baseResponse.Message = BaseResponseMessage.NotFound("Quyền");
                return NotFound(baseResponse);
            }

            baseResponse.Data = permission;
        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("lấy thông tin quyền");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }

        return Ok(baseResponse);
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse>> PostPermissionModel(PermissionDTO? permissionDTO)
    {
        BaseResponse baseResponse = new BaseResponse();

        if (permissionDTO is null)
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin quyền");
            return BadRequest(baseResponse);
        }

        if (string.IsNullOrEmpty(permissionDTO.Key))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Key quyền");
            return BadRequest(baseResponse);
        }

        try
        {
            var isExists = await _permissionsService.GetByKeyAsync(permissionDTO.Key);

            if (isExists != null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.Exists("Key quyền");
                return BadRequest(baseResponse);
            }

            PermissionModel permissionModel = new PermissionModel
            {
                Name = permissionDTO.Name,
                Key = permissionDTO.Key,
                Order = permissionDTO.Order,
                IsGroup = permissionDTO.IsGroup,
                Description = permissionDTO.Description,
                ParentId = permissionDTO.ParentId
            };

            await _permissionsService.CreateAsync(permissionModel);
            var createdPermission = await _permissionsService.GetByIdAsync(permissionModel.Id!);
            baseResponse.Data = createdPermission;
        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("thêm mới quyền");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }


        return Ok(baseResponse);
    }

    [HttpPatch("{id:length(24)}")]
    public async Task<ActionResult<BaseResponse>> PatchPermissionModel(string? id, PermissionDTO? permissionDTO)
    {
        BaseResponse baseResponse = new BaseResponse();

        if (string.IsNullOrEmpty(id))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Id quyền");
            return BadRequest(baseResponse);
        }

        if (permissionDTO is null)
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin quyền");
            return BadRequest(baseResponse);
        }

        try
        {
            var permissionModel = await _permissionsService.GetByIdAsync(id);

            if (permissionModel is null)
            {
                baseResponse.Status = StatusCodes.Status404NotFound;
                baseResponse.Message = BaseResponseMessage.NotFound("Quyền");
                return NotFound(baseResponse);
            }

            var updatedPermisison = Builders<PermissionModel>.Update.Set(x => x.UpdateAt, DateTime.Now);

            if (permissionDTO.Name != null)
            {
                updatedPermisison = updatedPermisison.Set(x => x.Name, permissionDTO.Name);
            }

            if (permissionDTO.Key != null)
            {
                var isExists = await _permissionsService.GetByKeyAsync(permissionDTO.Key);

                if (isExists != null)
                {
                    baseResponse.Status = StatusCodes.Status400BadRequest;
                    baseResponse.Message = BaseResponseMessage.Exists("Key quyền");
                    return BadRequest(baseResponse);
                }

                updatedPermisison = updatedPermisison.Set(x => x.Key, permissionDTO.Key);
            }

            if (permissionDTO.Order != null)
            {
                updatedPermisison = updatedPermisison.Set(x => x.Order, permissionDTO.Order);
            }

            if (permissionDTO.IsGroup != null)
            {
                updatedPermisison = updatedPermisison.Set(x => x.IsGroup, permissionDTO.IsGroup);
            }

            if (permissionDTO.Description != null)
            {
                updatedPermisison = updatedPermisison.Set(x => x.Description, permissionDTO.Description);
            }

            if (!string.IsNullOrEmpty(permissionDTO.ParentId))
            {
                updatedPermisison = updatedPermisison.Set(x => x.ParentId, permissionDTO.ParentId);
            }

            await _permissionsService.UpdateAsync(id, updatedPermisison);
            var updatedPermission = await _permissionsService.GetByIdAsync(id);
            baseResponse.Data = updatedPermission;
        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("cập nhật thông tin quyền");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }


        return Ok(baseResponse);
    }

    [HttpPatch("UpdateMany")]
    public async Task<ActionResult<BaseResponse>> PatchPermissions([FromQuery] PermissionFilter permissionFilter, PermissionDTO? permissionDTO)
    {
        BaseResponse baseResponse = new();
        _logger.LogInformation(permissionDTO.ToJson());

        if (permissionDTO is null || !permissionDTO.GetType().GetProperties().Any())
        {
            baseResponse.Status = 400;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Thông tin quyền không được để rỗng!");
            return BadRequest(baseResponse);
        }

        var filterBuilder = Builders<PermissionModel>.Filter;
        var filter = Builders<PermissionModel>.Filter.Empty;
        var updatedPermissions = Builders<PermissionModel>.Update.Set(x => x.UpdateAt, DateTime.Now);

        try
        {
            if (permissionFilter.ParentId != null)
            {
                if (permissionFilter.ParentId == "null")
                    permissionFilter.ParentId = null;

                filter &= filterBuilder.Eq(x => x.ParentId, permissionFilter.ParentId);
            }

            if (permissionFilter.Name != null)
            {
                filter &= filterBuilder.Regex(x => x.Name, new BsonRegularExpression(permissionFilter.Name, "i"));
            }

            if (!string.IsNullOrEmpty(permissionDTO.Key))
            {
                updatedPermissions = updatedPermissions.Set(x => x.Key, permissionDTO.Key);
            }

            if (permissionDTO.Name != null)
            {
                updatedPermissions = updatedPermissions.Set(x => x.Name, permissionDTO.Name);
            }

            if (permissionDTO.Order != null)
            {
                updatedPermissions = updatedPermissions.Set(x => x.Order, permissionDTO.Order);
            }

            if (!string.IsNullOrEmpty(permissionDTO.Description))
            {
                updatedPermissions = updatedPermissions.Set(x => x.Description, permissionDTO.Description);
            }

            if (!string.IsNullOrEmpty(permissionDTO.ParentId))
            {
                updatedPermissions = updatedPermissions.Set(x => x.ParentId, permissionDTO.ParentId);
            }

            await Task.WhenAny(new TaskCompletionSource().Task, Task.Delay(1000));
            // await _permissionsService.UpdateManyAsync(filter, updatedPermissions);
        }
        catch (System.Exception err)
        {
            _logger.LogError(err.ToString());
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("cập nhật danh sách quyền");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }

        return Ok(baseResponse);
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<ActionResult<BaseResponse>> DeletePermissionModel(string id)
    {
        BaseResponse baseResponse = new BaseResponse();

        if (string.IsNullOrEmpty(id))
        {
            baseResponse.Status = StatusCodes.Status400BadRequest;
            baseResponse.Message = BaseResponseMessage.FieldRequired("Id quyền");
            return BadRequest(baseResponse);
        }

        try
        {
            var isExists = await _permissionsService.GetByIdAsync(id);

            if (isExists is null)
            {
                baseResponse.Status = StatusCodes.Status400BadRequest;
                baseResponse.Message = BaseResponseMessage.NotFound("Quyền");
                return NotFound(baseResponse);
            }

            await _permissionsService.RemoveAsync(id);
            baseResponse.Data = id;
        }
        catch (System.Exception)
        {
            baseResponse.Status = StatusCodes.Status500InternalServerError;
            baseResponse.Message = BaseResponseMessage.InternalServerError("xoá quyền");
            return StatusCode(StatusCodes.Status500InternalServerError, baseResponse);
        }

        return Ok(baseResponse);
    }
}
