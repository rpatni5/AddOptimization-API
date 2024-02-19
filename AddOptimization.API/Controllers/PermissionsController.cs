

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers;

[Authorize]
public class PermissionsController : CustomApiControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(ILogger<PermissionsController> logger, IPermissionService permissionService) : base(logger)
    {
        _permissionService = permissionService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody]PageQueryFiterBase filter)
    {
        try
        {
            var result = await _permissionService.Search(filter);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpGet("configData")]
    public async Task<IActionResult> GetConfigData()
    {
        try
        {
            var result = await _permissionService.GetConfigData();
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("userAccess")]
    public async Task<IActionResult> GetUserAccess([FromBody] UserAccessDto model)
    {
        try
        {
            var result = await _permissionService.GetUserAccess(model);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveRolePermissions([FromBody] PermissionCreateDto model)
    {
        try
        {
            var result = await _permissionService.SaveRolePermissions(model);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteRolePermission([FromBody] PermissionDeleteDto model)
    {
        try
        {
            var result = await _permissionService.DeleteRolePermission(model);
            return HandleResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
