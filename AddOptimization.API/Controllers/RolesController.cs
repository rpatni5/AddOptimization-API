using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOptimization.API.Common;
using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Utilities.Models;

namespace AddOptimization.API.Controllers;

[Authorize]
public class RolesController : CustomApiControllerBase
{

    private readonly IRoleService _rolesService;
    public RolesController(ILogger<RolesController> logger, IRoleService rolesService) : base(logger)
    {
        _rolesService = rolesService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Get([FromBody] PageQueryFiterBase filter)
    {
        try
        {
            var retVal = await _rolesService.Search(filter);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RoleCreateDto model)
    {
        try
        {
            var retVal = await _rolesService.Create(model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id,[FromBody] RoleCreateDto model)
    {
        try
        {
            var retVal = await _rolesService.Update(id,model);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var retVal = await _rolesService.Delete(id);
            return HandleResponse(retVal);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

}